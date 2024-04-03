using UnityEngine;

namespace WUInity
{
    public abstract class SimulationModule
    {
        public abstract void Step(float currentTime, float deltaTime);
        public abstract bool IsSimulationDone();
    }
}

