using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity.Fire
{
    [System.Serializable]
    public struct WindData                  //Wind data struct
    {
        public float time;
        public float direction;
        public float speed;
        public float cloudCover;

        public WindData(float time, float direction, float speed, float cloudCover)     //wind data struct constructor
        {
            this.time = time;
            this.direction = direction;
            this.speed = speed;
            this.cloudCover = cloudCover;
        }
    }
    [System.Serializable]
    public class WindInput
    {
        [SerializeField] private WindData[] dataPoints;         //declare datapoints array of winddata structs

        private CatmullRomSpline1D directionSpline;               //declare three spline variables (unity)
        private CatmullRomSpline1D speedSpline;
        private CatmullRomSpline1D cloudSpline;

        public WindInput(WindData[] dataPoints)                 //CONSTRUCTOR
        {
            this.dataPoints = dataPoints;
        }

        public WindData GetWindDataAtTime(float time)           //Get data at specified time
        {
            if(dataPoints.Length > 1 && (directionSpline == null || speedSpline == null || cloudSpline == null))
            {
                CreateSplines();    //if there is at least one data point and any of the spline values are empty, initiate them.
            }

            WindData w = dataPoints[0];     //get first weather point
            w.time = time;                  //overwrite the first weather point time data as the target time (Maybe it is being left empty??)
            if(dataPoints.Length > 1)       //if there are more than 1 data points, get the rest of the data of the specified time and rewrite them as the first point data.
            {
                w.direction = directionSpline.GetYValue(time);
                w.speed = speedSpline.GetYValue(time) * WUInity.INPUT.fire.windMultiplier;
                w.cloudCover = cloudSpline.GetYValue(time);
            }

            return w;
        }

        private void CreateSplines()                                                    //Interpolate weather data
        {
            Vector2[] dir = new Vector2[dataPoints.Length];                             //Create 2D Vector arrays to store the time and wind direction of each node
            for (int i = 0; i < dir.Length; i++)
            {
                dir[i] = new Vector2(dataPoints[i].time, dataPoints[i].direction);
            }
            directionSpline = new CatmullRomSpline1D(dir);                                //this function uses a super special unity method to make a continuous curve based on some points

            Vector2[] speed = new Vector2[dataPoints.Length];                           //Create 2D Vector arrays to store the time and windspeed of each node
            for (int i = 0; i < speed.Length; i++)
            {
                speed[i] = new Vector2(dataPoints[i].time, dataPoints[i].speed);
            }
            speedSpline = new CatmullRomSpline1D(speed);

            Vector2[] cloud = new Vector2[dataPoints.Length];                           //Create 2D Vector arrays to store the time and cloud cover of each node
            for (int i = 0; i < cloud.Length; i++)
            {
                cloud[i] = new Vector2(dataPoints[i].time, dataPoints[i].cloudCover);
            }
            cloudSpline = new CatmullRomSpline1D(cloud);
        }

        public static WindInput GetTemplate()                                           //get some standard input values (guessing it is for testing purposes)
        {
            WindData[] w = new WindData[2];
            w[0] = new WindData(0f, 0f, 2f, 0f);
            w[1] = w[0];
            WindInput wI = new WindInput(w);
            return wI;
        }
    }
}
