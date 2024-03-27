using UnityEngine;

namespace WUInity
{
    public abstract class SimulationModule
    {
        public abstract void Update(float currentTime, float deltaTime);
        public abstract bool SimulationDone();
    }
}

