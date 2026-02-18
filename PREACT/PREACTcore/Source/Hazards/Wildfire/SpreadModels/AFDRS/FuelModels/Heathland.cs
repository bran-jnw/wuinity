using System;
using PREACT.Math;

namespace PREACT.Wildfire.AFDRS
{
    public class Heathland : AFDRSFuelModel
    {
        public enum States { Dry, Wet }// no diff in the models at this stage

        public override AFDRSOutput Calculate(AFDRSInput input)
        {
            return Calculate(input.Temp, input.RH, input.RainLast48Hours, input.HoursSinceRain, input.U_10, input.ElevatedFuelHeight, input.WindAdjustmentFactor, input.FuelLoadSurface, input.PercentSlope, input.WindAzimuth, input.SlopeAzimuth, input.HeathlandState);
        }

        /// temp: air temperature(C)
        /// rh: relative humidity (%)
        /// rain: precipitation in the last 48 hours (mm)
        /// hours: time since rain or dewfall stopped (h)
        /// U_10: 10 m wind speed (km/h)
        /// h_el: elevated fuel height (m)
        /// waf: wind adjustment factor
        public static AFDRSOutput Calculate(double temp, double rh, double rain, int hoursSinceRain, double U_10, double h_el, double waf, double fuel_load, double percentSlope, double windAzimuth, double slopeAzimuth, States state = States.Dry)
        {
            double fmc = FMC_heath(temp, rh, rain, hoursSinceRain);
            double SI = SI_heath(U_10, h_el, fmc, waf);

            double noWindNoSlopeROS = ROS_heath(0, h_el, fmc, SI, waf);
            double slopeROS = noWindNoSlopeROS * SpreadModelAFDRS.SlopeFactorFBP(percentSlope);
            double windROS = ROS_heath(U_10, h_el, fmc, SI, waf);

            //calculate final values
            SpreadModelAFDRS.CalculateDirectionOfMaxSpread(windAzimuth, slopeAzimuth, noWindNoSlopeROS, windROS, slopeROS, out double ros, out double direction); 
            double intensity = intensity_heath(ros, fuel_load);
            double flameHeight = Flame_height_heath(intensity);
            double lengthToWidth = SpreadModelAFDRS.GrasslandLengthToWidth(U_10);

            return new AFDRSOutput(fmc, ros, direction, intensity, flameHeight, lengthToWidth);
        }

        /// returns fuel moisture content (%). Based on:
        ///   Cruz, M., et al. (2010). Fire dynamics in mallee-heath: fuel, weather
        ///   and fire behaviour prediction in south Australian semi-arid shrublands.
        ///   Bushfire CRC Program A Rep 1(01).
        ///
        /// In addition, a fuel moisture modifier based on recent rainfall was used. Marsden-Smedley, J. B.,
        /// et al. (1999). Buttongrass moorland fire-behaviour prediction
        /// and management. Tasforests 11: 87-107.
        ///   temp: air temperature (C)
        ///   rh: relative humidity (%)
        ///   rain: precipitation in the last 48 hours (mm)
        ///   hours: time since rain or dewfall stopped (h)
        private static double FMC_heath(double temp, double rh, double rain, double hours)
        {
            double mc_1 = 4.37 + 0.161 * rh - 0.1 * (temp - 25);
            if(rh <= 60)
            {
                mc_1 = mc_1 - 0.027 * rh;
            }
            double mc_2 = 67.128f * (1 - Mathd.Exp(-3.132 * rain)) * Mathd.Exp(-0.0858 * hours);

            return mc_1 + mc_2;
        }

        /// returns the heathland moisture function
        ///
        /// args
        ///   mc: fuel moisture content (%)
        private static double Mf_heath(double mc)
        {
            double Mf_heath;
            if(mc < 4)
            {
                Mf_heath = Mathd.Exp(-0.0762 * 4);
            }
            else if(mc > 20)
            {
                Mf_heath = 0.05f;
            }
            else
            {
                Mf_heath = Mathd.Exp(-0.0762 * mc);
            }

            return Mf_heath;
        }

        /// returns Spread Index.
        ///
        /// args
        ///   U_10: 10 m wind speed (km/h)
        ///   h_el: elevated fuel height (m)
        ///   mc: fuel moisture content (%)
        ///   waf: wind adjustment factor
        private static double SI_heath(double U_10, double h_el, double mc, double waf)
        {
            double U_2 = U_10 * waf;
            double SI_heath = 2.57902560498943 + 0.175608738551563 * U_2 + 0.752448659028343 * h_el + 0.14916661946054 * h_el * U_2 - 0.430727111563859 * mc;

            return Mathd.Exp(SI_heath) / (1 + Mathd.Exp(SI_heath));
        }

        /// returns forward rate of spread (m/h) [range: 0-6000 m/h]
        /// Anderson, W. R., et al. (2015). "A generic, empirical-based model for predicting rate of fire
        /// spread in shrublands." International Journal of Wildland Fire 24(4): 443-460.
        ///   U_10: 10 m wind speed (km/h)
        ///   h_el: elevated fuel height (m)
        ///   mc: fuel moisture content (%)
        ///   overstorey: presence or absence of woodland overstorey (true/false)
        ///   waf: wind adjustment factor
        private static double ROS_heath(double U_10, double h_el, double mc, double SI, double waf)
        {
            double U_2 = U_10 * waf;
            double sqrt_U_2 = Mathd.Sqrt(U_2);
            //Dim SI As Double: SI = SI_heath(U2, h_el, mc)

            mc = mc / 100; //change to proportion
            double logit_mc = Mathd.Log(mc / (1 - mc));

            double ROS_heath = 3.34696092119763 + 0.588661598397372 * sqrt_U_2 - 0.788551298241711 * logit_mc + 0.414992984575498 * Mathd.Log(h_el);

            return SI * Mathd.Exp(ROS_heath);
        }

        /// returns the fire line intensity (kW/m)
        ///   ROS: forward rate of spread (m/h)
        ///   fl_max: maximum fuel load (t/ha)
        ///   tsf: time since fire (y)
        ///   k: fuel accumulation curve constant
        private static double intensity_heath(double ROS, double fuel_load)
        {
            //intensity_heath = intensity(ROS, fuel_load_)
            return 18600 * (fuel_load / 10) * (ROS / 3600);
        }

        /// returns flame height (m)
        /// No equation for flame height was given in the Anderson et al. paper (2015).
        /// Here we use the flame height calculation for mallee-heath shrublands (Cruz, M. G., et al. (2013).
        /// "Fire behaviour modelling in semi-arid mallee-heath shrublands of southern Australia.
        /// Environmental Modelling & Software 40: 21-34).
        ///   intensity: fire line intensity (kW/m)
        private static double Flame_height_heath(double intensity)
        {
            return Mathd.Exp(-4.142) * Mathd.Pow(intensity, 0.633);
        }
    }
}
