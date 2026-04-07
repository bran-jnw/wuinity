using PREACT.Evacuation;
using System.Collections.Generic;

namespace PREACT
{
    /// <summary>
    /// This class is supposed to collect all the communication between different sub-modules, 
    /// e.g. traffic simulation needing information from the smoke or fire simulation.
    /// This is done to not clutter up the simulation class itself.
    /// </summary>
    public class DetectionManager
    {
        private Simulation _simulation;

        private DroneModule _droneModule;

        public DetectionManager(Simulation simulation)
        {
            _simulation = simulation;
        }

        public List<SimulationModule> CreateModules(WeatherManager weather, TimeManager time, out bool success)
        {
            List<SimulationModule> createdModules = new List<SimulationModule>();            

            CreateDroneModule(out success);
            if (success && _droneModule != null)
            {
                createdModules.Add(_droneModule);
            }
            else
            {
                return createdModules;
            }

            return createdModules;
        }

        private void CreateDroneModule(out bool success)
        {
            //Panos
            success = true;
        }

        public void PostStep()
        {

        }
    }
}

