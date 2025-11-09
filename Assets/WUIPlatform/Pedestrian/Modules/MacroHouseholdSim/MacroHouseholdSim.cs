//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using PREACT.Evacuation;
using System.Numerics;
using PREACT.Visualization;
using PREACT.IO;
using PREACT.Utility.Math;

namespace PREACT.Pedestrian
{
    /// <summary>
    /// Simple human evacuation simulator that lumps households of people into one unit.
    /// Household position is randomized within a cell that has been discretized from the world plane.
    /// </summary>
    [System.Serializable]
    public class MacroHouseholdSim : PedestrianModule
    {
        int[] population;
        int cellsX;
        int cellsY;

        Vector2d realWorldSize;
        public Vector2d cellWorldSize;

        IO.PopulationData.HouseholdData[] _householdData;
        List<MacroHousehold> _macroHouseholds;
        int totalPopulation;
        int totalCars;
        int totalCarsReached;
        int totalPeopleWhoWillNotEvacuate;
        private bool evacuationDone = false;
        int peopleLeft;
        List<string> output;

        int totalHouseholds;
        Vector4[] householdPositions;
        int totalHouseholdsResponded = 0, totalHouseholdsReachedCar = 0;
        int totalPeopleReachedCar = 0;
        int totalPeopleResponded = 0;

        MacroHouseholdVisualizer _visualizer;
        public MacroHouseholdVisualizer Visualizer { get { return _visualizer; } }


        public MacroHouseholdSim(Simulation simulation) : base(simulation)
        {
            output = new List<string>();
            output.Add("Time(s),Households left,People left,Total households responded, Total people responded,Total households reached car,Total people reached car,Total cars activated,Avg. walking dist.");
        }

        public int GetCellsX()
        {
            return cellsX;
        }

        public int GetCellsY()
        {
            return cellsY;
        }

        public override int GetTotalPopulation()
        {
            return totalPopulation;
        }

        public override int GetTotalHouseHolds()
        {
            return totalHouseholds; 
        }

        public int[] GetPopulation() 
        {
            return population;
        }

        /// <summary>
        /// Advances the macro human simulation.
        /// Loops through all cells and all households per cell and check if they have reached their goal (car)
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="currentTime"></param>
        public override void Step(float currentTime, float deltaTime)
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

                    //if evac time is float.MaxValue they have decided to stay forever
                    if (household.evacuationTime < float.MaxValue)
                    {
                        if(!household.reachedCar)
                        {                                                                      
                            //see if the household has reacted yet, if so, set them in motion
                            if (!household.isMoving && household.responseTime <= currentTime)
                            {
                                household.isMoving = true;
                                ++totalHouseholdsResponded;
                                totalPeopleResponded += household.peopleInHousehold;
                            }

                            //if we yet have not reached our car, check if we have
                            if (household.evacuationTime <= currentTime)
                            {
                                ReachedCar(household);
                                totalPeopleReachedCar += household.peopleInHousehold;
                                ++totalHouseholdsReachedCar;
                                totalCarsReached += household.cars;
                            }
                            else
                            {
                                ++totalHouseholdsLeft;
                            }
                        } 
                        else
                        {
                            walkingDistance += household.walkingDistance;
                            ++householdsDone;
                        }
                    }
                    else
                    {
                        totalHouseholdsLeft++;                                
                    }
                }

                peopleLeft = totalPopulation - totalPeopleReachedCar;
                if (peopleLeft - totalPeopleWhoWillNotEvacuate == 0)
                {
                    evacuationDone = true;
                }

                float avgWalkDist = 0f;
                if (householdsDone > 0)
                {
                    avgWalkDist = walkingDistance / householdsDone;
                }

                //Time(s),Households left,People left,Total households responded, Total people responded,Total households reached car,Total people reached car,Total cars activated,Avg. walking dist.
                output.Add(currentTime + "," + totalHouseholdsLeft + "," + peopleLeft + "," + totalHouseholdsResponded + "," + totalPeopleResponded + "," + totalHouseholdsReachedCar + "," + totalPeopleReachedCar + "," + totalCarsReached + "," + avgWalkDist);
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
            return totalCars;
        }

        public override int GetCarsReached()
        {
            return totalCarsReached;
        }

        private void ReachedCar(MacroHousehold household)
        {
            if(_simulation.Input.Simulation.RunTrafficModule)
            {
                //assume all cars in household goes to the same goal, else we have to make a new call to select goal for every car
                EvacuationDestination evacGoal = GetEvacuationGoal(null, household.GetCellIndex());

                //TODO: more sophisticated choice of new goal
                if (evacGoal.Blocked)
                {
                    for (int i = 0; i < _simulation.Destinations.Count; i++)
                    {
                        if (_simulation.Destinations[i] != evacGoal)
                        {
                            if(!_simulation.Destinations[i].Blocked)
                            {
                                evacGoal = _simulation.Destinations[i];
                            }
                        }
                    }
                }
                Vector2d carLatLon = household.GetCarLatLon();


                if (household.cars > 1)
                {
                    int peopleLeftInHousehold = household.peopleInHousehold;
                    int carIndex = 0;
                    int[] peopleInCar = new int[household.cars];

                    while(peopleLeftInHousehold > 0)
                    {
                        ++peopleInCar[carIndex];
                        ++carIndex;
                        if(carIndex > household.cars - 1)
                        {
                            carIndex = 0;
                        }
                        --peopleLeftInHousehold;
                    }

                    for (int i = 0; i < household.cars; i++)
                    {
                        _simulation.InsertNewCar(carLatLon, evacGoal, (uint)peopleInCar[i]);
                    }
                }
                else
                {
                    _simulation.InsertNewCar(carLatLon, evacGoal, (uint)household.peopleInHousehold);
                }
            }
            
            household.reachedCar = true;
        }

        private EvacuationDestination GetEvacuationGoal(HumanEvacCell cell, int cellIndex)
        {
            EvacuationDestination goal = null;

            if (_simulation.Input.Traffic.TrafficModule == TrafficInput.TrafficModuleChoice.SUMO)
            {                
                if (_simulation.Input.Traffic.SumoInput.DestinationChoice == SUMOInput.DestinationChoiceEnum.EvacGroup)
                {
                    EvacuationGroup group = _simulation.Input.Evacuation.Data.GetEvacGroup(cellIndex);
                    goal = group.GetWeightedRandomDestination(_simulation.Destinations);
                }
                else if (_simulation.Input.Traffic.SumoInput.DestinationChoice == SUMOInput.DestinationChoiceEnum.Random)
                {
                    int randomChoice = Randomf.Range(0, _simulation.Destinations.Count - 1);
                    goal = _simulation.Destinations[randomChoice];
                }
            }
            else if(cell != null && _simulation.Input.Traffic.TrafficModule == TrafficInput.TrafficModuleChoice.MacroTrafficSim)
            {
                //this call picks new random route from route collection based on group goal probabilities (if groups are in use)
                Traffic.RouteCreator.UpdateRouteCollectionBasedOnRouteChoice(_simulation, cell.routeCollection, cell.GetCellIndex());
            }

            if(goal == null)
            {
                Engine.Message(null, Engine.LogType.SimulationError, "Issue with assigning evacuation goal in MacroHouseholdSim, traffic simulation will not run.");
            }

            return goal;
        }

        public void SaveToFile(string file)
        {            
            System.IO.File.WriteAllLines(file, output);
        }

        public void PopulateSimulation(IO.PopulationData.HouseholdData[] householdData)
        {
            cellsX = _simulation.Input.Evacuation.Data.CellCount.x;
            cellsY = _simulation.Input.Evacuation.Data.CellCount.y;
            realWorldSize = _simulation.Input.Simulation.DomainSize;            
            population = new int[cellsX * cellsY];
            _householdData = householdData;


            double cellSizeX = realWorldSize.x / cellsX;
            double cellSizeY = realWorldSize.y / cellsY;
            cellWorldSize = new Vector2d(cellSizeX, cellSizeY);
            totalPopulation = 0;
            totalHouseholds = _householdData.Length;

            for (int i = 0; i < _householdData.Length; i++)
            {
                totalPopulation += _householdData[i].peopleCount;
            }    

            _macroHouseholds = new List<MacroHousehold>();
            for (int i = 0; i < _householdData.Length; ++i)
            {
                Vector2d pos = _simulation.GetSimulationPosition(_householdData[i].originLatLon);
                int xIndex = (int)(pos.x / cellSizeX);
                int yIndex = (int)(pos.y / cellSizeY);

                //check that we are inside
                if(xIndex >= 0 && xIndex < cellsX && yIndex >= 0 && yIndex < cellsY)
                {
                    int cellIndex = xIndex + cellsX * yIndex;
                    population[cellIndex] += _householdData[i].peopleCount;
                    int evacGroupIndex = _simulation.Input.Evacuation.Data.EvacGroupIndices[cellIndex];
                    MacroHousehold mH = new MacroHousehold(_householdData[i], GetRandomWalkingSpeed(), GetRandomResponseTime(evacGroupIndex), cellIndex, _simulation);
                    _macroHouseholds.Add(mH);
                }
                else
                {
                    totalPopulation -= _householdData[i].peopleCount;
                    --totalHouseholds;
                    Engine.Message(null, Engine.LogType.Warning, "Household is outside simulation boundary, ignoring. Lat/Lon/row: " + _householdData[i].originLatLon.x + ", " + _householdData[i].originLatLon.y + ", " + (i + 2));
                }
            }            

            //sum up the number of people which will not evacuate and total cars
            totalPeopleWhoWillNotEvacuate = 0;
            totalCars = 0;
            for (int i = 0; i < _macroHouseholds.Count; ++i)
            {
                if (_macroHouseholds[i].responseTime < float.MaxValue)
                {
                    totalCars += _macroHouseholds[i].cars;
                }
                else
                {
                    totalPeopleWhoWillNotEvacuate += _macroHouseholds[i].peopleInHousehold;
                }
            }

            householdPositions = new Vector4[totalHouseholds];
            peopleLeft = totalPopulation;

            Engine.Message(null, Engine.LogType.Log, " Total households: " + totalHouseholds);
            Engine.Message(null, Engine.LogType.Log, " Total cars: " + totalCars);
            Engine.Message(null, Engine.LogType.Log, " Total people who will not evacuate: " + totalPeopleWhoWillNotEvacuate);
        }

        /// <summary>
        /// Pulls random response time along a response time curve
        /// </summary>
        /// <returns></returns>
        public float GetRandomResponseTime(int evacGroupIndex)
        {
            EvacuationInput evacIn = _simulation.Input.Evacuation;

            float responseTime = float.MaxValue;
            float r = Randomf.Range(0f, 1f);
            //get curve index from evac group
            int randomResponseCurveIndex = 0;
            for (int i = 0; i < _simulation.Input.Evacuation.Data.EvacuationGroups[evacGroupIndex].ResponseCurveIndices.Length; i++)
            {
                if(r <= _simulation.Input.Evacuation.Data.EvacuationGroups[evacGroupIndex].DestinationCumulativeWeights[i])
                {
                    randomResponseCurveIndex = i;
                    break;
                }
            }
            int curveIndex = _simulation.Input.Evacuation.Data.EvacuationGroups[evacGroupIndex].ResponseCurveIndices[randomResponseCurveIndex];


            //skip first as that is always zero probability
            for (int i = 1; i < _simulation.Input.Evacuation.Data.ResponseCurves[curveIndex].dataPoints.Length; i++)
            {
                if (r <= _simulation.Input.Evacuation.Data.ResponseCurves[curveIndex].dataPoints[i].probability)
                {
                    //offset with evacuation order time
                    responseTime = Randomf.Range(_simulation.Input.Evacuation.Data.ResponseCurves[curveIndex].dataPoints[i - 1].time + evacIn.EvacuationOrderStart, _simulation.Input.Evacuation.Data.ResponseCurves[curveIndex].dataPoints[i].time) + evacIn.EvacuationOrderStart;
                    break;
                }
            }

            return responseTime;
        }

        /// <summary>
        /// Gets random walking speed based on user input range
        /// </summary>
        /// <returns></returns>
        public float GetRandomWalkingSpeed()
        {
            MacroHouseholdSimInput eO = _simulation.Input.Pedestrian.MacroHouseholdSimInput;
            return Randomf.Range(eO.WalkingSpeedMinMax.X, eO.WalkingSpeedMinMax.Y) * eO.WalkingSpeedModifier;
        }

        /// <summary>
        /// Gets number of poeple in a cell. Clamps x and y to be within array.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int GetPopulation(int x, int y)
        {
            if (x < 0 || x > cellsX - 1 || y < 0 || y > cellsY - 1)
            {
                //Debug.Log("Population density was polled outside of data coverage.");
                return 0;
            }
            //x = Mathf.Clamp(x, 0, xSize - 1);
            //y = Mathf.Clamp(y, 0, ySize - 1);
            return population[x + y * cellsX];
        }

        public override void Stop()
        {
            //there is nothing to stop;
        }
    }
}