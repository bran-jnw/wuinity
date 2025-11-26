using PREACT.Math;
using PREACT.Fire.Behave;

namespace PREACT.Fire
{
    public class FuelCell
    {
        public Vector2int _index;
        public bool _dead;
        public bool _ignited;
        public LandscapeCellData _cellData;
        public float _maxROS;
        private double _cellSize;
        public int _linearIndex;
        private CellParticleHybrid _owner;
        private bool _rateOfSpreadIsSet;

        Surface _surface;
        Crown _crownFire;

        public Vector2int Index { get => _index; }

        public Vector3d IgnitionPoint;
        private float _residualTime;
        private InitialFuelMoisture _moisture;
        private int _activeVertexCount;

        public FuelCell(bool randomCenter, int xIndex, int yIndex, LandscapeData landscape, FuelModelSet fuelModelSet, bool[,] wuiArea, int xDim, int yDim, CellParticleHybrid owner, InitialFuelMoistureLibrary initialFuelMoistures)
        {
            _owner = owner;
            _index = new Vector2int(xIndex, yIndex);
            _cellData = landscape.GetCellData(_index.x, _index.y);
            _cellSize = landscape.RasterCellResolutionX;
            _moisture = initialFuelMoistures.GetInitialFuelMoisture(_cellData.fuel_model);
            _linearIndex = xIndex + yIndex * xDim;
            _surface = new Surface(fuelModelSet);
            _crownFire = new Crown(fuelModelSet);


            if (randomCenter)
            {
                double xPos = (Random.valued + xIndex) * _cellSize;
                double yPos = (Random.valued + yIndex) * _cellSize;
                double zPos = landscape.GetElevationLocalPos(xPos, yPos);
                IgnitionPoint = new Vector3d(xPos, yPos, zPos);
            }
            else
            {
                double xPos = (xIndex + 0.5) * _cellSize;
                double yPos = (yIndex + 0.5) * _cellSize;
                double zPos = _cellData.elevation;
                IgnitionPoint = new Vector3d(xPos, yPos, zPos);
            }

            _dead = false;
            if (wuiArea[_index.x, _index.y] || _surface.isAllFuelLoadZero(_cellData.fuel_model))
            {
                _dead = true;
                return;
            }

            _maxROS = float.MinValue;
            _activeVertexCount = 0;
            _rateOfSpreadIsSet = false;
            _ignited = false;
        }

        public void AddActiveVertex()
        {
            ++_activeVertexCount;
        }

        public void RemoveDeadVertex()
        {
            --_activeVertexCount;
            if (_activeVertexCount < 1)
            {

            }
        }

        /// <summary>
        /// Needed after creation of all cells.
        /// </summary>
        /// <param name="xDim"></param>
        /// <param name="yDim"></param>
        /// <param name="wuiArea"></param>
        /// <param name="surface"></param>
        /// <param name="cells"></param>
        private void SpawnFireVertices(int xDim, int yDim, FuelCell[,] cells, float currentTime)
        {
            for (int i = 0; i < CellParticleHybrid.NeighborIndices.Length; ++i)
            {
                Vector2int targetIndex = _index + CellParticleHybrid.NeighborIndices[i];
                if (CellParticleHybrid.IsInside(xDim, yDim, targetIndex) && !cells[targetIndex.x, targetIndex.y]._dead)
                {
                    int particleIndex = _linearIndex * 8 + i; //assume at most 8 particles per cell
                    FuelCell targetCell = cells[targetIndex.x, targetIndex.y];
                    FireParticle f = new FireParticle(this, targetCell, particleIndex, currentTime, _residualTime, _owner);                    
                }
            }
        }

        public void UpdateRateOfSpread()
        {
            //InitialFuelMoisture moisture = initialFuelMoistures.GetInitialFuelMoisture(_cellData.fuel_model);
            double crownRatio = 1.5; //TODO: how to get this data? LCP does not seem to carry it
            double midFlameWindspeed = 20;
            double windDirection = 180;

            _cellData.fuel_model = 1;
            _cellData.slope = 0;
            _cellData.aspect = 0;

            if (!_rateOfSpreadIsSet)
            {
                _surface.updateSurfaceInputs(_cellData.fuel_model, _moisture.OneHour, _moisture.TenHour, _moisture.HundredHour, _moisture.LiveHerbaceous, _moisture.LiveWoody, CellParticleHybrid.MoistureUnits,
                    midFlameWindspeed, CellParticleHybrid.WindSpeedUnits, CellParticleHybrid.WindHeightInputMode, windDirection, CellParticleHybrid.WindAndSpreadOrientationMode, _cellData.slope, CellParticleHybrid.SlopeUnits, _cellData.aspect, _cellData.canopy_cover, CellParticleHybrid.CoverUnits, _cellData.crown_canopy_height, CellParticleHybrid.LengthUnits, crownRatio);
                _rateOfSpreadIsSet = true; ;
            }
            else
            {
                //this should basically be all that is updated, maybe moisture
                _surface.setWindDirection(windDirection);
                _surface.setWindSpeed(midFlameWindspeed, CellParticleHybrid.WindSpeedUnits, CellParticleHybrid.WindHeightInputMode);
            }

            _surface.doSurfaceRunInDirectionOfMaxSpread();
            _owner.SetCellFirelineIntensity(_linearIndex, (float)_surface.getFirelineIntensity(BehaveUnits.FirelineIntensityUnits.FirelineIntensityUnitsEnum.KilowattsPerMeter));
        }

        public float GetSpreadRateInDirection(double spreadDirection)
        {
            if (!_rateOfSpreadIsSet)
            {
                UpdateRateOfSpread();
            }
            //TODO: check unit and convert to m/s
            return (float)BehaveUnits.SpeedUnits.fromBaseUnits(_surface.calculateSpreadRateAtVector(spreadDirection), CellParticleHybrid.WindSpeedUnits);
        }

        bool _scheduleIgnition;
        public float _timeOfArrival;
        public void SchedulelIgnition(float timeOfArrival, float residualTime)
        {
            if (!_scheduleIgnition)
            {
                _scheduleIgnition = true;
                _owner.AddCellToIgnite(this);
            }
            //we might be approached from more than one direction during one timestep, so take max residual time and min time of arrival
            _timeOfArrival = Mathf.Min(_timeOfArrival, timeOfArrival - residualTime);
            _residualTime = Mathf.Max(_residualTime, residualTime);
        }

        public void Ignite(int xDim, int yDim, FuelCell[,] cells, float currentTime)
        {
            if (!_ignited)
            {
                _ignited = true;
                UpdateRateOfSpread();
                SpawnFireVertices(xDim, yDim, cells, currentTime);
            }
        }
    }
}
