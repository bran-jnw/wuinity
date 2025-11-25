//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using PREACT.Fire.Behave;
using System.Threading.Tasks;
using PREACT.Math;

namespace PREACT.Fire
{
    public class CellVertexHybrid : FireModule
    {
        public static readonly Vector2int[] NeighborIndices = new Vector2int[] { Vector2int.up, new Vector2int(1, 1), Vector2int.right, new Vector2int(1, -1), Vector2int.down, new Vector2int(-1, -1), Vector2int.left, new Vector2int(-1, 1) };
        public static bool inverseSpreadDirection = false;

        public readonly TwoFuelModelsMethod TwoFuelModelsMethod = TwoFuelModelsMethod.NoMethod;
        public readonly BehaveUnits.MoistureUnits.MoistureUnitsEnum MoistureUnits = BehaveUnits.MoistureUnits.MoistureUnitsEnum.Percent;
        public readonly WindHeightInputMode WindHeightInputMode = WindHeightInputMode.TenMeter;
        public readonly BehaveUnits.SlopeUnits.SlopeUnitsEnum SlopeUnits = BehaveUnits.SlopeUnits.SlopeUnitsEnum.Degrees;
        public readonly BehaveUnits.CoverUnits.CoverUnitsEnum CoverUnits = BehaveUnits.CoverUnits.CoverUnitsEnum.Fraction;
        public readonly BehaveUnits.LengthUnits.LengthUnitsEnum LengthUnits = BehaveUnits.LengthUnits.LengthUnitsEnum.Meters;
        public readonly BehaveUnits.SpeedUnits.SpeedUnitsEnum WindSpeedUnits = BehaveUnits.SpeedUnits.SpeedUnitsEnum.MetersPerSecond;
        public readonly WindAndSpreadOrientationMode WindAndSpreadOrientationMode = WindAndSpreadOrientationMode.RelativeToNorth;

        private Stack<FuelCell> _cellsToIgnite;
        private Dictionary<int, FuelCell> _activeCells;
        private Stack<FuelCell> _cellsToRemove;
        private int xDim, yDim;
        private FuelCell[,] _fuelCells;

        public CellVertexHybrid(Simulation simulation) : base(simulation)
        {
            xDim = simulation.Input.Fire.Data.LCPData.GetCellCountX();
            yDim = simulation.Input.Fire.Data.LCPData.GetCellCountY();
            bool[,] wuiArea = GetWUIArea2D(simulation.Input.Fire.Data.WuiArea, xDim, yDim);

            List<Vector2int> wuiIgnitionBorder = GetWUIEdgeCellIndices(wuiArea);

            FuelModelSet fuelModelSet = new FuelModelSet();
            if (simulation.Input.Fire.Data.FuelModelsData != null)
            {
                for (int i = 0; i < simulation.Input.Fire.Data.FuelModelsData.Fuels.Count; i++)
                {
                    fuelModelSet.setFuelModelRecord(simulation.Input.Fire.Data.FuelModelsData.Fuels[i]);
                }
            }
            _fuelCells = new FuelCell[xDim, yDim];

            //create fuel cells
            float maxROS = float.MinValue;
            for (int y = 0; y < yDim; ++y)
            {
                for (int x = 0; x < xDim; ++x)
                {
                    _fuelCells[x, y] = new FuelCell(true, x, y, simulation.Input.Fire.Data.LCPData, fuelModelSet, wuiArea, xDim, yDim, this, simulation.Input.Fire.Data.InitialFuelMoistureData);
                    if (_fuelCells[x, y]._maxROS > maxROS)
                    {
                        maxROS = _fuelCells[x, y]._maxROS;
                    }
                }
            }

            _cellsToIgnite = new Stack<FuelCell>();
            _activeCells = new Dictionary<int, FuelCell>();
            _cellsToRemove = new Stack<FuelCell>();

            //initial ignition
            for (int i = 0; i < wuiIgnitionBorder.Count; ++i)
            {
                for (int j = 0; j < NeighborIndices.Length; ++j)
                {
                    Vector2int index = wuiIgnitionBorder[i] + NeighborIndices[j];
                    if (IsInside(xDim, yDim, index))
                    {
                        _fuelCells[index.x, index.y].SchedulelIgnition(0f, 0f);
                    }
                }
            }

            float deltaTime = 0.5f * (float)simulation.Input.Fire.Data.LCPData.RasterCellResolutionX / maxROS;
            if (deltaTime <= 0.0f)
            {
                Engine.Message(null, Engine.LogType.Log, "Something went wrong when calculating delta time (was less then/equal to zero), please check your input.");
            }
            else
            {
                Engine.Message(null, Engine.LogType.Log, "Initial fire spread delta time set to: " + deltaTime);
            }            
        }

        public FuelCell GetCell(Vector3d localPos)
        {
            int xIndex = (int)(localPos.x / _simulation.Input.Fire.Data.LCPData.GetLandscapeSizeX());
            int yIndex = (int)(localPos.y / _simulation.Input.Fire.Data.LCPData.GetLandscapeSizeY());
            return _fuelCells[xIndex, yIndex];
        }

        private static bool HasNonWUINeighbors(bool[,] wuiArea, int xDim, int yDim, Vector2int cellIndex)
        {
            bool hasNonWUINeighbors = false;

            for (int i = 0; i < NeighborIndices.Length; ++i)
            {
                Vector2int neighborIndex = cellIndex + NeighborIndices[i];
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
                if (wuiArea[i] == true)
                {
                    result[xIndex, yIndex] = true;
                }
            }

            return result;
        }

        private static List<Vector2int> GetWUIEdgeCellIndices(bool[,] wuiArea)
        {
            List<Vector2int> borderCells = new List<Vector2int>();

            int xDim = wuiArea.GetLength(0);
            int yDim = wuiArea.GetLength(1);

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

        public void AddCellToIgnite(FuelCell cell)
        {
            _cellsToIgnite.Push(cell);
        }

        public void AddActiveCell(FuelCell cell)
        {
            _activeCells.Add(cell._linearIndex, cell);
        }

        public void AddCellToRemove(FuelCell cell)
        {
            _cellsToRemove.Push(cell);
        }

        public override void Step(float currentTime, float deltaTime)
        {
            //handle ignitions
            while(_cellsToIgnite.Count > 0)
            {
                _cellsToIgnite.Pop().TryIgnite(xDim, yDim, _fuelCells);
            }
            /*for (int y = 0; y < yDim; ++y)
            {
                for (int x = 0; x < xDim; ++x)
                {
                    fireCells[x, y].TryIgnite();
                }
            }*/

            Parallel.ForEach(_activeCells.Values, f => f.UpdateRateOfSpread());

            //step forward in time
            Parallel.ForEach(_activeCells.Values, f => f.Step(currentTime, deltaTime));
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
            while (_cellsToRemove.Count > 0)
            {
                _activeCells.Remove(_cellsToRemove.Pop()._linearIndex);
            }

            if (_activeCells.Count == 0 && _cellsToIgnite.Count == 0)
            {
                Engine.Message(null, Engine.LogType.Log, "No more active cells left, stopping fire spread simulation");
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
                    _fuelCells[x, y].TryIgnite(xDim, yDim, _fuelCells);
                    if (_fuelCells[x, y]._ignited)
                    {
                        triggerBuffer[x, y] = _fuelCells[x, y]._timeOfArrival;
                    }
                }
            }

            Engine.Message(null, Engine.LogType.Log, "Finished backwards calculation of fire spread.");
        }
    }

    public class FireVertex
    {
        private FuelCell _targetCell;
        public Vector3d _localPosition;
        private Vector3d _spreadVector;
        private double _spreadDirection;
        public bool Dead;
        private double _distanceLeftToTarget;

        public FireVertex(FuelCell targetCell, Vector3d localPosition, Vector3d spreadVector, double spreadDirection)
        {
            _targetCell = targetCell;
            _localPosition = localPosition;                    

            if (CellVertexHybrid.inverseSpreadDirection)
            {
                spreadVector *= -1;
                spreadDirection += 180f;
                if (spreadDirection >= 360f)
                {
                    spreadDirection -= 360f;
                }
            }

            _spreadVector = spreadVector;
            _spreadDirection = spreadDirection;
            Dead = false;
            _distanceLeftToTarget = Vector3d.Distance(_localPosition, _targetCell.IgnitionPoint);
        }

        public void Step(float currentTime, float deltaTime, CellVertexHybrid sim)
        {
            if (!_targetCell._dead && !_targetCell._ignited)
            {                
                FuelCell currentCell = sim.GetCell(_localPosition);
                float spreadRate = currentCell.GetSpreadRateInDirection(_spreadDirection);

                double delta = deltaTime * spreadRate;
                _localPosition += delta * _spreadVector ;
                _distanceLeftToTarget -= delta;

                //we have reached the vertex of the neighbor
                if (_distanceLeftToTarget <= 0)
                {
                    if (!_targetCell._ignited)
                    {
                        float residualTime = (float)-_distanceLeftToTarget / spreadRate;
                        _targetCell.SchedulelIgnition(currentTime + deltaTime, residualTime);
                    }
                }
            }
            //nothing for us to do
            else
            {
                Dead = true;
            }
        }
    }

    public class FuelCell
    {       
        public Vector2int _index;
        public bool _dead, _ignited;
        public LandscapeCellData _cellData;
        public float _maxROS;
        private double _cellSize;
        public int _linearIndex;
        private CellVertexHybrid _owner;
        private float _insideDistance;//distance within cell before crossing over to neighbor and changes fire behavior (fuel)
        private bool _rateOfSpreadIsSet = false;

        Surface _surface;
        Crown _crownFire;

        public Vector3d IgnitionPoint;
        private List<FireVertex> _fireVertices;
        private float _residualTime;
        private InitialFuelMoisture _moisture;

        public FuelCell(bool randomCenter, int xIndex, int yIndex, LandscapeData landscape, FuelModelSet fuelModelSet, bool[,] wuiArea, int xDim, int yDim, CellVertexHybrid owner, InitialFuelMoistureLibrary initialFuelMoistures)
        {
            _owner = owner;
            _index = new Vector2int(xIndex, yIndex);
            _cellData = landscape.GetCellData(_index.x, _index.y);
            _cellSize = landscape.RasterCellResolutionX;
            _moisture = initialFuelMoistures.GetInitialFuelMoisture(_cellData.fuel_model);
            _linearIndex = xIndex + yIndex * xDim;
            _surface = new Surface(fuelModelSet);
            _crownFire = new Crown(fuelModelSet);


            if(randomCenter)
            {
                double xPos = Random.valued * _cellSize + xIndex * _cellSize;
                double yPos = Random.valued * _cellSize + yIndex * _cellSize;
                double zPos = landscape.GetElevationLocalPos(xPos, yPos);
                IgnitionPoint = new Vector3d(xPos, zPos, yPos);
            }
            else
            {
                double xPos = (xIndex + 0.5) * _cellSize;
                double yPos = (yIndex + 0.5) * _cellSize;
                double zPos = _cellData.elevation;
                IgnitionPoint = new Vector3d(xPos, zPos, yPos);
            }


            if (wuiArea[_index.x, _index.y] || _surface.isAllFuelLoadZero(_cellData.fuel_model))
            {
                _dead = true;
                return;
            }

            _maxROS = float.MinValue;
        }

        /// <summary>
        /// Needed after creation of all cells.
        /// </summary>
        /// <param name="xDim"></param>
        /// <param name="yDim"></param>
        /// <param name="wuiArea"></param>
        /// <param name="surface"></param>
        /// <param name="cells"></param>
        public void SpawnFireVertices(int xDim, int yDim, FuelCell[,] cells)
        {
            _fireVertices = new List<FireVertex>(8);
            for (int i = 0; i < CellVertexHybrid.NeighborIndices.Length; ++i)
            {
                Vector2int neighborIndex = _index + CellVertexHybrid.NeighborIndices[i];
                if (CellVertexHybrid.IsInside(xDim, yDim, neighborIndex))
                {
                    FuelCell neighbor = cells[neighborIndex.x, neighborIndex.y];
                    if (!neighbor._dead)
                    {
                        Vector3d delta = neighbor.IgnitionPoint - IgnitionPoint;
                        double spreadDirection = (float)Vector3d.Angle(Vector3d.up, delta);
                        FireVertex f = new FireVertex(neighbor, IgnitionPoint, delta.normalized, spreadDirection);
                        _fireVertices.Add(f);
                    }
                }
            }
        }

        public void UpdateRateOfSpread()
        {
            //InitialFuelMoisture moisture = initialFuelMoistures.GetInitialFuelMoisture(_cellData.fuel_model);
            double crownRatio = 1.5; //TODO: how to get this data? LCP does not seem to carry it
            double midFlameWindspeed = 0;
            double windDirection = 0;

            if(!_rateOfSpreadIsSet)
            {
                _surface.updateSurfaceInputs(_cellData.fuel_model, _moisture.OneHour, _moisture.TenHour, _moisture.HundredHour, _moisture.LiveHerbaceous, _moisture.LiveWoody, _owner.MoistureUnits,
                    midFlameWindspeed, _owner.WindSpeedUnits, _owner.WindHeightInputMode, windDirection, _owner.WindAndSpreadOrientationMode, _cellData.slope, _owner.SlopeUnits, _cellData.aspect, _cellData.canopy_cover, _owner.CoverUnits, _cellData.crown_canopy_height, _owner.LengthUnits, crownRatio);
                _rateOfSpreadIsSet = true; ;
            }
            else
            {
                //this should basically be all that is updated, maybe moisture
                _surface.setWindDirection(windDirection);
                _surface.setWindSpeed(midFlameWindspeed, _owner.WindSpeedUnits, _owner.WindHeightInputMode);
            }

            _surface.doSurfaceRunInDirectionOfMaxSpread();
        }

        public float GetSpreadRateInDirection(double spreadDirection)
        {
            if(!_rateOfSpreadIsSet)
            {
                UpdateRateOfSpread();
            }
            //TODO: check unit and convert to m/s
            return (float)_surface.calculateSpreadRateAtVector(spreadDirection);
        }

        public void Step(float currentTime, float deltaTime)
        {
            if (_dead || !_ignited)
            {
                return;
            }

            //we might have a residual here from ignition, so add that time to the spread
            deltaTime += _residualTime;
            _residualTime = 0;
            int aliveFlameFronts = 0;
            for (int i = 0; i < _fireVertices.Count; ++i)
            {                
                _fireVertices[i].Step(currentTime, deltaTime, _owner);
                if(!_fireVertices[i].Dead)
                {
                    aliveFlameFronts++;
                }
            }

            //we have nowhere left to spread, so we are done
            if (aliveFlameFronts == 0)
            {
                _dead = true;
                _owner.AddCellToRemove(this);
            }
        }

        bool scheduleIgnition;
        public float _timeOfArrival;
        public void SchedulelIgnition(float timeOfArrival, float residualTime)
        {
            if (!_dead && !_ignited)
            {
                if (!scheduleIgnition)
                {
                    scheduleIgnition = true;
                    _owner.AddCellToIgnite(this);
                    _timeOfArrival = timeOfArrival - residualTime;
                }
                //we might be approached from more than one direction during one timestep, so take max residual time
                _residualTime = Mathf.Max(_residualTime, residualTime);
            }
        }

        public void TryIgnite(int xDim, int yDim, FuelCell[,] cells)
        {
            if (scheduleIgnition && !_ignited)
            {
                _ignited = true;
                SpawnFireVertices(xDim, yDim, cells);
                _owner.AddActiveCell(this);
            }
        }
    }
}
