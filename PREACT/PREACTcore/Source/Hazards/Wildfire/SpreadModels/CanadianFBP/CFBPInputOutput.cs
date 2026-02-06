using System;
using System.Collections.Generic;
using System.Text;

namespace PREACT.Wildfire
{
    //some help for units and variable names https://cran.r-project.org/web/packages/cffdrs/refman/cffdrs.html
    public struct CanadianFBPInputs
    {
        public double FFMC, BUI, WindSpeed, GrassFuelLoad, Lat, Lon;
        public int Time, Pattern, JulianDay, JulianDayMin, WindAzimuth, PercentSlope, SlopeAzimuth, PercentCuring, Elevation;// not used: mon, hour, hourly 
    }
    public class FireData
    {
        public double RateOfSpread, Distance, rost, CrownFractionBurned, FuelConsumption, CrownFuelConsumed, Time, SurfaceRateOfSpread, ISI;
        public char FireDescription;
        public double FireIntensity;
    }

    public class MainOutputs
    {
        public double hffmc, SurfaceFuelConsumption, CriticalSurfaceIntensity, RSO, FoliarMoistureContent, SurfaceFireIntensity, SurfaceRateOfSpread, ISI, be, SpreadFactor, SpreadAzimuth, WSV, ff;
        public int JulianDayMin, JulianDay;
        public char CoverType;
    }

    public class SecondaryOutputs
    {
        public double LengthToBreadth, Area, Perimeter, pgr, lbt;
    }
}
