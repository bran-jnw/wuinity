//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using static WUIPlatform.Fire.MathWrap;

namespace WUIPlatform.Fire
{
    //------------------------------------------------------------------------------
    /*! \page DeadFuelMoisture Dead Fuel Moisture
        DeadFuelMoisture is an implementation of Nelson's dead fuel moisture model
        \ref nelson2000 as modified by \ref bevins2005.

        \section dfmmods Dead Fuel Moisture Model Modifications
        The DeadFuelMoisture class has been modified from Nelson \ref nelson2000
        description as follows:
        -- Moisture content and diffusivity computation time steps are independent
            of elapsed time since the previous update(), as recommended by Carlson
            2003 \ref carlson2003.
        -- Planar heat transfer rate, adsorption rate, rainfall runoff rate, and
            the number of moisture content and diffusivity computation steps are
            automatically derived from the stick radius using equations
            developed by Bevins \ref bevins2005.
        -- Precipitation is no longer divided into rainfall and rainstorm
            categories, making Nelson's storm transition value (stv) unnecessary.
        -- Rainfall events are no longer divided into first hour and subsequent
            hour categories, making Nelson's subsequent runoff factor (rai1)
            unnecessary.
        -- The following parameters are now assigned values independent of
            stick radius:
            -- maximum local moisture content is 0.60 g/g,
            -- desorption rate is 0.06 (cm3/cm2)/h.
            -- water film contribution is zero g/g.
        The above changes produce better stability and more accurate predictions
        for the available test data sets.
        \section dfmuse Using DeadFuelMoisture
        This is a quick tutorial on using the DeadFuelMoisture class.
        \subsection dfmuse1 Step 1: Create A Dead Fuel Moisture Stick
        To create a standard 1-h, 10-h, 100-h, or 1000-h time lag fuel moisture
        stick, call one of:
        -- DeadFuelMoisture* dfm1h = createDeadFuelMoisture1( "1-h stick name" );
        -- DeadFuelMoisture* dfm10h = createDeadFuelMoisture10( "10-h stick name" );
        -- DeadFuelMoisture* dfm100h = createDeadFuelMoisture100( "100-h stick name" );
        -- DeadFuelMoisture* dfm1000h = createDeadFuelMoisture1000( "1000-h stick name" );

        To create a stick with a specific radius:
        -- DeadFuelMoisture *dfm = new DeadFuelMoisture( radius, "stick name" );
        \subsection dfmuse2 Step 2: Customize the Dead Fuel Moisture Stick (Optional)
        This step is only necessary if you are experimenting with the internals
        of the DeadFuelMoisture class.0  If not, go on to Step 3.
        Call any of the following to set internal stick parameters:
        -- void setAdsorptionRate( double adsorptionRate ) ;
        -- void setDesorptionRate( double desorptionRate=0.06 ) ;
        -- void setDiffusivitySteps( int diffusivitySteps );
        -- void setPlanarHeatTransferRate( double planarHeatTransferRate ) ;
        -- void setMaximumLocalMoisture( double localMaxMc=0.6 ) ;
        -- void setMoistureSteps( int moistureSteps );
        -- void setRainfallRunoffFactor( double rainfallRunoffFactor );
        -- void setRandomSeed( int randseed=0 ) ;
        -- void setStickDensity( double stickDensity=0.4 );
        -- void setStickLength( double stickLength=41.0 );
        -- void setStickNodes( int stickNodes=11 ) ;
        -- void setWaterFilmContribution( double waterFilm=0.0 ) ;
        Finally, you \a must call:
        -- void initializeStick( void ) ;
        \subsection dfmuse3 Step 3: Initialize the Stick and Its Environment (Optional)
        If you wish to initialize the stick's internal temperature and moisture
        profile, call one of:
        -- void initializeEnvironment( int year, int month, int day, int hour,
            int minute, int second, double at, double rh, double sr, double rcum,
            double ti, double hi, double wi, double bp );
        -- void initializeEnvironment( double at, double rh, double sr, double rcum,
            double ti, double hi, double wi, double bp );

        The first method initializes the stick date and time, and should be
        used if you are using the version of update() that takes the date and
        time as arguments.
        The second method should be called if you are using the version of
        update() that takes the elapsed time as an argument.
        If neither of these methods are invoked, the stick internal and external
        environments are initialized by the first update() invocation.
        \subsection dfmuse4 Step 4: Update the Stick Environment
        Call the update() method to update the stick's clock and external
        environment, and recalculate the stick's internal temperature and moisture
        profile.0  There are two overloaded versions of update():
        -- bool update( int year, int month, int day, int hour, int minute,
            int second, double at, double rh, double sW, double rcum );
        -- bool update( double et, double at, double rh, double sW, double rcum );
        The first version determines elapsed time from the date and time arguments.
        Do not mix calls to the two versions for the same DeadFuelMoisture instance.
        \subsection dfmuse5 Step 5: Get Stick Temperature and Moisture Content
        To determine the current stick temperature and moisture content, call:
        -- double meanMoisture();
        -- double meanWtdMoisture();
        -- double meanWtdTemperature();
        -- double surfaceMoisture();
        -- double surfaceTemperature();
        Temperatures are oC and moisture contents are g/g.
     */

    //-----------------------------------------------------------------------------
    /*! \namespace Sem
        The C++ namespace identifier used for all common and shared code developed
        under the direction of Systems for Environmental Management.
       \brief The SEM common source C++ namespace identifier.
     */
    [System.Serializable]
    public class DeadFuelMoisture
    {
        // Non-integral static data members must be initialized outside the class
        const double Aks = 2.0e-13; //Permeability of a water saturated stick (2.0e-13 cm2).
        const double Alb = 0.6; //Shortwave albido (0.6 dl).
        const double Alpha = 0.25; //Fraction of cell length that overlaps adjacent cells (0.25 cm/cm).
        const double Ap = 0.000772; //Psychrometric constant(0.000772 / oC).
        const double Aw = 0.8; //Ratio of cell cavity to total cell width (0.8 cm/cm).
        const double Eps = 0.85; //Longwave emissivity of stick surface (0.85 dl).
        const double Hfs = 0.99; //Saturation value of the stick surface humidity (0.99 g/g).
        const double Kelvin = 273.2; //Celcius-to-Kelvin offset (273.2 oC).
        const double Pi = 3.141592654; //A well-rounded number (dl).
        const double Pr = 0.7; //Prandtl number (0.7 dl).
        const double Sbc = 1.37e-12; //Stefan-Boltzmann constant (1.37e-12 cal/cm2-s-K4).
        const double Sc = 0.58; //Schmidt number (0.58 dl).
        const double Smv = 94.743; //Factor to convert solar radiation from W/m2 to milliVolts
        const double St = 72.8; //Surface tension (72.8).
        const double Tcd = 6.0; //Day time clear sky temperature (6 oC).
        const double Tcn = 3.0; //Night time clear sky temperature (3 oC).
        const double Thdiff = 8.0; //Thermal diffusivity ( 8.0 cms/h).
        const double Wl = 0.0023; //Diameter of interior cell cavity ( 0.0023 cm).

        //------------------------------------------------------------------------------
        // Stick-independent intermediates derived in Fms_CreateConstant().
        //------------------------------------------------------------------------------

        const double Srf = 14.82052; //Factor to derive "sr", solar radiation received (14.82052 cal/cm2-h).
        const double Wsf = 4.60517; //Manifest constant equal to -log(1.0 - 0.99)

        /*! \var Hrd
            \brief Factor to derive daytime long wave radiative surface heat transfer.
            \arg Let Sb = 4.0 * 3600.0 * Sbc * Eps = 1.67688e-08;
            \arg Let tsk = Tcd + Kelvin;
            \arg Then Hrd = Sb * tsk * tsk * tsk / Pi;
            \arg And Hrd = 0.116171;
         */
        const double Hrd = 0.116171;        

        /*! \var Hrn
            \brief Factor to derive nighttime long wave radiative surface heat transfer.
            \arg Let Sb = 4.0 * 3600.0 * Sbc * Eps = 1.67688e-08;
            \arg Let tsk = Tcn + Kelvin;
            \arg Then Hrn = Sb * tsk * tsk * tsk / Pi;
            \arg And Hrn = 0.112467;
         */
        const double Hrn = 0.112467;
        
        /*! \var Sir
            \brief Saturation value below which liquid water columns no longer exist.
            \arg Sir = Aw * Alpha / (4.0 * (1.-(2.-Aw) * Alpha));
         */
        const double Sir = 0.0714285;

        /*! \var Scr
            \brief Saturation value at which liquid miniscus first enters the tapered
            portion of wood cells.
            \arg Scr = 4.0 * Sir = 0.285714;
         */
        const double Scr = 0.285714;

        //------------------------------------------------------------------------------
        /*! \enum DFM_State
            \brief DeadFuelMoisture environmental condition states.
         */

        protected enum DFM_State
        {
            DFM_State_None = 0,
            DFM_State_Adsorption = 1,
            DFM_State_Desorption = 2,
            DFM_State_Condensation1 = 3,
            DFM_State_Condensation2 = 4,
            DFM_State_Evaporation = 5,
            DFM_State_Rainfall1 = 6,
            DFM_State_Rainfall2 = 7,
            DFM_State_Rainstorm = 8,
            DFM_State_Stagnation = 9,
            DFM_State_Error = 10
        };

        //------------------------------------------------------------------------------
        /*! \var DFM_States
            \brief Number of DFM_State enumerations.
         */
        static readonly int DFM_States = 11;

        protected SemTime m_semTime;  //!< Current observation's date and time.
                                      // Stick and model parameters defined or inferred during construction:
        protected double m_density;  //!< Stick density (g/cm3).
        protected int m_dSteps;   //!< Number of diffusivity computation steps per observation.
        protected double m_hc;       //!< Stick planar heat transfer rate (cal/cm2-h-C).
        protected double m_length;   //!< Stick length (cm).
        protected string m_name;     //!< Stick name or other descriptive text.
        protected int m_nodes;    //!< Number of stick nodes in the radial direction.
        protected double m_radius;   //!< Stick radius (cm).
        protected double m_rai0;     //!< Rain runoff factor during the initial hour of rainfall (dl).
        protected double m_rai1;     //!< Rain runoff factor after the initial hour of rainfall (dl) [no longer used].
        protected double m_stca;     //!< Adsorption surface mass transfer rate ((cm3/cm2)/h).
        protected double m_stcd;     //!< Desorption surface mass transfer rate ((cm3/cm2)/h).
        protected int m_mSteps;   //!< Number of moisture content computation steps per observation.
        protected double m_stv;      //!< Storm transition value (cm/h) [no longer used].
        protected double m_wfilmk;   //!< Water film contribution to stick moisture content (g water/g dry fuel).
        protected double m_wmx;      //!< Stick maximum local moisture due to rain (g water/g dry fuel).

        // Configuration parameters
        protected bool m_allowRainfall2;   // If TRUE, applies Nelson's logic for rainfall runoff after the first hour
        protected bool m_allowRainstorm;   // If TRUE, applies Nelson's logic for rainstorm transition and state
        protected bool m_pertubateColumn;  // If TRUE, the continuous liquid column condition get pertubated
        protected bool m_rampRai0;         // If TRUE, used Bevins' ramping of rainfall runoff factor rather than Nelsons rai0 *= 0.15

        // Intermediate stick variables derived in initializeStick()
        protected double m_dx;       //!< Internodal radial distance (cm).
        protected double m_wmax;     //!< Maximum possible stick moisture content (g water/g dry fuel).
        protected List<double> m_x = new List<double>(); //!< Array of nodal radial distances from stick center (cm).
        protected List<double> m_v = new List<double>(); //!< Array of nodal volume weighting fractions (cm3 node/cm3 stick).

        // Optimization factors derived in initializeStick()
        protected double m_amlf;     //!< \a aml optimization factor.
        protected double m_capf;     //!< \a cap optimization factor.
        protected double m_hwf;      //!< \a hw and \a aml computation factor.
        protected double m_dx_2;     //!< 2 times the internodal distance \a m_dx (cm).
        protected double m_vf;       //!< optimization factor used in update().

        // Environmental variables provided to initializeEnvironment():
        protected double m_bp0;      //!< Previous observation's barometric presure (cal/cm3).
        protected double m_ha0;      //!< Previous observation's air humidity (dl).
        protected double m_rc0;      //!< Previous observation's cumulative rainfall amount (cm).
        protected double m_sv0;      //!< Previous observation's solar radiation (mV).
        protected double m_ta0;      //!< Previous observation's air temperature (oC).
        protected bool m_init;     //!< Flag set by initialize().

        // Environmental variables provided to update():
        protected double m_bp1;      //!< Current observation's barometric pressure (cal/cm3).
        protected double m_et;       //!< Elapsed time since previous observation (h).
        protected double m_ha1;      //!< Current observation's air humidity (dl).
        protected double m_rc1;      //!< Current observation's cumulative rainfall amount (cm).
        protected double m_sv1;      //!< Current observation's solar radiation (mV).
        protected double m_ta1;      //!< Current observation's air temperature (oC).

        // Intermediate environmental variables derived by update():
        protected double m_ddt;      //!< Stick diffusivity computation interval (h).
        protected double m_mdt;      //!< Stick moisture content computation interval (h).
        protected double m_mdt_2;    //!< 2 times the moisture time step \a m_mdt (h).
        protected double m_pptrate;  //!< Current observation period's rainfall rate (cm/h).
        protected double m_ra0;      //!< Previous observation period's rainfall amount (cm).
        protected double m_ra1;      //!< Current observation period's rainfall amount (cm).
        protected double m_rdur;     //!< Current rainfall event duration (h)
        protected double m_sf;       //!< optimization factor used in update().

        // Stick moisture condition variables derived in update():
        protected double m_hf;       //!< Stick surface humidity (g water/g dry fuel).
        protected double m_wsa;      //!< Stick fiber saturation point (g water/g dry fuel).
        protected double m_sem;      //!< Stick equilibrium moisture content (g water/g dry fuel).
        protected double m_wfilm;    //!< Amount of water film (0 or \a m_wfilmk) (g water/g dry fuel).
        protected double m_elapsed;  //!< Total simulation elapsed time (h).
        protected List<double> m_t = new List<double>(); //!< Array of nodal temperatures (oC).
        protected List<double> m_s = new List<double>(); //!< Array of nodal fiber saturation points (g water/g dry fuel).
        protected List<double> m_d = new List<double>(); //!< Array of nodal bound water diffusivities (cm2/h).
        protected List<double> m_w = new List<double>(); //!< Array of nodal moisture contents (g water/g dry fuel).
        protected long m_updates;  //!< Number of calls made to update().
        protected DFM_State m_state;  //!< Prevailing dead fuel moisture state.
        protected int m_randseed; //!< If not zero, nodal temperature, saturation, and moisture contents are pertubated by some small amount.0 If < 0, uses system clock for seed.
        protected List<double> m_Ttold = new List<double>(); //!< Temporary array of nodal temperatures (oC).
        protected List<double> m_Tsold = new List<double>(); //!< Temporary array of nodal fiber saturation points (g water/g dry fuel).
        protected List<double> m_Twold = new List<double>(); //!< Temporary array of nodal moisture contents (g water/g dry fuel).
        protected List<double> m_Tv = new List<double>();    //!< Temporary array used to redistribute nodal temperatures
        protected List<double> m_To = new List<double>();    //!< Temporary array used to redistribute moisture contents
        protected List<double> m_Tg = new List<double>();    //!< Temporary array of nodal free water transport coefficients

        //------------------------------------------------------------------------------
        /*! \brief Default class constructor.

            Creates a dead fuel moisture stick with the passed \a radius and \a name,
            and derives all other parameters using interpolations from \ref bevins2005.

            \param[in] radius Dead fuel stick radius (cm).
            \param[in] name Name or description of the dead fuel stick.
         */

        public DeadFuelMoisture(double radius = 0.64, string name = "" )            
        {
            m_semTime = new SemTime();
            initializeParameters(radius, name);
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Copy constructor.

            \param[in] r Reference to the DeadFuelMoisture from which to copy.
        */

        public DeadFuelMoisture(DeadFuelMoisture r )
        {
            m_semTime   = r.m_semTime;
            m_density   = r.m_density;
            m_dSteps    = r.m_dSteps;
            m_hc        = r.m_hc;
            m_length    = r.m_length;
            m_name      = r.m_name;
            m_nodes     = r.m_nodes;
            m_radius    = r.m_radius;
            m_rai0      = r.m_rai0;
            m_rai1      = r.m_rai1;
            m_stca      = r.m_stca;
            m_stcd      = r.m_stcd;
            m_mSteps    = r.m_mSteps;
            m_stv       = r.m_stv;
            m_wfilmk    = r.m_wfilmk;
            m_wmx       = r.m_wmx;
            m_allowRainfall2  = r.m_allowRainfall2;
            m_allowRainstorm  = r.m_allowRainstorm;
            m_pertubateColumn = r.m_pertubateColumn;
            m_rampRai0  = r.m_rampRai0;
            m_dx        = r.m_dx;
            m_wmax      = r.m_wmax;
            m_x         = r.m_x;
            m_v         = r.m_v;
            m_amlf      = r.m_amlf;
            m_capf      = r.m_capf;
            m_hwf       = r.m_hwf;
            m_dx_2      = r.m_dx_2;
            m_vf        = r.m_vf;
            m_bp0       = r.m_bp0;
            m_ha0       = r.m_ha0;
            m_rc0       = r.m_rc0;
            m_sv0       = r.m_sv0;
            m_ta0       = r.m_ta0;
            m_init      = r.m_init;
            m_bp1       = r.m_bp1;
            m_et        = r.m_et;
            m_ha1       = r.m_ha1;
            m_rc1       = r.m_rc1;
            m_sv1       = r.m_sv1;
            m_ta1       = r.m_ta1;
            m_ddt       = r.m_ddt;
            m_mdt       = r.m_mdt;
            m_mdt_2     = r.m_mdt_2;
            m_pptrate   = r.m_pptrate;
            m_ra0       = r.m_ra0;
            m_ra1       = r.m_ra1;
            m_rdur      = r.m_rdur;
            m_sf        = r.m_sf;
            m_hf        = r.m_hf;
            m_wsa       = r.m_wsa;
            m_sem       = r.m_sem;
            m_wfilm     = r.m_wfilm;
            m_elapsed   = r.m_elapsed;
            m_t         = r.m_t;
            m_s         = r.m_s;
            m_d         = r.m_d;
            m_w         = r.m_w;
            m_updates   = r.m_updates;
            m_state     = r.m_state;
            m_randseed  = r.m_randseed;
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Assignment operator.

        \param[in] r Reference to the DeadFuelMoisture instance from which to copy.
        */

        /*const DeadFuelMoisture& operator=( const DeadFuelMoisture& r )
        {
            if (this != &r)
            {
                m_semTime = r.m_semTime;
                m_density = r.m_density;
                m_dSteps = r.m_dSteps;
                m_hc = r.m_hc;
                m_length = r.m_length;
                m_name = r.m_name;
                m_nodes = r.m_nodes;
                m_radius = r.m_radius;
                m_rai0 = r.m_rai0;
                m_rai1 = r.m_rai1;
                m_stca = r.m_stca;
                m_stcd = r.m_stcd;
                m_mSteps = r.m_mSteps;
                m_stv = r.m_stv;
                m_wfilmk = r.m_wfilmk;
                m_wmx = r.m_wmx;
                m_allowRainfall2 = r.m_allowRainfall2;
                m_allowRainstorm = r.m_allowRainstorm;
                m_pertubateColumn = r.m_pertubateColumn;
                m_rampRai0 = r.m_rampRai0;
                m_dx = r.m_dx;
                m_wmax = r.m_wmax;
                m_x = r.m_x;
                m_v = r.m_v;
                m_amlf = r.m_amlf;
                m_capf = r.m_capf;
                m_hwf = r.m_hwf;
                m_dx_2 = r.m_dx_2;
                m_vf = r.m_vf;
                m_bp0 = r.m_bp0;
                m_ha0 = r.m_ha0;
                m_rc0 = r.m_rc0;
                m_sv0 = r.m_sv0;
                m_ta0 = r.m_ta0;
                m_init = r.m_init;
                m_bp1 = r.m_bp1;
                m_et = r.m_et;
                m_ha1 = r.m_ha1;
                m_rc1 = r.m_rc1;
                m_sv1 = r.m_sv1;
                m_ta1 = r.m_ta1;
                m_ddt = r.m_ddt;
                m_mdt = r.m_mdt;
                m_mdt_2 = r.m_mdt_2;
                m_pptrate = r.m_pptrate;
                m_ra0 = r.m_ra0;
                m_ra1 = r.m_ra1;
                m_rdur = r.m_rdur;
                m_sf = r.m_sf;
                m_hf = r.m_hf;
                m_wsa = r.m_wsa;
                m_sem = r.m_sem;
                m_wfilm = r.m_wfilm;
                m_elapsed = r.m_elapsed;
                m_t = r.m_t;
                m_s = r.m_s;
                m_d = r.m_d;
                m_w = r.m_w;
                m_updates = r.m_updates;
                m_state = r.m_state;
                m_randseed = r.m_randseed;
            }
            return (*this);
        }*/

            //------------------------------------------------------------------------------
            /*! \brief Virtual class destructor.
             */

        ~DeadFuelMoisture()
        {
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Access to the stick's adsorption surface mass transfer rate.
  
            \return Current surface mass transfer rate for adsorption
            (\f$\frac {cm^{3}} {cm^{2} \cdot h}\f$).
         */

        double adsorptionRate()
        {
            return( m_stca );
        }

        //------------------------------------------------------------------------------
        /*! \brief Access to DeadFuelMoisture class name.
  
            \return Pointer to static DeadFuelMoisture char string.
         */

        /*const char* className( void ) const
        {
            return( "DeadFuelMoisture" );
        }*/

        //------------------------------------------------------------------------------
        /*! \brief Static convenience method to create a 1-h time lag
            DeadFuelMoisture instance with a 0.20-cm radius (0.16-in diameter).
            \param[in] name Dead fuel moisture stick name.
  
            \relates DeadFuelMoisture
  
            \return Pointer to the dynamically-allocated DeadFuelMoisture instance.
         */

        static public DeadFuelMoisture createDeadFuelMoisture1(string name = "")
        {
            return (new DeadFuelMoisture(0.20, name));
        }

        //------------------------------------------------------------------------------
        /*! \brief Static convenience method to create a 10-h time lag
            DeadFuelMoisture instance with a 0.64-cm radius (0.5-in diameter).
            \param[in] name Dead fuel moisture stick name.
  
            \relates DeadFuelMoisture
  
            \return Pointer to the dynamically-allocated DeadFuelMoisture instance.
         */

        static public DeadFuelMoisture createDeadFuelMoisture10(string name = "")
        {
            return (new DeadFuelMoisture(0.64, name));
        }

        //------------------------------------------------------------------------------
        /*! \brief Static convenience method to create a 100-h time lag
            DeadFuelMoisture instance with a 2.0-cm radius (1.57-in diameter).
            \param[in] name Dead fuel moisture stick name.
  
            \relates DeadFuelMoisture
  
            \return Pointer to the dynamically-allocated DeadFuelMoisture instance.
         */

        static public DeadFuelMoisture createDeadFuelMoisture100(string name = "")
        {
            return (new DeadFuelMoisture(2.00, name));
        }

        //------------------------------------------------------------------------------
        /*! \brief Static convenience method to create a 1000-h time lag
            lag DeadFuelMoisture instance with a 6.4-cm radius (5-in diameter).
            \param[in] name Dead fuel moisture stick name.
  
            \relates DeadFuelMoisture
  
            \return Pointer to the dynamically-allocated DeadFuelMoisture instance.
         */

        static public DeadFuelMoisture createDeadFuelMoisture1000(string name = "")
        {
            return (new DeadFuelMoisture(6.40, name));
        }

        //------------------------------------------------------------------------------
        /*! \brief Static convenience method to determine the adsorption surface mass
            transfer rate for a DeadFuelMoisture stick of given \a radius.
  
            The adsorption surface mass transfer rate is determined from
            \ref bevins2005
            \f[\alpha = 0.0004509 + \frac {0.006126}{r^2.6}\f]
            where
            - \f$\alpha\f$ is the adsorption surface mass transfer rate
                (\f$\frac {cm^{3}} {cm^{2} \cdot h}\f$), and
            - \f$r\f$ is the stick radius (\f$cm\f$).
  
            \param[in] radius Dead fuel stick radius (\f$cm\f$).
  
            \return Estimated surface mass transfer rate for adsorption
            (\f$\frac {cm^{3}} {cm^{2} \cdot h}\f$).
         */

        double deriveAdsorptionRate(double radius)
        {
            double alpha = 0.0004509 + 0.006126 / pow(radius, 2.6);
            return (alpha);
        }

        //------------------------------------------------------------------------------
        /*! \brief Static convenience method to determine the minimum number of
            diffusivity computation time steps per observation
            for a DeadFuelMoisture stick of given \a radius.
  
            The minimum number of diffusivity computation time steps per observation
            must be large enough to provide stability in the model computations
            (\ref carlson2004a ), and is used to determine the internal diffusivity
            computation time step within update().
  
            The minumum number of diffusivity computation time steps is determined from
            \ref bevins2005
            \f[$n_{d} = int( 4.777 + \frac {2.496}{r^1.3} )\f]
            where
            - \f$n_{d}\f$ is the minimum number of diffusivity computation time steps
            per observation, and
            - \f$r\f$ is the stick radius (\f$cm\f$).
  
            \param[in] radius Dead fuel stick radius (\f$cm\f$).
  
            \return Minimum number of diffusivity computation time steps per observation.
         */

        int deriveDiffusivitySteps(double radius)
        {
            int steps = (int)(4.777 + 2.496 / pow(radius, 1.3));
            return (steps);
        }

        //------------------------------------------------------------------------------
        /*! \brief Static convenience method to determine the minimum number of
            moisture content computation time steps per observation
            for a DeadFuelMoisture stick of given \a radius.
  
            The number of moisture computation time steps per observation must be
            large enough to provide stability in the model computations
            (\ref carlson2004a ), and is used to determine the internal moisture content
            computation time step within update().
  
            The minimum number of moisture content computation time steps
            is determined from
            \ref bevins2005 \f[$n_{m} = int( 9.8202 + \frac {26.865}{r^1.4} )\f]
            where
            - \f$n_{m}\f$ is the minimum number of moisture content computation time
            steps per observation, and
            - \f$r\f$ is the stick radius (\f$cm\f$).
  
            \param[in] radius Dead fuel stick radius (\f$cm\f$).
  
            \return Minimum number of moisture computation time steps per observation.
         */

        int deriveMoistureSteps(double radius)
        {
            int steps = (int)(9.8202 + 26.865 / pow(radius, 1.4));
            return (steps);
        }

        //------------------------------------------------------------------------------
        /*! \brief Static convenience method to determine the planar heat transfer rate
            for a DeadFuelMoisture stick of given \a radius.
  
            The planar heat transfer rate is determined from \ref bevins2005
            \f[$h_{c} = 0.2195 + \frac {0.05260}{r^2.5}\f]
            where
            - \f$h_{c}\f$ is the planar heat transfer rate
                (\f$\frac {cal} {cm^{2} \cdot h \cdot C}\f$), and
            - \f$r\f$ is the stick radius (\f$cm\f$).
  
            \param[in] radius Dead fuel stick radius (\f$cm\f$).
  
            \return Estimated planar heat transfer rate
                (\f$\frac {cal} {cm^{2} \cdot h \cdot C}\f$).
         */

        double derivePlanarHeatTransferRate(double radius)
        {
            double hc = 0.2195 + 0.05260 / pow(radius, 2.5);
            return (hc);
        }

        //------------------------------------------------------------------------------
        /*! \brief Static convenience method to determine the rainfall runoff factor
            for a DeadFuelMoisture stick of given \a radius.
  
            The initial rainfall runoff factor is determined from \ref bevins2005
            \f[$f_{0} = 0.02822 + \frac {0.1056}{r^2.2}\f]
            where
            - \f$f_{0}\f$ is the initial rainfall runoff rate (\f$dl\f$), and
            - \f$r\f$ is the stick radius (\f$cm\f$).
  
            \param[in] radius Dead fuel stick radius (\f$cm\f$).
  
            \return Estimated initial rainfall runoff factor (\f$dl\f$).
         */

        double deriveRainfallRunoffFactor(double radius)
        {
            double rrf0 = 0.02822 + 0.1056 / pow(radius, 2.2);
            return (rrf0);
        }

        //------------------------------------------------------------------------------
        /*! \brief Static convenience method to determine the number of moisture content
            radial computation nodes for a DeadFuelMoisture stick of given \a radius.
  
            The number of moisture computation nodes must be large enough to provide
            stability in the model computations (\ref carlson2004a ).
  
            The minimum number of moisture content computation nodes
            is determined from
            \ref bevins2005 \f[$n_{m} = int( 10.727 + \frac {0.1746}{r} )\f]
            where
            - \f$n_{m}\f$ is the minimum number of moisture content computation
                radial nodes,
            - \f$r\f$ is the stick radius (\f$cm\f$).
  
            \param[in] radius Dead fuel stick radius (\f$cm\f$).
  
            \return Minimum number of moisture computation radial nodes.
         */

        int deriveStickNodes(double radius)
        {
            int nodes = (int)(10.727 + 0.1746 / radius);
            if ((nodes % 2) == 0)
            {
                nodes++;
            }
            return (nodes);
        }

        //------------------------------------------------------------------------------
        /*! \brief Access to the stick's desorption surface mass transfer rate.
  
            \return Current surface mass transfer rate for desorption
            (\f$\frac {cm^{3}} {cm^{2} \cdot h}\f$).
         */

        double desorptionRate()
        {
            return( m_stcd );
        }

        //------------------------------------------------------------------------------
        /*! \brief Determines bound water diffusivity at each radial nodes.
            \param[in] bp Barometric pressure (cal/m3)
         */

        void diffusivity(double bp)
        {
            //changed SB, 10/2009, moce variable declarations outside of loop
            double tk, qv, cpv, dv, ps1, c1, c2, wc, dhdm, daw, svaw, vfaw, vfcw, rfcw, fac, con, qw, e, dvpr;
            // Loop for each node
            for (int i = 0; i < m_nodes; i++)
            {
                // Stick temperature (oK)
                tk = m_t[i] + 273.2;
                // Latent heat of vaporization of water (cal/mol
                qv = 13550.0 - 10.22 * tk;
                // Specific heat of water vapor (cal/(mol*K))
                //cpv   = 7.22 + .002374 * tk + 2.67e-07 * tk * tk;
                cpv = 7.22 + (tk * (0.002374 + 2.67e-07 * tk));
                // Sea level atmospheric pressure = 0.0242 cal/cm3
                dv = 0.22 * 3600.0 * (0.0242 / bp)
                             * pow((tk / 273.2), 1.75);
                // Water saturation vapor pressure at surface temp (cal/cm3)
                ps1 = 0.0000239 * exp(20.58 - (5205.0 / tk));
                // Emc sorption isotherm parameter (g/g)
                c1 = 0.1617 - 0.001419 * m_t[i];
                // Emc sorption isotherm parameter (g/g)
                c2 = 0.4657 + 0.003578 * m_t[i];
                // Lesser of nodal or fiber saturation moisture (g/g)
                //wc; //bran-jnw
                // Reciprocal slope of the sorption isotherm
                dhdm = 0.0;
                if (m_w[i] < m_wsa)
                {
                    wc = m_w[i];
                    if (c2 != 1.0 && m_hf < 1.0 && c1 != 0.0 && c2 != 0.0)
                    {
                        dhdm = (1.0 - m_hf) * pow(-log(1.0 - m_hf), (1.0 - c2)) / (c1 * c2);
                    }
                }
                else
                {
                    wc = m_wsa;
                    if (c2 != 1.0 && Hfs < 1.0 && c1 != 0.0 && c2 != 0.0)
                    {
                        dhdm = (1.0 - Hfs) * pow(Wsf, (1.0 - c2)) / (c1 * c2);
                    }
                }
                // Density of adsorbed water (g/cm3)
                daw = 1.3 - 0.64 * wc;
                // Specific volume of adsorbed water (cm3/g)
                svaw = 1.0 / daw;
                // Volume fraction of adborbed water (dl)
                vfaw = svaw * wc / (0.685 + svaw * wc);
                // Volume fraction of moist cell wall (dl)
                vfcw = (0.685 + svaw * wc) / ((1.0 / m_density) + svaw * wc);
                // Converts D from wood substance to whole wood basis
                rfcw = 1.0 - sqrt(1.0 - vfcw);
                // Converts D from wood substance to whole wood basis
                fac = 1.0 / (rfcw * vfcw);
                // Correction for tortuous paths in cell wall
                con = 1.0 / (2.0 - vfaw);
                // Differential heat of sorption of water (cal/mol)
                qw = 5040.0 * exp(-14.0 * wc);
                // Activation energy for bound water diffusion (cal/mol)
                e = (qv + qw - cpv * tk) / 1.2;

                //----------------------------------------------------------------------
                // The factor 0.016 below is a correction for hindered water vapor
                // diffusion (this is 62.5 times smaller than the bulk vapor diffusion);
                //  0.0242 cal/cm3 = sea level atm pressure
                //      -- note from Ralph Nelson
                //----------------------------------------------------------------------

                dvpr = 18.0 * 0.016 * (1.0 - vfcw) * dv * ps1 * dhdm / (m_density * 1.987 * tk);
                m_d[i] = dvpr + 3600.0 * 0.0985 * con * fac * exp(-e / (1.987 * tk));
            }
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Access to the stick's number of moisture diffusivity computation
            time steps per observation.
  
            \return Current number of moisture diffusivity computation time steps
            per observation.
         */

        int diffusivitySteps()
        {
            return( m_dSteps );
        }

        //------------------------------------------------------------------------------
        /*! \brief Access to the current total running elapsed time.
  
            \return The current total running elapsed time (h).
         */

        double elapsedTime()
        {
            return( m_elapsed );
        }

        //------------------------------------------------------------------------------
        /*! \brief Reports whether the client has called initializeEnvironment().
  
            \retval TRUE if client has called initializeEnvironment().
            \retval FALSE if client has not called initializeEnvironment().
         */

        bool initialized()
        {
            return( m_init );
        }

        //------------------------------------------------------------------------------
        /*! \brief 
  
            \param[in] year     Observation year (4 digits).
            \param[in] month    Observation month (Jan==1, Dec==12).
            \param[in] day      Observation day-of-the-month [1..31].
            \param[in] hour     Observation elapsed hours in the day [0..23].
            \param[in] minute   Observation elapsed minutes in the hour (0..59].
            \param[in] second   Observation elapsed seconds in the minute [0..59].
            \param[in] ta Initial ambient air temperature (oC).
            \param[in] ha Initial ambient air relative humidity (g/g).
            \param[in] sr Initial solar radiation (W/m2).
            \param[in] rc Initial cumulative rainfall amount (cm).
            \param[in] ti Initial stick temperature (oC).
            \param[in] hi Initial stick surface relative humidty (g/g).
            \param[in] wi Initial stick fuel moisture fraction (g/g).
            \param[in] bp Initial stick barometric pressure (cal/cm3).
         */
        /// <summary>
        /// Initializes a dead fuel moisture stick's internal and external 
        /// environment and its DateTime.
        /// Initializes the stick's internal and external environmental variables.
        /// The stick's internal temperature and water content are presumed to be
        /// uniformly distributed.
        /// This overloaded version also initializes the stick's Julian date from
        /// the passed date and time parameters.0  The first call to update()
        /// calculates elapsed time from the date/time passed here.
        /// </summary>
        /// <param name="year">Observation year (4 digits).</param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        /// <param name="ta"></param>
        /// <param name="ha"></param>
        /// <param name="sr"></param>
        /// <param name="rc"></param>
        /// <param name="ti"></param>
        /// <param name="hi"></param>
        /// <param name="wi"></param>
        /// <param name="bp"></param>
        public void initializeEnvironment(
            int year,
            int month,
            int day,
            int hour,
            int minute,
            int second,
            double ta,
            double ha,
            double sr,
            double rc,
            double ti,
            double hi,
            double wi,
            double bp = 0.0218
        )
        {
            m_semTime.set(year, month, day, hour, minute, second);
            initializeEnvironment(ta, ha, sr, rc, ti, hi, wi, bp);
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Initializes a dead fuel moisture stick's internal and external
            environment.
  
            Initializes the stick's internal and external environmental variables.
            The stick's internal temperature and water content are presumed to be
            uniformly distributed.
  
            \param[in] ta Initial ambient air temperature (oC).
            \param[in] ha Initial ambient air relative humidity (g/g).
            \param[in] sr Initial solar radiation (W/m2).
            \param[in] rc Initial cumulative rainfall amount (cm).
            \param[in] ti Initial stick temperature (oC).
            \param[in] hi Initial stick surface relative humidty (g/g).
            \param[in] wi Initial stick fuel moisture fraction (g/g).
            \param[in] bp Initial stick barometric pressure (cal/cm3).
         */

        void initializeEnvironment(
            double ta,
            double ha,
            double sr,
            double rc,
            double ti,
            double hi,
            double wi,
            double bp = 0.0218
        )
        {
            // Environment initialization
            m_ta0 = m_ta1 = ta;     // Previous and current ambient air temperature (oC)
            m_ha0 = m_ha1 = ha;     // Previous and current ambient air relative humidity (g/g)
            m_sv0 = m_sv1 = sr / Smv; // Previous and current solar insolation (millivolts)
            m_rc0 = m_rc1 = rc;     // Previous and current cumulative rainfall amount (cm)
            m_ra0 = m_ra1 = 0.0;     // Previous and current observation period's rainfall amount (cm)
            m_bp0 = m_bp1 = bp;     // Previous and current observation barometric pressure (cal/m3)

            // Stick initialization
            m_hf = hi;           // Relative humidity at fuel surface (g/g)
            m_wfilm = 0.0;          // Water film moisture contribution (g/g)
            m_wsa = wi + 0.1;      // Stick fiber saturation point (g/g)

            //bran-jnw
            /*fill(m_t.begin(), m_t.end(), ti);
            fill(m_w.begin(), m_w.end(), wi);
            fill(m_s.begin(), m_s.end(), 0.0);*/
            for (int i = 0; i < m_t.Count; i++)
            {
                m_t[i] = ti;
            }
            for (int i = 0; i < m_w.Count; i++)
            {
                m_w[i] = wi;
            }
            for (int i = 0; i < m_s.Count; i++)
            {
                m_s[i] = 0.0;
            }

            diffusivity(m_bp0);
            m_init = true;
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Initializes a DeadFuelMoisture stick with model parameters inferred
            from the \a radius.
  
            \param[in] radius Dead fuel stick radius (cm).
            \param[in] name Name or description of the dead fuel stick.
         */

        void initializeParameters(double radius, string name )
        {
            initializeParameters(
                name,
                radius,
                deriveStickNodes(radius),
                deriveMoistureSteps(radius),
                deriveDiffusivitySteps(radius),
                derivePlanarHeatTransferRate(radius),
                deriveAdsorptionRate(radius),
                deriveRainfallRunoffFactor(radius),
                41.0,   // stick length (cm)
                0.400,  // stick density (g/cm3)
                0.0,    // water film contribution "wfilmk"
                0.60,   // maximum local moisture content "wmx" // 2014
                0.06,   // desorption rate "stcd"
                0.5,    // rainfall adjustment factor "rai1" (not used)
                9999.0,  // storm transition value "stv" (not used)
                0,      // random number seed (none)
                true,  // If TRUE, applies Nelson's logic for rainfall runoff after the first hour
                true,  // If TRUE, applies Nelson's logic for rainstorm transition and state
                true,   // If TRUE, the continuous liquid column condition get pertubated
                true    // If TRUE, used Bevins' ramping of rainfall runoff factor rather than Nelsons rai0 *= 0.15
            );
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Initializes a DeadFuelMoisture stick.
  
            Initializes a dead fuel moisture stick, applying the passed parameters.
  
            \param[in] name Name or description of the dead fuel stick.
            \param[in] radius Dead fuel stick radius (cm).
            \param[in] stickNodes Number of stick nodes in the radial direction
                        [required]
            \param[in] moistureSteps Number of moisture content computation steps
                        per observation [required].
            \param[in] diffusivitySteps Number of diffusivity computation steps per
                        observation [required].
            \param[in] planarHeatTransferRate Stick planar heat transfer rate
                        (cal/cm2-h-C) [required].
            \param[in] adsorptionRate Adsorption surface mass transfer rate
                        ((cm3/cm2)/h) [required].
            \param[in] rainfallRunoffFactor Rain runoff factor (dl) [required].
            \param[in] stickLength Stick length (cm) [optional, default = 41 cm].
            \param[in] stickDensity Stick density (g/cm3) [optional, default = 0.40].
            \param[in] waterFilmContribution Water film contribution to stick moisture
                        content (g water/g dry fuel) [optional, default = 0].
            \param[in] localMaxMc Stick maximum local moisture due to rain
                        (g water/g dry fuel) [optional, default = 0.6].
            \param[in] desorptionRate Desorption surface mass transfer rate
                        ((cm3/cm2)/h) [optional, default = 0.06].
            \param[in] rainfallAdjustmentFactor [not used].
            \param[in] stormTransitionValue Storm transition value (cm/h) [not used].
            \param[in] randseed If not zero, nodal temperature, saturation, and
                        moisture contents are pertibated by some small amount.
                        If > 0, this value is used as the seed.
                        If < 0, uses system clock for seed.
            \param[in] allowRainfall2 If TRUE, applies Nelson's logic for rainfall runoff
                        after the first hour [default=true].
            \param[in] allowRainstorm If TRUE, applies Nelson's logic for rainstorm transition
                        and state [default=true].
            \param[in] pertubateColumn If TRUE, whenever the continuous liquid column condition
                        occurs, the nodal moistures get pertubated by a small amount [default=true].
            \param[in] rampRai0 If TRUE, uses Bevins' ramping of rainfall runoff factor
                        rather than Nelsons rai0 *= 0.15 [default=false].
         */

        void initializeParameters(
                string name,
                double radius,
                int stickNodes,
                int moistureSteps,
                int diffusivitySteps,
                double planarHeatTransferRate,
                double adsorptionRate,
                double rainfallRunoffFactor,
                double stickLength = 41.0,
                double stickDensity = 0.4,
                double waterFilmContribution = 0.0,
                double localMaxMc = 0.6,
                double desorptionRate = 0.06,
                double rainfallAdjustmentFactor = 0.5,
                double stormTransitionValue = 99999.0,
                int randseed = 0,
                bool allowRainfall2 = false,
                bool allowRainstorm = false,
                bool pertubateColumn = true,
                bool rampRai0 = true
             )
        {
            // Start with everything set to zero
            zero();
            // Constrain and store the passed parameters
            m_name = name;
            m_randseed = randseed;
            m_radius = radius;
            m_length = stickLength;
            m_density = stickDensity;
            m_dSteps = diffusivitySteps;
            m_hc = planarHeatTransferRate;
            m_nodes = stickNodes;
            m_rai0 = rainfallRunoffFactor;
            m_rai1 = rainfallAdjustmentFactor;
            m_stca = adsorptionRate;
            m_stcd = desorptionRate;
            m_mSteps = moistureSteps;
            m_stv = stormTransitionValue;
            m_wmx = localMaxMc;
            m_wfilmk = waterFilmContribution;
            m_allowRainfall2 = allowRainfall2;
            m_allowRainstorm = allowRainstorm;
            m_pertubateColumn = pertubateColumn;
            m_rampRai0 = rampRai0;
            // Initialize all other stick parameters and intermediates
            initializeStick();
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Initializes a dead fuel moisture stick's model parameters.
            \note Should be called after and set<parameter>().
         */

        public void initializeStick()
        {
            // Should we randomize nodal moisture, saturation, and temperatures by some
            // small, insignificant amount to introduce computational stability when
            // propagating these parameters within the stick?
            // If > 0, the value is used as a random generator seed.
            // If < 0, the system clock is used to get the seed.
            setRandomSeed(m_randseed);

            // Internodal distance (cm)
            m_dx = m_radius / (double)(m_nodes - 1);
            m_dx_2 = m_dx * 2.0;

            // Maximum possible stick moisture content (g/g)
            m_wmax = (1.0 / m_density) - (1.0 / 1.53);

            //bran-jnw
            /*// Initialize ambient air temperature to 20 oC
            m_t.insert(m_t.begin(), m_nodes, 20.0);
            // Initialize fiber saturation point to 0 g/g
            m_s.insert(m_s.begin(), m_nodes, 0.0);
            // Initialize bound water diffusivity to 0 cm2/h
            m_d.insert(m_d.begin(), m_nodes, 0.0);
            // Initialize moisture content to half the local maximum (g/g)
            m_w.insert(m_w.begin(), m_nodes, (0.5 * m_wmx));*/
            //bran-jnw: should be cleared in zero()
            /*m_t.Clear();
            m_s.Clear();
            m_d.Clear();
            m_w.Clear();*/
            for (int i = 0; i < m_nodes; i++)
            {
                m_t.Add(20.0);
                m_s.Add(0.0);
                m_d.Add(0.0);
                m_w.Add(0.5 * m_wmx);
            }

            // Derive nodal radial distances
            m_x.Clear();
            for (int i = 0; i < m_nodes - 1; i++)
            {
                // Initialize radial distance from center of stick (cm)
                //m_x.push_back(m_radius - (m_dx * i));
                m_x.Add(m_radius - (m_dx * i));
            }
            m_x.Add(0.0);

            // Derive nodal volume fractions
            m_v.Clear();
            double ro = m_radius;
            double ri = ro - 0.5 * m_dx;
            double a2 = m_radius * m_radius;
            m_v.Add((ro * ro - ri * ri) / a2);
            double vwt = m_v[0];
            for (int i = 1; i < m_nodes - 1; i++)
            {
                ro = ri;
                ri = ro - m_dx;
                m_v.Add((ro * ro - ri * ri) / a2);
                vwt += m_v[i];
            }
            m_v.Add(ri * ri / a2);
            vwt += m_v[m_nodes - 1];

            //bran-jnw
            /*// Added by Stuart Brittain on 1/14/2007
            // for performance improvement in update()
            m_Twold.insert(m_Twold.begin(), m_nodes, 0.0);
            m_Ttold.insert(m_Ttold.begin(), m_nodes, 0.0);
            m_Tsold.insert(m_Tsold.begin(), m_nodes, 0.0);
            m_Tv.insert(m_Tv.begin(), m_nodes, 0.0);
            m_To.insert(m_To.begin(), m_nodes, 0.0);
            m_Tg.insert(m_Tg.begin(), m_nodes, 0.0);*/
            for (int i = 0; i < m_nodes; i++)
            {
                m_Twold.Add( 0.0);
                m_Ttold.Add(0.0);
                m_Tsold.Add(0.0);
                m_Tv.Add(0.0);
                m_To.Add(0.0);
                m_Tg.Add(0.0);
            }

            // Initialize the environment, but set m_init to FALSE when done
            initializeEnvironment(
                20.0,        // Ambient air temperature (oC)
                0.20,       // Ambient air relative humidity (g/g)
                0.0,        // Solar radiation (W/m2)
                0.0,        // Cumulative rainfall (cm)
                20.0,       // Initial stick temperature (oC)
                0.20,       // Initial stick surface humidity (g/g)
                0.5 * m_wmx   // Initial stick moisture content
            );
            m_init = false;

            //-------------------------------------------------------------------------
            // Computation optimization parameters
            //-------------------------------------------------------------------------

            // m_hwf == hw and aml computation factor used in update()
            m_hwf = 0.622 * m_hc * pow((Pr / Sc), 0.667);

            // m_amlf == aml optimization factor
            m_amlf = m_hwf / (0.24 * m_density * m_radius);

            // m_capf = cap optimization factor.0 */
            double rcav = 0.5 * Aw * Wl;
            m_capf = 3600.0 * Pi * St * rcav * rcav
                   / (16.0 * m_radius * m_radius * m_length * m_density);

            // m_vf == optimization factor used in update()
            // WAS: m_vf = St / (Aw * Wl * Scr);
            // WAS: m_vf = St / (Wl * Scr);
            m_vf = St / (m_density * Wl * Scr);
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Access to the stick's current maximum local moisture content.
  
            \return Stick's current maximum local moisture content (g/g).
         */

        double maximumLocalMoisture()
        {
            return( m_wmx );
        }

        //------------------------------------------------------------------------------
        /*! \brief Determines the mean moisture content of the stick's radial profile.
  
            \note
            The integral average of the stick's nodal moisture contents is calculated
            without consideration to the nodes' volumetric representation.
  
            \deprecated Use Fms_MeanWtdMoisture() for a volume-weighted mean
            moisture content.
  
            \return The mean moisture content of the stick's radial profile (g/g).
         */

        double meanMoisture()
        {
            double wea, web;
        double wec = m_w[0];
        double wei = m_dx / (3.0 * m_radius);
        for (int i = 1; i < m_nodes - 1; i += 2)
        {
            wea = 4.0 * m_w[i];
            web = 2.0 * m_w[i + 1];
            if ((i + 1) == (m_nodes - 1))
            {
                web = m_w[m_nodes - 1];
            }
            wec += web + wea;
        }
        double wbr = wei * wec;
        wbr = (wbr > m_wmx) ? m_wmx : wbr;

        // Add water film
        wbr += m_wfilm;
        return (wbr);
        }

        //------------------------------------------------------------------------------
        /*! \brief Determines the volume-weighted mean moisture content of the
            stick's radial profile.
  
            \return The volume-weighted mean moisture content of the stick's radial
            profile (g/g).
         */

        public double meanWtdMoisture()
        {
            double wbr = 0.0;
        for (int i = 0; i < m_nodes; i++)
        {
            wbr += m_w[i] * m_v[i];
        }
        wbr = (wbr > m_wmx) ? m_wmx : wbr;

        // Add water film
        wbr += m_wfilm;
        return (wbr);
        }

        //------------------------------------------------------------------------------
        /*! \brief Determines the volume-weighted mean temperature of the stick's
            radial profile.
  
            \return The volume-weighted mean temperature of the stick's radial
            profile (oC).
         */

        double meanWtdTemperature()
        {
            double wbr = 0.0;
        for (int i = 0; i < m_nodes; i++)
        {
            wbr += m_t[i] * m_v[i];
        }
        return (wbr);
        }

        //------------------------------------------------------------------------------
        /*! \brief Access to the stick's number of moisture content computation time
            steps per observation.
  
            \return Current number of moisture content computation time steps
             per observation.
         */

        int moistureSteps()
        {
            return( m_mSteps );
        }

        //------------------------------------------------------------------------------
        /*! \brief Access to the stick's name.
  
            \return The stick's name.
         */

        string name()
        {
            return( m_name );
        }

        //------------------------------------------------------------------------------
        /*! \brief Access to the stick's current precipitation rate (cm/h).
  
            \return The current (most recent) precipitation rate (cm/h).
         */

        double pptRate()
        {
            return( ( m_et > 0.00 ) ? (m_ra1 / m_et) : 0.00 );
        }

        //------------------------------------------------------------------------------
        /*! \brief Access to the stick's current planar heat transfer rate..
  
            \return Stick's current planar heat transfer rate
                (\f$\frac {cal} {cm^{2} \cdot h \cdot C}\f$).
         */

        double planarHeatTransferRate()
        {
            return( m_hc );
        }

        //------------------------------------------------------------------------------
        /*! \brief Access to the stick's current rainfall runoff factor.
  
            \return Stick's current rainfall runoff factor (dl).
         */

        double rainfallRunoffFactor()
        {
            return( m_rai0 );
        }

        //------------------------------------------------------------------------------
        /*! \brief Updates the stick's adsorption rate.
  
            \param[in] adsorptionRate Adsorption surface mass transfer rate ((cm3/cm2)/h).
         */

        void setAdsorptionRate(double adsorptionRate)
        {
            m_stca = adsorptionRate;
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Updates the stick's configuration to toggle Nelson's logic
            for rainfall runoff factor after the first hour of rain.
  
            \param[in]  allow (default=true).
         */

        void setAllowRainfall2(bool allow = true)
        {
            m_allowRainfall2 = allow;
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Updates the stick's configuration to toggle Nelson's logic
            to allow the Rainstorm state and rainfall-to-rainstorm transition.
  
            \param[in]  allow (default=true).
         */

        void setAllowRainstorm(bool allow = true)
        {
            m_allowRainstorm = allow; ;
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Updates the stick's desorption rate.
  
            \param[in] desorptionRate Desorption surface mass transfer rate ((cm3/cm2)/h).
         */

        void setDesorptionRate(double desorptionRate = 0.06)
        {
            m_stcd = desorptionRate;
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Updates the stick's diffusivity computation steps per update().
  
            \param[in] diffusivitySteps Number of diffusivity computation steps per observation.
         */

        void setDiffusivitySteps(int diffusivitySteps)
        {
            m_dSteps = diffusivitySteps;
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Updates the stick's maximum local moisture content.
  
            \param[in] localMaxMc Stick maximum local moisture due to rain (g water/g dry fuel).
         */

        void setMaximumLocalMoisture(double localMaxMc= 0.6)
        {
            m_wmx = localMaxMc;
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Updates the stick's moisture content computation steps per update().
  
            \param[in] moistureSteps Number of moisture content computation steps per observation.
         */

        void setMoistureSteps(int moistureSteps)
        {
            m_mSteps = moistureSteps;
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Updates the stick's planar heat transfer rate.
  
            \param[in] planarHeatTransferRate Stick planar heat transfer rate (cal/cm2-h-C).
         */

        void setPlanarHeatTransferRate(double planarHeatTransferRate)
        {
            m_hc = planarHeatTransferRate;
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Updates the stick's column pertubation configuration.
  
            \param[in] pertubate (default=true).
         */

        void setPertubateColumn(bool pertubate = true)
        {
            m_pertubateColumn = pertubate;
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Updates the stick's rainfall runoff factor.
  
            \param[in] rainfallRunoffFactor Rain runoff factor during the initial hour of rainfall (dl).
         */

        void setRainfallRunoffFactor(double rainfallRunoffFactor)
        {
            m_rai0 = rainfallRunoffFactor;
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Updates the stick's configuration to toggle Bevins' logic
            that ramps the rainfall runoff factor during falling humidity, rather than
            use Nelson's rai0 *= 0.15.
  
            \param[in]  ramp (default=true).
         */

        void setRampRai0(bool ramp)
        {
            m_rampRai0 = ramp;
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Sets the ransom seed behavior.
  
            \param[in] randseed If not zero, nodal temperature, saturation,
            and moisture contents are pertubated by some small amount.
            If > 0, this value is used as the seed.0 If < 0, uses system clock for seed.
         */

        void setRandomSeed(int randseed = 0)
        {
            m_randseed = randseed;
            if (m_randseed > 0)
            {
                srand(m_randseed );
            }
            else if (m_randseed < 0)
            {
                srand();
            }
            return;
        }

        System.Random randGen;
        void srand(int seed)
        {
            System.Random randGen = new System.Random(seed);
        }

        void srand()
        {
            System.Random randGen = new System.Random();
        }

        int rand()
        {
            if(randGen == null)
            {
                srand();
            }
            return randGen.Next();
        }

        //------------------------------------------------------------------------------
        /*! \brief Updates the stick density.
  
            \param[in] stickDensity Stick density (g/cm3) [optional, default = 0.40].
         */

        void setStickDensity(double stickDensity = 0.4)
        {
            m_density = stickDensity;
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Updates the stick length.
  
            \param[in] stickLength Stick length (cm) [optional, default= 41 cm].
         */

        void setStickLength(double stickLength = 41.0)
        {
            m_length = stickLength;
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Updates the number of stick radial computation nodes.
  
            \param[in] stickNodes Number of stick nodes in the radial direction [optional, default = 11].
         */

        void setStickNodes(int stickNodes = 11)
        {
            m_nodes = stickNodes;
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Updates the water film contribution to stick weight.
  
            \param[in] waterFilm Water film contribution to stick moisture content (g water/g dry fuel).
         */

        void setWaterFilmContribution(double waterFilm = 0.0)
        {
            m_wfilm = waterFilm;
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Access to prevailing state for most recent update.
            \return Prevailing state for most recent update.
         */

        DFM_State state()
        {
            return( m_state );
        }

        //------------------------------------------------------------------------------
        /*! \brief Access to the stick's current state name.
  
            \return The current state name.
         */

        string stateName()
        {
            string[] State = new string[11]
            {
                "None",             // 0,
                "Adsorption",       // 1,
                "Desorption",       // 2,
                "Condensation1",    // 3,
                "Condensation2",    // 4,
                "Evaporation",      // 5,
                "Rainfall1",        // 6,
                "Rainfall2",        // 7,
                "Rainstorm",        // 8,
                "Stagnation",       // 9,
                "Error"             // 10
            };
            return (State[(int)m_state]);
        }

        //------------------------------------------------------------------------------
        /*! \brief Access to the stick's particle density.
  
            \return Current stick density (g/cm3).
         */

        double stickDensity()
        {
            return( m_density );
        }

        //------------------------------------------------------------------------------
        /*! \brief Access to the stick's length.
  
            \return Current stick length (cm).
         */

        double stickLength()
        {
            return( m_length );
        }

        //------------------------------------------------------------------------------
        /*! \brief Access to the stick's number of moisture content radial computation
            nodes.
  
            \return Current number of moisture content radial computation  nodes.
             per observation.
         */

        int stickNodes()
        {
            return( m_nodes );
        }

        //------------------------------------------------------------------------------
        /*! \brief Access to the stick's surface fuel moisture content.
  
            \return The current surface fuel moisture content (g/g).
         */

        double surfaceMoisture()
        {
            return( m_w[0] );
        }

        //------------------------------------------------------------------------------
        /*! \brief Access to the stick's surface fuel temperature.
  
            \return The current surface fuel temperature (oC).
         */

        double surfaceTemperature()
        {
            return( m_t[0] );
        }

        //------------------------------------------------------------------------------
        /*! \brief Static convenience method to derive a random number uniformly
            distributed in the range [\a min ..0 \a max].
  
            \param[in] min  Minimum range value.
            \param[in] max  Maximum range value.
  
            Uses the system rand() to generate the number.
  
            \return A uniformly distributed random number within [\a min ..0 \a max].
         */

        double uniformRandom(double min, double max)
        {
            return ((max - min) * ((double)rand() / (double)int.MaxValue) + min);
        }

        //------------------------------------------------------------------------------
        /*! \brief Updates a dead moisture stick's internal and external environment
            based on the current weather observation values.
  
            This overloaded version accepts the current date and time as arguments,
            and automatically calculates elapsed time since the previous update().
  
            \note The client must have called the corresponding initializeEnvironment()
            method that accepts date and time arguments to ensure that the date and
            time has been initialized.
  
            \note The client program should try it's hardest to catch all bad input data
            and corrected or duplicated records \b before calling this method.
  
            \param[in] year     Observation year (4 digits).
            \param[in] month    Observation month (Jan==1, Dec==12).
            \param[in] day      Observation day-of-the-month [1..31].
            \param[in] hour     Observation elapsed hours in the day [0..23].
            \param[in] minute   Observation elapsed minutes in the hour (0..59].
            \param[in] second   Observation elapsed seconds in the minute [0..59].
            \param[in] at   Current observation's ambient air temperature (oC).
            \param[in] rh   Current observation's ambient air relative humidity (g/g).
            \param[in] sW   Current observation's solar radiation (W/m2).
            \param[in] rcum Current observation's total cumulative rainfall amount (cm).
            \param[in] bpr  Current observation's stick barometric pressure (cal/cm3).
  
            \retval TRUE if all inputs are ok and the stick is updated.
            \retval FALSE if inputs are out of range and the stick is \b not updated.
         */

        public bool update(
                int year,
                int month,
                int day,
                int hour,
                int minute,
                int second,
                double at,
                double rh,
                double sW,
                double rcum,
                double bpr = 0.0218
            )
        {
            // Determine Julian date for this new observation
            double jd0 = m_semTime.julianDate();
            m_semTime.set(year, month, day, hour, minute, second);
            double jd1 = m_semTime.julianDate();

            // Determine elapsed time (h) between the current and previous dates
            double et = 24.0 * (jd1 - jd0);

            // If the Julian date wasn't initialized,
            // or if the new time is less than the old time,
            // assume a 1-h elapsed time.
            //if ( jd1 < jd0 )
            //{
            //    et = 1.0;
            //}
        /*# ifdef DEBUG
            fprintf(stderr, "%d/%02d/%02d %02d:%02d:%02d\n",
            year, month, day, hour, minute, second);
        #endif*/
            // Update!


            return (update(et, at, rh, sW, rcum, bpr));
        }

        //------------------------------------------------------------------------------
        /*! \brief Updates a dead moisture stick's internal and external environment
            based on the passed (current) weather observation values.
  
            This overloaded version accepts the elapsed time since the previous
            observation, and does not automatically update the Julian date.
  
            \param[in] et   Elapsed time since the previous observation (h).
            \param[in] at   Current observation's ambient air temperature (oC).
            \param[in] rh   Current observation's ambient air relative humidity (g/g).
            \param[in] sW   Current observation's solar radiation (W/m2).
            \param[in] rcum Current observation's total cumulative rainfall amount (cm).
            \param[in] bpr  Current observation's stick barometric pressure (cal/cm3).
  
            \note The client program should try it's hardest to catch all bad input data
            and corrected or duplicated records \b before calling this method.
  
            \retval TRUE if all inputs are ok and the stick is updated.
            \retval FALSE if inputs are out of range and the stick is \b not updated.
         */

        bool update(
                double et,
                double at,
                double rh,
                double sW,
                double rcum,
                double bpr = 0.0218
            )
        {
            // Increment update counter
            m_updates++;
            m_elapsed += et;

            //--------------------------------------------------------------------------
            // Catch bad data here
            // The client program should try to catch all bad data and corrected records
            // before calling this function.
            //--------------------------------------------------------------------------

            // If the elapsed time < 0.00027 hours (<0.01 sec), then treat this as
            // a duplicate or corrected observation and return
            if (et < 0.0000027)
            {
                /*ostringstream str;
                str << "DeadFuelMoisture::update() "
                    << m_updates
                    << " has a regressive elapsed time of "
                    << et
                    << " hours.";
                cerr << str.str() << "\n";*/
                // Msg::Instance().userWarning( str.str() );
                return (false);
            }
            // Cumulative rainfall must equal or exceed its previous value
            if (rcum < m_rc1)
            {
                /*ostringstream str;
                str << "DeadFuelMoisture::update() "
                    << m_updates
                    << " has a regressive cumulative rainfall amount of "
                    << rcum
                    << " cm.";
                cerr << str.str() << "\n";*/
                //Msg::Instance().userWarning( str.str() );
                // Assume a RAWS station reset and return
                m_rc1 = rcum;
                m_ra0 = 0.0;
                return (false);
            }
            // Relative humidity must be reasonable
            if (rh < 0.001 || rh > 1.0)
            {
                /*ostringstream str;
                str << "DeadFuelMoisture::update() "
                    << m_updates
                    << " has a an out-of-range relative humidity of  "
                    << rh
                    << " g/g.";
                cerr << str.str() << "\n";*/
                //Msg::Instance().userWarning( str.str() );
                return (false);
            }
            // Ambient temperature must be reasonable
            if (at < -60.0 || at > 60.0 )
            {
                /*ostringstream str;
                str << "DeadFuelMoisture::update() "
                    << m_updates
                    << " has a an out-of-range air temperature of  "
                    << at
                    << " oC.";
                cerr << str.str() << "\n";*/
                //Msg::Instance().userWarning( str.str() );
                return (false);
            }
            // Insolation must be reasonable
            sW = (sW < 0.0) ? 0.0 : sW;
            if (sW > 2000.0 )
            {
                /*ostringstream str;
                str << "DeadFuelMoisture::update() "
                    << m_updates
                    << " has a an out-of-range solar insolation of  "
                    << sW
                    << " W/m2.";
                cerr << str.str() << "\n";*/
                //Msg::Instance().userWarning( str.str() );
                return (false);
            }

            // First save the previous weather observation values 
            m_ta0 = m_ta1;      // Previous air temperature (oC) 
            m_ha0 = m_ha1;      // Previous air relative humidity (g/g) 
            m_sv0 = m_sv1;      // Previous pyranometer voltage (millivolts) 
            m_rc0 = m_rc1;      // Previous cumulative rainfall (cm) 
            m_ra0 = m_ra1;      // Previous period's rainfall amount (cm) 
            m_bp0 = m_bp1;      // Previous barometric pressure (cal/m3)

            // Then save the current weather observation values 
            m_ta1 = at;         // Current air temperature (oC)
            m_ha1 = rh;         // Current air relative humidity (g/g)
            m_sv1 = sW / Smv;   // Current pyranometer voltage (millivolts)
            m_rc1 = rcum;       // Current cumulative rainfall (cm)
            m_bp1 = bpr;        // Current barometric pressure (cal/m3)
            m_et = et;         // Current elapsed time since previous update (h)

            // Precipitation amount since last observation
            m_ra1 = m_rc1 - m_rc0;
            // If no precipitation, reset the precipitation duration timer
            m_rdur = (m_ra1 < 0.0001) ? 0.0 : m_rdur;
            // Precipitation rate since last observation adjusted by Pi (cm/h)
            m_pptrate = m_ra1 / et / Pi;
            // Determine moisture computation time step interval (h)
            m_mdt = et / (double)m_mSteps;
            m_mdt_2 = m_mdt * 2.0;
            // Nelson's "s" factor used in update() loop
            m_sf = 3600.0 * m_mdt / (m_dx_2 * m_density);
            // Determine bound water diffusivity time step interval (h)
            m_ddt = et / (double)m_dSteps;
            // First hour runoff factor h-(g/(g-h))
            double rai0 = m_mdt * m_rai0 * (1.0 - exp(-100.0 * m_pptrate));
            // Adjustment for rainfall cases when humidity is dropping

            if (m_ha1 < m_ha0)
            {
                if (m_rampRai0)
                {
                    rai0 *= (1.0 - ((m_ha0 - m_ha1) / m_ha0));
                }
                else
                {
                    rai0 *= 0.15;
                }
            }
            // Subsequent runoff factor h-(g/(g/h))
            double rai1 = m_mdt * m_rai1 * m_pptrate;

            // DFM state counter
            int[] tstate = new int[DFM_States];
            for (int i = 0; i < DFM_States; i++)
            {
                tstate[i] = 0;
            }
            // Next time (tt) to run diffusivity computations.
            double ddtNext = m_ddt;
            // Elapsed moisture computation time (h)
            double tt = m_mdt;

            //changed SB, 10/2009, move internal loop variable declarations outside of loop
            double tfract, ta, ha, sv, bp, fsc, tka, tdw, tdp, tsk, hr, sr, psa, pa, psd, tfd, qv, qw, hw, tkf, gnu, c1, c2, wdiff, ps1, p1, hf_log, bi, aml, s_new, w_new, w_old, svp, ak, ae, aw, ar, ap;
            bool continuousLiquid;
            // Loop for each moisture time step between environmental inputs.

            if (m_name.Length > 0)
            {
                if (m_name.StartsWith("1"))
                {
                    int nada = 0;
                }                   
            }               

            for (int nstep = 1; tt <= et; tt = (double)(nstep) * m_mdt, nstep++)
            {
                // Fraction of time elapsed between previous and current obs (dl)
                tfract = tt / et;
                // Air temperature interpolated between previous and current obs (oC)
                ta = m_ta0 + (m_ta1 - m_ta0) * tfract;
                // Air humidity interpolated between previous and current obs (dl)
                ha = m_ha0 + (m_ha1 - m_ha0) * tfract;
                // Solar radiation interpolated between previous and current obs (millivolts)
                sv = m_sv0 + (m_sv1 - m_sv0) * tfract;
                // Barometric pressure interpolated between previous and current obs (bal/m3)
                bp = m_bp0 + (m_bp1 - m_bp0) * tfract;
                // Fraction of the solar constant interpolated between obs (mv)
                //double fsc = 0.07 * sv;
                fsc = sv / Srf;
                // Ambient air temperature (oK)
                tka = ta + Kelvin;
                // Dew point temperature (oK)
                tdw = 5205.0 / ((5205.0 / tka) - log(ha));
                // Dew point temperature (oC)
                tdp = tdw - Kelvin;
                // Sky temperature (oK)
                tsk = (fsc < 0.000001) ? Tcn + Kelvin : Tcd + Kelvin;
                // Long wave radiative surface heat transfer coefficient (cal/cm2-h-C)
                hr = (fsc < 0.000001) ? Hrn : Hrd;
                // Solar radiation received by half the stick (cal/cm2-h)
                sr = (fsc < 0.000001) ? 0.0 : Srf * fsc;
                // Water saturation vapor pressure in ambient air (cal/cm3)
                psa = 0.0000239 * exp(20.58 - (5205.0 / tka));
                // Water saturation vapor pressure in air (cal/cm3)
                pa = ha * psa;
                // Water saturation vapor pressure at dewpoint (cal/cm3)
                psd = 0.0000239 * exp(20.58 - (5205.0 / tdw));
                // Rainfall duration (h)
                m_rdur = (m_ra1 > 0.0001) ? (m_rdur + m_mdt) : 0.0;

                //----------------------------------------------------------------------
                // Stick surface temperature and humidity
                //----------------------------------------------------------------------

                // Intermediate stick surface temperature (oC)
                tfd = ta + (sr - hr * (ta - tsk + Kelvin)) / (hr + m_hc);
                // Latent heat of vaporization of water (cal/mole)
                qv = 13550.0 - 10.22 * (tfd + Kelvin);
                // Differential heat of sorption of water (cal/mole)
                qw = 5040.0 * exp(-14.0 * m_w[0]);
                // Stick heat transfer coefficient for vapor diffusion above FSP
                hw = (m_hwf * Ap / 0.24) * qv / 18.0;
                // Stick surface temperature (oC)
                m_t[0] = tfd - (hw * (tfd - ta) / (hr + m_hc + hw));
                // Stick surface temperature (oK)
                tkf = m_t[0] + Kelvin;
                // Kinematic viscosity of liquid water (cm2/s)
                gnu = 0.00439 + 0.00000177 * pow((338.76 - tkf), 2.1237);

                // EMC sorption isotherm parameter (g/g)
                c1 = 0.1617 - 0.001419 * m_t[0];
                // EMC sorption isotherm parameter (g/g)
                c2 = 0.4657 + 0.003578 * m_t[0];
                // Stick fiber saturation point (g/g)
                m_wsa = c1 * pow(Wsf, c2);
                // Maximum minus current fiber saturation (g/g)
                wdiff = m_wmax - m_wsa;
                wdiff = (wdiff < 0.000001) ? 0.000001 : wdiff;
                // Water saturation vapor pressure at surface temp (cal/cm3)
                ps1 = 0.0000239 * exp(20.58 - (5205.0 / tkf));
                // Water vapor pressure at the stick surface (cal/cm3)
                p1 = pa + Ap * bp * (qv / (qv + qw)) * (tka - tkf);
                p1 = (p1 < 0.000001) ? 0.000001 : p1;
                // Stick surface humidity (g/g)
                m_hf = p1 / ps1;
                m_hf = (m_hf > Hfs) ? Hfs : m_hf;
                // Stick equilibrium moisture content (g/g).0 */
                hf_log = -log(1.0 - m_hf);
                m_sem = c1 * pow(hf_log, c2);

                //----------------------------------------------------------------------
                // Stick surface moisture content
                //----------------------------------------------------------------------

                // Initialize state for this m_mdt
                m_state = DFM_State.DFM_State_None;
                // Start with no water film contribution
                m_wfilm = 0.0;
                // Factor related to rate of evaporation or condensation ((g/g)/h)
                aml = 0.0;
                // Mass transfer biot number (dl)
                bi = 0.0;
                // Previous and new value of m_w[0] (g/g) and m_s[0]
                s_new = m_s[0];
                w_new = m_w[0];
                w_old = m_w[0];

                //......1: If it is RAINING:
                if (m_ra1 > 0.0)
                {
                    //..........1a: If this is a RAINSTORM:
                    if (m_allowRainstorm && m_pptrate >= m_stv)
                    {
                        m_state = DFM_State.DFM_State_Rainstorm;
                        m_wfilm = m_wfilmk;
                        w_new = m_wmx;
                    }
                    //..........1b: Else this is RAINFALL:
                    else
                    {
                        if (m_rdur < 1.0 || !m_allowRainfall2)
                        {
                            m_state = DFM_State.DFM_State_Rainfall1;
                            w_new = w_old + rai0;
                        }
                        else
                        {
                            m_state = DFM_State.DFM_State_Rainfall2;
                            w_new = w_old + rai1;
                        }
                    }
                    m_wfilm = m_wfilmk;
                    s_new = (w_new - m_wsa) / wdiff;
                    m_t[0] = tfd;
                    m_hf = Hfs;
                }
                //......2: Else it is not raining:
                else
                {
                    //.........2a: If moisture content exceeds the fiber saturation point:
                    if (w_old > m_wsa)
                    {
                        p1 = ps1;
                        m_hf = Hfs;

                        // Factor related to evaporation or condensation rate ((g/g)/h)
                        aml = m_amlf * (ps1 - psd) / bp;
                        if (m_t[0] <= tdp && p1 > psd)
                        {
                            aml = 0.0;
                        }
                        w_new = w_old - aml * m_mdt_2;
                        if (aml > 0.0 )
                        {
                            w_new -= (m_mdt * m_capf / gnu);
                        }
                        w_new = (w_new > m_wmx) ? m_wmx : w_new;
                        s_new = (w_new - m_wsa) / wdiff;

                        //..............2a1: if moisture content is rising: CONDENSATION
                        if (w_new > w_old)
                        {
                            m_state = DFM_State.DFM_State_Condensation1;
                        }
                        //..............2a2: else if moisture content is steady: STAGNATION
                        else if (w_new == w_old)
                        {
                            m_state = DFM_State.DFM_State_Stagnation;
                        }
                        //..............2a3: else if moisture content is falling: EVAPORATION
                        else if (w_new < w_old)
                        {
                            m_state = DFM_State.DFM_State_Evaporation;
                        }
                    }
                    //..........2b: else if fuel temperature is less than dewpoint: CONDENSATION
                    else if (m_t[0] <= tdp)
                    {
                        m_state = DFM_State.DFM_State_Condensation2;
                        // Factor related to evaporation or condensation rate ((g/g)/h)
                        aml = (p1 > psd) ? 0.0 : m_amlf * (p1 - psd) / bp;
                        w_new = w_old - aml * m_mdt_2;
                        s_new = (w_new - m_wsa) / wdiff;
                    }
                    //..........2c: else surface moisture content less than fiber saturation point
                    //              and stick temperature greater than dewpoint ...
                    else
                    {
                        //..............2c1: if surface moisture greater than equilibrium: DESORPTION
                        if (w_old >= m_sem)
                        {
                            m_state = DFM_State.DFM_State_Desorption;
                            bi = m_stcd * m_dx / m_d[0];
                        }
                        //..............2c2: else surface moisture less than equilibrium: ADSORPTION
                        else
                        {
                            m_state = DFM_State.DFM_State_Adsorption;
                            bi = m_stca * m_dx / m_d[0];
                        }
                        w_new = (m_w[1] + bi * m_sem) / (1.0 + bi);
                        s_new = 0.0;
                    }
                }   // end of not raining

                // Store the new surface moisture and saturation
                m_w[0] = (w_new > m_wmx) ? m_wmx : w_new;
                m_s[0] = (s_new < 0.0 ) ? 0.0 : s_new;
                tstate[(int)m_state]++;

        /*# ifdef DEBUG
                fprintf(stdout,
                "%03d: ta=%7.4f ha=%6.4f sv=%6.2f rc=%f wold=%f rai0=%f rai1=%f state=%s t0=%f w0=%f\n",
                nstep, ta, ha, sv, m_rc1, w_old, rai0, rai1, stateName(), m_t[0], m_w[0]);
        #endif*/
                //----------------------------------------------------------------------
                // Compute interior nodal moisture content values.
                //----------------------------------------------------------------------

                /* Declaration of vectors moved outside of loop for performance reasons
                SB 1/6/2007
                        // Nodal moisture contents at the previous m_mdt (g/g)
                        vector<double> wold( m_nodes );
                        // Nodal temperatures at the previous m_mdt (oC)
                        vector<double> told( m_nodes );
                        // Nodal fiber saturation points at the previous m_mdt (g/g)
                        vector<double> sold( m_nodes );
                        // Used to redistribute fuel temperature
                        vector<double> v( m_nodes );
                        // Used to redistribute moisture content
                        vector<double> o( m_nodes );
                        // Free water transport coefficient (cm2/h)
                        vector<double> g( m_nodes );
                */
                for (int i = 0; i < m_nodes; i++)
                {
                    m_Twold[i] = m_w[i];
                    m_Tsold[i] = m_s[i];
                    m_Ttold[i] = m_t[i];
                    m_Tv[i] = Thdiff * m_x[i];
                    m_To[i] = m_d[i] * m_x[i];
                }

                // Propagate the moisture content changes
                if (m_state != DFM_State.DFM_State_Stagnation)
                {
                    for (int i = 0; i < m_nodes; i++)
                    {
                        m_Tg[i] = 0.0;
                        svp = (m_w[i] - m_wsa) / wdiff;
                        if (svp >= Sir && svp <= Scr)
                        {
                            // Permeability of stick when nonsaturated (cm2)
                            ak = Aks * (2.0 * sqrt(svp / Scr) - 1.0 );

                            // Free water transport coefficient (cm2/h)
                            m_Tg[i] = (ak / (gnu * wdiff))
                                 * m_x[i] * m_vf
                                 * pow((Scr / svp), 1.5);
                        }
                    }

                    // Propagate the fiber saturation moisture content changes
                    for (int i = 1; i < m_nodes - 1; i++)
                    {
                        ae = m_Tg[i + 1] / m_dx;
                        aw = m_Tg[i - 1] / m_dx;
                        ar = m_x[i] * m_dx / m_mdt;
                        ap = ae + aw + ar;
                        m_s[i] = (ae * m_Tsold[i + 1] + aw * m_Tsold[i - 1] + ar * m_Tsold[i]) / ap;
                        /*if ( m_randseed )
                        {
                            double rn = uniformRandom( -.0001, 0.0001 );
                            m_s[i] += rn;
                        }*/
                        m_s[i] = (m_s[i] > 1.0 ) ? 1.0 : m_s[i];
                        m_s[i] = (m_s[i] < 0.0 ) ? 0.0 : m_s[i];
                    }
                    m_s[m_nodes - 1] = m_s[m_nodes - 2];

                    // Check if m_s[] is less than Sir (limit of continuous liquid
                    // columns) at ANY stick node.
                    continuousLiquid = true;
                    for (int i = 1; i < m_nodes - 1; i++)
                    {
                        if (m_s[i] < Sir)
                        {
                            continuousLiquid = false;
                            break;
                        }
                    }

                    // If all nodes have continuous liquid columns (s >= Sir) ...
                    // This never happens for the 1-h or 10-h test data!
                    if (continuousLiquid && false)
                    {
                        for (int i = 1; i < m_nodes - 1; i++)
                        {
                            m_w[i] = m_wsa + m_s[i] * wdiff;
                            if (m_pertubateColumn)
                            {
                                double rn = uniformRandom(-.0001, 0.0001);
                                m_w[i] += rn;
                            }
                            m_w[i] = (m_w[i] > m_wmx) ? m_wmx : m_w[i];
                            m_w[i] = (m_w[i] < 0.0) ? 0.0 : m_w[i];
                        }
                    }
                    // ...0 else at least one node has s < Sir.
                    else
                    {
                        // Propagate the moisture content changes
                        for (int i = 1; i < m_nodes - 1; i++)
                        {
                            ae = m_To[i + 1] / m_dx;
                            aw = m_To[i - 1] / m_dx;
                            ar = m_x[i] * m_dx / m_mdt;
                            ap = ae + aw + ar;
                            m_w[i] = (ae * m_Twold[i + 1] + aw * m_Twold[i - 1] + ar * m_Twold[i])
                                   / ap;
                            /*if ( m_randseed )
                            {
                                double rn = uniformRandom( -.0001, 0.0001 );
                                m_w[i] += rn;
                            }*/
                            m_w[i] = (m_w[i] > m_wmx) ? m_wmx : m_w[i];
                            m_w[i] = (m_w[i] < 0.0) ? 0.0 : m_w[i];
                        }
                    }
                    m_w[m_nodes - 1] = m_w[m_nodes - 2];
                }

                // Propagate the fuel temperature changes
                for (int i = 1; i < m_nodes - 1; i++)
                {
                    ae = m_Tv[i + 1] / m_dx;
                    aw = m_Tv[i - 1] / m_dx;
                    ar = m_x[i] * m_dx / m_mdt;
                    ap = ae + aw + ar;
                    m_t[i] = (ae * m_Ttold[i + 1] + aw * m_Ttold[i - 1] + ar * m_Ttold[i]) / ap;
                    /* if ( m_randseed )
                     {
                         double rn = uniformRandom( -.0001, 0.0001 );
                         m_t[i] += rn;
                     }*/
                    m_t[i] = (m_t[i] > 71.0 ) ? 71.0 : m_t[i];
                }
                m_t[m_nodes - 1] = m_t[m_nodes - 2];

                // Update the moisture diffusivity if within less than half a time step
                if ((ddtNext - tt) < (0.5 * m_mdt))
                {
                    diffusivity(bp);
                    ddtNext += m_ddt;
                }
            }   // Next moisture time step

            // Store prevailing state
            m_state = DFM_State.DFM_State_None;
            int max = tstate[0];
            for (int i = 1; i < DFM_States; i++)
            {
                if (tstate[i] > max)
                {
                    m_state = (DFM_State)i;
                    max = tstate[i];
                }
            }
            return (true);
        }

        //------------------------------------------------------------------------------
        /*! \brief Access to the current number of observation updates.
  
            \return The current number of observation updates.
         */

        long updates()
        {
            return( m_updates );
        }

        //------------------------------------------------------------------------------
        /*! \brief Access to the stick's current water film contribution to the
            moisture content.
  
            \return Stick's current water film contribution to the moisture content
            (g/g).
         */

        double waterFilmContribution()
        {
            return( m_wfilmk );
        }

        //------------------------------------------------------------------------------
        /*! \brief Sets everything to zero.
         */

        void zero()
        {
            m_semTime.set(0, 0, 0, 0, 0, 0, 0);
            m_density = 0.0;
            m_dSteps = 0;
            m_hc = 0.0;
            m_length = 0.0;
            m_name = "";
            m_nodes = 0;
            m_radius = 0.0;
            m_rai0 = 0.0;
            m_rai1 = 0.0;
            m_stca = 0.0;
            m_stcd = 0.0;
            m_mSteps = 0;
            m_stv = 0.0;
            m_wfilmk = 0.0;
            m_wmx = 0.0;
            m_dx = 0.0;
            m_wmax = 0.0;
            m_x.Clear();
            m_v.Clear();
            m_amlf = 0.0;
            m_capf = 0.0;
            m_hwf = 0.0;
            m_dx_2 = 0.0;
            m_vf = 0.0;
            m_bp0 = 0.0;
            m_ha0 = 0.0;
            m_rc0 = 0.0;
            m_sv0 = 0.0;
            m_ta0 = 0.0;
            m_init = false;
            m_bp1 = 0.0;
            m_et = 0.0;
            m_ha1 = 0.0;
            m_rc1 = 0.0;
            m_sv1 = 0.0;
            m_ta1 = 0.0;
            m_ddt = 0.0;
            m_mdt = 0.0;
            m_mdt_2 = 0.0;
            m_pptrate = 0.0;
            m_ra0 = 0.0;
            m_ra1 = 0.0;
            m_rdur = 0.0;
            m_sf = 0.0;
            m_hf = 0.0;
            m_wsa = 0.0;
            m_sem = 0.0;
            m_wfilm = 0.0;
            m_elapsed = 0.0;
            m_t.Clear();
            m_s.Clear();
            m_d.Clear();
            m_w.Clear();
            m_updates = 0;
            m_state = DFM_State.DFM_State_None;
            m_randseed = 0;
            return;
        }
    }    
}

