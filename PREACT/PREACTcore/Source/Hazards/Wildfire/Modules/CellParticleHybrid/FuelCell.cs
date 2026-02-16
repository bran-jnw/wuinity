using PREACT.Math;
using PREACT.Wildfire.Behave;
using System.Collections.Generic;

namespace PREACT.Wildfire
{
    public class FuelCell
    {
        public Vector2int _index;
        private bool _dead;
        public bool _ignited;
        public LandscapeCellData _cellData;
        public float _maxROS;
        private double _cellSize;
        private int _linearIndex;
        private CellParticleHybrid _owner;
        private bool _rateOfSpreadIsSet;

        SpreadModel _spreadModel;

        public Vector2int Index { get => _index; }
        public int LinearIndex { get => _linearIndex; }

        public Vector3d IgnitionPoint;
        private float _timeOfArrival;
        List<FireParticle> _fireParticles;

        public bool Dead { get => _dead; }


        public float TimeOfArrival { get => _timeOfArrival; }

        public FuelCell(bool randomCenter, int xIndex, int yIndex, LandscapeData landscape, BehaveCore.FuelModels fuelModels, bool[,] wuiArea, int xDim, int yDim, CellParticleHybrid owner, InitialFuelMoistureLibrary initialFuelMoistures, IO.FireCellInput input)
        {
            _owner = owner;
            _index = new Vector2int(xIndex, yIndex);
            _linearIndex = xIndex + yIndex * xDim;
            _cellData = landscape.GetCellData(_index.x, _index.y);
            _cellSize = landscape.RasterCellResolutionX;                

            if (input.SpreadRateModel == IO.FireCellInput.SpreadRateModels.BehavePlus)
            {
                InitialFuelMoisture moisture = initialFuelMoistures.GetInitialFuelMoisture(_cellData.fuel_model);
                _spreadModel = new SpreadModelBehave(fuelModels, _cellData, moisture);
            }
            else if(input.SpreadRateModel == IO.FireCellInput.SpreadRateModels.CanadianFBP)
            {
                _spreadModel = new SpreadModelCFBP(_cellData, owner.Simulation.Input.WildfireModule.Data.CanadianFBPLookupTable, _owner.Simulation.Spatial);
            }
            else
            {
                _spreadModel = new SpreadModelLookUpTable(_cellData, _owner.Simulation.Input.WildfireModule.Data.ConstantLookupTable);
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

            _dead = true;
            if (!wuiArea[_index.x, _index.y] && _spreadModel.HasFuelLoad())
            {
                _dead = false;
            }
            _maxROS = float.MinValue;
            _rateOfSpreadIsSet = false;
            _ignited = false;
        }

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

        float lastUpdate; 
        public void UpdateRateOfSpread(float currentTime)
        {
            /*if (_dead)
            {
                Engine.Message(_owner.Simulation, Engine.LogType.Log, $"Trying to update dead cell, fuel number {_cellData.fuel_model}.");
                return;
            }*/

            //only update every 60 seconds, wetaher does not change that often
            if(currentTime - lastUpdate > 60f || !_rateOfSpreadIsSet)
            {
                lastUpdate = currentTime;
                _spreadModel.CalculateSpreadRate(_owner.Simulation.Weather, _owner.Simulation.Time);
                _owner.UpdateCellData(_index, _linearIndex, (float)_spreadModel.GetFireIntensity(), (float)_spreadModel.GetMaxSpreadRate(), (float)_spreadModel.GetDirectionOfMaxSpread());
                _rateOfSpreadIsSet = true;
            }            
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

            return (float)_spreadModel.GetSpreadRateInDirection(spreadDirection);
        }

        public void Ignite(float timeOfArrival, float residualTime)
        {
            //just in case we try to ignite a dead cell
            if (_dead)
            {
                Engine.Message(_owner.Simulation, Engine.LogType.Log, $"Trying to ignite dead cell, fuel number {_cellData.fuel_model}.");
                return;
            }

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
