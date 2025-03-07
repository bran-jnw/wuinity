﻿//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using static WUIPlatform.Fire.Behave.BehaveUnits;

namespace WUIPlatform.Fire.Behave
{
    public class FuelModelSet
    {
        public struct FuelModelRecord
        {
            public int fuelModelNumber_;               // Standard ID number for fuel model 
            public string code_;                  // Fuel model code, usually 2 letters followed by number,(e.g., "GR1")
            public string name_;                  // Fuel model name, (e.g., "Humid Climate Grass")
            public double fuelbedDepth_;               // Fuelbed depth in feet
            public double moistureOfExtinctionDead_;   // Dead fuel extinction moisture content (fraction)
            public double heatOfCombustionDead_;       // Dead fuel heat of combustion (Btu/lb)
            public double heatOfCombustionLive_;       // Live fuel heat of combustion (Btu/lb)
            public double fuelLoadOneHour_;            // Dead 1 hour fuel loading (lb/ft^2)
            public double fuelLoadTenHour_;            // Dead 10 hour fuel loading (lb/ft^2)
            public double fuelLoadHundredHour_;        // Dead 100 hour fuel loading (lb/ft^2)
            public double fuelLoadLiveHerbaceous_;     // Live herb fuel loading (lb/ft^2)
            public double fuelLoadLiveWoody_;          // Live wood fuel loading (lb/ft^2)
            public double savrOneHour_;                // Dead 1-h fuel surface area to volume ratio (ft^2/ft^3)
            public double savrLiveHerbaceous_;         // Live herb surface area to volume ratio (ft^2/ft^3)
            public double savrLiveWoody_;              // Live wood surface area to volume ratio (ft^2/ft^3)
            public bool isDynamic_;                    // If true, the fuel model is dynamic
            public bool isReserved_;                   // If true, record cannot be used for custom fuel model
            public bool isDefined_;                    // If true, record has been populated with values for its fields
        }

        FuelModelRecord[] FuelModelArray_;

        public FuelModelSet()
        {
            System.Array.Resize(ref FuelModelArray_, (int)SurfaceInputs.FuelConstants.NUM_FUEL_MODELS);
            initializeAllFuelModelRecords();
            populateFuelModels();
        }

        public FuelModelSet(FuelModelSet rhs)
        {
            memberwiseCopyAssignment(rhs);
        }

        /*FuelModelSet operator=(const FuelModelSet& rhs)
        {
            if (this != &rhs)
            {
                memberwiseCopyAssignment(rhs);
            }
            return *this;
        }*/

        void memberwiseCopyAssignment(FuelModelSet rhs)
        {
            System.Array.Resize(ref FuelModelArray_, rhs.FuelModelArray_.Length);
            for (int i = 0; i < rhs.FuelModelArray_.Length; i++)
            {
                FuelModelArray_[i].fuelModelNumber_ = rhs.FuelModelArray_[i].fuelModelNumber_;
                FuelModelArray_[i].code_ = rhs.FuelModelArray_[i].code_;
                FuelModelArray_[i].name_ = rhs.FuelModelArray_[i].name_;
                FuelModelArray_[i].fuelbedDepth_ = rhs.FuelModelArray_[i].fuelbedDepth_;
                FuelModelArray_[i].moistureOfExtinctionDead_ = rhs.FuelModelArray_[i].moistureOfExtinctionDead_;
                FuelModelArray_[i].heatOfCombustionDead_ = rhs.FuelModelArray_[i].heatOfCombustionDead_;
                FuelModelArray_[i].heatOfCombustionLive_ = rhs.FuelModelArray_[i].heatOfCombustionLive_;
                FuelModelArray_[i].fuelLoadOneHour_ = rhs.FuelModelArray_[i].fuelLoadOneHour_;
                FuelModelArray_[i].fuelLoadTenHour_ = rhs.FuelModelArray_[i].fuelLoadTenHour_;
                FuelModelArray_[i].fuelLoadHundredHour_ = rhs.FuelModelArray_[i].fuelLoadHundredHour_;
                FuelModelArray_[i].fuelLoadLiveHerbaceous_ = rhs.FuelModelArray_[i].fuelLoadLiveHerbaceous_;
                FuelModelArray_[i].fuelLoadLiveWoody_ = rhs.FuelModelArray_[i].fuelLoadLiveWoody_;
                FuelModelArray_[i].savrOneHour_ = rhs.FuelModelArray_[i].savrOneHour_;
                FuelModelArray_[i].savrLiveHerbaceous_ = rhs.FuelModelArray_[i].savrLiveHerbaceous_;
                FuelModelArray_[i].savrLiveWoody_ = rhs.FuelModelArray_[i].savrLiveWoody_;
                FuelModelArray_[i].isReserved_ = rhs.FuelModelArray_[i].isReserved_;
                FuelModelArray_[i].isDefined_ = rhs.FuelModelArray_[i].isDefined_;
            }
        }

        ~FuelModelSet()
        {

        }

        void initializeSingleFuelModelRecord(int fuelModelNumber)
        {
            FuelModelArray_[fuelModelNumber].fuelModelNumber_ = 0;
            FuelModelArray_[fuelModelNumber].code_ = "NO_CODE";
            FuelModelArray_[fuelModelNumber].name_ = "NO_NAME";
            FuelModelArray_[fuelModelNumber].fuelbedDepth_ = 0;
            FuelModelArray_[fuelModelNumber].moistureOfExtinctionDead_ = 0;
            FuelModelArray_[fuelModelNumber].heatOfCombustionDead_ = 0;
            FuelModelArray_[fuelModelNumber].heatOfCombustionLive_ = 0;
            FuelModelArray_[fuelModelNumber].fuelLoadOneHour_ = 0;
            FuelModelArray_[fuelModelNumber].fuelLoadTenHour_ = 0;
            FuelModelArray_[fuelModelNumber].fuelLoadHundredHour_ = 0;
            FuelModelArray_[fuelModelNumber].fuelLoadLiveHerbaceous_ = 0;
            FuelModelArray_[fuelModelNumber].fuelLoadLiveWoody_ = 0;
            FuelModelArray_[fuelModelNumber].savrOneHour_ = 0;
            FuelModelArray_[fuelModelNumber].savrLiveHerbaceous_ = 0;
            FuelModelArray_[fuelModelNumber].savrLiveWoody_ = 0;
            FuelModelArray_[fuelModelNumber].isReserved_ = false;
            FuelModelArray_[fuelModelNumber].isDefined_ = false;
        }

        void initializeAllFuelModelRecords()
        {
            for (int i = 0; i < (int)SurfaceInputs.FuelConstants.NUM_FUEL_MODELS; i++)
            {
                initializeSingleFuelModelRecord(i);
            }
        }

        void setFuelModelRecord(int fuelModelNumber, string code, string name,
            double fuelBedDepth, double moistureOfExtinctionDead, double heatOfCombustionDead, double heatOfCombustionLive,
            double fuelLoadOneHour, double fuelLoadTenHour, double fuelLoadHundredHour, double fuelLoadliveHerbaceous,
            double fuelLoadliveWoody, double savrOneHour, double savrLiveHerbaceous, double savrLiveWoody,
            bool isDynamic, bool isReserved)
        {
            FuelModelArray_[fuelModelNumber].fuelModelNumber_ = fuelModelNumber;
            FuelModelArray_[fuelModelNumber].code_ = code;
            FuelModelArray_[fuelModelNumber].name_ = name;
            FuelModelArray_[fuelModelNumber].fuelbedDepth_ = fuelBedDepth;
            FuelModelArray_[fuelModelNumber].moistureOfExtinctionDead_ = moistureOfExtinctionDead;
            FuelModelArray_[fuelModelNumber].heatOfCombustionDead_ = heatOfCombustionDead;
            FuelModelArray_[fuelModelNumber].heatOfCombustionLive_ = heatOfCombustionLive;
            FuelModelArray_[fuelModelNumber].fuelLoadOneHour_ = fuelLoadOneHour;
            FuelModelArray_[fuelModelNumber].fuelLoadTenHour_ = fuelLoadTenHour;
            FuelModelArray_[fuelModelNumber].fuelLoadHundredHour_ = fuelLoadHundredHour;
            FuelModelArray_[fuelModelNumber].fuelLoadLiveHerbaceous_ = fuelLoadliveHerbaceous;
            FuelModelArray_[fuelModelNumber].fuelLoadLiveWoody_ = fuelLoadliveWoody;
            FuelModelArray_[fuelModelNumber].savrOneHour_ = savrOneHour;
            FuelModelArray_[fuelModelNumber].savrLiveHerbaceous_ = savrLiveHerbaceous;
            FuelModelArray_[fuelModelNumber].savrLiveWoody_ = savrLiveWoody;
            FuelModelArray_[fuelModelNumber].isDynamic_ = isDynamic;
            FuelModelArray_[fuelModelNumber].isReserved_ = isReserved;
            FuelModelArray_[fuelModelNumber].isDefined_ = true;
        }

        //bran-jnw
        public void setFuelModelRecord(FuelModelInput.FuelModel fuelModel)
        {
            int fuelModelNumber = fuelModel.fuelModelNumber;
            FuelModelArray_[fuelModelNumber].fuelModelNumber_ = fuelModelNumber;
            FuelModelArray_[fuelModelNumber].code_ = fuelModel.code;
            FuelModelArray_[fuelModelNumber].name_ = fuelModel.name;
            FuelModelArray_[fuelModelNumber].fuelbedDepth_ = fuelModel.fuelBedDepth;
            FuelModelArray_[fuelModelNumber].moistureOfExtinctionDead_ = fuelModel.moistureOfExtinctionDead;
            FuelModelArray_[fuelModelNumber].heatOfCombustionDead_ = fuelModel.heatOfCombustionDead;
            FuelModelArray_[fuelModelNumber].heatOfCombustionLive_ = fuelModel.heatOfCombustionLive;
            FuelModelArray_[fuelModelNumber].fuelLoadOneHour_ = fuelModel.fuelLoadOneHour;
            FuelModelArray_[fuelModelNumber].fuelLoadTenHour_ = fuelModel.fuelLoadTenHour;
            FuelModelArray_[fuelModelNumber].fuelLoadHundredHour_ = fuelModel.fuelLoadHundredHour;
            FuelModelArray_[fuelModelNumber].fuelLoadLiveHerbaceous_ = fuelModel.fuelLoadLiveHerbaceous;
            FuelModelArray_[fuelModelNumber].fuelLoadLiveWoody_ = fuelModel.fuelLoadLiveWoody;
            FuelModelArray_[fuelModelNumber].savrOneHour_ = fuelModel.savrOneHour;
            FuelModelArray_[fuelModelNumber].savrLiveHerbaceous_ = fuelModel.savrLiveHerbaceous;
            FuelModelArray_[fuelModelNumber].savrLiveWoody_ = fuelModel.savrLiveWoody;
            FuelModelArray_[fuelModelNumber].isDynamic_ = fuelModel.isDynamic;
            FuelModelArray_[fuelModelNumber].isReserved_ = fuelModel.isReserved;
            FuelModelArray_[fuelModelNumber].isDefined_ = true;
        }

        // PopulateFuelModels() fills FuelModelArray[] with the standard fuel model parameters
        // as well as earmarking which models are available for use as custom models
        void populateFuelModels()
        {
            // See Standard Fire Behavior Fuel Models: A Comprehensive Set for Use with Rothermel’s
            // Surface Fire Spread Model by Joe H.Scott and Robert E.Burgan, 2005
            // https://www.fs.fed.us/rm/pubs/rmrs_gtr153.pdf

            // Index 0 is not used
            setFuelModelRecord(0, "NO_CODE", "NO_NAME", 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, false, false);
            FuelModelArray_[0].isDefined_ = false;
            /*
            fuelModelNumber, code, name
            fuelBedDepth, moistureOfExtinctionDeadFuel, heatOfCombustionDeadFuel, heatOfCombustionLiveFuel,
            fuelLoad1Hour, fuelLoad10Hour, fuelLoad100Hour, fuelLoadLiveHerb, fuelLoadLiveWood,
            savr1HourFuel, savrLiveHerb, savrLiveWood,
            isDynamic, isReserved
            - WMC 10/2015
            */

            // Code FMx: Original 13 Fuel Models
            setFuelModelRecord(1, "FM1", "Short grass [1]",
                1.0, 0.12, 8000, 8000,
                0.034, 0, 0, 0, 0,
                3500, 1500, 1500,
                false, true);
            setFuelModelRecord(2, "FM2", "Timber grass and understory [2]",
                1.0, 0.15, 8000, 8000,
                0.092, 0.046, 0.023, 0.023, 0,
                3000, 1500, 1500,
                false, true);
            setFuelModelRecord(3, "FM3", "Tall grass [3]",
                2.5, 0.25, 8000, 8000,
                0.138, 0, 0, 0, 0,
                1500, 1500, 1500,
                false, true);
            setFuelModelRecord(4, "FM4", "Chaparral [4]",
                6.0, 0.2, 8000, 8000,
                0.230, 0.184, 0.092, 0, 0.230,
                2000, 1500, 1500,
                false, true);
            setFuelModelRecord(5, "FM5", "Brush [5]",
                2.0, 0.20, 8000, 8000,
                0.046, 0.023, 0, 0, 0.092,
                2000, 1500, 1500,
                false, true);
            setFuelModelRecord(6, "FM6", "Dormant brush, hardwood slash [6]",
                2.5, 0.25, 8000, 8000,
                0.069, 0.115, 0.092, 0, 0,
                1750, 1500, 1500,
                false, true);
            setFuelModelRecord(7, "FM7", "Southern rough [7]",
                2.5, 0.40, 8000, 8000,
                0.052, 0.086, 0.069, 0, 0.017,
                1750, 1500, 1500,
                false, true);
            setFuelModelRecord(8, "FM8", "Short needle litter [8]",
                0.2, 0.3, 8000, 8000,
                0.069, 0.046, 0.115, 0, 0,
                2000, 1500, 1500,
                false, true);
            setFuelModelRecord(9, "FM9", "Long needle or hardwood litter [9]",
                0.2, 0.25, 8000, 8000,
                0.134, 0.019, 0.007, 0, 0,
                2500, 1500, 1500,
                false, true);
            setFuelModelRecord(10, "FM10", "Timber litter & understory [10]",
                1.0, 0.25, 8000, 8000,
                0.138, 0.092, 0.230, 0, 0.092,
                2000, 1500, 1500,
                false, true);
            setFuelModelRecord(11, "FM11", "Light logging slash [11]",
                1.0, 0.15, 8000, 8000,
                0.069, 0.207, 0.253, 0, 0,
                1500, 1500, 1500,
                false, true);
            setFuelModelRecord(12, "FM12", "Medium logging slash [12]",
                2.3, 0.20, 8000, 8000,
                0.184, 0.644, 0.759, 0, 0,
                1500, 1500, 1500,
                false, true);
            setFuelModelRecord(13, "FM13", "Heavy logging slash [13]",
                3.0, 0.25, 8000, 8000,
                0.322, 1.058, 1.288, 0, 0,
                1500, 1500, 1500,
                false, true);

            // 14-89 Available for custom models

            // Code NBx: Non-burnable
            // 90 Available for custom NB model  
            setFuelModelRecord(91, "NB1", "Urban, developed [91]",
                1.0, 0.10, 8000, 8000,
                0, 0, 0, 0, 0,
                1500, 1500, 1500,
                false, true);
            setFuelModelRecord(92, "NB2", "Snow, ice [92]",
                1.0, 0.10, 8000, 8000,
                0, 0, 0, 0, 0,
                1500, 1500, 1500,
                false, true);
            setFuelModelRecord(93, "NB3", "Agricultural [93]",
                1.0, 0.10, 8000, 8000,
                0, 0, 0, 0, 0,
                1500, 1500, 1500,
                false, true);

            // Indices 94-95 Reserved for future standard non-burnable models
            for (int i = 94; i <= 95; i++)
            {
                markAsReservedModel(i);
            }

            setFuelModelRecord(94, "NB4", "Future standard non-burnable [94]",
                1.0, 0.10, 8000, 8000,
                0, 0, 0, 0, 0,
                1500, 1500, 1500,
                false, true);
            setFuelModelRecord(95, "NB5", "Future standard non-burnable [95]",
                1.0, 0.10, 8000, 8000,
                0, 0, 0, 0, 0,
                1500, 1500, 1500,
                false, true);

            // Indices 96-97 Available for custom NB model

            setFuelModelRecord(98, "NB8", "Open water [98]",
                1.0, 0.10, 8000, 8000,
                0, 0, 0, 0, 0,
                1500, 1500, 1500,
                false, true);
            setFuelModelRecord(99, "NB9", "Bare ground [99]",
                1.0, 0.10, 8000, 8000,
                0, 0, 0, 0, 0,
                1500, 1500, 1500,
                false, true);

            // Code GRx: Grass
            // Index 100 Available for custom GR model
            double f = 2000.0 / 43560.0;
            setFuelModelRecord(101, "GR1", "Short, sparse, dry climate grass (D)",
                0.4, 0.15, 8000, 8000,
                0.10 * f, 0, 0, 0.30 * f, 0,
                2200, 2000, 1500,
                true, true);
            setFuelModelRecord(102, "GR2", "Low load, dry climate grass (D)",
                1.0, 0.15, 8000, 8000,
                0.10 * f, 0, 0, 1.0 * f, 0,
                2000, 1800, 1500,
                true, true);
            setFuelModelRecord(103, "GR3",
                "Low load, very coarse, humid climate grass (D)",
                2.0, 0.30, 8000, 8000,
                0.10 * f, 0.40 * f, 0, 1.50 * f, 0,
                1500, 1300, 1500,
                true, true);
            setFuelModelRecord(104, "GR4", "Moderate load, dry climate grass (D)",
                2.0, 0.15, 8000, 8000,
                0.25 * f, 0, 0, 1.9 * f, 0,
                2000, 1800, 1500,
                true, true);
            setFuelModelRecord(105, "GR5", "Low load, humid climate grass (D)",
                1.5, 0.40, 8000, 8000,
                0.40 * f, 0.0, 0.0, 2.50 * f, 0.0,
                1800, 1600, 1500,
                true, true);
            setFuelModelRecord(106, "GR6",
                "Moderate load, humid climate grass (D)",
                1.5, 0.40, 9000, 9000,
                0.10 * f, 0, 0, 3.4 * f, 0,
                2200, 2000, 1500,
                true, true);
            setFuelModelRecord(107, "GR7",
                "High load, dry climate grass (D)",
                3.0, 0.15, 8000, 8000,
                1.0 * f, 0, 0, 5.4 * f, 0,
                2000, 1800, 1500,
                true, true);
            setFuelModelRecord(108, "GR8",
                "High load, very coarse, humid climate grass (D)",
                4.0, 0.30, 8000, 8000,
                0.5 * f, 1.0 * f, 0, 7.3 * f, 0,
                1500, 1300, 1500,
                true, true);
            setFuelModelRecord(109, "GR9",
                "Very high load, humid climate grass (D)",
                5.0, 0.40, 8000, 8000,
                1.0 * f, 1.0 * f, 0, 9.0 * f, 0,
                1800, 1600, 1500,
                true, true);
            // 110-112 are reserved for future standard grass models
            for (int i = 110; i <= 112; i++)
            {
                markAsReservedModel(i);
            }
            // 113-119 are available for custom grass models

            // Code GSx: Grass and shrub
            // 120 available for custom grass and shrub model
            setFuelModelRecord(121, "GS1",
                "Low load, dry climate grass-shrub (D)",
                0.9, 0.15, 8000, 8000,
                0.2 * f, 0, 0, 0.5 * f, 0.65 * f,
                2000, 1800, 1800,
                true, true);
            setFuelModelRecord(122, "GS2",
                "Moderate load, dry climate grass-shrub (D)",
                1.5, 0.15, 8000, 8000,
                0.5 * f, 0.5 * f, 0, 0.6 * f, 1.0 * f,
                2000, 1800, 1800,
                true, true);
            setFuelModelRecord(123, "GS3",
                "Moderate load, humid climate grass-shrub (D)",
                1.8, 0.40, 8000, 8000,
                0.3 * f, 0.25 * f, 0, 1.45 * f, 1.25 * f,
                1800, 1600, 1600,
                true, true);
            setFuelModelRecord(124, "GS4",
                "High load, humid climate grass-shrub (D)",
                2.1, 0.40, 8000, 8000,
                1.9 * f, 0.3 * f, 0.1 * f, 3.4 * f, 7.1 * f,
                1800, 1600, 1600,
                true, true);
            // 125-130 reserved for future standard grass and shrub models
            for (int i = 125; i <= 130; i++)
            {
                markAsReservedModel(i);
            }
            // 131-139 available for custom grass and shrub models

            // Shrub
            // 140 available for custom shrub model
            setFuelModelRecord(141, "SH1",
                "Low load, dry climate shrub (D)",
                1.0, 0.15, 8000, 8000,
                0.25 * f, 0.25 * f, 0, 0.15 * f, 1.3 * f,
                2000, 1800, 1600,
                true, true);
            setFuelModelRecord(142, "SH2",
                "Moderate load, dry climate shrub (S)",
                1.0, 0.15, 8000, 8000,
                1.35 * f, 2.4 * f, 0.75 * f, 0, 3.85 * f,
                2000, 1800, 1600,
                true, true);
            setFuelModelRecord(143, "SH3",
                "Moderate load, humid climate shrub (S)",
                2.4, 0.40, 8000, 8000,
                0.45 * f, 3.0 * f, 0, 0, 6.2 * f,
                1600, 1800, 1400,
                true, true);
            setFuelModelRecord(144, "SH4",
                "Low load, humid climate timber-shrub (S)",
                3.0, 0.30, 8000, 8000,
                0.85 * f, 1.15 * f, 0.2 * f, 0, 2.55 * f,
                2000, 1800, 1600,
                true, true);
            setFuelModelRecord(145, "SH5",
                "High load, dry climate shrub (S)",
                6.0, 0.15, 8000, 8000,
                3.6 * f, 2.1 * f, 0, 0, 2.9 * f,
                750, 1800, 1600,
                true, true);
            setFuelModelRecord(146, "SH6",
                "Low load, humid climate shrub (S)",
                2.0, 0.30, 8000, 8000,
                2.9 * f, 1.45 * f, 0, 0, 1.4 * f,
                750, 1800, 1600,
                true, true);
            setFuelModelRecord(147, "SH7",
                "Very high load, dry climate shrub (S)",
                6.0, 0.15, 8000, 8000,
                3.5 * f, 5.3 * f, 2.2 * f, 0, 3.4 * f,
                750, 1800, 1600,
                true, true);
            setFuelModelRecord(148, "SH8",
                "High load, humid climate shrub (S)",
                3.0, 0.40, 8000, 8000,
                2.05 * f, 3.4 * f, 0.85 * f, 0, 4.35 * f,
                750, 1800, 1600,
                true, true);
            setFuelModelRecord(149, "SH9",
                "Very high load, humid climate shrub (D)",
                4.4, 0.40, 8000, 8000,
                4.5 * f, 2.45 * f, 0, 1.55 * f, 7.0 * f,
                750, 1800, 1500,
                true, true);
            // 150-152 reserved for future standard shrub models
            for (int i = 150; i <= 152; i++)
            {
                markAsReservedModel(i);
            }
            // 153-159 available for custom shrub models

            // Timber and understory
            // 160 available for custom timber and understory model
            setFuelModelRecord(161, "TU1",
                "Light load, dry climate timber-grass-shrub (D)",
                0.6, 0.20, 8000, 8000,
                0.2 * f, 0.9 * f, 1.5 * f, 0.2 * f, 0.9 * f,
                2000, 1800, 1600,
                true, true);
            setFuelModelRecord(162, "TU2",
                "Moderate load, humid climate timber-shrub (S)",
                1.0, 0.30, 8000, 8000,
                0.95 * f, 1.8 * f, 1.25 * f, 0, 0.2 * f,
                2000, 1800, 1600,
                true, true);
            setFuelModelRecord(163, "TU3",
                "Moderate load, humid climate timber-grass-shrub (D)",
                1.3, 0.30, 8000, 8000,
                1.1 * f, 0.15 * f, 0.25 * f, 0.65 * f, 1.1 * f,
                1800, 1600, 1400,
                true, true);
            setFuelModelRecord(164, "TU4",
                "Dwarf conifer understory (S)",
                0.5, 0.12, 8000, 8000,
                4.5 * f, 0, 0, 0, 2.0 * f,
                2300, 1800, 2000,
                true, true);
            setFuelModelRecord(165, "TU5",
                "Very high load, dry climate timber-shrub (S)",
                1.0, 0.25, 8000, 8000,
                4.0 * f, 4.0 * f, 3.0 * f, 0, 3.0 * f,
                1500, 1800, 750,
                true, true);
            // 166-170 reserved for future standard timber and understory models
            for (int i = 166; i <= 170; i++)
            {
                markAsReservedModel(i);
            }
            // 171-179 available for custom timber and understory models

            // Timber and litter
            // 180 available for custom timber and litter models
            setFuelModelRecord(181, "TL1",
                "Low load, compact conifer litter (S)",
                0.2, 0.30, 8000, 8000,
                1.0 * f, 2.2 * f, 3.6 * f, 0, 0,
                2000, 1800, 1600,
                true, true);
            setFuelModelRecord(182, "TL2",
                "Low load broadleaf litter (S)",
                0.2, 0.25, 8000, 8000,
                1.4 * f, 2.3 * f, 2.2 * f, 0, 0,
                2000, 1800, 1600,
                true, true);
            setFuelModelRecord(183, "TL3",
                "Moderate load conifer litter (S)",
                0.3, 0.20, 8000, 8000,
                0.5 * f, 2.2 * f, 2.8 * f, 0, 0,
                2000, 1800, 1600,
                true, true);
            setFuelModelRecord(184, "TL4",
                "Small downed logs (S)",
                0.4, 0.25, 8000, 8000,
                0.5 * f, 1.5 * f, 4.2 * f, 0, 0,
                2000, 1800, 1600,
                true, true);
            setFuelModelRecord(185, "TL5",
                "High load conifer litter (S)",
                0.6, 0.25, 8000, 8000,
                1.15 * f, 2.5 * f, 4.4 * f, 0, 0,
                2000, 1800, 160,
                true, true);
            setFuelModelRecord(186, "TL6",
                "High load broadleaf litter (S)",
                0.3, 0.25, 8000, 8000,
                2.4 * f, 1.2 * f, 1.2 * f, 0, 0,
                2000, 1800, 1600,
                true, true);
            setFuelModelRecord(187, "TL7",
                "Large downed logs (S)",
                0.4, 0.25, 8000, 8000,
                0.3 * f, 1.4 * f, 8.1 * f, 0, 0,
                2000, 1800, 1600,
                true, true);
            setFuelModelRecord(188, "TL8",
                "Long-needle litter (S)",
                0.3, 0.35, 8000, 8000,
                5.8 * f, 1.4 * f, 1.1 * f, 0, 0,
                1800, 1800, 1600,
                true, true);
            setFuelModelRecord(189, "TL9",
                "Very high load broadleaf litter (S)",
                0.6, 0.35, 8000, 8000,
                6.65 * f, 3.30 * f, 4.15 * f, 0, 0,
                1800, 1800, 1600,
                true, true);
            // 190-192 reserved for future standard timber and litter models
            for (int i = 190; i <= 192; i++)
            {
                markAsReservedModel(i);
            }

            // 193-199 available for custom timber and litter models

            // Slash and blowdown
            // 200 available for custom slash and blowdown model
            setFuelModelRecord(201, "SB1",
                "Low load activity fuel (S)",
                1.0, 0.25, 8000, 8000,
                1.5 * f, 3.0 * f, 11.0 * f, 0, 0,
                2000, 1800, 1600,
                true, true);
            setFuelModelRecord(202, "SB2",
                "Moderate load activity or low load blowdown (S)",
                1.0, 0.25, 8000, 8000,
                4.5 * f, 4.25 * f, 4.0 * f, 0, 0,
                2000, 1800, 1600,
                true, true);
            setFuelModelRecord(203, "SB3",
                "High load activity fuel or moderate load blowdown (S)",
                1.2, 0.25, 8000, 8000,
                5.5 * f, 2.75 * f, 3.0 * f, 0, 0,
                2000, 1800, 1600,
                true, true);
            setFuelModelRecord(204, "SB4",
                "High load blowdown (S)",
                2.7, 0.25, 8000, 8000,
                5.25 * f, 3.5 * f, 5.25 * f, 0, 0,
                2000, 1800, 1600,
                true, true);
            // 205-210 reserved for future slash and blowdown models
            for (int i = 205; i <= 210; i++)
            {
                markAsReservedModel(i);
            }
            // 211-219 available for custom  slash and blowdown models

            // 220 - 256 Available for custom models
        }

        // SetCustomFuelModel() is used by client code to define custom fuel types
        // The function can fail in the case of trying to set a record whose isReserve field is set to 1.
        // The return value is TRUE if successful, and FALSE in case of failure
        bool setCustomFuelModel(int fuelModelNumber, string code, string name,
            double fuelBedDepth, LengthUnits.LengthUnitsEnum lengthUnits, double moistureOfExtinctionDead,
            MoistureUnits.MoistureUnitsEnum moistureUnits, double heatOfCombustionDead, double heatOfCombustionLive,
            HeatOfCombustionUnits.HeatOfCombustionUnitsEnum heatOfCombustionUnits,
            double fuelLoadOneHour, double fuelLoadTenHour, double fuelLoadHundredHour, double fuelLoadLiveHerbaceous,
            double fuelLoadLiveWoody, LoadingUnits.LoadingUnitsEnum loadingUnits, double savrOneHour, double savrLiveHerbaceous,
            double savrLiveWoody, SurfaceAreaToVolumeUnits.SurfaceAreaToVolumeUnitsEnum savrUnits, bool isDynamic)
        {
            bool successStatus = false;

            fuelBedDepth = LengthUnits.toBaseUnits(fuelBedDepth, lengthUnits);

            moistureOfExtinctionDead = MoistureUnits.toBaseUnits(moistureOfExtinctionDead, moistureUnits);

            if (loadingUnits != LoadingUnits.LoadingUnitsEnum.PoundsPerSquareFoot)
            {
                fuelLoadOneHour = LoadingUnits.toBaseUnits(fuelLoadOneHour, loadingUnits);
                fuelLoadTenHour = LoadingUnits.toBaseUnits(fuelLoadTenHour, loadingUnits);
                fuelLoadHundredHour = LoadingUnits.toBaseUnits(fuelLoadHundredHour, loadingUnits);
                fuelLoadLiveHerbaceous = LoadingUnits.toBaseUnits(fuelLoadLiveHerbaceous, loadingUnits);
                fuelLoadLiveWoody = LoadingUnits.toBaseUnits(fuelLoadLiveWoody, loadingUnits);
            }

            code = code.Substring(0, 3);

            if (savrUnits != SurfaceAreaToVolumeUnits.SurfaceAreaToVolumeUnitsEnum.SquareFeetOverCubicFeet)
            {
                savrOneHour = SurfaceAreaToVolumeUnits.toBaseUnits(savrOneHour, savrUnits);
                savrLiveHerbaceous = SurfaceAreaToVolumeUnits.toBaseUnits(savrLiveHerbaceous, savrUnits);
                savrLiveWoody = SurfaceAreaToVolumeUnits.toBaseUnits(savrLiveWoody, savrUnits);
            }

            if (FuelModelArray_[fuelModelNumber].isReserved_ == false)
            {
                setFuelModelRecord(fuelModelNumber, code, name,
                    fuelBedDepth, moistureOfExtinctionDead, heatOfCombustionDead, heatOfCombustionLive,
                    fuelLoadOneHour, fuelLoadTenHour, fuelLoadHundredHour, fuelLoadLiveHerbaceous,
                    fuelLoadLiveWoody, savrOneHour, savrLiveHerbaceous, savrLiveWoody, isDynamic,
                    false);
                successStatus = true;
            }
            return successStatus;
        }

        bool clearCustomFuelModel(int fuelModelNumber)
        {
            bool successStatus = false;

            if (FuelModelArray_[fuelModelNumber].isReserved_)
            {
                successStatus = false;
            }
            else
            {
                initializeSingleFuelModelRecord(fuelModelNumber);
                successStatus = true;
            }
            return successStatus;
        }

        void markAsReservedModel(int fuelModelNumber)
        {
            FuelModelArray_[fuelModelNumber].isReserved_ = true;
        }

        public double getFuelbedDepth(int fuelModelNumber, LengthUnits.LengthUnitsEnum lengthUnits)
        {
            return LengthUnits.fromBaseUnits(FuelModelArray_[fuelModelNumber].fuelbedDepth_, lengthUnits);
        }

        public string getFuelCode(int fuelModelNumber)
        {
            return FuelModelArray_[fuelModelNumber].code_;
        }

        public string getFuelName(int fuelModelNumber)
        {
            return FuelModelArray_[fuelModelNumber].name_;
        }

        public double getMoistureOfExtinctionDead(int fuelModelNumber, MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            return MoistureUnits.fromBaseUnits(FuelModelArray_[fuelModelNumber].moistureOfExtinctionDead_, moistureUnits);
        }

        public double getHeatOfCombustionDead(int fuelModelNumber, HeatOfCombustionUnits.HeatOfCombustionUnitsEnum heatOfCombustionUnits)
        {
            return HeatOfCombustionUnits.fromBaseUnits(FuelModelArray_[fuelModelNumber].heatOfCombustionDead_, heatOfCombustionUnits);
        }

        public double getHeatOfCombustionLive(int fuelModelNumber, HeatOfCombustionUnits.HeatOfCombustionUnitsEnum heatOfCombustionUnits)
        {
            return HeatOfCombustionUnits.fromBaseUnits(FuelModelArray_[fuelModelNumber].heatOfCombustionLive_, heatOfCombustionUnits);
        }

        public double getFuelLoadOneHour(int fuelModelNumber, LoadingUnits.LoadingUnitsEnum loadingUnits)
        {
            return LoadingUnits.fromBaseUnits(FuelModelArray_[fuelModelNumber].fuelLoadOneHour_, loadingUnits);
        }

        public double getFuelLoadTenHour(int fuelModelNumber, LoadingUnits.LoadingUnitsEnum loadingUnits)
        {
            return LoadingUnits.fromBaseUnits(FuelModelArray_[fuelModelNumber].fuelLoadTenHour_, loadingUnits);
        }

        public double getFuelLoadHundredHour(int fuelModelNumber, LoadingUnits.LoadingUnitsEnum loadingUnits)
        {
            return LoadingUnits.fromBaseUnits(FuelModelArray_[fuelModelNumber].fuelLoadHundredHour_, loadingUnits);
        }

        public double getFuelLoadLiveHerbaceous(int fuelModelNumber, LoadingUnits.LoadingUnitsEnum loadingUnits)
        {
            return LoadingUnits.fromBaseUnits(FuelModelArray_[fuelModelNumber].fuelLoadLiveHerbaceous_, loadingUnits);
        }

        public double getFuelLoadLiveWoody(int fuelModelNumber, LoadingUnits.LoadingUnitsEnum loadingUnits)
        {
            return LoadingUnits.fromBaseUnits(FuelModelArray_[fuelModelNumber].fuelLoadLiveWoody_, loadingUnits);
        }

        public double getSavrOneHour(int fuelModelNumber, SurfaceAreaToVolumeUnits.SurfaceAreaToVolumeUnitsEnum savrUnits)
        {
            return SurfaceAreaToVolumeUnits.fromBaseUnits(FuelModelArray_[fuelModelNumber].savrOneHour_, savrUnits);
        }

        public double getSavrLiveHerbaceous(int fuelModelNumber, SurfaceAreaToVolumeUnits.SurfaceAreaToVolumeUnitsEnum savrUnits)
        {
            return SurfaceAreaToVolumeUnits.fromBaseUnits(FuelModelArray_[fuelModelNumber].savrLiveHerbaceous_, savrUnits);
        }

        public double getSavrLiveWoody(int fuelModelNumber, SurfaceAreaToVolumeUnits.SurfaceAreaToVolumeUnitsEnum savrUnits)
        {
            return SurfaceAreaToVolumeUnits.fromBaseUnits(FuelModelArray_[fuelModelNumber].savrLiveWoody_, savrUnits);
        }

        public bool getIsDynamic(int fuelModelNumber)
        {
            return FuelModelArray_[fuelModelNumber].isDynamic_;
        }

        public bool isFuelModelDefined(int fuelModelNumber)
        {
            if (fuelModelNumber <= 0 || fuelModelNumber > 256)
            {
                return false;
            }
            else
            {
                return FuelModelArray_[fuelModelNumber].isDefined_;
            }
        }
    }
}

