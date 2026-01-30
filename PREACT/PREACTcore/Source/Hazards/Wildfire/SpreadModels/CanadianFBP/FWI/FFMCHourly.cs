//Translated from: https://github.com/cffdrs/cffdrs_py/blob/main/cffdrs/hourly_fine_fuel_moisture_code.py and https://github.com/cffdrs/cffdrs_r/blob/main/R/hourly_fine_fuel_moisture_code.r

using System;
using PREACT.Math;

namespace PREACT.Wildfire
{
    public  class FFMCHourly
    {
        private const double FFMC_COEFFICIENT = 250.0 * 59.5 / 101.0; //used in conversion between FFMC and moisture content
        private double _ffmc0, _ffmc;

        public double Value {  get =>  _ffmc; }

        public FFMCHourly() 
        {
            _ffmc0 = 85.0;
        }

        /// <summary> 
        /// Hourly Fine Fuel Moisture Code is based on a calculation routine first described in detail by Van
        /// Wagner (1977) and which has been updated in minor ways by the Canadian Forest
        /// Service to have it agree with the calculation methodology for the daily FFMC.    
        /// In its simplest typical use this current routine calculates a value of FFMC based on a series of
        /// uninterrupted hourly weather observations of screen level (~1.4 m) temperature, relative humidity, 10 m
        /// wind speed, and 1-hour rainfall.This implementation of the function
        /// includes an optional time step input which is defaulted to one hour, but can
        /// be reduced if sub-hourly calculation of the code is needed.The FFMC is in
        /// essence a bookkeeping system for moisture content and thus it needs to use
        /// the last time step's value of FFMC in its calculation as well.
        /// 
        /// The hourly FFMC is very similar in its structure and calculation to the    
        /// Canadian Forest Fire Weather Index System's daily FFMC
        /// but has an altered drying and wetting rate which more realistically reflects
        /// the drying and wetting of a pine needle litter layer sitting on a decaying
        /// organic layer.  This particular implementation of the Canadian Forest Fire
        /// Danger Rating System's hourly FFMC provides for a flexible time step; that
        /// is, the data need not necessarily be in time increments of one hour.This
        /// flexibility has been added for some users who use this method with data
        /// sampled more frequently that one hour.We do not recommend using a time
        /// step much greater than one hour.An important and implicit assumption in
        /// this calculation is that the input weather is constant over the time step of
        /// each calculation (e.g., typically over the previous hour).  This is a
        /// reasonable assumption for an hour; however it can become problematic for
        /// longer periods.For brevity we have referred to this routine throughout
        /// this description as the hourly FFMC.
        /// </summary>
        /// <param name="temp">Temperature(centigrade)</param>
        /// <param name="rh">Relative Humidity(%)</param>
        /// <param name="ws">Wind speed(km/h)</param>
        /// <param name="prec">1-hour rainfall (mm)</param>
        /// <param name="t0">Time(in hours) between the previous value of FFMC and the current time at which we want to calculate a new value of the FFMC.</param>
        public void Calculate(double temp, double rh, double ws, double prec, double t0 = 1.0)
        {
            double mr, mo, rf, ko, kd, md, ew, k1, kw, mw, ed, m;
            // Eq. 1 (with a more precise multiplier than the daily)
            mo = FFMC_COEFFICIENT * (101 - _ffmc0) / (59.5 + _ffmc0);
            rf = prec;

            // Eqs. 3a & 3b (Van Wagner & Pickett 1985)
            if (mo <= 150)
            {
                mr = mo + 42.5 * rf * Mathd.Exp(-100 / (251 - mo)) * (1 - Mathd.Exp(-6.93 / rf));
            }                
            else
            {
                mr = (mo + 42.5 * rf * Mathd.Exp(-100 / (251 - mo)) * (1 - Mathd.Exp(-6.93 / rf)) + 0.0015 * Mathd.Pow2(mo - 150) * Mathd.Sqrt(rf));
            }
            
            // The real moisture content of pine litter ranges up to about max 250 percent
            mr = Mathd.Min(mr, 250);
            mo = prec > 0.0 ? mr : mo;

            // Eq. 2a Equilibrium moisture content from drying
            ed = 0.942 * Mathd.Pow(rh, 0.679) + 11 * Mathd.Exp((rh - 100) / 10) + 0.18 * (21.1 - temp) * (1 - Mathd.Exp(-0.115 * rh));

            // Eq. 3a Log drying rate at the normal temperature of 21.1C
            ko = 0.424 * Mathd.Pow(1 - (rh / 100), 1.7) + 0.0694 * Mathd.Sqrt(ws) * Mathd.Pow(1 - (rh / 100), 8);

            // Eq. 3b
            kd = ko * 0.0579 * Mathd.Exp(0.0365 * temp);

            // Eq. 8 (Van Wagner & Pickett 1985)
            md = ed + (mo - ed) * Mathd.Pow(10, (-kd * t0));

            // Eq. 2b Equilibrium moisture content from wetting
            ew = 0.618 * (Mathd.Pow(rh, 0.753) + 10 * Mathd.Exp((rh - 100) / 10) + 0.18 * (21.1 - temp) * (1 - Mathd.Exp(-0.115 * rh)));

            // Eq. 7a Log wetting rate at the normal temperature of 21.1 C
            k1 = 0.424 * Mathd.Pow(1 - ((100 - rh) / 100), 1.7) + 0.0694 * Mathd.Sqrt(ws) * Mathd.Pow(1 - ((100 - rh) / 100), 8);

            // Eq. 4b
            kw = k1 * 0.0579 * Mathd.Exp(0.0365 * temp);
            // Eq. 8 (Van Wagner & Pickett 1985)
            mw = ew - (ew - mo) * Mathd.Pow(10, (-kw * t0));

            // Constraints
            m = mw; 
            if (mo > ed)
            {
                m = md;
            }

            if (ed >= mo && mo >= ew)
            {
                m = mo;
            }            

            // Eq. 6 - Final hffmc calculation
            _ffmc0 = 59.5 * (250 - m) / (FFMC_COEFFICIENT + m);
            _ffmc0 = Mathd.Max(_ffmc0, 0);
            _ffmc = _ffmc0;
        }
    }
}
