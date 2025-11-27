using PREACT.Math;
using PREACT.Fire.Behave;
using System.Collections.Generic;

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
        private InitialFuelMoisture _moisture;
        private float _timeOfArrival;
        List<FireParticle> _fireParticles;

        public float TimeOfArrival { get => _timeOfArrival; }

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
            _rateOfSpreadIsSet = false;
            _ignited = false;
        }

        /// <summary>
        /// Needed after creation of all cells.
        /// </summary>
        /// <param name="xDim"></param>
        /// <param name="yDim"></param>
        /// <param name="wuiArea"></param>
        /// <param name="surface"></param>
        /// <param name="cells"></param>
        private void SpawnFireVertices(float ignitionTime, float residualTime)
        {
            FuelCell[,] cells = _owner.GetCells();
            _fireParticles = new List<FireParticle>();
            for (int i = 0; i < CellParticleHybrid.NeighborIndices.Length; ++i)
            {
                Vector2int targetIndex = _index + CellParticleHybrid.NeighborIndices[i];
                if (CellParticleHybrid.IsInside(_owner.GetCellCountX(), _owner.GetCellCountY(), targetIndex) && !cells[targetIndex.x, targetIndex.y]._dead)// && !cells[targetIndex.x, targetIndex.y]._ignited)// TODO: think about this, connected to same check in particle Step(). 2. Needs to be gone, as earlier particle might ignite during this loop
                {
                    FuelCell targetCell = cells[targetIndex.x, targetIndex.y];
                    FireParticle f = new FireParticle(this, targetCell, ignitionTime, residualTime, _owner); 
                    _fireParticles.Add(f);
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

        public void Ignite(float ignitionTime, float residualTime)
        {
            if (!_ignited)
            {
                _ignited = true;
                _timeOfArrival = ignitionTime;
                _owner.AddIgnitedCellIndex(_index);
                UpdateRateOfSpread(); 
                //only do compensation if time diff. is big enough
                /*if(residualTime < 1.0)
                {
                    residualTime = 0;
                }*/
                SpawnFireVertices(ignitionTime, residualTime);
            }   
            
            //after ignition another particle might show up during the same time step and would actually have arrived earlier, we then correct for this
            if(ignitionTime < _timeOfArrival)
            {
                residualTime = _timeOfArrival - ignitionTime;
                //if (residualTime >= 1.0)
                //{
                    for (int i = 0; i < _fireParticles.Count; ++i)
                    {
                        _fireParticles[i].UpdateIgnitionTime(ignitionTime);
                        _fireParticles[i].Step(ignitionTime, residualTime, _owner);
                    }
                //}                
                _timeOfArrival = ignitionTime;
            }
        }
    }
}
