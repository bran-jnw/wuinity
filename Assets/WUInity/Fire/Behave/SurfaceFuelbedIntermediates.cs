using System.Collections;
using System.Collections.Generic;
using static WUInity.Fire.MathWrap;
using static WUInity.Fire.BehaveUnits;

namespace WUInity.Fire
{
    public class SurfaceFuelbedIntermediates
    {
        FuelModelSet fuelModelSet_;      // Pointer to FuelModelSet object
        SurfaceInputs surfaceInputs_;    // Pointer to surfaceInputs object
        PalmettoGallberry palmettoGallberry_;
        WesternAspen westernAspen_;

        // Member variables
        int[] numberOfSizeClasses_ = new int[(int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES];                           // Number of size classes in the currently used fuel model
        double depth_;                                                                                      // Depth of fuelbed in feet
        double[] weightedMoisture_ = new double[(int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES];                           // Weighted moisture content for both live and dead fuels
        double[] totalSurfaceArea_ = new double[(int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES];                           // Total surface area for both live and dead fuels
        double[] weightedHeat_ = new double[(int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES];                               // Weighted heat content for both live and dead fuels
        double[] weightedSilica_ = new double[(int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES];                             // Weighted silica content for both live and dead fuels
        double[] weightedFuelLoad_ = new double[(int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES];                           // Weighted fuel loading for both live and dead fuels
        double[] moistureOfExtinction_ = new double[(int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES];                       // Moisture of extinction for both live and dead fuels
        double[] fractionOfTotalSurfaceArea_ = new double[(int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES];                 // Ratio of surface area to total surface area
        double[] fuelDensity_ = new double[(int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES];                                // Fuel density for live and dead fuels
        double[] totalLoadForLifeState_ = new double[(int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES];                      // Total fuel load for live and dead fuels
        double[] moistureDead_ = new double[(int)(int)SurfaceInputs.FuelConstants.MAX_PARTICLES];                                 // Moisture content for dead fuels by size class
        double[] moistureLive_ = new double[(int)(int)SurfaceInputs.FuelConstants.MAX_PARTICLES];                                 // Moisture content for live fuels by size class
        double[] loadDead_ = new double[(int)(int)SurfaceInputs.FuelConstants.MAX_PARTICLES];                                      // Fuel load for dead fuels by size class
        double[] loadLive_ = new double[(int)(int)SurfaceInputs.FuelConstants.MAX_PARTICLES];                                      // Fuel load for live fuels by size class
        double[] savrDead_ = new double[(int)(int)SurfaceInputs.FuelConstants.MAX_PARTICLES];                                      // Surface area to volume ratio for dead fuels by size class
        double[] savrLive_ = new double[(int)(int)SurfaceInputs.FuelConstants.MAX_PARTICLES];                                     // Surface area to volume ratio for live fuels by size class
        double[] surfaceAreaDead_ = new double[(int)(int)SurfaceInputs.FuelConstants.MAX_PARTICLES];                              // Surface area for dead size classes 
        double[] surfaceAreaLive_ = new double[(int)(int)SurfaceInputs.FuelConstants.MAX_PARTICLES];                              // Surface area for live size classes
        double[] heatDead_ = new double[(int)(int)SurfaceInputs.FuelConstants.MAX_PARTICLES];                                     // Heat of combustion for dead size classes
        double[] heatLive_ = new double[(int)(int)SurfaceInputs.FuelConstants.MAX_PARTICLES];                                     // Heat of combustion for live size classes
        double[] silicaEffectiveDead_ = new double[(int)(int)SurfaceInputs.FuelConstants.MAX_PARTICLES];                          // Effective silica constent for dead size classes
        double[] silicaEffectiveLive_ = new double[(int)(int)SurfaceInputs.FuelConstants.MAX_PARTICLES];                          // Effective silica constent for live size classes
        double[] fractionOfTotalSurfaceAreaDead_ = new double[(int)(int)SurfaceInputs.FuelConstants.MAX_PARTICLES];               // Fraction of surface area for dead size classes
        double[] fractionOfTotalSurfaceAreaLive_ = new double[(int)(int)SurfaceInputs.FuelConstants.MAX_PARTICLES];               // Fraction of surface area for live size classes
        double[] sizeSortedFractionOfSurfaceAreaDead_ = new double[(int)SurfaceInputs.FuelConstants.MAX_SAVR_SIZE_CLASSES];  // Intermediate fuel weighting values for dead fuels
        double[] sizeSortedFractionOfSurfaceAreaLive_ = new double[(int)SurfaceInputs.FuelConstants.MAX_SAVR_SIZE_CLASSES];  // Intermediate fuel weighting values for live fuels

        bool isUsingPalmettoGallberry_;
        bool isUsingWesternAspen_;

        int fuelModelNumber_;           // The number associated with the current fuel model being used
        double liveFuelMois_;           // Live fuel moisture content
        double liveFuelMext_;           // Live fuel moisture of extinction 
        double heatSink_;               // Rothermel 1972, Denominator of equation 52
        double sigma_;                  // Fuelbed characteristic SAVR, Rothermel 1972 
        double bulkDensity_;            // Ovendry bulk density in lbs/ft^2, Rothermale 1972, equation 40
        double packingRatio_;           // Packing ratio, Rothermel 1972, equation 31 
        double relativePackingRatio_;   // Packing ratio divided by the optimum packing ratio, Rothermel 1972, term in RHS equation 47
        double totalSilicaContent_;     // Total silica content in percent, Albini 1976, p. 91
        double propagatingFlux_;

        public SurfaceFuelbedIntermediates()
        {

        }

        public SurfaceFuelbedIntermediates(FuelModelSet fuelModelSet, SurfaceInputs surfaceInputs)
        {
            fuelModelSet_ = fuelModelSet;
            surfaceInputs_ = surfaceInputs;
            initializeMembers();
        }

        public SurfaceFuelbedIntermediates(SurfaceFuelbedIntermediates rhs)
        {
            memberwiseCopyAssignment(rhs);
        }

        /*SurfaceFuelbedIntermediates& operator=(const SurfaceFuelbedIntermediates& rhs)
        {
            if (this != &rhs)
            {
                memberwiseCopyAssignment(rhs);
            }
            return *this;
        }*/

        void memberwiseCopyAssignment(SurfaceFuelbedIntermediates rhs)
        {
            const int NUMBER_OF_LIVE_SIZE_CLASSES = 2;

            palmettoGallberry_ = rhs.palmettoGallberry_;
            westernAspen_ = rhs.westernAspen_;

            isUsingPalmettoGallberry_ = rhs.isUsingPalmettoGallberry_;
            isUsingWesternAspen_ = rhs.isUsingWesternAspen_;

            depth_ = rhs.depth_;
            relativePackingRatio_ = rhs.relativePackingRatio_;
            fuelModelNumber_ = rhs.fuelModelNumber_;
            liveFuelMois_ = rhs.liveFuelMois_;
            liveFuelMext_ = rhs.liveFuelMext_;
            sigma_ = rhs.sigma_;
            bulkDensity_ = rhs.bulkDensity_;
            packingRatio_ = rhs.packingRatio_;
            heatSink_ = rhs.heatSink_;
            totalSilicaContent_ = 0.0555;

            for (int i = 0; i < (int)SurfaceInputs.FuelConstants.MAX_SAVR_SIZE_CLASSES; i++)
            {
                sizeSortedFractionOfSurfaceAreaDead_[i] = rhs.sizeSortedFractionOfSurfaceAreaDead_[i];
                sizeSortedFractionOfSurfaceAreaLive_[i] = rhs.sizeSortedFractionOfSurfaceAreaLive_[i];
            }
            for (int i = 0; i < (int)(int)SurfaceInputs.FuelConstants.MAX_PARTICLES; i++)
            {
                fractionOfTotalSurfaceAreaDead_[i] = rhs.fractionOfTotalSurfaceAreaDead_[i];
                fractionOfTotalSurfaceAreaLive_[i] = rhs.fractionOfTotalSurfaceAreaLive_[i];
                surfaceAreaDead_[i] = rhs.surfaceAreaDead_[i];
                surfaceAreaLive_[i] = rhs.surfaceAreaLive_[i];
                moistureDead_[i] = rhs.moistureDead_[i];
                moistureLive_[i] = rhs.moistureLive_[i];
                loadDead_[i] = rhs.loadDead_[i];
                loadLive_[i] = rhs.loadLive_[i];
                savrDead_[i] = rhs.savrDead_[i];
                savrLive_[i] = rhs.savrLive_[i];
                heatDead_[i] = rhs.heatDead_[i];
                heatLive_[i] = rhs.heatLive_[i];
                silicaEffectiveDead_[i] = 0.01;
                if (i < NUMBER_OF_LIVE_SIZE_CLASSES)
                {
                    silicaEffectiveLive_[i] = 0.01;
                }
                else
                {
                    silicaEffectiveLive_[i] = 0.0;
                }
            }
            for (int i = 0; i < (int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES; i++)
            {
                numberOfSizeClasses_[i] = rhs.numberOfSizeClasses_[i];
                totalLoadForLifeState_[i] = rhs.totalLoadForLifeState_[i];
                fractionOfTotalSurfaceArea_[i] = rhs.fractionOfTotalSurfaceArea_[i];
                moistureOfExtinction_[i] = rhs.moistureOfExtinction_[i];
                totalSurfaceArea_[i] = rhs.totalSurfaceArea_[i];
                weightedMoisture_[i] = rhs.weightedMoisture_[i];
                weightedSilica_[i] = rhs.weightedSilica_[i];
                fuelDensity_[i] = 32; // Average density of dry fuel in lbs/ft^3, Albini 1976, p. 91
            }
        }

        ~SurfaceFuelbedIntermediates()
        {

        }

        public void calculateFuelbedIntermediates(int fuelModelNumber)
        {
            // TODO: Look into the creation of two new classes, FuelBed and Particle, these
            // new classes should aid in refactoring and also improve the overall design - WMC 08/2015

            // Rothermel spread equation based on BEHAVE source code,
            // support for dynamic fuel models added 10/13/2004

            //double ovendryFuelLoad = 0.0;           // Ovendry fuel loading, Rothermel 1972
            double optimumPackingRatio = 0.0;       // Optimum packing ratio, Rothermel 1972, equation 37
            bool isDynamic = false;                 // Whether or not fuel model is dynamic

            initializeMembers(); // Reset member variables to zero to forget previous state  

            fuelModelNumber_ = fuelModelNumber;

            setFuelLoad();

            setFuelbedDepth();

            countSizeClasses();

            setMoistureContent();

            setSAV();

            isDynamic = fuelModelSet_.getIsDynamic(fuelModelNumber_);
            if (isDynamic) // do the dynamic load transfer
            {
                dynamicLoadTransfer();
            }

            // Heat of combustion
            setHeatOfCombustion();

            // Fuel surface area weighting factors
            calculateFractionOfTotalSurfaceAreaForLifeStates();

            // Moisture of extinction
            setDeadFuelMoistureOfExtinction();
            calculateLiveMoistureOfExtinction();

            // Intermediate calculations, summing parameters by fuel component
            calculateCharacteristicSAVR();

            /* final calculations */
            double totalLoad = totalLoadForLifeState_[(int)(int)SurfaceInputs.FuelConstants.DEAD] + totalLoadForLifeState_[(int)(int)SurfaceInputs.FuelConstants.LIVE];

            bulkDensity_ = totalLoad / depth_;

            for (int lifeState = 0; lifeState < (int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES; lifeState++)
            {
                //packingRatio_ = totalLoad / (depth * ovendryFuelDensity);
                packingRatio_ += totalLoadForLifeState_[lifeState] / (depth_ * fuelDensity_[lifeState]);
            }

            optimumPackingRatio = 3.348 / pow(sigma_, 0.8189);
            relativePackingRatio_ = packingRatio_ / optimumPackingRatio;

            calculateHeatSink();
            calculatePropagatingFlux();
        }

        public void setFuelLoad()
        {
            if (isUsingPalmettoGallberry_)
            {
                // Calculate load values for Palmetto-Gallberry
                double ageOfRough = surfaceInputs_.getAgeOfRough();
                double heightOfUnderstory = surfaceInputs_.getHeightOfUnderstory();
                double palmettoCoverage = surfaceInputs_.getPalmettoCoverage();
                double overstoryBasalArea = surfaceInputs_.getOverstoryBasalArea();

                loadDead_[0] = palmettoGallberry_.calculatePalmettoGallberyDeadOneHourLoad(ageOfRough, heightOfUnderstory);
                loadDead_[1] = palmettoGallberry_.calculatePalmettoGallberyDeadTenHourLoad(ageOfRough, palmettoCoverage);
                loadDead_[2] = palmettoGallberry_.calculatePalmettoGallberyDeadFoliageLoad(ageOfRough, palmettoCoverage);
                loadDead_[3] = palmettoGallberry_.calculatePalmettoGallberyLitterLoad(ageOfRough, overstoryBasalArea);

                loadLive_[0] = palmettoGallberry_.calculatePalmettoGallberyLiveOneHourLoad(ageOfRough, heightOfUnderstory);
                loadLive_[1] = palmettoGallberry_.calculatePalmettoGallberyLiveTenHourLoad(ageOfRough, heightOfUnderstory);
                loadLive_[2] = palmettoGallberry_.calculatePalmettoGallberyLiveFoliageLoad(ageOfRough, palmettoCoverage, heightOfUnderstory);
                loadLive_[3] = 0.0;

                for (int i = 0; i < 3; i++)
                {
                    silicaEffectiveLive_[i] = 0.015;
                }
            }
            else if (isUsingWesternAspen_)
            {
                // Calculate load values for Western Aspen
                int aspenFuelModelNumber = surfaceInputs_.getAspenFuelModelNumber();
                double aspenCuringLevel = surfaceInputs_.getAspenCuringLevel();

                loadDead_[0] = westernAspen_.getAspenLoadDeadOneHour(aspenFuelModelNumber, aspenCuringLevel);
                loadDead_[1] = westernAspen_.getAspenLoadDeadTenHour(aspenFuelModelNumber);
                loadDead_[2] = 0.0;
                loadDead_[3] = 0.0;

                loadLive_[0] = westernAspen_.getAspenLoadLiveHerbaceous(aspenFuelModelNumber, aspenCuringLevel);
                loadLive_[1] = westernAspen_.getAspenLoadLiveWoody(aspenFuelModelNumber, aspenCuringLevel);
                loadLive_[2] = 0.0;
                loadLive_[3] = 0.0;
            }
            else
            {
                // Proceed as normal
                loadDead_[0] = fuelModelSet_.getFuelLoadOneHour(fuelModelNumber_, LoadingUnits.LoadingUnitsEnum.PoundsPerSquareFoot);
                loadDead_[1] = fuelModelSet_.getFuelLoadTenHour(fuelModelNumber_, LoadingUnits.LoadingUnitsEnum.PoundsPerSquareFoot);
                loadDead_[2] = fuelModelSet_.getFuelLoadHundredHour(fuelModelNumber_, LoadingUnits.LoadingUnitsEnum.PoundsPerSquareFoot);
                loadDead_[3] = 0.0;

                loadLive_[0] = fuelModelSet_.getFuelLoadLiveHerbaceous(fuelModelNumber_, LoadingUnits.LoadingUnitsEnum.PoundsPerSquareFoot);
                loadLive_[1] = fuelModelSet_.getFuelLoadLiveWoody(fuelModelNumber_, LoadingUnits.LoadingUnitsEnum.PoundsPerSquareFoot);
                loadLive_[2] = 0.0;
                loadLive_[3] = 0.0;
            }
        }

        public void setMoistureContent()
        {
            if (isUsingPalmettoGallberry_)
            {
                moistureDead_[0] = surfaceInputs_.getMoistureOneHour(MoistureUnits.MoistureUnitsEnum.Fraction);
                moistureDead_[1] = surfaceInputs_.getMoistureTenHour(MoistureUnits.MoistureUnitsEnum.Fraction);
                moistureDead_[2] = surfaceInputs_.getMoistureOneHour(MoistureUnits.MoistureUnitsEnum.Fraction);
                moistureDead_[3] = surfaceInputs_.getMoistureHundredHour(MoistureUnits.MoistureUnitsEnum.Fraction);

                moistureLive_[0] = surfaceInputs_.getMoistureLiveWoody(MoistureUnits.MoistureUnitsEnum.Fraction);
                moistureLive_[1] = surfaceInputs_.getMoistureLiveWoody(MoistureUnits.MoistureUnitsEnum.Fraction);
                moistureLive_[2] = surfaceInputs_.getMoistureLiveHerbaceous(MoistureUnits.MoistureUnitsEnum.Fraction);
                moistureLive_[3] = 0.0;
            }
            else
            {
                for (int i = 0; i < (int)(int)SurfaceInputs.FuelConstants.MAX_PARTICLES; i++)
                {
                    moistureDead_[i] = 0;
                    moistureLive_[i] = 0;
                }

                moistureDead_[0] = surfaceInputs_.getMoistureOneHour(MoistureUnits.MoistureUnitsEnum.Fraction);
                moistureDead_[1] = surfaceInputs_.getMoistureTenHour(MoistureUnits.MoistureUnitsEnum.Fraction);
                moistureDead_[2] = surfaceInputs_.getMoistureHundredHour(MoistureUnits.MoistureUnitsEnum.Fraction);
                moistureDead_[3] = surfaceInputs_.getMoistureOneHour(MoistureUnits.MoistureUnitsEnum.Fraction);

                moistureLive_[0] = surfaceInputs_.getMoistureLiveHerbaceous(MoistureUnits.MoistureUnitsEnum.Fraction);
                moistureLive_[1] = surfaceInputs_.getMoistureLiveWoody(MoistureUnits.MoistureUnitsEnum.Fraction);
            }
        }

        public void setDeadFuelMoistureOfExtinction()
        {
            if (isUsingPalmettoGallberry_)
            {
                moistureOfExtinction_[(int)(int)SurfaceInputs.FuelConstants.DEAD] = palmettoGallberry_.getMoistureOfExtinctionDead();
            }
            else if (isUsingWesternAspen_)
            {
                moistureOfExtinction_[(int)(int)SurfaceInputs.FuelConstants.DEAD] = westernAspen_.getAspenMoistureOfExtinctionDead();
            }
            else
            {
                moistureOfExtinction_[(int)(int)SurfaceInputs.FuelConstants.DEAD] = fuelModelSet_.getMoistureOfExtinctionDead(fuelModelNumber_, MoistureUnits.MoistureUnitsEnum.Fraction);
            }
        }

        public void setFuelbedDepth()
        {
            if (isUsingPalmettoGallberry_)
            {
                double heightOfUnderstory = surfaceInputs_.getHeightOfUnderstory();
                depth_ = palmettoGallberry_.calculatePalmettoGallberyFuelBedDepth(heightOfUnderstory);
            }
            else if (isUsingWesternAspen_)
            {
                int aspenFuelModelNumber = surfaceInputs_.getAspenFuelModelNumber();
                depth_ = westernAspen_.getAspenFuelBedDepth(aspenFuelModelNumber);
            }
            else
            {
                depth_ = fuelModelSet_.getFuelbedDepth(fuelModelNumber_, LengthUnits.LengthUnitsEnum.Feet);
            }
        }

        public void setSAV()
        {
            if (isUsingPalmettoGallberry_)
            {
                // Special values for Palmetto-Gallberry
                savrDead_[0] = 350.0;
                savrDead_[1] = 140.0;
                savrDead_[2] = 2000.0;
                savrDead_[3] = 2000.0; // TODO: find appropriate savr for palmetto-gallberry litter

                savrLive_[0] = 350.0;
                savrLive_[1] = 140.0;
                savrLive_[2] = 2000.0;
                savrLive_[3] = 0.0;
            }
            else if (isUsingWesternAspen_)
            {
                // Calculate SAVR values for Western Aspen
                int aspenFuelModelNumber = surfaceInputs_.getAspenFuelModelNumber();
                double aspenCuringLevel = surfaceInputs_.getAspenCuringLevel();

                savrDead_[0] = westernAspen_.getAspenSavrDeadOneHour(aspenFuelModelNumber, aspenCuringLevel);
                savrDead_[1] = westernAspen_.getAspenSavrDeadTenHour();
                savrDead_[2] = 0.0;
                savrDead_[3] = 0.0;

                savrLive_[0] = westernAspen_.getAspenSavrLiveHerbaceous();
                savrLive_[1] = westernAspen_.getAspenSavrLiveWoody(aspenFuelModelNumber, aspenCuringLevel);
                savrLive_[2] = 0.0;
                savrLive_[3] = 0.0;
            }
            else
            {
                // Proceed as normal
                savrDead_[0] = fuelModelSet_.getSavrOneHour(fuelModelNumber_, SurfaceAreaToVolumeUnits.SurfaceAreaToVolumeUnitsEnum.SquareFeetOverCubicFeet);
                savrDead_[1] = 109.0;
                savrDead_[2] = 30.0;
                savrDead_[3] = fuelModelSet_.getSavrLiveHerbaceous(fuelModelNumber_, SurfaceAreaToVolumeUnits.SurfaceAreaToVolumeUnitsEnum.SquareFeetOverCubicFeet);

                savrLive_[0] = fuelModelSet_.getSavrLiveHerbaceous(fuelModelNumber_, SurfaceAreaToVolumeUnits.SurfaceAreaToVolumeUnitsEnum.SquareFeetOverCubicFeet);
                savrLive_[1] = fuelModelSet_.getSavrLiveWoody(fuelModelNumber_, SurfaceAreaToVolumeUnits.SurfaceAreaToVolumeUnitsEnum.SquareFeetOverCubicFeet);
                savrLive_[2] = 0.0;
                savrLive_[3] = 0.0;
            }
        }

        public void setHeatOfCombustion()
        {
            const int NUMBER_OF_LIVE_SIZE_CLASSES = 3;

            double heatOfCombustionDead = 0.0;
            double heatOfCombustionLive = 0.0;

            if (isUsingPalmettoGallberry_)
            {
                heatOfCombustionDead = palmettoGallberry_.getHeatOfCombustionDead();
                heatOfCombustionLive = palmettoGallberry_.getHeatOfCombustionLive();
            }
            else if (isUsingWesternAspen_)
            {
                heatOfCombustionDead = westernAspen_.getAspenHeatOfCombustionDead();
                heatOfCombustionLive = westernAspen_.getAspenHeatOfCombustionLive();
            }
            else
            {
                heatOfCombustionDead = fuelModelSet_.getHeatOfCombustionDead(fuelModelNumber_, HeatOfCombustionUnits.HeatOfCombustionUnitsEnum.BtusPerPound);
                heatOfCombustionLive = fuelModelSet_.getHeatOfCombustionLive(fuelModelNumber_, HeatOfCombustionUnits.HeatOfCombustionUnitsEnum.BtusPerPound);
            }

            for (int i = 0; i < (int)(int)SurfaceInputs.FuelConstants.MAX_PARTICLES; i++)
            {
                heatDead_[i] = heatOfCombustionDead;
                if (i < NUMBER_OF_LIVE_SIZE_CLASSES)
                {
                    heatLive_[i] = heatOfCombustionLive;
                }
                else
                {
                    heatLive_[i] = 0.0;
                }
            }
        }

        public void calculatePropagatingFlux()
        {
            propagatingFlux_ = (sigma_ < 1.0e-07)
                ? (0.0)
                : (exp((0.792 + 0.681 * sqrt(sigma_)) * (packingRatio_ + 0.1)) / (192.0 + 0.2595 * sigma_));
        }

        public void calculateHeatSink()
        {
            double[] qigLive = new double[(int)SurfaceInputs.FuelConstants.MAX_PARTICLES]; // Heat of preigintion for live fuels
            double[] qigDead = new double[(int)SurfaceInputs.FuelConstants.MAX_PARTICLES]; // Heat of preigintion for dead fuels

            // Initialize variables
            heatSink_ = 0;
            for (int i = 0; i < (int)SurfaceInputs.FuelConstants.MAX_PARTICLES; i++)
            {
                qigLive[i] = 0.0;
                qigDead[i] = 0.0;
            }

            for (int i = 0; i < (int)SurfaceInputs.FuelConstants.MAX_PARTICLES; i++)
            {
                if (savrDead_[i] > 1.0e-07)
                {
                    qigDead[i] = 250.0 + 1116.0 * moistureDead_[i];
                    heatSink_ += fractionOfTotalSurfaceArea_[(int)(int)SurfaceInputs.FuelConstants.DEAD] * fractionOfTotalSurfaceAreaDead_[i] * qigDead[i] * exp(-138.0 / savrDead_[i]);
                }
                if (savrLive_[i] > 1.0e-07)
                {
                    qigLive[i] = 250.0 + 1116.0 * moistureLive_[i];
                    heatSink_ += fractionOfTotalSurfaceArea_[(int)(int)SurfaceInputs.FuelConstants.LIVE] * fractionOfTotalSurfaceAreaLive_[i] * qigLive[i] * exp(-138.0 / savrLive_[i]);
                }
            }
            heatSink_ *= bulkDensity_;
        }

        public void calculateCharacteristicSAVR()
        {
            double[] wnLive = new double[(int)SurfaceInputs.FuelConstants.MAX_PARTICLES];         // Net fuel loading for live fuels, Rothermel 1972, equation 24	
            double[] wnDead = new double[(int)SurfaceInputs.FuelConstants.MAX_PARTICLES];             // Net fuel loading for dead fuels, Rothermel 1972, equation 24

            double[] weightedSavr = new double[(int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES]; // Weighted SAVR for i-th categort (live/dead)

            // Initialize Accumulated values
            sigma_ = 0.0;
            for (int i = 0; i < (int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES; i++)
            {
                totalLoadForLifeState_[i] = 0.0;
                weightedHeat_[i] = 0.0;
                weightedSilica_[i] = 0.0;
                weightedMoisture_[i] = 0.0;
                weightedSavr[i] = 0.0;
                weightedFuelLoad_[i] = 0.0;
            }
            for (int i = 0; i < (int)SurfaceInputs.FuelConstants.MAX_PARTICLES; i++)
            {
                wnDead[i] = 0.0;
                wnLive[i] = 0.0;
            }

            if (isUsingPalmettoGallberry_)
            {
                totalSilicaContent_ = 0.030;
            }

            for (int i = 0; i < (int)SurfaceInputs.FuelConstants.MAX_PARTICLES; i++)
            {
                if (savrDead_[i] > 1.0e-07)
                {
                    wnDead[i] = loadDead_[i] * (1.0 - totalSilicaContent_); // Rothermel 1972, equation 24
                    weightedHeat_[(int)SurfaceInputs.FuelConstants.DEAD] += fractionOfTotalSurfaceAreaDead_[i] * heatDead_[i]; // weighted heat content
                    weightedSilica_[(int)SurfaceInputs.FuelConstants.DEAD] += fractionOfTotalSurfaceAreaDead_[i] * silicaEffectiveDead_[i]; // weighted silica content
                    weightedMoisture_[(int)SurfaceInputs.FuelConstants.DEAD] += fractionOfTotalSurfaceAreaDead_[i] * moistureDead_[i]; // weighted moisture content
                    weightedSavr[(int)SurfaceInputs.FuelConstants.DEAD] += fractionOfTotalSurfaceAreaDead_[i] * savrDead_[i]; // weighted SAVR
                    totalLoadForLifeState_[(int)SurfaceInputs.FuelConstants.DEAD] += loadDead_[i];
                }
                if (savrLive_[i] > 1.0e-07)
                {
                    wnLive[i] = loadLive_[i] * (1.0 - totalSilicaContent_); // Rothermel 1972, equation 24
                    weightedHeat_[(int)SurfaceInputs.FuelConstants.LIVE] += fractionOfTotalSurfaceAreaLive_[i] * heatLive_[i]; // weighted heat content
                    weightedSilica_[(int)SurfaceInputs.FuelConstants.LIVE] += fractionOfTotalSurfaceAreaLive_[i] * silicaEffectiveLive_[i]; // weighted silica content
                    weightedMoisture_[(int)SurfaceInputs.FuelConstants.LIVE] += fractionOfTotalSurfaceAreaLive_[i] * moistureLive_[i]; // weighted moisture content
                    weightedSavr[(int)SurfaceInputs.FuelConstants.LIVE] += fractionOfTotalSurfaceAreaLive_[i] * savrLive_[i]; // weighted SAVR
                    totalLoadForLifeState_[(int)SurfaceInputs.FuelConstants.LIVE] += loadLive_[i];
                }
                weightedFuelLoad_[(int)SurfaceInputs.FuelConstants.DEAD] += sizeSortedFractionOfSurfaceAreaDead_[i] * wnDead[i];
                weightedFuelLoad_[(int)SurfaceInputs.FuelConstants.LIVE] += sizeSortedFractionOfSurfaceAreaLive_[i] * wnLive[i];
            }

            for (int lifeState = 0; lifeState < (int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES; lifeState++)
            {
                sigma_ += fractionOfTotalSurfaceArea_[lifeState] * weightedSavr[lifeState];
            }
        }

        public void countSizeClasses()
        {
            // count number of fuels
            for (int i = 0; i < (int)SurfaceInputs.FuelConstants.MAX_DEAD_SIZE_CLASSES; i++)
            {
                //bran-jnw, fixed this, used to be if(loadDead_[i])
                if (loadDead_[i] != 0.0)
                {
                    numberOfSizeClasses_[(int)SurfaceInputs.FuelConstants.DEAD]++;
                }
            }
            for (int i = 0; i < (int)SurfaceInputs.FuelConstants.MAX_LIVE_SIZE_CLASSES; i++)
            {
                //bran-jnw, fixed this, used to be if(loadLive_[i])
                if (loadLive_[i] != 0.0)
                {
                    numberOfSizeClasses_[(int)SurfaceInputs.FuelConstants.LIVE]++;
                }
            }
            if (numberOfSizeClasses_[(int)SurfaceInputs.FuelConstants.LIVE] > 0)
            {
                numberOfSizeClasses_[(int)SurfaceInputs.FuelConstants.LIVE] = (int)SurfaceInputs.FuelConstants.MAX_LIVE_SIZE_CLASSES;  // Boost to max number
            }
            if (numberOfSizeClasses_[(int)SurfaceInputs.FuelConstants.DEAD] > 0)
            {
                numberOfSizeClasses_[(int)SurfaceInputs.FuelConstants.DEAD] = (int)SurfaceInputs.FuelConstants.MAX_DEAD_SIZE_CLASSES;  // Boost to max number
            }
        }

        public void dynamicLoadTransfer()
        {
            if (moistureLive_[0] < 0.30)
            {
                loadDead_[3] = loadLive_[0];
                loadLive_[0] = 0.0;
            }
            else if (moistureLive_[0] <= 1.20)
            {
                //loadDead_[3] = loadLive_[0] * (1.20 - moistureLive_[0]) / 0.9;
                loadDead_[3] = loadLive_[0] * (1.333 - 1.11 * moistureLive_[0]); // To keep consistant with BehavePlus
                loadLive_[0] -= loadDead_[3];
            }
        }

        public void calculateFractionOfTotalSurfaceAreaForLifeStates()
        {
            double[] summedFractionOfTotalSurfaceArea = new double[(int)SurfaceInputs.FuelConstants.MAX_SAVR_SIZE_CLASSES];   // Intermediate weighting factors for each size class

            for (int lifeState = 0; lifeState < (int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES; lifeState++)
            {
                if (numberOfSizeClasses_[lifeState] != 0)
                {
                    calculateTotalSurfaceAreaForLifeState(lifeState);
                    calculateFractionOfTotalSurfaceAreaForSizeClasses(lifeState);
                }
                for (int i = 0; i < (int)SurfaceInputs.FuelConstants.MAX_SAVR_SIZE_CLASSES; i++)
                {
                    summedFractionOfTotalSurfaceArea[i] = 0.0;
                }
                if (lifeState == (int)SurfaceInputs.FuelConstants.DEAD)
                {
                    sumFractionOfTotalSurfaceAreaBySizeClass(fractionOfTotalSurfaceAreaDead_, savrDead_, summedFractionOfTotalSurfaceArea);
                    assignFractionOfTotalSurfaceAreaBySizeClass(savrDead_, summedFractionOfTotalSurfaceArea, sizeSortedFractionOfSurfaceAreaDead_);
                }
                if (lifeState == (int)SurfaceInputs.FuelConstants.LIVE)
                {
                    sumFractionOfTotalSurfaceAreaBySizeClass(fractionOfTotalSurfaceAreaLive_, savrLive_, summedFractionOfTotalSurfaceArea);
                    assignFractionOfTotalSurfaceAreaBySizeClass(savrLive_, summedFractionOfTotalSurfaceArea, sizeSortedFractionOfSurfaceAreaLive_);
                }
            }

            fractionOfTotalSurfaceArea_[(int)SurfaceInputs.FuelConstants.DEAD] = totalSurfaceArea_[(int)SurfaceInputs.FuelConstants.DEAD] / (totalSurfaceArea_[(int)SurfaceInputs.FuelConstants.DEAD] +
                totalSurfaceArea_[(int)SurfaceInputs.FuelConstants.LIVE]);
            fractionOfTotalSurfaceArea_[(int)SurfaceInputs.FuelConstants.LIVE] = 1.0 - fractionOfTotalSurfaceArea_[(int)SurfaceInputs.FuelConstants.DEAD];
        }

        public void calculateTotalSurfaceAreaForLifeState(int lifeState)
        {
            for (int i = 0; i < (int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES; i++)
            {
                totalSurfaceArea_[lifeState] = 0.0;
            }

            bool isUsingPalmettoGallbery = surfaceInputs_.isUsingPalmettoGallberry();
            if (isUsingPalmettoGallbery)
            {
                fuelDensity_[(int)SurfaceInputs.FuelConstants.DEAD] = 30.0;
                fuelDensity_[(int)SurfaceInputs.FuelConstants.LIVE] = 46.0;
            }

            for (int i = 0; i < numberOfSizeClasses_[lifeState]; i++)
            {
                if (lifeState == (int)SurfaceInputs.FuelConstants.DEAD)
                {
                    //surfaceAreaDead_[i] = loadDead_[i] * savrDead_[i] / OVENDRY_FUEL_DENSITY;
                    surfaceAreaDead_[i] = loadDead_[i] * savrDead_[i] / fuelDensity_[(int)SurfaceInputs.FuelConstants.DEAD];
                    totalSurfaceArea_[lifeState] += surfaceAreaDead_[i];
                }
                if (lifeState == (int)SurfaceInputs.FuelConstants.LIVE)
                {
                    //surfaceAreaLive_[i] = loadLive_[i] * savrLive_[i] / OVENDRY_FUEL_DENSITY;
                    surfaceAreaLive_[i] = loadLive_[i] * savrLive_[i] / fuelDensity_[(int)SurfaceInputs.FuelConstants.LIVE];
                    totalSurfaceArea_[lifeState] += surfaceAreaLive_[i];
                }
            }
        }

        public void calculateFractionOfTotalSurfaceAreaForSizeClasses(int lifeState)
        {
            for (int i = 0; i < numberOfSizeClasses_[lifeState]; i++)
            {
                if (totalSurfaceArea_[lifeState] > 1.0e-7)
                {
                    if (lifeState == (int)SurfaceInputs.FuelConstants.DEAD)
                    {
                        fractionOfTotalSurfaceAreaDead_[i] = surfaceAreaDead_[i] / totalSurfaceArea_[(int)SurfaceInputs.FuelConstants.DEAD];
                    }
                    if (lifeState == (int)SurfaceInputs.FuelConstants.LIVE)
                    {
                        fractionOfTotalSurfaceAreaLive_[i] = surfaceAreaLive_[i] / totalSurfaceArea_[(int)SurfaceInputs.FuelConstants.LIVE];
                    }
                }
                else
                {
                    if (lifeState == (int)SurfaceInputs.FuelConstants.DEAD)
                    {
                        fractionOfTotalSurfaceAreaDead_[i] = 0.0;
                    }
                    if (lifeState == (int)SurfaceInputs.FuelConstants.LIVE)
                    {
                        fractionOfTotalSurfaceAreaLive_[i] = 0.0;
                    }
                }
            }
        }

        //bran-jnw
        /*void sumFractionOfTotalSurfaceAreaBySizeClass(
        const double fractionOfTotalSurfaceAreaDeadOrLive[(int)SurfaceInputs.FuelConstants.MAX_PARTICLES],
            const double savrDeadOrLive[(int)SurfaceInputs.FuelConstants.MAX_PARTICLES], double summedFractionOfTotalSurfaceArea[(int)SurfaceInputs.FuelConstants.MAX_PARTICLES])*/
        public void sumFractionOfTotalSurfaceAreaBySizeClass(double[] fractionOfTotalSurfaceAreaDeadOrLive, double[] savrDeadOrLive, double[] summedFractionOfTotalSurfaceArea)
        {
            // savrDeadOrLive[] is an alias for savrDead[] or savrLive[], which is determined by the method caller 
            // fractionOfTotalSurfaceAreaDeadOrLive  is an alias for fractionOfTotalSurfaceAreaDead[] or  fractionOfTotalSurfaceAreaLive[], 
            // which is determined by the method caller 

            for (int i = 0; i < (int)SurfaceInputs.FuelConstants.MAX_PARTICLES; i++)
            {
                summedFractionOfTotalSurfaceArea[i] = 0.0;
            }

            for (int i = 0; i < (int)SurfaceInputs.FuelConstants.MAX_PARTICLES; i++)
            {
                if (savrDeadOrLive[i] >= 1200.0)
                {
                    summedFractionOfTotalSurfaceArea[0] += fractionOfTotalSurfaceAreaDeadOrLive[i];
                }
                else if (savrDeadOrLive[i] >= 192.0)
                {
                    summedFractionOfTotalSurfaceArea[1] += fractionOfTotalSurfaceAreaDeadOrLive[i];
                }
                else if (savrDeadOrLive[i] >= 96.0)
                {
                    summedFractionOfTotalSurfaceArea[2] += fractionOfTotalSurfaceAreaDeadOrLive[i];
                }
                else if (savrDeadOrLive[i] >= 48.0)
                {
                    summedFractionOfTotalSurfaceArea[3] += fractionOfTotalSurfaceAreaDeadOrLive[i];
                }
                else if (savrDeadOrLive[i] >= 16.0)
                {
                    summedFractionOfTotalSurfaceArea[4] += fractionOfTotalSurfaceAreaDeadOrLive[i];
                }
            }
        }

        //bran-jnw
        /*void assignFractionOfTotalSurfaceAreaBySizeClass(const double savrDeadOrLive[(int)SurfaceInputs.FuelConstants.MAX_PARTICLES],
            const double summedFractionOfTotalSurfaceArea[(int)SurfaceInputs.FuelConstants.MAX_PARTICLES],
            double sizeSortedFractionOfSurfaceAreaDeadOrLive[(int)SurfaceInputs.FuelConstants.MAX_PARTICLES])*/
        public void assignFractionOfTotalSurfaceAreaBySizeClass(double[] savrDeadOrLive, double[] summedFractionOfTotalSurfaceArea, double[] sizeSortedFractionOfSurfaceAreaDeadOrLive)
        {
            // savrDeadOrLive[] is an alias for savrDead[] or savrLive[], which is determined by the method caller 
            // sizeSortedFractionOfSurfaceAreaDeadOrLive[] is an alias for sizeSortedFractionOfSurfaceAreaDead_[] or sizeSortedFractionOfSurfaceAreaLive_[], 
            // which is determined by the method caller

            for (int i = 0; i < (int)SurfaceInputs.FuelConstants.MAX_PARTICLES; i++)
            {
                if (savrDeadOrLive[i] >= 1200.0)
                {
                    sizeSortedFractionOfSurfaceAreaDeadOrLive[i] = summedFractionOfTotalSurfaceArea[0];
                }
                else if (savrDeadOrLive[i] >= 192.0)
                {
                    sizeSortedFractionOfSurfaceAreaDeadOrLive[i] = summedFractionOfTotalSurfaceArea[1];
                }
                else if (savrDeadOrLive[i] >= 96.0)
                {
                    sizeSortedFractionOfSurfaceAreaDeadOrLive[i] = summedFractionOfTotalSurfaceArea[2];
                }
                else if (savrDeadOrLive[i] >= 48.0)
                {
                    sizeSortedFractionOfSurfaceAreaDeadOrLive[i] = summedFractionOfTotalSurfaceArea[3];
                }
                else if (savrDeadOrLive[i] >= 16.0)
                {
                    sizeSortedFractionOfSurfaceAreaDeadOrLive[i] = summedFractionOfTotalSurfaceArea[4];
                }
                else
                {
                    sizeSortedFractionOfSurfaceAreaDeadOrLive[i] = 0.0;
                }
            }
        }

        public void calculateLiveMoistureOfExtinction()
        {
            if (numberOfSizeClasses_[(int)SurfaceInputs.FuelConstants.LIVE] != 0)
            {
                double fineDead = 0.0;                  // Fine dead fuel load
                double fineLive = 0.0;                  // Fine dead fuel load
                double fineFuelsWeightingFactor = 0.0;  // Exponential weighting factors for fine fuels, Albini 1976, p. 89
                double weightedMoistureFineDead = 0.0;  // Weighted sum of find dead moisture content
                double fineDeadMoisture = 0.0;          // Fine dead moisture content, Albini 1976, p. 89
                double fineDeadOverFineLive = 0.0;      // Ratio of fine fuel loadings, dead/living, Albini 1976, p. 89

                for (int i = 0; i < (int)SurfaceInputs.FuelConstants.MAX_PARTICLES; i++)
                {
                    fineFuelsWeightingFactor = 0.0;
                    if (savrDead_[i] > 1.0e-7)
                    {
                        fineFuelsWeightingFactor = loadDead_[i] * exp(-138.0 / savrDead_[i]);
                    }
                    fineDead += fineFuelsWeightingFactor;
                    weightedMoistureFineDead += fineFuelsWeightingFactor * moistureDead_[i];
                }
                if (fineDead > 1.0e-07)
                {
                    fineDeadMoisture = weightedMoistureFineDead / fineDead;
                }
                for (int i = 0; i < numberOfSizeClasses_[(int)SurfaceInputs.FuelConstants.LIVE]; i++)
                {
                    if (savrLive_[i] > 1.0e-07)
                    {
                        fineLive += loadLive_[i] * exp(-500.0 / savrLive_[i]);
                    }
                }
                if (fineLive > 1.0e-7)
                {
                    fineDeadOverFineLive = fineDead / fineLive;
                }
                moistureOfExtinction_[(int)SurfaceInputs.FuelConstants.LIVE] = (2.9 * fineDeadOverFineLive *
                    (1.0 - fineDeadMoisture / moistureOfExtinction_[(int)SurfaceInputs.FuelConstants.DEAD])) - 0.226;
                if (moistureOfExtinction_[(int)SurfaceInputs.FuelConstants.LIVE] < moistureOfExtinction_[(int)SurfaceInputs.FuelConstants.DEAD])
                {
                    moistureOfExtinction_[(int)SurfaceInputs.FuelConstants.LIVE] = moistureOfExtinction_[(int)SurfaceInputs.FuelConstants.DEAD];
                }
            }
        }

        public void initializeMembers()
        {
            const int NUMBER_OF_LIVE_SIZE_CLASSES = 2;

            if(palmettoGallberry_ == null)
            {
                palmettoGallberry_ = new PalmettoGallberry();
            }
            palmettoGallberry_.initializeMembers();
            if (westernAspen_== null)
            {
                westernAspen_ = new WesternAspen();
            }
            westernAspen_.initializeMembers();

            isUsingPalmettoGallberry_ = surfaceInputs_.isUsingPalmettoGallberry();
            isUsingWesternAspen_ = surfaceInputs_.isUsingWesternAspen();

            depth_ = 0.0;
            relativePackingRatio_ = 0.0;
            fuelModelNumber_ = 0;
            liveFuelMois_ = 0.0;
            liveFuelMext_ = 0.0;
            sigma_ = 0.0;
            bulkDensity_ = 0.0;
            packingRatio_ = 0.0;
            heatSink_ = 0.0;
            totalSilicaContent_ = 0.0555;

            for (int i = 0; i < (int)SurfaceInputs.FuelConstants.MAX_SAVR_SIZE_CLASSES; i++)
            {
                sizeSortedFractionOfSurfaceAreaDead_[i] = 0;
                sizeSortedFractionOfSurfaceAreaLive_[i] = 0;
            }
            for (int i = 0; i < (int)SurfaceInputs.FuelConstants.MAX_PARTICLES; i++)
            {
                fractionOfTotalSurfaceAreaDead_[i] = 0.0;
                fractionOfTotalSurfaceAreaLive_[i] = 0.0;
                surfaceAreaDead_[i] = 0.0;
                surfaceAreaLive_[i] = 0.0;
                moistureDead_[i] = 0.0;
                moistureLive_[i] = 0.0;
                loadDead_[i] = 0.0;
                loadLive_[i] = 0.0;
                savrDead_[i] = 0.0;
                savrLive_[i] = 0.0;
                heatDead_[i] = 0.0;
                heatLive_[i] = 0.0;
                silicaEffectiveDead_[i] = 0.01;
                if (i < NUMBER_OF_LIVE_SIZE_CLASSES)
                {
                    silicaEffectiveLive_[i] = 0.01;
                }
                else
                {
                    silicaEffectiveLive_[i] = 0.0;
                }
            }
            for (int i = 0; i < (int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES; i++)
            {
                numberOfSizeClasses_[i] = 0;
                totalLoadForLifeState_[i] = 0.0;
                fractionOfTotalSurfaceArea_[i] = 0.0;
                moistureOfExtinction_[i] = 0.0;
                totalSurfaceArea_[i] = 0.0;
                weightedMoisture_[i] = 0.0;
                weightedSilica_[i] = 0.0;
                fuelDensity_[i] = 32; // Average density of dry fuel in lbs/ft^3, Albini 1976, p. 91
            }
        }

        public double getFuelbedDepth()
        {
            return fuelModelSet_.getFuelbedDepth(fuelModelNumber_, LengthUnits.LengthUnitsEnum.Feet);
        }

        public double getBulkDensity()
        {
            return bulkDensity_;
        }

        public double getPackingRatio()
        {
            return packingRatio_;
        }

        public double getPropagatingFlux()
        {
            return propagatingFlux_;
        }

        public double getRelativePackingRatio()
        {
            return relativePackingRatio_;
        }

        public double getSigma()
        {
            return sigma_;
        }

        public double getHeatSink()
        {
            return heatSink_;
        }

        public double getWeightedMoistureByLifeState(int lifeState)
        {
            return weightedMoisture_[lifeState];
        }

        public double getMoistureOfExtinctionByLifeState(int lifeState)
        {
            return moistureOfExtinction_[lifeState];
        }

        public double getWeightedHeatByLifeState(int lifeState)
        {
            return weightedHeat_[lifeState];
        }

        public double getWeightedSilicaByLifeState(int lifeState)
        {
            return weightedSilica_[lifeState];
        }

        public double getWeightedFuelLoadByLifeState(int lifeState)
        {
            return weightedFuelLoad_[lifeState];
        }

        public double getPalmettoGallberyDeadOneHourLoad()
        {
            return palmettoGallberry_.getPalmettoGallberyDeadOneHourLoad();
        }

        public double getPalmettoGallberyDeadTenHourLoad()
        {
            return palmettoGallberry_.getPalmettoGallberyDeadTenHourLoad();
        }

        public double getPalmettoGallberyDeadFoliageLoad()
        {
            return palmettoGallberry_.getPalmettoGallberyDeadFoliageLoad();
        }

        public double getPalmettoGallberyFuelBedDepth()
        {
            return palmettoGallberry_.getPalmettoGallberyFuelBedDepth();
        }

        public double getPalmettoGallberyLitterLoad()
        {
            return palmettoGallberry_.getPalmettoGallberyLitterLoad();
        }

        public double getPalmettoGallberyLiveOneHourLoad()
        {
            return palmettoGallberry_.getPalmettoGallberyLiveOneHourLoad();
        }

        public double getPalmettoGallberyLiveTenHourLoad()
        {
            return palmettoGallberry_.getPalmettoGallberyLiveTenHourLoad();
        }

        public double getPalmettoGallberyLiveFoliageLoad()
        {
            return palmettoGallberry_.getPalmettoGallberyLiveFoliageLoad();
        }

        public double getAspenMortality()
        {
            return westernAspen_.getAspenMortality();
        }
    }
}

