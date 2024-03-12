using UnityEngine;
using WUInity.Evac;

namespace WUInity.Visualization
{
    public class MacroHumanSimVisualizerUnity : MacroHumanSimVisualizer
    {
        /// <summary>
        /// Creates texture that shown cells where people have decided to stay forever.
        /// </summary>
        /// <returns></returns>
        public override object CreateStayingPopulationTexture(MacroHumanSim sim)
        {
            Vector2Int res = new Vector2Int(2, 2);
            int cellsX = sim.GetCellsX();
            int cellsY = sim.GetCellsY();
            HumanEvacCell[] humanEvacCells = sim.GetHumanEvacCells();
            int[] population = sim.GetPopulation();

            while (cellsX > res.x)
            {
                res.x *= 2;
            }
            while (cellsX > res.y)
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
                    UnityEngine.Color color = GetStayColor(peopleStaying, population[x + y * cellsX]);
                    popTex.SetPixel(x, y, color);
                }
            }
            popTex.Apply();
            return popTex;
        }

        private static Color GetStayColor(int peopleStaying, int actualPopInCell)
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
