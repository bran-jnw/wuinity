using System;
using PREACT.Math;

namespace PREACT.Wildfire.AFDRS
{
    public static class MalleeHeath
    {
        ///   air_temperature: air temperature (C)
        ///   relative_humidity: relative humidity (%)
        ///   dateTime: 24 hour time format
        ///   precipitation: precipitation in the last 48 hours (mm)
        ///   time_since_rain: time since rain or dewfall stopped (h)
        public static AFDRSOutput Calculate(double air_temperature, double relative_humidity, DateTime dateTime, double precipitation, double time_since_rain, double U_10, double percentSlope, double windAzimuth, double slopeAzimuth, double overstoreyCover, double overstoreyHeight, double fuelLoadSurface, double fuelLoadCrown)
        {
            double fmc = FMC_mallee(air_temperature, relative_humidity, dateTime, precipitation, time_since_rain);
            double spreadProbability = spread_prob_mallee(U_10, fmc, overstoreyCover);
            double crownProbability = crown_prob_mallee(U_10, fmc);

            double noWindNoSlopeROS = ROS_mallee(0, fmc, overstoreyCover, overstoreyHeight, spreadProbability, crownProbability);
            double slopeROS = noWindNoSlopeROS * SpreadModelAFDRS.SlopeFactor(percentSlope);
            double windROS = ROS_mallee(U_10, fmc, overstoreyCover, overstoreyHeight, spreadProbability, crownProbability);

            //calculate final values
            SpreadModelAFDRS.CalculateDirectionOfMaxSpread(windAzimuth, slopeAzimuth, noWindNoSlopeROS, windROS, slopeROS, out double ros, out double direction);
            double fuel_load = fuel_load_mallee(fuelLoadSurface, fuelLoadCrown, crownProbability);
            double intensity = IntensityMaleeHeath(ros, fuel_load);
            double flameHeight = flame_height_mallee(intensity);

            return new AFDRSOutput(fmc, ros, direction, intensity, flameHeight);
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
        private static double FMC_mallee(double air_temperature, double relative_humidity, DateTime dateTime, double precipitation, double time_since_rain)
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

            double FMC_mallee = 4.74 + 0.108 * relative_humidity - 0.1 * (air_temperature - 25) - delta * (1.68 + (0.028 * relative_humidity));

            return FMC_mallee + 67.128 * (1 - Mathd.Exp(-3.132 * precipitation)) * Mathd.Exp(-0.0858 * time_since_rain);
        }

        /// return the likelihood of spread sustainability (go/no-go) [value between 0 and 1].
        /// Based on: Cruz, M. G., et al. (2013). "Fire behaviour modelling in semi-arid
        /// mallee-heath shrublands of southern Australia." Environmental Modelling & Software 40: 21-34.
        ///
        /// args
        ///   wind_speed: 10 m wind speed(km/h)
        ///   fuel_moisture: dead fuel moisture content (%)
        ///   overstorey_cover: (%)
        private static double spread_prob_mallee(double wind_speed, double fuel_moisture, double overstorey_cover)
        {
            return (1f / (1 + Mathd.Exp(-(14.624 + 0.2066 * wind_speed - 1.8719 * fuel_moisture - 0.030442 * overstorey_cover))));
        }

        /// type of fire, i.e. surface fire, crown fire, or an ensemble of the two, based on
        /// crown probability [value between 0 and 1].
        /// Based on: Cruz, M. G., et al. (2013). "Fire behaviour modelling in semi-arid
        /// mallee-heath shrublands of southern Australia." Environmental Modelling & Software 40: 21-34.
        ///
        /// args
        ///   wind_speed: 10 m wind speed(km/h)
        ///   fuel_moisture: dead fuel moisture content (%)
        private static double crown_prob_mallee(double wind_speed, double fuel_moisture)
        {
            return 1f / (1 + Mathd.Exp(-(-11.138 + 1.4054 * wind_speed - 3.4217 * fuel_moisture)));
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
        private static double ROS_mallee(double wind_speed, double fuel_moisture, double overstorey_cover, double overstorey_height, double spread_probability, double crown_probability)
        {
            double ros_surface = 3.337 * wind_speed * Mathd.Exp(-0.1284 * fuel_moisture) * Mathd.Pow(overstorey_height, -0.7073) * 60;
            double ros_crown = 9.5751 * wind_speed * Mathd.Exp(-0.1795 * fuel_moisture) * Mathd.Pow((overstorey_cover / 100), 0.3589) * 60;

            double ROS_mallee;

            if (spread_probability < 0.5)
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
        private static double fuel_load_mallee(double fuel_load_surface, double fuel_load_canopy, double crown_probability)
        {
            double fuel_load_mallee;

            if (crown_probability <= 0.01)
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
        public static double IntensityMaleeHeath(double ROS, double fuel_load)
        {
            // convert units
            ROS = ROS / 3600; // m/s
            fuel_load = fuel_load / 10; //kg/m^2

            return 18600 * ROS * fuel_load;
        }

        private static double flame_height_mallee(double intensity)
        {
            return Mathd.Exp(-4.142f) * Mathd.Pow(intensity, 0.633f);
        }
        
        static readonly int[] fbi_b = { 0, 6, 12, 24, 50, 100 }; //use same fbi bounds, fbi high anchor and intensity high anchor for all classes
        /// returns the AFDRS FBI for mallee
        ///   wind_speed: 10 m wind speed(km/h)
        ///   fuel_moisture: dead fuel moisture content (%)
        ///   overstorey_cover: (%)
        ///   intensity: fire line intensity (kW/m)
        private static int FBI_mallee(double wind_speed, double fuel_moisture, double overstorey_cover, double intensity)
        {
            double intensity_ha; //arbitrary high anchor for intensity
            double param_la, param_ua, fbi_la, fbi_ua; //upper and lower anchors for parameter and fbi
            
            int fbi_ha = 200; //arbitrary high anchor for fbi
            int param_ha = 90000;
            double param;


            double spread_probability = spread_prob_mallee(wind_speed, fuel_moisture, overstorey_cover);
            double crown_probability = crown_prob_mallee(wind_speed, fuel_moisture);

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

            double FBI_mallee = fbi_la + (fbi_ua - fbi_la) * (param - param_la) / (param_ua - param_la);

            return (int)FBI_mallee; //FBI needs to be truncated for National consistency
        }
    }
}
