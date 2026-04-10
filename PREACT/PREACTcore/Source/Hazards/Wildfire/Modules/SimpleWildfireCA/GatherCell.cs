using PREACT.Math;

namespace PREACT.Wildfire
{
    public enum States { Dead, CanBurn, Active, Ignited }

    public class GatherCell
    {
        GatherCell[] _neighbors = new GatherCell[8];
        double[] _distanceLeft = new double[8];
        public SimpleWildfireCA _owner;
        Vector2int _index;
        int _linearIndex;
        LandscapeCellData _cellData;
        double _timeOfArrival = double.MaxValue;
        double _lastUpdate = 0.0;

        States _state;

        SpreadModel _spreadModel;


        public States State { get => _state; }
        public LandscapeCellData CellData { get => _cellData; }
        public Vector2int Index { get => _index; }


        public GatherCell(int x, int y, LandscapeCellData cellData, SimpleWildfireCA owner, Input.FireCellInput input, InitialFuelMoistureLibrary initialFuelMoistures, BehaveCore.FuelModels fuelModels) 
        {
            _owner = owner;
            _index = new Vector2int(x, y);
            _linearIndex = x + y * owner.GetCellCountX();
            _cellData = cellData;

            if (input.SpreadRateModel == Input.FireCellInput.SpreadRateModels.Behave)
            {
                InitialFuelMoisture moisture = initialFuelMoistures.GetInitialFuelMoisture(_cellData.fuel_model);
                _spreadModel = new SpreadModelBehave(fuelModels, _cellData, moisture);
            }
            else if (input.SpreadRateModel == Input.FireCellInput.SpreadRateModels.CanadianFBP)
            {
                _spreadModel = new SpreadModelCFBP(_cellData, owner.Simulation.Input.WildfireModule.Data.CanadianFBPLookupTable, _owner.Simulation.Spatial);
            }
            else
            {
                _spreadModel = new SpreadModelLookupROS(_cellData, _owner.Simulation.Input.WildfireModule.Data.LookupROSTable);
            }

            _state = States.Dead;
            if(_spreadModel.HasFuelLoad())
            {
                _state = States.CanBurn;
            }
        }

        //dummy to use if no neighbor is found (edges)
        public GatherCell()
        {
            _state = States.Dead;
        }

        public void Initalize(int nx, int ny, double dx, double dy, GatherCell[][] cells)
        {
            if(_state != States.CanBurn)
            {
                return;
            }

            for (int i = 0; i < 8; ++i)
            {
                Vector2int neighborIndex = _index + SimpleWildfireCA.NeighborIndices[i];
                //check if inside
                if(neighborIndex.x > 0 && neighborIndex.x < nx - 1 && neighborIndex.y > 0 && neighborIndex.y < ny - 1)
                {
                    GatherCell neighbor = cells[neighborIndex.x][neighborIndex.y];
                    if(neighbor.State == States.CanBurn)
                    {
                        _neighbors[i] = neighbor;
                        _distanceLeft[i] = Vector3d.Distance(new Vector3d(_index.x * dx, _index.y * dy, _cellData.elevation), new Vector3d(neighborIndex.x * dx, neighborIndex.y * dy, neighbor.CellData.elevation));
                    }
                    else
                    {
                        _neighbors[i] = SimpleWildfireCA.DeadCell;
                    }
                }
                else
                {
                    _neighbors[i] = SimpleWildfireCA.DeadCell;
                }
            }
        }

        public void IgniteAndSpread(bool inputIgnition, double ignitionTime)
        {
            if(_state == States.CanBurn || _state == States.Active) //need both as CanBurn is when ignited from user input or spotting, Active is when front has reached centroid
            {
                _state = States.Ignited;
                _owner.AddBurnArea();

                for (int i = 0; i < 8; ++i)
                {
                    if (_neighbors[i].State == States.CanBurn)
                    {
                        int index = GetOppositeIndex(i);
                        double residual = 0.0;
                        if (inputIgnition)
                        {
                            _timeOfArrival = ignitionTime;
                            UpdateRateOfSpread();
                        }
                        else 
                        {
                            residual = _distanceLeft[index];
                        }
                        _neighbors[i].SetActive(index, residual);
                    }
                }
            }  
            else
            {
                Engine.Message(_owner.Simulation, Engine.LogType.Warning, "Tried to ignite a dead cell.");
            }
        }

        public void SetActive(int comingFromIndex, double distanceResidual)
        {
            _distanceLeft[comingFromIndex] += distanceResidual; //this is the overshoot from the current time step using the other cell, it is always less =< 0 (negative distance left)

            if(_state == States.CanBurn)
            {
                _state = States.Active;
                _owner.AddCellToActive(this);
            }
        }

        bool _registeredIgnition = false;
        public void Step(double simulationTime, double deltaTime, double windSpeed, double windDirection)
        {            
            for (int i = 0; i < 8; ++i)
            {
                if (_neighbors[i].State == States.Ignited) //needed as all neighbors might not be ignited
                {                    
                    double spreadRate = GetSpreadRateInDirection(i, SimpleWildfireCA.SpreadDirectionsTowards[i], simulationTime);
                    _distanceLeft[i] -= spreadRate * deltaTime;
                    if (_distanceLeft[i] <= 0.0)
                    {
                        _timeOfArrival = Mathd.Min(_timeOfArrival, simulationTime + deltaTime + _distanceLeft[i] / spreadRate);
                        if(!_registeredIgnition) //needed as multiple fronts might reach centroid during one time step
                        {
                            _registeredIgnition = true;
                            _owner.AddCellToIgnite(this);
                        }
                    }
                }
            }          
        }     

        private int GetOppositeIndex(int index)
        {
            int oppositeIndex = index - 4;
            if (oppositeIndex < 0)
            {
                oppositeIndex += 8;
            }

            return oppositeIndex;
        }

        public void UpdateRateOfSpread()
        {
            _spreadModel.CalculateSpreadRate(_owner.Simulation.Weather, _owner.Simulation.Time);
            _owner.UpdateCellData(_index, _linearIndex, (float)_spreadModel.GetFireIntensity(), (float)_spreadModel.GetMaxSpreadRate(), (float)_spreadModel.GetDirectionOfMaxSpread());
        }

        double[] _nextUpdate = new double[8];
        double[] _cachedSpreadRates = new double[8];
        public double GetSpreadRateInDirection(int directionIndex, double spreadDirection, double simulationTime)
        {
            if (simulationTime - _lastUpdate >= _nextUpdate[directionIndex])
            {
                _nextUpdate[directionIndex] = Random.valueD * 120 + 240.0; //update every 4-6 minutes, weather does not change that often
                _lastUpdate = simulationTime;
                UpdateRateOfSpread();
                _cachedSpreadRates[directionIndex] = _spreadModel.GetSpreadRateInDirection(spreadDirection);
            }

            return _cachedSpreadRates[directionIndex];
        }
    }
}
