using System;
using PREACT.Math;

namespace PREACT.Wildfire.AFDRS
{
    public class Grassland : AFDRSFuelModel
    {
        public enum FuelSubTypes { Grass, Pasture, ChenopodShrubland, LowWetland, GambaGrass }

        public override AFDRSOutput Calculate(AFDRSInput input)
        {
            return Calculate(input.Temp, input.RH, input.U_10, input.FuelLoadSurface, input.Curing, input.GrasslandSubType, input.GrasslandState, input.PercentSlope, input.WindAzimuth, input.SlopeAzimuth);
        }

        /*public struct Coefficients
        {
            double r_l0, r_lw, r_h0, r_hw;

            public Coefficients(double r_l0, double r_lw, double r_h0, double r_hw)
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
        
        public enum States { Natural, Grazed, EatenOut }

        ///   temp: air temperature (C)
        ///   rh: relative humidity (%)
        ///   U_10: 10 m wind speed (km/h)
        ///   fuel_load: fine fuel load (t/ha)
        ///   curing: degree of grass curing (%)
        ///   state: grass state (natural, grazed, eaten-out)
        public static AFDRSOutput Calculate(double temp, double rh, double U_10, double fuel_load, double curing, FuelSubTypes fuelSubType, States state, double percentSlope, double windAzimuth, double slopeAzimuth)
        {
            double fmc = FMC_grassland(temp, rh);

            //enfirce a few parameters if needed
            if (fuelSubType == FuelSubTypes.ChenopodShrubland || fuelSubType == FuelSubTypes.LowWetland)
            {
                state = States.EatenOut;
            }
            else if (fuelSubType == FuelSubTypes.GambaGrass)
            {
                state = States.Natural;
            }
            /*else
            {
                state = load_to_state_grass(fuel_load);
            }*/

            double noWindNoSlopeROS = ROS_grassland(0, fmc, curing, state, fuelSubType);
            double slopeROS = noWindNoSlopeROS * SpreadModelAFDRS.SlopeFactorFBP(percentSlope);
            double windROS = ROS_grassland(U_10, fmc, curing, state, fuelSubType);

            //calculate final values
            SpreadModelAFDRS.CalculateDirectionOfMaxSpread(windAzimuth, slopeAzimuth, noWindNoSlopeROS, windROS, slopeROS, out double ros, out double direction);
            double intensity = Intensity_grassland(ros, fuel_load);// fuelSubType);
            double flameHeight = Flame_height_grassland(ros, state);
            double lengthToWidth = SpreadModelAFDRS.GrasslandLengthToWidth(U_10);

            return new AFDRSOutput(fmc, ros, direction, intensity, flameHeight, lengthToWidth);
        }

        /// returns the grass fuel moisture content (%) based on McArthur (1966)
        ///
        /// args:
        ///   temp: air temperature (C)
        ///   rh: relative humidity (%)
        public static double FMC_grassland(double temp, double rh)
        {
            double FMC_grass;

            FMC_grass = 9.58 - 0.205 * temp + 0.138 * rh;
            return Mathd.Max(FMC_grass, 5);
        }

        /// returns the curing coefficient based on Cruz et al. (2015)
        ///   curing: degree of grass curing (%)
        public static double curing_coeff_grassland(double curing)
        {
            return 1.036 / (1 + 103.989 * Mathd.Exp(-0.0996 * (curing - 20)));
        }

        /// returns the grass moisture coefficient
        ///   U_10: 10 m wind speed (km/h)
        ///   mc: fuel moisture content (%)
        public static double moist_coeff_grassland(double U_10, double mc)
        {
            double moist_coeff_grass;

            if (mc < 12)
            {
                moist_coeff_grass = Mathd.Exp(-0.108 * mc);
            }
            else
            {
                if (U_10 <= 10)
                {
                    moist_coeff_grass = 0.684 - 0.0342 * mc;
                }
                else
                {
                    moist_coeff_grass = 0.547 - 0.0228 * mc;
                }
            }

            moist_coeff_grass = Mathd.Max(moist_coeff_grass, 0.001);

            return moist_coeff_grass;
        }

        /// returns the forward ROS (m/h) ignoring slope
        ///
        /// args
        ///   U_10: 10 m wind speed (km/h)
        ///   mc: fuel moisture content (%)
        ///   curing: degree of grass curing (%)
        ///   state: grass state (natural, grazed, eaten-out)
        public static double ROS_grassland(double U_10, double mc, double curing, States state, FuelSubTypes fuelSubType)
        {
            double curing_coeff = curing_coeff_grassland(curing);
            double moist_coeff = moist_coeff_grassland(U_10, mc);
            double waf = 1;

            /*if (fuelSubType == FuelSubTypes.GambaGrass)
            {
                waf = 1f; //TODO: what value should it be?
            }*/

            double ROS_grass;

            if (state == States.Natural)
            {
                if (U_10 < 5)
                {
                    ROS_grass = 0.054 + 0.269 * U_10;
                }
                else
                {
                    ROS_grass = 1.4 + 0.838 * Mathd.Pow(U_10 - 5, 0.844);
                }
            }
            else if (state == States.Grazed)
            {
                if (U_10 < 5)
                {
                    ROS_grass = 0.054 + 0.209 * U_10;
                }
                else
                {
                    ROS_grass = 1.1 + 0.715 * Mathd.Pow(U_10 - 5, 0.844);
                }
            }
            else //eaten-out
            {
                if (U_10 < 5)
                {
                    ROS_grass = 0.054 + 0.209 * U_10;
                }
                else
                {
                    ROS_grass = 0.55 + 0.357 * Mathd.Pow(U_10 - 5, 0.844);
                }
            }

            ROS_grass = ROS_grass * 1000 * moist_coeff * curing_coeff * waf;

            return ROS_grass;
        }

        /// returns the flame height (m) based on M. Plucinski, pers. comm.
        ///
        /// args
        ///   ROS: forward rate of spread (m/h)
        ///   state: grass state (natural, grazed, eaten-out)
        public static double Flame_height_grassland(double ROS, States state)
        {
            //adjust units from km/h to m/s
            ROS = ROS / 3600;

            double Flame_height_grass = 0;

            if (state == States.Natural)
            {
                Flame_height_grass = 2.66 * Mathd.Pow(ROS, 0.295);
            }
            else//eaten-out or grazed
            {
                Flame_height_grass = 1.12 * Mathd.Pow(ROS, 0.295);
            }

            return Flame_height_grass;
        }

        /// returns the fireline intensity (kW/m) based on Byram 1959
        /// for grass fuel loads are limited to range 1 to 6 t/ha
        ///   ROS: forward rate of spread (km/h)
        ///   fuel_load: fine fuel load (t/ha)
        public static double Intensity_grassland(double ROS, double fuel_load)//, FuelSubTypes fuelSubType)
        {
            double Intensity_grass;

            //TODO:check
            /*if (fuelSubType != FuelSubTypes.GambaGrass)
            {
                //limit fuel load to range 1 - 6
                fuel_load = Mathd.Max(1f, fuel_load);
                fuel_load = Mathd.Min(6f, fuel_load);                
            }*/
            Intensity_grass = intensity(ROS, fuel_load);

            return Intensity_grass;
        }

        /// returns the fireline intensity (kW/m) based on Byram 1959
        ///   ROS: forward rate of spread (km/h)
        ///   fuel_load: fine fuel load (t/ha)
        public static double intensity(double ROS, double fuel_load)
        {
            // convert units
            ROS = ROS / 3600; // m/s
            fuel_load = fuel_load / 10; //kg/m^2

            return 18600 * ROS * fuel_load;
        }

        /// returns the grass fuel load (t/ha)
        ///
        /// args
        ///   state: the grass fuel state - eaten-out, grazed or natural
        public static double state_to_load_grassland(States state)
        {
            //TODO: verify
            double load = 2.0; //eaten out

            if (state == States.Natural)
            {
                load = 5.0;
            }
            else if (state == States.Grazed)
            {
                load = 3.5;
            }

            return load;
        }


        /// returns the grass fuel state - eaten-out, grazed or natural
        ///
        /// args
        ///   load: the grass fuel load (t/ha)
        public static States load_to_state_grassland(double load)
        {
            States state = States.EatenOut;

            //TODO: verify
            if (load >= 6)
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
