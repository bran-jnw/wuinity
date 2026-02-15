using PREACT.Math;

namespace PREACT.Wildfire.AFDRS
{
    public class Buttongrass
    {
        double _fmc;
        double _ros;
        double _fuelLoad;
        double _intensity;
        double _flameHeight;

        int _productivity;

        private const double _b = 17.625f;
        private const double _c = 243.04f;

        public Buttongrass(int productivity)
        {
            _productivity = productivity;
        }

        public void Calculate(double temp, double rh, double tsr, double rain, double windSpeed, double tsf)
        {
            double dew_pt = AFDRS.dewpoint(temp, rh);

            _fmc = FMC_buttongrass(temp, rh, dew_pt, tsr, rain);
            _ros = ROSButtongrass(windSpeed, _fmc, tsf, _productivity);
            _fuelLoad = FuelLoadButtongrass(tsf, _productivity);
            _intensity = IntensityButtongrass(_ros, _fuelLoad);
            _flameHeight = FlameHeightButtongrass(_intensity);
        }

        ///   returns the grass fuel moisture content (%) based on McArthur (1966)
        ///   temp: air temperature (C)
        ///   rh: relative humidity (%)
        ///   tsr: time since rain (h)
        ///   rain: rainfall (mm)       
        ///   dew_pt: dewpoint temperature (c)
        private double FMC_buttongrass(double temp, double rh, double dew_pt, double tsr, double rain)
        {
            return (67.128 * (1 - Mathd.Exp(-3.132 * rain)) * Mathd.Exp(-0.0858 * tsr)) + (Mathd.Exp(1.66 + 0.0214 * rh - 0.0292 * dew_pt));
        }

        /// <summary>
        /// returns the curing coefficient based on Cruz et al. (2015)
        /// tsf: time since fire(y)
        /// productivity:
        /// </summary>
        /// <param name="tsf"></param>
        /// <param name="productivity"></param>
        /// <returns></returns>
        private double FuelLoadButtongrass(double tsf, int productivity)
        {
            double FuelLoadButtongrass;

            if(productivity == 1)
            {
                FuelLoadButtongrass = 11.73f * (1f - Mathd.Exp(-0.106 * tsf));
            }
            else
            {
                FuelLoadButtongrass = 44.61f * (1f - Mathd.Exp(-0.041 * tsf));
            }

            return FuelLoadButtongrass;
        }

        /// <summary>
        /// returns the grass moisture coefficient
        /// U_10: 10 m wind speed(km/h)       
        /// mc: fuel moisture content(%)
        /// productivity:
        /// </summary>
        /// <param name="U_10"></param>
        /// <param name="mc"></param>
        /// <param name="productivity"></param>
        private double SpreadProbButtongrass(double U_10, double mc, int productivity)
        {
            double U_2 = U_10 / 1.2f;
            return 1f / (1f + Mathd.Exp(-(-1 + 0.68 * U_2 - 0.07 * mc - 0.0037 * U_2 * mc + 2.1 * productivity)));
        }

        /// <summary>
        /// returns the forward ROS (m/h) ignoring slope
        /// U_10: 10 m wind speed(km/h)
        /// mc: fuel moisture content(%)
        /// tsf: time since fire(y))
        /// </summary>
        /// <param name="U_10"></param>
        /// <param name="mc"></param>
        /// <param name="tsf"></param>
        /// <param name="productivity"></param>
        /// <returns></returns>
        private  double ROSButtongrass(double U_10, double mc, double tsf, int productivity)
        {
            double spread_prob = SpreadProbButtongrass(U_10, mc, productivity);
            double U_2 = U_10 / 1.2f;

            double ROSButtongrass = 0.678f * Mathd.Pow(U_2, 1.312f) * Mathd.Exp(-0.0243f * mc) * (1 - Mathd.Exp(-0.116f * tsf)) * 60f;
            if (spread_prob <= 0.5)
            {
                ROSButtongrass = 0f;
            }

            return ROSButtongrass;
        }

        /// <summary>
        ///  returns the flame height (m) based on M. Plucinski, pers. comm.
        ///  Intensity(kW/m)
        /// </summary>
        /// <param name="intensity"></param>
        /// <returns></returns>
        private double FlameHeightButtongrass(double intensity)
        {
            return 0.148f * Mathd.Pow(intensity, 0.403f);
        }

        /// <summary>
        /// returns the fireline intensity (kW/m) based on Byram 1959
        /// ROS: forward rate of spread(km/h)
        /// fuel_load: fine fuel load(t/ha)
        /// </summary>
        /// <param name="ROS"></param>
        /// <param name="Double"></param>
        /// <param name="fuel_load"></param>
        /// <param name="Single"></param>
        /// <returns></returns>
        private double IntensityButtongrass(double ROS, double fuel_load)
        {
            ROS = ROS / 3600; // to m/s
            fuel_load = fuel_load / 10f; // to kg/m^2
            return 19900 * ROS * fuel_load;
        }  
    }
}
