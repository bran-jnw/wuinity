using System;
using PREACT.Math;

namespace PREACT.Wildfire.AFDRS
{
    public class Grassland
    {
        /*public struct Coefficients
        {
            float r_l0, r_lw, r_h0, r_hw;

            public Coefficients(float r_l0, float r_lw, float r_h0, float r_hw)
            {
                this.r_l0 = r_l0;
                this.r_lw = r_lw;
                this.r_h0 = r_h0;
                this.r_hw = r_hw;
            }

            public static readonly Coefficients Natural = new Coefficients(0.54f, 0.269f, 1.4f, 0.838f);
            public static readonly Coefficients Grazed = new Coefficients(0.054f, 0.209f, 1.1f, 0.715f);
            public static readonly Coefficients EatenOut = new Coefficients(0.027f, 0.1045f, 0.55f, 0.357f);
        }*/

        public enum FuelSubTypes { Grass, Pasture, ChenopodShrubland, LowWetland, GambaGrass }

        public enum States { Natural, Grazed, EatenOut }


        float _fmc, _ros, _intensity, _flameHeight;
        float _fuel_load, _curing;
        States _state;

        public Grassland(FuelSubTypes fuelSubType, float fuel_load, float curing)
        {
            if (fuelSubType == FuelSubTypes.ChenopodShrubland || fuelSubType == FuelSubTypes.LowWetland)
            {
                _state = States.EatenOut;
            }
            else if (fuelSubType == FuelSubTypes.GambaGrass)
            {
                _state = States.Natural;
            }
            else
            {
                _state = load_to_state_grass(fuel_load);
            }

            _fuel_load = fuel_load;
            _curing = curing;
        }

        public void Calculate(float temp, float rh, float U_10)
        {
            _fmc = FMC_grass(temp, rh);    
            _ros = ROS_grass(U_10, _fmc, _curing, _state);// fuelSubType);
            _intensity = Intensity_grass(_ros, _fuel_load);// fuelSubType);
            _flameHeight = Flame_height_grass(_ros, _state);
        }

        /// returns the grass fuel moisture content (%) based on McArthur (1966)
        ///
        /// args:
        ///   temp: air temperature (C)
        ///   rh: relative humidity (%)
        public static float FMC_grass(float temp, float rh)
        {
            float FMC_grass;

            FMC_grass = 9.58f - 0.205f * temp + 0.138f * rh;
            return Mathf.Max(FMC_grass, 5f);
        }

        /// returns the curing coefficient based on Cruz et al. (2015)
        ///   curing: degree of grass curing (%)
        public static float curing_coeff_grass(float curing)
        {
            return 1.036f / (1f + 103.989f * Mathf.Exp(-0.0996f * (curing - 20f)));
        }

        /// returns the grass moisture coefficient
        ///   U_10: 10 m wind speed (km/h)
        ///   mc: fuel moisture content (%)
        public static float moist_coeff_grass(float U_10, float mc)
        {
            float moist_coeff_grass;

            if (mc < 12)
            {
                moist_coeff_grass = Mathf.Exp(-0.108f * mc);
            }
            else
            {
                if (U_10 <= 10f)
                {
                    moist_coeff_grass = 0.684f - 0.0342f * mc;
                }
                else
                {
                    moist_coeff_grass = 0.547f - 0.0228f * mc;
                }
            }

            moist_coeff_grass = Mathf.Max(moist_coeff_grass, 0.001f);

            return moist_coeff_grass;
        }

        /// returns the forward ROS (m/h) ignoring slope
        ///
        /// args
        ///   U_10: 10 m wind speed (km/h)
        ///   mc: fuel moisture content (%)
        ///   curing: degree of grass curing (%)
        ///   state: grass state (natural, grazed, eaten-out)
        public static float ROS_grass(float U_10, float mc, float curing, States state)//, FuelSubTypes fuelSubType)
        {
            float curing_coeff = curing_coeff_grass(curing);
            float moist_coeff = moist_coeff_grass(U_10, mc);
            float waf = 1f;

            /*if (fuelSubType == FuelSubTypes.GambaGrass)
            {
                waf = 1f; //TODO: what value should it be?
            }*/

            float ROS_grass;

            if (state == States.Natural)
            {
                if (U_10 < 5)
                {
                    ROS_grass = 0.054f + 0.269f * U_10;
                }
                else
                {
                    ROS_grass = 1.4f + 0.838f * Mathf.Pow(U_10 - 5, 0.844f);
                }
            }
            else if (state == States.Grazed)
            {
                if (U_10 < 5)
                {
                    ROS_grass = 0.054f + 0.209f * U_10;
                }
                else
                {
                    ROS_grass = 1.1f + 0.715f * Mathf.Pow(U_10 - 5, 0.844f);
                }
            }
            else //eaten-out
            {
                if (U_10 < 5)
                {
                    ROS_grass = 0.054f + 0.209f * U_10;
                }
                else
                {
                    ROS_grass = 0.55f + 0.357f * Mathf.Pow(U_10 - 5f, 0.844f);
                }
            }

            ROS_grass = ROS_grass * 1000f * moist_coeff * curing_coeff * waf;

            return ROS_grass;
        }

        /// returns the flame height (m) based on M. Plucinski, pers. comm.
        ///
        /// args
        ///   ROS: forward rate of spread (m/h)
        ///   state: grass state (natural, grazed, eaten-out)
        public static float Flame_height_grass(float ROS, States state)
        {
            //adjust units from km/h to m/s
            ROS = ROS / 3600;

            float Flame_height_grass = 0f;

            if (state == States.Natural)
            {
                Flame_height_grass = 2.66f * Mathf.Pow(ROS, 0.295f);
            }
            else//eaten-out or grazed
            {
                Flame_height_grass = 1.12f * Mathf.Pow(ROS, 0.295f);
            }

            return Flame_height_grass;
        }

        /// returns the fireline intensity (kW/m) based on Byram 1959
        /// for grass fuel loads are limited to range 1 to 6 t/ha
        ///   ROS: forward rate of spread (km/h)
        ///   fuel_load: fine fuel load (t/ha)
        public static float Intensity_grass(float ROS, float fuel_load)//, FuelSubTypes fuelSubType)
        {
            float Intensity_grass;

            //TODO:check
            /*if (fuelSubType != FuelSubTypes.GambaGrass)
            {
                //limit fuel load to range 1 - 6
                fuel_load = Mathf.Max(1f, fuel_load);
                fuel_load = Mathf.Min(6f, fuel_load);                
            }*/
            Intensity_grass = intensity(ROS, fuel_load);

            return Intensity_grass;
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

        /// returns the grass fuel load (t/ha)
        ///
        /// args
        ///   state: the grass fuel state - eaten-out, grazed or natural
        public static float state_to_load_grass(States state)
        {
            //TODO: verify
            float load = 2.0f; //eaten out

            if (state == States.Natural)
            {
                load = 5.0f;
            }
            else if (state == States.Grazed)
            {
                load = 3.5f;
            }

            return load;
        }


        /// returns the grass fuel state - eaten-out, grazed or natural
        ///
        /// args
        ///   load: the grass fuel load (t/ha)
        public static States load_to_state_grass(float load)
        {
            States state = States.EatenOut;

            //TODO: verify
            if (load >= 6f)
            {
                state = States.Natural;
            }
            else if (load >= 3)
            {
                state = States.Grazed;
            }

            return state;
        }
    }
}
