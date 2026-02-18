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
        public int TimeSinceRain;// hours,

        //moisture
        public double DF;
        public double KBDI;

        //fire stuff
        public int TimeSinceFire;//years
        public double Curing;
        public double SurfaceFuelLoad;
        public double NearSurfaceFuelLoad;
        public double FuelLoadCrown;

        //fuel model specifics
        //Buttongrass
        public int Productivity;

        //Forest
        public Forest.FuelSubModel ForestSubModel;

        //Grassland
        public Grassland.FuelSubTypes GrasslandSubType;
        public Grassland.States GrasslandState;

        //Heathland
        public Heathland.States HeathlandState;
        public double ElevatedFuelHeight;

        //MalleeHeath
        public MalleeHeath.FuelSubTypes MalleeHeathSubType;
        public double MalleHeathOverstoreyCover;
        public double MalleHeathOverstoreyHeight;

        /// DF: Drought factor
        /// KBDI: Keetch Byram drought index KBDI
        public void UpdateTransientData(DateTime dateTime, double temp, double rh, double windSpeed, double windAzimuth, double rainLast48Hours, int timeSinceRain, double DF, double KBDI)
        {
            Temp = temp;
            RH = rh;
            U_10 = windSpeed;
            WindAzimuth = windAzimuth;
            RainLast48Hours = rainLast48Hours;
            TimeSinceRain = timeSinceRain;
            this.DF = DF;
            this.KBDI = KBDI;
        }

        ///temp: air temperature(C)
        ///   rh: relative humidity (%)
        ///   tsr: time since rain (h)
        ///   rain: rainfall last 48 hours (mm)  
        ///   U_10: 10 m wind speed(km/h)
        /// tsf: time since fire(y))
        /// productivity?
        public void SetButtonGrassInput(int tsf, int productivity, double percentSlope, double slopeAzimuth)
        {
            SlopeAzimuth = slopeAzimuth;
            PercentSlope = percentSlope;
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
        ///   DI: drought indes - KBDI except SDI in Tas        
        public void SetForestInput(double fhs_s, double fhs_ns, double h_ns, double DF, double h_el, double fl_s, double fl_ns, double fl_e, double fl_o, double h_o, double slopeAzimuth, double percentSlope, double DI = 100, Forest.FuelSubModel submodel = Forest.FuelSubModel.Dry, double waf = 3)
        {
            SlopeAzimuth = slopeAzimuth;
            PercentSlope = percentSlope;

        }

        ///   temp: air temperature (C)
        ///   rh: relative humidity (%)
        ///   U_10: 10 m wind speed (km/h)
        ///   fuel_load: fine fuel load (t/ha)
        ///   curing: degree of grass curing (%)
        ///   state: grass state (natural, grazed, eaten-out)
        public void SetGrasslandInput(double fuel_load, double curing, Grassland.FuelSubTypes fuelSubType, Grassland.States state, double percentSlope, double slopeAzimuth)
        {
            SlopeAzimuth = slopeAzimuth;
            PercentSlope = percentSlope;
        }

        /// h_el: elevated fuel height (m)
        /// waf: wind adjustment factor
        public void SetHeatlandInput(double h_el, double waf, double fuel_load, double percentSlope, double slopeAzimuth, Heathland.States state = Heathland.States.Dry)
        {

        }

        ///   overstorey_cover: (%)
        ///   overstorey_height: (m)
        ///   fuel_load_surface: surface fuel load (t/ha)
        ///   fuel_load_canopy: canopy fuel load (t/ha)
        public void SetMalleeHeathInput(MalleeHeath.FuelSubTypes subType, double overstoreyCover, double overstoreyHeight, double fuelLoadSurface, double fuelLoadNearSurface, double fuelLoadCrown, double percentSlope, double slopeAzimuth)
        {
            SlopeAzimuth = slopeAzimuth;
            PercentSlope = percentSlope;

            MalleeHeathSubType = subType;
            SurfaceFuelLoad = fuelLoadSurface;
        }

        public void SetPineInput(double percentSlope, double slopeAzimuth)
        {

        }
    }
}
