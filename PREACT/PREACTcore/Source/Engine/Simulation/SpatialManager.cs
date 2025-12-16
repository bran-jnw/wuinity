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

        public SpatialManager(Simulation simulation)
        {
            _simulation = simulation;
        }

        /// <summary>
        /// Returns the origin offset in meters (UTM coordinate offset compared to origin).
        /// </summary>
        /// <returns></returns>
        public Vector2d GetFireModuleOffset()
        {
            Vector2d result = Vector2d.zero;
            if(_simulation.FireModule != null)
            {
                result = _simulation.FireModule.GetOriginOffset();
            }

            return result;
        }
    }
}

