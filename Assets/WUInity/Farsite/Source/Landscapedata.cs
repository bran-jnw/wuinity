using System.Collections;
using System.Collections.Generic;
using System;
/*
namespace Farsite
{
    public class celldata
    {
        // structure for holding basic cell information
        short e;                 // elevation
        short s;                 // slope
        short a;                 // aspect
        short f;                 // fuel models
        short c;                 // canopy cover
    };


    public class crowndata
    {
        // structure for holding optional crown fuel information
        short h;                // canopy height
        short b;                // crown base
        short p;				// bulk density
    }


    public class grounddata
    {
        // structure for holding duff and woody fuel information
        short d;                // duff model
        short w;				// coarse woody model
    }


    public class CanopyCharacteristics
    {
        // contains average values, landscape wide, temporary until themes are used
        double DefaultHeight;
        double DefaultBase;
        double DefaultDensity;
        double Height;
        double CrownBase;
        double BulkDensity;
        double Diameter;
        double FoliarMC;
        long Tolerance;
        long Species;

        CanopyCharacteristics()
        {

        }
    }


    public class LandscapeStruct
    {
        public short elev;
        public short slope;
        public short aspect;
        public short fuel;
        public short cover;               // READ OR DERIVED FROM LANDSCAPE DATA
        public double aspectf;
        public double height;
        public double baseE; //JW added E since base is taken in C#
        public double density;
        public double duff;
        public long woody;
    }

    public class LandscapeData
    {
        Farsite5 pFarsite;
        public LandscapeStruct ld;

        //helper
        double modf(double x, ref double ipart)
        {
            ipart = Math.Floor(x);
            return x - ipart;
        }
        

        public LandscapeData(Farsite5 _pFarsite)
        {
            pFarsite = _pFarsite;
        }

        void FuelConvert(short fl)
        {
            ld.fuel = pFarsite.GetFuelConversion(fl);
        }


        void ElevConvert(short el)
        {
            ld.elev = el;
            if (ld.elev == -9999)
                return;

            switch (pFarsite.GetTheme_Units(E_DATA))
            {
                case 0:
                    // meters default
                    break;
                case 1:
                    // feet
                    ld.elev = (short)(ld.elev / 3.2804);
                    break;
            }
        }

        void SlopeConvert(short sl)
        {
            ld.slope = sl;
            if (ld.slope == -9999)
            {
                ld.slope = (short)0.0;
                return;
            }

            switch (pFarsite.GetTheme_Units(S_DATA))
            {
                case 0:
                    // degrees default
                    break;
                case 1:
                    // percent
                    double fraction, ipart = 0.0;
                    double slopef;

                    slopef = Math.Atan((double)ld.slope / 100.0) / Math.PI * 180.0;
                    ld.slope = (short)slopef;
                    fraction = modf(slopef, ref ipart);
                    if (fraction >= 0.5)
                        ld.slope++;
                    break;
            }
        }


        void AspectConvert(short aS)
        {
            if (aS == -9999)
            {
                ld.aspect = 0;
                ld.aspectf = 0.0;
                return;
            }
            ld.aspect = aS;
            ld.aspectf = (double) aS;
            switch (pFarsite.GetTheme_Units(A_DATA))
            {
                case 0:
                    // grass 1-25 counterclockwise from east
                    if (ld.aspect != 25)
                        ld.aspectf = (2.0 * Math.PI - (ld.aspectf / 12.0 * Math.PI)) +
                            (7.0 * Math.PI / 12.0);  // aspect from GRASS, east=1, north=7, west=13, south=19 
                    else
                    {
                        ld.aspectf = 25.0;
                        ld.slope = 0;
                    }
                    break;
                case 1:
                    // degrees 0 to 360 counterclockwise from east
                    ld.aspectf = (2.0 - ld.aspectf / 180.0) * Math.PI + Math.PI / 2.0;
                    break;
                case 2:
                    ld.aspectf = ld.aspectf / 180.0 * Math.PI;     // arcinfo, degrees AZIMUTH
                    break;
            }
            if (ld.aspectf > (2.0 * Math.PI))
                ld.aspectf -= (2.0 * Math.PI);
        }



        void CoverConvert(short cov)
        {
            ld.cover = cov;
            if (pFarsite.GetTheme_Units(C_DATA) == 0)
            {
                switch (ld.cover)
                {
                    case 99:
                        ld.cover = 0; break;
                    case 1:
                        ld.cover = 10; break;
                    case 2:
                        ld.cover = 30; break;
                    case 3:
                        ld.cover = 60; break;
                    case 4:
                        ld.cover = 75; break;
                    default:
                        ld.cover = 0; break;
                }
            }
        }


        void HeightConvert(short height)
        {
            if (pFarsite.HaveCrownFuels())
            {
                if (height >= 0)
                {
                    short units = pFarsite.GetTheme_Units(H_DATA);

                    ld.height = (double)height / 10.0;
                    if (units == 2 || units == 4)
                        ld.height /= 3.280839;
                    if (ld.height > 100.0)
                        ld.height /= 10.0;  // probably got wrong units
                }
                else
                    ld.height = pFarsite.GetDefaultCrownHeight();
            }
            else
                ld.height = pFarsite.GetDefaultCrownHeight();
        }

        void BaseConvert(short baseE)
        {
            if (pFarsite.HaveCrownFuels())
            {
                if (baseE >= 0)
                {
                    short units = pFarsite.GetTheme_Units(B_DATA);
                    ld.baseE = (double)baseE / 10.0;
                    if (units == 2 || units == 4)
                        ld.baseE /= 3.280839;
                    if (ld.baseE > 100)
				ld.baseE /= 10.0;    // probably got wrong units
                }
                else
                    ld.baseE = pFarsite.GetDefaultCrownBase();
            }
            else
                ld.baseE = pFarsite.GetDefaultCrownBase();
        }

        void DensityConvert(short density)
        {
            if (pFarsite.HaveCrownFuels())
            {
                if (density >= 0)
                {
                    short units = pFarsite.GetTheme_Units(P_DATA);
                    ld.density = ((double)density) / 100.0;
                    if (units == 2 || units == 4)
                        ld.density *= 1.601847; // convert 10lb/ft3 to kg/m3
                    if (ld.density > 1.0)
                        ld.density /= 100.0;    // probably got wrong units
                    if (pFarsite.LinkDensityWithCrownCover(GETVAL))
                        ld.density *= ((double)ld.cover) / 100.0;
                }
                else
                    ld.density = pFarsite.GetDefaultCrownBD(ld.cover);
            }
            else
                ld.density = pFarsite.GetDefaultCrownBD(ld.cover);
        }

        void DuffConvert(short duff)
        {
            if (pFarsite.HaveGroundFuels())
            {
                if (duff >= 0)
                {
                    ld.duff = (double)duff / 10.0;
                    if (pFarsite.GetTheme_Units(D_DATA) == 2)
                        ld.duff *= 2.2417088978002777;
                }
                else
                    ld.duff = 0.0;
            }
            else
                ld.duff = 0.0;
        }

        void WoodyConvert(short woody)
        {
            if (pFarsite.HaveGroundFuels())
                ld.woody = woody;
            else
                ld.woody = 0;
        }
    }
}
*/

