using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity.Fire
{
    [System.Serializable]
    public class WUInityFireIgnition
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
        public WUInityFireIgnition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public WUInityFireIgnition()
        {
            x = -1;
            y = -1;
        }

        public WUInityFireIgnition(Vector2D latLong)
        {
            this.latLong = latLong;
            x = -1;
            y = -1;
        }

        /// <summary>
        /// Used when creating something dynamically suring runtime.
        /// </summary>
        /// <param name="latLong"></param>
        /// <param name="mesh"></param>
        public WUInityFireIgnition(Vector2D latLong, WUInityFireMesh mesh)
        {
            this.latLong = latLong;

            Mapbox.Utils.Vector2d pos = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(latLong.x, latLong.y, WUInity.WUINITY_MAP.CenterMercator, WUInity.WUINITY_MAP.WorldRelativeScale);

            x = (int)(pos.x / mesh.cellSize.x);
            y = (int)(pos.y / mesh.cellSize.y);
        }

        /// <summary>
        /// Called when starting fire since we only specify lat/long in input file
        /// </summary>
        /// <param name="mesh"></param>
        public void CalculateMeshIndex(WUInityFireMesh mesh)
        {
            if(x < 0 && y < 0)
            {
                Mapbox.Utils.Vector2d pos = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(latLong.x, latLong.y, WUInity.WUINITY_MAP.CenterMercator, WUInity.WUINITY_MAP.WorldRelativeScale);

                x = (int)(pos.x / mesh.cellSize.x);
                y = (int)(pos.y / mesh.cellSize.y);
            }            
        }

        public bool IsInsideFire(Vector2Int cells)
        {
            bool inside = false;

            if (x >= 0 && x < cells.x && y >= 0 && y < cells.y)
            {
                inside = true;
            }

            return inside;
        }

        public static WUInityFireIgnition[] GetDefault()
        {
            WUInityFireIgnition[] ignitions = new WUInityFireIgnition[1];
            ignitions[0] = new WUInityFireIgnition(new Vector2D(39.479633, -105.037355));
            return ignitions;
        }
    }
}
