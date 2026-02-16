using System;
using PREACT.Math;

namespace PREACT.Wildfire.AFDRS
{
    public class Spinifex
    {
        public enum FuelSubTypes { Spinifex, SpinifexWoodland };

        private const double HEAT_CONTENT = 16700f; ////KJ/kg Malcolm Possell pers comm.
        private const double KGSQM_TO_TPH = 10f;
        private const double SECONDS_PER_HOUR = 3600f; // s
        private const double MAX_COVER = 75f; // %

        double _fmc, _ros, _intensity, _flameHeight; //output
        FuelSubTypes _subtype;

        public Spinifex(FuelSubTypes subtype) 
        {
            _subtype = subtype;
        }

        /// <summary>
        /// AWAP_uf: monthly top level soil moisture(unitless 0-1)from http://www.sciro.au/awap
        /// time_since_fire: (y)
        ///  relative_humidity: (%)
        ///  air _temperature: (°C)
        /// </summary>
        /// <param name="AWAP_uf"></param>
        /// <param name="time_since_fire"></param>
        /// <param name="relative_humidity"></param>
        /// <param name="air_temperature"></param>        
        public void Calculate(double AWAP_uf, int time_since_fire, double relative_humidity, double air_temperature, double wind_speed_10m, double wrf)
        {
            _fmc = FMC_spinifex(AWAP_uf, time_since_fire, relative_humidity, air_temperature, _subtype);
            _ros = ROS_spinifex(wind_speed_10m, time_since_fire, _fmc, wrf, _subtype);
            _intensity = intensity_spinifex(_ros, time_since_fire, _subtype);
            _flameHeight = flame_height_spinifex(_ros, time_since_fire, _subtype);
        }


        ///return estimated spinifex fuel cover
        ///Based on: Holmes AW, Krix D, Burrows ND, Kristina A, Jenkins M (2023)
        ///Adapting fire behaviour models in spinifex grasslands of arid australia to incorporate AWRA-Lv7 root zone soil moisture.
        ///args
        ///  time_since_fire: (y)
        ///  subtype: FuelSubTypes.Spinifex  or FuelSubTypes.SpinifexWoodland
        private static double fuel_cover_spinifex(int time_since_fire, FuelSubTypes subtype)
        {
            double fuel_cover_spinifex = -2.55991763 - 0.03838217 * time_since_fire + 1.10476581 * Mathd.Log(time_since_fire);

            if(subtype == FuelSubTypes.SpinifexWoodland)
            {
                fuel_cover_spinifex = (fuel_cover_spinifex - 0.13992188 + 0.12047025 * Mathd.Log(time_since_fire));
            }

            return (1 / (1 + Mathd.Exp(-fuel_cover_spinifex))) * 100;
        }

        ///return the fuel moisture content (%)
        ///Based on: Holmes AW, Krix D, Burrows ND, Kristina A, Jenkins M (2023)
        ///Adapting fire behaviour models in spinifex grasslands of arid australia to incorporate AWRA-Lv7 root zone soil moisture.
        ///args
        ///  AWAP_uf: monthly top level soil moisture (unitless 0-1)from http://www.sciro.au/awap
        ///  time_since_fire: (y)
        ///  relative_humidity: (%)
        ///  air _temperature: (°C)
        ///  subtype: FuelSubTypes.Spinifex  or FuelSubTypes.SpinifexWoodland
        private static double FMC_spinifex(double AWAP_uf, int time_since_fire, double relative_humidity, double air_temperature, FuelSubTypes subtype)
        {
            time_since_fire = Mathd.Min(25, time_since_fire); //AFDRS limits TSF to 25 years

            double fuel_cover = fuel_cover_spinifex(time_since_fire, subtype);
            double vpd = AFDRS.vp_deficit(air_temperature, relative_humidity);

            double pct_dead = -4.0936696 + 0.8619864 * time_since_fire - 1.613603 * Mathd.Log(time_since_fire) - 0.1739302 * time_since_fire * Mathd.Log(time_since_fire);
            pct_dead = (1 / (1 + Mathd.Exp(-pct_dead))) * 100; //TODO: check why this is not * 100, but the others are - not I have amended

            double live = (0.419130229421894 + 0.158980195 * Mathd.Sqrt(AWAP_uf) - 0.271357085 * Mathd.Sqrt(fuel_cover) - 0.007380343 * vpd);
            live = (1 / (1 + Mathd.Exp(-live))) * 100;

            double dead = -9.34004475 - 0.37649308 * relative_humidity + 3.17594774 * Mathd.Log(relative_humidity) + 0.06805771 * relative_humidity * Mathd.Log(relative_humidity);
            dead = (1 / (1 + Mathd.Exp(-dead))) * 100;

            //FMC_spinifex = dead
            return ((live * (fuel_cover - pct_dead)) + (dead * pct_dead)) / fuel_cover;
        }

        ///return estimated fuel load (t/ha) [Range: 0-20 t/ha]
        ///Based on: Holmes AW, Krix D, Burrows ND, Kristina A, Jenkins M (2023)
        ///Adapting fire behaviour models in spinifex grasslands of arid australia to incorporate AWRA-Lv7 root zone soil moisture.
        ///args
        ///  time_since_fire: (y)
        ///  subtype: FuelSubTypes.Spinifex  or FuelSubTypes.SpinifexWoodland
        private static double fuel_load_spinifex(int time_since_fire, FuelSubTypes subtype)
        {
            time_since_fire = Mathd.Min(25, time_since_fire); //AFDRS limits TSF to 25 years

            double fuel_load_spinifex = -0.6892583 - 0.0360736 * time_since_fire + 1.1552554 * Mathd.Log(time_since_fire);

            if (subtype == FuelSubTypes.SpinifexWoodland)
            {
                fuel_load_spinifex = (fuel_load_spinifex + 0.4253039 - 0.1723223 * Mathd.Log(time_since_fire));
            }

            return Mathd.Exp(fuel_load_spinifex);
        }

        ///returns the spread index (go/no-go).
        ///Very unlikely fire will spread at SI < 0. if(SI > 0 fire is likely to spread.
        ///Based on: Holmes AW, Krix D, Burrows ND, Kristina A, Jenkins M (2023)
        ///Adapting fire behaviour models in spinifex grasslands of arid australia to incorporate AWRA-Lv7 root zone soil moisture.
        ///args
        ///  wind_speed_10m: mean 10 m wind speed (km/h)
        ///  fuel_moisture: combined dead & live fuel moisture(%)
        ///  fuel_cover: total fuel cover (%)
        ///  wrf:
        private static double spread_index_spinifex(double wind_speed_10m, double fuel_moisture, double fuel_cover, double wrf)
        {
            const double intercept_value = -5.85681251780825;
            const double wind_speed_coefficient = 0.336940088553979;
            const double fuel_moisture_coefficient = -0.496404135425536;
            const double fuel_cover_coefficient = 0.272475260353266;

            double wind_speed_2m = wind_speed_10m * wrf;

            //calculate the linear predictions
            double lin_preds = (intercept_value + (wind_speed_coefficient * wind_speed_2m) + (fuel_moisture_coefficient * fuel_moisture) + (fuel_cover_coefficient * fuel_cover));

            //convert to spread index
            return Mathd.Exp(lin_preds) / (1 + Mathd.Exp(lin_preds));
        }

        ///return the steady-state forward rate of spread (m/h)
        ///Based on:
        ///Burrows, N., Gill, M., and Sharples, J. (2018). Development and validation of a model for
        ///predicting fire behaviour in spinifex grasslands of arid Australia [IJWF].
        ///args
        ///  wind_speed_10m: mean 10 m wind speed (km/h)
        ///  fuel_moisture: combined dead & live fuel moisture(%)
        ///  fuel_cover: total fuel cover (%)
        ///  wrf:
        private static double ROS_spinifex(double wind_speed_10m, int time_since_fire, double fuel_moisture, double wrf, FuelSubTypes subtype)
        {
            double wind_speed_2m = wind_speed_10m * wrf;
            double fuel_cover = fuel_cover_spinifex(time_since_fire, subtype);
            double spread_index = spread_index_spinifex(wind_speed_10m, fuel_moisture, fuel_cover, wrf);

            double ROS_spinifex = 40.982 * ((Mathd.Pow(wind_speed_2m, 1.399) * Mathd.Pow(fuel_cover, 1.201)) / (Mathd.Pow(fuel_moisture, 1.699)));

            if(spread_index <= 0 || ROS_spinifex < 0)
            {
                ROS_spinifex = 0;
            }

            return ROS_spinifex;
        }

        ///returns fire line intensity (kW/m)
        ///Based on definition in Byram, G. M. (1959). Combustion of forest fuels in Forest fire: control and use.(Ed. KP Davis) pp. 61 89.
        ///args
        ///  rate_of_spread: steady-state forward rate of spread (m/h)
        ///  time_since_fire: (y)
        ///  subtype: FuelSubTypes.Spinifex or FuelSubTypes.SpinifexWoodland
        private static double intensity_spinifex(double rate_of_spread, int time_since_fire, FuelSubTypes subtype)
        {
            double fuel_load = fuel_load_spinifex(time_since_fire, subtype); /// KGSQM_TO_TPH this conversion happens in intensity calc
            return intensity(rate_of_spread, fuel_load);
        }

        /// returns the fireline intensity (kW/m) based on Byram 1959
        ///   ROS: forward rate of spread (km/h)
        ///   fuel_load: fine fuel load (t/ha)
        private static double intensity(double ROS, double fuel_load)
        {
            // convert units
            ROS = ROS / 3600; // m/s
            fuel_load = fuel_load / 10; //kg/m^2

            return 16700 * ROS * fuel_load;
        }

        ///returns flame height (m) [range: 0 - 6 m]
        ///Based on:
        ///Burrows, N., Gill, M., and Sharples, J. (2018). Development and validation of a model for
        ///predicting fire behaviour in spinifex grasslands of arid Australia [IJWF].
        ///args
        ///  rate_of_spread: steady-state forward rate of spread (m/h)
        ///  time_since_fire: (y)
        ///  subtype: FuelSubTypes.Spinifex or FuelSubTypes.SpinifexWoodland
        private static double flame_height_spinifex(double rate_of_spread, int time_since_fire, FuelSubTypes subtype)
        {
            double fuel_load = fuel_load_spinifex(time_since_fire, subtype);
            return 0.097 * Mathd.Pow(rate_of_spread, 0.424) + 0.102 * fuel_load;
        }
    }
}
