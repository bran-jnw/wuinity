using PREACT.Math;

namespace PREACT.Wildfire.AFDRS
{
    public class Buttongrass : AFDRSFuelModel
    {
        public override AFDRSOutput Calculate(AFDRSInput input)
        {
            return Calculate(input.Temp, input.RH, input.HoursSinceRain, input.RainLast48Hours, input.U_10, input.YearsSinceFire, input.Productivity, input.PercentSlope, input.WindAzimuth, input.SlopeAzimuth);
        }

        ///temp: air temperature(C)
        ///   rh: relative humidity (%)
        ///   tsr: time since rain (h)
        ///   rain: rainfall last 48 hours(mm)  
        ///   U_10: 10 m wind speed(km/h)
        /// tsf: time since fire(y))
        public static AFDRSOutput Calculate(double temp, double rh, double tsr, double rain, double U_10, int tsf, int productivity, double percentSlope, double windAzimuth, double slopeAzimuth)
        {
            double dew_pt = AFDRS.dewpoint(temp, rh);
            double fmc = FMC_buttongrass(temp, rh, dew_pt, tsr, rain);

            double noWindNoSlopeROS = ROSButtongrass(0, fmc, tsf, productivity);
            double slopeROS = noWindNoSlopeROS * SpreadModelAFDRS.SlopeFactor(percentSlope);
            double windROS = ROSButtongrass(U_10, fmc, tsf, productivity);

            //calculate final values
            SpreadModelAFDRS.CalculateDirectionOfMaxSpread(windAzimuth, slopeAzimuth, noWindNoSlopeROS, windROS, slopeROS, out double ros, out double direction);
            double fuelLoad = FuelLoadButtongrass(tsf, productivity);
            double intensity = IntensityButtongrass(ros, fuelLoad);
            double flameHeight = FlameHeightButtongrass(intensity);
            double lengthToWidth = SpreadModelAFDRS.GrasslandLengthToWidth(U_10);

            return new AFDRSOutput(fmc, ros, direction, intensity, flameHeight, lengthToWidth);
        }

        //This might tbe useful at some point, this is used if doing the Canadian approach to spread direction taking into account wind and slope
        private static double SlopeWindEquivalent(double slopeROS, double mc, double tsf)
        {
            double U_2 = Mathd.Pow(slopeROS / (0.678 * Mathd.Exp(-0.0243 * mc) * (1 - Mathd.Exp(-0.116 * tsf)) * 60), 1 / 1.312);

            double U_10 = U_2 * 1.2;

            return U_10;
        }

        private static void CalculateEffectiveWind(double windSpeed, double windAzimuth, double slopeWindSpeed, double slopeAzimuth, out double effectiveWindSpeed, out double effectiveWindAzimuth)
        {
            //wind vector
            double wrad = windAzimuth * Mathd.Deg2Rad;
            double wsx = windSpeed * Mathd.Sin(wrad);
            double wsy = windSpeed * Mathd.Cos(wrad);

            //slope vector
            double srad = slopeAzimuth * Mathd.Deg2Rad;
            double wsex = slopeWindSpeed * Mathd.Sin(srad);
            double wsey = slopeWindSpeed * Mathd.Cos(srad);

            //combined vector
            double wsvx = wsx + wsex;
            double wsvy = wsy + wsey;
            double WSV = Mathd.Sqrt(wsvx * wsvx + wsvy * wsvy);
            effectiveWindSpeed = WSV;

            double raz = Mathd.Acos(wsvy / WSV);
            raz = raz * Mathd.Rad2Deg;

            if (wsvx < 0)
            {
                raz = 360 - raz;
            }
            effectiveWindAzimuth = raz;
        }

        ///   returns the grass fuel moisture content (%) based on McArthur (1966)
        ///   temp: air temperature (C)
        ///   rh: relative humidity (%)
        ///   tsr: time since rain (h)
        ///   rain: rainfall (mm)       
        ///   dew_pt: dewpoint temperature (c)
        private static double FMC_buttongrass(double temp, double rh, double dew_pt, double tsr, double rain)
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
        private static double FuelLoadButtongrass(int tsf, int productivity)
        {
            double FuelLoadButtongrass;

            if(productivity == 1)
            {
                FuelLoadButtongrass = 11.73 * (1 - Mathd.Exp(-0.106 * tsf));
            }
            else
            {
                FuelLoadButtongrass = 44.61 * (1 - Mathd.Exp(-0.041 * tsf));
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
        private static double SpreadProbButtongrass(double U_10, double mc, int productivity)
        {
            double U_2 = U_10 / 1.2;
            return 1 / (1 + Mathd.Exp(-(-1 + 0.68 * U_2 - 0.07 * mc - 0.0037 * U_2 * mc + 2.1 * productivity)));
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
        private static double ROSButtongrass(double U_10, double mc, int tsf, int productivity)
        {
            double ROSButtongrass;

            double spread_prob = SpreadProbButtongrass(U_10, mc, productivity);
            if (spread_prob <= 0.5)
            {
                ROSButtongrass = 0f;
            }
            else
            {
                double U_2 = U_10 / 1.2;
                ROSButtongrass = 0.678 * Mathd.Pow(U_2, 1.312) * Mathd.Exp(-0.0243 * mc) * (1 - Mathd.Exp(-0.116 * tsf)) * 60;
            }   

            return ROSButtongrass;
        }

        /// <summary>
        ///  returns the flame height (m) based on M. Plucinski, pers. comm.
        ///  Intensity(kW/m)
        /// </summary>
        /// <param name="intensity"></param>
        /// <returns></returns>
        private static double FlameHeightButtongrass(double intensity)
        {
            return 0.148 * Mathd.Pow(intensity, 0.403);
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
        private static double IntensityButtongrass(double ROS, double fuel_load)
        {
            ROS = ROS / 3600; // to m/s
            fuel_load = fuel_load / 10; // to kg/m^2
            return 19900 * ROS * fuel_load;
        }
    }
}
