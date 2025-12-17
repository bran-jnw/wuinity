//Adapted from https://github.com/bran-jnw/Cell2Fire/blob/main/cell2fire/Cell2FireC/FBPfunc5_NoDebug.c 2025 by Jonathan Wahlqvist
using System.Collections.Generic;
using PREACT.Math;

namespace PREACT.Fire
{
    public class inputs
    {
        public string fueltype = string.Empty;
        public double ffmc, ws, gfl, bui, lat, lon;
        public int time, pattern, mon, jd, jd_min, waz, ps, saz, pc,
             pdf, cur, elev, hour, hourly;
    }

    public class fuel_coefs
    {
        public string fueltype = string.Empty;
        public double q, bui0, cbh, cfl;
        public double a, b, c;
    }

    public class fire_struc
    {
        public double ros, dist, rost, cfb, fc, cfc, time, rss, isi;
        public char fd;
        public double fi;
    }

    public class main_outs
    {
        public double hffmc, sfc, csi, rso, fmc, sfi, rss, isi, be, sf, raz, wsv, ff;
        public int jd_min, jd;
        public char covertype;
    }

    public class snd_outs
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
    public class CanadianFBP
    {
        static readonly double slopelimit_isi = 0.01;
        static readonly int numfuels = 18;
        static readonly string version = "Last modified June 2015,  by BMW ";
        static readonly string ver = "Version 5.0001  ";       

        Dictionary<string, fuel_coefs> _fuels;

        public CanadianFBP()
        {
            setup_const();
        }

        public void calculate(inputs input, fuel_coefs[] fuels, main_outs mainOuts, snd_outs secondaryOuts, fire_struc headfire, fire_struc flankfire, fire_struc backfire)
        {
            char firetype = ' ';
            double accn;
            zero_main(mainOuts);
            zero_sec(secondaryOuts);
            zero_fire(headfire);
            zero_fire(flankfire);
            zero_fire(backfire);
            mainOuts.covertype = get_fueltype_number(input.fueltype);
            mainOuts.ff = ffmc_effect(input.ffmc);
            mainOuts.rss = rate_of_spread(input, fuels, mainOuts);
            headfire.rss = mainOuts.rss;
            mainOuts.sfc = surf_fuel_consump(input);
            mainOuts.sfi = fire_intensity(mainOuts.sfc, mainOuts.rss);

            if (mainOuts.covertype == 'c')
            {
                mainOuts.fmc = foliar_moisture(input, mainOuts);
                mainOuts.csi = crit_surf_intensity(fuels, mainOuts.fmc);
                mainOuts.rso = critical_ros(input.fueltype, mainOuts.sfc, mainOuts.csi);
                firetype = fire_type(mainOuts.csi, mainOuts.sfi);


                if (firetype == 'c')
                {
                    headfire.cfb = crown_frac_burn(mainOuts.rss, mainOuts.rso);
                    headfire.fd = fire_description(headfire.cfb);
                    headfire.ros = final_ros(input, mainOuts.fmc, mainOuts.isi, headfire.cfb, mainOuts.rss);
                    headfire.cfc = crown_consump(input, fuels, headfire.cfb);
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
            secondaryOuts.lb = l_to_b(input.fueltype, mainOuts.wsv);
            backfire.isi = backfire_isi(mainOuts);
            backfire.rss = backfire_ros(input, fuels, mainOuts, backfire.isi);
            flankfire.rss = flankfire_ros(headfire.rss, backfire.rss, secondaryOuts.lb);
            backfire.fi = fire_behaviour(input, fuels, mainOuts, backfire);
            flankfire.ros = flankfire_ros(headfire.ros, backfire.ros, secondaryOuts.lb);
            flankfire.fi = flank_fire_behaviour(input, fuels, mainOuts, flankfire);

            if (input.pattern == 1 && input.time > 0)
            {
                accn = acceleration(input, headfire.cfb);
                headfire.dist = spread_distance(input, headfire, accn);
                backfire.dist = spread_distance(input, backfire, accn);
                flankfire.dist = flank_spread_distance(input, flankfire, secondaryOuts, headfire.rost, backfire.rost, headfire.dist, backfire.dist, secondaryOuts.lb, accn);
                headfire.time = time_to_crown(headfire.ros, mainOuts.rso, accn);
                flankfire.time = time_to_crown(flankfire.ros, mainOuts.rso, accn);
                backfire.time = time_to_crown(backfire.ros, mainOuts.rso, accn);
            }
            else
            {
                set_all(headfire, input.time);
                set_all(flankfire, input.time);
                set_all(backfire, input.time);
            }
            secondaryOuts.area = area((headfire.dist + backfire.dist), flankfire.dist);
            if (input.pattern == 1 && input.time > 0) secondaryOuts.perm = perimeter(headfire, backfire, secondaryOuts, secondaryOuts.lbt);
            else secondaryOuts.perm = perimeter(headfire, backfire, secondaryOuts, secondaryOuts.lb);
        }

        private void setup_const()
        {
            _fuels = new Dictionary<string, fuel_coefs>(numfuels);

            /*   fuel type 0 */
            fuel_coefs fuel = new fuel_coefs();
            fuel.fueltype = "M1";
            fuel.a = 110.0; fuel.b = 0.0282; fuel.c = 1.5;
            fuel.q = 0.80; fuel.bui0 = 50; fuel.cbh = 6; fuel.cfl = 0.80;
            _fuels.Add(fuel.fueltype, fuel);

            /*   fuel type 1 */
            fuel = new fuel_coefs();
            fuel.fueltype = "M2";
            fuel.a = 110.0; fuel.b = 0.0282; fuel.c = 1.5;
            fuel.q = 0.80; fuel.bui0 = 50; fuel.cbh = 6; fuel.cfl = 0.80;
            _fuels.Add(fuel.fueltype, fuel);

            /*   fuel type 2 */
            fuel = new fuel_coefs();
            fuel.fueltype = "M3";
            fuel.a = 120.0; fuel.b = 0.0572; fuel.c = 1.4;
            fuel.q = 0.80; fuel.bui0 = 50; fuel.cbh = 6; fuel.cfl = 0.80;
            _fuels.Add(fuel.fueltype, fuel);

            /*   fuel type 3 */
            fuel = new fuel_coefs();
            fuel.fueltype = "M4";
            fuel.a = 100.0; fuel.b = 0.0404; fuel.c = 1.48;
            fuel.q = 0.80; fuel.bui0 = 50; fuel.cbh = 6; fuel.cfl = 0.80;
            _fuels.Add(fuel.fueltype, fuel);

            /*   fuel type 4 */
            fuel = new fuel_coefs();
            fuel.fueltype = "C1";
            fuel.a = 90.0; fuel.b = 0.0649; fuel.c = 4.5;
            fuel.q = 0.90; fuel.bui0 = 72; fuel.cbh = 2; fuel.cfl = 0.75;
            _fuels.Add(fuel.fueltype, fuel);

            /*  fuel type 5 */
            fuel = new fuel_coefs();
            fuel.fueltype = "C2";
            fuel.a = 110.0; fuel.b = 0.0282; fuel.c = 1.5;
            fuel.q = 0.70; fuel.bui0 = 64; fuel.cbh = 3; fuel.cfl = 0.80;
            _fuels.Add(fuel.fueltype, fuel);

            /*   fuel type 6 */
            fuel = new fuel_coefs();
            fuel.fueltype = "C3";
            fuel.a = 110.0; fuel.b = 0.0444; fuel.c = 3.0;
            fuel.q = 0.75; fuel.bui0 = 62; fuel.cbh = 8; fuel.cfl = 1.15;
            _fuels.Add(fuel.fueltype, fuel);

            /*   fuel type 7 */
            fuel = new fuel_coefs();
            fuel.fueltype = "C4";
            fuel.a = 110.0; fuel.b = 0.0293; fuel.c = 1.5;
            fuel.q = 0.80; fuel.bui0 = 66; fuel.cbh = 4; fuel.cfl = 1.20;
            _fuels.Add(fuel.fueltype, fuel);

            /*  fuel type 8 */
            fuel = new fuel_coefs();
            fuel.fueltype = "C5";
            fuel.a = 30.0; fuel.b = 0.0697; fuel.c = 4.0;
            fuel.q = 0.80; fuel.bui0 = 56; fuel.cbh = 18; fuel.cfl = 1.20;
            _fuels.Add(fuel.fueltype, fuel);

            /*  fuel type 9 */
            fuel = new fuel_coefs();
            fuel.fueltype = "C6";
            fuel.a = 30.0; fuel.b = 0.0800; fuel.c = 3.0;
            fuel.q = 0.80; fuel.bui0 = 62; fuel.cbh = 7; fuel.cfl = 1.80;
            _fuels.Add(fuel.fueltype, fuel);

            /*  fuel type 10 */
            fuel = new fuel_coefs();
            fuel.fueltype = "C7";
            fuel.a = 45.0; fuel.b = 0.0305; fuel.c = 2.0;
            fuel.q = 0.85; fuel.bui0 = 106; fuel.cbh = 10; fuel.cfl = 0.50;
            _fuels.Add(fuel.fueltype, fuel);

            /*  fuel type 11 */
            fuel = new fuel_coefs();
            fuel.fueltype = "D1";
            fuel.a = 30.0; fuel.b = 0.0232; fuel.c = 1.6;
            fuel.q = 0.90; fuel.bui0 = 32; fuel.cbh = 0; fuel.cfl = 0.0;
            _fuels.Add(fuel.fueltype, fuel);

            /*  fuel type 12 */
            fuel = new fuel_coefs();
            fuel.fueltype = "S1";
            fuel.a = 75.0; fuel.b = 0.0297; fuel.c = 1.3;
            fuel.q = 0.75; fuel.bui0 = 38; fuel.cbh = 0; fuel.cfl = 0.0;
            _fuels.Add(fuel.fueltype, fuel);

            /*  fuel type 13 */
            fuel = new fuel_coefs();
            fuel.fueltype = "S2";
            fuel.a = 40.0; fuel.b = 0.0438; fuel.c = 1.7;
            fuel.q = 0.75; fuel.bui0 = 63; fuel.cbh = 0; fuel.cfl = 0.0;
            _fuels.Add(fuel.fueltype, fuel);

            /*  fuel type 14 */
            fuel = new fuel_coefs();
            fuel.fueltype = "S3";
            fuel.a = 55.0; fuel.b = 0.0829; fuel.c = 3.2;
            fuel.q = 0.75; fuel.bui0 = 31; fuel.cbh = 0; fuel.cfl = 0.0;
            _fuels.Add(fuel.fueltype, fuel);

            /*  fuel type 15 */
            fuel = new fuel_coefs();
            fuel.fueltype = "O1a";
            fuel.a = 190.0; fuel.b = 0.0310; fuel.c = 1.40;
            fuel.q = 1.000; fuel.bui0 = 01; fuel.cbh = 0; fuel.cfl = 0.0;
            _fuels.Add(fuel.fueltype, fuel);

            /*  fuel type 16 */
            fuel = new fuel_coefs();
            fuel.fueltype = "O1b";
            fuel.a = 250.0; fuel.b = 0.0350; fuel.c = 1.7;
            fuel.q = 1.000; fuel.bui0 = 1; fuel.cbh = 0; fuel.cfl = 0.0;
            _fuels.Add(fuel.fueltype, fuel);

            /*  fuel type 17 */
            fuel = new fuel_coefs();
            fuel.fueltype = "D2";
            fuel.a = 6.0; fuel.b = 0.0232; fuel.c = 1.6;
            fuel.q = 0.90; fuel.bui0 = 32; fuel.cbh = 0; fuel.cfl = 0.0;
            _fuels.Add(fuel.fueltype, fuel);
        }

        char get_fueltype_number(string fuel)
        {
            int i;
            char cover = ' ';

            fuel_coefs f;
            if (_fuels.TryGetValue(fuel, out f))
            {
                if (fuel[0] == 'C' || fuel[0] == 'M')
                {
                    cover = 'c';
                }
                else
                {
                    cover = 'n';
                }
            }
            return (cover);
        }

        double ffmc_effect(double ffmc)
        {
            double mc, ff;
            mc = 147.2 * (101.0 - ffmc) / (59.5 + ffmc);
            ff = 91.9 * Mathd.Exp(-0.1386 * mc) * (1 + Mathd.Pow(mc, 5.31) / 49300000.0);
            return ff;
        }

        double rate_of_spread(inputs inp, fuel_coefs[] ptr, main_outs at)
        {
            double fw, isz, mult = 0, rsi;
            at.ff = ffmc_effect(inp.ffmc);
            at.raz = inp.waz;
            isz = 0.208 * at.ff;
            if (inp.ps > 0) at.wsv = slope_effect(inp, ptr, at, isz);
            else at.wsv = inp.ws;
            if (at.wsv < 40.0) fw = Mathd.Exp(0.05039 * at.wsv);
            else fw = 12.0 * (1.0 - Mathd.Exp(-0.0818 * (at.wsv - 28)));
            at.isi = isz * fw;
            rsi = ros_calc(inp, ptr, at.isi, ref mult);
            at.rss = rsi * bui_effect(ptr, at, inp.bui);
            return (at.rss);
        }

        double ros_calc(inputs inp, fuel_coefs[] ptr, double isi, ref double mult)
        {
            double ros;
            if (inp.fueltype == "O1")
            {
                return grass(ptr, inp.cur, isi, ref mult);
            }
            if (inp.fueltype == "M1" || inp.fueltype == "M2")
            {
                return (mixed_wood(ptr, isi, ref mult, inp.pc));
            }
            if (inp.fueltype == "M3" || inp.fueltype == "M4")
            {
                return (dead_fir(ptr, inp.pdf, isi, ref mult));
            }
            if (inp.fueltype == "D2")
            {
                return (D2_ROS(ptr, isi, inp.bui, ref mult));
            }

            /* if all else has fail its a conifer   */
            return conifer(ptr, isi, ref mult);
        }


        double grass(fuel_coefs[] ptr, double cur, double isi, ref double mult)
        {
            double mu, ros;
            if ((double)(cur) >= 58.8) mu = 0.176 + 0.02 * ((double)(cur) - 58.8);
            else mu = 0.005 * (Mathd.Exp(0.061 * (double)(cur)) - 1.0);
            ros = mu * (ptr[0].a * Mathd.Pow((1.0 - Mathd.Exp(-1.0 * ptr[0].b * isi)), ptr[0].c));
            if (mu < 0.001) mu = 0.001;  /* to have some value here*/
            mult = mu;
            return (ros);
        }

        double mixed_wood(fuel_coefs[] ptr, double isi, ref double mu, int pc)
        {
            double ros, mult, ros_d1, ros_c2;
            int i;
            mu = pc / 100.0;
            ros_c2 = ptr[0].a * Mathd.Pow((1.0 - Mathd.Exp(-1.0 * ptr[0].b * isi)), ptr[0].c);
            if (ptr[0].fueltype == "M2")
            {
                mult = 0.2;
            }
            else
            {
                mult = 1.0;
            }


            if (ptr[1].fueltype != "D1")
            {
                //bad
                //printf(" prob in mixedwood   d1 not found \n"); exit(9);
                return 0;
            }

            ros_d1 = ptr[1].a * Mathd.Pow((1.0 - Mathd.Exp(-1.0 * ptr[1].b * isi)), ptr[1].c);

            ros = (pc / 100.0) * ros_c2 + mult * (100 - pc) / 100.0 * ros_d1;
            return (ros);
        }

        double dead_fir(fuel_coefs[] ptr, int pdf, double isi, ref double mu)
        {
            double a, b, c;
            int i;
            double ros, rosm3or4_max, ros_d1, greenness = 1.0;

            if (ptr[0].fueltype == "M4")
            {
                greenness = 0.2;
            }

            rosm3or4_max = ptr[0].a * Mathd.Pow((1.0 - Mathd.Exp(-1.0 * ptr[0].b * isi)), ptr[0].c);

            if (ptr[1].fueltype != "D1")
            {
                //bad
                //printf(" prob in mixedwood   d1 not found \n"); exit(9);
                return 0;
            }
            ros_d1 = ptr[1].a * Mathd.Pow((1.0 - Mathd.Exp(-1.0 * ptr[1].b * isi)), ptr[1].c);

            ros = (double)(pdf) / 100.0 * rosm3or4_max + (100.0 - (double)(pdf)) / 100.0 * greenness * ros_d1;

            mu = (double)(pdf) / 100.0;

            return (ros);
        }

        double D2_ROS(fuel_coefs[] ptr, double isi, double bui, ref double mu)
        {
            mu = 1.0;
            if (bui >= 80)
            {
                return (ptr[0].a * Mathd.Pow((1.0 - Mathd.Exp(-1.0 * ptr[0].b * isi)), ptr[0].c));
            }
            else
            {
                return (0.0);
            }
        }

        double conifer(fuel_coefs[] ptr, double isi, ref double mu)
        {
            mu = 1.0;
            return (ptr[0].a * Mathd.Pow((1.0 - Mathd.Exp(-1.0 * ptr[0].b * isi)), ptr[0].c));
        }

        double bui_effect(fuel_coefs[] ptr, main_outs at, double bui)
        {
            double bui_avg = 50.0;

            if (bui == 0)
            {
                bui = 1.0;
            }
            at.be = Mathd.Exp(bui_avg * Mathd.Log(ptr[0].q) * ((1.0 / bui) - (1.0 / ptr[0].bui0)));
            return (at.be);
        }

        double slope_effect(inputs inp, fuel_coefs[] ptr, main_outs at, double isi)
        /* ISI is ISZ really */
        {
            double isf, rsf, wse, ps, rsz, wsx, wsy, wsex, wsey, wsvx, wsvy,
                wrad, srad, wsv, raz, check, wse2, wse1;
            double mu = 0.0;
            ps = inp.ps * 1.0;

            if (ps > 70.0)
            {
                ps = 70.0;   /* edited in version 4.6*/
            }
            at.sf = Mathd.Exp(3.533 * Mathd.Pow(ps / 100.0, 1.2));

            if (at.sf > 10.0)
            {
                at.sf = 10.00;  /* added to ensure maximum is correct in version 4.6  */
            }

            if (ptr[0].fueltype == "M1" || ptr[0].fueltype == "M2")
            {
                isf = ISF_mixedwood(ptr, isi, inp.pc, at.sf);
            }
            else if (ptr[0].fueltype == "M3" || ptr[0].fueltype == "M4")
            {
                isf = ISF_deadfir(ptr, isi, inp.pdf, at.sf);
            }
            else
            {
                rsz = ros_calc(inp, ptr, isi, ref mu);
                rsf = rsz * at.sf;

                if (rsf > 0.0)
                {
                    check = 1.0 - Mathd.Pow((rsf / (mu * ptr[0].a)), (1.0 / ptr[0].c));
                }
                else
                {
                    check = 1.0;
                }

                if (check < slopelimit_isi)
                {
                    check = slopelimit_isi;
                }

                isf = (1.0 / (-1.0 * ptr[0].b)) * Mathd.Log(check);
            }

            if (isf == 0.0) isf = isi;  /* should this be 0.0001 really  */
            wse1 = Mathd.Log(isf / (0.208 * at.ff)) / 0.05039;
            if (wse1 <= 40.0)
            {
                wse = wse1;
            }
            else
            {
                if (isf > (0.999 * 2.496 * at.ff)) isf = 0.999 * 2.496 * at.ff;
                wse2 = 28.0 - Mathd.Log(1.0 - isf / (2.496 * at.ff)) / 0.0818;
                wse = wse2;
            }
            wrad = inp.waz / 180.0 * 3.1415926;
            wsx = inp.ws * Mathd.Sin(wrad);
            wsy = inp.ws * Mathd.Cos(wrad);
            srad = inp.saz / 180.0 * 3.1415926;
            wsex = wse * Mathd.Sin(srad);
            wsey = wse * Mathd.Cos(srad);
            wsvx = wsx + wsex;
            wsvy = wsy + wsey;
            wsv = Mathd.Sqrt(wsvx * wsvx + wsvy * wsvy);
            raz = Mathd.Acos(wsvy / wsv);
            raz = raz / 3.1415926 * 180.0;
            if (wsvx < 0) raz = 360 - raz;
            at.raz = raz;
            return ((double)(wsv));
        }

        double ISF_mixedwood(fuel_coefs[] fuels, double isz, int pc, double sf)
        {
            double check, mult, rsf_d1, rsf_c2, isf_d1, isf_c2;
            int i;

            rsf_c2 = sf * fuels[0].a * Mathd.Pow((1.0 - Mathd.Exp(-1.0 * fuels[0].b * isz)), fuels[0].c);
            if (rsf_c2 > 0.0)
            {
                check = 1.0 - Mathd.Pow((rsf_c2 / (fuels[0].a)), (1.0 / fuels[0].c));
            }
            else
            { 
                check = 1.0;
            }

            if (check < slopelimit_isi)
            {
                check = slopelimit_isi;
            }
            isf_c2 = (1.0 / (-1.0 * fuels[0].b)) * Mathd.Log(check);

            if (fuels[0].fueltype == "M2")
            {
                mult = 0.2;
            }
            else
            {
                mult = 1.0;
            }

            if (fuels[1].fueltype != "D1")
            {
                //bad
                return 0;
            }
            rsf_d1 = sf * (mult * fuels[1].a) * Mathd.Pow((1.0 - Mathd.Exp(-1.0 * fuels[1].b * isz)), fuels[1].c);

            if (rsf_d1 > 0.0)
            { check = 1.0 - Mathd.Pow((rsf_d1 / (mult * fuels[1].a)), (1.0 / fuels[1].c));
            }
            else
            {
                check = 1.0;
            }

            if (check < slopelimit_isi)
            {
                check = slopelimit_isi;
            }
            isf_d1 = (1.0 / (-1.0 * fuels[1].b)) * Mathd.Log(check);

            return ((((double)(pc) / 100.0) * isf_c2 + (100 - pc)) / 100.0 * isf_d1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ptr">Assumes first fuel in array is M type fuel, second D1.</param>
        /// <param name="isz"></param>
        /// <param name="pdf"></param>
        /// <param name="sf"></param>
        /// <returns></returns>
        double ISF_deadfir(fuel_coefs[] ptr, double isz, int pdf, double sf)
        {
            double check, mult, rsf_d1, rsf_max, isf_d1, isf_max;
            int i;

            rsf_max = sf * ptr[0].a * Mathd.Pow((1.0 - Mathd.Exp(-1.0 * ptr[0].b * isz)), ptr[0].c);
            if (rsf_max > 0.0)
            {
                check = 1.0 - Mathd.Pow((rsf_max / (ptr[0].a)), (1.0 / ptr[0].c));
            }
            else
            {
                check = 1.0;
            }

            if (check < slopelimit_isi)
            {
                check = slopelimit_isi;
            }
            isf_max = (1.0 / (-1.0 * ptr[0].b)) * Mathd.Log(check);

            if (ptr[0].fueltype == "M4")
            {
                mult = 0.2;
            }
            else
            {
                mult = 1.0;
            }

            if (ptr[1].fueltype != "D1")
            {
                //bad
                return 0;
            }
            rsf_d1 = sf * (mult * ptr[1].a) * Mathd.Pow((1.0 - Mathd.Exp(-1.0 * ptr[1].b * isz)), ptr[1].c);

            if (rsf_d1 > 0.0)
            {
                check = 1.0 - Mathd.Pow((rsf_d1 / (mult * ptr[1].a)), (1.0 / ptr[1].c));
            }
            else
            {
                check = 1.0;
            }

            if (check < slopelimit_isi)
            {
                check = slopelimit_isi;
            }
            isf_d1 = (1.0 / (-1.0 * ptr[1].b)) * Mathd.Log(check);

            return (((double)(pdf) / 100.0) * isf_max + (100.0 - (double)(pdf)) / 100.0 * isf_d1);
        }


        double fire_intensity(double fc, double ros)
        {
            return (300.0 * fc * ros);
        }

        double foliar_moisture(inputs inp, main_outs at)
        {
            double latn;
            int nd;
            at.jd = inp.jd;
            at.jd_min = inp.jd_min;
            if (inp.jd_min <= 0)
            {
                if (inp.elev < 0)
                {
                    latn = 23.4 * Mathd.Exp(-0.0360 * (150 - inp.lon)) + 46.0;
                    at.jd_min = (int)(0.5 + 151.0 * inp.lat / latn);
                }
                else
                {
                    latn = 33.7 * Mathd.Exp(-0.0351 * (150 - inp.lon)) + 43.0;
                    at.jd_min = (int)(0.5 + 142.1 * inp.lat / latn + (0.0172 * inp.elev));
                }
            }
            nd = Mathd.Abs(inp.jd - at.jd_min);
            if (nd >= 50) return (120.0);
            if (nd >= 30 && nd < 50) return (32.9 + 3.17 * nd - 0.0288 * nd * nd);
            return (85.0 + 0.0189 * nd * nd);
        }

        double surf_fuel_consump(inputs inp)
        {
            double sfc, ffc, wfc, bui, ffmc, sfc_c2, sfc_d1;
            string ft;
            ft = inp.fueltype;
            bui = inp.bui;
            ffmc = inp.ffmc;
            if (ft == "C1")
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
            if (ft == "C2" || ft == "M3" || ft == "M4")
            {
                return (5.0 * (1.0 - Mathd.Exp(-0.0115 * bui)));
            }
            if (ft == "C3" || ft == "C4")
            {
                return (5.0 * Mathd.Pow((1.0 - Mathd.Exp(-0.0164 * bui)), 2.24));
            }
            if (ft == "C5" || ft == "C6")
            {
                return (5.0 * Mathd.Pow((1.0 - Mathd.Exp(-0.0149 * bui)), 2.48));
            }                
            if (ft == "C7")
            {
                ffc = 2.0 * (1.0 - Mathd.Exp(-0.104 * (ffmc - 70.0)));
                if (ffc < 0)
                {
                    ffc = 0.0;
                }
                wfc = 1.5 * (1.0 - Mathd.Exp(-0.0201 * bui));
                return (ffc + wfc);
            }
            if (ft == "O1")
            {
                return ((inp.gfl) /* change this*/ );
            }
            if (ft =="M1" || ft == "M2")
            {
                sfc_c2 = 5.0 * (1.0 - Mathd.Exp(-0.0115 * bui));
                sfc_d1 = 1.5 * (1.0 - Mathd.Exp(-0.0183 * bui));
                sfc = inp.pc / 100.0 * sfc_c2 + (100.0 - inp.pc) / 100.0 * sfc_d1;
                return (sfc);
            }
            if (ft == "S1")
            {
                ffc = 4.0 * (1.0 - Mathd.Exp(-0.025 * bui));
                wfc = 4.0 * (1.0 - Mathd.Exp(-0.034 * bui));
                return (ffc + wfc);
            }
            if (ft == "S2")
            {
                ffc = 10.0 * (1.0 - Mathd.Exp(-0.013 * bui));
                wfc = 6.0 * (1.0 - Mathd.Exp(-0.060 * bui));
                return (ffc + wfc);
            }
            if (ft == "S3")
            {
                ffc = 12.0 * (1.0 - Mathd.Exp(-0.0166 * bui));
                wfc = 20.0 * (1.0 - Mathd.Exp(-0.0210 * bui));
                return (ffc + wfc);
            }
            if (ft == "D1")
            {
                return (1.5 * (1.0 - Mathd.Exp(-0.0183 * bui)));
            }
            if (ft == "D2")
            {
                return (bui >= 80 ? 1.5 * (1.0 - Mathd.Exp(-0.0183 * bui)) : 0.0);
            }

            //printf("prob in sfc func \n"); exit(9);
            return (-99);
        }


        double crit_surf_intensity(fuel_coefs[] ptr, double fmc)
        {
            return (0.001 * Mathd.Pow(ptr[0].cbh * (460.0 + 25.9 * fmc), 1.5));
        }

        double critical_ros(string ft, double sfc, double csi)
        {
            if (sfc > 0) return (csi / (300.0 * sfc));
            else return (0.0);
        }

        double crown_frac_burn(double rss, double rso)
        {
            double cfb;
            cfb = 1.0 - Mathd.Exp(-0.230 * (rss - rso));
            return (cfb > 0 ? cfb : 0.0);
        }

        char fire_type(double csi, double sfi)
        {
            return (sfi > csi ? 'c' : 's');
        }

        char fire_description(double cfb)
        {
            if (cfb < 0.1) return ('S');
            if (cfb < 0.9 && cfb >= 0.1) return ('I');
            if (cfb >= 0.9) return ('C');
            return ('*');
        }

        double final_ros(inputs inp, double fmc, double isi, double cfb, double rss)
        {
            double rsc, ros;
            if (inp.fueltype == "C6")
            {
                rsc = foliar_mois_effect(isi, fmc);
                ros = rss + cfb * (rsc - rss);
            }
            else ros = rss;
            return (ros);
        }

        double foliar_mois_effect(double isi, double fmc)
        {
            double fme, rsc, fme_avg = 0.778;
            fme = 1000.0 * Mathd.Pow(1.5 - 0.00275 * fmc, 4.0) / (460.0 + 25.9 * fmc);
            rsc = 60.0 * (1.0 - Mathd.Exp(-0.0497 * isi)) * fme / fme_avg;
            return (rsc);
        }

        double crown_consump(inputs inp, fuel_coefs[] ptr, double cfb)
        {
            double cfc;
            cfc = ptr[0].cfl * cfb;
            if (inp.fueltype == "M1" || inp.fueltype == "M2")
            {
                cfc = inp.pc / 100.0 * cfc;
            }                
            if (inp.fueltype == "M3" || inp.fueltype == "M4")
            {
                cfc = inp.pdf / 100.0 * cfc;
            }
                
            return (cfc);
        }


        double l_to_b(string ft, double ws)
        {
            if (ft == "O1")
            {
                return (ws < 1.0 ? 1.0 : (1.1 * Mathd.Pow(ws, 0.464)));
            }
            else
            {
                return (1.0 + 8.729 * Mathd.Pow(1.0 - Mathd.Exp(-0.030 * ws), 2.155));
            }            
        }

        void set_all(fire_struc ptr, int time)
        {
            ptr.time = 0;
            ptr.rost = ptr.ros;
            ptr.dist = time * ptr.ros;
        }
        double backfire_isi(main_outs at)
        {
            double bfw;
            bfw = Mathd.Exp(-0.05039 * at.wsv);
            return (0.208 * at.ff * bfw);
        }

        double backfire_ros(inputs inp, fuel_coefs[] ptr, main_outs at, double bisi)
        {
            double mult = 0.0, bros;
            bros = ros_calc(inp, ptr, bisi, ref mult);
            bros *= bui_effect(ptr, at, inp.bui);
            return (bros);
        }

        double area(double dt, double df)
        {
            double a, b;
            a = dt / 2.0;
            b = df;
            return (a * b * 3.1415926 / 10000.0);
        }

        double perimeter(fire_struc h, fire_struc b, snd_outs sec, double lb)
        {
            double mult, p;
            mult = 3.1415926 * (1.0 + 1.0 / lb) * (1.0 + Mathd.Pow(((lb - 1.0) / (2.0 * (lb + 1.0))), 2.0));
            p = (h.dist + b.dist) / 2.0 * mult;
            sec.pgr = (h.rost + b.rost) / 2.0 * mult;

            return (p);
        }

        double acceleration(inputs inp, double cfb)
        {
            int i;
            char canopy = 'c';

            if (inp.fueltype == "O1" || inp.fueltype == "C1" || inp.fueltype == "S1" || inp.fueltype == "S2" || inp.fueltype == "S3")
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

        double flankfire_ros(double ros, double bros, double lb)
        {
            return ((ros + bros) / (lb * 2.0));
        }

        double flank_spread_distance(inputs inp, fire_struc ptr, snd_outs sec, double hrost, double brost, double hd, double bd, double lb, double a)
        {
            sec.lbt = (lb - 1.0) * (1.0 - Mathd.Exp(-a * inp.time)) + 1.0;
            ptr.rost = (hrost + brost) / (sec.lbt * 2.0);
            return ((hd + bd) / (2.0 * sec.lbt));
        }

        double spread_distance(inputs inp, fire_struc ptr, double a)
        {
            ptr.rost = ptr.ros * (1.0 - Mathd.Exp(-a * inp.time));
            return (ptr.ros * (inp.time + (Mathd.Exp(-a * inp.time) / a) - 1.0 / a));
        }

        int time_to_crown(double ros, double rso, double a)
        {
            double ratio;
            if (ros > 0) ratio = rso / ros;
            else ratio = 1.1;
            if (ratio > 0.9 && ratio <= 1.0) ratio = 0.9;
            if (ratio < 1.0) return (int)(Mathd.Log(1.0 - ratio) / -a);
            else return (99);
        }

        double fire_behaviour(inputs inp, fuel_coefs[] ptr, main_outs at, fire_struc f)
        {
            double sfi, fi = 0;
            char firetype;
            sfi = fire_intensity(at.sfc, f.rss);
            firetype = fire_type(at.csi, sfi);
            if (firetype == 'c')
            {
                f.cfb = crown_frac_burn(f.rss, at.rso);
                f.fd = fire_description(f.cfb);
                f.ros = final_ros(inp, at.fmc, f.isi, f.cfb, f.rss);
                f.cfc = crown_consump(inp, ptr, f.cfb);
                f.fc = f.cfc + at.sfc;
                fi = fire_intensity(f.fc, f.ros);
            }
            if (firetype != 'c' || at.covertype == 'n')
            {
                f.fc = at.sfc;
                fi = sfi;
                f.cfb = 0.0;
                f.fd = 'S';
                f.ros = f.rss;
            }
            return (fi);
        }

        double flank_fire_behaviour(inputs inp, fuel_coefs[] ptr, main_outs at, fire_struc f)
        {
            double sfi, fi = 0;
            char firetype;
            sfi = fire_intensity(at.sfc, f.rss);
            firetype = fire_type(at.csi, sfi);
            if (firetype == 'c')
            {
                f.cfb = crown_frac_burn(f.rss, at.rso);
                f.fd = fire_description(f.cfb);
                f.cfc = crown_consump(inp, ptr, f.cfb);
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


        void zero_main(main_outs m)
        {
            m.sfc = 0.0;
            m.csi = 0.0; m.rso = 0.0; m.fmc = 0; m.sfi = 0.0;
            m.rss = 0.0; m.isi = 0.0; m.be = 0.0; m.sf = 1.0; m.raz = 0.0; m.wsv = 0.0;
            m.ff = 0.0; m.jd = 0; m.jd_min = 0;
            m.covertype = ' ';
        }

        void zero_sec(snd_outs s)
        {
            s.lb = 0.0;
            s.area = 0.0;
            s.perm = 0.0;
            s.pgr = 0.0;
        }

        void zero_fire(fire_struc a)
        {
            a.ros = 0.0;
            a.dist = 0.0; a.rost = 0.0; a.cfb = 0.0; a.fi = 0.0;
            a.fc = 0.0; a.cfc = 0.0; a.time = 0.0;
        }
    }
}
