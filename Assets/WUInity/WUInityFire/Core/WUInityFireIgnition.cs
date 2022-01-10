using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity.Fire
{
    [System.Serializable]                           //Enable parallel processing (??)
    public class WUInityFireIgnition
    {
        public Vector2D latLong;                    //Declare latLong value that is of type Vector 2D
        public float startTime;                     //Declare variable to keep track of ignition start time

        private int x;
        private int y;

        public int GetX()                           //Write two getter functions for X and Y. They are public while the variables themselves are private so you can only call the get function externally
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
        public WUInityFireIgnition(int x, int y)    //CONSTRUCTOR: Used to instantiate an object of type WUInityFireIgnition where the input variable X is set to the value of parameter X, and similar for Y)
        {
            this.x = x;
            this.y = y;
        }

        public WUInityFireIgnition()                //CONSTRUCTOR OVERLOADING: if no input is given, do this
        {
            x = -1;
            y = -1;
        }

        public WUInityFireIgnition(Vector2D latLong)    //MORE CONSTRUCTOR OVERLOADING: if the input is given is a vector2D, do this. 
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
        public WUInityFireIgnition(Vector2D latLong, WUInityFireMesh mesh)      //EVEN MORE CONSTRUCTOR OVERLOADING: if a vector and a WUInityFireMesh is given (i.e. the thing is already running) do this
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
        public void CalculateMeshIndex(WUInityFireMesh mesh)        //Method to calculate the x and y values when not explicitly given from the above constructors.
        {
            if(x < 0 && y < 0)
            {
                Mapbox.Utils.Vector2d pos = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(latLong.x, latLong.y, WUInity.WUINITY_MAP.CenterMercator, WUInity.WUINITY_MAP.WorldRelativeScale);

                x = (int)(pos.x / mesh.cellSize.x);
                y = (int)(pos.y / mesh.cellSize.y);
            }            
        }

        public bool IsInsideFire(Vector2Int cells)                  //function to check if the ignition point is inside the domain / fire (depends on what cells represents)
        {
            if (x >= 0 && x < cells.x && y >= 0 && y < cells.y)     //(I changed this to simplify and take off the inbetween variable)
            {
                return true;
            }
            return false;
        }

        public static WUInityFireIgnition[] GetDefault()            //Method to create a standard WUInityFireIgnition object (probably set to roxborough) (no clue why this is an array of this type though)
        {
            WUInityFireIgnition[] ignitions = new WUInityFireIgnition[1];                   //Declare a new array of type WUInityFireIgnition called ignitions of length 1. 
            ignitions[0] = new WUInityFireIgnition(new Vector2D(39.479633, -105.037355));   //The first element of this array will be a new WUInityFireIgnition object, called by the appropriate constructor (thus, what will be set is the latlong, x and y values of that object)
            return ignitions;
        }
    }
}
