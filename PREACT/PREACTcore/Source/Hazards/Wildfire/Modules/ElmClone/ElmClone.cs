using PREACT.Math;
using System;
using System.Collections.Generic;
using System.Text;

namespace PREACT.Wildfire
{    
    public class ElmClone : WildfireModule
    {
        ElmCloneSolver _solver;        

        private List<IgnitionPoint> _ignitionPoints;
        private double _initialIgnition = -1;
        private WeatherManager _weather;
        private TimeManager _time;

        private float[] _maxFireIntensityData;
        private float[,] _maxRosData;
        private float[,] _maxRosDirectionData;
        private float[] _timeOfArrivalData;
        private float[] _sootInjection;
        Vector2d _landscapeSize;
        double _internalDeltaTime;
        private List<Vector2int> _ignitedCellIndices;                 
        public SpreadModel[,] Spread { get; }
     

        public ElmClone(Simulation simulation, LandscapeData landscape, List<IgnitionPointInput> ignitionPoints, WeatherManager weather, TimeManager time) : base(simulation)
        {
            _weather = weather;
            _time = time;
            _originOffset = landscape.OriginOffset;
            _landscapeSize = new Vector2d(landscape.GetLandscapeSizeX(), landscape.GetLandscapeSizeY());
            int xDim = simulation.Input.WildfireModule.Data.LandscapeData.GetCellCountX();
            int yDim = simulation.Input.WildfireModule.Data.LandscapeData.GetCellCountY();
            double Dx = simulation.Input.WildfireModule.Data.LandscapeData.RasterCellResolutionX;
            double Dy = simulation.Input.WildfireModule.Data.LandscapeData.RasterCellResolutionY;            

            Spread = new SpreadModel[xDim, yDim];
            for (int y = 0; y < yDim; y++) 
            {
                for (int x = 0; x < xDim; x++)
                {
                    if(simulation.Input.WildfireModule.FireCellInput.SpreadRateModel == Input.FireCellInput.SpreadRateModels.LookupROS)
                    {
                        Spread[x, y] = new SpreadModelLookupROS(landscape.GetCellData(x, y), simulation.Input.WildfireModule.Data.LookupROSTable);
                    }
                    else if (simulation.Input.WildfireModule.FireCellInput.SpreadRateModel == Input.FireCellInput.SpreadRateModels.Behave)
                    {

                    }
                    else if (simulation.Input.WildfireModule.FireCellInput.SpreadRateModel == Input.FireCellInput.SpreadRateModels.CanadianFBP)
                    {

                    }
                }
            }           
           
            _solver = new ElmCloneSolver(this, xDim, yDim, Dx, Dy, 16, 0.5);            

            _maxFireIntensityData = new float[xDim * yDim];
            _maxRosData = new float[xDim, yDim];
            _maxRosDirectionData = new float[xDim, yDim];
            _timeOfArrivalData = new float[xDim * yDim];
            _ignitedCellIndices = new List<Vector2int>(256);


            _ignitionPoints = new List<IgnitionPoint>(ignitionPoints.Count);
            for (int i = 0; i < ignitionPoints.Count; ++i)
            {
                _ignitionPoints.Add(new IgnitionPoint(_simulation, ignitionPoints[i]));
            }

            for (int i = 0; i < _ignitionPoints.Count; ++i)
            {
                if (_ignitionPoints[i].IgnitionTime <= 0.0)
                {
                    IgniteAtLatLon(_ignitionPoints[i].LatLon, 0.0);
                    _ignitionPoints.Remove(_ignitionPoints[i]);

                    if (_initialIgnition < 0)
                    {
                        _initialIgnition = 0.0;
                    }
                }
            }
        }

        public override void Step(double simulationTime, double deltaTime)
        {
            /*for (int i = 0; i < _ignitionPoints.Count; ++i)
            {
                if (_ignitionPoints[i].IgnitionTime <= simulationTime)
                {
                    IgniteAtLatLon(_ignitionPoints[i].LatLon, simulationTime);
                    _ignitionPoints.Remove(_ignitionPoints[i]);

                    if (_initialIgnition < 0)
                    {
                        _initialIgnition = simulationTime;
                    }
                }
            }*/

            _solver.Step(deltaTime, _weather, _time, out _internalDeltaTime);            
        }

        public void UpdateCellData(int xIndex, int yIndex, float firelineIntensity, float rateOfSpread, float rateOfSpreadDirection)
        {
            int linIndex = xIndex + yIndex * _solver.Nx;
            _maxFireIntensityData[linIndex] = Mathf.Max(firelineIntensity, _maxFireIntensityData[linIndex]);
            if (rateOfSpread > _maxRosData[xIndex, yIndex])
            {
                _maxRosData[xIndex, yIndex] = rateOfSpread;
                _maxRosDirectionData[xIndex, yIndex] = rateOfSpreadDirection;
            }
        }

        public void SetTimeOfArrival(int xIndex, int yIndex, float timeOfArrival)
        {
            _timeOfArrivalData[xIndex + yIndex * _solver.Nx] = timeOfArrival;
            _ignitedCellIndices.Add(new Vector2int(xIndex, yIndex));
        }

        private void IgniteAtLatLon(Vector2d latLon, double currentTime)
        {
            Vector2d pos = _simulation.Spatial.GetSimulationPosition(latLon);
            pos -= _originOffset;
            int xIndex = (int)(_solver.Nx * pos.x / _landscapeSize.x);
            int yIndex = (int)(_solver.Ny * pos.y / _landscapeSize.y);

            if (IsInside(xIndex, yIndex))
            {
                _solver.Burn(xIndex, yIndex, true);
                Engine.Message(_simulation, Engine.LogType.Log, $"Ignition happened at lat/lon {latLon.x}/{latLon.y} (x/y: {pos.x}, {pos.y}) as requested by user.");
            }
            else
            {
                Engine.Message(_simulation, Engine.LogType.Log, $"Tried to ignite at lat/lon [{latLon.x}/{latLon.y}] but this is outside of the provided landscape.");
            }
        }        

        public bool IsInside(double xPos, double yPos)
        {
            if (xPos < 0 || xPos > _landscapeSize.x || yPos < 0 || yPos > _landscapeSize.y)
            {
                return false;
            }

            return true;
        }

        public override void ConsumeIgnitedFireCells()
        {
            _ignitedCellIndices.Clear();
        }

        public override int GetActiveCellCount()
        {
            return _solver.ActiveCells;
        }

        public override int GetCellCountX()
        {
            return _solver.Nx;
        }

        public override int GetCellCountY()
        {
            return _solver.Ny;
        }

        public override float GetCellSizeX()
        {
            return (float)_solver.dx;
        }

        public override float GetCellSizeY()
        {
            return (float)_solver.dy;
        }

        public override FireCellState GetFireCellState(Vector2d simulationPos)
        {
            throw new NotImplementedException();
        }

        public override float[] GetFireLineIntensityData()
        {
            return _maxFireIntensityData;
        }

        public override float[] GetFuelModelNumberData()
        {
            throw new NotImplementedException();
        }

        public override List<Vector2int> GetIgnitedFireCells()
        {
            throw new NotImplementedException();
        }

        public override double GetInternalDeltaTime()
        {
            return _internalDeltaTime;
        }

        public override float[,] GetMaxROS()
        {
            throw new NotImplementedException();
        }

        public override float[,] GetMaxROSAzimuth()
        {
            throw new NotImplementedException();
        }

        public override void GetOffsetAndSize(out Vector2d offset, out Vector2d size)
        {
            offset = _originOffset;
            size = _landscapeSize;
        }

        public override float[] GetSootProduction()
        {
            throw new NotImplementedException();
        }

        public override bool IsSimulationDone()
        {
            return false;
        }

        public override void Stop()
        {
            //nothing to do
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
