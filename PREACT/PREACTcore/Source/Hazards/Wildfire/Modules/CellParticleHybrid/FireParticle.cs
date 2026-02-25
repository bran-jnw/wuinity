using PREACT.Math;

namespace PREACT.Wildfire
{
    public class FireParticle
    {
        private FuelCell _targetCell;
        private FuelCell _currentCell;
        private Vector3d _localPosition;
        private Vector3d _spreadVector;
        private double _spreadDirection;
        private bool _dead;
        private double _distanceLeftToTarget;
        private float _ignitionTime;
        private float _spreadRate;

        public bool Dead { get => _dead; }

        public FireParticle(FuelCell startCell, FuelCell targetCell, float ignitionTime, float residualTime, CellParticleHybrid wildfireSim, bool diagonal)
        {
            _ignitionTime = ignitionTime;
            _targetCell = targetCell;
            _localPosition = startCell.IgnitionPoint;
            _currentCell = startCell;
            wildfireSim.AddActiveFireParticle(this);            

            Vector3d delta = _targetCell.IgnitionPoint - _localPosition;
            _spreadVector = delta.normalized;
            //TODO: correct or should be "flat" (projected onto plane) angle?, and see if better way to determine sign
            //maybe use? https://stackoverflow.com/questions/14066933/direct-way-of-computing-the-clockwise-angle-between-two-vectors
            Vector2d flatSpreadVector = new Vector2d(_spreadVector.x, _spreadVector.y);
            _spreadDirection = Vector2d.Angle(Vector2d.up, flatSpreadVector) * Mathd.Sign(Vector2d.Dot(Vector2d.right, flatSpreadVector));
            if(_spreadDirection < 0)
            {
                _spreadDirection += 360.0;
            }
            _distanceLeftToTarget = delta.magnitude;

            //these are the factors to compensate for the average distance being longer with randomized ignition points
            //average distance between cells sharing one side is 1.088f;
            //average distance between cells with touching corners is 1.042f
            if (!diagonal)
            {
                _distanceLeftToTarget *= 0.9575533928173384; ; //ratio between  1.0419... / 1.088...  = 0.9575111441172938 done with 1 000 000 000 MonteCarlo samples per ratio
                //_distanceLeftToTarget *= 0.9792654911267634; //this is the same thing but with random factor 0.5
            }

            if (CellParticleHybrid.inverseSpreadDirection)
            {
                _spreadVector *= -1;
                _spreadDirection += 180f;
                if (_spreadDirection >= 360f)
                {
                    _spreadDirection -= 360f;
                }
            }

            _dead = false;

            //since we might have overshot the ignition point in the ignited fuel cell, we have to take the overshot time and move the new particle to compensate
            if (residualTime > 0)
            {
                Step(ignitionTime, residualTime, wildfireSim);
            }
        }

        public void UpdateIgnitionTime(float newIgnitionTime)
        {
            _ignitionTime = newIgnitionTime;
        }

        public void Step(float currentTime, float deltaTime, CellParticleHybrid sim)
        {
            //since we are immediately added to the active queue upon ignition we do not want to step at ignition (any residual time step us done during creation) 
            //we need to check if we are dead as we end up doing recursive calls outside of the main loop for large time steps
            if(currentTime < _ignitionTime || _dead)
            {
                return;
            }
                        
            if (!_targetCell.Dead)//the target should never be dead here as the particle will not be created then, but keep it as we might want to enable real-time changes from user
            {
                FuelCell cell = sim.GetCell(_localPosition);                
                _currentCell = cell;
                if(!_currentCell.Dead)
                {
                    double spreadDirection = _spreadDirection;
                    double headDirection = _currentCell.GetDirectionOfMaxSpread(currentTime);
                    double theta = Mathd.Abs(headDirection - _spreadDirection);
                    //modifier for cellular particle hybrid
                    if (theta < 22.5)
                    {
                        spreadDirection = headDirection;
                    }
                    else if(theta <45)
                    {
                        spreadDirection = headDirection + 45 * (theta - 22.5) / (45-22.5); //this is not a good idea as it overestimates the lateral spread
                    }

                    _spreadRate = _currentCell.GetSpreadRateInDirection(spreadDirection, currentTime);//TODO: cache the spread rate and only update if in new cell?                    
                }
                else
                {
                    //this means that we are crossing a cell that is dead, do we die or continue at same spread rate as before?
                    //_dead = true;
                    //return;
                }

                if (_spreadRate > 0)
                {
                    double delta = deltaTime * _spreadRate;
                    _localPosition += delta * _spreadVector;
                    _distanceLeftToTarget -= delta;

                    //we have reached the ignition point of the target cell
                    if (_distanceLeftToTarget <= 0.0)
                    {
                        _dead = true;
                        float residualTime = (float)-_distanceLeftToTarget / _spreadRate;
                        float timeOfArrival = currentTime + deltaTime - residualTime;
                        _targetCell.Ignite(timeOfArrival, residualTime);                        
                    }
                    //we had the chance to possibly ignite the target cell earlier than any other arrivals during the same time step, but if it does not happen during this time step we kill it as the target is ignited 
                    else if(_targetCell._ignited)
                    {
                        _dead = true;
                    }                    
                }
                else
                {
                    _dead = true;
                }
            }
            else
            {
                _dead = true;
            }
        }
    }
}
