using Mapbox.Map;
using WUIPlatform.Utility;

namespace WUIPlatform.Runtime
{
    public class SimulationData
    {
        public bool MultipleSimulations;
        public int NumberOfRuns = 1;
        public int ConvergenceMinSequence = 10;
        public float ConvergenceMaxDifference = 0.02f;

        Vector2d _utmOrigin;
        public Vector2d UTMOrigin { get => _utmOrigin; }

        Vector2d _centerMercator;
        public Vector2d CenterMercator { get => _centerMercator; }

        public SimulationData() 
        {
            LatLngUTMConverter.UTMResult utmData = LatLngUTMConverter.WGS84.convertLatLngToUtm(WUIEngine.INPUT.Simulation.LowerLeftLatLong.x, WUIEngine.INPUT.Simulation.LowerLeftLatLong.y);
            _utmOrigin = new Vector2d(utmData.Easting, utmData.Northing);

            _centerMercator = Conversions.LatLonToMeters(WUIEngine.INPUT.Simulation.LowerLeftLatLong.x, WUIEngine.INPUT.Simulation.LowerLeftLatLong.y);
        }
    }
}