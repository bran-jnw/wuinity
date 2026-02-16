using PREACT.Math;

namespace PREACT.Wildfire.AFDRS
{
    public static class AFDRS
    {      
        ///returns the vapour pressure deficit in hPa, calculated using Tetens (1930)
        ///args
        ///  temp: air temperature (C)
        ///  rh: relative humidity (%)
        public static double vp_deficit(double air_temperature, double relative_humidity)
        {
            double es = 610.78 / 1000 * Mathd.Exp((17.269 * air_temperature) / (237.3 + air_temperature));
            double ea = (relative_humidity * es / 100);

            return es - ea;
        }

        /// <summary>
        /// returns the dew point temperature based on the Magnus formula with the the Arden Buck modification
        ///temp: air temperature (C)
        ///rh: relative humidity (%)
        /// </summary>
        /// <param name="temp"></param>
        /// <param name="rh"></param>
        /// <returns></returns>
        public static double dewpoint(double temp, double rh)
        {
            //https://en.wikipedia.org/wiki/Dew_point

            const double a = 6.1121; //hPa
            const double b = 18.678;
            const double c = 257.14;//°C
            const double d = 1 / 234.5;//°C

            double Gamma = Mathd.Log((rh * 0.01) * Mathd.Exp((b - temp * d) * (temp / (c + temp))));
            return c * Gamma / (b - Gamma);
        }
    }
}
