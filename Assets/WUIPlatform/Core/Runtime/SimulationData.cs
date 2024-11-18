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

        public SimulationData() 
        {
            if(WUIEngine.INPUT != null)
            {
                LatLngUTMConverter.UTMResult utmData = LatLngUTMConverter.WGS84.convertLatLngToUtm(WUIEngine.INPUT.Simulation.LowerLeftLatLong.x, WUIEngine.INPUT.Simulation.LowerLeftLatLong.y);
                _utmOrigin = new Vector2d(utmData.Easting, utmData.Northing);
                _centerMercator = GeoConversions.LatLonToMeters(WUIEngine.INPUT.Simulation.LowerLeftLatLong.x, WUIEngine.INPUT.Simulation.LowerLeftLatLong.y);
            }            
        }
    }
}