//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using WUIPlatform.Fire.Behave;
using System.Threading.Tasks;

namespace WUIPlatform.Fire
{
    public class CellularAutomata2 : FireModule
    {
        public static readonly Vector2int[] neighborIndices = new Vector2int[] { Vector2int.up, new Vector2int(1, 1), Vector2int.right, new Vector2int(1, -1), Vector2int.down, new Vector2int(-1, -1), Vector2int.left, new Vector2int(-1, 1) };
        public static readonly float[] spreadDirections = new float[] { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };
        public static bool inverseSpreadDirection = true;
        public static  readonly float sqrt2 = Mathf.Sqrt(2f);

        private Stack<FireCell2> cellsToIgnite;
        private Dictionary<int, FireCell2> activeCells;
        private Stack<FireCell2> cellsToRemove;
        private int xDim, yDim;
        private FireCell2[,] fireCells;

        private CellularAutomata2(float windspeedTenMeters, float windDirection, float cellSize)
        {
            WUIEngine.LOG(WUIEngine.LogType.Log, "Beginning backwards calculation of fire spread.");

            xDim = WUIEngine.RUNTIME_DATA.Fire.LCPData.GetCellCountX();
            yDim = WUIEngine.RUNTIME_DATA.Fire.LCPData.GetCellCountY();
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
            fireCells = new FireCell2[xDim, yDim];

            //create
            float maxROS = float.MinValue;
            for (int y = 0; y < yDim; ++y)
            {
                for (int x = 0; x < xDim; ++x)
                {
                    fireCells[x, y] = new FireCell2(x, y, surfaceFire, WUIEngine.RUNTIME_DATA.Fire.LCPData, wuiArea, xDim, yDim, windDirection, windspeedTenMeters, cellSize, this);
                    if (fireCells[x, y]._maxROS > maxROS)
                    {
                        maxROS = fireCells[x, y]._maxROS;
                    }
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

            cellsToIgnite = new Stack<FireCell2>();
            activeCells = new Dictionary<int, FireCell2>();
            cellsToRemove = new Stack<FireCell2>();

            //initial ignition
            for (int i = 0; i < wuiIgnitionBorder.Count; ++i)
            {
                for (int j = 0; j < neighborIndices.Length; ++j)
                {
                    Vector2int index = wuiIgnitionBorder[i] + neighborIndices[j];
                    if (IsInside(xDim, yDim, index))
                    {
                        fireCells[index.x, index.y].SchedurelIgnite(0f, 0f, 0);
                    }
                }
            }

            float deltaTime = 0.5f * cellSize / maxROS;
            if (deltaTime <= 0.0f)
            {
                WUIEngine.LOG(WUIEngine.LogType.Log, "Something went wrong when calculating delta time (was less then/equal to zero), please check your input.");
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Log, "Fire spread delta time set to: " + deltaTime);
            }            
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

        public static bool IsInside(int xDim, int yDim, Vector2int index)
        {
            if (index.x < 0 || index.x > xDim - 1 || index.y < 0 || index.y > yDim - 1)
            {
                return false;
            }

            return true;
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

        private static List<Vector2int> GetWUIEdgeCellIndices(bool[,] wuiArea)
        {
            List<Vector2int> borderCells = new List<Vector2int>();

            int xDim = WUIEngine.RUNTIME_DATA.Fire.LCPData.GetCellCountX();
            int yDim = WUIEngine.RUNTIME_DATA.Fire.LCPData.GetCellCountY();

            //CellSpreadRates[,] rateOfSpreads = new CellSpreadRates[xDim, yDim];

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

        public override float GetInternalDeltaTime()
        {
            throw new System.NotImplementedException();
        }

        public override float[,] GetMaxROS()
        {
            throw new System.NotImplementedException();
        }

        public override float[,] GetMaxROSAzimuth()
        {
            throw new System.NotImplementedException();
        }

        public override int GetCellCountX()
        {
            throw new System.NotImplementedException();
        }

        public override int GetCellCountY()
        {
            throw new System.NotImplementedException();
        }

        public override float GetCellSizeX()
        {
            throw new System.NotImplementedException();
        }

        public override float GetCellSizeY()
        {
            throw new System.NotImplementedException();
        }

        public override float[] GetFireLineIntensityData()
        {
            throw new System.NotImplementedException();
        }

        public override float[] GetFuelModelNumberData()
        {
            throw new System.NotImplementedException();
        }

        public override float[] GetSootProduction()
        {
            throw new System.NotImplementedException();
        }

        public override int GetActiveCellCount()
        {
            throw new System.NotImplementedException();
        }

        public override List<Vector2int> GetIgnitedFireCells()
        {
            throw new System.NotImplementedException();
        }

        public override void ConsumeIgnitedFireCells()
        {
            throw new System.NotImplementedException();
        }

        public override FireCellState GetFireCellState(Vector2d latLong)
        {
            throw new System.NotImplementedException();
        }

        public override WindData GetCurrentWindData()
        {
            throw new System.NotImplementedException();
        }

        public void AddCellToIgnite(FireCell2 cell)
        {
            cellsToIgnite.Push(cell);
        }

        public void AddActiveCell(FireCell2 cell)
        {
            activeCells.Add(cell._linearIndex, cell);
        }

        public void AddCellToRemove(FireCell2 cell)
        {
            cellsToRemove.Push(cell);
        }

        public override void Step(float currentTime, float deltaTime)
        {
            //handle ignitions
            while(cellsToIgnite.Count > 0)
            {
                cellsToIgnite.Pop().TryIgnite();
            }
            /*for (int y = 0; y < yDim; ++y)
            {
                for (int x = 0; x < xDim; ++x)
                {
                    fireCells[x, y].TryIgnite();
                }
            }*/

            //step forward in time
            Parallel.ForEach(activeCells.Values, f => f.Step(currentTime, deltaTime));
            currentTime += deltaTime;
            /*for (int y = 0; y < yDim; ++y)
            {
                for (int x = 0; x < xDim; ++x)
                {
                    fireCells[x, y].Step(currentTime, deltaTime);
                    if (!fireCells[x, y]._burntOut)
                    {
                        ++aliveCells;
                    }
                }
            }*/
            while (cellsToRemove.Count > 0)
            {
                activeCells.Remove(cellsToRemove.Pop()._linearIndex);
            }

            if (activeCells.Count == 0 && cellsToIgnite.Count == 0)
            {
                WUIEngine.LOG(WUIEngine.LogType.Log, "No more active cells left, stopping fire spread simulation");
            }
        }

        public override bool IsSimulationDone()
        {
            throw new System.NotImplementedException();
        }

        public override void Stop()
        {
            float[,] triggerBuffer = new float[xDim, yDim];

            //collect time of arrival, make sure to update if any cells were ignited last time step
            for (int y = 0; y < yDim; ++y)
            {
                for (int x = 0; x < xDim; ++x)
                {
                    fireCells[x, y].TryIgnite();
                    if (fireCells[x, y]._ignited)
                    {
                        triggerBuffer[x, y] = fireCells[x, y]._timeOfArrival;
                    }
                }
            }

            WUIEngine.LOG(WUIEngine.LogType.Log, "Finished backwards calculation of fire spread.");
        }
    }

    public class FireCell2
    {
        //private const TwoFuelModelsMethod twoFuelModelsMethod = TwoFuelModelsMethod.NoMethod;
        private const BehaveUnits.MoistureUnits.MoistureUnitsEnum moistureUnits = BehaveUnits.MoistureUnits.MoistureUnitsEnum.Percent;
        private const WindHeightInputMode windHeightInputMode = WindHeightInputMode.TenMeter;
        private const BehaveUnits.SlopeUnits.SlopeUnitsEnum slopeUnits = BehaveUnits.SlopeUnits.SlopeUnitsEnum.Degrees;
        private const BehaveUnits.CoverUnits.CoverUnitsEnum coverUnits = BehaveUnits.CoverUnits.CoverUnitsEnum.Fraction;
        private const BehaveUnits.LengthUnits.LengthUnitsEnum lengthUnits = BehaveUnits.LengthUnits.LengthUnitsEnum.Meters;
        private const BehaveUnits.SpeedUnits.SpeedUnitsEnum windSpeedUnits = BehaveUnits.SpeedUnits.SpeedUnitsEnum.MetersPerSecond;
        private const WindAndSpreadOrientationMode windAndSpreadOrientationMode = WindAndSpreadOrientationMode.RelativeToNorth;        

        public Vector2int _index;
        FireCell2[] _neighbors = new FireCell2[8];
        public bool _burntOut, _ignited;
        public LandscapeStruct _lcp;
        float[] _flamefronts = new float[8];
        float[] _spreadRates = new float[8];
        float[] _spreadDistances = new float[8];
        public float _maxROS;
        private float _cellSize;
        public int _linearIndex;
        private CellularAutomata2 _owner;

        public FireCell2(int xIndex, int yIndex, Surface surface, LCPData lcpData, bool[,] wuiArea, int xDim, int yDim, float windDirection, float midFlameWindspeed, float cellSize, CellularAutomata2 owner)
        {
            _owner = owner;
            _index = new Vector2int(xIndex, yIndex);
            _lcp = lcpData.GetCellData(_index.x, _index.y);
            _cellSize = cellSize;
            _linearIndex = xIndex + yIndex * xDim;
            if (wuiArea[_index.x, _index.y] || surface.isAllFuelLoadZero(_lcp.fuel_model))
            {
                _burntOut = true;
                return;
            }

            _maxROS = float.MinValue;
            for (int i = 0; i < _spreadRates.Length; i++)
            {
                InitialFuelMoisture moisture = WUIEngine.RUNTIME_DATA.Fire.InitialFuelMoistureData.GetInitialFuelMoisture(_lcp.fuel_model);
                double crownRatio = 1.5; //TODO: how to get this data? LCP does not seem to carry it
                int fuelModel = _lcp.fuel_model;
                float slope = _lcp.slope;
                float aspect = _lcp.aspect;

                surface.updateSurfaceInputs(fuelModel, moisture.OneHour, moisture.TenHour, moisture.HundredHour, moisture.LiveHerbaceous, moisture.LiveWoody, moistureUnits,
                    midFlameWindspeed, windSpeedUnits, windHeightInputMode, windDirection, windAndSpreadOrientationMode, slope, slopeUnits, aspect, _lcp.canopy_cover, coverUnits, _lcp.crown_canopy_height, lengthUnits, crownRatio);

                float spreadDirection = CellularAutomata2.spreadDirections[i];
                if (CellularAutomata2.inverseSpreadDirection)
                {
                    spreadDirection += 180f;
                    if (spreadDirection >= 360f)
                    {
                        spreadDirection -= 360f;
                    }
                }
                surface.doSurfaceRunInDirectionOfInterest(spreadDirection);

                //from m/min to m/s
                _spreadRates[i] = (float)surface.getSpreadRateInDirectionOfInterest(windSpeedUnits);
                if (_spreadRates[i] > _maxROS)
                {
                    _maxROS = _spreadRates[i];
                }
            }           
        }

        public void SetNeighbors(int xDim, int yDim, bool[,] wuiArea, Surface surface, FireCell2[,] cells)
        {
            for (int i = 0; i < _neighbors.Length; ++i)
            {
                Vector2int neighborIndex = _index + CellularAutomata2.neighborIndices[i];
                if (CellularAutomata2.IsInside(xDim, yDim, neighborIndex))
                {
                    _neighbors[i] = cells[neighborIndex.x, neighborIndex.y];

                    float heightDifference = Mathf.Abs(_lcp.elevation - _neighbors[i]._lcp.elevation);
                    //heightDifference = 0f;
                    float topViewDistance = _cellSize;
                    //if not cardinal direction we go diagonally
                    if (i % 2 != 0)
                    {
                        topViewDistance *= CellularAutomata2.sqrt2;
                    }
                    _spreadDistances[i] = Mathf.Sqrt(heightDifference * heightDifference + topViewDistance * topViewDistance);

                    if (_neighbors[i]._burntOut)
                    {
                        _neighbors[i] = null;
                    }
                }
            }
        }

        public float GetSpreadrate(int index)
        {
            return _spreadRates[index];
        }

        //returns the flame front in the opposite direction of who is requesting it, e.g. a neighbor north of you will have you indexed as south, so we want the north spread rate
        public float GetFlamefront(int fromRelativeIndex)
        {
            return _flamefronts[(fromRelativeIndex + 4) % 8];
        }

        public void Step(float currentTime, float deltaTime)
        {
            if (_burntOut || !_ignited)
            {
                return;
            }

            int spreadLeft = 0;
            for (int i = 0; i < _flamefronts.Length; ++i)
            {
                if (_flamefronts[i] < 0.5f * _spreadDistances[i])
                {
                    _flamefronts[i] += deltaTime * _spreadRates[i];
                    ++spreadLeft;
                }
                else if (_neighbors[i] != null && !_neighbors[i]._burntOut)
                {
                    _flamefronts[i] += deltaTime * _neighbors[i].GetSpreadrate(i);
                    //flamefronts have crossed or we have reached the center of the neighbor
                    if (_flamefronts[i] + _neighbors[i].GetFlamefront(i) >= _spreadDistances[i])
                    {
                        if (!_neighbors[i]._ignited)
                        {
                            //residual is only from this cell
                            float residual = Mathf.Max(0, _flamefronts[i] - _spreadDistances[i]);
                            _neighbors[i].SchedurelIgnite(currentTime + deltaTime, residual, i);
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
                _owner.AddCellToRemove(this);
            }
        }

        bool scheduleIgnition;
        public float _timeOfArrival;
        public void SchedurelIgnite(float timeOfArrival, float residualFlamefront, int index)
        {
            if (!_burntOut && !_ignited)
            {
                if (!scheduleIgnition)
                {
                    scheduleIgnition = true;
                    _owner.AddCellToIgnite(this);
                    _timeOfArrival = timeOfArrival;
                }
                //we might be approached from more than one direction during one timestep
                _flamefronts[index] = Mathf.Max(_flamefronts[index], residualFlamefront);
            }
        }

        public void TryIgnite()
        {
            if (scheduleIgnition && !_ignited)
            {
                _ignited = true;
                _owner.AddActiveCell(this);
            }
        }
    }
}
