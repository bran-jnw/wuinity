using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        Texture2D popTexture;
        double cellArea;
        public bool[] populationMask;

        public PopulationData(GPWData rawData)
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
                    double density = rawData.GetDensityUnitySpaceBilinear(new Vector2D(xPos, yPos));
                    int pop = System.Convert.ToInt32(cellArea * density);
                    pop = Mathf.Clamp(pop, 0, pop);
                    cellPopulation[x + y * cells.x] = pop;
                    totalPopulation += pop;

                    //WUInity.OUTPUT.evac.rawPopulation[x + y * cells.x] = pop;
                }
            }

            CreateTexture();
        }

        public PopulationData(int totalPopulation)
        {
            lowerLeftLatLong = WUInity.INPUT.lowerLeftLatLong;
            size = WUInity.INPUT.size;
            cells = WUInity.SIM.EvacCellCount;
            cellSize = WUInity.INPUT.evac.routeCellSize;

            cellPopulation = new int[cells.x * cells.y];
            populationMask = new bool[cells.x* cells.y];

            cellArea = cellSize * cellSize / (1000000d); // people/square km
            this.totalPopulation = totalPopulation;

            CreateTexture();
        }

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
                if (populationMask[i] == true)
                {                    
                    cellPopulation[i] = peoplePerCell;
                    totalPopulation += peoplePerCell;
                }
            }
        }

        public int GetPeopleCount(int x, int y)
        {
            return cellPopulation[x + y * cells.x];
        }

        public Texture2D GetPopulationTexture()
        {
            return popTexture;
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
            //Debug.Log("GPW texture resolution: " + res.x + ", " + res.y);

            //paint texture based time of arrival
            popTexture = new Texture2D(res.x, res.y);
            popTexture.filterMode = FilterMode.Point;
            for (int y = 0; y < cells.y; y++)
            {
                for (int x = 0; x < cells.x; x++)
                {
                    double density = GetPeopleCount(x, y);
                    Color color = PopulationViewer.GetGPWColor((float)density);

                    popTexture.SetPixel(x, y, color);
                }
            }
            popTexture.Apply();
        }
    }
}

