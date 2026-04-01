//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using PREACT.Math;
using System.IO;
using System;

namespace PREACT.Wildfire
{
    public class CellParticleHybrid : WildfireModule
    {
        public static readonly Vector2int[] NeighborIndices = new Vector2int[] { Vector2int.up, new Vector2int(1, 1), Vector2int.right, new Vector2int(1, -1), Vector2int.down, new Vector2int(-1, -1), Vector2int.left, new Vector2int(-1, 1) };
        public static bool inverseSpreadDirection = false;        

        private Queue<FireParticle> _aliveParticles;
        private int _xDim, _yDim;
        private FuelCell[,] _fuelCells;
        private float[] _maxFireIntensityData;
        private float[,] _maxRosData;
        private float[,] _maxRosDirectionData;
        private float[] _timeOfArrivalData;
        private float[] _sootInjection;
        private List<Vector2int> _ignitedCellIndices;
        private bool _done;
        private LandscapeData _landscapeData;
        private BehaveCore.FuelModels _fuelModels;
        private List<IgnitionPoint> _ignitionPoints;
        private double _initialIgnition = -1;

        public Simulation Simulation { get => _simulation; }

        public CellParticleHybrid(Simulation simulation, LandscapeData landscapeData, bool[] wuiArea, FuelModelInput fuelModelInput, InitialFuelMoistureLibrary initialFuelMoisture, List<IgnitionPointInput> ignitionPoints) : base(simulation)
        {
            _landscapeData = landscapeData;
            _originOffset = landscapeData.OriginOffset;
            _xDim = _landscapeData.GetCellCountX();
            _yDim = _landscapeData.GetCellCountY();

            _maxFireIntensityData = new float[_xDim * _yDim];
            _maxRosData = new float[_xDim, _yDim];
            _maxRosDirectionData = new float[_xDim, _yDim];
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
                    _fuelCells[x, y] = new FuelCell(simulation.Input.WildfireModule.FireCellInput.CentroidMode, x, y, landscapeData, _fuelModels, wuiArea2D, _xDim, _yDim, this, initialFuelMoisture, simulation.Input.WildfireModule.FireCellInput);
                }
            }
            Engine.Message(_simulation, Engine.LogType.Debug, $"Size of raster is {landscapeData.RasterCellResolutionX}.");

            _aliveParticles = new Queue<FireParticle>(_xDim * _yDim * 8 / 10); //just a guess, 10 percent active at max           

            _done = false;      

            if(!inverseSpreadDirection)
            {
                _ignitionPoints = new List<IgnitionPoint>(ignitionPoints.Count);
                for (int i = 0; i < ignitionPoints.Count; ++i)
                {
                    _ignitionPoints.Add(new IgnitionPoint(_simulation, ignitionPoints[i]));
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

        private void IgniteAtLatLon(Vector2d latLon, double currentTime)
        {
            Vector2d pos = _simulation.Spatial.GetSimulationPosition(latLon);
            pos -= _originOffset;
            int xIndex = (int)(_landscapeData.GetCellCountX() * pos.x / _landscapeData.GetLandscapeSizeX());
            int yIndex = (int)(_landscapeData.GetCellCountY() * pos.y / _landscapeData.GetLandscapeSizeY());

            if(IsInside(xIndex, yIndex))
            {
                _fuelCells[xIndex, yIndex].Ignite((float)currentTime, 0f);
                Engine.Message(_simulation, Engine.LogType.Log, $"Ignition happened at lat/lon [{latLon.x}/{latLon.y}] as requested by user.");
            }
            else
            {
                Engine.Message(_simulation, Engine.LogType.Log, $"Tried to ignite at lat/lon [{latLon.x}/{latLon.y}] but this is outside of the provided landscape.");
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

        public void UpdateCellData(Vector2int index, int linearIndex, float firelineIntensity, float rateOfSpread, float rateOfSpreadDirection)
        {
            _maxFireIntensityData[linearIndex] = Mathf.Max(firelineIntensity, _maxFireIntensityData[linearIndex]);
            if (rateOfSpread > _maxRosData[index.x, index.y])
            {
                _maxRosData[index.x, index.y] = rateOfSpread;
                _maxRosDirectionData[index.x, index.y] = rateOfSpreadDirection;
            }        
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

        public override double GetInternalDeltaTime()
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
            return _maxFireIntensityData;
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

        public override void Step(double simulationTime, double deltaTime)
        {
            if(_done)
            {
                return;
            }
            _internalDeltaTime = deltaTime;

            //check ignitions
            if (!inverseSpreadDirection)
            {
                for (int i = 0; i < _ignitionPoints.Count; ++i)
                {
                    if (_ignitionPoints[i].IgnitionTime <= simulationTime)
                    {
                        IgniteAtLatLon(_ignitionPoints[i].LatLon, simulationTime);
                        _ignitionPoints.Remove(_ignitionPoints[i]);

                        if(_initialIgnition < 0)
                        {
                            _initialIgnition = simulationTime;
                        }
                    }
                }
            }

            //step forward in time
            Queue<FireParticle> stillAliveParticles = new Queue<FireParticle>(_aliveParticles.Count);//a reasonable guess it that particles die and gets created about the same rate?
            while (_aliveParticles.Count > 0)
            {
                FireParticle f = _aliveParticles.Dequeue();
                f.Step((float)simulationTime, (float)deltaTime, this);
                if(!f.Dead)
                {
                    stillAliveParticles.Enqueue(f);
                }
            }
            _aliveParticles = stillAliveParticles;

            if (_aliveParticles.Count == 0 && _ignitionPoints.Count == 0)
            {
                _done = true;
                Engine.Message(null, Engine.LogType.Log, "No more active fire particles left, stopping fire spread simulation after " + (simulationTime + deltaTime) + " seconds.");
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
            string filePath = Path.Combine(_simulation.Engine.OutputFolder, $"{_simulation.Input.Simulation.Name}_wildfire_{_simulation.SimulationIndex}.tif");
            SaveOutputMaps(filePath);
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

        private void SaveOutputMaps(string filePath)
        {
            //usage map as geotiff
            try
            {
                using (OSGeo.GDAL.Driver driver = OSGeo.GDAL.Gdal.GetDriverByName("GTiff"))
                {
                    OSGeo.GDAL.Dataset output = driver.Create(filePath, _xDim, _yDim, 4, OSGeo.GDAL.DataType.GDT_Float32, null);

                    double leftX = _simulation.Spatial.UTMOrigin.x + _originOffset.x;
                    double lowerLeftY = _simulation.Spatial.UTMOrigin.y + _originOffset.y;
                    double[] geoTransform = new double[] { leftX, GetCellSizeX(), 0.0, lowerLeftY, 0.0, GetCellSizeY() };
                    output.SetGeoTransform(geoTransform);

                    OSGeo.OSR.SpatialReference reference = new OSGeo.OSR.SpatialReference("");
                    reference.SetProjCS("UTM " + _simulation.Spatial.UTMData.Zona + " (WGS84)");
                    reference.SetWellKnownGeogCS("WGS84");
                    reference.SetUTM(_simulation.Spatial.UTMData.ZoneNumber, _simulation.Input.Simulation.LowerLeftLatLon.x > 0 ? 1 : 0); ;
                    output.SetSpatialRef(reference);

                    //time of arrival
                    OSGeo.GDAL.Band band = output.GetRasterBand(1); //starts from 1, not zero                
                    band.SetNoDataValue(-9999f);
                    band.SetDescription($"Time of arrival [hours] from initial ignition.");
                    float[] row = new float[_xDim];
                    for (int y = 0; y < _yDim; ++y)
                    {
                        for (int x = 0; x < _xDim; ++x)
                        {
                            row[x] = (_fuelCells[x, y].TimeOfArrival - (float)_initialIgnition) / 3600; //hours
                            if (row[x] <= 0f)
                            {
                                row[x] = -9999f;
                            }
                        }
                        band.WriteRaster(0, y, _xDim, 1, row, _xDim, 1, 0, 0);
                    }
                    band.FlushCache();

                    // spread rate
                    band = output.GetRasterBand(2);
                    band.SetNoDataValue(-9999f);
                    band.SetDescription("Max rate of spread [m/s].");
                    for (int y = 0; y < _yDim; ++y)
                    {
                        for (int x = 0; x < _xDim; ++x)
                        {
                            row[x] = _maxRosData[x, y]; 
                            if (row[x] <= 0f)
                            {
                                row[x] = -9999f;
                            }
                        }
                        band.WriteRaster(0, y, _xDim, 1, row, _xDim, 1, 0, 0);
                    }
                    band.FlushCache();

                    // max spread rate direction
                    band = output.GetRasterBand(3);
                    band.SetNoDataValue(-9999f);
                    band.SetDescription("Max rate of spread direction [degree azimuth, counter clock-wise from North].)");
                    for (int y = 0; y < _yDim; ++y)
                    {
                        for (int x = 0; x < _xDim; ++x)
                        {
                            row[x] = _maxRosDirectionData[x, y];
                            if (row[x] <= 0f)
                            {
                                row[x] = -9999f;
                            }
                        }
                        band.WriteRaster(0, y, _xDim, 1, row, _xDim, 1, 0, 0);
                    }
                    band.FlushCache();

                    // fire intenisty
                    band = output.GetRasterBand(4);
                    band.SetNoDataValue(-9999f);
                    band.SetDescription("Fire intensity [kW/m].)");
                    for (int y = 0; y < _yDim; ++y)
                    {
                        for (int x = 0; x < _xDim; ++x)
                        {
                            row[x] = _maxFireIntensityData[_fuelCells[x, y].LinearIndex];
                            if (row[x] <= 0f)
                            {
                                row[x] = -9999f;
                            }
                        }
                        band.WriteRaster(0, y, _xDim, 1, row, _xDim, 1, 0, 0);
                    }
                    band.FlushCache();

                    //close
                    output.FlushCache();
                    //reminder, output.Close() crashes violently, do not use or investigate further why...
                }

            }
            catch (Exception e)
            {
                Engine.Message(null, Engine.LogType.Warning, e.Message);
            }
        }

        public override Vector2int SimulationPosToCellIndex(Vector2d simulationPos, out bool inside)
        {
            throw new NotImplementedException();
        }

        public override bool Ignited()
        {
            throw new NotImplementedException();
        }
    }    
}
