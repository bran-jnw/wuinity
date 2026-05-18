using PREACT.Math;
using PREACT.Math;
using System.Collections.Generic;
using PREACT.Input;
using PREACT.Pedestrian;
using PREACT.Wildfire;
using PREACT.Traffic;
using System.Diagnostics;
using System.IO;

namespace PREACT.Evacuation
{
    public class EvacuationManager
    {
        Simulation _simulation;
        private TrafficModule _trafficModule;
        private PedestrianModule _pedestrianModule;
        private TriggerBufferModule _triggerBufferModule;


        private Stopwatch _pathfindingStopwatch = new Stopwatch();
        private Stopwatch _roadClosureStopwatch = new Stopwatch();


        PREACTInput _input;
        EvacuationGroup _defaultEvacuationGroup;        
        Dictionary<string, EvacuationDestination> _evacuationDestinationsDict;
        List<EvacuationDestination> _evacuationDestinations;
        List<EvacuationDestination> _availableEvacuationDestinations;
        EvacuationGroup[] _evacuationGroups;
        DemographicsInput _defaultDemographics;

        public PedestrianModule PedestrianModule { get => _pedestrianModule; }
        public TrafficModule TrafficModule { get => _trafficModule; }
        public TriggerBufferModule TriggerBufferModule { get => _triggerBufferModule; }

        //Data, move?
        public List<EvacuationDestination> Destinations { get => _evacuationDestinations; }
        public DemographicsInput DefaultDemographics { get => _defaultDemographics; }

        public EvacuationManager(Simulation simulation)
        {
            _simulation = simulation;
            _input = _simulation.Input;
            _evacuationDestinationsDict = EvacuationDestination.CreateEvacacuationDestinationsFromInput(_simulation, _input.Evacuation.EvacuationDestinationInputs);
            SetDefaulDemographics(_input.Population.Demographics);
            _evacuationGroups = EvacuationGroup.CreateGroupsFromInput(_input.Evacuation.EvacuationGroupInputs, _evacuationDestinationsDict, _input.Evacuation.ResponseCurves, _input.Population.Demographics, _simulation);
            SetDefaulEvacuationtGroup(); //just sets default group fallback
            BuildEvacuationDestinationList(); //duplicate of destination but in an array, needed for random pull of destination
            BuildAvailableEvacuationDestinations();
        }

        public void PostStep()
        {
            //check for distance to wildfire front, affect evacuees
            if(_simulation.Hazards.Wildfire != null)
            {
                _pedestrianModule.ReactToWildfire(_simulation.Time.SimulationTime);
            }            

            //handle all damage/impact on road network
            AffectRoadNetwork();

            //check if any goal has been blocked by fire, this is done after everything has progressed the current time step
            UpdateDestinationsWildfireStatus();

            //inject vehicles from all sources
            HandleNewVehicles();
        }

        private void AffectRoadNetwork()
        {
            //handle any fire effects on road network
            if (_simulation.Hazards.Wildfire != null)
            {
                if (_trafficModule != null)
                {
                    _roadClosureStopwatch.Start();
                    _trafficModule.HandleIgnitedFireCells(_simulation.Hazards.Wildfire.GetIgnitedFireCells());
                    _roadClosureStopwatch.Stop();
                }
                _simulation.Hazards.Wildfire.ConsumeIgnitedFireCells();
            }
        }

        private void HandleNewVehicles()
        {
            //handle/inject cars that arrived this timestep
            if (_trafficModule != null)
            {
                _pathfindingStopwatch.Start();
                _trafficModule.HandleNewCars();
                _pathfindingStopwatch.Stop();
            }
        }


        public List<SimulationModule> CreateModules(WeatherManager weather, TimeManager time, out bool success)
        {
            List<SimulationModule> createdModules = new List<SimulationModule>();

            CreatePedestrianModule(_simulation, _input, weather, time, out success);
            if(success && _pedestrianModule != null)
            {
                createdModules.Add(_pedestrianModule);
            }
            else
            {
                return createdModules;
            }

            CreateTrafficModule(_simulation, _input, weather, time, out success);
            if (success && _trafficModule != null)
            {
                createdModules.Add(_trafficModule);
            }
            else
            {
                return createdModules;
            }

            return createdModules;
        }

        private void CreatePedestrianModule(Simulation simulation, PREACTInput input, WeatherManager weather, TimeManager time, out bool success)
        {
            success = false;

            if (_input.PedestrianModule.Enabled)
            {
                if (_input.PedestrianModule.Module == PedestrianModuleInput.PedestrianModules.JupedSimSUMO)
                {
                    //placeholder for JupedSim
                }
                else if (_input.PedestrianModule.Module == PedestrianModuleInput.PedestrianModules.MacroHouseholdSim)
                {
                    _pedestrianModule = new MacroHouseholdSim(simulation);                    
                    Engine.Message(simulation, Engine.LogType.Log, "Pedestrian module MacroPedestrianSim initiated.");
                }
            }
            else
            {
                success = true;
                Engine.Message(simulation, Engine.LogType.Log, "No pedestrian module was enabled.");
            }

            if(_pedestrianModule != null)
            {
                success = true;
            }
        }

        private void CreateTrafficModule(Simulation simulation, PREACTInput input, WeatherManager weather, TimeManager time, out bool success)
        {
            success = false;

            if (_input.TrafficModule.Enabled)
            {
                if (_input.TrafficModule.Module == TrafficModuleInput.TrafficModules.SUMO)
                {
                    _trafficModule = new SUMOModule(simulation, out success);
                    if (success)
                    {
                        Engine.Message(simulation, Engine.LogType.Log, "Traffic module SUMO initiated.");
                    }
                    else
                    {
                        _trafficModule = null;
                    }
                }
                else
                {
                    _trafficModule = new MacroTrafficSim(simulation);
                    Engine.Message(simulation, Engine.LogType.Log, "Traffic module MacroTrafficSim initiated.");
                }
            }
            else
            {
                success = true;
                Engine.Message(simulation, Engine.LogType.Log, "No traffic module was enabled.");
            }

            if (_trafficModule != null)
            {
                success = true;
            }
        }

        public void CreateAndRunTriggerBufferModule(Simulation simulation, PREACTInput input, WeatherManager weather, TimeManager time)
        {
            if (_input.TriggerBufferModule.Enabled)
            {
                if (_input.TriggerBufferModule.Module == TriggerBufferModuleInput.TriggerBufferModules.kPERIL)
                {
                    if (!_input.WildfireModule.Enabled && !_input.TriggerBufferModule.kPERILInput.CalculateROSFromBehave)
                    {
                        Engine.Message(simulation, Engine.LogType.Warning, "Can't run kPERIL without fire module (user set not to use BEHAVE).");
                        return;
                    }
                    else
                    {
                        if (_input.TriggerBufferModule.kPERILInput.CalculateROSFromBehave)
                        {
                            _triggerBufferModule = new kPERIL(_input.WildfireModule.Data.LandscapeData, time.SimulationTime, _input.WildfireModule.Data.WuiArea, _input.TriggerBufferModule.kPERILInput.MidflameWindspeed, 0f, _input.WildfireModule.Data.InitialFuelMoistureData, _input.WildfireModule.Data.FuelModelsData);
                        }
                        else
                        {
                            _triggerBufferModule = new kPERIL(time.SimulationTime, _input.WildfireModule.Data.WuiArea, _input.TriggerBufferModule.kPERILInput.MidflameWindspeed, 0f, simulation.Hazards.Wildfire.GetMaxROS(), simulation.Hazards.Wildfire.GetMaxROSAzimuth());
                        }
                        _triggerBufferModule.Run();
                        string outputFilePath = Path.Combine(simulation.Engine.OutputFolder, simulation.SimulationIndex + "_" + _input.TriggerBufferModule.kPERILInput.OutputName);
                        kPERIL.SaveToFile(_triggerBufferModule.TriggerBufferOutput, simulation.Hazards.Wildfire.GetCellSizeX(), outputFilePath);
                    }
                }
                else
                {

                }

                if (_triggerBufferModule != null)
                {
                    simulation.Output.AddTriggerBufferOutput(_triggerBufferModule.TriggerBufferOutput, simulation.SimulationIndex);
                }
            }
            else
            {
                Engine.Message(simulation, Engine.LogType.Log, "Trigger buffer module was enabled.");
            }
        }

        public void InsertNewCar(Vector2d startLatLon, EvacuationDestination evacuationGoal, uint numberOfPeopleInCar)
        {
            if (_trafficModule != null)
            {
                _trafficModule.InsertNewCar(startLatLon, evacuationGoal, numberOfPeopleInCar);
            }
        }

        int _runtimeDestinationCount = 0;
        public EvacuationDestination AddRuntimeDestination(Vector2d latLon)
        {
            EvacuationDestination eD = new EvacuationDestination(_simulation, latLon, "RuntimeDestination" + _runtimeDestinationCount);
            _evacuationDestinations.Add(eD);
            ++_runtimeDestinationCount;
            _simulation.Engine.UpdateEvacuationDestinations(_simulation, _evacuationDestinations);

            return eD;
        }

        public uint GetTotalEvacuated()
        {
            uint result = 0;
            foreach (EvacuationDestination eD in _evacuationDestinations)
            {
                result += eD.CurrentPeople;
            }

            return result;
        }

        public void UpdateDestinationsWildfireStatus()
        {
            if (!_input.WildfireModule.Enabled)
            {
                return;
            }

            foreach (EvacuationDestination eD in _evacuationDestinations)
            {
                if (!eD.Blocked)
                {
                    FireCellState cellState = _simulation.Hazards.Wildfire.GetFireCellState(eD.SimulationPos);
                    if (cellState == FireCellState.Ignited)
                    {
                        Engine.Message(_simulation, Engine.LogType.Log, " Destination blocked by fire: " + eD.Name);
                        BlockDestination(eD);
                    }
                }
            }
        }

        public void BlockDestinationEvent(string destinationName)
        {
            EvacuationDestination eD;
            if(_evacuationDestinationsDict.TryGetValue(destinationName, out eD))
            {
                BlockDestination(eD);
                Engine.Message(_simulation, Engine.LogType.Event, "Goal blocked: " + eD.Name);
            }            
        }

        private void BlockDestination(EvacuationDestination eD)
        {
            if (!eD.Blocked)
            {
                eD.BlockDestination();
                UpdateEvacuationDestinations();
            }
        }

        /// <summary>
        /// Called from goal when blocked internally.
        /// </summary>
        public void GoalBlocked()
        {
            UpdateEvacuationDestinations();
        }

        private void UpdateEvacuationDestinations()
        {
            //check that we have at least one goal left
            bool allBlocked = true;
            _availableEvacuationDestinations.Clear();
            foreach (EvacuationDestination eD in _evacuationDestinations)
            {
                if (!eD.Blocked)
                {
                    _availableEvacuationDestinations.Add(eD);
                    allBlocked = false;
                }
            }
            if (allBlocked)
            {
                _simulation.Stop("All destinations are unavailable, stopping simulation.", false);
                return;
            }

            //update raster evac routes first as traffic might use some of the updated choices
            //TODO

            //update cars already in traffic
            if(_trafficModule != null)
            {
                _trafficModule.UpdateEvacuationGoals();
            }            
        }

        private void BuildEvacuationDestinationList()
        {
            _evacuationDestinations = new List<EvacuationDestination>(_evacuationDestinationsDict.Count);
            foreach (EvacuationDestination eD in _evacuationDestinationsDict.Values)
            {
                _evacuationDestinations.Add(eD);
            }
        }

        private void BuildAvailableEvacuationDestinations()
        {
            _availableEvacuationDestinations = new List<EvacuationDestination>(_evacuationDestinations.Count);
            foreach(EvacuationDestination eD in _evacuationDestinations)
            {
                if(!eD.Blocked)
                {
                    _availableEvacuationDestinations.Add(eD);
                }
            }
        }

        private EvacuationDestination GetRandomEvacuationDestination()
        {
            int randomChoice = Random.Range(0, _evacuationDestinations.Count);
            return _evacuationDestinations[randomChoice];
        }

        private EvacuationDestination GetRandomAvailableEvacuationDestination()
        {
            int randomChoice = Random.Range(0, _availableEvacuationDestinations.Count);
            return _availableEvacuationDestinations[randomChoice];
        }

        private EvacuationDestination GetClosestEuclideanAvailableDestination(Vector2d vehicleLatLon)
        {
            double closestDistance = double.MaxValue;
            Vector2d householdPos = _input.Simulation.Data.GetSimulationPosition(vehicleLatLon);
            EvacuationDestination pickedDestination = null;

            foreach (EvacuationDestination eD in _availableEvacuationDestinations)
            {
                Vector2d destPos = _input.Simulation.Data.GetSimulationPosition(eD.LatLon);
                double distance = Vector2d.SqrMagnitude(destPos - householdPos);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    pickedDestination = eD;
                }
            }

            return pickedDestination;
        }

        private EvacuationDestination GetClosestEuclideanDestination(Vector2d vehicleLatLon)
        {
            double closestDistance = double.MaxValue;
            Vector2d householdPos = _input.Simulation.Data.GetSimulationPosition(vehicleLatLon);
            EvacuationDestination pickedDestination = null;

            foreach (EvacuationDestination eD in _evacuationDestinations)
            {
                Vector2d destPos = _input.Simulation.Data.GetSimulationPosition(eD.LatLon);
                double distance = Vector2d.SqrMagnitude(destPos - householdPos);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    pickedDestination = eD;
                }
            }

            return pickedDestination;
        }


        private void SetDefaulEvacuationtGroup()
        {
            for(int i = 0; i < _evacuationGroups.Length; ++i)
            {
                if (_evacuationGroups[i].Default)
                {
                    _defaultEvacuationGroup = _evacuationGroups[i];
                    break;
                }
            }
        }

        private void SetDefaulDemographics(Dictionary<string, DemographicsInput> demographics)
        {
            foreach(DemographicsInput d in demographics.Values)
            {
                if(d.Default)
                {
                    _defaultDemographics = d;
                    break;
                }
            }
        }

        public EvacuationDestination GetEvacuationDestination(Vector2d latLon, EvacuationGroup evacuationGroup)
        {
            EvacuationDestination goal = null;

            if (evacuationGroup.DestinationChoice == DestinationChoices.EvacGroupCDF)
            {
                goal = evacuationGroup.GetWeightedRandomDestination();
            }
            else if (evacuationGroup.DestinationChoice == DestinationChoices.EvacGroupClosestEuclidean)
            {
                goal = evacuationGroup.GetClosestEuclideanDestination(latLon, _simulation);
            }
            else if (evacuationGroup.DestinationChoice == DestinationChoices.Random)
            {
                goal = GetRandomEvacuationDestination();

            }
            else //default to closest
            {
                goal = GetClosestEuclideanDestination(latLon);
            }

            if (goal == null)
            {
                Engine.Message(_simulation, Engine.LogType.SimulationError, "Issue with assigning evacuation destination, traffic simulation will not run.");
            }

            return goal;
        }

        public EvacuationGroup GetEvacuationGroup(Vector2d latLon, out bool insideGroup)
        {
            EvacuationGroup pickedGroup = _defaultEvacuationGroup;
            insideGroup = false;

            for (int i = 0; i < _evacuationGroups.Length; ++i)
            {
                if (_evacuationGroups[i].LatLonBelongsToGroup(latLon, _simulation))
                {
                    pickedGroup = _evacuationGroups[i];
                    insideGroup = true;
                    break;
                }
            }

            return pickedGroup;
        }

        public EvacuationDestination GetBestAvailableDestination(EvacuationGroup evacuationGroup, Vector2d latLon)
        {
            EvacuationDestination result = null;

            //TODO: actual priority pick based on random weight or proximity?
            for (int i = 0; i < evacuationGroup.Destinations.Count; ++i)
            {
                if (!evacuationGroup.Destinations[i].Blocked)
                {
                    result = evacuationGroup.Destinations[i];
                    break;
                }
            }

            //all group choices are blocked, pick something else
            if(result == null)
            {
                result = GetClosestEuclideanAvailableDestination(latLon);
            }

            return result;
        }

        public void PostRun(Stopwatch simStopwatch)
        {
            Engine.Message(_simulation, Engine.LogType.Log, "Total time spent on road closures [s]:" + _roadClosureStopwatch.ElapsedMilliseconds * 0.001 + string.Format(" [{0}%]", (int)(100.0 * _roadClosureStopwatch.ElapsedMilliseconds / simStopwatch.ElapsedMilliseconds)));
            Engine.Message(_simulation, Engine.LogType.Log, "Total time spent on initial traffic route pathfinding [s]:" + _pathfindingStopwatch.ElapsedMilliseconds * 0.001 + string.Format(" [{0}%]", (int)(100.0 * _pathfindingStopwatch.ElapsedMilliseconds / simStopwatch.ElapsedMilliseconds)));
        }
    }
}
