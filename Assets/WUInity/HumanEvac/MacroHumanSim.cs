using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WUInity.Population;

namespace WUInity.Evac
{
    /// <summary>
    /// Simple human evacuation simulator that lumps households of people into one unit.
    /// Household position is randomized within a cell that has been discretized from the world plane.
    /// </summary>
    [System.Serializable]
    public class MacroHumanSim
    {
        int[] population;
        int cellsX;
        int cellsY;
        int maxPop = -1;

        Vector2D realWorldSize;
        public Vector2D cellWorldSize;
        RouteCollection[] cellRoutes;
        HumanEvacCell[] humanEvacCells;
        int totalPopulation;
        int totalCars;
        int totalCarsReached;
        int totalPeopleWhoWillNotEvacuate;
        public bool evacuationDone = false;
        int peopleLeft;
        List<string> output;

        int totalHouseholds;
        Vector4[] householdPositions;


        public MacroHumanSim()
        {
            output = new List<string>();
            output.Add("Time(s),Households left,People left,Households starting moving,People started moving,Households reached car,People reached car,Total cars activated,Avg. walking dist.");
            //string output = "Time(s),People reached car";
            //SaveToFile(output, true);
        }

        /// <summary>
        /// Returns the amount of people still left in a cell.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int GetPeopleLeftInCell(int x, int y)
        {
            int count = 0;

            if (humanEvacCells[x + y * cellsX] != null)
            {
                HumanEvacCell hR = humanEvacCells[x + y * cellsX];
                for (int j = 0; j < hR.macroHouseholds.Length; ++j)
                {
                    if (hR.macroHouseholds[j] != null)
                    {
                        MacroHousehold rH = hR.macroHouseholds[j];
                        if (!rH.reachedCar)
                        {
                            count += rH.peopleInHousehold;
                        }
                    }
                }
            }
            return count;
        }

        public int GetPeopleLeftInCellIntendingToLeave(int index)
        {
            int count = 0;

            if (humanEvacCells[index] != null)
            {
                HumanEvacCell hR = humanEvacCells[index];
                for (int j = 0; j < hR.macroHouseholds.Length; ++j)
                {
                    if (hR.macroHouseholds[j] != null)
                    {
                        MacroHousehold rH = hR.macroHouseholds[j];
                        if (!rH.reachedCar && rH.evacuationTime < float.MaxValue)
                        {
                            count += rH.peopleInHousehold;
                        }
                    }
                }
            }

            if(count == 0)
            {
                humanEvacCells[index].cellIsEvacuated = true;
            }
            return count;
        }
        
        public bool IsCellEvacuated(int index)
        {
            return humanEvacCells[index].cellIsEvacuated;
        }

        /// <summary>
        /// Advances the macro human simulation.
        /// Loops through all cells and all households per cell and check if they have reached their goal (car)
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="currentTime"></param>
        public void Update(float deltaTime, float currentTime)
        {            
            if (!evacuationDone)
            {
                int peopleWhoReachedCar = 0;
                int peopleStartedMoving = 0;
                int householdsLeft = 0, householdsStartedMoving = 0, householdsReachedCar = 0;
                float walkingDistance = 0f;
                int householdsDone = 0;
                int householdIndex = 0;

                for (int i = 0; i < humanEvacCells.Length; ++i)
                {
                    if (population[i] > 0)
                    {
                        HumanEvacCell cell = humanEvacCells[i];
                        for (int j = 0; j < cell.macroHouseholds.Length; ++j)
                        {
                            MacroHousehold household = cell.macroHouseholds[j];

                            //update position array for visualization
                            householdPositions[householdIndex] = household.GetPositionAndState(currentTime);
                            ++householdIndex;

                            //if evac time is float.MaxValue they have decided to stay forever
                            if (household.evacuationTime != float.MaxValue)
                            {
                                if(!household.reachedCar)
                                {
                                    //see if the household has reacted yet, if so, set them in motion
                                    if (!household.isMoving && household.responseTime <= currentTime)
                                    {
                                        household.isMoving = true;
                                        ++householdsStartedMoving;
                                        peopleStartedMoving += household.peopleInHousehold;
                                    }

                                    //if we yet have not reached our car, check if we have
                                    if (household.evacuationTime <= currentTime)
                                    {
                                        ReachedCar(household, cell, ref peopleWhoReachedCar);
                                        ++householdsReachedCar;
                                        totalCarsReached += household.cars;
                                    }
                                    else
                                    {
                                        ++householdsLeft;
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
                                householdsLeft++;
                            }
                        }
                    }
                }

                peopleLeft -= peopleWhoReachedCar;
                if (peopleLeft - totalPeopleWhoWillNotEvacuate == 0)
                {
                    evacuationDone = true;
                }

                float avgWalkDist = 0f;
                if (householdsDone > 0)
                {
                    avgWalkDist = walkingDistance / householdsDone;
                }

                //Time(s),Households left,People left,Households starting moving,People started moving,Households reached car,People reached car,Cars activated
                output.Add(currentTime + "," + householdsLeft + "," + peopleLeft + "," + householdsStartedMoving + "," + peopleStartedMoving + "," + householdsReachedCar + "," + peopleWhoReachedCar + "," + totalCarsReached + "," + avgWalkDist);
                //string output = currentTime + "," +  peopleWhoReachedCar";
                //SaveToFile(output, false);
            }
        }

        public Vector4[] GetHouseholdPositions()
        {
            return householdPositions;
        }

        public int GetPeopleLeft()
        {
            return peopleLeft;
        }

        public int GetPeopleStaying()
        {
            return totalPeopleWhoWillNotEvacuate;
        }

        public int GetTotalCars()
        {
            return totalCars;
        }

        public int GetCarsReached()
        {
            return totalCarsReached;
        }

        private void ReachedCar(MacroHousehold household, HumanEvacCell cell, ref int peopleWhoReachedCar)
        {
            if(WUInity.INPUT.runTrafficSim)
            {
                if (household.cars > 1)
                {
                    int peopleLeftInHousehold = household.peopleInHousehold;
                    int carIndex = 0;
                    int[] cars = new int[household.cars];

                    while(peopleLeftInHousehold > 0)
                    {
                        ++cars[carIndex];
                        ++carIndex;
                        if(carIndex > household.cars - 1)
                        {
                            carIndex = 0;
                        }
                        --peopleLeftInHousehold;
                    }

                    for (int i = 0; i < household.cars; i++)
                    {
                        WUInity.SIM.GetMacroTrafficSim().InsertNewCar(cell.rasterRoute.GetSelectedRoute(), cars[i]);
                    }
                }
                else
                {
                    WUInity.SIM.GetMacroTrafficSim().InsertNewCar(cell.rasterRoute.GetSelectedRoute(), household.peopleInHousehold);
                }
            }
            
            household.reachedCar = true;
            //update counter
            peopleWhoReachedCar += household.peopleInHousehold;
        }

        public void SaveToFile(int runNumber)
        {
            WUInityInput wO = WUInity.INPUT;
            string path = System.IO.Path.Combine(WUInity.OUTPUT_FOLDER, wO.simName + "_pedestrian_output_" + runNumber + ".csv");
            System.IO.File.WriteAllLines(path, output);
        }

        /// <summary>
        /// Populates cells based on loaded data.
        /// </summary>
        public void PopulateCells(RouteCollection[] routeCollection, PopulationData populationData)
        {          
            cellsX = WUInity.SIM_DATA.EvacCellCount.x;
            cellsY = WUInity.SIM_DATA.EvacCellCount.x;
            this.realWorldSize = WUInity.INPUT.size;
            population = new int[cellsX * cellsY];

            cellRoutes = routeCollection;

            double cellSizeX = realWorldSize.x / cellsX;
            double cellSizeY = realWorldSize.y / cellsY;
            cellWorldSize = new Vector2D(cellSizeX, cellSizeY);
            totalPopulation = populationData.totalPopulation;
            peopleLeft = totalPopulation;
            maxPop = int.MinValue;
            for (int i = 0; i < population.Length; ++i)
            { 
                population[i] = populationData.cellPopulation[i];
                if (populationData.cellPopulation[i] > maxPop)
                {
                    maxPop = populationData.cellPopulation[i];
                }
            }            
        }

        /// <summary>
        /// Pulls random response time along a response time curve
        /// </summary>
        /// <returns></returns>
        static public float GetRandomResponseTime(int evacGroupIndex)
        {
            EvacInput eO = WUInity.INPUT.evac;

            float responseTime = float.MaxValue;
            float r = Random.Range(0f, 1f);

            //get curve index from evac group
            int curveIndex = WUInity.INPUT.evac.evacGroups[evacGroupIndex].responseCurveIndex;

            for (int i = 0; i < eO.responseCurves[curveIndex].dataPoints.Length; i++)
            {
                if (r <= eO.responseCurves[curveIndex].dataPoints[i].probability)
                {
                    responseTime = Random.Range(eO.responseCurves[curveIndex].dataPoints[i].timeMinMax.x, eO.responseCurves[curveIndex].dataPoints[i].timeMinMax.y);
                    break;
                }
            }

            return responseTime;
        }

        /// <summary>
        /// Gets random walking speed based on user input range
        /// </summary>
        /// <returns></returns>
        static public float GetRandomWalkingSpeed()
        {
            EvacInput eO = WUInity.INPUT.evac;
            return Random.Range(eO.walkingSpeedMinMax.x, eO.walkingSpeedMinMax.y) * eO.walkingSpeedModifier;
        }

        /// <summary>
        /// Creates the human evac cell if cell contains population and deploys households in all of the cells based on total amount of people per cell
        /// </summary>
        public void PlaceHouseholdsInCells()
        {
            totalHouseholds = 0;
            humanEvacCells = new HumanEvacCell[population.Length];
            for (int i = 0; i < humanEvacCells.Length; ++i)
            {
                if (population[i] > 0)
                {
                    int y = i / cellsX;
                    int x = i - y * cellsX;
                    Vector2D worldPos = new Vector2D((x + 0.5) * cellWorldSize.x, (y + 0.5) * cellWorldSize.y);
                    humanEvacCells[i] = new HumanEvacCell(worldPos, cellWorldSize, cellRoutes[i], population[i]);
                    totalHouseholds += humanEvacCells[i].macroHouseholds.Length;
                }
            }

            //sum up the number of people which will not evacuate and total cars
            totalPeopleWhoWillNotEvacuate = 0;
            totalCars = 0;
            for (int i = 0; i < humanEvacCells.Length; ++i)
            {
                if (population[i] > 0)
                {
                    HumanEvacCell hR = humanEvacCells[i];
                    for (int j = 0; j < hR.macroHouseholds.Length; j++)
                    {
                        MacroHousehold mH  = hR.macroHouseholds[j];
                        if (mH.responseTime == float.MaxValue)
                        {
                            totalPeopleWhoWillNotEvacuate += mH.peopleInHousehold;
                        }
                        else
                        {
                            totalCars += mH.cars;
                        }                        
                    }
                }
            }

            householdPositions = new Vector4[totalHouseholds];

            WUInity.WUI_LOG("LOG: Total households: " + totalHouseholds);
            WUInity.WUI_LOG("LOG: Total cars: " +  totalCars);
            WUInity.WUI_LOG("LOG: Total people who will not evacuate: " + totalPeopleWhoWillNotEvacuate);
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

        /// <summary>
        /// Get number of people in cell base don "world space" coordinates. Clamps to dimensions of defined area.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int GetPopulationUnitySpace(double x, double y)
        {
            int xInt = (int)((x / realWorldSize.x) * cellsX);
            int yInt = (int)((y / realWorldSize.y) * cellsY);
            return GetPopulation(xInt, yInt);
        }     

        /// <summary>
        /// Creates texture that shown cells where people have decided to stay forever.
        /// </summary>
        /// <returns></returns>
        public Texture2D CreateStayingPopulationTexture()
        {
            Vector2Int res = new Vector2Int(2, 2);

            while (cellsX > res.x)
            {
                res.x *= 2;
            }
            while (cellsY > res.y)
            {
                res.y *= 2;
            }

            Texture2D popTex = new Texture2D(res.x, res.y);
            popTex.filterMode = FilterMode.Point;

            for (int y = 0; y < cellsY; y++)
            {
                for (int x = 0; x < cellsX; x++)
                {
                    int peopleStaying = 0;
                    if (humanEvacCells[x + y * cellsX] != null)
                    {
                        MacroHousehold[] rH = humanEvacCells[x + y * cellsX].macroHouseholds;
                        for (int i = 0; i < rH.Length; i++)
                        {
                            if (rH[i].evacuationTime == float.MaxValue)
                            {
                                peopleStaying += rH[i].peopleInHousehold;
                            }
                        }
                    }
                    Color color = GetStayColor(peopleStaying, population[x + y * cellsX]);
                    popTex.SetPixel(x, y, color);
                }
            }
            popTex.Apply();
            return popTex;
        }

        private Color GetStayColor(int peopleStaying, int actualPopInCell)
        {
            Color color = Color.Lerp(Color.yellow, Color.red, peopleStaying / 20f);
            if (actualPopInCell == 0 || peopleStaying == 0)
            {
                color.a = 0f;
            }
            return color;
        }
    }
}