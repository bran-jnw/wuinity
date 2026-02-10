//Adapted from https://github.com/bran-jnw/Cell2Fire/blob/main/cell2fire/Cell2FireC/FBPfunc5_NoDebug.c 2025 by Jonathan Wahlqvist
//Original comments
/*   Subroutine of  FBP.C   version 4.4   Aug,2007
    Canadian Forest Fire Behaviour Prediction System
    This code is copyright of the Canadian Forest Service, Natural Resources Canada (1992-2005)
    It is provide free of charge to anyone who wishes to incorporate it within their 
    forest fire management applications, however users should note in their application that
    the FBP calucaltions come from the Canadian Forest Services Fire Behaviour Prediction
    System. The Canadian Forest Service has gone through considerable testing to ensure that
    these computer functions duplicate the system as laid out in ST-X-3 (The Development
    and Structure of the Canadian Forest Fire Behaviour Prediction System (1992)) and the subsequent corrections and additions to the system (the draft "FBP Note"), however no
    guarentee is given as to the absolute accuracy of the code.
    This file contains a series of functions that go thru all the
    FBP System calculations.
    Originally  Written at P.N.F.I.  December 91, by Mike Wotton
    Corrections to version  1.0
    1.01  -  b value in O1b was wrong.   KA at nofc (mar5,92) ...bmw
    1.01a  - no error just added upper and lowercase fueltype entry ...bmw
    1.02   - l_to_b problem with inequalitiy...minor prob fixed
    1.5  - modifications to stop small(improbable) numbers from blowing
    up the slope calculation stuff
    this caused a slope problem which BA @ PFC pointed out
    may 93.......bmw
    3.0  jan /96   bmw
    -  c6  - constant sfc
    3.01  apr/97
    - add line to recognize uppercase in grass fuels
    3.02  apr/97
    - change accn funtion to recognize lower case fuels for open
    3.1  jul/97
    - change slope function to stop overflow when cur<=50 in O1
    3.2 sept/97
    - change the c6 calc of flank ros to avoid variation in l/b with
    RSC calculation in flank.
    3.3 jan/98
    - error was SFC >0  if CUR<50 (and hence ROS=0)
    changed this within the surf_fuel_consump() fn
    3.4 apr/98  (ya i know the dates aren't in sequence)
    - change O1a  A value from 1.41 to 1.4  from KA apr/98
         
    4.0 jan/98
    - changes listed within the FBP note and TEST dataset 
    4.1  - August 2004
    - final changes in the FBP note (new M-3/M-4 model) and a new grass CF model
    -removed some verstigaes of older functions for input /output
    4.2 - Oct 2004
    - changes as a result of discussion with Prometheus team
    -  SFC in grass ...4.1 did not include changes to grass consumption though
    spread at values of CUR<50% are now possible (see point 3.3 above)
    - at  very short time periods uMathd.Sing acceleration a CFB(t) should be caluclated
    and used for final ROS in the C-6 model. 
    4.3 - August 2005
    - changes as a result of further discussions with Prometheus team. These were:
    - changes to include the alternate ISI calculation formula (53a footnote 2) in 
    the calculation of WSE  (...added as a section to The FBP Note)
    - addition of D-2 for the promtheus team to evaluate their model
    ( D-2 is leafed out pure aspen
    ROS(D-2)= 0.2 * ROS(D-1) if BUI>80  otherwise ROS(D-2)=0)
    - NOTE that this (D-2) is not an official part of the FBP System however.
    4.4  August 2007
    - crown consump in m3/m4. It was calcuating based full conifer content.
    but should be modifed by PDF
    - change to match changes in FBP Note section 3.3 (eqn 66c)
    4.5 Nov 2007
    - perimeter calc wasn't based on LB(t).
    - Flankfire final ROS (FROS) could be set to FRSS if flank fire did not
    involve crowning 
    - change to function headers in perimeter() and  flank_spread_distance()
    to accomodate LB(t) assignment
    - LB(t) is now added to the secondary inputs structure and kept
    it is asdsigned in  flank_spread_distance().
    4.6 jan 2009
    - uping the slope limit to 70%...and so the default after that is 10.0
    5.0  Oct 2014
    - PGR is corrected to be a function of ROS(t) not equilibrium ROS  
    5.0001  June 2015
    - forgot to change the encoded version number to 5.0.  now 5.0001
    - updated header to FBP5.h */

using System.Collections.Generic;
using PREACT.Math;
using System.IO;

namespace PREACT.Wildfire
{    
    
    /// <summary>
    /// Canadian Fire Behavior Prediction
    /// </summary>
    public static class CanadianFBP
    {
        public enum FuelTypes { C1, C2, C3, C4, C5, C6, C7, D1, D2, M1, M2, M3, M4, S1, S2, S3, O1a, O1b, NonFuel }
        public enum CoverTypes { Open, Canopy}

        private static readonly double slopelimit_isi = 0.01;
        private static readonly int numfuels = 18;      

        private static readonly Dictionary<string, FuelCoefficients> _fuelCoeffs;

        static CanadianFBP()
        {
            _fuelCoeffs = new Dictionary<string, FuelCoefficients>(numfuels);
            CreateDefaultDatabase();
            Engine.Message(null, Engine.LogType.Debug, "Default CFBP database created.");
        }

        public static FuelCoefficients GetFuelCoefficients(string fuelType, out bool success)
        {
            success = _fuelCoeffs.TryGetValue(fuelType, out FuelCoefficients f);
            return f;
        }

        public static void Calculate(CanadianFBPInputs input, CanadianFBPFuel fuel, MainOutputs mainOuts, SecondaryOutputs secondaryOuts, FireData headfire, FireData flankfire, FireData backfire)
        {
            char firetype = ' ';
            double acceleration;
            zero_main(mainOuts);
            zero_sec(secondaryOuts);
            zero_fire(headfire);
            zero_fire(flankfire);
            zero_fire(backfire);
            mainOuts.CoverType = get_fueltype_number(fuel.Coefficients.FuelType);
            mainOuts.SurfaceRateOfSpread = rate_of_spread(input, fuel, mainOuts);
            headfire.SurfaceRateOfSpread = mainOuts.SurfaceRateOfSpread;
            mainOuts.SurfaceFuelConsumption = SurfaceFuelConsumption(input, fuel);
            mainOuts.SurfaceFireIntensity = fire_intensity(mainOuts.SurfaceFuelConsumption, mainOuts.SurfaceRateOfSpread);            

            if (mainOuts.CoverType == 'c')
            {
                mainOuts.FoliarMoistureContent = FoliarMoisture(input, mainOuts);
                mainOuts.CriticalSurfaceIntensity = CriticalSurfaceIntensity(fuel, mainOuts.FoliarMoistureContent);
                mainOuts.RSO = critical_ros(mainOuts.SurfaceFuelConsumption, mainOuts.CriticalSurfaceIntensity); //critical spread rate for crowning
                firetype = fire_type(mainOuts.CriticalSurfaceIntensity, mainOuts.SurfaceFireIntensity); //crown fire or surface fire

                if (firetype == 'c')
                {
                    headfire.CrownFractionBurned = crown_frac_burn(mainOuts.SurfaceRateOfSpread, mainOuts.RSO);
                    headfire.FireDescription = fire_description(headfire.CrownFractionBurned);
                    headfire.RateOfSpread = final_ros(fuel, mainOuts.FoliarMoistureContent, mainOuts.ISI, headfire.CrownFractionBurned, mainOuts.SurfaceRateOfSpread);
                    headfire.CrownFuelConsumed = crown_consump(fuel, headfire.CrownFractionBurned);
                    headfire.FuelConsumption = headfire.CrownFuelConsumed + mainOuts.SurfaceFuelConsumption;
                    headfire.FireIntensity = fire_intensity(headfire.FuelConsumption, headfire.RateOfSpread);
                }
            }
            if (mainOuts.CoverType == 'n' || firetype == 's')
            {
                headfire.FireDescription = 'S';
                headfire.RateOfSpread = mainOuts.SurfaceRateOfSpread;
                headfire.FuelConsumption = mainOuts.SurfaceFuelConsumption;
                headfire.FireIntensity = mainOuts.SurfaceFireIntensity;
                headfire.CrownFractionBurned = 0.0;
            }
            secondaryOuts.LengthToBreadth = LengthToBreadth(fuel.FuelType, mainOuts.WSV);

            //backfire stuff
            backfire.ISI = backfire_isi(mainOuts);
            backfire.SurfaceRateOfSpread = backfire_ros(input, fuel, mainOuts, backfire.ISI);
            backfire.FireIntensity = fire_behaviour(input, fuel, mainOuts, backfire); //also sets .RateOfSpread

            //flank fire stuff
            flankfire.SurfaceRateOfSpread = flankfire_ros(headfire.SurfaceRateOfSpread, backfire.SurfaceRateOfSpread, secondaryOuts.LengthToBreadth);
            flankfire.RateOfSpread = flankfire_ros(headfire.RateOfSpread, backfire.RateOfSpread, secondaryOuts.LengthToBreadth);
            flankfire.FireIntensity = flank_fire_behaviour(fuel, mainOuts, flankfire);

            //point time evolving fire, should not be needed in WUI-nity as that is taken care in actual transport model
            if (input.Pattern == 1 && input.Time > 0)
            {
                acceleration = Acceleration(fuel, headfire.CrownFractionBurned);
                headfire.Distance = SpreadDistance(input, headfire, acceleration);
                backfire.Distance = SpreadDistance(input, backfire, acceleration);
                flankfire.Distance = flank_spread_distance(input, flankfire, secondaryOuts, headfire.rost, backfire.rost, headfire.Distance, backfire.Distance, secondaryOuts.LengthToBreadth, acceleration);
                headfire.Time = time_to_crown(headfire.RateOfSpread, mainOuts.RSO, acceleration);
                flankfire.Time = time_to_crown(flankfire.RateOfSpread, mainOuts.RSO, acceleration);
                backfire.Time = time_to_crown(backfire.RateOfSpread, mainOuts.RSO, acceleration);
            }
            else
            {
                set_all(headfire, input.Time);
                set_all(flankfire, input.Time);
                set_all(backfire, input.Time);
            }

            secondaryOuts.Area = Area((headfire.Distance + backfire.Distance), flankfire.Distance);
            if (input.Pattern == 1 && input.Time > 0)
            {
                secondaryOuts.Perimeter = Perimeter(headfire, backfire, secondaryOuts, secondaryOuts.lbt);
            }
            else
            {
                secondaryOuts.Perimeter = Perimeter(headfire, backfire, secondaryOuts, secondaryOuts.LengthToBreadth);
            }
        }

        static char get_fueltype_number(string fuelType)
        {
            char cover;

            if (fuelType[0] == 'C' || fuelType[0] == 'M')
            {
                cover = 'c';
            }
            else
            {
                cover = 'n';
            }

            return cover;
        }       

        static double rate_of_spread(CanadianFBPInputs inputs, CanadianFBPFuel fuel, MainOutputs outputs)
        {
            double fw, isz, mult = 0, rsi;
            outputs.ff = ffmc_effect(inputs.FFMC);
            outputs.SpreadAzimuth = inputs.WindAzimuth;
            isz = 0.208 * outputs.ff;

            if (inputs.PercentSlope > 0)
            {
                outputs.WSV = slope_effect(inputs, fuel, outputs, isz);
            }
            else
            {
                outputs.WSV = inputs.WindSpeed;
            }

            if (outputs.WSV < 40.0)
            {
                fw = Mathd.Exp(0.05039 * outputs.WSV);
            }
            else
            {
                fw = 12.0 * (1.0 - Mathd.Exp(-0.0818 * (outputs.WSV - 28)));
            }

            outputs.ISI = isz * fw;
            rsi = ros_calc(inputs, fuel, outputs.ISI, ref mult);
            outputs.SurfaceRateOfSpread = rsi * bui_effect(fuel, outputs, inputs.BUI);
            return (outputs.SurfaceRateOfSpread);
        }

        static double ffmc_effect(double ffmc)
        {
            double mc, ff;
            mc = 147.2 * (101.0 - ffmc) / (59.5 + ffmc);
            ff = 91.9 * Mathd.Exp(-0.1386 * mc) * (1.0 + Mathd.Pow(mc, 5.31) / 49300000.0);
            return ff;
        }

        static double ros_calc(CanadianFBPInputs input, CanadianFBPFuel fuel, double isi, ref double mult)
        {
            double ros;

            if (fuel.FuelType == FuelTypes.O1a || fuel.FuelType == FuelTypes.O1b)
            {
                return grass(fuel, input.PercentCuring, isi, ref mult);
            }

            if (fuel.FuelType == FuelTypes.M1 || fuel.FuelType == FuelTypes.M2)
            {
                return (mixed_wood(fuel, isi, ref mult, fuel.Coefficients.PercentConifer));
            }

            if (fuel.FuelType == FuelTypes.M3 || fuel.FuelType == FuelTypes.M4)
            {
                return (dead_fir(fuel, fuel.Coefficients.PercentDeadFir, isi, ref mult));
            }

            if (fuel.FuelType == FuelTypes.D2)
            {
                return (D2_ROS(fuel, isi, input.BUI, ref mult));
            }

            /* if all else has fail its a conifer   */
            return conifer(fuel, isi, ref mult);
        }


        static double grass(CanadianFBPFuel fuel, double PercentCuring, double isi, ref double mult)
        {
            double mu, ros;
            if ((double)(PercentCuring) >= 58.8)
            {
                mu = 0.176 + 0.02 * ((double)(PercentCuring) - 58.8);
            }
            else
            {
                mu = 0.005 * (Mathd.Exp(0.061 * (double)(PercentCuring)) - 1.0);
            }

            ros = mu * (fuel.Coefficients.a * Mathd.Pow((1.0 - Mathd.Exp(-1.0 * fuel.Coefficients.b * isi)), fuel.Coefficients.c));
            if (mu < 0.001)
            {
                mu = 0.001;  /* to have some value here*/
            }
            mult = mu;
            return (ros);
        }

        static double mixed_wood(CanadianFBPFuel fuel, double InitialSpreadIndex, ref double mu, int pc)
        {
            double ros, mult, ros_d1, ros_c2;
            int i;
            mu = pc / 100.0;
            ros_c2 = fuel.Coefficients.a * Mathd.Pow((1.0 - Mathd.Exp(-1.0 * fuel.Coefficients.b * InitialSpreadIndex)), fuel.Coefficients.c);
            if (fuel.FuelType == FuelTypes.M2)
            {
                mult = 0.2;
            }
            else
            {
                mult = 1.0;
            }

            //swap fuel
            FuelCoefficients fuel2;
            bool success = _fuelCoeffs.TryGetValue("D1", out fuel2);
            if (!success)
            {
                //bad
                //printf(" prob in mixedwood   d1 not found \n"); exit(9);
                return -9999;
            }

            ros_d1 = fuel2.a * Mathd.Pow((1.0 - Mathd.Exp(-1.0 * fuel2.b * InitialSpreadIndex)), fuel2.c);

            ros = (pc / 100.0) * ros_c2 + mult * (100 - pc) / 100.0 * ros_d1;
            return (ros);
        }

        static double dead_fir(CanadianFBPFuel fuel, int pdf, double isi, ref double mu)
        {
            double a, b, c;
            int i;
            double ros, rosm3or4_max, ros_d1, greenness = 1.0;

            if (fuel.FuelType == FuelTypes.M4)
            {
                greenness = 0.2;
            }

            rosm3or4_max = fuel.Coefficients.a * Mathd.Pow((1.0 - Mathd.Exp(-1.0 * fuel.Coefficients.b * isi)), fuel.Coefficients.c);

            //swap fuel
            FuelCoefficients fuel2;
            bool success = _fuelCoeffs.TryGetValue("D1", out fuel2);
            if (!success)
            {
                //bad
                //printf(" prob in mixedwood   d1 not found \n"); exit(9);
                return 0;
            }
            ros_d1 = fuel2.a * Mathd.Pow((1.0 - Mathd.Exp(-1.0 * fuel2.b * isi)), fuel2.c);

            ros = (double)(pdf) / 100.0 * rosm3or4_max + (100.0 - (double)(pdf)) / 100.0 * greenness * ros_d1;

            mu = (double)(pdf) / 100.0;

            return (ros);
        }

        static double D2_ROS(CanadianFBPFuel fuel, double isi, double bui, ref double mu)
        {
            mu = 1.0;
            if (bui >= 80)
            {
                return (fuel.Coefficients.a * Mathd.Pow((1.0 - Mathd.Exp(-1.0 * fuel.Coefficients.b * isi)), fuel.Coefficients.c));
            }
            else
            {
                return (0.0);
            }
        }

        static double conifer(CanadianFBPFuel fuel, double isi, ref double mu)
        {
            mu = 1.0;
            return (fuel.Coefficients.a * Mathd.Pow((1.0 - Mathd.Exp(-1.0 * fuel.Coefficients.b * isi)), fuel.Coefficients.c));
        }

        static double bui_effect(CanadianFBPFuel fuel, MainOutputs at, double bui)
        {
            double bui_avg = 50.0;

            if (bui == 0)
            {
                bui = 1.0;
            }
            at.be = Mathd.Exp(bui_avg * Mathd.Log(fuel.Coefficients.q) * ((1.0 / bui) - (1.0 / fuel.Coefficients.BUI0)));
            return (at.be);
        }

        static double slope_effect(CanadianFBPInputs input, CanadianFBPFuel fuel, MainOutputs output, double isi)
        /* ISI is ISZ really */
        {
            double isf, rsf, wse, percentSlope, rsz, wsx, wsy, wsex, wsey, wsvx, wsvy, wrad, srad, WSV, raz, check, wse2, wse1;
            double mu = 0.0;

            if (input.PercentSlope > 70.0)
            {
                output.SpreadFactor = 10.00;
            }
            else
            {
                output.SpreadFactor = Mathd.Exp(3.533 * Mathd.Pow(input.PercentSlope / 100.0, 1.2));
            }

            if (fuel.FuelType == FuelTypes.M1 || fuel.FuelType == FuelTypes.M2)
            {
                isf = ISF_mixedwood(fuel, isi, fuel.Coefficients.PercentConifer, output.SpreadFactor);
            }
            else if (fuel.FuelType == FuelTypes.M3 || fuel.FuelType == FuelTypes.M4)
            {
                isf = ISF_deadfir(fuel, isi, fuel.Coefficients.PercentDeadFir, output.SpreadFactor);
            }
            else
            {
                rsz = ros_calc(input, fuel, isi, ref mu);
                rsf = rsz * output.SpreadFactor;

                if (rsf > 0.0)
                {
                    check = 1.0 - Mathd.Pow((rsf / (mu * fuel.Coefficients.a)), (1.0 / fuel.Coefficients.c));
                }
                else
                {
                    check = 1.0;
                }

                if (check < slopelimit_isi)
                {
                    check = slopelimit_isi;
                }

                isf = (1.0 / (-1.0 * fuel.Coefficients.b)) * Mathd.Log(check);
            }

            if (isf == 0.0)
            {
                isf = isi;  /* should this be 0.0001 really  */
            }
            wse1 = Mathd.Log(isf / (0.208 * output.ff)) / 0.05039;

            if (wse1 <= 40.0)
            {
                wse = wse1;
            }
            else
            {
                if (isf > (0.999 * 2.496 * output.ff))
                {
                    isf = 0.999 * 2.496 * output.ff;
                }
                wse2 = 28.0 - Mathd.Log(1.0 - isf / (2.496 * output.ff)) / 0.0818;
                wse = wse2;
            }
            wrad = input.WindAzimuth * Mathd.Deg2Rad;
            wsx = input.WindSpeed * Mathd.Sin(wrad);
            wsy = input.WindSpeed * Mathd.Cos(wrad);
            srad = input.SlopeAzimuth  * Mathd.Deg2Rad;
            wsex = wse * Mathd.Sin(srad);
            wsey = wse * Mathd.Cos(srad);
            wsvx = wsx + wsex;
            wsvy = wsy + wsey;
            WSV = Mathd.Sqrt(wsvx * wsvx + wsvy * wsvy);
            raz = Mathd.Acos(wsvy / WSV);
            raz = raz * Mathd.Rad2Deg;

            if (wsvx < 0)
            {
                raz = 360 - raz;
            }
            output.SpreadAzimuth = raz;
            return WSV;
        }

        static double ISF_mixedwood(CanadianFBPFuel fuel, double isz, int pc, double sf)
        {
            double check, mult, rsf_d1, rsf_c2, isf_d1, isf_c2;
            int i;

            rsf_c2 = sf * fuel.Coefficients.a * Mathd.Pow((1.0 - Mathd.Exp(-1.0 * fuel.Coefficients.b * isz)), fuel.Coefficients.c);
            if (rsf_c2 > 0.0)
            {
                check = 1.0 - Mathd.Pow((rsf_c2 / fuel.Coefficients.a), (1.0 / fuel.Coefficients.c));
            }
            else
            { 
                check = 1.0;
            }

            if (check < slopelimit_isi)
            {
                check = slopelimit_isi;
            }
            isf_c2 = (1.0 / (-1.0 * fuel.Coefficients.b)) * Mathd.Log(check);

            if (fuel.FuelType == FuelTypes.M2)
            {
                mult = 0.2;
            }
            else
            {
                mult = 1.0;
            }

            //switch to second fuel
            FuelCoefficients fuel2 = null;
            _fuelCoeffs.TryGetValue("D1", out fuel2);
            if (fuel2 == null)
            {
                //bad
                return -9999;
            }
            rsf_d1 = sf * (mult * fuel2.a) * Mathd.Pow((1.0 - Mathd.Exp(-1.0 * fuel2.b * isz)), fuel2.c);

            if (rsf_d1 > 0.0)
            { check = 1.0 - Mathd.Pow((rsf_d1 / (mult * fuel2.a)), (1.0 / fuel2.c));
            }
            else
            {
                check = 1.0;
            }

            if (check < slopelimit_isi)
            {
                check = slopelimit_isi;
            }
            isf_d1 = (1.0 / (-1.0 * fuel2.b)) * Mathd.Log(check);

            return ((((double)(pc) / 100.0) * isf_c2 + (100 - pc)) / 100.0 * isf_d1);
        }

        static double ISF_deadfir(CanadianFBPFuel fuel, double isz, int pdf, double sf)
        {
            double check, mult, rsf_d1, rsf_max, isf_d1, isf_max;
            int i;

            rsf_max = sf * fuel.Coefficients.a * Mathd.Pow((1.0 - Mathd.Exp(-1.0 * fuel.Coefficients.b * isz)), fuel.Coefficients.c);
            if (rsf_max > 0.0)
            {
                check = 1.0 - Mathd.Pow((rsf_max / (fuel.Coefficients.a)), (1.0 / fuel.Coefficients.c));
            }
            else
            {
                check = 1.0;
            }

            if (check < slopelimit_isi)
            {
                check = slopelimit_isi;
            }
            isf_max = (1.0 / (-1.0 * fuel.Coefficients.b)) * Mathd.Log(check);

            if (fuel.FuelType == FuelTypes.M4)
            {
                mult = 0.2;
            }
            else
            {
                mult = 1.0;
            }

            //switch to second fuel
            FuelCoefficients fuel2 = null;
            _fuelCoeffs.TryGetValue("D1", out fuel2);
            if (fuel2 == null)
            {
                //bad
                return 0;
            }
            rsf_d1 = sf * (mult * fuel2.a) * Mathd.Pow((1.0 - Mathd.Exp(-1.0 * fuel2.b * isz)), fuel2.c);

            if (rsf_d1 > 0.0)
            {
                check = 1.0 - Mathd.Pow((rsf_d1 / (mult * fuel2.a)), (1.0 / fuel2.c));
            }
            else
            {
                check = 1.0;
            }

            if (check < slopelimit_isi)
            {
                check = slopelimit_isi;
            }
            isf_d1 = (1.0 / (-1.0 * fuel2.b)) * Mathd.Log(check);

            return (((double)(pdf) / 100.0) * isf_max + (100.0 - (double)(pdf)) / 100.0 * isf_d1);
        }


        static double fire_intensity(double fc, double ros)
        {
            return (300.0 * fc * ros);
        }

        static double FoliarMoisture(CanadianFBPInputs input, MainOutputs output)
        {
            double LATN;
            int ND;
            output.JulianDay = input.JulianDay;
            output.JulianDayMin = input.JulianDayMin;
            if (input.JulianDayMin <= 0)
            {
                if (input.Elevation < 0)
                {
                    LATN = 46.0 + 23.4 * Mathd.Exp(-0.0360 * (150 - input.Lon));
                    output.JulianDayMin = (int)(0.5 + 151.0 * input.Lat / LATN);
                }
                else
                {
                    LATN = 43.0 + 33.7 * Mathd.Exp(-0.0351 * (150 - input.Lon));
                    output.JulianDayMin = (int)(0.5 + 142.1 * input.Lat / LATN + (0.0172 * input.Elevation));
                }
            }
            ND = Mathd.Abs(input.JulianDay - output.JulianDayMin);
            if (ND >= 50)
            {
                return (120.0);
            }

            if (ND >= 30 && ND < 50)
            {
                return (32.9 + 3.17 * ND - 0.0288 * ND * ND);
            }

            return (85.0 + 0.0189 * ND * ND);
        }

        static double SurfaceFuelConsumption(CanadianFBPInputs input, CanadianFBPFuel fuel)
        {
            double SFC, ffc, wfc, bui, ffmc, sfc_c2, sfc_d1;
            FuelTypes fuelType;
            fuelType = fuel.FuelType;
            bui = input.BUI;
            ffmc = input.FFMC;
            if (fuelType == FuelTypes.C1)
            {
                /*       sfc=1.5*(1.0-Mathd.Exp(-0.23*(ffmc-81.0)));*/
                if (ffmc > 84)
                {
                    SFC = 0.75 + 0.75 * Mathd.Sqrt(1 - Mathd.Exp(-0.23 * (ffmc - 84)));
                }
                else
                {
                    SFC = 0.75 - 0.75 * Mathd.Sqrt(1 - Mathd.Exp(0.23 * (ffmc - 84)));
                }
                return (SFC >= 0 ? SFC : 0.0);
            }
            if (fuelType == FuelTypes.C2|| fuelType == FuelTypes.M3|| fuelType == FuelTypes.M4)
            {
                return (5.0 * (1.0 - Mathd.Exp(-0.0115 * bui)));
            }
            if (fuelType == FuelTypes.C3|| fuelType == FuelTypes.C4)
            {
                return (5.0 * Mathd.Pow((1.0 - Mathd.Exp(-0.0164 * bui)), 2.24));
            }
            if (fuelType == FuelTypes.C5|| fuelType == FuelTypes.C6)
            {
                return (5.0 * Mathd.Pow((1.0 - Mathd.Exp(-0.0149 * bui)), 2.48));
            }                
            if (fuelType == FuelTypes.C7)
            {
                ffc = 2.0 * (1.0 - Mathd.Exp(-0.104 * (ffmc - 70.0)));
                if (ffc < 0)
                {
                    ffc = 0.0;
                }
                wfc = 1.5 * (1.0 - Mathd.Exp(-0.0201 * bui));
                return (ffc + wfc);
            }
            if (fuelType == FuelTypes.O1a || fuelType == FuelTypes.O1b)
            {
                return ((input.GrassFuelLoad) /* change this*/ );
            }
            if (fuelType == FuelTypes.M1 || fuelType == FuelTypes.M2)
            {
                sfc_c2 = 5.0 * (1.0 - Mathd.Exp(-0.0115 * bui));
                sfc_d1 = 1.5 * (1.0 - Mathd.Exp(-0.0183 * bui));
                SFC = fuel.Coefficients.PercentConifer / 100.0 * sfc_c2 + (100.0 - fuel.Coefficients.PercentConifer) / 100.0 * sfc_d1;
                return (SFC);
            }
            if (fuelType == FuelTypes.S1)
            {
                ffc = 4.0 * (1.0 - Mathd.Exp(-0.025 * bui));
                wfc = 4.0 * (1.0 - Mathd.Exp(-0.034 * bui));
                return (ffc + wfc);
            }
            if (fuelType == FuelTypes.S2)
            {
                ffc = 10.0 * (1.0 - Mathd.Exp(-0.013 * bui));
                wfc = 6.0 * (1.0 - Mathd.Exp(-0.060 * bui));
                return (ffc + wfc);
            }
            if (fuelType == FuelTypes.S3)
            {
                ffc = 12.0 * (1.0 - Mathd.Exp(-0.0166 * bui));
                wfc = 20.0 * (1.0 - Mathd.Exp(-0.0210 * bui));
                return (ffc + wfc);
            }
            if (fuelType == FuelTypes.D1)
            {
                return (1.5 * (1.0 - Mathd.Exp(-0.0183 * bui)));
            }
            if (fuelType == FuelTypes.D2)
            {
                return (bui >= 80 ? 1.5 * (1.0 - Mathd.Exp(-0.0183 * bui)) : 0.0);
            }

            //printf("prob in sfc func \n"); exit(9);
            return (-99);
        }


        static double CriticalSurfaceIntensity(CanadianFBPFuel fuel, double fmc)
        {
            return (0.001 * Mathd.Pow(fuel.Coefficients.CrownBaseHeight * (460.0 + 25.9 * fmc), 1.5));
        }

        static double critical_ros(double sfc, double csi)
        {
            if (sfc > 0)
            {
                return (csi / (300.0 * sfc));
            }
            else
            {
                return (0.0);
            }
        }

        static double crown_frac_burn(double rss, double rso)
        {
            double cfb;
            cfb = 1.0 - Mathd.Exp(-0.230 * (rss - rso));
            return (cfb > 0 ? cfb : 0.0);
        }

        static char fire_type(double csi, double sfi)
        {
            return (sfi > csi ? 'c' : 's');
        }

        static char fire_description(double cfb)
        {
            if (cfb < 0.1) return ('S');
            if (cfb < 0.9 && cfb >= 0.1) return ('I');
            if (cfb >= 0.9) return ('C');
            return ('*');
        }

        static double final_ros(CanadianFBPFuel fuel, double fmc, double isi, double cfb, double rss)
        {
            double rsc, ros;
            if (fuel.FuelType == FuelTypes.C6)
            {
                rsc = foliar_mois_effect(isi, fmc);
                ros = rss + cfb * (rsc - rss);
            }
            else ros = rss;
            return (ros);
        }

        static double foliar_mois_effect(double isi, double fmc)
        {
            double fme, rsc, fme_avg = 0.778;
            fme = 1000.0 * Mathd.Pow(1.5 - 0.00275 * fmc, 4.0) / (460.0 + 25.9 * fmc);
            rsc = 60.0 * (1.0 - Mathd.Exp(-0.0497 * isi)) * fme / fme_avg;
            return (rsc);
        }

        static double crown_consump(CanadianFBPFuel fuel, double cfb)
        {
            double cfc;
            cfc = fuel.Coefficients.CrownFuelLoad * cfb;
            if (fuel.FuelType == FuelTypes.M1|| fuel.FuelType == FuelTypes.M2)
            {
                cfc = fuel.Coefficients.PercentConifer / 100.0 * cfc;
            }                
            if (fuel.FuelType == FuelTypes.M3 || fuel.FuelType == FuelTypes.M4)
            {
                cfc = fuel.Coefficients.PercentDeadFir / 100.0 * cfc;
            }
                
            return (cfc);
        }

        static double LengthToBreadth(FuelTypes fuelType, double WSV)
        {
            if (fuelType == FuelTypes.O1a || fuelType == FuelTypes.O1b)
            {
                return (WSV < 1.0 ? 1.0 : (1.1 * Mathd.Pow(WSV, 0.464)));
            }
            else
            {
                return (1.0 + 8.729 * Mathd.Pow(1.0 - Mathd.Exp(-0.030 * WSV), 2.155));
            }            
        }

        static void set_all(FireData fire, int time)
        {
            fire.Time = 0;
            fire.rost = fire.RateOfSpread;
            fire.Distance = time * fire.RateOfSpread;
        }

        static double backfire_isi(MainOutputs output)
        {
            double bfw;
            bfw = Mathd.Exp(-0.05039 * output.WSV);
            return (0.208 * output.ff * bfw);
        }

        static double backfire_ros(CanadianFBPInputs input, CanadianFBPFuel fuel, MainOutputs output, double bisi)
        {
            double mult = 0.0, bros;
            bros = ros_calc(input, fuel, bisi, ref mult);
            bros *= bui_effect(fuel, output, input.BUI);
            return (bros);
        }

        static double Area(double dt, double df)
        {
            double a, b;
            a = dt / 2.0;
            b = df;
            return (a * b * 3.1415926 / 10000.0);
        }

        static double Perimeter(FireData h, FireData b, SecondaryOutputs sec, double lb)
        {
            double mult, p;
            mult = 3.1415926 * (1.0 + 1.0 / lb) * (1.0 + Mathd.Pow(((lb - 1.0) / (2.0 * (lb + 1.0))), 2.0));
            p = (h.Distance + b.Distance) / 2.0 * mult;
            sec.pgr = (h.rost + b.rost) / 2.0 * mult;

            return (p);
        }

        static double Acceleration(CanadianFBPFuel fuel, double CrownFractionBurned)
        {
            int i;
            char canopy = 'c';

            if (fuel.FuelType == FuelTypes.O1a || fuel.FuelType == FuelTypes.O1b || fuel.FuelType == FuelTypes.C1 || fuel.FuelType == FuelTypes.S1 || fuel.FuelType == FuelTypes.S2 || fuel.FuelType == FuelTypes.S3)
            {
                canopy = 'o';
            }

            if (canopy == 'o')
            {
                return (0.115);
            }
            else
            {
                return (0.115 - 18.8 * Mathd.Pow(CrownFractionBurned, 2.5) * Mathd.Exp(-8.0 * CrownFractionBurned));
            }
        }

        static double flankfire_ros(double ros, double bros, double lb)
        {
            return ((ros + bros) / (lb * 2.0));
        }

        static double flank_spread_distance(CanadianFBPInputs input, FireData fire, SecondaryOutputs sec, double hrost, double brost, double hd, double bd, double lb, double a)
        {
            sec.lbt = (lb - 1.0) * (1.0 - Mathd.Exp(-a * input.Time)) + 1.0;
            fire.rost = (hrost + brost) / (sec.lbt * 2.0);
            return ((hd + bd) / (2.0 * sec.lbt));
        }

        static double SpreadDistance(CanadianFBPInputs input, FireData fire, double acceleration)
        {
            fire.rost = fire.RateOfSpread * (1.0 - Mathd.Exp(-acceleration * input.Time));
            return (fire.RateOfSpread * (input.Time + (Mathd.Exp(-acceleration * input.Time) / acceleration) - 1.0 / acceleration));
        }

        static int time_to_crown(double ros, double rso, double acceleration)
        {
            double ratio;
            if (ros > 0) ratio = rso / ros;
            else ratio = 1.1;
            if (ratio > 0.9 && ratio <= 1.0) ratio = 0.9;
            if (ratio < 1.0) return (int)(Mathd.Log(1.0 - ratio) / -acceleration);
            else return (99);
        }

        static double fire_behaviour(CanadianFBPInputs input, CanadianFBPFuel fuel, MainOutputs output, FireData fire)
        {
            double sfi, fi = 0;
            char firetype;
            sfi = fire_intensity(output.SurfaceFuelConsumption, fire.SurfaceRateOfSpread);
            firetype = fire_type(output.CriticalSurfaceIntensity, sfi);
            if (firetype == 'c')
            {
                fire.CrownFractionBurned = crown_frac_burn(fire.SurfaceRateOfSpread, output.RSO);
                fire.FireDescription = fire_description(fire.CrownFractionBurned);
                fire.RateOfSpread = final_ros(fuel, output.FoliarMoistureContent, fire.ISI, fire.CrownFractionBurned, fire.SurfaceRateOfSpread);
                fire.CrownFuelConsumed = crown_consump(fuel, fire.CrownFractionBurned);
                fire.FuelConsumption = fire.CrownFuelConsumed + output.SurfaceFuelConsumption;
                fi = fire_intensity(fire.FuelConsumption, fire.RateOfSpread);
            }
            if (firetype != 'c' || output.CoverType == 'n')
            {
                fire.FuelConsumption = output.SurfaceFuelConsumption;
                fi = sfi;
                fire.CrownFractionBurned = 0.0;
                fire.FireDescription = 'S';
                fire.RateOfSpread = fire.SurfaceRateOfSpread;
            }
            return (fi);
        }

        static double flank_fire_behaviour(CanadianFBPFuel fuel, MainOutputs output, FireData fire)
        {
            double sfi, fi = 0;
            char firetype;
            sfi = fire_intensity(output.SurfaceFuelConsumption, fire.SurfaceRateOfSpread);
            firetype = fire_type(output.CriticalSurfaceIntensity, sfi);
            if (firetype == 'c')
            {
                fire.CrownFractionBurned = crown_frac_burn(fire.SurfaceRateOfSpread, output.RSO);
                fire.FireDescription = fire_description(fire.CrownFractionBurned);
                fire.CrownFuelConsumed = crown_consump(fuel, fire.CrownFractionBurned);
                fire.FuelConsumption = fire.CrownFuelConsumed + output.SurfaceFuelConsumption;
                fi = fire_intensity(fire.FuelConsumption, fire.RateOfSpread);
            }
            if (firetype != 'c' || output.CoverType == 'n')
            {
                fire.FuelConsumption = output.SurfaceFuelConsumption;
                fi = sfi;
                fire.CrownFractionBurned = 0.0;
                fire.FireDescription = 'S';
                /*   f.ros=f.rss;  removed...v4.5   should not have been here ros set in flankfire_ros()  */
            }

            return fi;
        }

        static void zero_main(MainOutputs m)
        {
            m.SurfaceFuelConsumption = 0.0;
            m.CriticalSurfaceIntensity = 0.0; m.RSO = 0.0; m.FoliarMoistureContent = 0; m.SurfaceFireIntensity = 0.0;
            m.SurfaceRateOfSpread = 0.0; m.ISI = 0.0; m.be = 0.0; m.SpreadFactor = 1.0; m.SpreadAzimuth = 0.0; m.WSV = 0.0;
            m.ff = 0.0; m.JulianDay = 0; m.JulianDayMin = 0;
            m.CoverType = ' ';
        }

        static void zero_sec(SecondaryOutputs s)
        {
            s.LengthToBreadth = 0.0;
            s.Area = 0.0;
            s.Perimeter = 0.0;
            s.pgr = 0.0;
        }

        static void zero_fire(FireData a)
        {
            a.RateOfSpread = 0.0;
            a.Distance = 0.0; a.rost = 0.0; a.CrownFractionBurned = 0.0; a.FireIntensity = 0.0;
            a.FuelConsumption = 0.0; a.CrownFuelConsumed = 0.0; a.Time = 0.0;
        }

        private static void CreateDefaultDatabase()
        {
            /*   fuel type 0 */
            FuelCoefficients fuel = new FuelCoefficients();
            fuel.FuelType = "M1";
            fuel.a = 110.0; fuel.b = 0.0282; fuel.c = 1.5;
            fuel.q = 0.80; fuel.BUI0 = 50; fuel.CrownBaseHeight = 6; fuel.CrownFuelLoad = 0.80;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*   fuel type 1 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "M2";
            fuel.a = 110.0; fuel.b = 0.0282; fuel.c = 1.5;
            fuel.q = 0.80; fuel.BUI0 = 50; fuel.CrownBaseHeight = 6; fuel.CrownFuelLoad = 0.80;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*   fuel type 2 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "M3";
            fuel.a = 120.0; fuel.b = 0.0572; fuel.c = 1.4;
            fuel.q = 0.80; fuel.BUI0 = 50; fuel.CrownBaseHeight = 6; fuel.CrownFuelLoad = 0.80;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*   fuel type 3 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "M4";
            fuel.a = 100.0; fuel.b = 0.0404; fuel.c = 1.48;
            fuel.q = 0.80; fuel.BUI0 = 50; fuel.CrownBaseHeight = 6; fuel.CrownFuelLoad = 0.80;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*   fuel type 4 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "C1";
            fuel.a = 90.0; fuel.b = 0.0649; fuel.c = 4.5;
            fuel.q = 0.90; fuel.BUI0 = 72; fuel.CrownBaseHeight = 2; fuel.CrownFuelLoad = 0.75;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*  fuel type 5 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "C2";
            fuel.a = 110.0; fuel.b = 0.0282; fuel.c = 1.5;
            fuel.q = 0.70; fuel.BUI0 = 64; fuel.CrownBaseHeight = 3; fuel.CrownFuelLoad = 0.80;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*   fuel type 6 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "C3";
            fuel.a = 110.0; fuel.b = 0.0444; fuel.c = 3.0;
            fuel.q = 0.75; fuel.BUI0 = 62; fuel.CrownBaseHeight = 8; fuel.CrownFuelLoad = 1.15;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*   fuel type 7 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "C4";
            fuel.a = 110.0; fuel.b = 0.0293; fuel.c = 1.5;
            fuel.q = 0.80; fuel.BUI0 = 66; fuel.CrownBaseHeight = 4; fuel.CrownFuelLoad = 1.20;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*  fuel type 8 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "C5";
            fuel.a = 30.0; fuel.b = 0.0697; fuel.c = 4.0;
            fuel.q = 0.80; fuel.BUI0 = 56; fuel.CrownBaseHeight = 18; fuel.CrownFuelLoad = 1.20;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*  fuel type 9 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "C6";
            fuel.a = 30.0; fuel.b = 0.0800; fuel.c = 3.0;
            fuel.q = 0.80; fuel.BUI0 = 62; fuel.CrownBaseHeight = 7; fuel.CrownFuelLoad = 1.80;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*  fuel type 10 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "C7";
            fuel.a = 45.0; fuel.b = 0.0305; fuel.c = 2.0;
            fuel.q = 0.85; fuel.BUI0 = 106; fuel.CrownBaseHeight = 10; fuel.CrownFuelLoad = 0.50;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*  fuel type 11 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "D1";
            fuel.a = 30.0; fuel.b = 0.0232; fuel.c = 1.6;
            fuel.q = 0.90; fuel.BUI0 = 32; fuel.CrownBaseHeight = 0; fuel.CrownFuelLoad = 0.0;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*  fuel type 12 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "S1";
            fuel.a = 75.0; fuel.b = 0.0297; fuel.c = 1.3;
            fuel.q = 0.75; fuel.BUI0 = 38; fuel.CrownBaseHeight = 0; fuel.CrownFuelLoad = 0.0;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*  fuel type 13 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "S2";
            fuel.a = 40.0; fuel.b = 0.0438; fuel.c = 1.7;
            fuel.q = 0.75; fuel.BUI0 = 63; fuel.CrownBaseHeight = 0; fuel.CrownFuelLoad = 0.0;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*  fuel type 14 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "S3";
            fuel.a = 55.0; fuel.b = 0.0829; fuel.c = 3.2;
            fuel.q = 0.75; fuel.BUI0 = 31; fuel.CrownBaseHeight = 0; fuel.CrownFuelLoad = 0.0;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*  fuel type 15 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "O1a";
            fuel.a = 190.0; fuel.b = 0.0310; fuel.c = 1.40;
            fuel.q = 1.000; fuel.BUI0 = 01; fuel.CrownBaseHeight = 0; fuel.CrownFuelLoad = 0.0;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*  fuel type 16 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "O1b";
            fuel.a = 250.0; fuel.b = 0.0350; fuel.c = 1.7;
            fuel.q = 1.000; fuel.BUI0 = 1; fuel.CrownBaseHeight = 0; fuel.CrownFuelLoad = 0.0;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*  fuel type 17 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "D2";
            fuel.a = 6.0; fuel.b = 0.0232; fuel.c = 1.6;
            fuel.q = 0.90; fuel.BUI0 = 32; fuel.CrownBaseHeight = 0; fuel.CrownFuelLoad = 0.0;
            _fuelCoeffs.Add(fuel.FuelType, fuel);
        }
    }
}
