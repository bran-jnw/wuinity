using System;
using System.Collections.Generic;
using System.Text;

namespace PREACT.Wildfire.AFDRS
{
    public enum FuelSubTypes { WoodyGrassland, AcaciaWoodland, WoodyHorticulture, Rural }

    public class GrassyWoodland
    {
        float _fmc, _ros, _intensity, _flameHeight;
        Grassland.States _state;
        FuelSubTypes _fuelSubType;

        public GrassyWoodland(FuelSubTypes fuelSubType)
        {
            if(fuelSubType == FuelSubTypes.AcaciaWoodland)
            {
                _state = Grassland.States.EatenOut;
            }
            else if(fuelSubType == FuelSubTypes.Rural)
            {
                _state = Grassland.States.Grazed;
            }
            //TODO: Gamba is mentioned in original
            /*Case "Gamba"
            Range("state_woodland").Value = "natural"*/
        }

        public void Calculate()
        {

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
        private float ROS_woodland(float U_10, float mc, float curing, Grassland.States state, float waf)
        {
            return Grassland.ROS_grass(U_10, mc, curing, state) * waf;
        }

        /// returns the woodland fuel moisture content (%)
        /// uses grass fuel moisture content based on McArthur (1966)
        ///   temp: air temperature (C)
        ///   rh: relative humidity (%)
        private float FMC_woodland(float temp, float rh)
        {
            return Grassland.FMC_grass(temp, rh);
        }

        /// returns the flame height (m) based on M. Plucinski, pers. comm.
        /// uses the grass model
        ///   ROS: forward rate of spread (m/h)
        ///   state: grass state (natural, eaten out, grazed)
        private float Flame_height_woodland(float ROS, Grassland.States state)
        {
            return Grassland.Flame_height_grass(ROS, state);
        }

        /// returns the fireline intensity (kW/m) based on Byram 1959
        ///
        /// args
        ///   ROS: forward rate of spread (km/h)
        ///   fuel_load: fine fuel load (t/ha)
        private float Intensity_woodland(float ROS, float fuel_load)
        {
            return Grassland.Intensity_grass(ROS, fuel_load);
        }
    }
}
