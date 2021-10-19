using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WUInity.GPW;

namespace WUInity.Evac
{
    /// <summary>
    /// Simple human evacuation simulator that lumps households of people into one unit.
    /// Household position is randomized within a cell that has been discretized from the world plane.
    /// </summary>
    [System.Serializable]
    public class MacroHumanSim
    {
        [SerializeField] int[] population;
        [SerializeField] int xSize;
        [SerializeField] int ySize;
        [SerializeField] int maxPop = -1;

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

            if (humanEvacCells[x + y * xSize] != null)
            {
                HumanEvacCell hR = humanEvacCells[x + y * xSize];
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

        /// <summary>
        /// Advances the macro human simulation.
        /// Loops through all cells and all households per cell and check if they have reached their goal (car)
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="currentTime"></param>
        public void AdvanceEvacuatePopulation(float deltaTime, float currentTime)
        {            
            if (!evacuationDone)
            {
                int peopleWhoReachedCar = 0;
                int peopleStartedMoving = 0;
                int householdsLeft = 0, householdsStartedMoving = 0, householdsReachedCar = 0;
                float walkingDistance = 0f;
                int householdsDone = 0;

                for (int i = 0; i < humanEvacCells.Length; ++i)
                {
                    if (population[i] > 0)
                    {
                        HumanEvacCell cell = humanEvacCells[i];
                        for (int j = 0; j < cell.macroHouseholds.Length; ++j)
                        {
                            MacroHousehold household = cell.macroHouseholds[j];
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

        public int GetPeopleLeft()
        {
            return peopleLeft;
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
            if(WUInity.WUINITY_SIM.runTrafficSim)
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
                        WUInity.WUINITY_SIM.GetMacroTrafficSim().InsertNewCar(cell.rasterRoute.GetSelectedRoute(), cars[i]);
                    }
                }
                else
                {
                    WUInity.WUINITY_SIM.GetMacroTrafficSim().InsertNewCar(cell.rasterRoute.GetSelectedRoute(), household.peopleInHousehold);
                }
            }
            
            household.reachedCar = true;
            //update counter
            peopleWhoReachedCar += household.peopleInHousehold;
        }

        public void SaveToFile(int runNumber)
        {
            WUInityInput wO = WUInity.WUINITY_IN;
            System.IO.File.WriteAllLines(Application.dataPath + "/Resources/_output/" + wO.simName + "_pedestrian_output_" + runNumber + ".csv", output);
        }

        //alternate way of saving, neeed to print each loop iteration, saves memory but is way slower. Update: useless? uses more memory?
        /*public void SaveToFile(string output, bool newFile)
        {
            if (newFile)
            {
                System.IO.File.Delete(@"E:\unity_projects\WUI-NITY\wui-nity_git\Assets\Resources\pedestrian_output.csv");
            }
            System.IO.File.AppendAllText(@"E:\unity_projects\WUI-NITY\wui-nity_git\Assets\Resources\pedestrian_output.csv", output + "\n");
        }*/

        /// <summary>
        /// Populates cells based on loaded GPW data, returns total evacuees placed.
        /// </summary>
        public int PopulateCells(Vector2Int cellCount, Vector2D realWorldSize, GPWData gpwData)
        {
            xSize = cellCount.x;
            ySize = cellCount.y;
            this.realWorldSize = realWorldSize;
            population = new int[cellCount.x * cellCount.y];
            WUInity.WUINITY_OUT.evac.rawPopulation = new int[cellCount.x * cellCount.y];

            double cellSizeX = realWorldSize.x / xSize;
            double cellSizeY = realWorldSize.y / ySize;
            cellWorldSize = new Vector2D(cellSizeX, cellSizeY);
            double cellArea = cellSizeX * cellSizeY / (1000000d); // people/square km
            totalPopulation = 0;
            for (int y = 0; y < ySize; ++y)
            {
                double yPos = ((double)y + 0.5) * cellSizeY;
                for (int x = 0; x < xSize; ++x)
                {
                    double xPos = ((double)x + 0.5) * cellSizeX;
                    double density = gpwData.GetDensityUnitySpaceBilinear(new Vector2D(xPos, yPos));
                    int pop = System.Convert.ToInt32(cellArea * density);
                    pop = Mathf.Clamp(pop, 0, pop);
                    population[x + y * xSize] = pop;
                    totalPopulation += pop;
                    if (pop > maxPop)
                    {
                        maxPop = pop;
                    }

                    WUInity.WUINITY_OUT.evac.rawPopulation[x + y * xSize] = pop;
                }
            }
            return totalPopulation;
        }


        /// <summary>
        /// Receives an array filled with pre-calculated routes available to the cell
        /// Should (only?) be called by the RouteCreator
        /// </summary>
        /// <param name="rasterRoutes"></param>
        public void SetRasterRoutes(global::WUInity.RouteCollection[] rasterRoutes)
        {
            this.cellRoutes = rasterRoutes;
        }

        /// <summary>
        /// Goues through all cells and check if they have a valid route to get away or not.
        /// If not they are summed up for later re-distribution
        /// </summary>
        /// <returns></returns>
        public int CollectStuckPeople()
        {
            int stuckPeople = 0;
            for (int i = 0; i < population.Length; ++i)
            {
                if (population[i] > 0 && cellRoutes[i] == null)
                {
                    //MonoBehaviour.print(population[i] + " persons are stuck. Index: " + i);
                    stuckPeople += population[i];
                    //delete people in the current cell since we are relocating them
                    population[i] = 0;
                }
            }
            //MonoBehaviour.print("Total stuck people: " + stuckPeople);
            return stuckPeople;
        }

        /// <summary>
        /// Relocates stuck people (no route in cell), relocation based on ratio between people in cell / total people, so relative density is conserved
        /// </summary>
        /// <param name="stuckPeople"></param>
        public void RelocateStuckPeople(int stuckPeople)
        {
            if (stuckPeople > 0)
            {
                int remainingPop = totalPopulation - stuckPeople;
                int addedPop = 0;
                for (int i = 0; i < population.Length; ++i)
                {
                    if (population[i] > 0)
                    {
                        float weight = population[i] / (float)remainingPop;
                        int extraPersonsToCell = Mathf.CeilToInt(weight * stuckPeople);
                        population[i] += extraPersonsToCell;
                        addedPop += extraPersonsToCell;
                    }
                }
                totalPopulation = remainingPop + addedPop;
                peopleLeft = totalPopulation;
            }

            WUInity.WUINITY_OUT.evac.actualTotalEvacuees = totalPopulation;
        }

        /// <summary>
        /// Scales the actual poulation count from GPW down to desired amount of people
        /// </summary>
        /// <param name="wantedTotal"></param>
        /// <returns></returns>
        public int ScaleTotalPopulation(int wantedTotal)
        {
            int newTotalPop = 0;
            for (int i = 0; i < population.Length; ++i)
            {
                if (population[i] > 0)
                {
                    float weight = population[i] / (float)totalPopulation;
                    int newPop = Mathf.RoundToInt(weight * wantedTotal);
                    population[i] = newPop;
                    newTotalPop += newPop;
                }
            }
            totalPopulation = newTotalPop;
            peopleLeft = totalPopulation;
            return totalPopulation;
        }

        /// <summary>
        /// Pulls random response time along a response time curve
        /// </summary>
        /// <returns></returns>
        static public float GetRandomResponseTime()
        {
            EvacInput eO = WUInity.WUINITY_IN.evac;

            float responseTime = float.MaxValue;
            float r = Random.Range(0f, 1f);

            //get curve index from evac group
            int curveIndex = 0;

            for (int i = 0; i < eO.responseCurves[curveIndex].dataPoints.Length; i++)
            {
                if (r <= eO.responseCurves[curveIndex].dataPoints[i].probability)
                {
                    responseTime = Random.Range(eO.responseCurves[curveIndex].dataPoints[i].timeMinMax.x, eO.responseCurves[curveIndex].dataPoints[i].timeMinMax.y);
                    break;
                }
            }

            /*if(r <= 0.14f)
            {
                responseTime = Random.Range(-420f, 0f);
            }
            else if(r <= 0.81f)
            {
                responseTime = Random.Range(0f, 1200f);
            }
            else if (r <= 0.95f)
            {
                responseTime = Random.Range(1200f, 3600f);
            }
            else
            {
                responseTime = float.MaxValue;
            }*/

            return responseTime;
        }

        /// <summary>
        /// Gets random walking speed based on user input range
        /// </summary>
        /// <returns></returns>
        static public float GetRandomWalkingSpeed()
        {
            EvacInput eO = WUInity.WUINITY_IN.evac;
            return Random.Range(eO.walkingSpeedMinMax.x, eO.walkingSpeedMinMax.y) * eO.walkingSpeedModifier;
        }

        /// <summary>
        /// Creates the human evac cell if cell contains population and deploys households in all of the cells based on total amount of people per cell
        /// </summary>
        public void PlaceHouseholdsInCells()
        {
            int totalHouseholds = 0;
            humanEvacCells = new HumanEvacCell[population.Length];
            for (int i = 0; i < humanEvacCells.Length; ++i)
            {
                if (population[i] > 0)
                {
                    int y = i / xSize;
                    int x = i - y * xSize;
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
                        MacroHousehold rH = hR.macroHouseholds[j];
                        if (rH.responseTime == float.MaxValue)
                        {
                            totalPeopleWhoWillNotEvacuate += rH.peopleInHousehold;
                        }
                        else
                        {
                            totalCars += rH.cars;
                        }                        
                    }
                }
            }

            WUInity.WUINITY_SIM.LogMessage("Total households: " + totalHouseholds);
            WUInity.WUINITY_SIM.LogMessage("Total cars: " +  totalCars);
            WUInity.WUINITY_SIM.LogMessage("Total people who will not evacuate: " + totalPeopleWhoWillNotEvacuate);
            WUInity.WUINITY_OUT.evac.stayingPeople = totalPeopleWhoWillNotEvacuate;
        }

        /// <summary>
        /// Gets number of poeple in a cell. Clamps x and y to be within array.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int GetPopulation(int x, int y)
        {
            if (x < 0 || x > xSize - 1 || y < 0 || y > ySize - 1)
            {
                //Debug.Log("Population density was polled outside of data coverage.");
                return 0;
            }
            //x = Mathf.Clamp(x, 0, xSize - 1);
            //y = Mathf.Clamp(y, 0, ySize - 1);
            return population[x + y * xSize];
        }

        /// <summary>
        /// Get number of people in cell base don "world space" coordinates. Clamps to dimensions of defined area.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int GetPopulationUnitySpace(double x, double y)
        {
            int xInt = (int)((x / realWorldSize.x) * xSize);
            int yInt = (int)((y / realWorldSize.y) * ySize);
            return GetPopulation(xInt, yInt);
        }

        /// <summary>
        /// Returns cell area in km^2
        /// </summary>
        /// <returns></returns>
        private float GetCellArea()
        {
            return (float)(cellWorldSize.x * cellWorldSize.y / 1000000d); //people /square km
        }

        /// <summary>
        /// Creates texture which shows cells that had people who were stuck and had to be relocated
        /// </summary>
        /// <returns></returns>
        public Texture2D CreateStuckPopulationTexture()
        {
            //first find the correct texture size
            int maxSide = Mathf.Max(xSize, ySize);
            Vector2Int res = new Vector2Int(2, 2);

            while (xSize > res.x)
            {
                res.x *= 2;
            }
            while (ySize > res.y)
            {
                res.y *= 2;
            }

            Texture2D popTex = new Texture2D(res.x, res.y);
            popTex.filterMode = FilterMode.Point;
            for (int y = 0; y < ySize; y++)
            {
                for (int x = 0; x < xSize; x++)
                {
                    Color color = Color.grey;
                    if (population[x + y * xSize] == 0)
                    {
                        color = Color.blue;
                    }
                    else if (cellRoutes[x + y * xSize] == null)
                    {
                        color = Color.red;
                    }
                    color.a = 0.4f;
                    popTex.SetPixel(x, y, color);
                }
            }
            popTex.Apply();
            return popTex;
        }

        /// <summary>
        /// Creates texture that shown cells where people have decided to stay forever.
        /// </summary>
        /// <returns></returns>
        public Texture2D CreateStayingPopulationTexture()
        {
            Vector2Int res = new Vector2Int(2, 2);

            while (xSize > res.x)
            {
                res.x *= 2;
            }
            while (ySize > res.y)
            {
                res.y *= 2;
            }

            Texture2D popTex = new Texture2D(res.x, res.y);
            popTex.filterMode = FilterMode.Point;

            WUInity.WUINITY_OUT.evac.stayingPopulation = new int[xSize * ySize];

            for (int y = 0; y < ySize; y++)
            {
                for (int x = 0; x < xSize; x++)
                {
                    int peopleStaying = 0;
                    if (humanEvacCells[x + y * xSize] != null)
                    {
                        MacroHousehold[] rH = humanEvacCells[x + y * xSize].macroHouseholds;
                        for (int i = 0; i < rH.Length; i++)
                        {
                            if (rH[i].evacuationTime == float.MaxValue)
                            {
                                peopleStaying += rH[i].peopleInHousehold;
                            }
                        }
                    }
                    Color color = GetStayColor(peopleStaying, population[x + y * xSize]);
                    popTex.SetPixel(x, y, color);
                    WUInity.WUINITY_OUT.evac.stayingPopulation[x + y * xSize] = peopleStaying;
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
            /*if (peopleStaying == 0)
            {
                color = new Color(190f / 255f, 232f / 255f, 255f / 255f);
            }
            else if (peopleStaying <= 5)
            {
                color = new Color(1.0f, 241f / 255f, 208f / 255f);
            }
            else if (peopleStaying <= 10)
            {
                color = new Color(1.0f, 218f / 255f, 165f / 255f);
            }
            else if (peopleStaying <= 30.0f)
            {
                color = new Color(252f / 255f, 183f / 255f, 82f / 255f);
            }
            else if (peopleStaying <= 40)
            {
                color = new Color(1.0f, 137f / 255f, 63f / 255f);
            }
            else if (peopleStaying <= 50)
            {
                color = new Color(238f / 255f, 60f / 255f, 30f / 255f);
            }
            else
            {
                color = new Color(191f / 255f, 1f / 255f, 39f / 255f);
            }*/
            return color;
        }

        /// <summary>
        /// Returns texture with colors based on people density on the same scale as GPW.
        /// </summary>
        /// <returns></returns>
        public Texture2D CreatePopulationTexture()
        {
            //first find the correct texture size
            int maxSide = Mathf.Max(xSize, ySize);
            Vector2Int res = new Vector2Int(2, 2);

            while (xSize > res.x)
            {
                res.x *= 2;
            }
            while (ySize > res.y)
            {
                res.y *= 2;
            }
            Texture2D popTex = new Texture2D(res.x, res.y);
            popTex.filterMode = FilterMode.Point;

            float cellArea = GetCellArea();

            for (int y = 0; y < ySize; y++)
            {
                for (int x = 0; x < xSize; x++)
                {
                    float pop = GetPopulation(x, y);
                    Color color = GPWViewer.GetGPWColor(pop / cellArea);
                    popTex.SetPixel(x, y, color);
                }
            }
            popTex.Apply();
            return popTex;
        }
    }
}