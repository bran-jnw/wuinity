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
        private int _index;

        public int Index { get => _index; }
        public bool Dead { get => _dead; }

        public FireParticle(FuelCell targetCell, Vector3d startLocalPosition, int index, float currentTime, float residualTime, CellParticleHybrid sim)
        {            
            _index = index;
            sim.AddActiveFireParticle(this); //has to be done after index is set
            _targetCell = targetCell;
            _localPosition = startLocalPosition;
            _currentCell = sim.GetCell(_localPosition);
            _currentCell.AddActiveVertex();

            Vector3d delta = _targetCell.IgnitionPoint - _localPosition;
            _spreadVector = delta.normalized;
            _spreadDirection = (float)Vector3d.Angle(Vector3d.up, _spreadVector);
            _distanceLeftToTarget = delta.magnitude;

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

            if (residualTime > 0)
            {
                Step(currentTime - residualTime, residualTime, sim);
            }
        }

        public void Step(float currentTime, float deltaTime, CellParticleHybrid sim)
        {
            if (!_targetCell._dead && !_targetCell._ignited)
            {
                FuelCell cell = sim.GetCell(_localPosition);
                if (cell != _currentCell)
                {
                    _currentCell.RemoveDeadVertex();
                }
                _currentCell = cell;
                float spreadRate = _currentCell.GetSpreadRateInDirection(_spreadDirection);
                if (spreadRate > 0)
                {
                    double delta = deltaTime * spreadRate;
                    _localPosition += delta * _spreadVector;
                    _distanceLeftToTarget -= delta;

                    //we have reached the vertex of the neighbor
                    if (_distanceLeftToTarget <= 0.0)
                    {
                        float residualTime = (float)-_distanceLeftToTarget / spreadRate;
                        _targetCell.SchedulelIgnition(currentTime + deltaTime, residualTime);
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

            if (_dead)
            {
                sim.AddVertexToRemove(this);
                _currentCell.RemoveDeadVertex();
            }
        }
    }
}
