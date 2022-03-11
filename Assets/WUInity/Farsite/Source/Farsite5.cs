using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/*
namespace Farsite
{
    using CSpotList = System.Collections.Generic.List<CSpotData>;

    public class AbsoluteBurnPeriod
    {
        long Month;
        long Day;
        long Year;      // Add this 4-11-12, to help check against wther
        long Start;
        long End;
    }

    public class RelativeBurnPeriod
    {
        double Start;
        double End;
    }


    public class WoodyData
    {
        double SurfaceAreaToVolume;
        double Load;
        double HeatContent;
        double Density;
        double FuelMoisture;
    }

    public class CoarseWoody
    {
        long Units;
        double Depth;
        double TotalWeight;
        long NumClasses;
        char[] Description = new char[64];
        WoodyData wd;

        CoarseWoody()
        {
            wd = null;
            NumClasses = 0;
            Units = -1;
            TotalWeight = 0.0;
            Depth = 0.0;
        }
    }

    public class VectorStorage
    {
        long ID;
        VectorTheme theme;
        VectorStorage next;
        VectorStorage last;
    }



//
//   new fuel model support
//
//------------------------------------------------------------------

    public class NewFuel
    {
    public long number;
        public char[] code = new char[8];
        public double h1;
        public double h10;
        public double h100;
        public double lh;
        public double lw;
        public long dynamic;
        public long sav1;
        public long savlh;
        public long savlw;
        public double depth;
        public double xmext;
        public double heatd;
        public double heatl;
        public char[] desc = new char[256];
    }

// WN-Test...........
//----------------------------------------------------------------------
//                 Farsite WindNinja Struct                             
// used in the WindData struct tbl                                      
public class d_FWN
{
    float f_Temperature;  // weather & cloud at time of simulation 
    long nWindRows;      // size of grid 
    long nWindCols;
    double windsXllCorner;
    double windsYllCorner;
    double windsResolution;

    short windDirGrid;
    short windSpeedGrid;
 } ;

/-----------------------------------------------------------------------/
public class WindData
{   // wind data structure

     long mo;
    long dy;
    long yr;      // add 4-11-12
    long hr;
    double ws;      // windspeed mph
    long wd;        // winddirection azimuth
    long cl;        // cloudiness

    // WN-Test
    static int e_NA = 0;  // Not Applicable, record isn't in a burn period 
    static int e_BP = 1;  // Record is in the burn period sim time 
    static int e_WN = 2;  // Record has Ninja run on it and contains speed & dir grids 
    static int e_PT = 3;  // Record contains Pointer to a record that has the grids 
    int iS_WN;
    d_FWN a_FWN;
    // WN-Test
  }

---------------------------------------------------------/
public class WeatherData
{
     long mo;
    long dy;
    long yr;     // add 4-11-12
    double rn;
    long t1;
    long t2;
    double T1;
    double T2;
    long H1;
    long H2;
    double el;
    long tr1;
    long tr2;
 } ;

/-------------------------------------------------------------/
    public class FuelConversions
    {
        int[] Type = new int[257];            // each fuel type contains a fuel model corresponding
        // load defaults
        FuelConversions()
        {
            for (int i = 0; i < 257; ++i)
            {
                // could also read default file here
                Type[i] = i;
            }
                
            Type[99] = -1;
            Type[98] = -2;
            Type[97] = -3;
            Type[96] = -4;
            Type[95] = -5;
            Type[94] = -6;
            Type[93] = -7;
            Type[92] = -8;
            Type[91] = -9;
        }
    }

public class InitialFuelMoisture
{
    // initial fuel moistures by fuel type
    bool FuelMoistureIsHere;
    long TL1;
    long TL10;
    long TL100;
    long TLLH;
    long TLLW;
}

    public class StationGrid
    {
        long XDim;                 // number of Easting cells
        long YDim;                 // number of Northing cells
        double Width;              // width of grid cell
        double Height;             // height of grid cell
        long[] Grid;                // holds weather/wind station numbers
        public StationGrid()
        {
            Grid = null;
        }

        ~StationGrid()
        {
            if (Grid != null)
            {
                Grid = null; //delete[] Grid;
            }
                
        }
    }

    class CSpotData
    {
   
	    public CSpotData()
        {
            launchTime = launchX = launchY = landTime = landX = landY = 0.0;
        }

        ~CSpotData()
        {

        }

        double launchTime;
        double launchX;
        double launchY;
        double landTime;
        double landX;
        double landY;
    }

public class headdata
{// header for landscape file

    int CrownFuels;         // 20 if no crown fuels, 21 if crown fuels exist
    int GroundFuels;      // 20 if no ground fuels, 21 if ground fuels exist
    int latitude;
    double loeast;
    double hieast;
    double lonorth;
    double hinorth;
    int loelev;
    int hielev;
    int numelev;          //-1 if more than 100 categories
    int[] elevs= new int[100];
    int loslope;
    int hislope;
    int numslope;         //-1 if more than 100 categories
    int[] slopes = new int[100];
    int loaspect;
    int hiaspect;
    int numaspect;        //-1 if more than 100 categories
    int[] aspects = new int[100];
    int lofuel;
    int hifuel;
    int numfuel;          //-1 if more than 100 categories
    int[] fuels = new int[100];
    int locover;
    int hicover;
    int numcover;         //-1 if more than 100 categories
    int[] covers = new int[100];
    int loheight;
    int hiheight;
    int numheight;        //-1 if more than 100 categories
    int[] heights = new int[100];
    int lobase;
    int hibase;
    int numbase;          //-1 if more than 100 categories
    int[] bases = new int[100];
    int lodensity;
    int hidensity;
    int numdensity;       //-1 if more than 100 categories
    int[] densities = new int[100];
    int loduff;
    int hiduff;
    int numduff;          //-1 if more than 100 categories
    int[] duffs = new int[100];
    int lowoody;
    int hiwoody;
    int numwoody;         //-1 if more than 100 categories
    int[] woodies = new int[100];
    int numeast;
    int numnorth;
    double EastUtm;
    double WestUtm;
    double NorthUtm;
    double SouthUtm;
    int GridUnits;        // 0 for metric, 1 for English
    double XResol;
    double YResol;
    short EUnits;
    short SUnits;
    short AUnits;
    short FOptions;
    short CUnits;
    short HUnits;
    short BUnits;
    short PUnits;
    short DUnits;
    short WOptions;
    char[] ElevFile = new char[256];
    char[] SlopeFile = new char[256];
    char[] AspectFile = new char[256];
    char[] FuelFile = new char[256];
    char[] CoverFile = new char[256];
    char[] HeightFile = new char[256];
    char[] BaseFile = new char[256];
    char[] DensityFile = new char[256];
    char[] DuffFile = new char[256];
    char[] WoodyFile = new char[256];
    char[] Description = new char[512];
}

public class streamdata
{// structure for holding weather/wind stream data max and min values

    int wtr;
int wnd;
int all;
}



    /-----------------------------------------------------------------------------/
    //                        Farsite Progress Class                    
    public class FPC
    {
        static int e_Start = 0;
        static int e_Ninja = 1;
        static int e_NinjaPrep = 2;    // Ninja preparing to run 
        static int e_PreCond = 3;
        static int e_Cond = 4;
        static int e_Far = 5;

        int i_State;
        float f_WNToDo;       // How Many ninja runs are needed 
        float f_WNComplete;   // How many competed so far 


        double f_PreProg;

        double f_pcNinja;    // percentages of time that each section 
        double f_pcCond;     // is approximated to run 
        double f_pcFar;

        public FPC()
        {
            Init();
        }
        ~FPC()
        {
            // DeleteCriticalSection(&CriSec);
        }
        void Init()
        {
            i_State = e_Start;
            f_PreProg = 0;

            f_pcNinja = 0;    // percentages of time that each section 
            f_pcCond = 0;     // is approximated to run 
            f_pcFar = 0;

            //InitializeCriticalSection(&CriSec);
        }

        //float GetProgress(Farsite5 *a_F5, CWindNinja2 *a_WN2, CFMC *a_cfmc, char cr[]);
        float GetProgress(Farsite5 a_F5, CFMC a_cfmc, char cr[])
        {
            double f, pc;

            //	EnterCriticalSection(&CriSec);

            strcpy(cr, "");

            // NOTE things run in this order Start, Ninja-Prep, Ninja, Cond, Farsite 

            // Haven't even got to ninja yet 
            if (this.i_State == e_Start)
            {
                strcpy(cr, "Startup");
                return LeaCriSec(0);
            }

            // Ninja Prep only takes a few seconds, mostly to run the Nearest Neighbot stuff once 
            if (this.i_State == e_NinjaPrep)
            {
                strcpy(cr, "WindNinja-Prep");
                return LeaCriSec(0);
            }


            if (f_pcFar == 0)
            {    // this might occur, means ninja didn't start yet 
                strcpy(cr, "Startup");
                return LeaCriSec(0);
            }


            if (i_State == e_Ninja)
            {
                sprintf(cr, "WindNinja %1.0f-%1.0f", this.f_WNComplete + 1, this.f_WNToDo);
                pc = 1.0;//WindNinja_Progress (a_WN2);
                f = pc * this.f_pcNinja;
                return LeaCriSec((float)f);
            }


            if (i_State == e_Cond)
            {
                strcpy(cr, "Conditiong");
                pc = a_cfmc->Get_ProgressF();   // Comes back 0 -> 1.0 
                pc = pc * this.f_pcCond;       // allocted portion of total run time 
                f = this.f_pcNinja + pc;       // add it to previous time spent running 
                return LeaCriSec(f);
            }

            if (i_State == e_Far)
            {
                strcpy(cr, "FarsiteBurning");
                pc = a_F5.GetFarsiteProgress();
                f = this.f_pcNinja + this.f_pcCond;
                f = f + (pc * this.f_pcFar);
                return LeaCriSec((float)f);
            }

            // Shouldn't get here, everthing should have been taken care of above 
            strcpy(cr, "Unknown process state");
            // LeaveCriticalSection(&CriSec);
            return 0;
        }

        //float WindNinja_Progress (CWindNinja2 *a_WN2)
        //{
        //    float f, g, pc, f_One, f_Done;
  // if ( this->f_WNToDo == 0 )  // humm should even be calling this //
    //  return 0;
   //    f_One = 1.0 / this->f_WNToDo;
    //    f_Done = f_One * this->f_WNComplete;
   //if (f_Done > 1.0 )
   //  return 1.0;
  //pc = a_WN2->Get_Progress();  //comes back as 0 -> 1.0 //
   //     pc = pc* f_One;
    //    f =  f_Done + pc;
   //return f;
    //    }

float LeaCriSec(float f)
        {
            //LeaveCriticalSection(&CriSec);
            return f;
        }


    int SetTimeSlice(int i_NumNinRun, Farsite5 aFar)
        {
            double f;

            // EnterCriticalSection(&CriSec);


            // NOTE - not using this yet - this is how many minutes Conditioning will run 
            // I'm thinking of using it do help determine what portion of time Conditioning 
            // will take up, the more minutes it needs to Condition the longer it takes 
            //  f_CondMin = GetCondTime (a_F5);

            // Ninja could run serveral times, so set a rough percent based on number of 
            //  that will need to be run 
            // FOR NOW - this is just a wild guess 
            if (i_NumNinRun <= 0)
                f_pcNinja = 0;
            else if (i_NumNinRun == 1)
                f_pcNinja = 0.25;
            else if (i_NumNinRun == 2)
                f_pcNinja = 0.40;
            else if (i_NumNinRun == 3)
                f_pcNinja = 0.60;
            else
                f_pcNinja = 0.80;

            f = 1.0 - f_pcNinja;  // Get remaining percentage

            f_pcCond = f * 0.25;  // Give Condtioning 25 percent of remaining 

            f_pcFar = 1.0 - (f_pcCond + f_pcNinja); // Farsite gets the rest 

            // LeaveCriticalSection(&CriSec);
            return 0;
        }
    float GetCondTime(Farsite5 a_F5)
        {
            int i_StrYr, i_MthStart, i_DayStart, i_HourStart, date, min;
            int EndMin, StrMin, MaxMin;
            i_StrYr = a_F5.icf.a_Wtr[1].i_Year;     // Start Conditioning For Daily Weather
            i_MthStart = a_F5.icf.a_Wtr[1].i_Mth;    //  on the second day
            i_DayStart = a_F5.icf.a_Wtr[1].i_Day;
            i_HourStart = 0;


            //   cfmc->Set_DateStart (i_StrYr, i_MthStart, i_DayStart, i_HourStart);

            date = GetMCDate(i_MthStart, i_DayStart, i_StrYr);
            min = MilToMin(i_HourStart);              // Military time to minutes
            StrMin = (date * e_MinPerDay) + min;       // to total minutes

            date = GetMCDate(a_F5.icf.i_FarsiteEndMth, a_F5.icf.i_FarsiteEndDay, a_F5.icf.i_FarsiteEndYear);
            min = MilToMin(a_F5.icf.i_FarsiteEndHour);
            EndMin = (date * e_MinPerDay) + min;      // to total minutes

            MaxMin = EndMin - StrMin;

            MaxMin = MaxMin + (int)a_F5.actual;
            //   d = (double) MaxMin;

            //   d += this->actual;


            return MaxMin;
        }

        void Set_NinjaRunning(float f)
        {
            //  EnterCriticalSection(&CriSec);

            this.i_State = e_Ninja;
            this.f_WNComplete = 0;
            this.f_WNToDo = f;

            //  LeaveCriticalSection(&CriSec);
        }
        void Set_NinjaPrep()
        {
            //EnterCriticalSection(&CriSec);
            this.i_State = e_NinjaPrep;
            // LeaveCriticalSection(&CriSec);
        }
        void Set_CondRunning()
        {
            // EnterCriticalSection(&CriSec);
            this.i_State = e_Cond;
            // LeaveCriticalSection(&CriSec);
        }
        void Set_FarsiteRunning()
        {
            // EnterCriticalSection(&CriSec);
            this.i_State = e_Far;
            //  LeaveCriticalSection(&CriSec);
        }

        void Set_NinjaInc()
        {
            // EnterCriticalSection(&CriSec);
            this.f_WNComplete++;
            // LeaveCriticalSection(&CriSec);
        }

        // CRITICAL_SECTION CriSec;
    };


    public class Farsite5
    {
        NewFuel[] newfuels;

        bool GetNewFuel(long number, NewFuel newfuel)
        {
            if (number < 0)
                return false;

            if (newfuels[number].number == 0)
                return false;

            if (newfuel != null)
            {
                newfuel.number = Math.Abs(newfuel.number);    // return absolute value of number
            }

            return true;
        }

    }
}
*/
