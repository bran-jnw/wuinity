//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using PREACT.Wildfire.Behave;
using System.Threading.Tasks;
using PREACT.Math;

namespace PREACT.Wildfire
{
    public class CellParticleHybrid : WildfireModule
    {
        public static readonly Vector2int[] NeighborIndices = new Vector2int[] { Vector2int.up, new Vector2int(1, 1), Vector2int.right, new Vector2int(1, -1), Vector2int.down, new Vector2int(-1, -1), Vector2int.left, new Vector2int(-1, 1) };
        public static bool inverseSpreadDirection = false;        

        private Queue<FireParticle> _aliveParticles;
        private int _xDim, _yDim;
        private FuelCell[,] _fuelCells;
        private float[] _fireLineIntensityData;
        private float[,] _maxRosData;
        private float[] _timeOfArrivalData;
        private float[] _sootInjection;
        private List<Vector2int> _ignitedCellIndices;
        private bool _done;
        private LandscapeData _landscapeData;
        private BehaveCore.FuelModels _fuelModels;

        public Simulation Simulation { get => _simulation; }

        public CellParticleHybrid(Simulation simulation, LandscapeData landscapeData, bool[] wuiArea, FuelModelInput fuelModelInput, InitialFuelMoistureLibrary initialFuelMoisture, IgnitionPointInput[] ignitionPoints) : base(simulation)
        {
            _landscapeData = landscapeData;
            _originOffset = landscapeData.OriginOffset;
            _xDim = _landscapeData.GetCellCountX();
            _yDim = _landscapeData.GetCellCountY();

            _fireLineIntensityData = new float[_xDim * _yDim];
            _maxRosData = new float[_xDim, _yDim];
            _timeOfArrivalData = new float[_xDim * _yDim];

            bool[,] wuiArea2D = GetWUIArea2D(wuiArea, _xDim, _yDim);

            List<Vector2int> wuiIgnitionBorder = GetWUIEdgeCellIndices(wuiArea2D);

            //need to keep this in memory
            _fuelModels = new BehaveCore.FuelModels();
            /*if (fuelModelInput != null)
            {
                for (int i = 0; i < fuelModelInput.Fuels.Count; i++)
                {
                    fuelModels.setFuelModelRecord(fuelModelInput.Fuels[i]);
                }
            }*/
            _fuelCells = new FuelCell[_xDim, _yDim];
            _sootInjection = new float[_xDim * _yDim];
            _ignitedCellIndices = new List<Vector2int>();

            //create fuel cells
            for (int y = 0; y < _yDim; ++y)
            {
                for (int x = 0; x < _xDim; ++x)
                {
                    _fuelCells[x, y] = new FuelCell(true, x, y, landscapeData, _fuelModels, wuiArea2D, _xDim, _yDim, this, initialFuelMoisture, simulation.Input.WildfireModule.FireCellInput);
                }
            }

            _aliveParticles = new Queue<FireParticle>();            

            _done = false;      

            if(!inverseSpreadDirection)
            {
                for (int i = 0; i < ignitionPoints.Length; ++i)
                {
                    if (ignitionPoints[i].IgnitionTime <= 0)
                    {
                        IgniteAtLatLon(ignitionPoints[i].LatLon, 0f);
                    }
                }
            }
            else
            {
                //initial ignition backwards spread is the wui area border
                for (int i = 0; i < wuiIgnitionBorder.Count; ++i)
                {
                    for (int j = 0; j < NeighborIndices.Length; ++j)
                    {
                        Vector2int index = wuiIgnitionBorder[i] + NeighborIndices[j];
                        if (IsInside(_xDim, _yDim, index))
                        {
                            _fuelCells[index.x, index.y].Ignite(0f, 0f);
                        }
                    }
                }
            }                   
        }

        private void IgniteAtLatLon(Vector2d latLon, float currentTime)
        {
            Vector2d pos = _simulation.GetSimulationPosition(latLon);
            pos += _originOffset;
            int xIndex = (int)(_landscapeData.GetCellCountX() * pos.x / _landscapeData.GetLandscapeSizeX());
            int yIndex = (int)(_landscapeData.GetCellCountY() * pos.y / _landscapeData.GetLandscapeSizeY());

            if(IsInside(xIndex, yIndex))
            {
                _fuelCells[xIndex, yIndex].Ignite(currentTime, 0f);
            }
        }

        public FuelCell[,] GetCells()
        {
            return _fuelCells;
        }

        public FuelCell GetCell(Vector3d localPos)
        {
            int xIndex = (int)(_landscapeData.GetCellCountX() * localPos.x / _landscapeData.GetLandscapeSizeX());
            int yIndex = (int)(_landscapeData.GetCellCountY() * localPos.y / _landscapeData.GetLandscapeSizeY());
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

        public void UpdateCellData(Vector2int index, int linearIndex, float firelineIntensity, float rateOfSpread)
        {
            _fireLineIntensityData[linearIndex] = firelineIntensity;
            _maxRosData[index.x, index.y] = Mathf.Max(rateOfSpread, _maxRosData[index.x, index.y]);            
        }

        public void SetTimeOfArrival(int linearIndex, float timeOfArrival)
        {
            _timeOfArrivalData[linearIndex] = Mathf.Min(timeOfArrival, _timeOfArrivalData[linearIndex]);
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

        public bool IsInside(double xPos, double yPos)
        {
            if (xPos < 0 || xPos > _landscapeData.GetLandscapeSizeX()  || yPos < 0 || yPos > _landscapeData.GetLandscapeSizeY())
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
            return _internalDeltaTime;
        }

        public override float[,] GetMaxROS()
        {
            return _maxRosData;
        }

        public override float[,] GetMaxROSAzimuth()
        {
            throw new System.NotImplementedException();
        }

        public override int GetCellCountX()
        {
            return _xDim;
        }

        public override int GetCellCountY()
        {
            return _yDim;
        }

        public override float GetCellSizeX()
        {
            return (float)_landscapeData.RasterCellResolutionX;
        }

        public override float GetCellSizeY()
        {
            return (float)_landscapeData.RasterCellResolutionY;
        }

        public override float[] GetFireLineIntensityData()
        {
            return _fireLineIntensityData;
        }

        public override float[] GetFuelModelNumberData()
        {
            throw new System.NotImplementedException();
        }

        public override float[] GetSootProduction()
        {
            return _sootInjection;
        }

        public override int GetActiveCellCount()
        {
            return -1;
        }

        public override List<Vector2int> GetIgnitedFireCells()
        {
            return _ignitedCellIndices;
        }

        public override void ConsumeIgnitedFireCells()
        {
            for(int i = 0; i < _ignitedCellIndices.Count; ++i)
            {
                Vector2int index = _ignitedCellIndices[i];
                //_sootInjection[index.x + index.y * _xDim] = 0;
            }
            _ignitedCellIndices.Clear();
            
        }

        public override FireCellState GetFireCellState(Vector2d latLong)
        {
            return FireCellState.Dead;
        }

        public void AddActiveFireParticle(FireParticle particle)
        {
            _aliveParticles.Enqueue(particle);
        }

        public override void Step(float currentTime, float deltaTime)
        {
            if(_done)
            {
                return;
            }

            _internalDeltaTime = deltaTime;

            //step forward in time
            Queue<FireParticle> stillAliveParticles = new Queue<FireParticle>(_aliveParticles.Count);//a reasonable guess it that particles die and gets created about the same rate?
            while (_aliveParticles.Count > 0)
            {
                FireParticle f = _aliveParticles.Dequeue();
                f.Step(currentTime, deltaTime, this);
                if(!f.Dead)
                {
                    stillAliveParticles.Enqueue(f);
                }
            }
            _aliveParticles = stillAliveParticles;

            if (_aliveParticles.Count == 0)
            {
                _done = true;
                Engine.Message(null, Engine.LogType.Log, "No more active fire particles left, stopping fire spread simulation after " + (currentTime + deltaTime) + " seconds.");
            }
        }

        public void AddIgnitedCellIndex(Vector2int cellIndex)
        {
            _ignitedCellIndices.Add(cellIndex);
            _sootInjection[cellIndex.x + cellIndex.y * _xDim] = 50;
        }

        public override bool IsSimulationDone()
        {
            return _done;
        }

        public override void Stop()
        {
            /*float[,] triggerBuffer = new float[_xDim, _yDim];

            //collect time of arrival, make sure to update if any cells were ignited last time step
            for (int y = 0; y < _yDim; ++y)
            {
                for (int x = 0; x < _xDim; ++x)
                {
                    _fuelCells[x, y].Ignite(_xDim, _yDim, _fuelCells, 0);
                    if (_fuelCells[x, y]._ignited)
                    {
                        triggerBuffer[x, y] = _fuelCells[x, y]._timeOfArrival;
                    }
                }
            }

            Engine.Message(null, Engine.LogType.Log, "Finished backwards calculation of fire spread.");*/
        }

        public override void GetOffsetAndSize(out Vector2d offset, out Vector2d size)
        {
            offset = _originOffset;
            size = new Vector2d(_landscapeData.GetLandscapeSizeX(), _landscapeData.GetLandscapeSizeY());
        }
    }    
}
