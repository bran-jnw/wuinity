using System;
using System.Collections.Generic;
using System.Text;

namespace PREACT.Drones
{
    public abstract class DroneModule : SimulationModule
    {
        protected DroneModule(Simulation simulation) : base(simulation)
        {
        }

        public override bool IsSimulationDone()
        {
            throw new NotImplementedException();
        }

        public override void Step(double simulationTime, double deltaTime)
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
