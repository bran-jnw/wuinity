using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity.Fire
{
    [System.Serializable]
    public struct WindData
    {
        public float time;
        public float direction;
        public float speed;
        public float cloudCover;

        public WindData(float time, float direction, float speed, float cloudCover)
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
        [SerializeField] private WindData[] dataPoints;

        private CatmullRomSpline directionSpline;
        private CatmullRomSpline speedSpline;
        private CatmullRomSpline cloudSpline;

        public WindInput(WindData[] dataPoints)
        {
            this.dataPoints = dataPoints;
        }

        public WindData GetWindDataAtTime(float time)
        {
            if(dataPoints.Length > 1 && (directionSpline == null || speedSpline == null || cloudSpline == null))
            {
                CreateSplines();
            }

            WindData w = dataPoints[0];
            w.time = time;
            if(dataPoints.Length > 1)
            {
                w.direction = directionSpline.GetYValue(time);
                w.speed = speedSpline.GetYValue(time);
                w.cloudCover = cloudSpline.GetYValue(time);
            }
            
            return w;
        }

        private void CreateSplines()
        {
            Vector2[] dir = new Vector2[dataPoints.Length];
            for (int i = 0; i < dir.Length; i++)
            {
                dir[i] = new Vector2(dataPoints[i].time, dataPoints[i].direction);
            }
            directionSpline = new CatmullRomSpline(dir);

            Vector2[] speed = new Vector2[dataPoints.Length];
            for (int i = 0; i < speed.Length; i++)
            {
                speed[i] = new Vector2(dataPoints[i].time, dataPoints[i].speed);
            }
            speedSpline = new CatmullRomSpline(speed);

            Vector2[] cloud = new Vector2[dataPoints.Length];
            for (int i = 0; i < cloud.Length; i++)
            {
                cloud[i] = new Vector2(dataPoints[i].time, dataPoints[i].cloudCover);
            }
            cloudSpline = new CatmullRomSpline(cloud);
        }

        public static WindInput GetTemplate()
        {
            WindData[] w = new WindData[2];
            w[0] = new WindData(0f, 0f, 2f, 0f);
            w[1] = w[0];
            WindInput wI = new WindInput(w);
            return wI;
        }
    }
}
