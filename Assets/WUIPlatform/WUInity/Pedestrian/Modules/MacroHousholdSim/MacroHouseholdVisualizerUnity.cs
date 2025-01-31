//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using UnityEngine;
using WUIPlatform.Pedestrian;

namespace WUIPlatform.Visualization
{
    public class MacroHouseholdVisualizerUnity : MacroHouseholdVisualizer
    {
        /// <summary>
        /// Creates texture that shown cells where people have decided to stay forever.
        /// </summary>
        /// <returns></returns>
        /*public override object CreateStayingPopulationTexture(MacroHouseholdSim sim)
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
        }*/

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
