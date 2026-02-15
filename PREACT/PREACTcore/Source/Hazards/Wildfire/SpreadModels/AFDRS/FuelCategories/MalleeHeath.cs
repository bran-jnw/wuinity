using System;
using PREACT.Math;

namespace PREACT.Wildfire.AFDRS
{
    public class MalleeHeath
    {
        float _overstoreyCover, _overstoreyHeight, _fuelLoadSurface,_fuelLoadCrown;
        float _fmc, _ros, _intensity, _flameHeight;


        public MalleeHeath(float overstoreyCover, float overstoreyHeight, float fuelLoadSurface, float fuelLoadCrown)
        {  
            _overstoreyCover = overstoreyCover;
            _overstoreyHeight = overstoreyHeight;
            _fuelLoadSurface = fuelLoadSurface;
            _fuelLoadCrown = fuelLoadCrown;
        }

        public void Calculate(float air_temperature, float relative_humidity, DateTime dateTime, float precipitation, float time_since_rain, float windSpeed)
        {
            _fmc = FMC_mallee(air_temperature, relative_humidity, dateTime, precipitation, time_since_rain);
            float spreadProbability = spread_prob_mallee(windSpeed, _fmc, _overstoreyCover);
            float crownProbability = crown_prob_mallee(windSpeed, _fmc);
            _ros = ROS_mallee(windSpeed, _fmc, _overstoreyCover, _overstoreyHeight, spreadProbability, crownProbability);
            float fuel_load = fuel_load_mallee(_fuelLoadSurface, _fuelLoadCrown, crownProbability);
            _intensity = intensity(_ros, fuel_load);
            _flameHeight = flame_height_mallee(_intensity);
        }

        /// return fuel moisture content (%). Based on:
        ///   Cruz, M., et al. (2010). Fire dynamics in mallee-heath: fuel, weather
        ///   and fire behaviour prediction in south Australian semi-arid shrublands.
        ///   Bushfire CRC Program A Rep 1(01).
        ///
        /// In addition, a fuel moisture modifier based on recent rainfall was used. Marsden-Smedley, J. B.,
        /// et al. (1999). Buttongrass moorland fire-behaviour prediction
        /// and management. Tasforests 11: 87-107.
        /// Precipitation in mm. Time_since_rain in hours.
        /// args
        ///   air_temperature: air temperature (C)
        ///   relative_humidity: relative humidity (%)
        ///   date_: (underscore due to VBA Date objects)
        ///   time: 24 hour time format
        ///   precipitation: precipitation in the last 48 hours (mm)
        ///   time_since_rain: time since rain or dewfall stopped (h)
        private static float FMC_mallee(float air_temperature, float relative_humidity, DateTime dateTime, float precipitation, float time_since_rain)
        {
            const int start_peak_month = 10; //October
            const int end_peak_month = 3; //March
            const int start_afternoon = 12;
            const int end_afternoon = 17;

            int delta;            
            int months = dateTime.Month;
            int hours = dateTime.Hour;

            if(((months >= start_peak_month) || (months <= end_peak_month)) && (hours >= start_afternoon) && (hours <= end_afternoon))
            {
                delta = 1;
            }
            else
            {
                delta = 0;
            }

            float FMC_mallee = 4.74f + 0.108f * relative_humidity - 0.1f * (air_temperature - 25f) - delta * (1.68f + (0.028f * relative_humidity));

            return FMC_mallee + 67.128f * (1f - Mathf.Exp(-3.132f * precipitation)) * Mathf.Exp(-0.0858f * time_since_rain);
        }

        /// return the likelihood of spread sustainability (go/no-go) [value between 0 and 1].
        /// Based on: Cruz, M. G., et al. (2013). "Fire behaviour modelling in semi-arid
        /// mallee-heath shrublands of southern Australia." Environmental Modelling & Software 40: 21-34.
        ///
        /// args
        ///   wind_speed: 10 m wind speed(km/h)
        ///   fuel_moisture: dead fuel moisture content (%)
        ///   overstorey_cover: (%)
        private static float spread_prob_mallee(float wind_speed, float fuel_moisture, float overstorey_cover)
        {
            return (1f / (1f + Mathf.Exp(-(14.624f + 0.2066f * wind_speed - 1.8719f * fuel_moisture - 0.030442f * overstorey_cover))));
        }

        /// type of fire, i.e. surface fire, crown fire, or an ensemble of the two, based on
        /// crown probability [value between 0 and 1].
        /// Based on: Cruz, M. G., et al. (2013). "Fire behaviour modelling in semi-arid
        /// mallee-heath shrublands of southern Australia." Environmental Modelling & Software 40: 21-34.
        ///
        /// args
        ///   wind_speed: 10 m wind speed(km/h)
        ///   fuel_moisture: dead fuel moisture content (%)
        private static float crown_prob_mallee(float wind_speed, float fuel_moisture)
        {
            return 1f / (1f + Mathf.Exp(-(-11.138f + 1.4054f * wind_speed - 3.4217f * fuel_moisture)));
        }

        /// return rate of spread (m/h) [Range = 0 - 8000].
        /// Based on: Cruz, M. G., et al. (2013). "Fire behaviour modelling in semi-arid
        /// mallee-heath shrublands of southern Australia." Environmental Modelling & Software 40: 21-34.
        ///
        /// args
        ///   wind_speed: 10 m wind speed(km/h)
        ///   fuel_moisture: dead fuel moisture content (%)
        ///   overstorey_cover: (%)
        ///   overstorey_height: (m)
        private static float ROS_mallee(float wind_speed, float fuel_moisture, float overstorey_cover, float overstorey_height, float spread_probability, float crown_probability)
        {
            float ros_surface = 3.337f * wind_speed * Mathf.Exp(-0.1284f * fuel_moisture) * Mathf.Pow(overstorey_height, -0.7073f) * 60f;
            float ros_crown = 9.5751f * wind_speed * Mathf.Exp(-0.1795f * fuel_moisture) * Mathf.Pow((overstorey_cover / 100f), 0.3589f) * 60f;

            float ROS_mallee;

            if (spread_probability < 0.5f)
            {
                ROS_mallee = 0;
            }
            else if (crown_probability <= 0.01)
            {
                ROS_mallee = ros_surface;
            }
            else if (crown_probability > 0.99)
            {
                ROS_mallee = ros_crown;
            }
            else
            {
                ROS_mallee = ros_surface * (1 - crown_probability) + ros_crown * crown_probability;
            }

            return ROS_mallee;
        }

        /// return fuel load based on crown probability.
        /// if fuel loads are know don//t pass the k values as arguments
        /// if k values passed then use exponetial decay model to adjust fuel for age (fuel build-up).
        /// Based on Olson, J. S. (1963). Energy storage and the balance of producers
        /// and decomposers in ecological systems. Ecology, 44(2), 322-331.
        /// Include canopy fuel based on crown_probability (Cruz pers. comm.).
        ///
        /// args
        ///   fuel_load_surface: surface fuel load (t/ha)
        ///   fuel_load_canopy: canopy fuel load (t/ha)
        ///   crown_probability: crown probability %
        private static float fuel_load_mallee(float fuel_load_surface, float fuel_load_canopy, float crown_probability)
        {
            float fuel_load_mallee;

            if (crown_probability <= 0.01f)
            {
                fuel_load_mallee = fuel_load_surface;
            }
            else if(crown_probability > 0.99)
            {
                fuel_load_mallee = fuel_load_surface + fuel_load_canopy;
            }
            else
            {
                fuel_load_mallee = fuel_load_surface + crown_probability * fuel_load_canopy;
            }
            
            return fuel_load_mallee;
        }

        /// returns the fireline intensity (kW/m) based on Byram 1959
        ///   ROS: forward rate of spread (km/h)
        ///   fuel_load: fine fuel load (t/ha)
        public static float intensity(float ROS, float fuel_load)
        {
            // convert units
            ROS = ROS / 3600; // m/s
            fuel_load = fuel_load / 10; //kg/m^2

            return 18600f * ROS * fuel_load;
        }

        private static float flame_height_mallee(float intensity)
        {
            return Mathf.Exp(-4.142f) * Mathf.Pow(intensity, 0.633f);
        }
        
        static readonly int[] fbi_b = { 0, 6, 12, 24, 50, 100 }; //use same fbi bounds, fbi high anchor and intensity high anchor for all classes
        /// returns the AFDRS FBI for mallee
        ///   wind_speed: 10 m wind speed(km/h)
        ///   fuel_moisture: dead fuel moisture content (%)
        ///   overstorey_cover: (%)
        ///   intensity: fire line intensity (kW/m)
        private static int FBI_mallee(float wind_speed, float fuel_moisture, float overstorey_cover, float intensity)
        {
            float intensity_ha; //arbitrary high anchor for intensity
            float param_la, param_ua, fbi_la, fbi_ua; //upper and lower anchors for parameter and fbi
            
            int fbi_ha = 200; //arbitrary high anchor for fbi
            int param_ha = 90000;
            float param;


            float spread_probability = spread_prob_mallee(wind_speed, fuel_moisture, overstorey_cover);
            float crown_probability = crown_prob_mallee(wind_speed, fuel_moisture);

            if(spread_probability < 0.5f) //category 1
            {
                param = spread_probability;
                param_ua = 0.5f;
                param_la = 0f;
                fbi_ua = fbi_b[1];
                fbi_la = fbi_b[0];
            }
            else
            {
                if (crown_probability < 0.33f) //category 2
                {
                    param = crown_probability;
                    param_ua = 0.33f;
                    param_la = 0;
                    fbi_ua = fbi_b[2];
                    fbi_la = fbi_b[1];
                }
                else if(crown_probability >= 0.66)
                {
                    param = intensity;
                    if (intensity < 20000) //category 4
                    {
                        param_ua = 20000f;
                        param_la = 0f;
                        fbi_ua = fbi_b[4];
                        fbi_la = fbi_b[3];
                    }
                    else if (intensity >= 40000f)//category 6
                    {
                        param_ua = param_ha;
                        param_la = 40000f;
                        fbi_ua = fbi_ha;
                        fbi_la = fbi_b[5];
                    }
                    else//category 5
                    {
                        param_ua = 40000f;
                        param_la = 20000f;
                        fbi_ua = fbi_b[5];
                        fbi_la = fbi_b[4];
                    }
                }
                else //category 3
                {
                    param = crown_probability;
                    param_ua = 0.66f;
                    param_la = 0.66f;
                    fbi_ua = fbi_b[3];
                    fbi_la = fbi_b[2];
                }                   
            }

            float FBI_mallee = fbi_la + (fbi_ua - fbi_la) * (param - param_la) / (param_ua - param_la);

            return (int)FBI_mallee; //FBI needs to be truncated for National consistency
        }
    }
}
