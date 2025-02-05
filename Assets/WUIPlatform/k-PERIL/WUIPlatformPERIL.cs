//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.IO;
using System.Collections.Generic;

namespace WUIPlatform
{
    public static class WUIPlatformPERIL
    {
        private static kPERIL_DLL.kPERIL PERIL;

                
        /// <summary>
        /// Runs k-PERIL, should be called after a completed simulation
        /// </summary>
        /// <param name="midFlameWindspeed">User have to pick a representative mid flame wind speed as k-PERIL does not take changing weather into account</param>
        public static float[,] RunPERIL(float midFlameWindspeed)
        {
            if (PERIL == null)
            {
                PERIL = new kPERIL_DLL.kPERIL();
            }

            int xDim = WUIEngine.SIM.FireModule.GetCellCountX();
            int yDim = WUIEngine.SIM.FireModule.GetCellCountY();
            //assume cell/raster is square
            int cellSize = Mathf.RoundToInt(WUIEngine.SIM.FireModule.GetCellSizeX());
            //get wuiarea from a user defined map painted in wuinity
            int[,] WUIarea = GetWUIArea();

            //create multiple time buffers
            float RSET = WUIEngine.OUTPUT.TotalAverageEvacTime / 60f;
            float[] RSETs = new float[10];
            for (int i = 0; i < RSETs.Length; ++i)
            {
                RSETs[i] = RSET + i * 60 * 12f;
            }

            //collect ROS, how to send ROS data is ROStheta[X*Y,8], y first, x second, start in north and then clockwise
            float[,] maxROS = WUIEngine.SIM.FireModule.GetMaxROS();
            float[,] rosAzimuth = WUIEngine.SIM.FireModule.GetMaxROSAzimuth();
            int[,] wuiArea = GetWUIArea();

            List<int[,]> perilOutput = new List<int[,]>(RSETs.Length);
            for (int i = 0; i < RSETs.Length; ++i)
            {
                using (StringWriter output = new StringWriter())
                {
                    int[,] result = PERIL.CalculateBoundary(cellSize, RSETs[i], midFlameWindspeed, wuiArea, maxROS, rosAzimuth, output);
                    perilOutput.Add(result);
                    WUIEngine.LOG(WUIEngine.LogType.Log, "k-PERIL output, RSET= " + RSETs[i] + " minutes.\n" + output.ToString());
                }
            }

            //k-PERIL returns flipped x/y, so just copy structure and traverse, we fix the flipping in visualization
            float[,] combinedPerilOutput = new float[perilOutput[0].GetLength(0), perilOutput[0].GetLength(1)];
            float fraction = 1f / perilOutput.Count;
            for (int i = 0; i < perilOutput.Count; ++i)
            {
                for (int y = 0; y < perilOutput[0].GetLength(1); ++y)
                {
                    for (int x = 0; x < perilOutput[0].GetLength(0); ++x)
                    {
                        combinedPerilOutput[x, y] += perilOutput[i][x, y] * fraction;
                    }
                }
            }

            return combinedPerilOutput;

            /*if(PERIL == null)
            {
                PERIL = new kPERIL_DLL.kPERIL();
            }

            int xDim = WUIEngine.SIM.FireModule.GetCellCountX();
            int yDim = WUIEngine.SIM.FireModule.GetCellCountY();
            //assume cell/raster is square
            int cellSize = Mathf.RoundToInt(WUIEngine.SIM.FireModule.GetCellSizeX());
            //get wuiarea from a user defined map painted in wuinity
            int[,] WUIarea = GetWUIArea();           

            int RSET = (int)(WUIEngine.OUTPUT.TotalAverageEvacTime / 60f);
            //collect ROS, how to send ROS data is ROStheta[X*Y,8], y first, x second, start in north and then clockwise
            float[,] maxROS = WUIEngine.SIM.FireModule.GetMaxROS();
            float[,] rosAzimuth = WUIEngine.SIM.FireModule.GetMaxROSAzimuth();
            int[,] wuiArea = GetWUIArea();

            int[,] perilOutput = null;
            using (StringWriter output = new StringWriter())
            {
                perilOutput = PERIL.CalculateBoundary(cellSize, RSET, midFlameWindspeed, wuiArea, maxROS, rosAzimuth, output);
                WUIEngine.LOG(WUIEngine.LogType.Log, "k-PERIL output:\n" + output.ToString());
            }   

            return perilOutput;*/
        }

        private static int[,] GetWUIArea()
        {
            //first count how many cells we have to add to array
            int count = 0;
            for (int i = 0; i < WUIEngine.RUNTIME_DATA.Fire.WuiAreaIndices.Length; i++)
            {
                int xIndex = i % WUIEngine.SIM.FireModule.GetCellCountX();
                int yIndex = i / WUIEngine.SIM.FireModule.GetCellCountY();
                if(WUIEngine.RUNTIME_DATA.Fire.WuiAreaIndices[i] == true)
                {
                    ++count;
                }
            }

            //then create array of correct size and fill it
            int[,] wuiArea = new int[2, count];
            int position = 0;
            for (int i = 0; i < WUIEngine.RUNTIME_DATA.Fire.WuiAreaIndices.Length; i++)
            {
                int xIndex = i % WUIEngine.SIM.FireModule.GetCellCountX();
                int yIndex = i / WUIEngine.SIM.FireModule.GetCellCountY();
                int yFlipped = WUIEngine.SIM.FireModule.GetCellCountY() - 1 - yIndex;
                if (WUIEngine.RUNTIME_DATA.Fire.WuiAreaIndices[i] == true)
                {
                    wuiArea[0, position] = xIndex;
                    wuiArea[1, position] = yFlipped;
                    ++position;    
                }
            }
            return wuiArea;
        }
    }    
}