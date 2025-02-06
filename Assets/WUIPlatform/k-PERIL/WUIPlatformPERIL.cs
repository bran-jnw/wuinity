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

            WUIEngine.LOG(WUIEngine.LogType.Debug, WUIEngine.RUNTIME_DATA.Fire.LCPData.GetCellCountX() + ", " + WUIEngine.RUNTIME_DATA.Fire.LCPData.GetCellCountY() + ", " + WUIEngine.SIM.FireModule.GetCellCountX() + ", " + WUIEngine.SIM.FireModule.GetCellCountY());

            int xDim = WUIEngine.RUNTIME_DATA.Fire.LCPData.GetCellCountX();
            int yDim = WUIEngine.RUNTIME_DATA.Fire.LCPData.GetCellCountY();
            //assume cell/raster is square
            int cellSize = Mathf.RoundToInt((float)WUIEngine.RUNTIME_DATA.Fire.LCPData.RasterCellResolutionX);
            //get wuiarea from a user defined map painted in wuinity
            int[,] WUIarea = GetWUIArea();

            //create multiple time buffers
            float RSET = WUIEngine.OUTPUT.TotalAverageEvacTime / 60f;
            float[] RSETs = new float[10];
            for (int i = 0; i < RSETs.Length; ++i)
            {
                RSETs[i] = RSET + i * 60 * 12f;
            }

            float[,] maxROS;
            float[,] rosAzimuth;
            bool useFireScenarioInput = false;
            if (useFireScenarioInput)
            {
                WUIEngine.LOG(WUIEngine.LogType.Debug, "Using ROS from fire.");
                maxROS = WUIEngine.SIM.FireModule.GetMaxROS();
                rosAzimuth = WUIEngine.SIM.FireModule.GetMaxROSAzimuth();
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Debug, "Calculating ROS with Behave.");
                CalculateRateOfSpreadAndDirection(out maxROS, out rosAzimuth, midFlameWindspeed, 150f);
            }
            
            int[,] wuiArea = GetWUIArea();

            WUIEngine.LOG(WUIEngine.LogType.Debug, maxROS.GetLength(0) + ", " + maxROS.GetLength(1) + ", " + rosAzimuth.GetLength(0) + ", " + rosAzimuth.GetLength(1));

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

        //private const TwoFuelModelsMethod twoFuelModelsMethod = TwoFuelModelsMethod.NoMethod;
        private const BehaveUnits.MoistureUnits.MoistureUnitsEnum moistureUnits = BehaveUnits.MoistureUnits.MoistureUnitsEnum.Percent;
        private const WindHeightInputMode windHeightInputMode = WindHeightInputMode.DirectMidflame;
        private const BehaveUnits.SlopeUnits.SlopeUnitsEnum slopeUnits = BehaveUnits.SlopeUnits.SlopeUnitsEnum.Degrees;
        private const BehaveUnits.CoverUnits.CoverUnitsEnum coverUnits = BehaveUnits.CoverUnits.CoverUnitsEnum.Fraction;
        private const BehaveUnits.LengthUnits.LengthUnitsEnum lengthUnits = BehaveUnits.LengthUnits.LengthUnitsEnum.Meters;
        private const BehaveUnits.SpeedUnits.SpeedUnitsEnum speedUnits = BehaveUnits.SpeedUnits.SpeedUnitsEnum.MetersPerMinute;
        private const WindAndSpreadOrientationMode windAndSpreadOrientationMode = WindAndSpreadOrientationMode.RelativeToNorth;

        private static void CalculateRateOfSpreadAndDirection(out float[,] rateOfSpreads, out float[,] spreadDirections, float midFlameWindspeed, float windDirection)
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
                    //k-PERIL crashes if edges has non-zero data
                    if(x < 1 || x > xDim - 2 || y < 1 || y > yDim - 2)
                    {
                        continue;
                    }
                    LandscapeStruct cellData = lcpData.GetCellData(x, y);
                    int fuelModelNumber = cellData.fuel_model;
                    int slope = cellData.slope;
                    int aspect = cellData.aspect;

                    InitialFuelMoisture moisture = WUIEngine.RUNTIME_DATA.Fire.InitialFuelMoistureData.GetInitialFuelMoisture(fuelModelNumber);
                    double moistureOneHour = moisture.OneHour;
                    double moistureTenHour = moisture.TenHour;
                    double moistureHundredHour = moisture.HundredHour;
                    double moistureLiveHerbaceous = moisture.LiveHerbaceous;
                    double moistureLiveWoody = moisture.LiveWoody;    

                    double canopyCover = cellData.canopy_cover;
                    double canopyHeight = cellData.crown_canopy_height;
                    double crownRatio = cellData.crown_bulk_density; //TODO: is this correct?

                    surfaceFire.updateSurfaceInputs(fuelModelNumber, moistureOneHour, moistureTenHour, moistureHundredHour, moistureLiveHerbaceous, moistureLiveWoody, moistureUnits,
                        midFlameWindspeed, speedUnits, windHeightInputMode, windDirection, windAndSpreadOrientationMode, slope, slopeUnits, aspect, canopyCover, coverUnits, canopyHeight, lengthUnits, crownRatio);

                    surfaceFire.doSurfaceRunInDirectionOfMaxSpread();
                    //flip y-axis
                    rateOfSpreads[x, yDim - 1 - y]=  (float)surfaceFire.getSpreadRate(speedUnits);
                    spreadDirections[x, yDim - 1 - y] = (float)surfaceFire.getDirectionOfMaxSpread();
                }
            }            
        }

        private static int[,] GetWUIArea()
        {
            int xDim = WUIEngine.RUNTIME_DATA.Fire.LCPData.GetCellCountX();
            int yDim = WUIEngine.RUNTIME_DATA.Fire.LCPData.GetCellCountY();

            //first count how many cells we have to add to array
            int count = 0;
            for (int i = 0; i < WUIEngine.RUNTIME_DATA.Fire.WuiAreaIndices.Length; i++)
            {
                int xIndex = i % xDim;
                int yIndex = i / yDim; 
                if (WUIEngine.RUNTIME_DATA.Fire.WuiAreaIndices[i] == true)
                {
                    ++count;
                }
            }

            //then create array of correct size and fill it
            int[,] wuiArea = new int[2, count];
            int position = 0;
            for (int i = 0; i < WUIEngine.RUNTIME_DATA.Fire.WuiAreaIndices.Length; i++)
            {
                int xIndex = i % xDim;
                int yIndex = i / yDim;
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