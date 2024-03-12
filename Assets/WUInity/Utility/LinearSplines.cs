using System.Collections.Generic;
using System.Numerics;

namespace WUInity
{
    public class LinearSpline1D
    {
        Vector2[] points;
        public float maxX;
        public float minX;
        public float range;

        /// <summary>
        /// Create linear spline based on at least two points. Y-value of ends are assumed to be the same value as first/last value while X-value is +- 1 (to approximate clamping).
        /// When alpha = 0.5 the curve is a centripetal variant and when alpha = 1, the result is a chordal variant.
        /// </summary>
        /// <param name="points">X value is the time/distance variation in absolute value, Y is the 1D coordinate</param>
        /// <param name="alpha"></param>
        /// <param name="tension"></param>
        public LinearSpline1D(Vector2[] points)
        {
            this.points = points;

            minX = points[0].X;
            maxX = points[points.Length - 1].X;
            range = maxX - minX;
        }

        /// <summary>
        /// Create Catmull-Rom spline based on at least two points. Y-value of ends are assumed to be the same value as first/last value while X-value is +- 1 (to approximate clamping).
        /// When alpha = 0.5 the curve is a centripetal variant and when alpha = 1, the result is a chordal variant.
        /// </summary>
        /// <param name="points">X value is the time/distance variation in absolute value, Y is the 1D coordinate</param>
        /// <param name="alpha"></param>
        /// <param name="tension"></param>
        public LinearSpline1D(List<Vector2> points)
        {
            this.points = new Vector2[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                this.points[i] = points[i];
            }

            minX = points[0].X;
            maxX = points[this.points.Length - 1].X;
            range = maxX - minX;
        }

        /// <summary>
        /// Called when user want an Y value based on X placement, usually wanting a Y value at time or distance X.
        /// </summary>
        /// <param name="xValue"></param>
        /// <returns></returns>
        public float GetYValue(float xValue)
        {
            Vector2 value = Vector2.Zero;

            if(xValue <= minX)
            {
                value = points[0];
            }
            else if(xValue >= maxX)
            {
                value = points[points.Length - 1];
            }
            else
            {
                for (int i = 1; i < points.Length; i++)
                {
                    if (xValue <= points[i].X)
                    {
                        Vector2 p1 = points[i - 1];
                        Vector2 p2 = points[i];
                        float fraction = (xValue - p1.X) / (p2.X - p1.X);
                        value = Vector2.Lerp(p1, p2, fraction);
                        break;
                    }
                }
            }            

            return value.Y;
        }
    }

    public class LinearSpline2D
    {
        Vector3[] points;
        public float maxX;
        public float minX;
        public float range;

        /// <summary>
        /// Create Catmull-Rom spline based on at least two points. Y-value of ends are assumed to be the same value as first/last value while X-value is +- 1 (to approximate clamping).
        /// When alpha = 0.5 the curve is a centripetal variant and when alpha = 1, the result is a chordal variant.
        /// </summary>
        /// <param name="points">X value is the time/distance variation in absolute value, Y is the 1D coordinate</param>
        /// <param name="alpha"></param>
        /// <param name="tension"></param>
        public LinearSpline2D(Vector3[] points)
        {
            this.points = points;

            minX = points[0].X;
            maxX = points[points.Length - 1].X;
            range = maxX - minX;
        }

        /// <summary>
        /// Called when user want an YZ point based on X placement (time/distance), usually wanting a Y value at time or distance X.
        /// </summary>
        /// <param name="xValue"></param>
        /// <returns></returns>
        public Vector2 GetYZValue(float xValue)
        {
            Vector3 value = Vector3.Zero;

            if (xValue <= minX)
            {
                value = points[0];
            }
            else if (xValue >= maxX)
            {
                value = points[points.Length - 1];
            }
            else
            {
                for (int i = 1; i < points.Length; i++)
                {
                    if (xValue <= points[i].X)
                    {
                        Vector3 p1 = points[i - 1];
                        Vector3 p2 = points[i];
                        float fraction = (xValue - p1.X) / (p2.X - p1.X);
                        value = Vector3.Lerp(p1, p2, fraction);
                        break;
                    }
                }
            }

            return new Vector2(value.Y, value.Z);
        }
    }
}

