using System.Collections;
using System.Collections.Generic;
using System;

/*
namespace Farsite
{   
    public class Mechanix
    {
        //wrap some methods for easy use
        static double cos(double d)
        {
            return Math.Cos(d);
        }

        static double pow(double d, double e)
        {
            return Math.Pow(d, e);
        }

        static double pow2(double d)
        {
            return d * d;
        }

        static double exp(double d)
        {
            return Math.Exp(d);
        }

        static double sqrt(double d)
        {
            return Math.Sqrt(d);
        }

        double lb_ratio, hb_ratio, rateo;            // rateo is ROS w/o slope or wind
        double b, part1;                             // parameters for phiw

        //double headback(void);
        //double CalcEffectiveWindSpeed();

        //protected
        double m_ones, m_tens, m_hundreds, m_livew, m_liveh;                // local copies of FE data for use in spreadrate
        double phiw, phis, phiew, LocalWindDir;
        double FirePerimeterDist, slopespd;

        //double accel(double RosE, double RosT, double A, double* avgros, double* cosslope);
        //void TransformWindDir(long slope, double aspectf);
        //double vectordir(double aspectf);
        //double vectorspd(double* VWindSpeed, double aspectf, long FireType);
        Farsite5 pFarsite;

        //public
        double xpt, ypt, midx, midy, xt, yt, xptn, yptn, xptl, yptl, xdiff, ydiff;
        double vecspeed, ivecspeed, vecdir, m_windspd, m_winddir, m_twindspd;
        double ActualSurfaceSpread, HorizSpread, avgros, cros, fros, vecros, RosT1, RosT, R10;
        double fli, FliFinal, ExpansionRate;
        double timerem, react, savx, cosslope;
        double CurrentTime, FlameLength, CrownLoadingBurned, CrownFractionBurned;
        double head, back, flank;

        public Mechanix(Farsite5 _pFarsite)
        {

        }

        ~Mechanix()
        {

        }

        //void limgrow(void);                         // limits growth to distance checking
        //void grow(double ivecdir);                  // Richards (1990) differential equation
        //void distchek(long CurrentFire);                // checks perimeter and updates distance check
        //void ellipse(double iros, double wspeed);       // calculates elliptical dimensions
        //void scorrect(long slope, double aspectf);      // slope correction
        //void GetEquationTerms(double* pphiw, double* pphis, double* bb, double* ppart1);

public double spreadrate(long slope, double windspd, int fuel)
        {
            // Rothermel spread equation based on BEHAVE source code,
         //  support for dynamic fuel models added 10/13/2004
            long i, j, ndead = 0, nlive = 0;
            double depth;
            double[,] seff = { { 0.01, 0.01 }, { 0.01, 0.01 }, { 0.01, 0 }, { 0.01, 0.0 } };         //mineral content
            double wtfact, fined = 0.0, finel = 0.0, wmfd = 0.0, fdmois = 0.0, w = 0.0, wo = 0.0, beta;
            double rm, sigma = 0.0, rhob, sum3 = 0.0, betaop, rat, aa, gammax = 0.0, gamma = 0, wind = 0;
            double xir, rbqig = 0, xi = 0, c, e, slopex = 0;
            double ewind = 0, wlim, sum1 = 0, sum2 = 0;            

            //double accel(double RosE, double RosT, double A, double* avgros, double* cosslope);
            //void TransformWindDir(long slope, double aspectf);
            //double vectordir(double aspectf);
            //double vectorspd(double* VWindSpeed, double aspectf, long FireType);
            //Farsite5* pFarsite;

            //public 
            double xpt, ypt, midx, midy, xt, yt, xptn, yptn, xptl, yptl, xdiff, ydiff;
            double vecspeed, ivecspeed, vecdir, m_windspd, m_winddir, m_twindspd;
            double ActualSurfaceSpread, HorizSpread, avgros, cros, fros, vecros, RosT1, RosT, R10;
            double fli, FliFinal, ExpansionRate;
            double timerem, react, savx, cosslope;
            double CurrentTime, FlameLength, CrownLoadingBurned, CrownFractionBurned;
            double head, back, flank;

            //temp defaults taken from fsxwmech.cpp
            m_ones = 0.05;
            m_tens = 0.10;
            m_hundreds = 0.15;
            m_livew = 0.95;
            m_liveh = 0.95;
            m_windspd = 1.0;
            m_twindspd = 6;
            m_winddir = Math.PI / 2.0;

            double[,] mois =    // fraction of oven-dry weight
            {   
                {m_ones, m_liveh},        // use Mechanix Class copies of FE data
		        {m_tens, m_livew},
                {m_hundreds, 0.0},
                {m_ones, 0.0}
            };
            double[] gx = { 0.0, 0.0, 0.0, 0.0, 0.0 };
            double[,] wn = {{0,0},{0,0},{0,0},{0,0}};
            double[,] qig = {{0,0},{0,0},{0,0},{0,0}};
            double[,] a = {{0,0},{0,0},{0,0},{0,0}};
            double[,] f = {{0,0},{0,0},{0,0},{0,0}};
            double[,] g = {{0,0},{0,0},{0,0},{0,0}};
            double[] ai = { 0, 0 };
            double[] fi = { 0, 0 };
            double[] hi = { 0, 0 };
            double[] se = { 0, 0 };
            double[] xmf = { 0, 0 };
            double[] si = { 0, 0 };
            double[] wni = { 0, 0 };
            double[] etam = { 0, 0 };
            double[] etas = { 0, 0 };
            double[] rir = { 0, 0 };

            NewFuel newfuel = new NewFuel();
        
            if(!pFarsite.GetNewFuel(fuel, newfuel)) // will get all models, 13, 40, and custom
            {	
                rateo  = 0.0;
                return rateo;
            }

             // count number of fuels
            if(newfuel.h1 > 0.0) ndead++;
	        if(newfuel.h10 > 0.0) ndead++;
	        if(newfuel.h100 > 0.0) ndead++;
	        if(newfuel.lh > 0.0) nlive++;
	        if(newfuel.lw > 0.0) nlive++;

            if(nlive==0)
     	        newfuel.dynamic=0;

            if(nlive>0)
                nlive=2;                      // boost to max number
            if(ndead>0)
                ndead=4;

            double[] nclas = { (double)ndead, (double)nlive };  // # of dead & live fuel classes
            double[] xmext = { newfuel.xmext, 0 };

            double[,] load=			     // tons per acre, later converted to lb/ft2
                  {	{newfuel.h1, newfuel.lh
                  },
                    {newfuel.h10, newfuel.lw},
                    {newfuel.h100, 0.0},
                    {0.0, 0.0},
                  };

            depth = newfuel.depth;

            //-------------------------------------------------------------------------
            // do the dynamic load transfer
            //-------------------------------------------------------------------------
            if(newfuel.dynamic > 0)
            {    if(mois[0,1]<0.30) // if live herbaceous is less than 30.0
                {    load[3,0]=load[0,1];
                    load[0,1]=0.0;
                }
                else if(mois[0,1]<1.20)
                {    load[3,0]=load[0,1]* (1.20-mois[0,1])/0.9;
                    load[0,1]-=load[3,0];
                }
            }
            //-------------------------------------------------------------------------

	        double[,] sav=			     // 1/ft
                 {	{(double)newfuel.sav1, (double)newfuel.savlh},
                    {109.0, (double)newfuel.savlw},
                    {30.0, 0.0},
                    {(double)newfuel.savlh, 0.0},
                 };

	        double[,] heat=
             {    {newfuel.heatd, newfuel.heatl},
                  {newfuel.heatd, newfuel.heatl},
                  {newfuel.heatd, 0.0},
                  {newfuel.heatd, 0.0},
             };

	        wind=windspd*88.0;                      // ft/minute
	        slopex = Math.Tan(slope/180.0 * Math.PI);  	// convert from degrees to tan

	        // fuel weighting factors
	        for(i=0; i<2; i++)
	        {
                for (j=0; j<nclas[i]; j++)
		        {    a[j,i]=load[j,i]* sav[j,i]/32.0;
			        ai[i]=ai[i]+a[j,i];
			        wo=wo+0.04591* load[j,i];
		        }
		        if(nclas[i]!=0)
		        {
                    for (j=0; j<nclas[i]; j++)
			        {    if(ai[i]>0.0)
                        {
                            f[j, i] = a[j, i] / ai[i];
                        }
                        else
                        {
                            f[j, i] = 0.0;
                        }                                 
                    }

                    for(j=0; j<nclas[i]; j++)
			        {    if(sav[j,i]>=1200.0)
	              		        gx[0]+=f[j,i];
				        else if(sav[j,i]>=192.0)
               		        gx[1]+=f[j,i];
				        else if(sav[j,i]>=96.0)
               		        gx[2]+=f[j,i];
				        else if(sav[j,i]>=48.0)
               		        gx[3]+=f[j,i];
				        else if(sav[j,i]>=16.0)
               		        gx[4]+=f[j,i];
			        }

                    for(j=0; j<nclas[i]; j++)
			        {    if(sav[j,i]>=1200.0)
	              		        g[j,i]=gx[0];
				        else if(sav[j,i]>=192.0)
	              		        g[j,i]=gx[1];
				        else if(sav[j,i]>=96.0)
	              		        g[j,i]=gx[2];
				        else if(sav[j,i]>=48.0)
	              		        g[j,i]=gx[3];
				        else if(sav[j,i]>=16.0)
	              		        g[j,i]=gx[4];
                            else
                    	        g[j,i]=0.0;
			        }
		        }
	        }
	fi[0]=ai[0]/(ai[0]+ai[1]);
	fi[1]=1.0-fi[0];

	// no need for this, because extinction moistures are assigned 
	// as on last page of Burgan and Rothermel 1984 
	//	rhob=(wo/depth);
		//beta=rhob/32;
		//xmext[0]=.12+4.* beta;

	//moisture of extinction
	if(nclas[1]!=0)
	{	for(j=0; j<nclas[0]; j++)
		{    wtfact=load[j,0]* exp(-138.0/sav[j,0]);
fined=fined+wtfact;
			wmfd=wmfd+wtfact* mois[j,0];
		}
		fdmois=wmfd/fined;
		for(j=0; j<nclas[1]; j++)
		{    if(sav[j,1]<1e-6)
              		continue;
			finel=finel+load[j,1]* exp(-500.0/sav[j,1]);
          }
		w=fined/finel;
		xmext[1]=2.9* w* (1.0-fdmois/xmext[0])-0.226;
		if(xmext[1]<xmext[0])
			xmext[1]=xmext[0];
	}

	// intermediate calculations, summing parameters by fuel component
	for(i=0; i<2; i++)
	{	if(nclas[i]!=0)
		{	for(j=0; j<nclas[i]; j++)
			{    if(sav[j,i]<1e-6)
               		continue;
               	wn[j,i]=0.04591* load[j,i]* (1.0-0.0555);
				qig[j,i]=250.0+1116.0* mois[j,i];
				hi[i]=hi[i]+f[j,i]* heat[j,i];
				se[i]=se[i]+f[j,i]* seff[j,i];
				xmf[i]=xmf[i]+f[j,i]* mois[j,i];
				si[i]=si[i]+f[j,i]* sav[j,i];
				sum1=sum1+0.04591* load[j,i];
				sum2=sum2+0.04591* load[j,i]/32.0;
				sum3=sum3+fi[i]* f[j,i]* qig[j,i]* exp(-138.0/sav[j,i]);
			}
			for(j=0; j<nclas[i]; j++)
				//wni[i]=wni[i]+f[j,i]*wn[j,i];
				wni[i]=wni[i]+g[j,i]* wn[j,i];  // g[j,i] should be subst for f[j,i] in the wni[i] equation 
										   // if the above g-factors are calculated 
			rm=xmf[i]/xmext[i];
			etam[i]=1.0-2.59* rm+5.11* pow2(rm)-3.52* pow(rm,3.0);
			if(xmf[i] >= xmext[i])
				etam[i]=0;
			etas[i]=0.174/(pow(se[i],0.19));
			if(etas[i]>1.0)
				etas[i]=1.0;
			sigma=sigma+fi[i]* si[i];
rir[i]=wni[i]* hi[i]* etas[i]* etam[i];
		}
	}

	// final calculations 
	rhob=sum1/depth;
	beta=sum2/depth;
	betaop=3.348/pow(sigma,0.8189);
rat=beta/betaop;
	aa=133.0/pow(sigma,0.7913);
//gammax=pow(sigma,1.5)/(495.0+0.0594*pow(sigma,1.5));
gammax=(sigma* sqrt(sigma))/(495.0+0.0594* sigma* sqrt(sigma));
	gamma=gammax* pow(rat, aa)* exp(aa*(1.0-rat));
	xir=gamma* (rir[0]+rir[1]);
	rbqig=rhob* sum3;
xi=exp((0.792+0.681*sqrt(sigma))* (beta+0.1))/(192.0+0.2595* sigma);
//	flux=xi*xir;
	rateo=xir* xi/rbqig;   // this is in English units 

	phis=5.275* pow(beta,-0.3)* pow2(slopex);
c=7.47* exp(-0.133*pow(sigma,0.55));
		b=0.02526* pow(sigma,0.54);
e=0.715* exp(-0.000359*sigma);
part1=c* pow(rat,-e);
phiw=pow(wind, b)* part1;

wlim=0.9* xir;

	if(phis>0.0)
     {	if(phis>wlim)             // can't have inifinite windspeed
     		phis=wlim;
		slopespd=pow((phis/part1),1.0/b)/88.0; 	// converts phis to windspd in mph
     }
    else
            {
                slopespd = 0.0;
            }
		

//   rate=(rateo*(1+phiw+phis));   
//   *byram=384*xir*rate/(60*sigma);
//	flame=.45*pow(byram,.46);      
//	hpua=xir*384/sigma;            

//   maximum windspeed effect on ros
	phiew=phiw+phis;
	ewind=pow(((phiew* pow(rat, e))/c),1.0/b);

	if(ewind>wlim)
	{	ewind=wlim;
		phiew=c* pow(wlim, b)* pow(rat,-e);
//		rate=rateo*(*phiew+1);	     
//		byram=384*xir*rate/(60*sigma);
//		flame=.45*pow(byram,.46);     
	}
	savx=sigma;					// SAVX, REACT IN MECHANIX.PUBLIC:
    react=xir*0.189275;  		    	// convert btu/f2/min to kW/m2
	rateo=rateo*0.30480060960;				// convert from f/min to m/min

     return rateo;
}
    }

    void LoadGlobalFEData(FELocalSite* fe)
    {
        // load fire envt data back up to mechanix class functions


        fe->GetEnvironmentData(&gmw);

        m_ones = gmw.ones;
        m_tens = gmw.tens;
        m_hundreds = gmw.hundreds;
        m_livew = gmw.livew;
        m_liveh = gmw.liveh;
        m_windspd = gmw.windspd;
        m_twindspd = gmw.tws;
        m_winddir = gmw.winddir;
        ld = fe->ld;

        if (m_winddir == -1.0)
            m_winddir = ld.aspectf;
        else if (m_winddir < -1.0)
            m_winddir = (Math.PI - ld.aspectf);

        
        //m_ones=gmw.ones=0.05;
        //m_tens=gmw.tens=0.10;
        //m_hundreds=gmw.hundreds=0.15;
        //m_livew=gmw.livew=0.95;
        //m_liveh=gmw.liveh=0.95;
        //m_windspd=gmw.windspd=1.0;
        //m_twindspd=gmw.tws=6;
        //m_winddir=gmw.winddir=PI/2.0;
        
    }

}
*/
