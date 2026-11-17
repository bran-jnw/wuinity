//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using PREACT.Evacuation;
using System.Numerics;
using PREACT.Visualization;
using PREACT.Input;
using PREACT.Math;

namespace PREACT.Pedestrian
{
    //Simple human evacuation simulator that lumps households of people into one unit.
    [System.Serializable]
    public class MacroHouseholdSim : PedestrianModule
    {
        List<MacroHousehold> _macroHouseholds;     
        private bool evacuationDone = false;        
        List<string> output;

        Vector4[] householdPositions;
        int _totalPopulation, _totalHouseholds, _totalVehicles;
        int _peopleRemaining, _householdsRemaining;
        int _totalPeopleWhoWillNotEvacuate, _totalHouseholdsWhoWillNotEvacuate;
        int _peopleWaitingForPickup, _householdsWaitingForPickup;
        int totalHouseholdsReacted = 0, totalPeopleReacted = 0;
        int _peopleReachedAccessPoint = 0, _householdsReachedAccessPoint = 0, _vehiclesReached;

        MacroHouseholdVisualizer _visualizer;
        public MacroHouseholdVisualizer Visualizer { get { return _visualizer; } }


        public MacroHouseholdSim(Simulation simulation) : base(simulation)
        {
            PopulateSimulation(simulation.Input.Population.Data.Households);
            output = new List<string>();
            output.Add("Time(s),Households remaining,People remaining,Total households responded, Total people responded,Total households reached access point,Total people reached access point,Total vehicles used");
        }

        public override int GetTotalPopulation()
        {
            return _totalPopulation;
        }

        public override int GetTotalHouseHolds()
        {
            return _totalHouseholds; 
        }

        int tick = 0;
        const int BATCHES = 8;
        /// <summary>
        /// Advances the macro human simulation.
        /// Loops through all cells and all households per cell and check if they have reached their goal (car)
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="currentTime"></param>
        public override void Step(double currentTime, double deltaTime)
        {
            currentTime += deltaTime; //we are checking what happens after time step
            deltaTime *= BATCHES;
            ++tick;
            int startIndex = tick % BATCHES;

            if (!evacuationDone)
            {                
                //int totalHouseholdsLeft = 0; 

                for (int i = startIndex; i < _macroHouseholds.Count; i += BATCHES)
                {
                    MacroHousehold household = _macroHouseholds[i];

                    //update position array for visualization
                    householdPositions[i] = household.GetPositionAndState(currentTime);

                    household.Step(this, currentTime, deltaTime);
                }

                _peopleRemaining = _totalPopulation - _peopleReachedAccessPoint;
                _householdsRemaining = _totalHouseholds - _householdsReachedAccessPoint;
                if (_peopleRemaining - _totalPeopleWhoWillNotEvacuate == 0)
                {
                    evacuationDone = true;
                }

                //Time(s),Households remaining,People remaining,Total households responded, Total people responded,Total households reached car,Total people reached car,Total _vehicleCount activated,Avg. walking dist.
                output.Add(currentTime + "," + _householdsRemaining + "," + _peopleRemaining + "," + totalHouseholdsReacted + "," + totalPeopleReacted + "," + _householdsReachedAccessPoint + "," + _peopleReachedAccessPoint + "," + _vehiclesReached);
                //string output = currentTime + "," +  peopleWhoReachedCar";
                //SaveToFile(output, false);
            }
        }

        public override bool IsSimulationDone()
        {
            return evacuationDone;
        }

        public Vector4[] GetHouseholdPositions()
        {
            return householdPositions;
        }

        public override int GetPeopleRemaining()
        {
            return _peopleRemaining;
        }

        public override int GetPeopleStaying()
        {
            return _totalPeopleWhoWillNotEvacuate;
        }

        public override int GetTotalVehicles()
        {
            return _totalVehicles;
        }

        public override int GetVehiclesReached()
        {
            return _vehiclesReached;
        }

        public override void SendEvacuationSignal(double simulationTime, EvacuationGroup evacuationGroup)
        {
            for(int i = 0; i < evacuationGroup.Households.Count; i++)
            {
                MacroHousehold household = evacuationGroup.Households[i];
                household.ReceiveEvacuationOrder(simulationTime, this);
            }
        }

        public void HouseholdReacted(MacroHousehold household)
        {
            ++totalHouseholdsReacted;
            totalPeopleReacted += household.PeopleInHousehold;

            if(household.CurrentMovementState == MacroHousehold.MovementState.Defending)
            {
                ++_totalHouseholdsWhoWillNotEvacuate;
                _totalPeopleWhoWillNotEvacuate += household.PeopleInHousehold;
            }
        }

        public void ReachedAccessPoint(MacroHousehold household, int numberOfVehiclesUsed)
        {
            _peopleReachedAccessPoint += household.PeopleInHousehold;
            ++_householdsReachedAccessPoint;
            _vehiclesReached += numberOfVehiclesUsed;

            if(numberOfVehiclesUsed == 0)
            {
                ++_householdsWaitingForPickup;
                _peopleWaitingForPickup += household.PeopleInHousehold;
            }

            if (_simulation.Input.TrafficModule.Enabled)
            {
                //assume all _vehicleCount in household goes to the same goal, else we have to make a new call to select goal for every car
                EvacuationDestination evacDest = _simulation.Evacuation.GetEvacuationDestination(household.GetAccessPointLatLon(), household.EvacuationGroup);

                if (evacDest != null && evacDest.Blocked)
                {
                    evacDest = _simulation.Evacuation.GetBestAvailableDestination(household.EvacuationGroup, household.GetAccessPointLatLon());
                }

                Vector2d accessPointLatLon = household.GetAccessPointLatLon();
                if (numberOfVehiclesUsed > 1)
                {
                    int peopleLeftInHousehold = household.PeopleInHousehold;
                    int vehicleIndex = 0;
                    int[] peopleInVehicle = new int[numberOfVehiclesUsed];

                    while(peopleLeftInHousehold > 0)
                    {
                        ++peopleInVehicle[vehicleIndex];
                        ++vehicleIndex;
                        if(vehicleIndex > numberOfVehiclesUsed - 1)
                        {
                            vehicleIndex = 0;
                        }
                        --peopleLeftInHousehold;
                    }

                    for (int i = 0; i < numberOfVehiclesUsed; i++)
                    {
                        _simulation.Evacuation.InsertNewCar(accessPointLatLon, evacDest, (uint)peopleInVehicle[i]);
                    }
                }
                else
                {
                    _simulation.Evacuation.InsertNewCar(accessPointLatLon, evacDest, (uint)household.PeopleInHousehold);
                }
            }
        }

        public void SaveToFile(string file)
        {            
            System.IO.File.WriteAllLines(file, output);
        }

        private void PopulateSimulation(PopulationData.HouseholdData[] householdData)
        {       
            _totalPopulation = 0;
            _totalHouseholds = householdData.Length;
            for (int i = 0; i < householdData.Length; i++)
            {
                _totalPopulation += householdData[i].peopleCount;
            }    

            _macroHouseholds = new List<MacroHousehold>();
            int culledHouseholdsOutsideDomain = 0;
            int culledPeopleOutsideDomain = 0;

            int culledHouseholdsOutsideGroups = 0;           
            int culledPeopleOutsideGroups = 0;

            int peoplewithoutVehicles = 0;
            int householdswithoutVehicles = 0;
            _totalVehicles = 0;
            for (int i = 0; i < householdData.Length; ++i)
            {
                Vector2d pos = _simulation.Spatial.GetSimulationPosition(householdData[i].originLatLon);

                //check that we are inside
                if(pos.x >= 0.0 && pos.x <= _simulation.Input.Simulation.DomainSize.x && pos.y >= 0.0 && pos.y <= _simulation.Input.Simulation.DomainSize.y)
                {
                    EvacuationGroup eG = _simulation.Evacuation.GetEvacuationGroup(householdData[i].originLatLon, out bool insideGroup);
                    if(!insideGroup && _simulation.Input.Population.CullOutsideGroups)
                    {
                        _totalPopulation -= householdData[i].peopleCount;
                        --_totalHouseholds;
                        ++culledHouseholdsOutsideGroups;
                        culledPeopleOutsideGroups += householdData[i].peopleCount;                        
                    }
                    else
                    {
                        MacroHousehold mH = new MacroHousehold(householdData[i], GetRandomWalkingSpeed(), eG, _simulation);
                        _macroHouseholds.Add(mH);
                        eG.RegisterHousehold(mH);
                        if (mH.VehicleCount > 0)
                        {                            
                            _totalVehicles += mH.VehicleCount;
                        }
                        else
                        {
                            peoplewithoutVehicles += householdData[i].peopleCount;
                            ++householdswithoutVehicles;                         
                        }
                    }                        
                }
                else
                {
                    _totalPopulation -= householdData[i].peopleCount;
                    --_totalHouseholds;
                    ++culledHouseholdsOutsideDomain;
                    culledPeopleOutsideDomain += householdData[i].peopleCount;
                }
            }

            for(int i = 0; i < _macroHouseholds.Count; ++i)
            {
                _macroHouseholds[i].CollectNeighbors();
            }

            Engine.Message(_simulation, Engine.LogType.Warning, $"Number of households/people culled outside of defined groups: {culledHouseholdsOutsideGroups}/{culledPeopleOutsideGroups}");
            Engine.Message(_simulation, Engine.LogType.Warning, $"Number of households/people culled outside of defined simulation domain: {culledHouseholdsOutsideDomain}/{culledPeopleOutsideDomain}");
            Engine.Message(_simulation, Engine.LogType.Warning, $"Number of households/people without vehicles: {householdswithoutVehicles}/{peoplewithoutVehicles}");
            Engine.Message(_simulation, Engine.LogType.Log, $"Total included population: {_totalPopulation}.");
            Engine.Message(_simulation, Engine.LogType.Log, $"Total included households: {_totalHouseholds}.");
            Engine.Message(_simulation, Engine.LogType.Log, $"Total vehicle count: {_totalVehicles}.");

            householdPositions = new Vector4[_totalHouseholds];
            _peopleRemaining = _totalPopulation;            
        }

        /// <summary>
        /// Gets random walking speed based on user input range
        /// </summary>
        /// <returns></returns>
        public float GetRandomWalkingSpeed()
        {
            MacroHouseholdSimInput eO = _simulation.Input.PedestrianModule.MacroHouseholdSimInput;
            return Random.Range(eO.WalkingSpeedMinMax.X, eO.WalkingSpeedMinMax.Y) * eO.WalkingSpeedModifier;
        }

        public override void Stop()
        {
            //there is nothing to stop;
        }

        public override void ReactToWildfire(double simulationTime)
        {
            for (int i = 0; i < _macroHouseholds.Count; ++i)
            {
                MacroHousehold household = _macroHouseholds[i];
                //if we are doing nothing or preparing as preparation might be sped up
                if(household.CurrentMovementState == MacroHousehold.MovementState.Routine || household.CurrentMovementState == MacroHousehold.MovementState.PreEvacuation)
                {
                    float distance = _simulation.Hazards.DistanceToWildfire(_macroHouseholds[i].HomePosition);
                    if (distance <= 500.0)
                    {
                        household.ReceiveEvacuationOrder(simulationTime, this);
                    }
                }
                           
            }            
        }
    }
}