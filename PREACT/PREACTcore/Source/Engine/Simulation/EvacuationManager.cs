using PREACT.Math;
using System.Collections.Generic;
using PREACT.IO;
using PREACT.Pedestrian;
using PREACT.Fire;

namespace PREACT.Evacuation
{
    public class EvacuationManager
    {
        Simulation _simulation;
        PREACTInput _input;
        EvacuationGroup _defaultEvacutionGroup;        
        Dictionary<string, EvacuationDestination> _evacuationDestinations;
        EvacuationDestination[] _evacuationDestinationsArray;
        List<EvacuationDestination> _availableEvacuationDestinations;

        public Dictionary<string, EvacuationDestination> Destinations { get => _evacuationDestinations; }
        public EvacuationDestination[] DestinationsArray { get => _evacuationDestinationsArray; }

        public EvacuationManager(Simulation simulation)
        {
            _simulation = simulation;
            _input = _simulation.Input;
            _evacuationDestinations = EvacuationDestination.CreateEvacacuationDestinationsFromInput(_simulation, _input.Evacuation.EvacuationDestinationInputs);
            SetDefaulEvacuationtGroup(); //just sets default group fallback
            BuildEvacuationDestinationArray(); //duplicate of destination but in an array, needed for random pull of destination
            BuildAvailableEvacuationDestinations();
        }

        public uint GetTotalEvacuated()
        {
            uint result = 0;
            foreach (EvacuationDestination eD in _evacuationDestinations.Values)
            {
                result += eD.CurrentPeople;
            }

            return result;
        }

        public void CheckEvacuationGoalStatus()
        {
            foreach (EvacuationDestination eD in _evacuationDestinations.Values)
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

        public void BlockDestination(string destinationName)
        {
            EvacuationDestination eD;
            if(_evacuationDestinations.TryGetValue(destinationName, out eD))
            {
                BlockDestination(eD);
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
            foreach (EvacuationDestination eD in _evacuationDestinations.Values)
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

        private void BuildEvacuationDestinationArray()
        {
            _evacuationDestinationsArray = new EvacuationDestination[_evacuationDestinations.Count];
            int index = 0;
            foreach (EvacuationDestination eD in _evacuationDestinations.Values)
            {
                _evacuationDestinationsArray[index] = eD;
                ++index;
            }
        }

        private void BuildAvailableEvacuationDestinations()
        {
            _availableEvacuationDestinations = new List<EvacuationDestination>(_evacuationDestinations.Count);
            foreach(EvacuationDestination eD in _evacuationDestinations.Values)
            {
                if(!eD.Blocked)
                {
                    _availableEvacuationDestinations.Add(eD);
                }
            }
        }

        private EvacuationDestination GetRandomEvacuationDestination()
        {
            int randomChoice = Random.Range(0, _evacuationDestinationsArray.Length);
            return _evacuationDestinationsArray[randomChoice];
        }

        private EvacuationDestination GetRandomAvailableEvacuationDestination()
        {
            int randomChoice = Random.Range(0, _availableEvacuationDestinations.Count);
            return _availableEvacuationDestinations[randomChoice];
        }

        private EvacuationDestination GetClosestEuclideanDestination(Vector2d vehicleLatLon)
        {
            double closestDistance = double.MaxValue;
            Vector2d householdPos = _input.Simulation.Data.GetSimulationPosition(vehicleLatLon);
            EvacuationDestination pickedDestination = null;

            foreach (EvacuationDestination eD in _evacuationDestinations.Values)
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
            foreach (EvacuationGroup eG in _input.Evacuation.EvacuationGroups.Values)
            {
                if (eG.Default)
                {
                    _defaultEvacutionGroup = eG;
                }
            }
        }

        public EvacuationDestination GetEvacuationDestination(MacroHousehold household)
        {
            EvacuationDestination goal = null;

            if (_input.Traffic.DestinationChoice == TrafficInput.DestinationChoices.EvacGroupWeighted)
            {
                goal = household.EvacuationGroup.GetWeightedRandomDestination(_evacuationDestinations);
            }
            else if (_input.Traffic.DestinationChoice == TrafficInput.DestinationChoices.EvacGroupClosestEuclidean)
            {
                goal = household.EvacuationGroup.GetClosestEuclideanDestination(_evacuationDestinations, household.GetVehicleLatLon(), _simulation);
            }
            else if (_input.Traffic.DestinationChoice == TrafficInput.DestinationChoices.Random)
            {
                goal = GetRandomEvacuationDestination();

            }
            else //default to closest
            {
                GetClosestEuclideanDestination(household.GetVehicleLatLon());
            }

            if (goal == null)
            {
                Engine.Message(_simulation, Engine.LogType.SimulationError, "Issue with assigning evacuation destination, traffic simulation will not run.");
            }

            return goal;
        }

        public EvacuationGroup GetEvacuationGroup(Vector2d latLon)
        {
            EvacuationGroup pickedGroup = _defaultEvacutionGroup;

            foreach (EvacuationGroup eG in _input.Evacuation.EvacuationGroups.Values)
            {
                if (eG.LatLonBelongsToGroup(latLon, _simulation))
                {
                    pickedGroup = eG;
                    break;
                }
            }

            return pickedGroup;
        }

        public EvacuationDestination GetBestAvailableDestination(EvacuationGroup evacuationGroup)
        {
            EvacuationDestination result = null;

            foreach(string destName in evacuationGroup.DestinationNames)
            {
                //TODO: actual priority pick based on random weight or proximity?
                EvacuationDestination evacuationDestination;
                if(_evacuationDestinations.TryGetValue(destName, out evacuationDestination))
                {
                    if(!evacuationDestination.Blocked)
                    {
                        result = evacuationDestination;
                        break;
                    }
                }
            }

            //all group choices are blocked, pick something else
            if(result == null)
            {
                result = GetRandomAvailableEvacuationDestination();
            }

            return result;
        }
    }
}
