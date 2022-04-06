using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace WUInity.Population
{
    public class PopulationData
    {
        public Vector2D lowerLeftLatLong;
        public Vector2D size;
        public Vector2Int cells;
        public float cellSize;
        public int totalPopulation;
        public int[] cellPopulation;        
        double cellArea;
        public bool[] populationMask;

        //not saved
        public bool isLoaded, correctedForRoutes;
        public Texture2D populationTexture;

        public PopulationData()
        {
            lowerLeftLatLong = WUInity.INPUT.lowerLeftLatLong;
            size = WUInity.INPUT.size;
            cells = WUInity.SIM_DATA.EvacCellCount;
            cellSize = WUInity.INPUT.evac.routeCellSize;

            cellArea = cellSize * cellSize / (1000000d); // people/square km
            totalPopulation = 0;

            cellPopulation = new int[cells.x * cells.y];
            populationMask = new bool[cells.x * cells.y];

            isLoaded = false;
            correctedForRoutes = false;
        }

        public int GetPeopleCount(int x, int y)
        {
            return cellPopulation[x + y * cells.x];
        }

        public Texture2D GetPopulationTexture()
        {
            return populationTexture;
        }

        public void CreatePopulationFromLocalGPW(LocalGPWData localGPW)
        {
            lowerLeftLatLong = WUInity.INPUT.lowerLeftLatLong;
            size = WUInity.INPUT.size;
            cells = WUInity.SIM_DATA.EvacCellCount;
            cellSize = WUInity.INPUT.evac.routeCellSize;

            cellPopulation = new int[cells.x * cells.y];
            populationMask = new bool[cells.x * cells.y];

            cellArea = cellSize * cellSize / (1000000d); // people/square km
            totalPopulation = 0;
            for (int y = 0; y < cells.y; ++y)
            {
                double yPos = (y + 0.5) * cellSize;
                for (int x = 0; x < cells.x; ++x)
                {
                    double xPos = (x + 0.5) * cellSize;
                    double density = localGPW.GetDensityUnitySpaceBilinear(new Vector2D(xPos, yPos));
                    int pop = Mathf.CeilToInt((float)(cellArea * density));
                    pop = Mathf.Max(0, pop);
                    cellPopulation[x + y * cells.x] = pop;
                    totalPopulation += pop;

                    //WUInity.OUTPUT.evac.rawPopulation[x + y * cells.x] = pop;
                }
            }

            CreateTexture();
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
            int activeCells = 0;
            for (int i = 0; i < populationMask.Length; i++)
            {
                if(populationMask[i] == true)
                {
                    ++activeCells;
                }
            }

            int peoplePerCell = Mathf.CeilToInt((float)newTotalPopulation / activeCells);
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

            CreateTexture();
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
                CreateTexture();
                SavePopulation();
            }

            correctedForRoutes = true;
        }

        /// <summary>
        /// Scales the actual poulation count from GPW down to desired amount of people
        /// </summary>
        /// <param name="desiredPopulation"></param>
        /// <returns></returns>
        public void ScaleTotalPopulation(int desiredPopulation)
        {
            int newTotalPop = 0;
            for (int i = 0; i < cellPopulation.Length; ++i)
            {
                if (cellPopulation[i] > 0)
                {
                    float weight = cellPopulation[i] / (float)totalPopulation;
                    int newPop = Mathf.CeilToInt(weight * desiredPopulation);
                    cellPopulation[i] = newPop;
                    newTotalPop += newPop;
                }
            }
            totalPopulation = newTotalPop;

            CreateTexture();
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

            string path = WUInity.INPUT.population.populationFile;
            if(!File.Exists(path))
            {
                path = Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.simName + ".pop");
            }
            WUInity.INPUT.population.populationFile = path;
            System.IO.File.WriteAllLines(path, data);
        }

        public bool LoadPopulationFromFile()
        {
            //first try if local filtered file exists
            string path = WUInity.INPUT.population.populationFile;
            bool success = false;
            if (File.Exists(path))
            {
                success = LoadPopulation(path);
            }
            else
            {
                WUInity.WUI_LOG("LOG: Could not load population, file not found.");

            }

            return success;
        }

        private bool LoadPopulation(string path)
        {
            string[] d = File.ReadAllLines(path);

            bool success = true; // IsDataValid(d[0]);
            if (success && d.Length > 9)
            {
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
                    }
                }                
            }
            else
            {
                success = false;                
            }

            if(success)
            {
                CreateTexture();
                isLoaded = true;                
                WUInity.WUI_LOG("LOG: Loaded population from file.");
            }
            else
            {
                WUInity.WUI_LOG("ERROR: Population data not valid for current map.");
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
                WUInity.WUI_LOG("ERROR: Route collection and population does not have same size.");
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
                int remainingPop = totalPopulation - stuckPeople;
                totalPopulation = 0;
                for (int i = 0; i < cellPopulation.Length; ++i)
                {
                    if (cellPopulation[i] > 0)
                    {
                        float weight = cellPopulation[i] / (float)remainingPop;
                        int extraPersonsToCell = Mathf.CeilToInt(weight * stuckPeople);
                        cellPopulation[i] += extraPersonsToCell;
                        totalPopulation += cellPopulation[i];
                    }
                }                
            }
        }

        private void CreateTexture()
        {
            //first find the correct texture size
            int maxSide = Mathf.Max(cells.x, cells.y);
            Vector2Int res = new Vector2Int(2, 2);

            while (cells.x > res.x)
            {
                res.x *= 2;
            }
            while (cells.y > res.y)
            {
                res.y *= 2;
            }

            populationTexture = new Texture2D(res.x, res.y);
            populationTexture.filterMode = FilterMode.Point;
            for (int y = 0; y < cells.y; y++)
            {
                for (int x = 0; x < cells.x; x++)
                {
                    double density = GetPeopleCount(x, y) / cellArea;
                    Color color = PopulationManager.GetGPWColor((float)density);

                    populationTexture.SetPixel(x, y, color);
                }
            }
            populationTexture.Apply();
        }
    }
}

