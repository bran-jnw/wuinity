using PREACT.Math;

namespace PREACT.Wildfire.AFDRS
{
    public class Buttongrass
    {
        float _fmc;
        float _ros;
        float _fuelLoad;
        float _intensity;
        float _flameHeight;

        int _productivity;

        private const float _b = 17.625f;
        private const float _c = 243.04f;

        public Buttongrass(int productivity)
        {
            _productivity = productivity;
        }

        public void Calculate(float temp, float rh, float tsr, float rain, float windSpeed, float tsf)
        {
            //https://en.wikipedia.org/wiki/Dew_point
            float gamma = Mathf.Log(rh * 0.01f) + _b * temp / (_c + temp); 
            float dew_pt = _c * gamma / (_b - gamma);

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
        private float FMC_buttongrass(float temp, float rh, float dew_pt, float tsr, float rain)
        {
            return (67.128f * (1 - Mathf.Exp(-3.132f * rain)) * Mathf.Exp(-0.0858f * tsr)) + (Mathf.Exp(1.66f + 0.0214f * rh - 0.0292f * dew_pt));
        }

        /// <summary>
        /// returns the curing coefficient based on Cruz et al. (2015)
        /// tsf: time since fire(y)
        /// productivity:
        /// </summary>
        /// <param name="tsf"></param>
        /// <param name="productivity"></param>
        /// <returns></returns>
        private float FuelLoadButtongrass(float tsf, int productivity)
        {
            float FuelLoadButtongrass;

            if(productivity == 1)
            {
                FuelLoadButtongrass = 11.73f * (1f - Mathf.Exp(-0.106f * tsf));
            }
            else
            {
                FuelLoadButtongrass = 44.61f * (1f - Mathf.Exp(-0.041f * tsf));
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
        private float SpreadProbButtongrass(float U_10, float mc, int productivity)
        {
            float U_2 = U_10 / 1.2f;
            return 1f / (1f + Mathf.Exp(-(-1f + 0.68f * U_2 - 0.07f * mc - 0.0037f * U_2 * mc + 2.1f * productivity)));
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
        private  float ROSButtongrass(float U_10, float mc, float tsf, int productivity)
        {
            float spread_prob = SpreadProbButtongrass(U_10, mc, productivity);
            float U_2 = U_10 / 1.2f;

            float ROSButtongrass = 0.678f * Mathf.Pow(U_2, 1.312f) * Mathf.Exp(-0.0243f * mc) * (1 - Mathf.Exp(-0.116f * tsf)) * 60f;
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
        private float FlameHeightButtongrass(float intensity)
        {
            return 0.148f * Mathf.Pow(intensity, 0.403f);
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
        private float IntensityButtongrass(float ROS, float fuel_load)
        {
            ROS = ROS / 3600; // to m/s
            fuel_load = fuel_load / 10f; // to kg/m^2
            return 19900 * ROS * fuel_load;
        }  
    }
}
