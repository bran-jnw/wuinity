using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity.Fire
{
    public static class MathWrap                        //Define a "wrapper class" that contains helpful math functions (possibly to avoid including the entire of System.math?). Static since we want it to work irrespective of instantiated object. 
    {
        public static double PI = System.Math.PI;
        public static double M_PI = System.Math.PI;

        public static double sqrt(double x)
        {
            return System.Math.Sqrt(x);
        }

        public static double pow2(double x)
        {
            return x * x;
        }

        public static double sin(double x)
        {
            return System.Math.Sin(x);
        }

        public static double cos(double x)
        {
            return System.Math.Cos(x);
        }

        public static double exp(double x)
        {
            return System.Math.Exp(x);
        }

        public static double pow(double x, double p)
        {
            return System.Math.Pow(x, p);
        }

        public static double tan(double x)
        {
            return System.Math.Tan(x);
        }

        public static double ln(double x)
        {
            return System.Math.Log(x);
        }

        public static double sec(double x)
        {
            return 1.0 / System.Math.Cos(x);
        }

        public static double cot(double x)
        {
            return 1.0 / System.Math.Tan(x);
        }

        public static double atan(double x)
        {
            return System.Math.Atan(x);
        }

        public static double log(double x)
        {
            return System.Math.Log(x);
        }

        public static double fabs(double x)
        {
            return System.Math.Abs(x);
        }

        public static double abs(double x)
        {
            return System.Math.Abs(x);
        }

        public static double atan2(double y, double x)
        {
            return System.Math.Atan2(y, x);
        }

        public static double modf(double x, ref double intpart)
        {
            intpart = (int)x; // Integer portion
            return x - intpart;  // Fractional portion
        }

        public static double acos(double x)
        {
            return System.Math.Acos(x);
        }

        public static double asin(double x)
        {
            return System.Math.Asin(x);
        }

        public static double RadToDegrees(double x)
        {
            return x * 180.0 / PI; 
        }
    }
    
}
