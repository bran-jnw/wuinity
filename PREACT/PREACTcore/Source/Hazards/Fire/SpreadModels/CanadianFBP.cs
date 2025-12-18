//Adapted from https://github.com/bran-jnw/Cell2Fire/blob/main/cell2fire/Cell2FireC/FBPfunc5_NoDebug.c 2025 by Jonathan Wahlqvist
using System.Collections.Generic;
using PREACT.Math;
using System;
using System.IO;

namespace PREACT.Fire
{  
    public class Inputs
    {
        public double FFMC, WindSpeed, gfl, BUI, Lat, Lon;
        public int Time, Pattern, mon, jd, jd_min, WindAzimuth, PercentSlope, saz, pc, pdf, cur, elev, hour, hourly;
    }

    public class CFBPFuel
    {
        public string DescriptiveName;
        public CanadianFBP.FuelTypes FuelType;
        public FuelCoefficients Coefficients;
        public PREACTColor Color;

        public CFBPFuel(string fuel_type_column, string descriptiveName, PREACTColor color)
        {
            DescriptiveName = descriptiveName;

            if (fuel_type_column.StartsWith("C-1")) FuelType = CanadianFBP.FuelTypes.C1;
            else if (fuel_type_column.StartsWith("C-2")) FuelType = CanadianFBP.FuelTypes.C2;
            else if (fuel_type_column.StartsWith("C-3")) FuelType = CanadianFBP.FuelTypes.C3;
            else if (fuel_type_column.StartsWith("C-4")) FuelType = CanadianFBP.FuelTypes.C4;
            else if (fuel_type_column.StartsWith("C-5")) FuelType = CanadianFBP.FuelTypes.C5;
            else if (fuel_type_column.StartsWith("C-6")) FuelType = CanadianFBP.FuelTypes.C6;
            else if (fuel_type_column.StartsWith("C-7")) FuelType = CanadianFBP.FuelTypes.C7;

            else if (fuel_type_column.StartsWith("D-1")) FuelType = CanadianFBP.FuelTypes.D1;
            else if (fuel_type_column.StartsWith("D-2")) FuelType = CanadianFBP.FuelTypes.D2;

            else if (fuel_type_column.StartsWith("S-1")) FuelType = CanadianFBP.FuelTypes.S1;
            else if (fuel_type_column.StartsWith("S-2")) FuelType = CanadianFBP.FuelTypes.S2;
            else if (fuel_type_column.StartsWith("S-3")) FuelType = CanadianFBP.FuelTypes.S3;

            else if (fuel_type_column.StartsWith("O-1a")) FuelType = CanadianFBP.FuelTypes.O1a;
            else if (fuel_type_column.StartsWith("O-1b")) FuelType = CanadianFBP.FuelTypes.O1b;

            else if (fuel_type_column.StartsWith("M-1")) FuelType = CanadianFBP.FuelTypes.M1;
            else if (fuel_type_column.StartsWith("M-2")) FuelType = CanadianFBP.FuelTypes.M2;
            else if (fuel_type_column.StartsWith("M-3")) FuelType = CanadianFBP.FuelTypes.M3;
            else if (fuel_type_column.StartsWith("M-4")) FuelType = CanadianFBP.FuelTypes.M4;

            Coefficients = new FuelCoefficients(CanadianFBP.GetFuelCoefficients(nameof(FuelType)));

            //extra information that might be required
            int percent_conifer = 0, percent_dead_fir = 0;
            if (FuelType == CanadianFBP.FuelTypes.M1 || FuelType == CanadianFBP.FuelTypes.M2)
            {
                string[] split = fuel_type_column.Split('(');
                if (split.Length > 1)
                {
                    split = split[1].Split(' ');
                    if (int.TryParse(split[0], out percent_conifer)) ;
                }
            }
            Coefficients.PercentConifer = percent_conifer;

            if (FuelType == CanadianFBP.FuelTypes.M3 || FuelType == CanadianFBP.FuelTypes.M4)
            {
                string[] split = fuel_type_column.Split('(');
                if (split.Length > 1)
                {
                    split = split[1].Split(' ');
                    int.TryParse(split[0], out percent_dead_fir);
                }
            }
            Coefficients.PercentDeadFir = percent_dead_fir;
        }
    }

    public class FuelCoefficients
    {        
        public string FuelType = string.Empty;
        public double q, bui0, CrownBaseHeight, cfl;
        public int PercentConifer, PercentDeadFir;
        public double a, b, c;

        public FuelCoefficients()
        {

        }
        
        public FuelCoefficients(FuelCoefficients copyFrom)
        {
            if(copyFrom != null)
            {
                FuelType = copyFrom.FuelType;
                q = copyFrom.q;
                bui0 = copyFrom.bui0;
                CrownBaseHeight = copyFrom.CrownBaseHeight;
                cfl = copyFrom.cfl;
                PercentConifer = copyFrom.PercentConifer;
                PercentDeadFir = copyFrom.PercentDeadFir;
                a = copyFrom.a;
                b = copyFrom.b;
                c = copyFrom.c;
            }            
        }
    }

    public class FireStruct
    {
        public double ros, dist, rost, cfb, fc, cfc, time, rss, isi;
        public char fd;
        public double fi;
    }

    public class MainOutputs
    {
        public double hffmc, sfc, csi, rso, fmc, sfi, rss, isi, be, sf, raz, wsv, ff;
        public int jd_min, jd;
        public char covertype;
    }

    public class SecondaryOutputs
    {
        public double lb, area, perm, pgr, lbt;
    }

    /*   Subroutine of  FBP.C   version 4.4   Aug,2007
    Canadian Forest Fire Behaviour Prediction System
    This code is copyright of the Canadian Forest Service, Natural Resources Canada (1992-2005)
    It is provide free of charge to anyone who wishes to incorporate it within their 
    forest fire management applications, however users should note in their application that
    the FBP calucaltions come from the Canadianf Forest Services Fire Behaviour Prediction
    System. The Canadian Forest Service has gone through considerable testing to ensure that
    these computer functions duplicate the system as laid out in ST-X-3 (The Development
    and Structure of the Canadian Forest Fire Behaviour Prediction System (1992)) and the subsequent corrections and additions to the system (the draft "FBP Note"), however no
    guarentte is given as to the absolute accuracy of the code.
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
    /// <summary>
    /// Canadian Fire Behavior Prediction
    /// </summary>
    public static class CanadianFBP
    {
        public enum FuelTypes { C1, C2, C3, C4, C5, C6, C7, D1, D2, M1, M2, M3, M4, S1, S2, S3, O1a, O1b }
        public enum CoverTypes { Open, }

        private static readonly double slopelimit_isi = 0.01;
        private static readonly int numfuels = 18;      

        private static readonly Dictionary<string, FuelCoefficients> _fuelCoeffs;

        static CanadianFBP()
        {
            _fuelCoeffs = new Dictionary<string, FuelCoefficients>(numfuels);
            CreateDefaultDatabase();
        }

        public static FuelCoefficients GetFuelCoefficients(string fuelType)
        {
            FuelCoefficients f = null;
            _fuelCoeffs.TryGetValue(fuelType, out f);
            return f;
        }

        /// <summary>
        /// Reads the user defined FBP lookup table and creates a database that can be used by the fire spread model.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="success"></param>
        /// <returns></returns>
        public static Dictionary<int, CFBPFuel> CreateFuelDatabaseFromFile(string filePath, out bool success)
        {        
            Engine.Message(null, Engine.LogType.Log, " Attempting to load FBP lookup table.");
            success = false;

            string[] lines;
            if (File.Exists(filePath))
            {
                lines = File.ReadAllLines(filePath);
            }
            else
            {
                Engine.Message(null, Engine.LogType.Warning, "FBP lookup table file " + filePath + " not found.");
                return null;
            }

            if(lines.Length < 2)
            {
                Engine.Message(null, Engine.LogType.Warning, "FBP lookup table file " + filePath + " does not contain data.");
                return null;
            }

            Dictionary<int, CFBPFuel> result = new Dictionary<int, CFBPFuel>();

            //skip first line as that is just the header
            for (int i = 1; i < lines.Length; i++)
            {
                string[] columns = lines[i].Split(',');
                //make sure there is some data and not just empty line
                if (columns.Length > 1)
                {
                    int grid_value, export_value;
                    string descriptive_name, fuel_type_column;
                    int r, g, b, h, s, l;

                    int.TryParse(columns[0], out grid_value);
                    int.TryParse(columns[1], out export_value);
                    descriptive_name = columns[2];
                    fuel_type_column = columns[3];
                    int.TryParse(columns[4], out r);
                    int.TryParse(columns[5], out g);
                    int.TryParse(columns[6], out b);
                    //int.TryParse(columns[7], out h);
                    //int.TryParse(columns[8], out s);
                    //int.TryParse(columns[9], out l);

                    PREACTColor c = new PREACTColor(r, g, b);
                    CFBPFuel f = new CFBPFuel(fuel_type_column, descriptive_name, c); 
                    result.Add(grid_value, f);
                }
            }

            if (result.Count == 0)
            {
                result = null;
                success = false;
            }
            else
            {
                success = true;
            }

            return result;
        }

        public static void Calculate(Inputs input, CFBPFuel fuel, MainOutputs mainOuts, SecondaryOutputs secondaryOuts, FireStruct headfire, FireStruct flankfire, FireStruct backfire)
        {
            char firetype = ' ';
            double accn;
            zero_main(mainOuts);
            zero_sec(secondaryOuts);
            zero_fire(headfire);
            zero_fire(flankfire);
            zero_fire(backfire);
            mainOuts.covertype = get_fueltype_number(fuel.Coefficients.FuelType);
            mainOuts.ff = ffmc_effect(input.FFMC);
            mainOuts.rss = rate_of_spread(input, fuel, mainOuts);
            headfire.rss = mainOuts.rss;
            mainOuts.sfc = surf_fuel_consump(input, fuel);
            mainOuts.sfi = fire_intensity(mainOuts.sfc, mainOuts.rss);            

            if (mainOuts.covertype == 'c')
            {
                mainOuts.fmc = foliar_moisture(input, mainOuts);
                mainOuts.csi = crit_surf_intensity(fuel, mainOuts.fmc);
                mainOuts.rso = critical_ros(mainOuts.sfc, mainOuts.csi);
                firetype = fire_type(mainOuts.csi, mainOuts.sfi);


                if (firetype == 'c')
                {
                    headfire.cfb = crown_frac_burn(mainOuts.rss, mainOuts.rso);
                    headfire.fd = fire_description(headfire.cfb);
                    headfire.ros = final_ros(fuel, mainOuts.fmc, mainOuts.isi, headfire.cfb, mainOuts.rss);
                    headfire.cfc = crown_consump(fuel, headfire.cfb);
                    headfire.fc = headfire.cfc + mainOuts.sfc;
                    headfire.fi = fire_intensity(headfire.fc, headfire.ros);
                }
            }
            if (mainOuts.covertype == 'n' || firetype == 's')
            {
                headfire.fd = 'S';
                headfire.ros = mainOuts.rss;
                headfire.fc = mainOuts.sfc;
                headfire.fi = mainOuts.sfi;
                headfire.cfb = 0.0;
            }
            secondaryOuts.lb = l_to_b(fuel.FuelType, mainOuts.wsv);
            backfire.isi = backfire_isi(mainOuts);
            backfire.rss = backfire_ros(input, fuel, mainOuts, backfire.isi);
            flankfire.rss = flankfire_ros(headfire.rss, backfire.rss, secondaryOuts.lb);
            backfire.fi = fire_behaviour(input, fuel, mainOuts, backfire);
            flankfire.ros = flankfire_ros(headfire.ros, backfire.ros, secondaryOuts.lb);
            flankfire.fi = flank_fire_behaviour(fuel, mainOuts, flankfire);

            if (input.Pattern == 1 && input.Time > 0)
            {
                accn = acceleration(fuel, headfire.cfb);
                headfire.dist = spread_distance(input, headfire, accn);
                backfire.dist = spread_distance(input, backfire, accn);
                flankfire.dist = flank_spread_distance(input, flankfire, secondaryOuts, headfire.rost, backfire.rost, headfire.dist, backfire.dist, secondaryOuts.lb, accn);
                headfire.time = time_to_crown(headfire.ros, mainOuts.rso, accn);
                flankfire.time = time_to_crown(flankfire.ros, mainOuts.rso, accn);
                backfire.time = time_to_crown(backfire.ros, mainOuts.rso, accn);
            }
            else
            {
                set_all(headfire, input.Time);
                set_all(flankfire, input.Time);
                set_all(backfire, input.Time);
            }
            secondaryOuts.area = area((headfire.dist + backfire.dist), flankfire.dist);
            if (input.Pattern == 1 && input.Time > 0)
            {
                secondaryOuts.perm = perimeter(headfire, backfire, secondaryOuts, secondaryOuts.lbt);
            }
            else
            {
                secondaryOuts.perm = perimeter(headfire, backfire, secondaryOuts, secondaryOuts.lb);
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

            return (cover);
        }

        static double ffmc_effect(double ffmc)
        {
            double mc, ff;
            mc = 147.2 * (101.0 - ffmc) / (59.5 + ffmc);
            ff = 91.9 * Mathd.Exp(-0.1386 * mc) * (1 + Mathd.Pow(mc, 5.31) / 49300000.0);
            return ff;
        }

        static double rate_of_spread(Inputs inputs, CFBPFuel fuel, MainOutputs outputs)
        {
            double fw, isz, mult = 0, rsi;
            outputs.ff = ffmc_effect(inputs.FFMC);
            outputs.raz = inputs.WindAzimuth;
            isz = 0.208 * outputs.ff;

            if (inputs.PercentSlope > 0)
            {
                outputs.wsv = slope_effect(inputs, fuel, outputs, isz);
            }
            else
            {
                outputs.wsv = inputs.WindSpeed;
            }

            if (outputs.wsv < 40.0)
            {
                fw = Mathd.Exp(0.05039 * outputs.wsv);
            }
            else
            {
                fw = 12.0 * (1.0 - Mathd.Exp(-0.0818 * (outputs.wsv - 28)));
            }

            outputs.isi = isz * fw;
            rsi = ros_calc(inputs, fuel, outputs.isi, ref mult);
            outputs.rss = rsi * bui_effect(fuel, outputs, inputs.BUI);
            return (outputs.rss);
        }

        static double ros_calc(Inputs input, CFBPFuel fuel, double isi, ref double mult)
        {
            double ros;

            if (fuel.FuelType == FuelTypes.O1a || fuel.FuelType == FuelTypes.O1b)
            {
                return grass(fuel, input.cur, isi, ref mult);
            }

            if (fuel.FuelType == FuelTypes.M1 || fuel.FuelType == FuelTypes.M2)
            {
                return (mixed_wood(fuel, isi, ref mult, input.pc));
            }

            if (fuel.FuelType == FuelTypes.M3 || fuel.FuelType == FuelTypes.M4)
            {
                return (dead_fir(fuel, input.pdf, isi, ref mult));
            }

            if (fuel.FuelType == FuelTypes.D2)
            {
                return (D2_ROS(fuel, isi, input.BUI, ref mult));
            }

            /* if all else has fail its a conifer   */
            return conifer(fuel, isi, ref mult);
        }


        static double grass(CFBPFuel fuel, double cur, double isi, ref double mult)
        {
            double mu, ros;
            if ((double)(cur) >= 58.8)
            {
                mu = 0.176 + 0.02 * ((double)(cur) - 58.8);
            }
            else
            {
                mu = 0.005 * (Mathd.Exp(0.061 * (double)(cur)) - 1.0);
            }

            ros = mu * (fuel.Coefficients.a * Mathd.Pow((1.0 - Mathd.Exp(-1.0 * fuel.Coefficients.b * isi)), fuel.Coefficients.c));
            if (mu < 0.001)
            {
                mu = 0.001;  /* to have some value here*/
            }
            mult = mu;
            return (ros);
        }

        static double mixed_wood(CFBPFuel fuel, double InitialSpreadIndex, ref double mu, int pc)
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

        static double dead_fir(CFBPFuel fuel, int pdf, double isi, ref double mu)
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

        static double D2_ROS(CFBPFuel fuel, double isi, double bui, ref double mu)
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

        static double conifer(CFBPFuel fuel, double isi, ref double mu)
        {
            mu = 1.0;
            return (fuel.Coefficients.a * Mathd.Pow((1.0 - Mathd.Exp(-1.0 * fuel.Coefficients.b * isi)), fuel.Coefficients.c));
        }

        static double bui_effect(CFBPFuel fuel, MainOutputs at, double bui)
        {
            double bui_avg = 50.0;

            if (bui == 0)
            {
                bui = 1.0;
            }
            at.be = Mathd.Exp(bui_avg * Mathd.Log(fuel.Coefficients.q) * ((1.0 / bui) - (1.0 / fuel.Coefficients.bui0)));
            return (at.be);
        }

        static double slope_effect(Inputs input, CFBPFuel fuel, MainOutputs output, double isi)
        /* ISI is ISZ really */
        {
            double isf, rsf, wse, percentSlope, rsz, wsx, wsy, wsex, wsey, wsvx, wsvy, wrad, srad, wsv, raz, check, wse2, wse1;
            double mu = 0.0;

            percentSlope = input.PercentSlope * 1.0;
            if (percentSlope > 70.0)
            {
                percentSlope = 70.0;   /* edited in version 4.6*/
            }
            output.sf = Mathd.Exp(3.533 * Mathd.Pow(percentSlope / 100.0, 1.2));

            if (output.sf > 10.0)
            {
                output.sf = 10.00;  /* added to ensure maximum is correct in version 4.6  */
            }

            if (fuel.FuelType == FuelTypes.M1 || fuel.FuelType == FuelTypes.M2)
            {
                isf = ISF_mixedwood(fuel, isi, fuel.Coefficients.PercentConifer, output.sf);
            }
            else if (fuel.FuelType == FuelTypes.M3 || fuel.FuelType == FuelTypes.M4)
            {
                isf = ISF_deadfir(fuel, isi, input.pdf, output.sf);
            }
            else
            {
                rsz = ros_calc(input, fuel, isi, ref mu);
                rsf = rsz * output.sf;

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

            if (isf == 0.0) isf = isi;  /* should this be 0.0001 really  */
            wse1 = Mathd.Log(isf / (0.208 * output.ff)) / 0.05039;
            if (wse1 <= 40.0)
            {
                wse = wse1;
            }
            else
            {
                if (isf > (0.999 * 2.496 * output.ff)) isf = 0.999 * 2.496 * output.ff;
                wse2 = 28.0 - Mathd.Log(1.0 - isf / (2.496 * output.ff)) / 0.0818;
                wse = wse2;
            }
            wrad = input.WindAzimuth / 180.0 * 3.1415926;
            wsx = input.WindSpeed * Mathd.Sin(wrad);
            wsy = input.WindSpeed * Mathd.Cos(wrad);
            srad = input.saz / 180.0 * 3.1415926;
            wsex = wse * Mathd.Sin(srad);
            wsey = wse * Mathd.Cos(srad);
            wsvx = wsx + wsex;
            wsvy = wsy + wsey;
            wsv = Mathd.Sqrt(wsvx * wsvx + wsvy * wsvy);
            raz = Mathd.Acos(wsvy / wsv);
            raz = raz / 3.1415926 * 180.0;
            if (wsvx < 0)
            {
                raz = 360 - raz;
            }
            output.raz = raz;
            return ((double)(wsv));
        }

        static double ISF_mixedwood(CFBPFuel fuel, double isz, int pc, double sf)
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

        static double ISF_deadfir(CFBPFuel fuel, double isz, int pdf, double sf)
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

        static double foliar_moisture(Inputs inp, MainOutputs at)
        {
            double latn;
            int nd;
            at.jd = inp.jd;
            at.jd_min = inp.jd_min;
            if (inp.jd_min <= 0)
            {
                if (inp.elev < 0)
                {
                    latn = 23.4 * Mathd.Exp(-0.0360 * (150 - inp.Lon)) + 46.0;
                    at.jd_min = (int)(0.5 + 151.0 * inp.Lat / latn);
                }
                else
                {
                    latn = 33.7 * Mathd.Exp(-0.0351 * (150 - inp.Lon)) + 43.0;
                    at.jd_min = (int)(0.5 + 142.1 * inp.Lat / latn + (0.0172 * inp.elev));
                }
            }
            nd = Mathd.Abs(inp.jd - at.jd_min);
            if (nd >= 50) return (120.0);
            if (nd >= 30 && nd < 50) return (32.9 + 3.17 * nd - 0.0288 * nd * nd);
            return (85.0 + 0.0189 * nd * nd);
        }

        static double surf_fuel_consump(Inputs inp, CFBPFuel fuel)
        {
            double sfc, ffc, wfc, bui, ffmc, sfc_c2, sfc_d1;
            FuelTypes ft;
            ft = fuel.FuelType;
            bui = inp.BUI;
            ffmc = inp.FFMC;
            if (ft == FuelTypes.C1)
            {
                /*       sfc=1.5*(1.0-Mathd.Exp(-0.23*(ffmc-81.0)));*/
                if (ffmc > 84)
                {
                    sfc = 0.75 + 0.75 * Mathd.Sqrt(1 - Mathd.Exp(-0.23 * (ffmc - 84)));
                }
                else
                {
                    sfc = 0.75 - 0.75 * Mathd.Sqrt(1 - Mathd.Exp(0.23 * (ffmc - 84)));
                }
                return (sfc >= 0 ? sfc : 0.0);
            }
            if (ft == FuelTypes.C2|| ft == FuelTypes.M3|| ft == FuelTypes.M4)
            {
                return (5.0 * (1.0 - Mathd.Exp(-0.0115 * bui)));
            }
            if (ft == FuelTypes.C3|| ft == FuelTypes.C4)
            {
                return (5.0 * Mathd.Pow((1.0 - Mathd.Exp(-0.0164 * bui)), 2.24));
            }
            if (ft == FuelTypes.C5|| ft == FuelTypes.C6)
            {
                return (5.0 * Mathd.Pow((1.0 - Mathd.Exp(-0.0149 * bui)), 2.48));
            }                
            if (ft == FuelTypes.C7)
            {
                ffc = 2.0 * (1.0 - Mathd.Exp(-0.104 * (ffmc - 70.0)));
                if (ffc < 0)
                {
                    ffc = 0.0;
                }
                wfc = 1.5 * (1.0 - Mathd.Exp(-0.0201 * bui));
                return (ffc + wfc);
            }
            if (ft == FuelTypes.O1a || ft == FuelTypes.O1b)
            {
                return ((inp.gfl) /* change this*/ );
            }
            if (ft == FuelTypes.M1 || ft == FuelTypes.M2)
            {
                sfc_c2 = 5.0 * (1.0 - Mathd.Exp(-0.0115 * bui));
                sfc_d1 = 1.5 * (1.0 - Mathd.Exp(-0.0183 * bui));
                sfc = inp.pc / 100.0 * sfc_c2 + (100.0 - inp.pc) / 100.0 * sfc_d1;
                return (sfc);
            }
            if (ft == FuelTypes.S1)
            {
                ffc = 4.0 * (1.0 - Mathd.Exp(-0.025 * bui));
                wfc = 4.0 * (1.0 - Mathd.Exp(-0.034 * bui));
                return (ffc + wfc);
            }
            if (ft == FuelTypes.S2)
            {
                ffc = 10.0 * (1.0 - Mathd.Exp(-0.013 * bui));
                wfc = 6.0 * (1.0 - Mathd.Exp(-0.060 * bui));
                return (ffc + wfc);
            }
            if (ft == FuelTypes.S3)
            {
                ffc = 12.0 * (1.0 - Mathd.Exp(-0.0166 * bui));
                wfc = 20.0 * (1.0 - Mathd.Exp(-0.0210 * bui));
                return (ffc + wfc);
            }
            if (ft == FuelTypes.D1)
            {
                return (1.5 * (1.0 - Mathd.Exp(-0.0183 * bui)));
            }
            if (ft == FuelTypes.D2)
            {
                return (bui >= 80 ? 1.5 * (1.0 - Mathd.Exp(-0.0183 * bui)) : 0.0);
            }

            //printf("prob in sfc func \n"); exit(9);
            return (-99);
        }


        static double crit_surf_intensity(CFBPFuel fuel, double fmc)
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

        static double final_ros(CFBPFuel fuel, double fmc, double isi, double cfb, double rss)
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

        static double crown_consump(CFBPFuel fuel, double cfb)
        {
            double cfc;
            cfc = fuel.Coefficients.cfl * cfb;
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
        static double l_to_b(FuelTypes fuelType, double ws)
        {
            if (fuelType == FuelTypes.O1a || fuelType == FuelTypes.O1b)
            {
                return (ws < 1.0 ? 1.0 : (1.1 * Mathd.Pow(ws, 0.464)));
            }
            else
            {
                return (1.0 + 8.729 * Mathd.Pow(1.0 - Mathd.Exp(-0.030 * ws), 2.155));
            }            
        }

        static void set_all(FireStruct ptr, int time)
        {
            ptr.time = 0;
            ptr.rost = ptr.ros;
            ptr.dist = time * ptr.ros;
        }

        static double backfire_isi(MainOutputs at)
        {
            double bfw;
            bfw = Mathd.Exp(-0.05039 * at.wsv);
            return (0.208 * at.ff * bfw);
        }

        static double backfire_ros(Inputs inp, CFBPFuel fuel, MainOutputs at, double bisi)
        {
            double mult = 0.0, bros;
            bros = ros_calc(inp, fuel, bisi, ref mult);
            bros *= bui_effect(fuel, at, inp.BUI);
            return (bros);
        }

        static double area(double dt, double df)
        {
            double a, b;
            a = dt / 2.0;
            b = df;
            return (a * b * 3.1415926 / 10000.0);
        }

        static double perimeter(FireStruct h, FireStruct b, SecondaryOutputs sec, double lb)
        {
            double mult, p;
            mult = 3.1415926 * (1.0 + 1.0 / lb) * (1.0 + Mathd.Pow(((lb - 1.0) / (2.0 * (lb + 1.0))), 2.0));
            p = (h.dist + b.dist) / 2.0 * mult;
            sec.pgr = (h.rost + b.rost) / 2.0 * mult;

            return (p);
        }

        static double acceleration(CFBPFuel fuel, double cfb)
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
                return (0.115 - 18.8 * Mathd.Pow(cfb, 2.5) * Mathd.Exp(-8.0 * cfb));
            }
        }

        static double flankfire_ros(double ros, double bros, double lb)
        {
            return ((ros + bros) / (lb * 2.0));
        }

        static double flank_spread_distance(Inputs inp, FireStruct ptr, SecondaryOutputs sec, double hrost, double brost, double hd, double bd, double lb, double a)
        {
            sec.lbt = (lb - 1.0) * (1.0 - Mathd.Exp(-a * inp.Time)) + 1.0;
            ptr.rost = (hrost + brost) / (sec.lbt * 2.0);
            return ((hd + bd) / (2.0 * sec.lbt));
        }

        static double spread_distance(Inputs inp, FireStruct ptr, double a)
        {
            ptr.rost = ptr.ros * (1.0 - Mathd.Exp(-a * inp.Time));
            return (ptr.ros * (inp.Time + (Mathd.Exp(-a * inp.Time) / a) - 1.0 / a));
        }

        static int time_to_crown(double ros, double rso, double a)
        {
            double ratio;
            if (ros > 0) ratio = rso / ros;
            else ratio = 1.1;
            if (ratio > 0.9 && ratio <= 1.0) ratio = 0.9;
            if (ratio < 1.0) return (int)(Mathd.Log(1.0 - ratio) / -a);
            else return (99);
        }

        static double fire_behaviour(Inputs input, CFBPFuel fuel, MainOutputs output, FireStruct f)
        {
            double sfi, fi = 0;
            char firetype;
            sfi = fire_intensity(output.sfc, f.rss);
            firetype = fire_type(output.csi, sfi);
            if (firetype == 'c')
            {
                f.cfb = crown_frac_burn(f.rss, output.rso);
                f.fd = fire_description(f.cfb);
                f.ros = final_ros(fuel, output.fmc, f.isi, f.cfb, f.rss);
                f.cfc = crown_consump(fuel, f.cfb);
                f.fc = f.cfc + output.sfc;
                fi = fire_intensity(f.fc, f.ros);
            }
            if (firetype != 'c' || output.covertype == 'n')
            {
                f.fc = output.sfc;
                fi = sfi;
                f.cfb = 0.0;
                f.fd = 'S';
                f.ros = f.rss;
            }
            return (fi);
        }

        static double flank_fire_behaviour(CFBPFuel fuel, MainOutputs at, FireStruct f)
        {
            double sfi, fi = 0;
            char firetype;
            sfi = fire_intensity(at.sfc, f.rss);
            firetype = fire_type(at.csi, sfi);
            if (firetype == 'c')
            {
                f.cfb = crown_frac_burn(f.rss, at.rso);
                f.fd = fire_description(f.cfb);
                f.cfc = crown_consump(fuel, f.cfb);
                f.fc = f.cfc + at.sfc;
                fi = fire_intensity(f.fc, f.ros);
            }
            if (firetype != 'c' || at.covertype == 'n')
            {
                f.fc = at.sfc;
                fi = sfi;
                f.cfb = 0.0;
                f.fd = 'S';
                /*   f.ros=f.rss;  removed...v4.5   should not have been here ros set in flankfire_ros()  */
            }
            return (fi);
        }

        static void zero_main(MainOutputs m)
        {
            m.sfc = 0.0;
            m.csi = 0.0; m.rso = 0.0; m.fmc = 0; m.sfi = 0.0;
            m.rss = 0.0; m.isi = 0.0; m.be = 0.0; m.sf = 1.0; m.raz = 0.0; m.wsv = 0.0;
            m.ff = 0.0; m.jd = 0; m.jd_min = 0;
            m.covertype = ' ';
        }

        static void zero_sec(SecondaryOutputs s)
        {
            s.lb = 0.0;
            s.area = 0.0;
            s.perm = 0.0;
            s.pgr = 0.0;
        }

        static void zero_fire(FireStruct a)
        {
            a.ros = 0.0;
            a.dist = 0.0; a.rost = 0.0; a.cfb = 0.0; a.fi = 0.0;
            a.fc = 0.0; a.cfc = 0.0; a.time = 0.0;
        }

        private static void CreateDefaultDatabase()
        {
            /*   fuel type 0 */
            FuelCoefficients fuel = new FuelCoefficients();
            fuel.FuelType = "M1";
            fuel.a = 110.0; fuel.b = 0.0282; fuel.c = 1.5;
            fuel.q = 0.80; fuel.bui0 = 50; fuel.CrownBaseHeight = 6; fuel.cfl = 0.80;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*   fuel type 1 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "M2";
            fuel.a = 110.0; fuel.b = 0.0282; fuel.c = 1.5;
            fuel.q = 0.80; fuel.bui0 = 50; fuel.CrownBaseHeight = 6; fuel.cfl = 0.80;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*   fuel type 2 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "M3";
            fuel.a = 120.0; fuel.b = 0.0572; fuel.c = 1.4;
            fuel.q = 0.80; fuel.bui0 = 50; fuel.CrownBaseHeight = 6; fuel.cfl = 0.80;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*   fuel type 3 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "M4";
            fuel.a = 100.0; fuel.b = 0.0404; fuel.c = 1.48;
            fuel.q = 0.80; fuel.bui0 = 50; fuel.CrownBaseHeight = 6; fuel.cfl = 0.80;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*   fuel type 4 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "C1";
            fuel.a = 90.0; fuel.b = 0.0649; fuel.c = 4.5;
            fuel.q = 0.90; fuel.bui0 = 72; fuel.CrownBaseHeight = 2; fuel.cfl = 0.75;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*  fuel type 5 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "C2";
            fuel.a = 110.0; fuel.b = 0.0282; fuel.c = 1.5;
            fuel.q = 0.70; fuel.bui0 = 64; fuel.CrownBaseHeight = 3; fuel.cfl = 0.80;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*   fuel type 6 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "C3";
            fuel.a = 110.0; fuel.b = 0.0444; fuel.c = 3.0;
            fuel.q = 0.75; fuel.bui0 = 62; fuel.CrownBaseHeight = 8; fuel.cfl = 1.15;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*   fuel type 7 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "C4";
            fuel.a = 110.0; fuel.b = 0.0293; fuel.c = 1.5;
            fuel.q = 0.80; fuel.bui0 = 66; fuel.CrownBaseHeight = 4; fuel.cfl = 1.20;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*  fuel type 8 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "C5";
            fuel.a = 30.0; fuel.b = 0.0697; fuel.c = 4.0;
            fuel.q = 0.80; fuel.bui0 = 56; fuel.CrownBaseHeight = 18; fuel.cfl = 1.20;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*  fuel type 9 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "C6";
            fuel.a = 30.0; fuel.b = 0.0800; fuel.c = 3.0;
            fuel.q = 0.80; fuel.bui0 = 62; fuel.CrownBaseHeight = 7; fuel.cfl = 1.80;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*  fuel type 10 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "C7";
            fuel.a = 45.0; fuel.b = 0.0305; fuel.c = 2.0;
            fuel.q = 0.85; fuel.bui0 = 106; fuel.CrownBaseHeight = 10; fuel.cfl = 0.50;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*  fuel type 11 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "D1";
            fuel.a = 30.0; fuel.b = 0.0232; fuel.c = 1.6;
            fuel.q = 0.90; fuel.bui0 = 32; fuel.CrownBaseHeight = 0; fuel.cfl = 0.0;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*  fuel type 12 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "S1";
            fuel.a = 75.0; fuel.b = 0.0297; fuel.c = 1.3;
            fuel.q = 0.75; fuel.bui0 = 38; fuel.CrownBaseHeight = 0; fuel.cfl = 0.0;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*  fuel type 13 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "S2";
            fuel.a = 40.0; fuel.b = 0.0438; fuel.c = 1.7;
            fuel.q = 0.75; fuel.bui0 = 63; fuel.CrownBaseHeight = 0; fuel.cfl = 0.0;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*  fuel type 14 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "S3";
            fuel.a = 55.0; fuel.b = 0.0829; fuel.c = 3.2;
            fuel.q = 0.75; fuel.bui0 = 31; fuel.CrownBaseHeight = 0; fuel.cfl = 0.0;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*  fuel type 15 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "O1a";
            fuel.a = 190.0; fuel.b = 0.0310; fuel.c = 1.40;
            fuel.q = 1.000; fuel.bui0 = 01; fuel.CrownBaseHeight = 0; fuel.cfl = 0.0;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*  fuel type 16 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "O1b";
            fuel.a = 250.0; fuel.b = 0.0350; fuel.c = 1.7;
            fuel.q = 1.000; fuel.bui0 = 1; fuel.CrownBaseHeight = 0; fuel.cfl = 0.0;
            _fuelCoeffs.Add(fuel.FuelType, fuel);

            /*  fuel type 17 */
            fuel = new FuelCoefficients();
            fuel.FuelType = "D2";
            fuel.a = 6.0; fuel.b = 0.0232; fuel.c = 1.6;
            fuel.q = 0.90; fuel.bui0 = 32; fuel.CrownBaseHeight = 0; fuel.cfl = 0.0;
            _fuelCoeffs.Add(fuel.FuelType, fuel);
        }
    }
}
