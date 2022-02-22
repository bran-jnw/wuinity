using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace WUInity.GPW
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
        public bool isLoaded;
        public Texture2D populationTexture;

        public PopulationData()
        {
            lowerLeftLatLong = WUInity.INPUT.lowerLeftLatLong;
            size = WUInity.INPUT.size;
            cells = WUInity.SIM.EvacCellCount;
            cellSize = WUInity.INPUT.evac.routeCellSize;

            cellArea = cellSize * cellSize / (1000000d); // people/square km
            totalPopulation = 0;

            cellPopulation = new int[cells.x * cells.y];
            populationMask = new bool[cells.x * cells.y];

            isLoaded = false;
        }

        public void CreatePopulationFromLocalGPW(LocalGPWData localGPW)
        {
            lowerLeftLatLong = WUInity.INPUT.lowerLeftLatLong;
            size = WUInity.INPUT.size;
            cells = WUInity.SIM.EvacCellCount;
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
                    int pop = System.Convert.ToInt32(cellArea * density);
                    pop = Mathf.Clamp(pop, 0, pop);
                    cellPopulation[x + y * cells.x] = pop;
                    totalPopulation += pop;

                    //WUInity.OUTPUT.evac.rawPopulation[x + y * cells.x] = pop;
                }
            }

            CreateTexture();
            SavePopulation();

            isLoaded = true;
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
            isLoaded = true;
        }

        public int GetPeopleCount(int x, int y)
        {
            return cellPopulation[x + y * cells.x];
        }

        public Texture2D GetPopulationTexture()
        {
            return populationTexture;
        }

        private void SavePopulation()
        {
            string[] data = new string[9];

            data[0] = lowerLeftLatLong.x.ToString();
            data[1] = lowerLeftLatLong.y.ToString();
            data[2] = size.x.ToString();
            data[3] = size.y.ToString();
            data[4] = cells.x.ToString();
            data[5] = cells.y.ToString();
            data[6] = cellSize.ToString();
            data[7] = totalPopulation.ToString();
            data[8] = "";
            for (int i = 0; i < cellPopulation.Length; i++)
            {
                data[8] += cellPopulation[i] + " ";
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
                WUInity.SIM.LogMessage("LOG: Could not load population, file not found.");

            }

            return success;
        }

        private bool LoadPopulation(string path)
        {
            string[] d = File.ReadAllLines(path);

            bool success = true; // IsDataValid(d[0]);
            if (success && d.Length > 8)
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
                cellPopulation = new int[cells.x * cells.y];
                populationMask = new bool[cells.x * cells.y];
                string[] dummy = d[8].Split(' ');
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
                WUInity.SIM.LogMessage("LOG: Loaded population from file.");
            }
            else
            {
                WUInity.SIM.LogMessage("ERROR: Population data not valid for current map.");
            }

            return success;
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

