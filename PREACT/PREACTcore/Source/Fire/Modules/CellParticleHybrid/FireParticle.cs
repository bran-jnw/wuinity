using PREACT.Math;

namespace PREACT.Fire
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
        private float _distanceCorrection;

        public bool Dead { get => _dead; }

        public FireParticle(FuelCell startCell, FuelCell targetCell, float ignitionTime, float residualTime, CellParticleHybrid sim, bool diagonal)
        {
            _ignitionTime = ignitionTime;
            _targetCell = targetCell;
            _localPosition = startCell.IgnitionPoint;
            _currentCell = startCell;
            sim.AddActiveFireParticle(this);            

            Vector3d delta = _targetCell.IgnitionPoint - _localPosition;
            _spreadVector = delta.normalized;
            //TODO: correct or should be "flat" (projected onto plane) angle?, and see if better way to determine sign
            //maybe use? https://stackoverflow.com/questions/14066933/direct-way-of-computing-the-clockwise-angle-between-two-vectors
            _spreadDirection = (float)Vector3d.Angle(Vector3d.up, _spreadVector) * Mathd.Sign(Vector3d.Dot(Vector3d.right, _spreadVector)); 
            _distanceLeftToTarget = delta.magnitude;

            //these are the factors to compensate for the average distance being longer
            _distanceCorrection = 1.088f;
            if (diagonal)
            {
                _distanceCorrection = 1.042f;
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
                Step(ignitionTime, residualTime, sim);
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
                        
            if (!_targetCell._dead)//the target should never be dead here as the particle will not be created then, but keep it as we might want to enable real-time changes from user
            {
                FuelCell cell = sim.GetCell(_localPosition);                
                _currentCell = cell;
                float spreadRate = _distanceCorrection * _currentCell.GetSpreadRateInDirection(_spreadDirection, currentTime);//TODO: cache the spread rate and only update if in new cell?
                if (spreadRate > 0)
                {
                    double delta = deltaTime * spreadRate;
                    _localPosition += delta * _spreadVector;
                    _distanceLeftToTarget -= delta;

                    //we have reached the ignition point of the target cell
                    if (_distanceLeftToTarget <= 0.0)
                    {
                        _dead = true;
                        float residualTime = (float)-_distanceLeftToTarget / spreadRate;
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
