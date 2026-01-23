using PREACT.Math;
using PREACT.Wildfire.Behave;
using System.Collections.Generic;

namespace PREACT.Wildfire
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

        SpreadModel _spreadModel;

        public Vector2int Index { get => _index; }

        public Vector3d IgnitionPoint;
        private InitialFuelMoisture _moisture;
        private float _timeOfArrival;
        List<FireParticle> _fireParticles;


        public float TimeOfArrival { get => _timeOfArrival; }

        public FuelCell(bool randomCenter, int xIndex, int yIndex, LandscapeData landscape, BehaveCore.FuelModels fuelModels, bool[,] wuiArea, int xDim, int yDim, CellParticleHybrid owner, InitialFuelMoistureLibrary initialFuelMoistures, IO.FireCellInput input)
        {
            _owner = owner;
            _index = new Vector2int(xIndex, yIndex);
            _linearIndex = xIndex + yIndex * xDim;
            _cellData = landscape.GetCellData(_index.x, _index.y);
            _cellSize = landscape.RasterCellResolutionX;
            _moisture = initialFuelMoistures.GetInitialFuelMoisture(_cellData.fuel_model);
            
            if(input.SpreadRateModel == IO.FireCellInput.SpreadRateModels.BehavePlus)
            {
                _spreadModel = new SpreadModelBehave(fuelModels, _cellData, _moisture);
            }
            else if(input.SpreadRateModel == IO.FireCellInput.SpreadRateModels.CanadianFBP)
            {
                //_spreadModel = new SpreadModelCFBP(_cellData, input);
            }


            if (randomCenter)
            {
                double xPos = (Random.valueD + xIndex) * _cellSize;
                double yPos = (Random.valueD + yIndex) * _cellSize;
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
            if (wuiArea[_index.x, _index.y] || !_spreadModel.HasFuelLoad())
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
            
            WindData w = _owner.Simulation.Input.WildfireModule.Data.WindInput.GetWindDataAtTime(currentTime);
            double moistureFoliar = 0;

            //_spreadModel.SetFuelMoisture(
            _spreadModel.CalculateSpreadRate(_owner.Simulation.Weather, _owner.Simulation.Time);
            _owner.UpdateCellData(_index, _linearIndex, (float)_spreadModel.GetFirelineIntensity(), (float)_spreadModel.GetMaxSpreadRate());
        }

        /// <summary>
        /// Ported from Behave as Crown does not expose this.
        /// </summary>
        /// <param name="directionOfInterest"></param>
        /// <returns></returns>
        private double CalculateSpreadRateInDirection(double directionOfInterest)
        {
            return _spreadModel.GetSpreadRateInDirection(directionOfInterest);
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

            _owner.SetTimeOfArrival(_linearIndex, timeOfArrival);
        }
    }
}
