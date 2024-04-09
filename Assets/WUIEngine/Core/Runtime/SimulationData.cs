using WUIEngine.Utility;

namespace WUIEngine.Runtime
{
    public class SimulationData
    {
        public bool MultipleSimulations;
        public int NumberOfRuns = 1;
        public int ConvergenceMinSequence = 10;
        public float ConvergenceMaxDifference = 0.02f;

        Vector2d utmOrigin;
        public Vector2d UTMOrigin { get => utmOrigin; }

        public SimulationData() 
        {
            LatLngUTMConverter.UTMResult utmData = LatLngUTMConverter.WGS84.convertLatLngToUtm(Engine.INPUT.Simulation.LowerLeftLatLong.x, Engine.INPUT.Simulation.LowerLeftLatLong.y);
            utmOrigin = new Vector2d(utmData.Easting, utmData.Northing);
        }
    }
}