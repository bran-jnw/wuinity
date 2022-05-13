using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity
{
    public static class FunctionalAnalysis
    {
        public enum DimensionScalingMode { Max, Min, Average };

        public static float[] CalculateAverageCurve(List<List<float>> curves, DimensionScalingMode scalingMode)
        {      
            if(curves.Count == 1)
            {
                return curves[0].ToArray();
            }

            //get data for re-scaling dimension
            int maxDimension = int.MinValue;
            int minDimension = int.MinValue;
            int averageDimension = 0;
            for (int i = 0; i < curves.Count; i++)
            {
                maxDimension = Mathf.Max(maxDimension, curves[i].Count);
                minDimension = Mathf.Min(minDimension, curves[i].Count);
                averageDimension += curves[i].Count;
            }
            averageDimension = Mathf.RoundToInt((float)averageDimension / curves.Count);

            int desiredDimensions = averageDimension;
            if(scalingMode == DimensionScalingMode.Max)
            {
                desiredDimensions = maxDimension;
            }
            else if(scalingMode == DimensionScalingMode.Min)
            {
                desiredDimensions = minDimension;
            }

            //correct dimensionality
            float[][] newCurves = new float[curves.Count][];
            for (int i = 0; i < curves.Count; i++)
            {
                newCurves[i] = ScaleDimension(curves[i], desiredDimensions);
            }

            //finally calculate average
            float[] result = new float[desiredDimensions];
            for (int i = 0; i < desiredDimensions; i++)
            {
                float average = 0;
                for (int j = 0; j < curves.Count; j++)
                {
                    average += newCurves[j][i]; 
                }
                average /= curves.Count;
                result[i] = average;
            }

            return result;
        }

        public static float[] ScaleDimension(List<float> data, int desiredDimension)
        {
            if(data.Count == desiredDimension)
            {
                return data.ToArray();
            }
            //how big are the jumps in the data set
            float sampleStep = (float)data.Count / desiredDimension;
            //save first and last value as they are known
            float[] result = new float[desiredDimension];
            result[0] = data[0];
            result[desiredDimension - 1] = data[data.Count - 1];
            //interpolate the rest of the values
            for (int i = 1; i < desiredDimension - 1; i++)
            {
                float sample = sampleStep * i;
                float fraction = sample - (int)sample;
                result[i] = fraction * data[(int)sample + 1] + data[(int)sample];
            }

            return result;
        }
    }
}