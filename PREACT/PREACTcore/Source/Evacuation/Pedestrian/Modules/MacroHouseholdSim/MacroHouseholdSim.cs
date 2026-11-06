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
    /// <summary>
    /// Simple human evacuation simulator that lumps households of people into one unit.
    /// Household position is randomized within a cell that has been discretized from the world plane.
    /// </summary>
    [System.Serializable]
    public class MacroHouseholdSim : PedestrianModule
    {
        List<MacroHousehold> _macroHouseholds;
        int totalPopulation;
        int totalVehicles;
        int totalVehiclesReached;
        int totalPeopleWhoWillNotEvacuate;
        private bool evacuationDone = false;
        int peopleLeft;
        List<string> output;

        int totalHouseholds;
        Vector4[] householdPositions;
        int totalHouseholdsReacted = 0, totalHouseholdsReachedVehicle = 0;
        int totalPeopleReachedVehicle = 0;
        int totalPeopleReacted = 0;

        MacroHouseholdVisualizer _visualizer;
        public MacroHouseholdVisualizer Visualizer { get { return _visualizer; } }


        public MacroHouseholdSim(Simulation simulation) : base(simulation)
        {
            PopulateSimulation(simulation.Input.Population.Data.Households);
            output = new List<string>();
            output.Add("Time(s),Households left,People left,Total households responded, Total people responded,Total households reached car,Total people reached car,Total vehicleCount activated,Avg. walking dist.");
        }

        public override int GetTotalPopulation()
        {
            return totalPopulation;
        }

        public override int GetTotalHouseHolds()
        {
            return totalHouseholds; 
        }

        /// <summary>
        /// Advances the macro human simulation.
        /// Loops through all cells and all households per cell and check if they have reached their goal (car)
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="currentTime"></param>
        public override void Step(double currentTime, double deltaTime)
        {            
            if (!evacuationDone)
            {                
                float walkingDistance = 0f;
                int householdsDone = 0;
                int householdIndex = 0;
                int totalHouseholdsLeft = 0; 

                for (int i = 0; i < _macroHouseholds.Count; ++i)
                {
                    MacroHousehold household = _macroHouseholds[i];

                    //update position array for visualization
                    householdPositions[householdIndex] = household.GetPositionAndState(currentTime);
                    ++householdIndex;

                    if(household.CurrentState == MacroHousehold.State.Defending)
                    {
                        totalHouseholdsLeft++;
                        continue;
                    }

                    household.Step(this, currentTime, deltaTime);

                    if(household.CurrentState == MacroHousehold.State.EvacByVehicle)
                    {
                        //walkingDistance += household.walkingDistance;
                        ++householdsDone;
                    }
                    else
                    {
                        ++totalHouseholdsLeft;
                    }
                }

                peopleLeft = totalPopulation - totalPeopleReachedVehicle;
                if (peopleLeft - totalPeopleWhoWillNotEvacuate == 0)
                {
                    evacuationDone = true;
                }

                float avgWalkDist = 0f;
                if (householdsDone > 0)
                {
                    avgWalkDist = walkingDistance / householdsDone;
                }

                //Time(s),Households left,People left,Total households responded, Total people responded,Total households reached car,Total people reached car,Total vehicleCount activated,Avg. walking dist.
                output.Add(currentTime + "," + totalHouseholdsLeft + "," + peopleLeft + "," + totalHouseholdsReacted + "," + totalPeopleReacted + "," + totalHouseholdsReachedVehicle + "," + totalPeopleReachedVehicle + "," + totalVehiclesReached + "," + avgWalkDist);
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

        public override int GetPeopleLeft()
        {
            return peopleLeft;
        }

        public override int GetPeopleStaying()
        {
            return totalPeopleWhoWillNotEvacuate;
        }

        public override int GetTotalCars()
        {
            return totalVehicles;
        }

        public override int GetVehiclesReached()
        {
            return totalVehiclesReached;
        }

        public override void SendEvacuationSignal(double simulationTime, EvacuationGroup evacuationGroup)
        {
            for(int i = 0; i < evacuationGroup.Households.Count; i++)
            {
                MacroHousehold household = evacuationGroup.Households[i];
                household.StartEvacuation(simulationTime, this);
            }
        }

        public void HouseholdReacted(MacroHousehold household)
        {
            ++totalHouseholdsReacted;
            totalPeopleReacted += household.PeopleInHousehold;
            //for now waiting for pickup counts as staying until we have a model that can actually pick them up
            if(household.CurrentState == MacroHousehold.State.Defending || household.CurrentState == MacroHousehold.State.WaitingForPickup)
            {
                totalPeopleWhoWillNotEvacuate += household.PeopleInHousehold;
            }
        }

        public void ReachedVehicle(MacroHousehold household)
        {
            totalPeopleReachedVehicle += household.PeopleInHousehold;
            ++totalHouseholdsReachedVehicle;
            totalVehiclesReached += household.vehicleCount;

            if (_simulation.Input.TrafficModule.Enabled)
            {
                //assume all vehicleCount in household goes to the same goal, else we have to make a new call to select goal for every car
                EvacuationDestination evacDest = _simulation.Evacuation.GetEvacuationDestination(household.GetVehicleLatLon(), household.EvacuationGroup);

                if (evacDest != null && evacDest.Blocked)
                {
                    _simulation.Evacuation.GetBestAvailableDestination(household.EvacuationGroup, household.GetVehicleLatLon());
                }

                Vector2d vehicleLatLon = household.GetVehicleLatLon();
                if (household.vehicleCount > 1)
                {
                    int peopleLeftInHousehold = household.PeopleInHousehold;
                    int carIndex = 0;
                    int[] peopleInCar = new int[household.vehicleCount];

                    while(peopleLeftInHousehold > 0)
                    {
                        ++peopleInCar[carIndex];
                        ++carIndex;
                        if(carIndex > household.vehicleCount - 1)
                        {
                            carIndex = 0;
                        }
                        --peopleLeftInHousehold;
                    }

                    for (int i = 0; i < household.vehicleCount; i++)
                    {
                        _simulation.Evacuation.InsertNewCar(vehicleLatLon, evacDest, (uint)peopleInCar[i]);
                    }
                }
                else
                {
                    _simulation.Evacuation.InsertNewCar(vehicleLatLon, evacDest, (uint)household.PeopleInHousehold);
                }
            }
        }

        public void SaveToFile(string file)
        {            
            System.IO.File.WriteAllLines(file, output);
        }

        public void PopulateSimulation(PopulationData.HouseholdData[] householdData)
        {       
            totalPopulation = 0;
            totalHouseholds = householdData.Length;
            for (int i = 0; i < householdData.Length; i++)
            {
                totalPopulation += householdData[i].peopleCount;
            }    

            _macroHouseholds = new List<MacroHousehold>();
            int culledHouseholdsOutsideDomain = 0;
            int culledPeopleOutsideDomain = 0;

            int culledHouseholdsOutsideGroups = 0;           
            int culledPeopleOutsideGroups = 0;

            int peoplewithoutVehicles = 0;
            int householdswithoutVehicles = 0;
            totalVehicles = 0;
            for (int i = 0; i < householdData.Length; ++i)
            {
                Vector2d pos = _simulation.Spatial.GetSimulationPosition(householdData[i].originLatLon);

                //check that we are inside
                if(pos.x >= 0.0 && pos.x <= _simulation.Input.Simulation.DomainSize.x && pos.y >= 0.0 && pos.y <= _simulation.Input.Simulation.DomainSize.y)
                {
                    bool insideGroup;
                    EvacuationGroup eG = _simulation.Evacuation.GetEvacuationGroup(householdData[i].originLatLon, out insideGroup);
                    if(!insideGroup && _simulation.Input.Population.CullOutsideGroups)
                    {
                        totalPopulation -= householdData[i].peopleCount;
                        --totalHouseholds;
                        ++culledHouseholdsOutsideGroups;
                        culledPeopleOutsideGroups += householdData[i].peopleCount;                        
                    }
                    else
                    {
                        MacroHousehold mH = new MacroHousehold(householdData[i], GetRandomWalkingSpeed(), eG, _simulation);
                        if(mH.vehicleCount > 0)
                        {
                            _macroHouseholds.Add(mH);
                            eG.RegisterHousehold(mH);
                            totalVehicles += mH.vehicleCount;
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
                    totalPopulation -= householdData[i].peopleCount;
                    --totalHouseholds;
                    ++culledHouseholdsOutsideDomain;
                    culledPeopleOutsideDomain += householdData[i].peopleCount;
                }
            }

            Engine.Message(_simulation, Engine.LogType.Warning, $"Number of households/people culled outside of defined groups: {culledHouseholdsOutsideGroups}/{culledPeopleOutsideGroups}");
            Engine.Message(_simulation, Engine.LogType.Warning, $"Number of households/people culled outside of defined simulation domain: {culledHouseholdsOutsideDomain}/{culledPeopleOutsideDomain}");
            Engine.Message(_simulation, Engine.LogType.Warning, $"Number of households/people without vehicles: {householdswithoutVehicles}/{peoplewithoutVehicles}");

            householdPositions = new Vector4[totalHouseholds];
            peopleLeft = totalPopulation;

            Engine.Message(_simulation, Engine.LogType.Log, $"Total active population: {totalPopulation}.");
            Engine.Message(_simulation, Engine.LogType.Log, $"Total active households: {totalHouseholds}.");
            Engine.Message(_simulation, Engine.LogType.Log, $"Total active vehicle count: {totalVehicles}.");
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
                if(household.CurrentState == MacroHousehold.State.Routine || household.CurrentState == MacroHousehold.State.PreEvacuation)
                {
                    float distance = _simulation.Hazards.DistanceToWildfire(_macroHouseholds[i].HomePosition);
                    if (distance <= 500.0)
                    {
                        household.StartEvacuation(simulationTime, this);
                    }
                }
                           
            }            
        }
    }
}