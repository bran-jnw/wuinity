using System;
using System.Collections.Generic;
using System.Text;

namespace PREACT.Wildfire.AFDRS
{
    public struct AFDRSInput
    {
        //Time
        public DateTime DateTime;

        //DEM
        public double PercentSlope, SlopeAzimuth;

        //wind
        public double U_10, WindAzimuth;
        public double WindAdjustmentFactor;

        //weather
        public double Temp, RH;
        public double RainLast48Hours; //mm last 48 hours
        public int HoursSinceRain;// hours,

        //moisture
        public double DroughtFactor;
        public double KBDI;
        public double AWAP;

        //fire stuff
        public int YearsSinceFire;//years
        public double Curing;
        public double FuelLoadSurface;
        public double FuelLoadNearSurface;
        public double FuelLoadCrown;

        //fuel model specifics
        //Buttongrass
        public int Productivity;

        //Forest
        public Forest.FuelSubModel ForestSubModel;

        //Grassland
        public Grassland.FuelSubTypes GrasslandSubType;
        public Grassland.States GrasslandState; //also used by Savannah

        //Heathland
        public Heathland.States HeathlandState;
        public double ElevatedFuelHeight;

        //MalleeHeath
        public MalleeHeath.FuelSubTypes MalleeHeathSubType;
        public double MalleHeathOverstoreyCover;
        public double MalleHeathOverstoreyHeight;

        //Pine has none

        //Savannah
        public Savanna.FuelSubTypes SavannahSubType;

        //Spinifex
        public Spinifex.FuelSubTypes SpinifexSubType;

        /// DF: Drought factor
        /// KBDI: Keetch Byram drought index KBDI
        public void UpdateTransientData(DateTime dateTime, double temp, double rh, double windSpeed, double windAzimuth, double rainLast48Hours, int hoursSinceRain, double df, double kbdi)
        {
            Temp = temp;
            RH = rh;
            U_10 = windSpeed;
            WindAzimuth = windAzimuth;
            RainLast48Hours = rainLast48Hours;
            HoursSinceRain = hoursSinceRain;
            DroughtFactor = df;
            KBDI = kbdi;
        }

        ///temp: air temperature(C)
        ///   rh: relative humidity (%)
        ///   tsr: time since rain (h)
        ///   rain: rainfall last 48 hours (mm)  
        ///   U_10: 10 m wind speed(km/h)
        /// tsf: time since fire(y))
        /// productivity?
        public void SetButtonGrassInput(int hoursSinceRain, int productivity, double percentSlope, double slopeAzimuth)
        {       
            HoursSinceRain = hoursSinceRain;
            Productivity = productivity;

            PercentSlope = percentSlope;
            SlopeAzimuth = slopeAzimuth;            
        }

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
        public void SetForestInput(double fhs_s, double fhs_ns, double h_ns, double DF, double h_el, double fl_s, double fl_ns, double fl_e, double fl_o, double h_o, double slopeAzimuth, double percentSlope, double DI = 100, Forest.FuelSubModel subModel = Forest.FuelSubModel.Dry, double waf = 3)
        {
            PercentSlope = percentSlope;
            SlopeAzimuth = slopeAzimuth;

        }

        ///   fuel_load: fine fuel load (t/ha)
        ///   curing: degree of grass curing (%)
        ///   state: grass state (natural, grazed, eaten-out)
        public void SetGrasslandInput(double fuel_load, double curing, Grassland.FuelSubTypes fuelSubType, Grassland.States state, double percentSlope, double slopeAzimuth)
        {
            FuelLoadSurface = fuel_load;
            Curing = curing;
            GrasslandSubType = fuelSubType;
            GrasslandState = state;

            PercentSlope = percentSlope;
            SlopeAzimuth = slopeAzimuth;
        }

        /// h_el: elevated fuel height (m)
        /// waf: wind adjustment factor
        public void SetHeathlandInput(double h_el, double waf, double fuel_load, double percentSlope, double slopeAzimuth, Heathland.States state = Heathland.States.Dry)
        {
            ElevatedFuelHeight = h_el;
            WindAdjustmentFactor = waf;
            FuelLoadSurface = fuel_load;
            HeathlandState = state;

            PercentSlope = percentSlope;
            SlopeAzimuth = slopeAzimuth;
        }

        ///   overstorey_cover: (%)
        ///   overstorey_height: (m)
        ///   fuel_load_surface: surface fuel load (t/ha)
        ///   fuel_load_canopy: canopy fuel load (t/ha)
        public void SetMalleeHeathInput(MalleeHeath.FuelSubTypes subType, double overstoreyCover, double overstoreyHeight, double fuelLoadSurface, double fuelLoadNearSurface, double fuelLoadCrown, double percentSlope, double slopeAzimuth)
        {       
            MalleeHeathSubType = subType;
            MalleHeathOverstoreyCover = overstoreyCover;
            MalleHeathOverstoreyHeight = overstoreyHeight;
            FuelLoadSurface = fuelLoadSurface;
            FuelLoadNearSurface = fuelLoadNearSurface;
            FuelLoadCrown = fuelLoadCrown;

            PercentSlope = percentSlope;
            SlopeAzimuth = slopeAzimuth;
        }

        public void SetPineInput(double percentSlope, double slopeAzimuth)
        {
            PercentSlope = percentSlope;
            SlopeAzimuth = slopeAzimuth;
        }

        ///   WAF: wind adjustment factor
        ///   subtype: woodland, acacia_woodland, woody_forticulture, rural, urban
        ///   curing: degree of grass curing (%)
        ///   fuel_load: fine fuel load (t/ha)
        ///   state: grass state (natural, eaten out, grazed)
        public void SetSavannahInput(double waf, Savanna.FuelSubTypes fuelSubType, double curing, double fuel_load, Grassland.States state, double percentSlope, double slopeAzimuth)
        {
            WindAdjustmentFactor = waf;
            SavannahSubType = fuelSubType;
            Curing = curing;
            FuelLoadSurface = fuel_load;
            GrasslandState = state;

            PercentSlope = percentSlope;
            SlopeAzimuth = slopeAzimuth;
        }

        /// AWAP_uf: monthly top level soil moisture(unitless 0-1)from http://www.sciro.au/awap
        /// time_since_fire: (y)      
        ///  fuel_moisture: combined dead & live fuel moisture(%)
        ///  fuel_cover: total fuel cover (%)
        ///  wrf: wind reduction factor
        ///  subtype: spinifex or spinifex woodland
        public void SetSpinifexInput(double AWAP_uf, int time_since_fire, double wrf, Spinifex.FuelSubTypes fuelSubType, double percentSlope, double slopeAzimuth)
        {
            AWAP = AWAP_uf;
            YearsSinceFire = time_since_fire;
            WindAdjustmentFactor = wrf;
            SpinifexSubType = fuelSubType;

            PercentSlope = percentSlope;
            SlopeAzimuth = slopeAzimuth;
        }
    }
}
