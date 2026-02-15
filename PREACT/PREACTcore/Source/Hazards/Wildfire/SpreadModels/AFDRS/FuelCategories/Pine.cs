using PREACT.Math;

namespace PREACT.Wildfire.AFDRS
{
    public class Pine
    {
        private const float HEAT_CONTENT = 18600f; //KJ/kg
        private const float KGSQM_TO_TPH = 10f; //kg/m2 to t/ha
        private const float SECONDS_PER_HOUR = 3600f; //s
        private const float KGM2_PER_LBFT2 = 4.88243f; //kg/m2 per lb/ft2
        private const float KJKG_PER_BTULB = 2.326f; //kg/kJ per Btu/lb
        private const float MSEC_PER_FTMIN = 0.00508f; //m/s per ft/min

        float _fmc, _ros, _intensity, _flameHeight; //output

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
        public void Calculate(float temp, float rh, float U_10, float DF, float KBDI)
        {
            _fmc = FMC_pine(temp, rh);
            float[] fhp = fb_pine_ensemble(U_10, _fmc, DF, KBDI); //TODO:correct call?
            _ros = fhp[0];
            _intensity = fhp[1];
            _flameHeight = fhp[2];
        }

        /// returns the grass fuel moisture content (%) based on McArthur (1966)
        ///   temp: air temperature (C)
        ///   rh: relative humidity (%)
        private static float FMC_pine(float temp, float rh)
        {
            return 4.3426f + 0.1188f * rh - 0.0211f * temp;
        }

        /// returns fuel availability estimates using drought factor
        /// From Cruz et al. (2022) Vesta Mk 2 model
        ///
        /// args
        ///   DF: drought factor
        ///   DI: Drought IndexKeetch Byram drought index KBDI
        ///   WAF: wind adjustment factor restricted to range 3 to 5
        private static float FA_pine(float DF, float DI, float waf)
        {
            float C1 = 0.1f * ((0.0046f * Mathf.Pow(waf, 2f) - 0.0079f * waf - 0.0175f) * DI + (-0.9167f * Mathf.Pow(waf, 2f) + 1.5833f * waf + 13.5f));
            C1 = Mathf.Max(C1, 0);
            C1 = Mathf.Min(C1, 1);

            return 1.008f / (1f + 104.9f * Mathf.Exp(-0.9306f * C1 * DF));
        }

        /// returns wind speed at flame height (km/h) based on Cruz et al. 2006
        ///
        /// args
        ///   U_10: 10 m wind speed (km/h)
        ///   h_o: stand (overstorey) height m
        private static float U_flame_height(float U_10, float h_o)
        {
            //wind speed at stand height
            float U_stand_height = U_10 * Mathf.Log((0.36f * h_o) / (0.13f * h_o)) / Mathf.Log((10f + 0.36f * h_o) / (0.13f * h_o));
            return U_stand_height * Mathf.Exp(-0.48f);
        }

        static readonly float[] _fuel_models_default = { 5f, 10.5f, 11f, 5f, 0.1f };
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
        private static float[] fire_behaviour_pine(float U_10, float mc, float DF, float KBDI, float[] fuel_models)
        {
            //fuel parameters
            //fuel_models array is empty or imcomplete, use defaults
            if (fuel_models.Length < 5)
            {
                fuel_models = _fuel_models_default;
            }

            float wrf = fuel_models[0];
            float fl_s = fuel_models[1];
            float fl_o = fuel_models[2];
            float bh_o = fuel_models[3];
            float bd_o = fuel_models[4];

            //Model parameters
            const float moisture_fraction_extinction = 0.3f;  //Moisture content of extinction, mass water / mass ovendry wood
            const float mineral_content_silica_free = 0.01f;  //fuel particle effective mineral content, mass silica-free minerals / mass ovendry wood
            const float mineral_content_total = 0.0555f; //fuel particle total mineral content, mass minerals / mass ovendry wood
            const float surface_volume_ratio = 1700f; //surface area to volume ratio, 1/ft
            const float particle_density = 32f; //ovendry particle density, lb/ft^3
            const float heat_of_combustion_IMP = 8000f; //Btu/lb
            const float heat_of_combustion_SI = heat_of_combustion_IMP * KJKG_PER_BTULB; //kJ/kg
            const float critical_mass_flow_rate = 3f; // //Critical mass flow rate for solid crown flame, estimated as 3 kg/m^2/min
            const float fuel_depth = 1.148f; //ft
            const float stand_height = 15f;  //m

            //change mc to fraction
            float moisture_fraction = mc / 100f;

            //adjust units
            float fuel_load_SI = (fl_s / KGSQM_TO_TPH) * FA_pine(DF, KBDI, (wrf));
            float fuel_load_IMP = fuel_load_SI / KGM2_PER_LBFT2; // convert to imperial kg/m2 per lb/ft2

            //foliar moisture content
            float foliar_moisture_content = 150 - 5 * DF;

            float wind_mid_flame = U_flame_height(U_10, (stand_height));


            float bulk_density = fuel_load_IMP / fuel_depth;
            float packing_ratio = bulk_density / particle_density;
            float heat_of_preignition = 250 + 1116 * moisture_fraction; //Btu/lb
            float effective_heating_number = Mathf.Exp(-138 / surface_volume_ratio);

            float net_fuel_load_IMP = fuel_load_IMP / (1 + mineral_content_total);

            float e = 0.715f * Mathf.Exp(-0.000359f * surface_volume_ratio);
            float b = 0.02562f * Mathf.Pow(surface_volume_ratio, 0.54f);
            float c = 7.47f * Mathf.Exp(-0.133f * Mathf.Pow(surface_volume_ratio, 0.55f));
            float packing_ratio_op = 3.348f * Mathf.Pow(surface_volume_ratio, -0.8189f); //Optimum packing ratio
            float wind_coefficient = c * Mathf.Pow(wind_mid_flame * 54.68f, b) * Mathf.Pow(packing_ratio / packing_ratio_op, -e);

            float xi = Mathf.Pow(192f + 0.2595f * surface_volume_ratio, -1f) * Mathf.Exp((0.792f * 0.681f * Mathf.Pow(surface_volume_ratio, 0.5f)) * (packing_ratio + 0.1f)); //Propagating flux ratio

            float eta_S = 0.174f * Mathf.Pow(mineral_content_silica_free, -0.19f); //Mineral damping coefficient
            float eta_M = 1f - 2.59f * moisture_fraction / moisture_fraction_extinction + 5.11f * Mathf.Pow(moisture_fraction / moisture_fraction_extinction, 2f) - 3.52f * Mathf.Pow(moisture_fraction / moisture_fraction_extinction, 3f); //Moisture damping coefficient

            float a = 1f / (4.77f * Mathf.Pow(surface_volume_ratio, 0.1f) - 7.27f);
            float gamma_max = Mathf.Pow(surface_volume_ratio, 1.5f) / (495f + 0.0594f * Mathf.Pow(surface_volume_ratio, 1.5f)); //Maximum reaction velocity
            float Gamma = gamma_max * Mathf.Pow((packing_ratio / packing_ratio_op), a) * Mathf.Exp(a * (1f - packing_ratio / packing_ratio_op)); //Optimum reaction velocity

            float reaction_intensity = Gamma * net_fuel_load_IMP * heat_of_combustion_IMP * eta_M * eta_S; //Btu/ft^2 min

            float speed_surface = reaction_intensity * xi * (1 + wind_coefficient) / (bulk_density * effective_heating_number * heat_of_preignition);  //Surface rate of spread, ft/min
            speed_surface = speed_surface * MSEC_PER_FTMIN; //Convert to m/s

            //Using Byram (1959) to calculate surface fire intensity
            float intensity_ = heat_of_combustion_SI * fuel_load_SI * speed_surface; // Fire intensity, kW/m

            //Using Van Wagner (1977) for the crowning criteria threshold
            float heat_of_ignition = 460f + 26f * foliar_moisture_content; //Heat of ignition, kJ/kg
            float crowning_intensity = Mathf.Pow(0.01f * bh_o * heat_of_ignition, 1.5f); //Crowning threshold intensity, kW/m
            float crowning_ratio = intensity_ / crowning_intensity; //If this is greater than 1 then crowning is predicted and vice versa
            float speed_active_MMIN = 11.021f * Mathf.Pow(U_10, 0.8966f) * Mathf.Pow(bd_o, 0.1901f) * Mathf.Exp(-0.1714f * moisture_fraction * 100f); //Active crown fire spread rate, m/min.
            float speed_active_MS = speed_active_MMIN / 60; //Converting to m/s

            //Calculate criteria for active crowning from Cruz (2008)
            float CAC = speed_active_MMIN / (critical_mass_flow_rate / bd_o); //Criteria for active crowning.
            float speed_passive = speed_active_MS * Mathf.Exp(-1f * CAC); //Passive ROS

            //passive = ((crowning_ratio > 1) & (CAC < 1))
            bool passive = crowning_ratio > 1f && CAC < 1f;
            bool Active = crowning_ratio > 1f && CAC >= 1f;
            bool surface = crowning_ratio <= 1;

            float ROS;
            if(surface)
            {
                ROS = speed_surface;
            }
            else if(passive)
            {
                ROS = Mathf.Max(speed_passive, speed_surface);
            }
            else
            {
                ROS = speed_active_MS;
            }

            float fuel_load = fuel_load_SI * 10f; //convert back to t/ha
            if (Active || passive)
            {
                fuel_load = fuel_load + fl_o;
            }

            //convert to m/h
            ROS = ROS * 3600;
            float Intensity_total = intensity(ROS, fuel_load);
            float flame_height = 0.07755f * Mathf.Pow(Intensity_total, 0.46f);

            if(Active)
            {
                flame_height = flame_height + stand_height;
            }

            float[] fire_behaviour_array = new float[3];
            fire_behaviour_array[0] = ROS;
            fire_behaviour_array[1] = Intensity_total;
            fire_behaviour_array[2] = flame_height;

            return fire_behaviour_array;
        }

        /// returns the fireline intensity (kW/m) based on Byram 1959
        ///   ROS: forward rate of spread (km/h)
        ///   fuel_load: fine fuel load (t/ha)
        public static float intensity(float ROS, float fuel_load)
        {
            // convert units
            ROS = ROS / 3600; // m/s
            fuel_load = fuel_load / 10; //kg/m^2

            return 18608f * ROS * fuel_load;
        }


        /*private static float ROS_pine(float U_10, float mc, float DF, float KBDI)
        {
            float[] FB_pine = fire_behaviour_pine(U_10, mc, DF, KBDI, fuel_models_default); //TODO:check added deault
            return FB_pine[0];
        }

        private static float Intensity_pine(U_10, mc, DF, KBDI) As Single
            Dim FB_pine() As Single
            FB_pine = fire_behaviour_pine(U_10, mc, DF, KBDI)
            Intensity_pine = FB_pine(1)
        End Function

        private static float FH_pine(U_10, mc, DF, KBDI) As Single
            Dim FB_pine() As Single
            FB_pine = fire_behaviour_pine(U_10, mc, DF, KBDI)
            FH_pine = FB_pine(2)
        }*/

        // fuel array elements: proportion, fl_s, fl_o, bh_o, bd_o
        static readonly float[] fuelArray1 = { 0.151f, 4f, 11.5f, 0.7f, 0.17f };
        static readonly float[] fuelArray2 = { 0.151f, 5f, 12f, 1.5f, 0.18f };
        static readonly float[] fuelArray3 = { 0.121f, 8.5f, 12f, 2.5f, 0.18f };
        static readonly float[] fuelArray4 = { 0.091f, 10f, 8f, 6f, 0.12f };
        static readonly float[] fuelArray5 = { 0.394f, 7f, 10f, 14f, 0.15f };
        static readonly float[][] fuel_arrays = { fuelArray1, fuelArray2, fuelArray3, fuelArray4, fuelArray5 };
        /// returns array of the the forward rate of spread (m/h), intensity (kW/m) and flame height (m) for pine using an mixed stand ensemble
        ///
        /// args
        ///   U_10: 10 m wind speed (km/h)
        ///   mc: dead fuel moisture content %
        ///   DF: drought factor
        ///   KBDI: Keetch Byram drought index KBDI
        private static float[] fb_pine_ensemble(float U_10, float mc, float DF, float KBDI)
        {
            //initialise
            float grass_proportion = 0.091f;
            float wrf = 5f;
            float ROS = Grassland.ROS_grass(U_10, (mc), 100, Grassland.States.EatenOut);
            float Intensity_total = intensity(ROS, 1.5f) * grass_proportion;
            float flame_height = Grassland.Flame_height_grass((ROS), Grassland.States.EatenOut) * grass_proportion;
            ROS = ROS * grass_proportion;

            foreach (float[] fuel_model in fuel_arrays)
            {
                float proportion = fuel_model[0];
                float fl_s = fuel_model[1];
                float fl_o = fuel_model[2];
                float bh_o = fuel_model[3];
                float bd_o = fuel_model[4];

                float[] input = { wrf, fl_s, fl_o, bh_o, bd_o };
                
                float[] fb_array = fire_behaviour_pine(U_10, mc, DF, KBDI, input);

                ROS = ROS + proportion * fb_array[0];
                Intensity_total = Intensity_total + proportion * fb_array[1];
                flame_height = flame_height + proportion * fb_array[2];
            }

            float[] fire_behaviour_array = { ROS, Intensity_total, flame_height };

            return fire_behaviour_array;
            //fb_pine_ensemble = result_array
        }
    }
}
