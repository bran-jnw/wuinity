using PREACT.Math;

namespace PREACT
{
    /// <summary>
    /// This class is supposed to collect all the communication between different sub-modules, 
    /// e.g. traffic simulation needing information from the smoke or fire simulation.
    /// This is done to not clutter up the simulation class itself.
    /// </summary>
    public class HazardManager
    {
        private Simulation _simulation;

        public HazardManager(Simulation simulation)
        {
            _simulation = simulation;
        }

        /// <summary>
        /// Returns optical density at ground level and location in simulation space.
        /// </summary>
        /// <returns></returns>
        public float GetExtinctionCoefficientAtPos(Vector2d pos)
        {
            float result = 0f;
            if(_simulation.SmokeModule != null)
            {
                result = _simulation.SmokeModule.GetSootDensityAtPos(pos) * 8700f; //TODO: user specified mass specific extinction coefficient
            }

            return result;
        }
    }
}
