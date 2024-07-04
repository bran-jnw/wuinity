/* Contains a portion of modified Mapbox Unity SDK
- Original by Mapbox https://github.com/mapbox/mapbox-unity-sdk
- Modified by Jonathan Wahlqvist

The MIT License (MIT)

Copyright (c) 2016 - 2017 Mapbox
Copyright (c) Jonathan Wahlqvist 2024

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE. */

using System.Numerics;

namespace WUIPlatform
{
    public static class VectorExtensions
    {
        /// <summary>
        /// Vector2 convenience method to convert Vector2 to Vector3.
        /// </summary>
        /// <returns>Vector3 with a y value of zero.</returns>
        /// <param name="v">Vector2.</param>
        public static Vector3 ToVector3xz(this Vector2 v)
        {
            return new Vector3(v.X, 0, v.Y);
        }

        /// <summary>
        /// Vector2d convenience method to convert Vector2d to Vector3.
        /// </summary>
        /// <returns>Vector3 with a y value of zero.</returns>
        /// <param name="v">Vector2d.</param>
        public static Vector3 ToVector3xz(this Vector2d v)
        {
            return new Vector3((float)v.x, 0, (float)v.y);
        }

        /// <summary>
        /// Vector3 convenience method to convert Vector3 to Vector2.
        /// </summary>
        /// <returns>The Vector2.</returns>
        /// <param name="v">Vector3.</param>
        public static Vector2 ToVector2xz(this Vector3 v)
        {
            return new Vector2(v.X, v.Z);
        }

        /// <summary>
        /// Vector3 convenience method to convert Vector3 to Vector2d.
        /// </summary>
        /// <returns>The Vector2d.</returns>
        /// <param name="v">Vector3.</param>
        public static Vector2d ToVector2d(this Vector3 v)
        {
            return new Vector2d(v.X, v.Z);
        }


        public static Vector3 Perpendicular(this Vector3 v)
        {
            return new Vector3(-v.Z, v.Y, v.X);
        }

        /*/// <summary>
        /// Transform extension method to move a Unity transform to a specific latitude/longitude.
        /// </summary>
        /// <param name="t">Transform.</param>
        /// <param name="lat">Latitude.</param>
        /// <param name="lng">Longitude.</param>
        /// <param name="refPoint">Reference point.</param>
        /// <param name="scale">Scale.</param>
        /// <example>
        /// Place a Unity transform at 10, 10, with a map center of (0, 0) and scale 1:
        /// <code>
        /// transform.MoveToGeocoordinate(10, 10, new Vector2d(0, 0), 1f);
        /// Debug.Log(transform.position);
        /// // (1113195.0, 0.0, 1118890.0)
        /// </code>
        /// </example>
        public static void MoveToGeocoordinate(this Transform t, double lat, double lng, Vector2d refPoint, float scale = 1)
        {
            t.position = Conversions.GeoToWorldPosition(lat, lng, refPoint, scale).ToVector3xz();
        }*/

        /*/// <summary>
        /// Transform extension method to move a Unity transform to a specific Vector2d.
        /// </summary>
        /// <param name="t">Transform.</param>
        /// <param name="latLon">Latitude Longitude.</param>
        /// <param name="refPoint">Reference point.</param>
        /// <param name="scale">Scale.</param>
        /// <example>
        /// Place a Unity transform at 10, 10, with a map center of (0, 0) and scale 1:
        /// <code>
        /// transform.MoveToGeocoordinate(new Vector2d(10, 10), new Vector2d(0, 0), 1f);
        /// Debug.Log(transform.position);
        /// // (1113195.0, 0.0, 1118890.0)
        /// </code>
        /// </example>
        public static void MoveToGeocoordinate(this Transform t, Vector2d latLon, Vector2d refPoint, float scale = 1)
        {
            t.MoveToGeocoordinate(latLon.x, latLon.y, refPoint, scale);
        }*/

        /// <summary>
        /// Vector2 extension method to convert from a latitude/longitude to a Unity Vector3.
        /// </summary>
        /// <returns>The Vector3 Unity position.</returns>
        /// <param name="latLon">Latitude Longitude.</param>
        /// <param name="refPoint">Reference point.</param>
        /// <param name="scale">Scale.</param>
        public static Vector3 AsUnityPosition(this Vector2 latLon, Vector2d refPoint, float scale = 1)
        {
            return GeoConversions.GeoToWorldPosition(latLon.X, latLon.Y, refPoint, scale).ToVector3xz();
        }

        /*/// <summary>
        /// Transform extension method to return the transform's position as a Vector2d latitude/longitude.
        /// </summary>
        /// <returns>Vector2d that represents latitude/longitude.</returns>
        /// <param name="t">T.</param>
        /// <param name="refPoint">Reference point.</param>
        /// <param name="scale">Scale.</param>
        /// <example>
        /// Get the latitude/longitude of a transform at (1113195, 0, 1118890), map center (0, 0) and scale 1.
        /// <code>
        /// var latLng = transform.GetGeoPosition(new Vector2d(0, 0), 1);
        /// Debug.Log(latLng);
        /// // (10.00000, 10.00000)
        /// </code>
        /// </example>
        public static Vector2d GetGeoPosition(this Transform t, Vector2d refPoint, float scale = 1)
        {
            var pos = refPoint + (t.position / scale).ToVector2d();
            return Conversions.MetersToLatLon(pos);
        }*/

        public static Vector2d GetGeoPosition(this Vector3 position, Vector2d refPoint, float scale = 1)
        {
            var pos = refPoint + (position / scale).ToVector2d();
            return GeoConversions.MetersToLatLon(pos);
        }

        public static Vector2d GetGeoPosition(this Vector2 position, Vector2d refPoint, float scale = 1)
        {
            return position.ToVector3xz().GetGeoPosition(refPoint, scale);
        }

        /*public static Vector2d GetGeoPosition(this Vector3d position, Vector2d refPoint, float scale = 1)
        {
            var pos = refPoint + (position / scale).ToVector2d();
            return Conversions.MetersToLatLon(pos);
        }*/

        public static Vector2d GetGeoPosition(this Vector2d position, Vector2d refPoint, float scale = 1)
        {
            return position.ToVector3xz().GetGeoPosition(refPoint, scale);
        }
    }
}
