using PREACT.Math;
using System.Collections.Generic;
using PREACT.IO;
using PREACT.Pedestrian;
using PREACT.Wildfire;

namespace PREACT.Evacuation
{
    public class EvacuationManager
    {
        Simulation _simulation;
        PREACTInput _input;
        EvacuationGroup _defaultEvacutionGroup;        
        Dictionary<string, EvacuationDestination> _evacuationDestinationsDict;
        List<EvacuationDestination> _evacuationDestinations;
        List<EvacuationDestination> _availableEvacuationDestinations;
        EvacuationGroup[] _evacuationGroups;

        DemographicsInput _defaultDemographics;

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

        public void CheckEvacuationGoalStatus()
        {
            foreach (EvacuationDestination eD in _evacuationDestinations)
            {
                if (!eD.Blocked)
                {
                    FireCellState cellState = _simulation.FireModule.GetFireCellState(eD.LatLon);
                    if (cellState == FireCellState.Burning)
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
                _simulation.Stop("No evacuation goals available, stopping simulation.", false);
                return;
            }

            //update raster evac routes first as traffic might use some of the updated choices
            //TODO

            //update cars already in traffic
            _simulation.TrafficModule.UpdateEvacuationGoals();
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
                    _defaultEvacutionGroup = _evacuationGroups[i];
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
                GetClosestEuclideanDestination(latLon);
            }

            if (goal == null)
            {
                Engine.Message(_simulation, Engine.LogType.SimulationError, "Issue with assigning evacuation destination, traffic simulation will not run.");
            }

            return goal;
        }

        public EvacuationGroup GetEvacuationGroup(Vector2d latLon, out bool insideGroup)
        {
            EvacuationGroup pickedGroup = _defaultEvacutionGroup;
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
    }
}
