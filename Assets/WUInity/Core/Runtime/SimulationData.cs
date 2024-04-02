using WUInity.Utility;

namespace WUInity.Runtime
{
    public class SimulationData
    {
        public bool MultipleSimulations;
        public int NumberOfRuns = 100;
        public int ConvergenceMinSequence = 10;
        public float ConvergenceMaxDifference = 0.02f;

        Vector2d utmOrigin;
        public Vector2d UTMOrigin { get => utmOrigin; }

        public SimulationData() 
        {
            LatLngUTMConverter.UTMResult utmData = LatLngUTMConverter.WGS84.convertLatLngToUtm(WUInity.INPUT.Simulation.LowerLeftLatLong.x, WUInity.INPUT.Simulation.LowerLeftLatLong.y);
            utmOrigin = new Vector2d(utmData.Easting, utmData.Northing);
        }
    }
}