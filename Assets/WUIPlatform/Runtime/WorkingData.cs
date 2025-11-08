using PREACT.Population;
using PREACT.Utility.Math;

namespace PREACT.Runtime
{
    /// <summary>
    /// Holds data that is convenient for any external manager (GUI)
    /// </summary>
    public class WorkingData
    {
        private IO.SimulationData _simulationData;
        private PopulationMap _populationMap;

        public LocalGPWData LocalGPWData;
        public PopulationMap PopulationMap { get => _populationMap; }
        public Itinero.RouterDb RouterDb;
        public IO.SimulationData SimulationData { get => _simulationData; }

        public bool HaveLocalGPW { get => LocalGPWData != null; }
        public bool HavePopulationMap { get => PopulationMap.HaveData; }
        public bool PopulationMapCorrectedForRoadAccess { get => PopulationMap.CorrectedForRoadAccess; }
        public bool HaveRouterDb { get => RouterDb == null ? false : true; }

        /// <summary>
        /// By default all resources are null.
        /// </summary>
        public WorkingData()
        {
            _simulationData = new IO.SimulationData(Vector2d.zero);
            _populationMap = new PopulationMap();
        }        
    }
}