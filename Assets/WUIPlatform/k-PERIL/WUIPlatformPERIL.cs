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
            return CalculateBackwardsFire(midFlameWindspeed, 150f);

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
                bool[,] wuiArea2D = null;// GetWUIArea2D(WUIEngine.RUNTIME_DATA.Fire.WuiArea, xDim, yDim);
                CalculateAllRateOfSpreadsAndDirections(out maxROS, out rosAzimuth, midFlameWindspeed, 150f, true, wuiArea2D);
            }

            List<int[,]> perilOutput = new List<int[,]>(RSETs.Length);
            for (int i = 0; i < RSETs.Length; ++i)
            {
                using (StringWriter output = new StringWriter())
                {
                    int[,] result = PERIL.CalculateBoundary(cellSize, RSETs[i], midFlameWindspeed, perilWUIArea, maxROS, rosAzimuth, output);
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

            return combinedPerilOutput;
        }

        private static readonly Vector2int[] neighborIndices = new Vector2int[] { Vector2int.up, new Vector2int(1, 1), Vector2int.right, new Vector2int(1, -1), Vector2int.down, new Vector2int(-1, -1), Vector2int.left, new Vector2int(-1, 1) };
        //as all spread rates point inwards to center north becomes 180 degrees, south 0 degrees etc
        private static readonly float[] spreadDegrees = new float[] { 180f, 225f, 270f, 315f, 0f, 45f, 90f, 135f };
        //private static readonly float[] deadCell = new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
        private struct CellSpreadRates
        {
            public float[] _spreadRates;

            public CellSpreadRates(float[] spreadRates)
            {
                _spreadRates = spreadRates;
            }
        }

        static readonly float sqrt2 = Mathf.Sqrt(2f);
        private class BackwardsFireCell
        {
            Vector2int _index;
            BackwardsFireCell[] _neighbors = new BackwardsFireCell[8];
            public bool _burntOut, _ignited;
            public LandscapeStruct _lcp;
            float[] _flamefronts = new float[8];
            float[] _inverseSpreadRates = new float[8];
            float[] _spreadDistances = new float[8];


            public BackwardsFireCell(int xIndex, int yIndex, Surface surface, LCPData lcpData, bool[,] wuiArea, int xDim, int yDim)
            {
                _index = new Vector2int(xIndex, yIndex);
                _lcp = lcpData.GetCellData(_index.x, _index.y);
                if (wuiArea[_index.x, _index.y] || surface.isAllFuelLoadZero(_lcp.fuel_model))
                {
                    _burntOut = true;
                    return;
                }

                //TODO: calculate using BEHAVE
                for (int i = 0; i < _inverseSpreadRates.Length; i++)
                {
                    _inverseSpreadRates[i] = 0.3f;
                }
            }

            public void SetNeighbors(int xDim, int yDim, bool[,] wuiArea, Surface surface, BackwardsFireCell[,] cells)
            {
                for (int i = 0; i < _neighbors.Length; ++i)
                {
                    Vector2int neighborIndex = _index + neighborIndices[i];
                    if (IsInside(xDim, yDim, neighborIndex))
                    {
                        _neighbors[i] = cells[neighborIndex.x, neighborIndex.y];
                        if (_neighbors[i]._burntOut)
                        {
                            _neighbors[i] = null;
                        }
                        else
                        {
                            _spreadDistances[i] = 30f;
                            //if not cardinal direction we go diagonally
                            if (i % 2 != 0)
                            {
                                _spreadDistances[i] *= sqrt2;
                            }
                        }                        
                    }
                }
            }

            //returns the spreadrate in the opposite direction of who is requesting it, e.g. a neighbor north of you will have you indexed as south, so we want the north spread rate
            public float GetSpreadrate(int fromRelativeIndex)
            {
                return _inverseSpreadRates[(fromRelativeIndex + 4) % 8];
            }

            public float GetFlamefront(int fromRelativeIndex)
            {
                return _flamefronts[(fromRelativeIndex + 4) % 8];
            }

            public void Step(float currentTime, float deltaTime)
            {
                if(_burntOut || !_ignited)
                {
                    return;
                }

                int spreadLeft = 0;
                for (int i = 0; i < _flamefronts.Length; ++i)
                {
                    if (_flamefronts[i] < 0.5f * _spreadDistances[i])
                    {
                        _flamefronts[i] += deltaTime * _inverseSpreadRates[i];
                        ++spreadLeft;
                    }
                    else if (_neighbors[i] != null && !_neighbors[i]._burntOut)
                    {
                        _flamefronts[i] += deltaTime * _neighbors[i].GetSpreadrate(i);
                        //flamefronts have crossed or we have reached the center of the neighbor
                        if (_flamefronts[i] + _neighbors[i].GetFlamefront(i) > _spreadDistances[i])
                        {
                            if (!_neighbors[i]._ignited)
                            {
                                _neighbors[i].SchedurelIgnite(currentTime + deltaTime);
                            }
                            _neighbors[i] = null;
                        }
                        else
                        {
                            ++spreadLeft;
                        }
                    }
                }

                //we have nowhere left to spread, so we are done
                if (spreadLeft == 0)
                {
                    _burntOut = true;
                }
            }

            bool scheduleIgnition;
            public float _timeOfArrival;
            public void SchedurelIgnite(float timeOfArrival)
            {
                if(!_burntOut && !_ignited)
                {
                    scheduleIgnition = true;
                    _timeOfArrival = timeOfArrival;
                }                
            }

            public void TryIgnite()
            {
                if (scheduleIgnition)
                {
                    _ignited = true;
                }
            }            
        }

        private static bool IsInside(int xDim, int yDim, Vector2int index)
        {
            if (index.x < 0 || index.x > xDim - 1 || index.y < 0 || index.y > yDim - 1)
            {
                return false;
            }

            return true;
        }

        private static float[,] CalculateBackwardsFire(float midFlameWindspeed, float windDirection)
        {
            WUIEngine.LOG(WUIEngine.LogType.Log, "Beginning backwards calculation of fire spread.");

            int xDim = WUIEngine.RUNTIME_DATA.Fire.LCPData.GetCellCountX();
            int yDim = WUIEngine.RUNTIME_DATA.Fire.LCPData.GetCellCountY();
            bool[,] wuiArea = GetWUIArea2D(WUIEngine.RUNTIME_DATA.Fire.WuiArea, xDim, yDim);
            float distance = (float)WUIEngine.RUNTIME_DATA.Fire.LCPData.RasterCellResolutionX;
            float distanceDiagonal = Mathf.Sqrt(2) * distance;

            List<Vector2int> wuiIgnitionBorder = GetWUIEdgeCellIndices(wuiArea);

            FuelModelSet fuelModelSet = new FuelModelSet();
            if (WUIEngine.DATA_STATUS.FuelModelsLoaded)
            {
                for (int i = 0; i < WUIEngine.RUNTIME_DATA.Fire.FuelModelsData.Fuels.Count; i++)
                {
                    fuelModelSet.setFuelModelRecord(WUIEngine.RUNTIME_DATA.Fire.FuelModelsData.Fuels[i]);
                }
            }
            Surface surfaceFire = new Surface(fuelModelSet);
            BackwardsFireCell[,] fireCells = new BackwardsFireCell[xDim, yDim];

            //create
            for(int y = 0; y < yDim; ++y)
            {
                for (int x = 0; x < xDim; ++x)
                {
                    fireCells[x, y] = new BackwardsFireCell(x, y, surfaceFire, WUIEngine.RUNTIME_DATA.Fire.LCPData, wuiArea, xDim, yDim);
                }
            }

            //initialize
            for (int y = 0; y < yDim; ++y)
            {
                for (int x = 0; x < xDim; ++x)
                {
                    fireCells[x, y].SetNeighbors(xDim, yDim, wuiArea, surfaceFire, fireCells);
                }
            }

            //initial ignition
            for(int i = 0; i < wuiIgnitionBorder.Count; ++i)
            {
                for (int j = 0; j < neighborIndices.Length; ++j)
                {
                    Vector2int index = wuiIgnitionBorder[i] + neighborIndices[j];
                    if(IsInside(xDim, yDim, index))
                    {
                        fireCells[index.x, index.y].SchedurelIgnite(0f);
                    }
                }
            }

            float currentTime = 0f;
            float deltaTime = 10f;
            float endTime = 10000f;
            float[,] triggerBuffer = new float[xDim, yDim];
            while (currentTime < endTime)
            {
                if(currentTime + deltaTime > endTime)
                {
                    deltaTime = endTime - currentTime;
                }

                //handle ignitions
                for (int y = 0; y < yDim; ++y)
                {
                    for (int x = 0; x < xDim; ++x)
                    {
                        fireCells[x, y].TryIgnite();
                    }
                }

                //step forward in time
                for (int y = 0; y < yDim; ++y)
                {
                    for (int x = 0; x < xDim; ++x)
                    {
                        fireCells[x, y].Step(currentTime, deltaTime);
                    }
                }
                currentTime += deltaTime;
            }

            //collect time of arrival, make sure to update if any cells were ignited last time step
            for (int y = 0; y < yDim; ++y)
            {
                for (int x = 0; x < xDim; ++x)
                {
                    fireCells[x, y].TryIgnite();
                    if (fireCells[x, y]._ignited)
                    {
                        triggerBuffer[x, y] = fireCells[x, y]._timeOfArrival / endTime;
                    }
                }
            }

            WUIEngine.LOG(WUIEngine.LogType.Log, "Finished backwards calculation of fire spread.");
            return triggerBuffer;

            float[,] rateOfSpreads, spreadDirections;
            CalculateAllRateOfSpreadsAndDirections(out rateOfSpreads, out spreadDirections, midFlameWindspeed, windDirection, false, wuiArea);
        }

        

        private static List<Vector2int> GetWUIEdgeCellIndices(bool[,] wuiArea)
        {
            List<Vector2int> borderCells = new List<Vector2int>();

            int xDim = WUIEngine.RUNTIME_DATA.Fire.LCPData.GetCellCountX();
            int yDim = WUIEngine.RUNTIME_DATA.Fire.LCPData.GetCellCountY();

            CellSpreadRates[,] rateOfSpreads = new CellSpreadRates[xDim, yDim];

            for (int y = 0; y < yDim; ++y)
            {
                for (int x = 0; x < xDim; ++x)
                {
                    if (wuiArea[x, y])
                    {
                        Vector2int index = new Vector2int(x, y);
                        if (HasNonWUINeighbors(wuiArea, xDim, yDim, index))
                        {
                            borderCells.Add(index);
                        }
                    }
                }
            }

            return borderCells;
        }        

        private static bool HasNonWUINeighbors(bool[,] wuiArea, int xDim, int yDim, Vector2int cellIndex)
        {
            bool hasNonWUINeighbors = false;

            for (int i = 0; i < neighborIndices.Length; ++i)
            {
                Vector2int neighborIndex = cellIndex + neighborIndices[i];
                CorrectForEdges(xDim, yDim, ref neighborIndex, cellIndex);
                //if we were outside of our area we get the same value back
                if (neighborIndex != cellIndex)
                {
                    if (!wuiArea[neighborIndex.x, neighborIndex.y])
                    {
                        hasNonWUINeighbors = true;
                        break;
                    }
                }
            }

            return hasNonWUINeighbors;
        }

        //Checks if we are outside of border in any direction, if so we return the "origin"
        private static void CorrectForEdges(int xDim, int yDim, ref Vector2int neighborIndex, Vector2int originIndex)
        {
            if (neighborIndex.x < 0 || neighborIndex.x > xDim - 1 || neighborIndex.y < 0 || neighborIndex.y > yDim - 1)
            {
                neighborIndex = originIndex;
            }
        }

        //private const TwoFuelModelsMethod twoFuelModelsMethod = TwoFuelModelsMethod.NoMethod;
        private const BehaveUnits.MoistureUnits.MoistureUnitsEnum moistureUnits = BehaveUnits.MoistureUnits.MoistureUnitsEnum.Percent;
        private const WindHeightInputMode windHeightInputMode = WindHeightInputMode.DirectMidflame;
        private const BehaveUnits.SlopeUnits.SlopeUnitsEnum slopeUnits = BehaveUnits.SlopeUnits.SlopeUnitsEnum.Degrees;
        private const BehaveUnits.CoverUnits.CoverUnitsEnum coverUnits = BehaveUnits.CoverUnits.CoverUnitsEnum.Fraction;
        private const BehaveUnits.LengthUnits.LengthUnitsEnum lengthUnits = BehaveUnits.LengthUnits.LengthUnitsEnum.Meters;
        private const BehaveUnits.SpeedUnits.SpeedUnitsEnum speedUnits = BehaveUnits.SpeedUnits.SpeedUnitsEnum.MetersPerMinute;
        private const WindAndSpreadOrientationMode windAndSpreadOrientationMode = WindAndSpreadOrientationMode.RelativeToNorth;

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
                    int fuelModelNumber = cellData.fuel_model;
                    int slope = cellData.slope;
                    int aspect = cellData.aspect;

                    InitialFuelMoisture moisture = WUIEngine.RUNTIME_DATA.Fire.InitialFuelMoistureData.GetInitialFuelMoisture(fuelModelNumber);
                    double crownRatio = 1.5; //TODO: how to get this data? LCP does not seem to carry it

                    surfaceFire.updateSurfaceInputs(fuelModelNumber, moisture.OneHour, moisture.TenHour, moisture.HundredHour, moisture.LiveHerbaceous, moisture.LiveWoody, moistureUnits,
                        midFlameWindspeed, speedUnits, windHeightInputMode, windDirection, windAndSpreadOrientationMode, slope, slopeUnits, aspect, cellData.canopy_cover, coverUnits, cellData.crown_canopy_height, lengthUnits, crownRatio);

                    surfaceFire.doSurfaceRunInDirectionOfMaxSpread();
                    int yIndex = y;
                    if(flipYaxis)
                    {
                        yIndex = yDim - 1 - y;
                    }
                    rateOfSpreads[x, yIndex] = (float)surfaceFire.getSpreadRate(speedUnits);
                    spreadDirections[x, yIndex] = (float)surfaceFire.getDirectionOfMaxSpread();

                }
            }
        }

        private static bool[,] GetWUIArea2D(bool[] wuiArea, int xDim, int yDim)
        {
            bool[,] result = new bool[xDim, yDim];  
            for (int i = 0; i < wuiArea.Length; i++)
            {
                int xIndex = i % xDim;
                int yIndex = i / xDim;
                if (WUIEngine.RUNTIME_DATA.Fire.WuiArea[i] == true)
                {
                    result[xIndex, yIndex] = true;
                }
            }

            return result;
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
                int yFlipped = WUIEngine.SIM.FireModule.GetCellCountY() - 1 - yIndex;
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