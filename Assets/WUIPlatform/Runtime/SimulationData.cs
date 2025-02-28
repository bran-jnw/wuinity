//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

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


        Vector2d _mercatorToUtmScale;
        public Vector2d MercatorToUtmScale { get => _mercatorToUtmScale; }

        float _mercatorCorrectionScale;
        /// <summary>
        /// As we move away from the equator the mercator projection streteches distances, this is the correction applied by e.g. Mapbox to give reasonable distance assessment and match with UTM etc.
        /// </summary>
        public float MercatorCorrectionScale{ get => _mercatorCorrectionScale; }

        public SimulationData() 
        {
            if(WUIEngine.INPUT != null)
            {
                LatLngUTMConverter.UTMResult utmData = LatLngUTMConverter.WGS84.convertLatLngToUtm(WUIEngine.INPUT.Simulation.LowerLeftLatLon.x, WUIEngine.INPUT.Simulation.LowerLeftLatLon.y);
                _utmOrigin = new Vector2d(utmData.Easting, utmData.Northing);
                _centerMercator = GeoConversions.LatLonToMeters(WUIEngine.INPUT.Simulation.LowerLeftLatLon.x, WUIEngine.INPUT.Simulation.LowerLeftLatLon.y);

                //Calculate scaling factors to correct overlay between web mercator and UTM
                Vector2d mercatorBounds = _centerMercator + WUIEngine.INPUT.Simulation.DomainSize;
                Vector2d wgs84Bounds = GeoConversions.MetersToLatLon(mercatorBounds);
                LatLngUTMConverter.UTMResult utmBoundsData = LatLngUTMConverter.WGS84.convertLatLngToUtm(wgs84Bounds.x, wgs84Bounds.y);
                Vector2d utmBounds = new Vector2d(utmBoundsData.Easting, utmBoundsData.Northing);
                Vector2d utmDistances = utmBounds - _utmOrigin;
                Vector2d realScale;
                realScale.x = utmDistances.x / WUIEngine.INPUT.Simulation.DomainSize.x;
                realScale.y = utmDistances.y / WUIEngine.INPUT.Simulation.DomainSize.y;

                double mercatorCorrectionScale = Mathd.Cos(Mathd.PI * WUIEngine.INPUT.Simulation.LowerLeftLatLon.x / 180.0);
                _mercatorToUtmScale = new Vector2d(realScale.x / mercatorCorrectionScale, realScale.y / mercatorCorrectionScale);

                double lat = Mathd.PI * WUIEngine.INPUT.Simulation.LowerLeftLatLon.x / 180.0;
                _mercatorCorrectionScale = (float)Mathd.Cos(lat);
            }            
        }
    }
}