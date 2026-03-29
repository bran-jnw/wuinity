using PREACT.Math;
using PREACT.Utility;

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
        public Vector2d UTMOrigin { get => _simulation.Input.Simulation.Data.UTMOrigin; }
        public LatLngUTMConverter.UTMResult UTMData { get => _simulation.Input.Simulation.Data.UTMData; }

        public SpatialManager(Simulation simulation)
        {
            _simulation = simulation;

            _simulationCenterLatLon = simulation.Input.Simulation.Data.GetWGS84FromSimulationPosition(simulation.Input.Simulation.DomainSize * 0.5);
        }

        public Vector2d GetSimulationPosition(Vector2d latLon)
        {
            return _simulation.Input.Simulation.Data.GetSimulationPosition(latLon);
        }

        public Vector2d GetWGS84FromSimulationPosition(Vector2d pos)
        {
            return _simulation.Input.Simulation.Data.GetWGS84FromSimulationPosition(pos);
        }

        /// <summary>
        /// Returns the origin offset in meters (UTM coordinate offset compared to origin).
        /// </summary>
        /// <returns></returns>
        public Vector2d GetWildfireModuleOffset()
        {
            Vector2d result = Vector2d.zero;
            if(_simulation.Hazards.WildfireModule != null)
            {
                result = _simulation.Hazards.WildfireModule.GetOriginOffset();
            }

            return result;
        }

        public Vector2int GetWildfireCellIndex(Vector2d simulationPos, out bool inside)
        {
            inside = false;

            if (_simulation.Hazards.WildfireModule != null)
            {
                return _simulation.Hazards.WildfireModule.SimulationPosToCellIndex(simulationPos, out inside);
            }

            return Vector2int.zero;
        }
    }
}

