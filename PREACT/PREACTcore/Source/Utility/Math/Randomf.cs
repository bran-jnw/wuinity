namespace PREACT.Math
{
    public class Randomf
    {
        private static Random RANDOM = new Random();
        public static float Range(float minInclusive, float maxExlusive)
        {
            return (float)(minInclusive + RANDOM.NextDouble() * (maxExlusive - minInclusive));
        }

        /// <summary>
        /// Will result in overflow if using full int range as input.
        /// </summary>
        /// <param name="minInclusive"></param>
        /// <param name="maxInclusive"></param>
        /// <returns></returns>
        public static int Range(int minInclusive, int maxInclusive)
        {
            //adding 1 as Next has exclusive upper bound
            return RANDOM.Next(minInclusive, maxInclusive + 1);
        }

        /// <summary>
        /// Inlusive lower bound, exclusive upper bound.
        /// </summary>
        public static float value
        {
            get{ return (float)RANDOM.NextDouble(); }
        }
    }
}