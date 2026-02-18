using System;
using System.Collections.Generic;
using System.Text;

namespace PREACT.Wildfire.AFDRS
{   
    public static class Savannah
    {
        public enum FuelSubTypes { WoodyGrassland, AcaciaWoodland, WoodyHorticulture, Rural }

        public static AFDRSOutput Calculate(double temp, double rh, double U_10, double mc, double waf, double percentSlope, double windAzimuth, double slopeAzimuth, FuelSubTypes fuelSubType, double curing, double fuel_load, Grassland.States state)
        {
            double fmc = FMC_woodland(temp, rh);

            if (fuelSubType == FuelSubTypes.AcaciaWoodland || fuelSubType == FuelSubTypes.WoodyHorticulture)
            {
                state = Grassland.States.EatenOut;
            }
            else if (fuelSubType == FuelSubTypes.Rural)
            {
                state = Grassland.States.Grazed;
            }

            double noWindNoSlopeROS = ROS_woodland(0, fmc, curing, state, waf);
            double slopeROS = noWindNoSlopeROS * SpreadModelAFDRS.SlopeFactor(percentSlope);
            double windROS = ROS_woodland(U_10, fmc, curing, state, waf);

            //calculate final values
            SpreadModelAFDRS.CalculateDirectionOfMaxSpread(windAzimuth, slopeAzimuth, noWindNoSlopeROS, windROS, slopeROS, out double ros, out double direction);
            double intensity = Intensity_woodland(fmc, fuel_load);
            double flameHeight = Flame_height_woodland(ros, state);

            return new AFDRSOutput(fmc, ros, direction, intensity, flameHeight);
        }

        /// returns the forward ROS (m/h) ignoring slope
        /// Based on:
        /// Cheney, N. P., Gould, J. S., & Catchpole, W. R. (1998). Prediction of fire
        /// spread in grasslands. International Journal of Wildland Fire, 8(1), 1-13.
        ///
        /// Cruz, M. G., Gould, J. S., Kidnie, S., Bessell, R., Nichols, D., &
        /// Slijepcevic, A. (2015). Effects of curing on grassfires: II. Effect of grass
        /// senescence on the rate of fire spread. International Journal of Wildland
        /// Fire, 24(6), 838-848.
        ///   U_10: 10 m wind speed (km/h)
        ///   mc: fuel moisture content (%)
        ///   curing: degree of grass curing (%)
        ///   subtype: woodland, acacia_woodland, woody_forticulture, rural, urban
        ///   state: grass state (natural, eaten out, grazed)
        ///   WAF: wind adjustment factor
        private static double ROS_woodland(double U_10, double mc, double curing, Grassland.States state, double waf)
        {
            return Grassland.ROS_grassland(U_10, mc, curing, state, Grassland.FuelSubTypes.Grass) * waf;
        }

        /// returns the woodland fuel moisture content (%)
        /// uses grass fuel moisture content based on McArthur (1966)
        ///   temp: air temperature (C)
        ///   rh: relative humidity (%)
        private static double FMC_woodland(double temp, double rh)
        {
            return Grassland.FMC_grassland(temp, rh);
        }

        /// returns the flame height (m) based on M. Plucinski, pers. comm.
        /// uses the grass model
        ///   ROS: forward rate of spread (m/h)
        ///   state: grass state (natural, eaten out, grazed)
        private static double Flame_height_woodland(double ROS, Grassland.States state)
        {
            return Grassland.Flame_height_grassland(ROS, state);
        }

        /// returns the fireline intensity (kW/m) based on Byram 1959
        ///
        /// args
        ///   ROS: forward rate of spread (km/h)
        ///   fuel_load: fine fuel load (t/ha)
        private static double Intensity_woodland(double ROS, double fuel_load)
        {
            return Grassland.Intensity_grassland(ROS, fuel_load);
        }
    }
}
