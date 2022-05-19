using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace WUInity.Fire
{
    [System.Serializable]                           
    public struct IgnitionPoint
    {
        public Vector2D LatLong;                    
        public float IgnitionTime;                     

        private int x;
        private int y;

        private bool _hasBeenIgnited;

        public int GetX()                           
        {
            return x;
        }

        public int GetY()
        {
            return y;
        }

        public bool HasBeenIgnited()
        {
            return _hasBeenIgnited;
        }

        public void MarkAsIgnited()
        {
            _hasBeenIgnited = true;
        }

        /// <summary>
        /// Only used for testing, creates ignition point directly on mesh
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public IgnitionPoint(int x, int y, float ignitionTime)    
        {
            this.x = x;
            this.y = y;
            this.IgnitionTime = ignitionTime;

            LatLong = Vector2D.zero;

            _hasBeenIgnited = false;
        }

        public IgnitionPoint(Vector2D latLong, float ignitionTime)    
        {
            this.LatLong = latLong;
            x = -1;
            y = -1;
            this.IgnitionTime = ignitionTime;

            _hasBeenIgnited = false;
        }

        /// <summary>
        /// Used when creating something dynamically during runtime.
        /// </summary>
        /// <param name="latLong"></param>
        /// <param name="mesh"></param>
        public IgnitionPoint(Vector2D latLong, FireMesh mesh, float ignitionTime)      
        {
            this.LatLong = latLong;

            Mapbox.Utils.Vector2d pos = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(latLong.x, latLong.y, WUInity.MAP.CenterMercator, WUInity.MAP.WorldRelativeScale);

            x = (int)(pos.x / mesh.cellSize.x);
            y = (int)(pos.y / mesh.cellSize.y);

            this.IgnitionTime = ignitionTime;

            _hasBeenIgnited = false;
        }

        /// <summary>
        /// Called when starting fire since we only specify lat/long in input file
        /// </summary>
        /// <param name="mesh"></param>
        public void CalculateMeshIndex(FireMesh mesh)        
        {
            if(x < 0 && y < 0)
            {
                Mapbox.Utils.Vector2d pos = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(LatLong.x, LatLong.y, WUInity.MAP.CenterMercator, WUInity.MAP.WorldRelativeScale);

                x = (int)(pos.x / mesh.cellSize.x);
                y = (int)(pos.y / mesh.cellSize.y);
            }            
        }

        public bool IsInsideFire(Vector2Int cells)                  
        {
            if (x >= 0 && x < cells.x && y >= 0 && y < cells.y)     
            {
                return true;
            }
            return false;
        }

        /*public static IgnitionPoint[] GetDefault()           
        {
            IgnitionPoint[] ignitions = new IgnitionPoint[1];                   
            ignitions[0] = new IgnitionPoint(new Vector2D(39.479633, -105.037355), 0.0f); 
            return ignitions;
        }*/

        /// <summary>
        /// Tries to load ignition points froma file defined in the general input file.
        /// Returns an array with anu loaded ignition points, othwerwise returns null.
        /// Sends message to the WUI_LOG to inform the user.
        /// </summary>
        /// <returns></returns>
        public static IgnitionPoint[] LoadIgnitionPointsFile()
        {
            IgnitionPoint[] result = null;
            List<IgnitionPoint> ignitionPoints= new List<IgnitionPoint>();

            string path = Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.fire.ignitionPointsFile + ".ign");
            bool fileExists = File.Exists(path);
            if (fileExists)
            {
                string[] dataLines = File.ReadAllLines(path);
                //skip first line (header)
                for (int j = 1; j < dataLines.Length; j++)
                {
                    string[] data = dataLines[j].Split(',');
                    if (data.Length >= 3)
                    {
                        double lati, longi;
                        float ignitionTime;

                        bool b1 = double.TryParse(data[0], out lati);
                        bool b2 = double.TryParse(data[1], out longi);
                        bool b3 = float.TryParse(data[2], out ignitionTime);

                        if (b1 && b2 && b3)
                        {
                            IgnitionPoint iP = new IgnitionPoint(new Vector2D(lati, longi), ignitionTime);
                            ignitionPoints.Add(iP);
                        }
                    }
                }
            }
            else
            {
                WUInity.WUI_LOG("WARNING: Ignition points data file " + path + " not found and could not be loaded, fire and smoke spread will have to rely on other ignition methods (painted map).");
            }

            if (ignitionPoints.Count > 0)
            {
                result = ignitionPoints.ToArray();
                WUInity.WUI_LOG("LOG: Ignition points data file " + path + " was found, " + ignitionPoints.Count + " valid data points were succesfully loaded.");
            }
            else if (fileExists)
            {
                WUInity.WUI_LOG("WARNING: Ignition points data file " + path + " was found but did not contain any valid data, fire and smoke spread will have to rely on other ignition methods (painted map).");
            }

            return result;
        }
    }
}
