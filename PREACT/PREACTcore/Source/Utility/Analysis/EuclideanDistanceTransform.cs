using static System.Math;
using System.Threading.Tasks;

namespace PREACT.Utility.Analysis
{
    public static class EuclideanDistanceTransform
    {
        /// <summary>
        /// Anything larger than threshold inside input is set as distance 0. input and outputDistance must have same dimensions.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static void ComputeEDT(float[,] input, float[,] outputDistance, float dx, float dy, float threshold = 0.0f)
        {
            int h = input.GetLength(0);
            int w = input.GetLength(1);

            //start with inside burnt area is zero distance, outside max distance
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    outputDistance[y, x] = (input[y, x] > threshold) ? 0f : float.MaxValue;
                }                    
            }

            float[] buffer_d = new float[Max(h, w)];
            float[] buffer_f = new float[Max(h, w)];

            //Transform along columns
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++) buffer_f[y] = outputDistance[y, x];
                EDT1D(buffer_f, buffer_d, h);
                for (int y = 0; y < h; y++) outputDistance[y, x] = buffer_d[y];
            }

            //Transform along rows
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++) buffer_f[x] = outputDistance[y, x];
                EDT1D(buffer_f, buffer_d, w);
                for (int x = 0; x < w; x++) outputDistance[y, x] = (float)Sqrt(buffer_d[x]);
            }
        }

        // 1‑D exact squared Euclidean distance transform (Felzenszwalb & Huttenlocher algorithm)
        private static void EDT1D(float[] f, float[] d, int n)
        {
            int[] v = new int[n];
            float[] z = new float[n + 1];

            int k = 0;
            v[0] = 0;
            z[0] = float.NegativeInfinity;
            z[1] = float.PositiveInfinity;

            for (int q = 1; q < n; q++)
            {
                float s;
                do
                {
                    int p = v[k];
                    s = ((f[q] + q * q) - (f[p] + p * p)) / (2f * (q - p));
                    if (s <= z[k]) k--;
                }
                while (s <= z[k]);

                k++;
                v[k] = q;
                z[k] = s;
                z[k + 1] = float.PositiveInfinity;
            }

            int kk = 0;
            for (int q = 0; q < n; q++)
            {
                while (z[kk + 1] < q) kk++;
                int p = v[kk];
                d[q] = (q - p) * (q - p) + f[p];
            }
        }
    }
}
