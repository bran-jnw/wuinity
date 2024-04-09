using static WUIEngine.Fire.MathWrap;

namespace WUIEngine
{
    public static class InterpolationLibrary
    {
        /// <summary>
        /// Only works in one dimension as in 2D it reverts to being linear.
        /// </summary>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        /// <param name="fraction"></param>
        /// <returns></returns>
        /// 
        //http://paulbourke.net/miscellaneous/interpolation/#:~:text=Often%20a%20smoother%20interpolating%20function,smooth%20transition%20between%20adjacent%20segments.&text=Cubic%20interpolation%20is%20the%20simplest,true%20continuity%20between%20the%20segments.
        public static double CosineInterpolate(double y1, double y2, double fraction)
        {
            double frac;
            //range fom 0-1 over half circle
            frac = (1.0 - cos(fraction * PI)) / 2.0;

            //return weighted value between y1 and y2 based on fraction
            return (y1 * (1 - frac) + y2 * frac);
        }
    }
}

