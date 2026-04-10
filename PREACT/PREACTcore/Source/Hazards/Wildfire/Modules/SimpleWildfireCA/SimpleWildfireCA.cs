using PREACT.Math;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace PREACT.Wildfire
{
    public class SimpleWildfireCA : WildfireModule
    {
        public static readonly Vector2int[] NeighborIndices = new Vector2int[] { Vector2int.up, new Vector2int(1, 1), Vector2int.right, new Vector2int(1, -1), Vector2int.down, new Vector2int(-1, -1), Vector2int.left, new Vector2int(-1, 1) };
        public static readonly double[] SpreadDirectionsForward = new double[] { 0.0, 45.0, 90.0, 135.0, 180.0, 225.0, 270.0, 315.0 };
        public static readonly double[] SpreadDirectionsTowards = new double[] { 180.0, 225.0, 270.0, 315.0, 0.0, 45.0, 90.0, 135.0 };

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
        

        //Behave stuff
        BehaveCore.FuelModels _fuelModels = null;

        public Simulation Simulation { get => _simulation; }


        private GatherCell[][] _cells;
        public static readonly GatherCell DeadCell = new GatherCell();
        int _nx, _ny;
        double _dx, _dy, _cellArea;

        public SimpleWildfireCA(Simulation simulation, LandscapeData landscape, List<IgnitionPointInput> ignitionPoints, WeatherManager weather, TimeManager time) : base(simulation)
        {
            _weather = weather;
            _time = time;
            _originOffset = landscape.OriginOffset;
            _landscapeSize = new Vector2d(landscape.GetLandscapeSizeX(), landscape.GetLandscapeSizeY());
            _nx = simulation.Input.WildfireModule.Data.LandscapeData.GetCellCountX();
            _ny = simulation.Input.WildfireModule.Data.LandscapeData.GetCellCountY();
            _dx = simulation.Input.WildfireModule.Data.LandscapeData.RasterCellResolutionX;
            _dy = simulation.Input.WildfireModule.Data.LandscapeData.RasterCellResolutionY;
            _cellArea = _dx * _dy;

            _maxFireIntensityData = new float[_nx * _ny];
            _maxRosDirectionData = new float[_nx, _ny];
            _maxRosData = new float[_nx, _ny];
            _maxRosDirectionData = new float[_nx, _ny];
            _timeOfArrivalData = new float[_nx * _ny];
            _ignitedCellIndices = new List<Vector2int>(256);

            _activeCells = new HashSet<GatherCell>(_nx * _ny / 10);
            _cellsToIgnite = new ConcurrentBag<GatherCell>();
            
            InitialFuelMoistureLibrary initialFuelMoistures = null;
            if (_simulation.Input.WildfireModule.FireCellInput.SpreadRateModel == Input.FireCellInput.SpreadRateModels.Behave)
            {
                _fuelModels = new BehaveCore.FuelModels();
                initialFuelMoistures = _simulation.Input.WildfireModule.Data.InitialFuelMoistureData;
            }

            //create
            _cells = new GatherCell[_nx][];
            for(int i = 0; i < _nx; ++i)
            {
                _cells[i] = new GatherCell[_ny];
                for (int j = 0; j < _ny; ++j)
                {
                    _cells[i][j] = new GatherCell(i, j, landscape.GetCellData(i, j), this, _simulation.Input.WildfireModule.FireCellInput, initialFuelMoistures, _fuelModels);
                }
            }

            //initialize
            for (int i = 0; i < _nx; ++i)
            {
                for (int j = 0; j < _ny; ++j)
                {
                    _cells[i][j].Initalize(_nx, _ny, _dx, _dy, _cells);
                }
            }

            _ignitionPoints = new List<IgnitionPoint>(ignitionPoints.Count);
            for (int i = 0; i < ignitionPoints.Count; ++i)
            {
                _ignitionPoints.Add(new IgnitionPoint(_simulation, ignitionPoints[i]));
            }
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

        public void SetTimeOfArrival(int xIndex, int yIndex, float timeOfArrival)
        {
            _timeOfArrivalData[xIndex + yIndex * _nx] = timeOfArrival;
            _ignitedCellIndices.Add(new Vector2int(xIndex, yIndex));
        }        

        public override void ConsumeIgnitedFireCells()
        {
            _ignitedCellIndices.Clear();
        }

        public override int GetActiveCellCount()
        {
            return _activeCells.Count;
        }

        public override int GetCellCountX()
        {
            return _nx;
        }

        public override int GetCellCountY()
        {
            return _ny;
        }

        public override float GetCellSizeX()
        {
            return (float)_dx;
        }

        public override float GetCellSizeY()
        {
            return (float)_dy;
        }

        public override FireCellState GetFireCellState(Vector2d simulationPos)
        {
            Vector2int index = SimulationPosToCellIndex(simulationPos, out bool inside);
            if (inside)
            {
                if (_cells[index.x][index.y].State == States.Ignited)
                {
                    return FireCellState.Ignited;
                }
            }

            return FireCellState.Dead;
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
            return _ignitedCellIndices;
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
            return _maxRosDirectionData;
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
        
        ConcurrentBag<GatherCell> _cellsToIgnite;
        //only cells can add themselves
        public void AddCellToIgnite(GatherCell cell)
        {
            _cellsToIgnite.Add(cell);
            _ignitedCellIndices.Add(cell.Index);
        }

        HashSet<GatherCell> _activeCells;
        //only cells can remove themselves
        public void AddCellToActive(GatherCell cell)
        {
            _activeCells.Add(cell);
        }

        public override void Step(double simulationTime, double deltaTime)
        {
            _internalDeltaTime = deltaTime;
            double windSpeed, windDirection;
            _weather.GetWind(out windSpeed, out windDirection);

            HandleIgnitionInput(simulationTime);

            //update, if cell has reached its centroid it adds to _cellsToIgnite
            Parallel.ForEach(_activeCells, cell =>
            {
                cell.Step(simulationTime, deltaTime, windSpeed, windDirection);             
            });

            //set to ignited and activate neighbors, then remove from active as the cells job is done
            foreach (GatherCell cell in _cellsToIgnite)
            {
                cell.IgniteAndSpread(false, simulationTime + deltaTime);
                _activeCells.Remove(cell);
            }
            _cellsToIgnite.Clear();
        }

        public void AddBurnArea()
        {
            _currentBurnArea += _cellArea;
        }

        private void HandleIgnitionInput(double simulationTime)
        {
            for (int i = 0; i < _ignitionPoints.Count; ++i)
            {
                if (_ignitionPoints[i].IgnitionTime <= simulationTime)
                {
                    IgniteAtSimulationPos(_ignitionPoints[i]);
                    _ignitionPoints.Remove(_ignitionPoints[i]);

                    if (_initialIgnition < 0)
                    {
                        _initialIgnition = simulationTime;
                    }
                }
            }
        }

        private void IgniteAtSimulationPos(IgnitionPoint ignition)
        {
            Vector2d localPos = ignition.SimulationPos;
            localPos -= _originOffset;
            int xIndex = (int)(_nx * localPos.x / _landscapeSize.x);
            int yIndex = (int)(_ny * localPos.y / _landscapeSize.y);

            if (IsInside(xIndex, yIndex))
            {
                _cells[xIndex][yIndex].IgniteAndSpread(true, ignition.IgnitionTime);
                _simulation.Detection.RegisterFireIgnition(ignition.LatLon, _cells[xIndex][yIndex].CellData.elevation);
                Engine.Message(_simulation, Engine.LogType.Log, $"Ignition happened at position [{ignition.SimulationPos.x}, {ignition.SimulationPos.y}] (lat/lon {ignition.LatLon.x}, {ignition.LatLon.y}) as requested by user.");
            }
            else
            {
                Engine.Message(_simulation, Engine.LogType.Log, $"Tried to ignite at position [{ignition.SimulationPos.x}, {ignition.SimulationPos.y}] (lat/lon {ignition.LatLon.x}, {ignition.LatLon.y}) but this is outside of the provided landscape.");
            }
        }

        public override Vector2int SimulationPosToCellIndex(Vector2d simulationPos, out bool inside)
        {
            Vector2d LocalPos = simulationPos;
            LocalPos -= _originOffset;
            int xIndex = (int)(_nx * LocalPos.x / _landscapeSize.x);
            int yIndex = (int)(_ny * LocalPos.y / _landscapeSize.y);
            inside = IsInside(xIndex, yIndex);

            return new Vector2int(xIndex, yIndex);
        }
       
        public bool IsInside(double xPos, double yPos)
        {
            if (xPos < 0 || xPos > _landscapeSize.x || yPos < 0 || yPos > _landscapeSize.y)
            {
                return false;
            }

            return true;
        }

        public bool IsInside(int xIndex, int yIndex)
        {
            if (xIndex < 0 || xIndex >= _nx || yIndex < 0 || yIndex >= _ny)
            {
                return false;
            }

            return true;
        }

        public override void Stop()
        {
            //save output
        }

        public override bool Ignited()
        {
            return _initialIgnition > 0 ? true : false;
        }
    }
}
