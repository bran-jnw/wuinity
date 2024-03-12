using System.Collections;
using System.Collections.Generic;

/*
namespace Farsite
{
    public class BurnThread : MechCalls
    {
        long CurrentFire, CurrentPoint, CuumPoint, TotalPoints;
        long Begin, End, turn;
        bool FireIsUnderAttack;
        double TimeIncrement, CuumTimeIncrement, SIMTIME, SimTimeOffset;
        uint ThreadID;
        //X_HANDLE hBurnThread;
        //Crown cf;
        //FireRing* firering;

        Farsite5 pFarsite;

        //public:
        long ThreadOrder;
        bool CanStillBurn, StillBurning, Started, DoSpots;
        double TimeMaxRem;               // maximum time remaining
        double EventTimeStep;

        X_HANDLE hBurnEvent;

        Embers embers;
        AreaPerimeter prod;
        // 	FireEnvironment2* env;
        FELocalSite fe;

        public BurnThread(Farsite5 _pFarsite) : MechCalls(_pFarsite), cf(_pFarsite), prod(_pFarsite), embers(_pFarsite)
        {
            pFarsite = _pFarsite;
            FireIsUnderAttack = false;
            ThreadID = 0;
            Begin = End = -1;
            turn = 0;
            hBurnThread = 0;
            fe = new FELocalSite(pFarsite);
            // 	env = Env;
            firering = 0;
            embers.SetFireEnvironmentCalls(fe);
            Started = false;
            DoSpots = false;
            TotalPoints = CuumPoint = 0;
            //hBurnEvent=0;
        }

        ~BurnThread()
        {
            //     if(hBurnEvent)
            //     	CloseHandle(hBurnEvent);
            fe = null;
        }

        void SetRange(long currentfire, double SimTime, double cuumtimeincrement, double timeincrement, double timemaxrem, long begin, long end, long Turn, bool attack, FireRing Ring)
        {
            if (begin > -1)
            {
                Begin = begin;
                End = end;
            }
            CurrentFire = currentfire;
            SIMTIME = SimTime;
            CuumTimeIncrement = cuumtimeincrement;
            TimeMaxRem = timemaxrem;
            //if (timemaxrem > 0.001)
            //{
            //    printf("%lf\n", timemaxrem);	}
            TimeIncrement = EventTimeStep = timeincrement;
            turn = Turn;
            FireIsUnderAttack = attack;
            firering = Ring;
            CanStillBurn = StillBurning = false;
            SimTimeOffset = CuumTimeIncrement;
            //if(DistanceCheckMethod(GETVAL)==0)
            //     SimTimeOffset-=GetTemporaryTimeStep();
        }

        void BurnThread::StartBurnThread(long ID)
        {
            RunBurnThread(this);
            return hBurnThread;
        }

        void GetThreadHandle()
        {
            return hBurnThread;
        }


        void SurfaceFire()
        {
            // does surface fire calculations of spread rate and intensity
            //LoadGlobalFEData(fe);
            fros = Mechanix.spreadrate(ld.slope, m_windspd, ld.fuel);
            if (fros > 0.0)                     // if rate of spread is >0
            {
                StillBurning = 1;           // allows fire to continue burning
                GetAccelConst();              // get acceleration constants
                VecSurf();                  // vector wind with slope
                ellipse(vecros, ivecspeed);     // compute elliptical dimensions
                grow(vecdir);               // compute time derivatives xt and yt for perimeter point
                AccelSurf1();                 // compute new equilibrium ROS and Avg. ROS in remaining timestep
                SlopeCorrect(1);
                SubTimeStep = timerem;
                limgrow();                      // limits HORIZONTAL growth to mindist
                AccelSurf2();                   // calcl ros & fli for SubTimeStep Kw/m from BTU/ft/s
                //	if(SSpotOK && cover==99)
                //	{    cf.FlameLength=0;		// may want to set other crown parameters to 0 here
                //		SpotFire(3);	   	 	// spotting from  winddriven surface fires
                //	}
            }
            else
            {
                timerem = 0;
                RosT = 0;
                fli = 0;
                FliFinal = 0;
            }
        }

        void CrownFire()
        {

        }
        void SpotFire(int SpotSource)
        {

        }
        void EmberCoords()
        {

        }
        uint RunBurnThread(void* burnthread)
        {
            static_cast<BurnThread*>(bt)->PerimeterThread();

            return 1;
        }
        void PerimeterThread()
        {

        }
        unsigned RunSpotThread(void* burnthread);
        void SpotThread();
        
        void SetRange(long CurrentFire, double SimTime, double CuumTimeIncrement,
        double TimeIncrement, double TimeMaxRem, long Begin, long End,
        long turn, bool FireIsUnderAttack, FireRing* ring);
    }    
}
*/