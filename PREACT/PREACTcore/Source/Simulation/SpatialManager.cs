using PREACT.Math;

namespace PREACT
{
    /// <summary>
    /// This class is supposed to collect all the communication between different sub-modules, 
    /// e.g. traffic simulation needing information from the smoke or fire simulation.
    /// This is done to not clutter up the simulation class itself.
    /// </summary>
    public class SpatialManager
    {
        private Simulation _simulation;

        private Vector2d _simulationCenterLatLon;

        public Vector2d SimulationCenterLatLon { get => _simulationCenterLatLon; }

        public SpatialManager(Simulation simulation)
        {
            _simulation = simulation;

            _simulationCenterLatLon = simulation.Input.Simulation.Data.GetWGS84FromSimulationPosition(simulation.Input.Simulation.DomainSize * 0.5);
        }

        /// <summary>
        /// Returns the origin offset in meters (UTM coordinate offset compared to origin).
        /// </summary>
        /// <returns></returns>
        public Vector2d GetFireModuleOffset()
        {
            Vector2d result = Vector2d.zero;
            if(_simulation.WildfireModule != null)
            {
                result = _simulation.WildfireModule.GetOriginOffset();
            }

            return result;
        }
    }
}

