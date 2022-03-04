using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity
{
    public class CatmullRomSpline1D
    {
        private struct Segment1D
        {
            public Vector2 a;
            public Vector2 b;
            public Vector2 c;
            public Vector2 d;
            public float maxX;
            public float minX;
            public float range;
        }
        Segment1D[] segments;

        /// <summary>
        /// Create Catmull-Rom spline based on at least two points. Y-value of ends are assumed to be the same value as first/last value while X-value is +- 1 (to approximate clamping).
        /// When alpha = 0.5 the curve is a centripetal variant and when alpha = 1, the result is a chordal variant.
        /// </summary>
        /// <param name="points">X value is the time/distance variation in FRACTION, Y is the 1D coordinate</param>
        /// <param name="alpha"></param>
        /// <param name="tension"></param>
        public CatmullRomSpline1D(Vector2[] points, float alpha = 0.5f, float tension = 0.0f)
        {
            if(points == null || points.Length < 2)
            {
                throw new System.ArgumentException("Need at least two points!");
            }

            segments = new Segment1D[points.Length - 1];

            //don't do last point since segment is based on going from that point, we are only interested in going towards that point
            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector2 p0, p1, p2, p3;
                GetPoints(i, points, out p0, out p1, out p2, out p3);
                
                float t0 = 0.0f;
                float t1 = t0 + Mathf.Pow(Vector2.Distance(p0, p1), alpha);
                float t2 = t1 + Mathf.Pow(Vector2.Distance(p1, p2), alpha);
                float t3 = t2 + Mathf.Pow(Vector2.Distance(p2, p3), alpha);

                Vector2 m1 = (1.0f - tension) * (t2 - t1) * ((p1 - p0) / (t1 - t0) - (p2 - p0) / (t2 - t0) + (p2 - p1) / (t2 - t1));
                Vector2 m2 = (1.0f - tension) * (t2 - t1) * ((p2 - p1) / (t2 - t1) - (p3 - p1) / (t3 - t1) + (p3 - p2) / (t3 - t2));

                /*//We can get the same result slightly more efficiently by simplifying the equations and using the following code to calculate m1 and m2:
                float t01 = pow(distance(p0, p1), alpha);
                float t12 = pow(distance(p1, p2), alpha);
                float t23 = pow(distance(p2, p3), alpha);

                Vector2 m1 = (1.0f - tension) * (p2 - p1 + t12 * ((p1 - p0) / t01 - (p2 - p0) / (t01 + t12)));
                Vector2 m2 = (1.0f - tension) * (p2 - p1 + t12 * ((p3 - p2) / t23 - (p3 - p1) / (t12 + t23)));*/

                Segment1D segment;
                segment.a = 2.0f * (p1 - p2) + m1 + m2;
                segment.b = -3.0f * (p1 - p2) - m1 - m1 - m2;
                segment.c = m1;
                segment.d = p1;

                segment.minX = p1.x;
                segment.maxX = p2.x;
                segment.range = segment.maxX - segment.minX;
                segments[i] = segment;
            }
        }

        private void GetPoints(int i, Vector2[] points, out Vector2 p0, out Vector2 p1, out Vector2 p2, out Vector2 p3)
        {
            if (i - 1 < 0)
            {
                p0 = points[i] - Vector2.right;                
            }
            else
            {
                p0 = points[i - 1];
            }

            p1 = points[i];
            p2 = points[i + 1];

            if (i + 2 > points.Length - 1)
            {
                p3 = points[i + 1] + Vector2.right;
            }
            else
            {
                p3 = points[i + 2];
            }
        }

        /// <summary>
        /// Called when user want an Y value based on X placement, usually wanting a Y value at time or fractinal distance X.
        /// </summary>
        /// <param name="xValue"></param>
        /// <returns></returns>
        public float GetYValue(float xValue)
        {
            //get last point on series for clamping upper end
            Vector2 point = segments[segments.Length - 1].a + segments[segments.Length - 1].b + segments[segments.Length - 1].c + segments[segments.Length - 1].d;
            for (int i = 0; i < segments.Length; i++)
            {
                if (xValue <= segments[i].maxX)
                {
                    float t = (xValue - segments[i].minX) / segments[i].range;
                    t = Mathf.Clamp01(t);
                    point = segments[i].a * t * t * t + segments[i].b * t * t + segments[i].c * t + segments[i].d;
                    break;
                }
            }

            return point.y;
        }
    }

    public class CatmullRomSpline2D
    {
        private struct Segment2D
        {
            public Vector3 a;
            public Vector3 b;
            public Vector3 c;
            public Vector3 d;
            public float maxX;
            public float minX;
            public float range;
        }
        Segment2D[] segments;

        /// <summary>
        /// Create Catmull-Rom spline based on at least two points. Y-value of ends are assumed to be the same value as first/last value while X-value is +- 1 (to approximate clamping).
        /// When alpha = 0.5 the curve is a centripetal variant and when alpha = 1, the result is a chordal variant.
        /// </summary>
        /// <param name="points">X value is the time/distance variation in FRACTION, Y and Z are the 2D coordinates</param>
        /// <param name="alpha"></param>
        /// <param name="tension"></param>
        public CatmullRomSpline2D(Vector3[] points, float alpha = 0.5f, float tension = 0.0f)
        {
            if (points == null || points.Length < 2)
            {
                throw new System.ArgumentException("Need at least two points!");
            }

            segments = new Segment2D[points.Length - 1];

            //don't do last point since segment is based on going from that point, we are only interested in going towards that point
            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector3 p0, p1, p2, p3;
                GetPoints(i, points, out p0, out p1, out p2, out p3);

                float t0 = 0.0f;
                float t1 = t0 + Mathf.Pow(Vector2.Distance(p0, p1), alpha);
                float t2 = t1 + Mathf.Pow(Vector2.Distance(p1, p2), alpha);
                float t3 = t2 + Mathf.Pow(Vector2.Distance(p2, p3), alpha);

                Vector3 m1 = (1.0f - tension) * (t2 - t1) * ((p1 - p0) / (t1 - t0) - (p2 - p0) / (t2 - t0) + (p2 - p1) / (t2 - t1));
                Vector3 m2 = (1.0f - tension) * (t2 - t1) * ((p2 - p1) / (t2 - t1) - (p3 - p1) / (t3 - t1) + (p3 - p2) / (t3 - t2));

                /*//We can get the same result slightly more efficiently by simplifying the equations and using the following code to calculate m1 and m2:
                float t01 = pow(distance(p0, p1), alpha);
                float t12 = pow(distance(p1, p2), alpha);
                float t23 = pow(distance(p2, p3), alpha);

                Vector2 m1 = (1.0f - tension) * (p2 - p1 + t12 * ((p1 - p0) / t01 - (p2 - p0) / (t01 + t12)));
                Vector2 m2 = (1.0f - tension) * (p2 - p1 + t12 * ((p3 - p2) / t23 - (p3 - p1) / (t12 + t23)));*/

                Segment2D segment;
                segment.a = 2.0f * (p1 - p2) + m1 + m2;
                segment.b = -3.0f * (p1 - p2) - m1 - m1 - m2;
                segment.c = m1;
                segment.d = p1;

                segment.minX = p1.x;
                segment.maxX = p2.x;
                segment.range = segment.maxX - segment.minX;
                segments[i] = segment;
            }
        }

        private void GetPoints(int i, Vector3[] points, out Vector3 p0, out Vector3 p1, out Vector3 p2, out Vector3 p3)
        {
            if (i - 1 < 0)
            {
                p0 = points[i] - Vector3.right;
            }
            else
            {
                p0 = points[i - 1];
            }

            p1 = points[i];
            p2 = points[i + 1];

            if (i + 2 > points.Length - 1)
            {
                p3 = points[i + 1] + Vector3.right;
            }
            else
            {
                p3 = points[i + 2];
            }
        }

        /// <summary>
        /// Called when user want an point YZ value based on X value, usually wanting a YZ value at time or fractinal distance X.
        /// </summary>
        /// <param name="xValue"></param>
        /// <returns></returns>
        public Vector2 GetYZValue(float xValue)
        {
            //get last point on series for clamping upper end
            Vector3 point = segments[segments.Length - 1].a + segments[segments.Length - 1].b + segments[segments.Length - 1].c + segments[segments.Length - 1].d;
            for (int i = 0; i < segments.Length; i++)
            {
                if (xValue <= segments[i].maxX)
                {
                    float t = (xValue - segments[i].minX) / segments[i].range;
                    t = Mathf.Clamp01(t);
                    point = segments[i].a * t * t * t + segments[i].b * t * t + segments[i].c * t + segments[i].d;
                    break;
                }
            }

            return new Vector2(point.y, point.z);
        }
    }
}

