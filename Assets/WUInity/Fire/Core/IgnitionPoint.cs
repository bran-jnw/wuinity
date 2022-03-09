using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity.Fire
{
    [System.Serializable]                           
    public class IgnitionPoint
    {
        public Vector2D latLong;                    
        public float startTime;                     

        private int x;
        private int y;

        public int GetX()                           
        {
            return x;
        }

        public int GetY()
        {
            return y;
        }

        /// <summary>
        /// Only used for testing, creates ignition point directly on mesh
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public IgnitionPoint(int x, int y)    
        {
            this.x = x;
            this.y = y;
        }

        public IgnitionPoint()                
        {
            x = -1;
            y = -1;
        }

        public IgnitionPoint(Vector2D latLong)    
        {
            this.latLong = latLong;
            x = -1;
            y = -1;
        }

        /// <summary>
        /// Used when creating something dynamically during runtime.
        /// </summary>
        /// <param name="latLong"></param>
        /// <param name="mesh"></param>
        public IgnitionPoint(Vector2D latLong, FireMesh mesh)      
        {
            this.latLong = latLong;

            Mapbox.Utils.Vector2d pos = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(latLong.x, latLong.y, WUInity.MAP.CenterMercator, WUInity.MAP.WorldRelativeScale);

            x = (int)(pos.x / mesh.cellSize.x);
            y = (int)(pos.y / mesh.cellSize.y);
        }

        /// <summary>
        /// Called when starting fire since we only specify lat/long in input file
        /// </summary>
        /// <param name="mesh"></param>
        public void CalculateMeshIndex(FireMesh mesh)        
        {
            if(x < 0 && y < 0)
            {
                Mapbox.Utils.Vector2d pos = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(latLong.x, latLong.y, WUInity.MAP.CenterMercator, WUInity.MAP.WorldRelativeScale);

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

        public static IgnitionPoint[] GetDefault()           
        {
            IgnitionPoint[] ignitions = new IgnitionPoint[1];                   
            ignitions[0] = new IgnitionPoint(new Vector2D(39.479633, -105.037355)); 
            return ignitions;
        }
    }
}
