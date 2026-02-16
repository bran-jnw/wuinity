using PREACT.Math;

namespace PREACT.Wildfire.AFDRS
{
    public class Pine
    {
        private const double HEAT_CONTENT = 18600; //KJ/kg
        private const double KGSQM_TO_TPH = 10; //kg/m2 to t/ha
        private const double SECONDS_PER_HOUR = 3600; //s
        private const double KGM2_PER_LBFT2 = 4.88243; //kg/m2 per lb/ft2
        private const double KJKG_PER_BTULB = 2.326; //kg/kJ per Btu/lb
        private const double MSEC_PER_FTMIN = 0.00508; //m/s per ft/min

        double _fmc, _ros, _intensity, _flameHeight; //output

        public Pine()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="temp"></param>
        /// <param name="rh"></param>
        /// <param name="U_10"></param>
        /// <param name="DF">Drought factor</param>
        /// <param name="KBDI">Keetch Byram drought index KBDI</param>
        public void Calculate(double temp, double rh, double U_10, double DF, double KBDI)
        {
            _fmc = FMC_pine(temp, rh);
            double[] fhp = fb_pine_ensemble(U_10, _fmc, DF, KBDI); //TODO:correct call?
            _ros = fhp[0];
            _intensity = fhp[1];
            _flameHeight = fhp[2];
        }

        /// returns the grass fuel moisture content (%) based on McArthur (1966)
        ///   temp: air temperature (C)
        ///   rh: relative humidity (%)
        private static double FMC_pine(double temp, double rh)
        {
            return 4.3426 + 0.1188 * rh - 0.0211 * temp;
        }

        /// returns fuel availability estimates using drought factor
        /// From Cruz et al. (2022) Vesta Mk 2 model
        ///
        /// args
        ///   DF: drought factor
        ///   DI: Drought IndexKeetch Byram drought index KBDI
        ///   WAF: wind adjustment factor restricted to range 3 to 5
        private static double FA_pine(double DF, double DI, double waf)
        {
            double C1 = 0.1 * ((0.0046 * Mathd.Pow(waf, 2) - 0.0079 * waf - 0.0175) * DI + (-0.9167 * Mathd.Pow(waf, 2) + 1.5833 * waf + 13.5));
            C1 = Mathd.Max(C1, 0);
            C1 = Mathd.Min(C1, 1);

            return 1.008 / (1 + 104.9 * Mathd.Exp(-0.9306 * C1 * DF));
        }

        /// returns wind speed at flame height (km/h) based on Cruz et al. 2006
        ///
        /// args
        ///   U_10: 10 m wind speed (km/h)
        ///   h_o: stand (overstorey) height m
        private static double U_flame_height(double U_10, double h_o)
        {
            //wind speed at stand height
            double U_stand_height = U_10 * Mathd.Log((0.36 * h_o) / (0.13 * h_o)) / Mathd.Log((10 + 0.36 * h_o) / (0.13 * h_o));
            return U_stand_height * Mathd.Exp(-0.48);
        }

        static readonly double[] _fuel_models_default = { 5, 10.5, 11, 5, 0.1 };
        /// returns array of the the forward rate of spread m/h, intensity kW/m and flame height m for pine based on Cruz model
        ///
        /// args
        ///   U_10: 10 m wind speed (km/h)
        ///   mc: dead fuel moisture content %
        ///   DF: drought factor
        ///   KBDI: Keetch Byram drought index KBDI
        ///   fuel_models array comprising
        ///     wrf: wind adjustment factor restricted to range 3 to 5
        ///     fl_s: surface fuel load (t/ha)
        ///     fl_o: overstorey (canopy) fuel load (t/ha)
        ///     bh_o: overstorey (canopy) base height m
        ///     bd_o: overstorey (canopy) bulk density
        private static double[] fire_behaviour_pine(double U_10, double mc, double DF, double KBDI, double[] fuel_models)
        {
            //fuel parameters
            //fuel_models array is empty or imcomplete, use defaults
            if (fuel_models.Length < 5)
            {
                fuel_models = _fuel_models_default;
            }

            double wrf = fuel_models[0];
            double fl_s = fuel_models[1];
            double fl_o = fuel_models[2];
            double bh_o = fuel_models[3];
            double bd_o = fuel_models[4];

            //Model parameters
            const double moisture_fraction_extinction = 0.3;  //Moisture content of extinction, mass water / mass ovendry wood
            const double mineral_content_silica_free = 0.01;  //fuel particle effective mineral content, mass silica-free minerals / mass ovendry wood
            const double mineral_content_total = 0.0555; //fuel particle total mineral content, mass minerals / mass ovendry wood
            const double surface_volume_ratio = 1700; //surface area to volume ratio, 1/ft
            const double particle_density = 32; //ovendry particle density, lb/ft^3
            const double heat_of_combustion_IMP = 8000; //Btu/lb
            const double heat_of_combustion_SI = heat_of_combustion_IMP * KJKG_PER_BTULB; //kJ/kg
            const double critical_mass_flow_rate = 3; // //Critical mass flow rate for solid crown flame, estimated as 3 kg/m^2/min
            const double fuel_depth = 1.148; //ft
            const double stand_height = 15;  //m

            //change mc to fraction
            double moisture_fraction = mc / 100;

            //adjust units
            double fuel_load_SI = (fl_s / KGSQM_TO_TPH) * FA_pine(DF, KBDI, (wrf));
            double fuel_load_IMP = fuel_load_SI / KGM2_PER_LBFT2; // convert to imperial kg/m2 per lb/ft2

            //foliar moisture content
            double foliar_moisture_content = 150 - 5 * DF;

            double wind_mid_flame = U_flame_height(U_10, (stand_height));


            double bulk_density = fuel_load_IMP / fuel_depth;
            double packing_ratio = bulk_density / particle_density;
            double heat_of_preignition = 250 + 1116 * moisture_fraction; //Btu/lb
            double effective_heating_number = Mathd.Exp(-138 / surface_volume_ratio);

            double net_fuel_load_IMP = fuel_load_IMP / (1 + mineral_content_total);

            double e = 0.715 * Mathd.Exp(-0.000359 * surface_volume_ratio);
            double b = 0.02562 * Mathd.Pow(surface_volume_ratio, 0.54);
            double c = 7.47 * Mathd.Exp(-0.133 * Mathd.Pow(surface_volume_ratio, 0.55));
            double packing_ratio_op = 3.348 * Mathd.Pow(surface_volume_ratio, -0.8189); //Optimum packing ratio
            double wind_coefficient = c * Mathd.Pow(wind_mid_flame * 54.68, b) * Mathd.Pow(packing_ratio / packing_ratio_op, -e);

            double xi = Mathd.Pow(192 + 0.2595 * surface_volume_ratio, -1) * Mathd.Exp((0.792 * 0.681 * Mathd.Pow(surface_volume_ratio, 0.5)) * (packing_ratio + 0.1)); //Propagating flux ratio

            double eta_S = 0.174 * Mathd.Pow(mineral_content_silica_free, -0.19); //Mineral damping coefficient
            double eta_M = 1 - 2.59 * moisture_fraction / moisture_fraction_extinction + 5.11 * Mathd.Pow(moisture_fraction / moisture_fraction_extinction, 2) - 3.52 * Mathd.Pow(moisture_fraction / moisture_fraction_extinction, 3); //Moisture damping coefficient

            double a = 1 / (4.77 * Mathd.Pow(surface_volume_ratio, 0.1) - 7.27);
            double gamma_max = Mathd.Pow(surface_volume_ratio, 1.5) / (495 + 0.0594 * Mathd.Pow(surface_volume_ratio, 1.5)); //Maximum reaction velocity
            double Gamma = gamma_max * Mathd.Pow((packing_ratio / packing_ratio_op), a) * Mathd.Exp(a * (1 - packing_ratio / packing_ratio_op)); //Optimum reaction velocity

            double reaction_intensity = Gamma * net_fuel_load_IMP * heat_of_combustion_IMP * eta_M * eta_S; //Btu/ft^2 min

            double speed_surface = reaction_intensity * xi * (1 + wind_coefficient) / (bulk_density * effective_heating_number * heat_of_preignition);  //Surface rate of spread, ft/min
            speed_surface = speed_surface * MSEC_PER_FTMIN; //Convert to m/s

            //Using Byram (1959) to calculate surface fire intensity
            double intensity_ = heat_of_combustion_SI * fuel_load_SI * speed_surface; // Fire intensity, kW/m

            //Using Van Wagner (1977) for the crowning criteria threshold
            double heat_of_ignition = 460 + 26 * foliar_moisture_content; //Heat of ignition, kJ/kg
            double crowning_intensity = Mathd.Pow(0.01 * bh_o * heat_of_ignition, 1.5); //Crowning threshold intensity, kW/m
            double crowning_ratio = intensity_ / crowning_intensity; //If this is greater than 1 then crowning is predicted and vice versa
            double speed_active_MMIN = 11.021 * Mathd.Pow(U_10, 0.8966) * Mathd.Pow(bd_o, 0.1901) * Mathd.Exp(-0.1714 * moisture_fraction * 100); //Active crown fire spread rate, m/min.
            double speed_active_MS = speed_active_MMIN / 60; //Converting to m/s

            //Calculate criteria for active crowning from Cruz (2008)
            double CAC = speed_active_MMIN / (critical_mass_flow_rate / bd_o); //Criteria for active crowning.
            double speed_passive = speed_active_MS * Mathd.Exp(-1 * CAC); //Passive ROS

            //passive = ((crowning_ratio > 1) & (CAC < 1))
            bool passive = crowning_ratio > 1 && CAC < 1;
            bool Active = crowning_ratio > 1 && CAC >= 1;
            bool surface = crowning_ratio <= 1;

            double ROS;
            if(surface)
            {
                ROS = speed_surface;
            }
            else if(passive)
            {
                ROS = Mathd.Max(speed_passive, speed_surface);
            }
            else
            {
                ROS = speed_active_MS;
            }

            double fuel_load = fuel_load_SI * 10; //convert back to t/ha
            if (Active || passive)
            {
                fuel_load = fuel_load + fl_o;
            }

            //convert to m/h
            ROS = ROS * 3600;
            double Intensity_total = intensity(ROS, fuel_load);
            double flame_height = 0.07755 * Mathd.Pow(Intensity_total, 0.46);

            if(Active)
            {
                flame_height = flame_height + stand_height;
            }

            double[] fire_behaviour_array = new double[3];
            fire_behaviour_array[0] = ROS;
            fire_behaviour_array[1] = Intensity_total;
            fire_behaviour_array[2] = flame_height;

            return fire_behaviour_array;
        }

        /// returns the fireline intensity (kW/m) based on Byram 1959
        ///   ROS: forward rate of spread (km/h)
        ///   fuel_load: fine fuel load (t/ha)
        public static double intensity(double ROS, double fuel_load)
        {
            // convert units
            ROS = ROS / 3600; // m/s
            fuel_load = fuel_load / 10; //kg/m^2

            return 18608 * ROS * fuel_load;
        }


        /*private static double ROS_pine(double U_10, double mc, double DF, double KBDI)
        {
            double[] FB_pine = fire_behaviour_pine(U_10, mc, DF, KBDI, fuel_models_default); //TODO:check added deault
            return FB_pine[0];
        }

        private static double Intensity_pine(U_10, mc, DF, KBDI) As Single
            Dim FB_pine() As Single
            FB_pine = fire_behaviour_pine(U_10, mc, DF, KBDI)
            Intensity_pine = FB_pine(1)
        End Function

        private static double FH_pine(U_10, mc, DF, KBDI) As Single
            Dim FB_pine() As Single
            FB_pine = fire_behaviour_pine(U_10, mc, DF, KBDI)
            FH_pine = FB_pine(2)
        }*/

        // fuel array elements: proportion, fl_s, fl_o, bh_o, bd_o
        static readonly double[] fuelArray1 = { 0.151, 4, 11.5, 0.7, 0.17 };
        static readonly double[] fuelArray2 = { 0.151, 5, 12, 1.5, 0.18 };
        static readonly double[] fuelArray3 = { 0.121, 8.5, 12, 2.5, 0.18 };
        static readonly double[] fuelArray4 = { 0.091, 10, 8, 6, 0.12 };
        static readonly double[] fuelArray5 = { 0.394, 7, 10, 14, 0.15 };
        static readonly double[][] fuel_arrays = { fuelArray1, fuelArray2, fuelArray3, fuelArray4, fuelArray5 };
        /// returns array of the the forward rate of spread (m/h), intensity (kW/m) and flame height (m) for pine using an mixed stand ensemble
        ///
        /// args
        ///   U_10: 10 m wind speed (km/h)
        ///   mc: dead fuel moisture content %
        ///   DF: drought factor
        ///   KBDI: Keetch Byram drought index KBDI
        private static double[] fb_pine_ensemble(double U_10, double mc, double DF, double KBDI)
        {
            //initialise
            double grass_proportion = 0.091;
            double wrf = 5;
            double ROS = Grassland.ROS_grass(U_10, (mc), 100, Grassland.States.EatenOut);
            double Intensity_total = intensity(ROS, 1.5) * grass_proportion;
            double flame_height = Grassland.Flame_height_grass((ROS), Grassland.States.EatenOut) * grass_proportion;
            ROS = ROS * grass_proportion;

            foreach (double[] fuel_model in fuel_arrays)
            {
                double proportion = fuel_model[0];
                double fl_s = fuel_model[1];
                double fl_o = fuel_model[2];
                double bh_o = fuel_model[3];
                double bd_o = fuel_model[4];

                double[] input = { wrf, fl_s, fl_o, bh_o, bd_o };
                
                double[] fb_array = fire_behaviour_pine(U_10, mc, DF, KBDI, input);

                ROS = ROS + proportion * fb_array[0];
                Intensity_total = Intensity_total + proportion * fb_array[1];
                flame_height = flame_height + proportion * fb_array[2];
            }

            double[] fire_behaviour_array = { ROS, Intensity_total, flame_height };

            return fire_behaviour_array;
            //fb_pine_ensemble = result_array
        }
    }
}
