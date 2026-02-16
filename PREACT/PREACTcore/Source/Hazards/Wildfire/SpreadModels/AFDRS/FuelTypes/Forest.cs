using System;
using PREACT.Math;

namespace PREACT.Wildfire.AFDRS
{   
    public class Forest
    {
        public enum FuelSubTypes { Dry, Wet }

        float _fmc;
        float _ros;
        float _fuelLoad;
        float _intensity;
        float _flameHeight;

        FuelSubTypes _subtype;

        public Forest(FuelSubTypes submodel)
        {
            _subtype = submodel;
        }

        public void Calculate(float temp, float rh, float U_10, DateTime dateTime)
        {
            _fmc = FMC_forest(temp, rh, dateTime, _subtype);
            //_ros = ROS_forest(U_10)
            //_fuelLoad = fuel_availability_forest()
            //_intensity = Intensity_forest()
            //_flameHeight = Flame_height_forest(_ros)
        }

        /// return the intensity based on fuel load and ROS
        /// note AFDRS caps surface fuel load at 10 t/ha (1 kg/m)
        ///   ROS: forward rate of spread (km/h)
        ///   DF: drought (fuel availability) factor (1-10)
        ///   flame_h: flame height (m)
        ///   fl_s: surface fuel load (t/ha)
        ///   fl_ns: near surface fuel load (t/ha)
        ///   fl_e: elevated fuel load (t/ha)
        ///   fl_o: overstorey (canopy) fuel load (t/ha)
        ///   h_o: overstorey (canopy) height (m)
        private float Intensity_forest(float ROS, float DF, float flame_h, float fl_s, float fl_ns, float fl_e, float fl_o, float h_o, float waf = 3f, float DI = 100f, FuelSubTypes submodel = FuelSubTypes.Dry)
        {
            float fuel_avail;
            float fuel_load;
            float flame_h_elev = 1f; //m
            float flame_h_crown_frac = 0.66f; //dimensionless

            //modify fuel parameters with fuel availability
            fuel_avail = fuel_availability_forest(DF, DI, waf, submodel);
            fl_s = fl_s * fuel_avail;
            fl_ns = fl_ns * fuel_avail;
            fl_e = fl_e * fuel_avail;
            fl_o = fl_o * fuel_avail;

            //cap surface fuel load
            fl_s = Mathf.Min(10f, fl_s);

            //accumulate fuel load based on flame height
            fuel_load = fl_s + fl_ns;

            if(flame_h > flame_h_elev)
            {
                fuel_load = fuel_load + fl_e;
            }

            if(flame_h > h_o * flame_h_crown_frac)
            {
                fuel_load = fuel_load + 0.5f * fl_o;
            }

            return intensity(ROS, fuel_load);
        }

        /// returns the fireline intensity (kW/m) based on Byram 1959
        ///   ROS: forward rate of spread (km/h)
        ///   fuel_load: fine fuel load (t/ha)
        private float intensity(float ROS, float fuel_load)
        {
            // convert units
            ROS = ROS / 3600; // m/s
            fuel_load = fuel_load / 10; //kg/m^2

            return 18600f * ROS * fuel_load;
        }

        /// returns the flame height (m)
        ///   ROS - forward rate of spread (m/h)
        ///   h_el - elevated fuel height (m)
        private float Flame_height_forest(float ROS, float h_el)
        {
            return 0.0193f * Mathf.Pow(ROS, 0.723f) * Mathf.Exp(h_el * 0.64f) * 1.07f;
        }

        /// returns the forward ROS (m/h) ignoring slope
        ///   U_10: 10 m wind speed (km/h)
        ///   fhs_s: surface fuel hazard score
        ///   fhs_ns: near surface fuel hazard score
        ///   h_ns: near surface fuel height (cm)
        ///   fmc: fuel moisture content (%)
        ///   DF: Drought factor
        ///   DI: drought indes - KBDI except SDI in Tas
        ///   WAF: wind adjustment factor
        ///   submodel: dry or wet
        private float ROS_forest(float U_10, float fhs_s, float fhs_ns, float h_ns, float fmc, float DF, float waf, float DI = 100f, FuelSubTypes submodel = FuelSubTypes.Dry)
        {
            float wind_threshold = 5;
            float fuel_avail;
            h_ns = Mathf.Min(h_ns, 20);
            float mf = Mf_forest(fmc); //moisture function

            //modify fuel parameters with fuel availability
            fuel_avail = fuel_availability_forest(DF, DI, waf, submodel);
            fhs_s = fhs_s * fuel_avail;
            fhs_ns = fhs_ns * fuel_avail;

            //apply wind reduction factor
            float wind_speed = U_10 * 3 / waf;

            //calculate ROS for 7% moisture
            float ROS_forest;
            if (wind_speed > wind_threshold)
            {
                ROS_forest = 30f + 1.5308f * Mathf.Pow(wind_speed - wind_threshold, 0.8576f) * Mathf.Pow(fhs_s, 0.9301f) * Mathf.Pow(fhs_ns * h_ns,  0.6366f) * 1.03f;
            }
            else
            {
                ROS_forest = 30f;
            }

            //apply moisture factor
            return ROS_forest * mf;
        }

        /// return the fine fuel moisture content (%)
        ///   temp: air temperature (C)
        ///   rh: relative humidity (%)
        ///   date_: (underscore due to VBA Date objects)
        ///   time:
        private float FMC_forest(float temp, float rh, DateTime dateTime, FuelSubTypes submodel)
        {
            const int start_peak_month = 10; //October
            const int end_peak_month = 3; //March
            const int start_afternoon = 12;
            const int end_afternoon = 17;
            const int sunrise = 6;
            const int sunset = 19;

            float FMC_forest;
            if((dateTime.Month >= start_peak_month || dateTime.Month <= end_peak_month) 
                && (dateTime.Hour >= start_afternoon && dateTime.Hour <= end_afternoon)
                && submodel == FuelSubTypes.Dry)
            {
                FMC_forest = 2.76f + 0.124f * rh - 0.0187f * temp;
            }                
            else if (dateTime.Hour <= sunrise || dateTime.Hour >= sunset)
            {
                FMC_forest = 3.08f + 0.198f * rh - 0.0483f * temp;
            }
            else
            {
                FMC_forest = 3.6f + 0.169f * rh - 0.045f * temp;
            }

            return FMC_forest;
        }

        /// returns the forest fuel moisture factor
        ///   fmc: fine fule moisture content (%)
        private float Mf_forest(float fmc)
        {
            float Mf_forest;
            if (fmc <= 4)
            {
                Mf_forest = 2.31f;
            }                
            else if( fmc > 20)
            {
                Mf_forest = 0.05f;
            }
            else
            {
                Mf_forest = 18.35f * Mathf.Pow(fmc, -1.495f);
            }

            return Mf_forest;
        }

        /// returns the spotting distance (m)
        ///   ROS: forward rate of spread (m/h)
        ///   U_10: 10m wind speed (km/h)
        ///   fhs_s: fuel hazard score surface
        private float Spotting_forest(float ROS, float U_10, float fhs_s)
        {
            float Spotting_forest;

            if (ROS < 150f)
            {
                Spotting_forest = 50f;
            }                
            else
            {
                Spotting_forest = Mathf.Abs(176.969f * Mathf.Atan(fhs_s) * Mathf.Sqrt(ROS / Mathf.Pow(U_10, 0.25f)) + 1568800f * Mathf.Pow(fhs_s, -1) * Mathf.Pow(ROS / Mathf.Pow(U_10, 0.25f), -1.5f) - 3015.09f);
            }

            return Spotting_forest;
        }

        /// returns the fuel availability - proportion of fuel available to be burnt
        ///   DF: Drought factor
        ///   DI: drought indes - KBDI except SDI in Tas
        ///   WAF: wind adjustment factor
        ///   submodel: dry or wet
        float fuel_availability_forest(float DF, float DI = 100f, float waf = 3f, FuelSubTypes submodel = FuelSubTypes.Dry)
        {
            float fuel_availability_forest = 0f;

            if (submodel == FuelSubTypes.Dry)
            {
                fuel_availability_forest = DF * 0.1f;
            }                
            else //wet
            {
                float C1 = 0.1f * ((0.0046f * Mathf.Pow(waf, 2) - 0.0079f * waf - 0.0175f) * DI + (-0.9167f * Mathf.Pow(waf, 2f) + 1.5833f * waf + 13.5f));
                C1 = Mathf.Max(C1, 0);
                C1 = Mathf.Min(C1, 1);
                fuel_availability_forest = 1.008f / (1f + 104.9f * Mathf.Exp(-0.9306f * C1 * DF));
                fuel_availability_forest = Mathf.Min(fuel_availability_forest, DF * 0.1f); //shouldn't get higher ros for wet when WAF is low
            }

            return fuel_availability_forest;
        }

        /*Public Sub update_from_LUT_Forest(bool loads_only = false)
        {
            float FTno;
            float time_since_fire;
            float load_max;
            float fuel_load_k;

            FTno = Application.WorksheetFunction.VLookup(Range("ClassForest").Value, Range("ForestLUT"), 2, False)


            if (FTno = 9999)
            {
                Exit Sub
            }

                    Dim lut As String
            lut = "AFDRS Fuel LUT"
            Dim table As String
            table = "AFDRS_LUT"
            Dim fuel_sub_type As String
            fuel_sub_type = "Fuel_FDR"


            if (Range("State").Value = "NSWv402")
                    {
                        lut = "NSW_Fuel_v402_LUT"
                table = "NSW_fuel_LUT"
                fuel_sub_type = "AFDRS fuel type"
            }


                    time_since_fire = Range("tsf").Value

            // surface fuel
            fuel_load_max = LookupValueInTable(FTno, "FTno_State", "FL_s", lut, table)
            fuel_load_k = LookupValueInTable(FTno, "FTno_State", "Fk_s", lut, table)
            Range("fl_s_forest").Value = fuel_amount(fuel_load_max, time_since_fire, fuel_load_k)
            fhs_max = LookupValueInTable(FTno, "FTno_State", "FHS_s", lut, table)
            Range("fhs_s").Value = fuel_amount(fhs_max, time_since_fire, fuel_load_k)


            // near surface fuel
            fuel_load_max = LookupValueInTable(FTno, "FTno_State", "FL_ns", lut, table)
            fuel_load_k = LookupValueInTable(FTno, "FTno_State", "Fk_ns", lut, table)
            Range("fl_ns_forest").Value = fuel_amount(fuel_load_max, time_since_fire, fuel_load_k)
            fhs_max = LookupValueInTable(FTno, "FTno_State", "FHS_ns", lut, table)
            Range("fhs_ns").Value = fuel_amount(fhs_max, time_since_fire, fuel_load_k)

            // elevated fuel
            fuel_load_max = LookupValueInTable(FTno, "FTno_State", "FL_el", lut, table)
            fuel_load_k = LookupValueInTable(FTno, "FTno_State", "Fk_el", lut, table)
            Range("fl_e_forest").Value = fuel_amount(fuel_load_max, time_since_fire, fuel_load_k)

            // bark fuel load
            fuel_load_max = LookupValueInTable(FTno, "FTno_State", "FL_b", lut, table)
            fuel_load_k = LookupValueInTable(FTno, "FTno_State", "Fk_b", lut, table)
            Range("fl_b_forest").Value = fuel_amount(fuel_load_max, time_since_fire, fuel_load_k)

            // canopy fuel load
            fuel_load_max = LookupValueInTable(FTno, "FTno_State", "FL_o", lut, table)
            fuel_load_k = LookupValueInTable(FTno, "FTno_State", "Fk_o", lut, table)
            Range("fl_o_forest").Value = fuel_amount(fuel_load_max, time_since_fire, fuel_load_k)


            if (!loads_only)
            {         
                // near surface fuel height
                Range("h_ns_forest").Value = LookupValueInTable(FTno, "FTno_State", "H_ns", lut, table)

                // near surface fuel height
                Range("h_e_forest").Value = LookupValueInTable(FTno, "FTno_State", "H_el", lut, table)

                // near surface fuel height
                Range("h_o_forest").Value = LookupValueInTable(FTno, "FTno_State", "H_o", lut, table)

                //WRF
                Range("waf_forest").Value = LookupValueInTable(FTno, "FTno_State", "WRF_For", lut, table)

                // submodel
                if (LookupValueInTable(FTno, "FTno_State", fuel_sub_type, lut, table) = "Wet_forest")
                {
                    Range("submodel_forest").Value = "wet";
                }                    
                else
                {
                    Range("submodel_forest").Value = "dry";
                }

            }
        }*/    
    }
}
