//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using PREACT.Utility;
using PREACT.Math;

namespace PREACT.Input
{
    public class SimulationData
    {       
        Vector2d _utmOrigin;
        LatLngUTMConverter.UTMResult _utmData;
        Vector2d _lowerLeftLatLon;

        public Vector2d UTMOrigin { get => _utmOrigin; }        
        public LatLngUTMConverter.UTMResult UTMData { get => _utmData; }


        public SimulationData(Vector2d lowerLeftLatLon) 
        {
            UpdateData(lowerLeftLatLon);
        }

        public void UpdateData(string lat, string lon, out bool success)
        {
            Vector2d latLon;
            if (double.TryParse(lat, out latLon.x) && double.TryParse(lon, out latLon.y))
            {
                success = true;
                UpdateData(latLon);
            }
            else
            {
                success = false;
                Engine.Message(null, Engine.LogType.InputError, "Could not parse latitude/longitude.");
            }
        }

        public void UpdateData(Vector2d lowerLeftLatLon)
        {
            _lowerLeftLatLon = lowerLeftLatLon;
            _utmData = LatLngUTMConverter.WGS84.convertLatLngToUtm(lowerLeftLatLon.x, lowerLeftLatLon.y);
            _utmOrigin = new Vector2d(_utmData.Easting, _utmData.Northing);
        }

        public Vector2d GetSimulationPosition(Vector2d latLon)
        {
            LatLngUTMConverter.UTMResult utmPos = LatLngUTMConverter.WGS84.convertLatLngToUtm(latLon.x, latLon.y);
            if(utmPos.ZoneNumber != _utmData.ZoneNumber)
            {
                int utmZone = GetUtmZone(_lowerLeftLatLon.y);
                string utmEPSG = GetUtmEpsg(_lowerLeftLatLon.x, _lowerLeftLatLon.y);
                (double easting, double northing) eastNorth = Wgs84ToUtm(latLon.x, latLon.y, utmEPSG);
                return new Vector2d(eastNorth.easting, eastNorth.northing) - _utmOrigin;
            }
            else
            {
                return new Vector2d(utmPos.Easting, utmPos.Northing) - _utmOrigin;
            }            
        }
        private static int GetUtmZone(double longitude)
        {
            return (int)Mathd.Floor((longitude + 180.0) / 6.0) + 1;
        }

        private static string GetUtmEpsg(double latitude, double longitude)
        {
            int zone = GetUtmZone(longitude);
            return latitude >= 0
                ? "EPSG:" + (32600 + zone)   // North
                : "EPSG:" + (32700 + zone);  // South
        }

        private static (double easting, double northing) Wgs84ToUtm(double lat, double lon, string utmEpsg)
        {
            var src = new OSGeo.OSR.SpatialReference("");
            src.ImportFromEPSG(4326); // WGS84

            var dst = new OSGeo.OSR.SpatialReference("");
            dst.ImportFromEPSG(int.Parse(utmEpsg.Replace("EPSG:", "")));

            var transform = new OSGeo.OSR.CoordinateTransformation(src, dst);

            double[] point = { lat, lon, 0 }; //be careful, some versions of GDAL uses different order
            transform.TransformPoint(point);

            return (point[0], point[1]); // easting, northing
        }


        public Vector2d GetWGS84FromSimulationPosition(Vector2d pos)
        {
            pos += _utmOrigin;
            LatLngUTMConverter.LatLng wgs84 = LatLngUTMConverter.WGS84.convertUtmToLatLng(pos.x, pos.y, _utmData.ZoneNumber, _utmData.ZoneLetter);
            return new Vector2d(wgs84.Lat, wgs84.Lng);
        }

        public Vector2d GetWGS84FromUTMPosition(Vector2d pos)
        {
            LatLngUTMConverter.LatLng wgs84 = LatLngUTMConverter.WGS84.convertUtmToLatLng(pos.x, pos.y, _utmData.ZoneNumber, _utmData.ZoneLetter);
            return new Vector2d(wgs84.Lat, wgs84.Lng);
        }
    }
}