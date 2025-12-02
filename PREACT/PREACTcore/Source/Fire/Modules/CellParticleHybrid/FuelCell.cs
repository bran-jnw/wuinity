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

        //Surface _surfaceOnly;
        //Crown _crownAndSurface;
        BehaveCore.Crown _crownBehave;

        public Vector2int Index { get => _index; }

        public Vector3d IgnitionPoint;
        private InitialFuelMoisture _moisture;
        private float _timeOfArrival;
        List<FireParticle> _fireParticles;
        double _forwardSpreadRate;
        double _directionOfMaxSpread;
        double _eccentricity;


        public float TimeOfArrival { get => _timeOfArrival; }

        public FuelCell(bool randomCenter, int xIndex, int yIndex, LandscapeData landscape, BehaveCore.FuelModels fuelModels, bool[,] wuiArea, int xDim, int yDim, CellParticleHybrid owner, InitialFuelMoistureLibrary initialFuelMoistures)
        {
            _owner = owner;
            _index = new Vector2int(xIndex, yIndex);
            _linearIndex = xIndex + yIndex * xDim;
            _cellData = landscape.GetCellData(_index.x, _index.y);
            _cellSize = landscape.RasterCellResolutionX;
            _moisture = initialFuelMoistures.GetInitialFuelMoisture(_cellData.fuel_model);

            //_surfaceOnly = new Surface(fuelModels);
            //_crownAndSurface = new Crown(fuelModels);
            _crownBehave = new BehaveCore.Crown(fuelModels);

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
            if (wuiArea[_index.x, _index.y] || _crownBehave.isAllFuelLoadZero(_cellData.fuel_model))
            {
                _dead = true;
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
                    FireParticle f = new FireParticle(this, targetCell, ignitionTime, residualTime, _owner, i % 2 > 0); 
                    _fireParticles.Add(f);
                }
            }
        }

        public void UpdateRateOfSpread(float currentTime)
        {
            //InitialFuelMoisture moisture = initialFuelMoistures.GetInitialFuelMoisture(_cellData.fuel_model);
            double crownRatio = 1.0; //This can be whatever as Behave clauclates it internally each time anyway, so not sure why it is an input
            WindData w = _owner.Simulation.Input.Fire.Data.WindInput.GetWindDataAtTime(currentTime);
            float windSPeed = w.speed;
            double windDirection = w.direction;
            double moistureFoliar = 0;

            if (!_rateOfSpreadIsSet)
            {
                /*_surfaceOnly.updateSurfaceInputs(_cellData.fuel_model, _moisture.OneHour, _moisture.TenHour, _moisture.HundredHour, _moisture.LiveHerbaceous, _moisture.LiveWoody, CellParticleHybrid.MoistureUnits,
                    windSPeed, CellParticleHybrid.WindSpeedUnits, CellParticleHybrid.WindHeightInputMode, windDirection, CellParticleHybrid.WindAndSpreadOrientationMode, _cellData.slope, CellParticleHybrid.SlopeUnits, 
                    _cellData.aspect, _cellData.canopy_cover, CellParticleHybrid.CoverUnits, _cellData.crown_canopy_height, CellParticleHybrid.LengthUnits, crownRatio);

                _crownAndSurface.updateCrownInputs(_cellData.fuel_model, _moisture.OneHour, _moisture.TenHour, _moisture.HundredHour, _moisture.LiveHerbaceous, _moisture.LiveWoody, moistureFoliar, CellParticleHybrid.MoistureUnits,
                    windSPeed, CellParticleHybrid.WindSpeedUnits, CellParticleHybrid.WindHeightInputMode, windDirection, CellParticleHybrid.WindAndSpreadOrientationMode, _cellData.slope, CellParticleHybrid.SlopeUnits,
                    _cellData.aspect, _cellData.canopy_cover, CellParticleHybrid.CoverUnits, _cellData.crown_canopy_height, _cellData.crown_base, CellParticleHybrid.LengthUnits, crownRatio, _cellData.crown_bulk_density, CellParticleHybrid.DensityUnits);*/

                _crownBehave.updateCrownInputs(_cellData.fuel_model, _moisture.OneHour, _moisture.TenHour, _moisture.HundredHour, _moisture.LiveHerbaceous, _moisture.LiveWoody, moistureFoliar, CellParticleHybrid.MoistureUnits,
                    windSPeed, CellParticleHybrid.WindSpeedUnits, CellParticleHybrid.WindHeightInputMode, windDirection, CellParticleHybrid.WindAndSpreadOrientationMode, _cellData.slope, CellParticleHybrid.SlopeUnits,
                    _cellData.aspect, _cellData.canopy_cover, CellParticleHybrid.FractionUnits, _cellData.crown_canopy_height, _cellData.crown_base, CellParticleHybrid.LengthUnits, crownRatio, CellParticleHybrid.FractionUnits, _cellData.crown_bulk_density, CellParticleHybrid.DensityUnits);

                _rateOfSpreadIsSet = true;
            }
            else
            {
                //this should basically be all that is updated after intitial set, maybe moisture
                /*_surfaceOnly.setWindDirection(windDirection);
                _surfaceOnly.setWindSpeed(windSPeed, CellParticleHybrid.WindSpeedUnits, CellParticleHybrid.WindHeightInputMode);*/
                _crownBehave.setWindDirection(windDirection);
                _crownBehave.setWindSpeed(windSPeed, CellParticleHybrid.WindSpeedUnits, CellParticleHybrid.WindHeightInputMode);
            }

            _crownBehave.doCrownRunRothermel();
            _forwardSpreadRate = _crownBehave.getFinalSpreadRate(BehaveCore.SpeedUnits.SpeedUnitsEnum.MetersPerSecond);
            _eccentricity = _crownBehave.getFireEccentricity();
            _directionOfMaxSpread = _crownBehave.getDirectionOfMaxSpread();
            _owner.SetCellFirelineIntensity(_linearIndex, (float)_crownBehave.getFinalFirelineIntesity(BehaveCore.FirelineIntensityUnits.FirelineIntensityUnitsEnum.KilowattsPerMeter));
        }

        /// <summary>
        /// Ported from Behave as Crown does not expose this.
        /// </summary>
        /// <param name="directionOfInterest"></param>
        /// <returns></returns>
        private double CalculateSpreadRateInDirection(double directionOfInterest)
        {
            double rosDirection = _forwardSpreadRate;
            if (_forwardSpreadRate != 0.0) // if forward spread rate is not zero
            {
                // Calculate the fire spread rate in this azimuth
                // if it deviates more than a tenth degree from the maximum azimuth

                // Calculate beta: the angle between the direction of max spread and the direction of interest
                double beta = Mathd.Abs(_directionOfMaxSpread - directionOfInterest);

                // Calculate the fire spread rate in this azimuth
                // if it deviates more than a tenth degree from the maximum azimuth
                if (beta > 180.0)
                {
                    beta = (360.0 - beta);
                }
                if (Mathd.Abs(beta) > 0.1)
                {
                    double radians = beta * Mathd.PI / 180.0;
                    rosDirection = _forwardSpreadRate * (1.0 - _eccentricity) / (1.0 - _eccentricity * Mathd.Cos(radians));
                }
            }
            return rosDirection;
        }

        /// <summary>
        /// Returns spread rate in meters per second in direction of interest.
        /// </summary>
        /// <param name="spreadDirection"></param>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        public float GetSpreadRateInDirection(double spreadDirection, float currentTime)
        {
            if (!_rateOfSpreadIsSet)
            {
                UpdateRateOfSpread(currentTime);
            }
            return (float)CalculateSpreadRateInDirection(spreadDirection);
        }

        public void Ignite(float timeOfArrival, float residualTime)
        {
            if (!_ignited)
            {
                _ignited = true;
                _timeOfArrival = timeOfArrival;
                _owner.AddIgnitedCellIndex(_index);
                UpdateRateOfSpread(timeOfArrival); 
                //only do compensation if time diff. is big enough
                /*if(residualTime < 1.0)
                {
                    residualTime = 0;
                }*/
                SpawnFireVertices(timeOfArrival, residualTime);
            }   
            
            //after ignition another particle might show up during the same time step and would actually have arrived earlier, we then correct for this
            if(timeOfArrival < _timeOfArrival)
            {
                residualTime = _timeOfArrival - timeOfArrival;
                //if (residualTime >= 1.0)
                //{
                    for (int i = 0; i < _fireParticles.Count; ++i)
                    {
                        _fireParticles[i].UpdateIgnitionTime(timeOfArrival);
                        _fireParticles[i].Step(timeOfArrival, residualTime, _owner);
                    }
                //}                
                _timeOfArrival = timeOfArrival;
            }
        }
    }
}
