using System;
using System.Numerics;

namespace WUInity
{  
    /// <summary>
    /// A set of Geo and Terrain Conversion utils.
    /// </summary>
    public static class Conversions
    {
        private const int TileSize = 256;
        /// <summary>according to https://wiki.openstreetmap.org/wiki/Zoom_levels</summary>
        private const int EarthRadius = 6378137; //no seams with globe example
        private const double InitialResolution = 2 * Math.PI * EarthRadius / TileSize;
        private const double OriginShift = 2 * Math.PI * EarthRadius / 2;

        /// <summary>
        /// Converts <see cref="Vector2d"/> struct, WGS84
        /// lat/lon to Spherical Mercator EPSG:900913 xy meters.
        /// </summary>
        /// <param name="v"> The <see cref="Vector2d"/>. </param>
        /// <returns> A <see cref="T:Vector2d"/> of coordinates in meters. </returns>
        public static Vector2d LatLonToMeters(Vector2d v)
        {
            return LatLonToMeters(v.x, v.y);
        }

        /// <summary>
        /// Converts WGS84 lat/lon to Spherical Mercator EPSG:900913 xy meters.
        /// SOURCE: http://stackoverflow.com/questions/12896139/geographic-coordinates-converter.
        /// </summary>
        /// <param name="lat"> The latitude. </param>
        /// <param name="lon"> The longitude. </param>
        /// <returns> A <see cref="T:Vector2d"/> of xy meters. </returns>
        public static Vector2d LatLonToMeters(double lat, double lon)
        {
            var posx = lon * OriginShift / 180;
            var posy = Math.Log(Math.Tan((90 + lat) * Math.PI / 360)) / (Math.PI / 180);
            posy = posy * OriginShift / 180;
            return new Vector2d(posx, posy);
        }

        /// <summary>
        /// Converts WGS84 lat/lon to x/y meters in reference to a center point
        /// </summary>
        /// <param name="lat"> The latitude. </param>
        /// <param name="lon"> The longitude. </param>
        /// <param name="refPoint"> A <see cref="T:Vector2d"/> center point to offset resultant xy, this is usually map's center mercator</param>
        /// <param name="scale"> Scale in meters. (default scale = 1) </param>
        /// <returns> A <see cref="T:Vector2d"/> xy tile ID. </returns>
        /// <example>
        /// Converts a Lat/Lon of (37.7749, 122.4194) into Unity coordinates for a map centered at (10,10) and a scale of 2.5 meters for every 1 Unity unit 
        /// <code>
        /// var worldPosition = Conversions.GeoToWorldPosition(37.7749, 122.4194, new Vector2d(10, 10), (float)2.5);
        /// // worldPosition = ( 11369163.38585, 34069138.17805 )
        /// </code>
        /// </example>
        public static Vector2d GeoToWorldPosition(double lat, double lon, Vector2d refPoint, float scale = 1)
        {
            var posx = lon * OriginShift / 180;
            var posy = Math.Log(Math.Tan((90 + lat) * Math.PI / 360)) / (Math.PI / 180);
            posy = posy * OriginShift / 180;
            return new Vector2d((posx - refPoint.x) * scale, (posy - refPoint.y) * scale);
        }

        public static Vector2d GeoToWorldPosition(double lat, double lon, Mapbox.Utils.Vector2d refPoint, float scale = 1)
        {
            var posx = lon * OriginShift / 180;
            var posy = Math.Log(Math.Tan((90 + lat) * Math.PI / 360)) / (Math.PI / 180);
            posy = posy * OriginShift / 180;
            return new Vector2d((posx - refPoint.x) * scale, (posy - refPoint.y) * scale);
        }
        
        public static Vector2d GeoToWorldPosition(Vector2d latLong, Vector2d refPoint, float scale = 1)
        {
            return GeoToWorldPosition(latLong.x, latLong.y, refPoint, scale);
        }

        public static Vector3 GeoToWorldGlobePosition(double lat, double lon, float radius)
        {
            double xPos = (radius) * Math.Cos(Mathf.Deg2Rad * lat) * Math.Cos(Mathf.Deg2Rad * lon);
            double zPos = (radius) * Math.Cos(Mathf.Deg2Rad * lat) * Math.Sin(Mathf.Deg2Rad * lon);
            double yPos = (radius) * Math.Sin(Mathf.Deg2Rad * lat);

            return new Vector3((float)xPos, (float)yPos, (float)zPos);
        }

        public static Vector3 GeoToWorldGlobePosition(Vector2d latLong, float radius)
        {
            return GeoToWorldGlobePosition(latLong.x, latLong.y, radius);
        }

        public static Vector2d GeoFromGlobePosition(Vector3 point, float radius)
        {
            float latitude = Mathf.Asin(point.Y / radius);
            float longitude = Mathf.Atan2(point.Z, point.X);
            return new Vector2d(latitude * Mathf.Rad2Deg, longitude * Mathf.Rad2Deg);
        }

        /// <summary>
        /// Converts Spherical Mercator EPSG:900913 in xy meters to WGS84 lat/lon.
        /// Inverse of LatLonToMeters.
        /// </summary>
        /// <param name="m"> A <see cref="T:Vector2d"/> of coordinates in meters.  </param>
        /// <returns> The <see cref="Vector2d"/> in lat/lon. </returns>

        /// <example>
        /// Converts EPSG:900913 xy meter coordinates to lat lon 
        /// <code>
        /// var worldPosition =  new Vector2d (4547675.35434,13627665.27122);
        /// var latlon = Conversions.MetersToLatLon(worldPosition);
        /// // latlon = ( 37.77490, 122.41940 )
        /// </code>
        /// </example>
        public static Vector2d MetersToLatLon(Vector2d m)
        {
            var vx = (m.x / OriginShift) * 180;
            var vy = (m.y / OriginShift) * 180;
            vy = 180 / Math.PI * (2 * Math.Atan(Math.Exp(vy * Math.PI / 180)) - Math.PI / 2);
            return new Vector2d(vy, vx);
        }

        /// <summary>
        /// Gets the xy tile ID from Spherical Mercator EPSG:900913 xy coords.
        /// </summary>
        /// <param name="m"> <see cref="T:Vector2d"/> XY coords in meters. </param>
        /// <param name="zoom"> Zoom level. </param>
        /// <returns> A <see cref="T:Vector2d"/> xy tile ID. </returns>
        /// 
        /// <example>
        /// Converts EPSG:900913 xy meter coordinates to web mercator tile XY coordinates at zoom 12.
        /// <code>
        /// var meterXYPosition = new Vector2d (4547675.35434,13627665.27122);
        /// var tileXY = Conversions.MetersToTile (meterXYPosition, 12);
        /// // tileXY = ( 655, 2512 )
        /// </code>
        /// </example>
        public static Vector2 MetersToTile(Vector2d m, int zoom)
        {
            var p = MetersToPixels(m, zoom);
            return PixelsToTile(p);
        }
          
        /// <summary>
        /// Gets the WGS84 longitude of the northwest corner from a tile's X position and zoom level.
        /// See: http://wiki.openstreetmap.org/wiki/Slippy_map_tilenames.
        /// </summary>
        /// <param name="x"> Tile X position. </param>
        /// <param name="zoom"> Zoom level. </param>
        /// <returns> NW Longitude. </returns>
        public static double TileXToNWLongitude(int x, int zoom)
        {
            var n = Math.Pow(2.0, zoom);
            var lon_deg = x / n * 360.0 - 180.0;
            return lon_deg;
        }

        /// <summary>
        /// Gets the WGS84 latitude of the northwest corner from a tile's Y position and zoom level.
        /// See: http://wiki.openstreetmap.org/wiki/Slippy_map_tilenames.
        /// </summary>
        /// <param name="y"> Tile Y position. </param>
        /// <param name="zoom"> Zoom level. </param>
        /// <returns> NW Latitude. </returns>
        public static double TileYToNWLatitude(int y, int zoom)
        {
            var n = Math.Pow(2.0, zoom);
            var lat_rad = Math.Atan(Math.Sinh(Math.PI * (1 - 2 * y / n)));
            var lat_deg = lat_rad * 180.0 / Math.PI;
            return lat_deg;
        }

        /// <summary>
        /// Gets the meters per pixels at given latitude and zoom level for a 256x256 tile.
        /// See: http://wiki.openstreetmap.org/wiki/Zoom_levels.
        /// </summary>
        /// <param name="latitude"> The latitude. </param>
        /// <param name="zoom"> Zoom level. </param>
        /// <returns> Meters per pixel. </returns>
        public static float GetTileScaleInMeters(float latitude, int zoom)
        {
            return (float)(40075016.685578d * Math.Cos(Mathf.Deg2Rad * latitude) / Math.Pow(2f, zoom + 8));
        }

        /// <summary>
        /// Gets the degrees per tile at given zoom level for Web Mercator tile.
        /// See: http://wiki.openstreetmap.org/wiki/Zoom_levels.
        /// </summary>
        /// <param name="latitude"> The latitude. </param>
        /// <param name="zoom"> Zoom level. </param>
        /// <returns> Degrees per tile. </returns>
        public static float GetTileScaleInDegrees(float latitude, int zoom)
        {
            return (float)(360.0f / Math.Pow(2f, zoom + 8));
        }

        /*/// <summary>
        /// Gets height from terrain-rgb adjusted for a given scale.
        /// </summary>
        /// <param name="color"> The <see cref="T:UnityEngine.Color"/>. </param>
        /// <param name="relativeScale"> Relative scale. </param>
        /// <returns> Adjusted height in meters. </returns>
        public static float GetRelativeHeightFromColor(Color color, float relativeScale)
        {
            return GetAbsoluteHeightFromColor(color) * relativeScale;
        }

        /// <summary>
        /// Specific formula for mapbox.terrain-rgb to decode height values from pixel values.
        /// See: https://www.mapbox.com/blog/terrain-rgb/.
        /// </summary>
        /// <param name="color"> The <see cref="T:UnityEngine.Color"/>. </param>
        /// <returns> Height in meters. </returns>
        public static float GetAbsoluteHeightFromColor(Color color)
        {
            return (float)(-10000 + ((color.r * 255 * 256 * 256 + color.g * 255 * 256 + color.b * 255) * 0.1));
        }

        public static float GetAbsoluteHeightFromColor32(Color32 color)
        {
            return (float)(-10000 + ((color.r * 256 * 256 + color.g * 256 + color.b) * 0.1));
        }*/

        public static float GetAbsoluteHeightFromColor(float r, float g, float b)
        {
            return (float)(-10000 + ((r * 256 * 256 + g * 256 + b) * 0.1));
        }

        private static double Resolution(int zoom)
        {
            return InitialResolution / Math.Pow(2, zoom);
        }

        private static Vector2d PixelsToMeters(Vector2d p, int zoom)
        {
            var res = Resolution(zoom);
            var met = new Vector2d();
            met.x = (p.x * res - OriginShift);
            met.y = -(p.y * res - OriginShift);
            return met;
        }

        private static Vector2d MetersToPixels(Vector2d m, int zoom)
        {
            var res = Resolution(zoom);
            var pix = new Vector2d(((m.x + OriginShift) / res), ((-m.y + OriginShift) / res));
            return pix;
        }

        private static Vector2 PixelsToTile(Vector2d p)
        {
            var t = new Vector2((int)Math.Ceiling(p.x / (double)TileSize) - 1, (int)Math.Ceiling(p.y / (double)TileSize) - 1);
            return t;
        }
    }
}
