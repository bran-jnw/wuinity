using System.Collections.Generic;
using System.IO;
using WUIPlatform.IO;

namespace WUIPlatform.Population
{
    public class PopulationData
    {
        public Vector2d lowerLeftLatLong;
        public Vector2d size;
        public Vector2int cells;
        public float cellSize;
        public int totalPopulation;
        public int totalActiveCells;
        public int[] cellPopulation;        
        public  double cellArea;
        public bool[] populationMask;

        //not saved
        public bool isLoaded, correctedForRoutes;
        PopulationManager manager;

        public PopulationData(PopulationManager manager)
        {
            lowerLeftLatLong = WUIEngine.INPUT.Simulation.LowerLeftLatLong;
            size = WUIEngine.INPUT.Simulation.Size;
            cells = WUIEngine.RUNTIME_DATA.Evacuation.CellCount;
            cellSize = WUIEngine.INPUT.Evacuation.RouteCellSize;

            cellArea = cellSize * cellSize / (1000000d); // people/square km
            totalPopulation = 0;

            cellPopulation = new int[cells.x * cells.y];
            populationMask = new bool[cells.x * cells.y];

            isLoaded = false;
            correctedForRoutes = false;
            this.manager = manager;
        }

        public int GetPeopleCount(int x, int y)
        {
            return cellPopulation[x + y * cells.x];
        }

        

        public void CreatePopulationFromLocalGPW(LocalGPWData localGPW)
        {
            lowerLeftLatLong = WUIEngine.INPUT.Simulation.LowerLeftLatLong;
            size = WUIEngine.INPUT.Simulation.Size;
            cells = WUIEngine.RUNTIME_DATA.Evacuation.CellCount;
            cellSize = WUIEngine.INPUT.Evacuation.RouteCellSize;

            cellPopulation = new int[cells.x * cells.y];
            populationMask = new bool[cells.x * cells.y];

            cellArea = cellSize * cellSize / (1000000d); // people/square km
            totalPopulation = 0;
            totalActiveCells = 0;
            for (int y = 0; y < cells.y; ++y)
            {
                double yPos = (y + 0.5) * cellSize;
                for (int x = 0; x < cells.x; ++x)
                {
                    double xPos = (x + 0.5) * cellSize;
                    double density = localGPW.GetDensitySimulationSpaceBilinear(new Vector2d(xPos, yPos));
                    int pop = 0;
                    //if data has negative values (NO_DATA) it should be zero
                    //else force at least one person if there is some density
                    if (density > 0.0)
                    {                        
                        pop = Mathf.Max(1, Mathf.RoundToInt((float)(cellArea * density)));
                    }
                    cellPopulation[x + y * cells.x] = pop;
                    totalPopulation += pop;
                    if(pop > 0)
                    {
                        ++totalActiveCells;
                    }

                    //WUIEngine.OUTPUT.evac.rawPopulation[x + y * cells.x] = pop;
                }
            }

            manager.CreateTexture();
            SavePopulation();

            isLoaded = true;
            correctedForRoutes = false;
        }

        /// <summary>
        /// Creates uniform population based on a pre-made population mask (made in painter).
        /// </summary>
        /// <param name="newTotalPopulation"></param>
        public void PlaceUniformPopulation(int newTotalPopulation)
        {
            totalActiveCells = 0;
            for (int i = 0; i < populationMask.Length; i++)
            {
                if(populationMask[i] == true)
                {
                    ++totalActiveCells;
                }
            }

            int peoplePerCell = Mathf.Max(1, Mathf.RoundToInt((float)newTotalPopulation / totalActiveCells));
            totalPopulation = 0;
            for (int i = 0; i < populationMask.Length; i++)
            {
                cellPopulation[i] = 0;
                if (populationMask[i] == true)
                {                    
                    cellPopulation[i] = peoplePerCell;
                    totalPopulation += peoplePerCell;
                }
            }

            if(newTotalPopulation != totalPopulation)
            {
                ScaleTotalPopulation(newTotalPopulation);
            }

            manager.CreateTexture();
            SavePopulation();
            isLoaded = true;
            correctedForRoutes = false;
        }

        public void UpdatePopulationBasedOnRoutes(RouteCollection[] cellRoutes)
        {
            int stuckPeople = CollectStuckPeople(cellRoutes);
            if (stuckPeople > 0)
            {
                RelocateStuckPeople(stuckPeople);
                manager.CreateTexture();                
            }            

            correctedForRoutes = true;
            SavePopulation();
        }

        /// <summary>
        /// Scales the actual poulation count from GPW down to desired amount of people
        /// </summary>
        /// <param name="desiredPopulation"></param>
        /// <returns></returns>
        public void ScaleTotalPopulation(int desiredPopulation)
        {
            int newTotalPop = 0;
            List<int> activeCellIndices = new List<int>();
            for (int i = 0; i < cellPopulation.Length; ++i)
            {
                if (cellPopulation[i] > 0)
                {
                    float weight = cellPopulation[i] / (float)totalPopulation;
                    int newPop = Mathf.Max(1, (int)(weight * desiredPopulation));
                    cellPopulation[i] = newPop;
                    newTotalPop += newPop;

                    //save all of the indices for later random distribution
                    activeCellIndices.Add(i);
                }
            }
            totalPopulation = newTotalPop;

            //make sure we hit our target, should always be lower if not matching since we are flooring the int
            if(desiredPopulation != totalPopulation)
            {
                int loopCount = desiredPopulation - totalPopulation;
                for (int i = 0; i < loopCount; i++)
                {
                    int randomIndex = Random.Range(0, activeCellIndices.Count);
                    ++cellPopulation[activeCellIndices[randomIndex]];
                    ++totalPopulation;
                }
            }

            manager.CreateTexture();
            SavePopulation();
        }

        private void SavePopulation()
        {
            string[] data = new string[10];

            data[0] = lowerLeftLatLong.x.ToString();
            data[1] = lowerLeftLatLong.y.ToString();
            data[2] = size.x.ToString();
            data[3] = size.y.ToString();
            data[4] = cells.x.ToString();
            data[5] = cells.y.ToString();
            data[6] = cellSize.ToString();
            data[7] = totalPopulation.ToString();
            data[8] = (correctedForRoutes ? 1 : 0).ToString();
            data[9] = "";
            for (int i = 0; i < cellPopulation.Length; i++)
            {
                data[9] += cellPopulation[i] + " ";
            }

            string path = WUIEngine.INPUT.Population.populationFile;
            if(!File.Exists(path))
            {
                path = Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Simulation.SimulationID + ".pop");
            }
            WUIEngine.INPUT.Population.populationFile = Path.GetFileName(path);
            File.WriteAllLines(path, data);
        }

        public bool LoadPopulationFromFile(string file, bool updateInput)
        {
            bool success = false;
            if (File.Exists(file))
            {
                success = LoadPopulation(file, updateInput);
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Log, " Could not load population, file not found.");

            }

            return success;
        }

        private bool LoadPopulation(string path, bool updateInput)
        {
            string[] d = File.ReadAllLines(path);

            bool success = true; // IsDataValid(d[0]);
            if (success && d.Length > 9)
            {
                totalActiveCells = 0;
                double.TryParse(d[0], out lowerLeftLatLong.x);
                double.TryParse(d[1], out lowerLeftLatLong.y);
                double.TryParse(d[2], out size.x);
                double.TryParse(d[3], out size.y);
                int temp;
                int.TryParse(d[4], out temp);
                cells.x = temp;
                int.TryParse(d[5], out temp);
                cells.y = temp;
                float.TryParse(d[6], out cellSize);
                cellArea = cellSize * cellSize / 1000000d; // people/square km
                int.TryParse(d[7], out totalPopulation);
                int.TryParse(d[8], out temp);
                correctedForRoutes = temp == 1 ? true : false;
                cellPopulation = new int[cells.x * cells.y];
                populationMask = new bool[cells.x * cells.y];                
                string[] dummy = d[9].Split(' ');
                for (int i = 0; i < cellPopulation.Length; ++i)
                {
                    int.TryParse(dummy[i], out cellPopulation[i]);
                    if(cellPopulation[i] > 0)
                    {
                        populationMask[i] = true;
                        ++totalActiveCells;
                    }
                }                
            }
            else
            {
                success = false;                
            }

            if(success)
            {
                if(updateInput)
                {
                    WUIEngineInput.SaveInput();
                }
                manager.CreateTexture();
                isLoaded = true;
                WUIEngine.INPUT.Population.populationFile = Path.GetFileName(path);                
                WUIEngine.LOG(WUIEngine.LogType.Log, " Loaded population from file " + path + ".");
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, " Population data not valid for current map.");
            }

            return success;
        }        

        /// <summary>
        /// Goes through all cells and check if they have a valid route to get away or not.
        /// If not they are summed up for later re-distribution
        /// </summary>
        /// <returns></returns>
        private int CollectStuckPeople(RouteCollection[] cellRoutes)
        {
            if(cellRoutes.Length != cellPopulation.Length)
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, " Route collection and population does not have same size.");
                return -1;
            }

            int stuckPeople = 0;
            for (int i = 0; i < cellPopulation.Length; ++i)
            {
                if (cellPopulation[i] > 0 && cellRoutes[i] == null)
                {
                    //MonoBehaviour.print(population[i] + " persons are stuck. Index: " + i);
                    stuckPeople += cellPopulation[i];
                    //delete people in the current cell since we are relocating them
                    cellPopulation[i] = 0;
                }
            }
            return stuckPeople;
        }

        /// <summary>
        /// Relocates stuck people (no route in cell), relocation based on ratio between people in cell / total people, so relative density is conserved
        /// </summary>
        /// <param name="stuckPeople"></param>
        private void RelocateStuckPeople(int stuckPeople)
        {
            if (stuckPeople > 0)
            {
                int oldTotalPopulation = totalPopulation;
                int remainingPop = totalPopulation - stuckPeople;
                totalPopulation = 0;
                totalActiveCells = 0;
                for (int i = 0; i < cellPopulation.Length; ++i)
                {
                    if (cellPopulation[i] > 0)
                    {
                        float weight = cellPopulation[i] / (float)remainingPop;
                        int extraPersonsToCell = Mathf.Max(1, Mathf.RoundToInt(weight * stuckPeople));
                        cellPopulation[i] += extraPersonsToCell;
                        totalPopulation += cellPopulation[i];
                        ++totalActiveCells;
                    }
                }

                if (totalPopulation != oldTotalPopulation)
                {
                    ScaleTotalPopulation(oldTotalPopulation);
                }
            }            
        }
    }
}

