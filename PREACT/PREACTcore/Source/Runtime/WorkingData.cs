using PREACT.Math;
using PREACT.Population;

namespace PREACT.Runtime
{
    /// <summary>
    /// Holds data that is convenient for any external manager (GUI)
    /// </summary>
    public class WorkingData
    {
        private Input.SimulationInput _simulationInput;
        public LocalGPWData LocalGPWData;
        public PopulationMap PopulationMap;
        public Itinero.RouterDb RouterDb;
        public Input.SimulationInput SimulationInput { get => _simulationInput; }

        public bool HaveSimulationInput { get => SimulationInput != null; }
        public bool HaveLocalGPW { get => LocalGPWData != null; }
        public bool HavePopulationMap { get => PopulationMap != null && PopulationMap.HaveData; }
        public bool PopulationMapCorrectedForRoadAccess { get => PopulationMap != null && PopulationMap.CorrectedForRoadAccess; }
        public bool HaveRouterDb { get => RouterDb == null ? false : true; }

        /// <summary>
        /// By default all resources are null.
        /// </summary>
        public WorkingData()
        {
            _simulationInput = new Input.SimulationInput();
            //_populationMap = new PopulationMap();
        }     
        
        public void SetSimulatonData(Vector2d lowerLeftLatLon, Vector2d domainSize)
        {
            _simulationInput.LowerLeftLatLon = lowerLeftLatLon;
            _simulationInput.DomainSize = domainSize;        
        }
    }
}