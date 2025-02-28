//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.IO;
using System.Collections.Generic;
using WUIPlatform.Fire.Behave;
using WUIPlatform.Fire;

namespace WUIPlatform
{
    public static class WUIPlatformPERIL
    {
        private static kPERIL_DLL.kPERIL PERIL;
        //private const TwoFuelModelsMethod twoFuelModelsMethod = TwoFuelModelsMethod.NoMethod;
        private const BehaveUnits.MoistureUnits.MoistureUnitsEnum moistureUnits = BehaveUnits.MoistureUnits.MoistureUnitsEnum.Percent;
        private const WindHeightInputMode windHeightInputMode = WindHeightInputMode.DirectMidflame;
        private const BehaveUnits.SlopeUnits.SlopeUnitsEnum slopeUnits = BehaveUnits.SlopeUnits.SlopeUnitsEnum.Degrees;
        private const BehaveUnits.CoverUnits.CoverUnitsEnum coverUnits = BehaveUnits.CoverUnits.CoverUnitsEnum.Fraction;
        private const BehaveUnits.LengthUnits.LengthUnitsEnum lengthUnits = BehaveUnits.LengthUnits.LengthUnitsEnum.Meters;
        private const BehaveUnits.SpeedUnits.SpeedUnitsEnum windSpeedUnits = BehaveUnits.SpeedUnits.SpeedUnitsEnum.MetersPerSecond;
        private const WindAndSpreadOrientationMode windAndSpreadOrientationMode = WindAndSpreadOrientationMode.RelativeToNorth;


        /// <summary>
        /// Runs k-PERIL, should be called after a completed simulation
        /// </summary>
        /// <param name="midflameWindspeed">User have to pick a representative mid flame wind speed as k-PERIL does not take changing weather into account</param>
        public static float[,] RunPERIL(float midflameWindspeed)
        {
            WUIEngine.LOG(WUIEngine.LogType.Log, "Starting calculation of trigger buffer using k-PERIL.");

            if (PERIL == null)
            {
                PERIL = new kPERIL_DLL.kPERIL();
            }

            int xDim = WUIEngine.RUNTIME_DATA.Fire.LCPData.GetCellCountX();
            int yDim = WUIEngine.RUNTIME_DATA.Fire.LCPData.GetCellCountY();
            //assume cell/raster is square
            int cellSize = Mathf.RoundToInt((float)WUIEngine.RUNTIME_DATA.Fire.LCPData.RasterCellResolutionX);
            //get wuiarea from a user defined map painted in wuinity
            int[,] perilWUIArea = GetPerilWUIArea();

            //create multiple time buffers
            float RSET = WUIEngine.OUTPUT.TotalAverageEvacTime / 60f;
            float[] RSETs = new float[1];
            for (int i = 0; i < RSETs.Length; ++i)
            {
                RSETs[i] = RSET + i * 60 * 12f;
            }

            float[,] maxROS;
            float[,] rosAzimuth;
            if (WUIEngine.INPUT.TriggerBuffer.kPERILInput.CalculateROSFromBehave)
            {
                WUIEngine.LOG(WUIEngine.LogType.Log, "k-PERIL is using ROS calculate using Behave.");
                bool[,] wuiArea2D = null;// GetWUIArea2D(WUIEngine.RUNTIME_DATA.Fire.WuiArea, xDim, yDim);
                CalculateAllRateOfSpreadsAndDirections(out maxROS, out rosAzimuth, midflameWindspeed, 150f, true, wuiArea2D);
            }
            else
            {    
                WUIEngine.LOG(WUIEngine.LogType.Log, "k-PERIL is using ROS from simulation.");
                maxROS = WUIEngine.SIM.FireModule.GetMaxROS();
                rosAzimuth = WUIEngine.SIM.FireModule.GetMaxROSAzimuth();
            }

            List<int[,]> perilOutput = new List<int[,]>(RSETs.Length);
            for (int i = 0; i < RSETs.Length; ++i)
            {
                using (StringWriter output = new StringWriter())
                {
                    int[,] result = PERIL.CalculateBoundary(cellSize, RSETs[i], midflameWindspeed, perilWUIArea, maxROS, rosAzimuth, output);
                    perilOutput.Add(result);
                    WUIEngine.LOG(WUIEngine.LogType.Log, "k-PERIL output, RSET= " + RSETs[i] + " minutes.\n" + output.ToString());
                }
            }

            //k-PERIL returns flipped x/y and inversed y, so fix this here
            float[,] combinedPerilOutput = new float[perilOutput[0].GetLength(1), perilOutput[0].GetLength(0)];
            float fraction = 1f / perilOutput.Count;
            for (int i = 0; i < perilOutput.Count; ++i)
            {
                for (int y = 0; y < perilOutput[0].GetLength(1); ++y)
                {
                    for (int x = 0; x < perilOutput[0].GetLength(0); ++x)
                    {
                        combinedPerilOutput[y, yDim  - 1 - x] += perilOutput[i][x, y] * fraction;
                    }
                }
            }

            SaveToFile(combinedPerilOutput, xDim, yDim, cellSize);

            return combinedPerilOutput;
        }   

        private static void SaveToFile(float[,] data, int xDim, int yDim, float cellsize)
        {
            try
            {
                string file;
                if (WUIEngine.INPUT.TriggerBuffer.kPERILInput.OutputName == null || WUIEngine.INPUT.TriggerBuffer.kPERILInput.OutputName.Length == 0)
                {
                    file = "trigger_buffer.asc";
                }
                else
                {
                    file = WUIEngine.INPUT.TriggerBuffer.kPERILInput.OutputName;
                }
                string path = Path.Combine(WUIEngine.WORKING_FOLDER, file);
                using (StreamWriter outputFile = new StreamWriter(path))
                {
                    string line;

                    line = "ncols " + xDim;
                    outputFile.WriteLine(line);
                    line = "nrows " + yDim;
                    outputFile.WriteLine(line);
                    line = "xllcorner " + 0;
                    outputFile.WriteLine(line);
                    line = "yllcorner " + 0;
                    outputFile.WriteLine(line);
                    line = "cellsize " + cellsize;
                    outputFile.WriteLine(line);
                    line = "NODATA_value " + -9999;
                    outputFile.WriteLine(line);

                    for (int y = 0; y < yDim; ++y) 
                    {
                        line = "";
                        for (int x = 0; x < xDim; ++x)
                        {
                            //flip y to follow asc standard
                            line += data[x, yDim - 1- y] + " ";
                        }
                        outputFile.WriteLine(line);
                    }
                }
            }
            catch (System.Exception e)
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, e.Message);
            }
        }

        private static void CalculateAllRateOfSpreadsAndDirections(out float[,] rateOfSpreads, out float[,] spreadDirections, float midFlameWindspeed, float windDirection, bool flipYaxis, bool[,] wuiArea = null)
        {
            LCPData lcpData = WUIEngine.RUNTIME_DATA.Fire.LCPData;
            int xDim = lcpData.GetCellCountX();
            int yDim = lcpData.GetCellCountY();
            rateOfSpreads = new float[xDim, yDim];
            spreadDirections = new float[xDim, yDim];

            //we need to create and fill the fuel model set. TODO: create this data globally in RUNTIME_DATA.Fire
            FuelModelSet fuelModelSet = new FuelModelSet();
            if (WUIEngine.DATA_STATUS.FuelModelsLoaded)
            {
                for (int i = 0; i < WUIEngine.RUNTIME_DATA.Fire.FuelModelsData.Fuels.Count; i++)
                {
                    fuelModelSet.setFuelModelRecord(WUIEngine.RUNTIME_DATA.Fire.FuelModelsData.Fuels[i]);
                }
            }
            Surface surfaceFire = new Surface(fuelModelSet);

            for (int y = 0; y < yDim; ++y)
            {
                for (int x = 0; x < xDim; ++x)
                {
                    //if WUI area specified we skip calc here
                    if (wuiArea != null && wuiArea[x, y])
                    {
                        continue;
                    }

                    //k-PERIL crashes if edges has non-zero data
                    if (x < 1 || x > xDim - 2 || y < 1 || y > yDim - 2)
                    {
                        continue;
                    }
                    LandscapeStruct cellData = lcpData.GetCellData(x, y);
                    InitialFuelMoisture moisture = WUIEngine.RUNTIME_DATA.Fire.kPERILInitialFuelMoistureData.GetInitialFuelMoisture(cellData.fuel_model);
                    double crownRatio = 1.5; //TODO: how to get this data? LCP does not seem to carry it

                    surfaceFire.updateSurfaceInputs(cellData.fuel_model, moisture.OneHour, moisture.TenHour, moisture.HundredHour, moisture.LiveHerbaceous, moisture.LiveWoody, moistureUnits,
                        midFlameWindspeed, windSpeedUnits, windHeightInputMode, windDirection, windAndSpreadOrientationMode, cellData.slope, slopeUnits, cellData.aspect, cellData.canopy_cover, coverUnits, cellData.crown_canopy_height, lengthUnits, crownRatio);

                    surfaceFire.doSurfaceRunInDirectionOfMaxSpread();
                    int yIndex = y;
                    if(flipYaxis)
                    {
                        yIndex = yDim - 1 - y;
                    }
                    rateOfSpreads[x, yIndex] = (float)surfaceFire.getSpreadRate(BehaveUnits.SpeedUnits.SpeedUnitsEnum.MetersPerMinute);
                    spreadDirections[x, yIndex] = (float)surfaceFire.getDirectionOfMaxSpread();

                }
            }
        }

        private static int[,] GetPerilWUIArea()
        {
            int xDim = WUIEngine.RUNTIME_DATA.Fire.LCPData.GetCellCountX();
            int yDim = WUIEngine.RUNTIME_DATA.Fire.LCPData.GetCellCountY();

            //first count how many cells we have to add to array
            int count = 0;
            for (int i = 0; i < WUIEngine.RUNTIME_DATA.Fire.WuiArea.Length; i++)
            {
                if (WUIEngine.RUNTIME_DATA.Fire.WuiArea[i] == true)
                {
                    ++count;
                }
            }

            //then create array of correct size and fill it
            int[,] wuiArea = new int[2, count];
            int position = 0;
            for (int i = 0; i < WUIEngine.RUNTIME_DATA.Fire.WuiArea.Length; i++)
            {
                int xIndex = i % xDim;
                int yIndex = i / xDim;
                int yFlipped = yDim - 1 - yIndex;
                if (WUIEngine.RUNTIME_DATA.Fire.WuiArea[i])
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