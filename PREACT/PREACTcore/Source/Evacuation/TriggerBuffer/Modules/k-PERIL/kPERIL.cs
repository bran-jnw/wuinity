//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using PREACT.Wildfire.Behave;
using PREACT.Wildfire;
using PREACT.Math;
using kPERIL;
using System.IO;

namespace PREACT
{
    public class kPERIL : TriggerBufferModule
    {        
        //private const TwoFuelModelsMethod twoFuelModelsMethod = TwoFuelModelsMethod.NoMethod;
        private const BehaveUnits.MoistureUnits.MoistureUnitsEnum moistureUnits = BehaveUnits.MoistureUnits.MoistureUnitsEnum.Percent;
        private const WindHeightInputMode windHeightInputMode = WindHeightInputMode.DirectMidflame;
        private const BehaveUnits.SlopeUnits.SlopeUnitsEnum slopeUnits = BehaveUnits.SlopeUnits.SlopeUnitsEnum.Degrees;
        private const BehaveUnits.CoverUnits.CoverUnitsEnum coverUnits = BehaveUnits.CoverUnits.CoverUnitsEnum.Fraction;
        private const BehaveUnits.LengthUnits.LengthUnitsEnum lengthUnits = BehaveUnits.LengthUnits.LengthUnitsEnum.Meters;
        private const BehaveUnits.SpeedUnits.SpeedUnitsEnum windSpeedUnits = BehaveUnits.SpeedUnits.SpeedUnitsEnum.MetersPerSecond;
        private const WindAndSpreadOrientationMode windAndSpreadOrientationMode = WindAndSpreadOrientationMode.RelativeToNorth;

        private int _xDim, _yDim;
        private kPERILcore _peril;
        private bool _calculateROS;
        private LandscapeData? _lcpData;
        private float _RSET;
        private bool[] _wuiArea;
        private float _midflameWindspeed;
        private float _windDirection;

        private float[,] _maxROS;
        private float[,] _rosAzimuth;
        private float[,] _elevation;
        private InitialFuelMoistureLibrary? _fuelMoisture;
        private FuelModelInput? _fuelModel;


        public kPERIL(float RSET, bool[] wuiArea, float midflameWindspeed, float windDirection, float[,] maxROS, float[,] rosAzimuth)
        {
            _calculateROS = false;
            _peril = new kPERILcore();
            _xDim = maxROS.GetLength(0);
            _yDim = maxROS.GetLength(1);
     
            _RSET = RSET;
            _wuiArea = wuiArea;
            _midflameWindspeed = midflameWindspeed;
            _windDirection = windDirection;

            _maxROS = maxROS;
            _rosAzimuth = rosAzimuth;
        }

        public kPERIL(LandscapeData lcpData, float RSET, bool[] wuiArea, float midflameWindspeed, float windDirection,  InitialFuelMoistureLibrary fuelMoisture, FuelModelInput fuelModel)
        {
            _calculateROS = true;
            _peril = new kPERILcore();
            _xDim = lcpData.GetCellCountX();
            _yDim = lcpData.GetCellCountY();           

            _lcpData = lcpData;
            _RSET = RSET;
            _wuiArea = wuiArea;
            _midflameWindspeed = midflameWindspeed;
            _windDirection = windDirection;

            _fuelMoisture = fuelMoisture;
            _fuelModel = fuelModel;
        }

        /// <summary>
        /// Runs k-PERIL, should be called after a completed simulation
        /// </summary>
        /// <param name="_midflameWindspeed">User have to pick a representative mid flame wind speed as k-PERIL does not take changing weather into account</param>
        public override void Run()
        {
            Engine.Message(null, Engine.LogType.Log, "Starting calculation of trigger buffer using k-PERIL.");            

            //assume cell/raster is square
            int cellSize = Mathf.RoundToInt((float)_lcpData.RasterCellResolutionX);

            if (_calculateROS)
            {
                Engine.Message(null, Engine.LogType.Log, "k-PERIL is calculating ROS using Behave.");
                bool[,] wuiArea2D = GetWUIArea2D(_wuiArea, _xDim, _yDim, false);
                CalculateAllRateOfSpreadsAndDirections(_lcpData, out _maxROS, out _rosAzimuth, _midflameWindspeed, _windDirection, true, _fuelMoisture, wuiArea2D, _fuelModel);
            }
            else
            {    
                Engine.Message(null, Engine.LogType.Log, "k-PERIL is using ROS (and azimuth) from provided data.");
                _peril.perilData.importFireRastersByVariable(FlipOnYAxis(_maxROS), FlipOnYAxis(_rosAzimuth));
                float[,] perilWUIArea = GetPerilWUIArea(_xDim, _yDim, _wuiArea);
                _peril.perilData.importWuiRastersByVariable(FlipOnYAxis(perilWUIArea));
                _peril.perilData.importTopographyRastersByVariable(FlipOnYAxis(_elevation));
                _peril.perilData.rasteriseWindFromScalars(_midflameWindspeed, _windDirection);
            }

            using (StringWriter output = new StringWriter())
            {
                _triggerBufferOutput = _peril.Run(_RSET);
                Engine.Message(null, Engine.LogType.Log, "k-PERIL output, RSET= " + _RSET + " minutes.\n" + output.ToString());
            }

            //k-PERIL returns flipped x/y and inversed y, so fix this here
            float[,] temp = _triggerBufferOutput;
            _triggerBufferOutput = new float[_xDim, _yDim];
            for (int y = 0; y < _yDim; ++y)
            {
                for (int x = 0; x < _xDim; ++x)
                {
                    _triggerBufferOutput[y, _yDim - 1 - x] = temp[x, y];
                }
            }            
        }   

        public static void SaveToFile(float[,] data, float cellsize, string outputFilePath)
        {
            try
            {
                using (StreamWriter outputWriter = new StreamWriter(outputFilePath))
                {
                    string line;
                    int xDim = data.GetLength(0); 
                    int yDim = data.GetLength(1);  

                    line = "ncols " + xDim;
                    outputWriter.WriteLine(line);
                    line = "nrows " + yDim;
                    outputWriter.WriteLine(line);
                    line = "xllcorner " + 0;
                    outputWriter.WriteLine(line);
                    line = "yllcorner " + 0;
                    outputWriter.WriteLine(line);
                    line = "cellsize " + cellsize;
                    outputWriter.WriteLine(line);
                    line = "NODATA_value " + -9999;
                    outputWriter.WriteLine(line);

                    for (int y = 0; y < yDim; ++y)
                    {
                        line = "";
                        for (int x = 0; x < xDim; ++x)
                        {
                            //flip y to follow asc standard
                            line += data[x, yDim - 1 - y];
                            if (x < xDim - 1)
                            {
                                line += " ";
                            }
                        }
                        outputWriter.WriteLine(line);
                    }
                }
            }
            catch (System.Exception e)
            {
                Engine.Message(null, Engine.LogType.Warning, e.Message);
            }
        }

        private static void CalculateAllRateOfSpreadsAndDirections(LandscapeData lcpData, out float[,] rateOfSpreads, out float[,] spreadDirections, float midFlameWindspeed, float windDirection, bool flipYaxis, InitialFuelMoistureLibrary initialFuelMoistureLibrary, bool[,]? wuiArea = null, FuelModelInput? fuelModelInputs = null)
        {
            int xDim = lcpData.GetCellCountX();
            int yDim = lcpData.GetCellCountY();
            rateOfSpreads = new float[xDim, yDim];
            spreadDirections = new float[xDim, yDim];

            //we need to create and fill the fuel model set. TODO: create this data globally in RUNTIME_DATA.Fire
            FuelModelSet fuelModelSet = new FuelModelSet();
            if (fuelModelInputs != null)
            {
                for (int i = 0; i < fuelModelInputs.Fuels.Count; i++)
                {
                    fuelModelSet.setFuelModelRecord(fuelModelInputs.Fuels[i]);
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
                    LandscapeCellData cellData = lcpData.GetCellData(x, y);
                    InitialFuelMoisture moisture = initialFuelMoistureLibrary.GetInitialFuelMoisture(cellData.fuel_model);
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

        private static bool[,] GetWUIArea2D(bool[] wuiArea, int xDim, int yDim, bool flipY)
        {
            bool[,] result = new bool[xDim, yDim];
            for (int i = 0; i < wuiArea.Length; i++)
            {
                int xIndex = i % xDim;
                int yIndex = i / xDim;
                if (wuiArea[i] == true)
                {
                    if(flipY)
                    {
                        int yFlipped = yDim - 1 - yIndex;
                    }
                    result[xIndex, yIndex] = true;
                }
            }

            return result;
        }

        private static float[,] GetPerilWUIArea(int xDim, int yDim, bool[] wuiArea)
        {
            float[,] newWuiArea = new float[xDim, yDim];
            for (int i = 0; i < wuiArea.Length; i++)
            {
                int xIndex = i % xDim;
                int yIndex = i / xDim;
                int yFlipped = yDim - 1 - yIndex;
                if (wuiArea[i])
                {
                    newWuiArea[xIndex, yIndex] = 1f;
                }
            }
            return newWuiArea;
        }

        private static float[,] FlipOnYAxis(float[,] input)
        {
            int xDim = input.GetLength(0);
            int yDim = input.GetLength(1);  

            float[,] output = new float[xDim, yDim];
            for (int y = 0; y < yDim; y++)
            {
                for (int x = 0; x < xDim; x++)
                {
                    int yFlipped = yDim - 1 - y;
                    output[x, y] = input[x, yFlipped];
                }
            }
            return output;
        }
    }    
}