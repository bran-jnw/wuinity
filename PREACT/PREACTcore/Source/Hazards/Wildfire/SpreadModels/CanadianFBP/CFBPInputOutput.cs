using System;
using System.Collections.Generic;
using System.Text;

namespace PREACT.Fire
{
    public struct CFBPInputs
    {
        public double FFMC, WindSpeed, GrassFuelLoad, BUI, Lat, Lon;
        public int Time, Pattern, mon, JulianDate, jd_min, WindAzimuth, PercentSlope, SlopeAzimuth, PercentCuring, Elevation, hour, hourly;
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
        public int JulianDateMin, JulianDate;
        public char CoverType;
    }

    public class SecondaryOutputs
    {
        public double LengthToBreadth, Area, Perimeter, pgr, lbt;
    }
}
