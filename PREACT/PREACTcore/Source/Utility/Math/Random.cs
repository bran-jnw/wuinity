namespace PREACT.Math
{
    public class Random
    {
        private static System.Random RANDOM = new System.Random();
        public static float Range(float minInclusive, float maxExlusive)
        {
            return (float)(minInclusive + RANDOM.NextDouble() * (maxExlusive - minInclusive));
        }

        public static double Range(double minInclusive, double maxExlusive)
        {
            return minInclusive + RANDOM.NextDouble() * (maxExlusive - minInclusive);
        }

        /// <summary>
        /// Will result in overflow if using full int range as input.
        /// </summary>
        /// <param name="minInclusive"></param>
        /// <param name="maxInclusive"></param>
        /// <returns></returns>
        public static int Range(int minInclusive, int maxExclusive)
        {
            //adding 1 as Next has exclusive upper bound
            return RANDOM.Next(minInclusive, maxExclusive);
        }

        /// <summary>
        /// Random float from 0 (inclusive) to 1 (exclusive).
        /// </summary>
        public static float valuef
        {
            get{ return (float)RANDOM.NextDouble(); }
        }

        /// <summary>
        /// Random double from 0 (inclusive) to 1 (exclusive).
        /// </summary>
        public static double valued
        {
            get { return RANDOM.NextDouble(); }
        }
    }
}