using System;
using PREACT.Math;

namespace PREACT.Wildfire.AFDRS
{   
    public class Forest : AFDRSFuelModel
    {
        public override AFDRSOutput Calculate(AFDRSInput input)
        {            
            return new AFDRSOutput();//Calculate(input.Temp, input.RH, input.U_10, input.DateTime, input.WindAzimuth, input.SlopeAzimuth, input.PercentSlope, )
        }

        public enum FuelSubModel { Dry, Wet }

        ///   temp: air temperature(C)
        ///   rh: relative humidity (%)
        ///   dateTime:
        ///   U_10: 10 m wind speed (km/h)
        ///   fhs_s: surface fuel hazard score
        ///   fhs_ns: near surface fuel hazard score
        ///   h_ns: near surface fuel height (cm)
        ///   DF: Drought factor          
        ///   h_el - elevated fuel height (m)
        ///   fl_s: surface fuel load (t/ha)
        ///   fl_ns: near surface fuel load (t/ha)
        ///   fl_e: elevated fuel load (t/ha)
        ///   fl_o: overstorey (canopy) fuel load (t/ha)
        ///   h_o: overstorey (canopy) height (m)  
        ///   DI: drought index - KBDI except SDI in Tas        
        public static AFDRSOutput Calculate(double temp, double rh, double U_10, DateTime dateTime, double windAzimuth, double slopeAzimuth, double percentSlope, double fhs_s, double fhs_ns, double h_ns, double DF, double h_el, double fl_s, double fl_ns, double fl_e, double fl_o, double h_o, double DI = 100, FuelSubModel submodel = FuelSubModel.Dry, double waf = 3)
        {
            double fmc = FMC_forest(temp, rh, dateTime, submodel);
            double fuel_availability = fuel_availability_forest(DF, DI, waf, submodel);

            double noWindNoSlopeROS = ROS_forest(0, fhs_s, fhs_ns, h_ns, fmc, waf, fuel_availability);
            double slopeROS = noWindNoSlopeROS * SpreadModelAFDRS.SlopeFactorFBP(percentSlope);
            double windROS = ROS_forest(U_10, fhs_s, fhs_ns, h_ns, fmc, waf, fuel_availability);

            //calculate final values
            SpreadModelAFDRS.CalculateDirectionOfMaxSpread(windAzimuth, slopeAzimuth, noWindNoSlopeROS, windROS, slopeROS, out double ros, out double direction);
            //double fuelLoad = fuel_availability_forest(DF, DI, waf, submodel);
            double flameHeight = Flame_height_forest(ros, h_el);
            double intensity = Intensity_forest(ros, flameHeight, fl_s, fl_ns, fl_e, fl_o, h_o, fuel_availability);
            double lengthToWidth = SpreadModelAFDRS.ForestLengthToWidth(U_10);

            return new AFDRSOutput(fmc, ros, direction, intensity, flameHeight, lengthToWidth);
        }

        /// return the intensity based on fuel load and ROS
        /// note AFDRS caps surface fuel load at 10 t/ha (1 kg/m)
        ///   ROS: forward rate of spread (km/h)
        ///   flame_h: flame height (m)
        ///   fl_s: surface fuel load (t/ha)
        ///   fl_ns: near surface fuel load (t/ha)
        ///   fl_e: elevated fuel load (t/ha)
        ///   fl_o: overstorey (canopy) fuel load (t/ha)
        ///   h_o: overstorey (canopy) height (m)
        private static double Intensity_forest(double ROS, double flame_h, double fl_s, double fl_ns, double fl_e, double fl_o, double h_o, double fuel_avail)
        {     
            //modify fuel parameters with fuel availability
            fl_s = fl_s * fuel_avail;
            fl_ns = fl_ns * fuel_avail;
            fl_e = fl_e * fuel_avail;
            fl_o = fl_o * fuel_avail;

            //cap surface fuel load
            fl_s = Mathd.Min(10f, fl_s);

            //accumulate fuel load based on flame height
            double fuel_load = fl_s + fl_ns;

            double flame_h_elev = 1f; //m
            const double flame_h_crown_frac = 0.66f; //dimensionless

            if (flame_h > flame_h_elev)
            {
                fuel_load = fuel_load + fl_e;
            }

            if(flame_h > h_o * flame_h_crown_frac)
            {
                fuel_load = fuel_load + 0.5f * fl_o;
            }

            return intensity(ROS, fuel_load);
        }

        /// returns the fireline intensity (kW/m) based on Byram 1959
        ///   ROS: forward rate of spread (km/h)
        ///   fuel_load: fine fuel load (t/ha)
        private static double intensity(double ROS, double fuel_load)
        {
            // convert units
            ROS = ROS / 3600; // m/s
            fuel_load = fuel_load / 10; //kg/m^2

            return 18600f * ROS * fuel_load;
        }

        /// returns the flame height (m)
        ///   ROS - forward rate of spread (m/h)
        ///   h_el - elevated fuel height (m)
        private static double Flame_height_forest(double ROS, double h_el)
        {
            return 0.0193f * Mathd.Pow(ROS, 0.723f) * Mathd.Exp(h_el * 0.64f) * 1.07f;
        }

        /// returns the forward ROS (m/h) ignoring slope
        ///   U_10: 10 m wind speed (km/h)
        ///   fhs_s: surface fuel hazard score
        ///   fhs_ns: near surface fuel hazard score
        ///   h_ns: near surface fuel height (cm)
        ///   fmc: fuel moisture content (%)
        ///   DF: Drought factor
        ///   waf : wind adjustment factor
        ///   DI: drought indes - KBDI except SDI in Tas
        ///   submodel: dry or wet
        private static double ROS_forest(double U_10, double fhs_s, double fhs_ns, double h_ns, double fmc, double waf, double fuel_avail)
        {
            const double wind_threshold = 5;
            h_ns = Mathd.Min(h_ns, 20);
            double mf = Mf_forest(fmc); //moisture function

            //modify fuel parameters with fuel availability
            fhs_s = fhs_s * fuel_avail;
            fhs_ns = fhs_ns * fuel_avail;

            //apply wind reduction factor
            double wind_speed = U_10 * 3 / waf;

            //calculate ROS for 7% moisture
            double ROS_forest;
            if (wind_speed > wind_threshold)
            {
                ROS_forest = 30f + 1.5308f * Mathd.Pow(wind_speed - wind_threshold, 0.8576f) * Mathd.Pow(fhs_s, 0.9301f) * Mathd.Pow(fhs_ns * h_ns,  0.6366f) * 1.03f;
            }
            else
            {
                ROS_forest = 30f;
            }

            //apply moisture factor
            return ROS_forest * mf;
        }

        /// return the fine fuel moisture content (%)
        ///   temp: air temperature (C)
        ///   rh: relative humidity (%)
        ///   date_: (underscore due to VBA Date objects)
        ///   time:
        private static double FMC_forest(double temp, double rh, DateTime dateTime, FuelSubModel submodel)
        {
            const int start_peak_month = 10; //October
            const int end_peak_month = 3; //March
            const int start_afternoon = 12;
            const int end_afternoon = 17;
            const int sunrise = 6;
            const int sunset = 19;

            double FMC_forest;
            if((dateTime.Month >= start_peak_month || dateTime.Month <= end_peak_month) 
                && (dateTime.Hour >= start_afternoon && dateTime.Hour <= end_afternoon)
                && submodel == FuelSubModel.Dry)
            {
                FMC_forest = 2.76f + 0.124f * rh - 0.0187f * temp;
            }                
            else if (dateTime.Hour <= sunrise || dateTime.Hour >= sunset)
            {
                FMC_forest = 3.08f + 0.198f * rh - 0.0483f * temp;
            }
            else
            {
                FMC_forest = 3.6f + 0.169f * rh - 0.045f * temp;
            }

            return FMC_forest;
        }

        /// returns the forest fuel moisture factor
        ///   fmc: fine fuel moisture content (%)
        private static double Mf_forest(double fmc)
        {
            double Mf_forest;
            if (fmc <= 4)
            {
                Mf_forest = 2.31f;
            }                
            else if( fmc > 20)
            {
                Mf_forest = 0.05f;
            }
            else
            {
                Mf_forest = 18.35f * Mathd.Pow(fmc, -1.495f);
            }

            return Mf_forest;
        }

        /// returns the spotting distance (m)
        ///   ROS: forward rate of spread (m/h)
        ///   U_10: 10m wind speed (km/h)
        ///   fhs_s: fuel hazard score surface
        private static double Spotting_forest(double ROS, double U_10, double fhs_s)
        {
            double Spotting_forest;

            if (ROS < 150f)
            {
                Spotting_forest = 50f;
            }                
            else
            {
                Spotting_forest = Mathd.Abs(176.969f * Mathd.Atan(fhs_s) * Mathd.Sqrt(ROS / Mathd.Pow(U_10, 0.25f)) + 1568800f * Mathd.Pow(fhs_s, -1) * Mathd.Pow(ROS / Mathd.Pow(U_10, 0.25f), -1.5f) - 3015.09f);
            }

            return Spotting_forest;
        }

        /// returns the fuel availability - proportion of fuel available to be burnt
        ///   DF: Drought factor
        ///   DI: drought index - KBDI except SDI in Tas
        ///   WAF: wind adjustment factor
        ///   submodel: dry or wet
        static double fuel_availability_forest(double DF, double DI = 100f, double waf = 3f, FuelSubModel submodel = FuelSubModel.Dry)
        {
            double fuel_availability_forest = 0f;

            if (submodel == FuelSubModel.Dry)
            {
                fuel_availability_forest = DF * 0.1f;
            }                
            else //wet
            {
                double C1 = 0.1f * ((0.0046f * Mathd.Pow(waf, 2) - 0.0079f * waf - 0.0175f) * DI + (-0.9167f * Mathd.Pow(waf, 2f) + 1.5833f * waf + 13.5f));
                C1 = Mathd.Max(C1, 0);
                C1 = Mathd.Min(C1, 1);
                fuel_availability_forest = 1.008f / (1f + 104.9f * Mathd.Exp(-0.9306f * C1 * DF));
                fuel_availability_forest = Mathd.Min(fuel_availability_forest, DF * 0.1f); //shouldn't get higher ros for wet when WAF is low
            }

            return fuel_availability_forest;
        }
    }
}
