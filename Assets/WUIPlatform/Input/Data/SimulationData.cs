//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using PREACT.Utility;
using PREACT.Utility.Math;
using PREACT.IO;

namespace PREACT.IO
{
    public class SimulationData
    {       
        Vector2d _utmOrigin;
        LatLngUTMConverter.UTMResult _utmData;

        public Vector2d UTMOrigin { get => _utmOrigin; }        
        public LatLngUTMConverter.UTMResult UTMData { get => _utmData; }


        public SimulationData(Vector2d lowerLeftLatLon) 
        {
            UpdateData(lowerLeftLatLon);
        }

        public void UpdateData(string lat, string lon)
        {
            Vector2d latLon;
            if (double.TryParse(lat, out latLon.x) && double.TryParse(lon, out latLon.y))
            {
                _utmData = LatLngUTMConverter.WGS84.convertLatLngToUtm(latLon.x, latLon.y);
                _utmOrigin = new Vector2d(_utmData.Easting, _utmData.Northing);
            }            
        }

        public void UpdateData(Vector2d lowerLeftLatLon)
        {
            _utmData = LatLngUTMConverter.WGS84.convertLatLngToUtm(lowerLeftLatLon.x, lowerLeftLatLon.y);
            _utmOrigin = new Vector2d(_utmData.Easting, _utmData.Northing);
        }

        public Vector2d GetSimulationPosition(Vector2d latLon)
        {
            LatLngUTMConverter.UTMResult utmPos = LatLngUTMConverter.WGS84.convertLatLngToUtm(latLon.x, latLon.y);
            return new Vector2d(utmPos.Easting, utmPos.Northing) - _utmOrigin;
        }

        public Vector2d GetWGS84FromSimulationPosition(Vector2d pos)
        {
            pos += _utmOrigin;
            LatLngUTMConverter.LatLng wgs84 = LatLngUTMConverter.WGS84.convertUtmToLatLng(pos.x, pos.y, _utmData.ZoneNumber, _utmData.ZoneLetter);
            return new Vector2d(wgs84.Lat, wgs84.Lng);
        }
    }
}