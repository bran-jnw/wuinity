//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using static WUIPlatform.Fire.Behave.BehaveUnits;

namespace WUIPlatform.Fire.Behave
{
    public class SurfaceTwoFuelModels
    {
        SurfaceFire surfaceFireSpread_;
        // Member arrays, stores data for each of the two fuel models
        int[] fuelModelNumber_ = new int[(int)SurfaceInputs.TwoFuelModelsContants.NUMBER_OF_MODELS];                      // fuel model number
        double[] coverageForFuelModel_ = new double[(int)SurfaceInputs.TwoFuelModelsContants.NUMBER_OF_MODELS];              // percent coverage of fuel model
        double[] rosForFuelModel_ = new double[(int)SurfaceInputs.TwoFuelModelsContants.NUMBER_OF_MODELS];                   // rate of spread
        double[] firelineIntensityForFuelModel_ = new double[(int)SurfaceInputs.TwoFuelModelsContants.NUMBER_OF_MODELS];     // fireline intensity
        double[] maxFlameLengthForFuelModel_ = new double[(int)SurfaceInputs.TwoFuelModelsContants.NUMBER_OF_MODELS];        // flame length in direction of max spread
        double[] flameLengthForFuelModel_ = new double[(int)SurfaceInputs.TwoFuelModelsContants.NUMBER_OF_MODELS];           // flame length
        double[] fuelbedDepthForFuelModel_ = new double[(int)SurfaceInputs.TwoFuelModelsContants.NUMBER_OF_MODELS];          // fuel bed depth in feet
        double[] effectiveWindSpeedForFuelModel_ = new double[(int)SurfaceInputs.TwoFuelModelsContants.NUMBER_OF_MODELS];    // effective wind speed
        double[] lengthToWidthRatioForFuelModel_ = new double[(int)SurfaceInputs.TwoFuelModelsContants.NUMBER_OF_MODELS];    // fire length-to-width ratio
        double[] reactionIntensityForFuelModel_ = new double[(int)SurfaceInputs.TwoFuelModelsContants.NUMBER_OF_MODELS];     // reaction intensity, 
        double[] heatPerUnitAreaForFuelModel_ = new double[(int)SurfaceInputs.TwoFuelModelsContants.NUMBER_OF_MODELS];       // heat per unit area
        double[] dirMaxSpreadForFuelModel_ = new double[(int)SurfaceInputs.TwoFuelModelsContants.NUMBER_OF_MODELS];          // direction of max spread
        double[] windAdjustmentFactorForFuelModel_ = new double[(int)SurfaceInputs.TwoFuelModelsContants.NUMBER_OF_MODELS];  // wind adjustment factor
        double[] midFlameWindSpeedForFuelModel_ = new double[(int)SurfaceInputs.TwoFuelModelsContants.NUMBER_OF_MODELS];     // wind speed at midflame
        double[] windSpeedLimitForFuelModel_ = new double[(int)SurfaceInputs.TwoFuelModelsContants.NUMBER_OF_MODELS];        // wind speed limit
        bool[] windLimitExceededForFuelModel_ = new bool[(int)SurfaceInputs.TwoFuelModelsContants.NUMBER_OF_MODELS];       // wind speed exceeded flag
        // Member variables
        TwoFuelModelsMethod twoFuelModelsMethod_;
        bool windLimitExceeded_;        // (flag)
        double reactionIntensity_;      // (Btu / ft^2 / min)
        double spreadRate_;             // (ft / min)
        double directionOfMaxSpread_;   // (clockwise from upslope or north)
        double effectiveWind_;          // (mi / h)
        double fuelbedDepth_;           // (ft)
        double heatPerUnitArea_;        // (Btu / ft^2)
        double midFlameWindSpeed_;
        double windSpeedLimit_;         // (mi / h)
        double windAdjustmentFactor_;
        double fireLineIntensity_;      // (Btu / ft / s)
        double flameLength_;            // (ft)
        double maxFlameLength_;         // flame length in direction of maximum spread (ft)
        double fireLengthToWidthRatio_;

        public SurfaceTwoFuelModels(SurfaceFire surfaceFireSpread)
        {
            surfaceFireSpread_ = surfaceFireSpread;
        }

        bool getWindLimitExceeded()
        {
            return windLimitExceeded_;
        }

        double getReactionIntensity()
        {
            return reactionIntensity_;
        }

        double getSpreadRate()
        {
            return spreadRate_;
        }

        double getDirectionOfMaxSpread()
        {
            return directionOfMaxSpread_;
        }

        double getEffectiveWind()
        {
            return effectiveWind_;
        }

        double getFuelbedDepth()
        {
            return fuelbedDepth_;
        }

        double getHeatPerUnitArea()
        {
            return heatPerUnitArea_;
        }

        double getMidFlameWindSpeed()
        {
            return midFlameWindSpeed_;
        }

        double getWindSpeedLimit()
        {
            return windSpeedLimit_;
        }

        double WindAdjustmentFactor()
        {
            return windAdjustmentFactor_;
        }

        double getFireLineIntensity()
        {
            return fireLineIntensity_;
        }

        double getflameLength()
        {
            return flameLength_;
        }

        double getFireLengthToWidthRatio()
        {
            return fireLengthToWidthRatio_;
        }

        public void calculateWeightedSpreadRate(TwoFuelModelsMethod twoFuelModelsMethod,
            int firstFuelModelNumber, double firstFuelModelCoverage, int secondFuelModelNumber,
            bool hasDirectionOfInterest, double directionOfInterest)
        {
            fuelModelNumber_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST] = firstFuelModelNumber;
            fuelModelNumber_[(int)SurfaceInputs.TwoFuelModelsContants.SECOND] = secondFuelModelNumber;

            coverageForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST] = firstFuelModelCoverage;
            coverageForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.SECOND] = 1 - coverageForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST];

            // Calculate fire outputs for each fuel model
            calculateFireOutputsForEachModel(hasDirectionOfInterest, directionOfInterest);

            //------------------------------------------------
            // Determine and store combined fuel model outputs
            //------------------------------------------------
            // Fire spread rate depends upon the weighting method...
            twoFuelModelsMethod_ = twoFuelModelsMethod;
            calculateSpreadRateBasedOnMethod();

            // The following assignments are based on Pat's rules:
            // If only 1 fuel is present (whether primary or secondary), use its values exclusively
            if (coverageForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST] > 0.999 || coverageForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.SECOND] > 0.999)
            {
                int i = ((int)coverageForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST] > 0.999) ? 0 : 1;
                reactionIntensity_ = reactionIntensityForFuelModel_[i];
                surfaceFireSpread_.setReactionIntensity(reactionIntensity_);

                directionOfMaxSpread_ = dirMaxSpreadForFuelModel_[i];
                surfaceFireSpread_.setDirectionOfMaxSpread(directionOfMaxSpread_);

                windAdjustmentFactor_ = windAdjustmentFactorForFuelModel_[i];
                surfaceFireSpread_.setWindAdjustmentFactor(windAdjustmentFactor_);

                midFlameWindSpeed_ = midFlameWindSpeedForFuelModel_[i];
                surfaceFireSpread_.setMidflameWindSpeed(midFlameWindSpeed_);

                effectiveWind_ = effectiveWindSpeedForFuelModel_[i];
                surfaceFireSpread_.setEffectiveWindSpeed(effectiveWind_);

                windSpeedLimit_ = windSpeedLimitForFuelModel_[i];
                surfaceFireSpread_.setWindSpeedLimit(windSpeedLimit_);

                windLimitExceeded_ = windLimitExceededForFuelModel_[i];
                surfaceFireSpread_.setIsWindLimitExceeded(windLimitExceeded_);

                fireLengthToWidthRatio_ = lengthToWidthRatioForFuelModel_[i];
                surfaceFireSpread_.setFireLengthToWidthRatio(fireLengthToWidthRatio_);

                heatPerUnitArea_ = heatPerUnitAreaForFuelModel_[i];
                surfaceFireSpread_.setHeatPerUnitArea(heatPerUnitArea_);

                fireLineIntensity_ = firelineIntensityForFuelModel_[i];
                surfaceFireSpread_.setFirelineIntensity(fireLineIntensity_);

                flameLength_ = flameLengthForFuelModel_[i];
                surfaceFireSpread_.setFlameLength(flameLength_);

                fuelbedDepth_ = fuelbedDepthForFuelModel_[i];
            }
            // Otherwise the wtd value depends upon Pat's criteria; could be wtd, min, max, or primary
            else
            {
                // Reaction intensity is the maximum of the two models
                reactionIntensity_ = (reactionIntensityForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST] >
                    reactionIntensityForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.SECOND]) ?
                    reactionIntensityForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST] : reactionIntensityForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.SECOND];
                surfaceFireSpread_.setReactionIntensity(reactionIntensity_);

                // Direction of maximum spread is for the FIRST (not necessarily dominant) fuel model
                directionOfMaxSpread_ = dirMaxSpreadForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST];
                surfaceFireSpread_.setDirectionOfMaxSpread(directionOfMaxSpread_);

                // Wind adjustment factor is for the FIRST (not necessarily dominant) fuel model
                windAdjustmentFactor_ = windAdjustmentFactorForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST]; // TODO: Incorporate Wind Adjustment Factor model in Behave
                //		surfaceFireSpread_.setWindAdjustmentFactor[windAdjustmentFactor_];

                // Midflame wind speed is for the FIRST (not necessarily dominant) fuel model
                midFlameWindSpeed_ = midFlameWindSpeedForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST]; // TODO:  Incorporate Wind Speed at Midflame model in Behave
                surfaceFireSpread_.setMidflameWindSpeed(midFlameWindSpeed_);

                // Effective wind speed is for the FIRST (not necessarily dominant) fuel model
                effectiveWind_ = effectiveWindSpeedForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST];
                surfaceFireSpread_.setEffectiveWindSpeed(effectiveWind_);

                // Maximum reliable wind speed is the minimum of the two models
                windSpeedLimit_ = (windSpeedLimitForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST] < windSpeedLimitForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.SECOND]) ?
                    windSpeedLimitForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST] : windSpeedLimitForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.SECOND];
                surfaceFireSpread_.setWindSpeedLimit(windSpeedLimit_);

                // If either wind limit is exceeded, set the flag
                windLimitExceeded_ = (windLimitExceededForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST] || windLimitExceededForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.SECOND]);
                surfaceFireSpread_.setIsWindLimitExceeded(windLimitExceeded_);

                // Fire length-to-width ratio is for the FIRST (not necessarily dominant) fuel model
                fireLengthToWidthRatio_ = lengthToWidthRatioForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST];
                surfaceFireSpread_.setFireLengthToWidthRatio(fireLengthToWidthRatio_);

                // Heat per unit area is the maximum of the two models
                heatPerUnitArea_ = (heatPerUnitAreaForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST] > heatPerUnitAreaForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.SECOND]) ?
                    heatPerUnitAreaForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST] : heatPerUnitAreaForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.SECOND];
                surfaceFireSpread_.setHeatPerUnitArea(heatPerUnitArea_);

                // Fireline intensity is the maximum of the two models
                fireLineIntensity_ = (firelineIntensityForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST] > firelineIntensityForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.SECOND]) ?
                    firelineIntensityForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST] : firelineIntensityForFuelModel_[1];
                surfaceFireSpread_.setFirelineIntensity(fireLineIntensity_);

                // Flame length is the maximum of the two models
                flameLength_ = (flameLengthForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST] > flameLengthForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.SECOND]) ?
                    flameLengthForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST] : flameLengthForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.SECOND];
                maxFlameLength_ = (maxFlameLengthForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST] > maxFlameLengthForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.SECOND]) ?
                    maxFlameLengthForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST] : maxFlameLengthForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.SECOND];
                surfaceFireSpread_.setFlameLength(flameLength_);

                // Fuel bed depth is the maximum of the two fuel bed depths
                fuelbedDepth_ = (fuelbedDepthForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST] > fuelbedDepthForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.SECOND]) ?
                    fuelbedDepthForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST] : fuelbedDepthForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.SECOND];
            }
            surfaceFireSpread_.forwardSpreadRate_ = spreadRate_;
        }

        double surfaceFireExpectedSpreadRate(ref double[] ros, ref double[] cov, int fuels,
            double lbRatio, int samples, int depth, int laterals)
        {
            // Initialize results
            double expectedRos = 0.0;

            // Create a RandFuel instance
            RandFuel randFuel = new RandFuel();

            // Mark says the cell size is irrelevant, but he sets it anyway.
            randFuel.setCellDimensions(10);

            // Get total fuel coverage
            double totalCov = 0.0;
            int i;
            for (i = 0; i < fuels; i++)
            {
                totalCov += cov[i];
            }
            // If no fuel coverage, we're done.
            if (totalCov <= 0.0)
            {
                return (expectedRos);
            }
            // Allocate the fuels
            if (!randFuel.allocFuels(fuels))
            {
                return (expectedRos);
            }
            // Normalize fuel coverages and store the fuel ros and cov
            for (i = 0; i < fuels; i++)
            {
                cov[i] = cov[i] / totalCov;
                randFuel.setFuelData(i, ros[i], cov[i]);
            }

            double maximumRos = 0.0;
            double harmonicRos = 0.0;    // only exists to match computeSpread2's method signature
            //harmonicRos = null;        // point harmonicRos to null
            // Compute the expected rate
            expectedRos = randFuel.computeSpread2(
                samples,            // columns
                depth,              // rows
                lbRatio,            // fire length-to-breadth ratio
                1,                  // always use 1 thread
                ref maximumRos,        // returned maximum spread rate
                ref harmonicRos,        // returned harmonic spread rate
                laterals,           // lateral extensions
                0);                 // less ignitions
            randFuel.freeFuels();

            // Determine expected spread rates.
            expectedRos *= maximumRos;

            return (expectedRos);
        }

        void calculateFireOutputsForEachModel(bool hasDirectionOfInterest, double directionOfInterest)
        {
            for (int i = 0; i < (int)SurfaceInputs.TwoFuelModelsContants.NUMBER_OF_MODELS; i++)
            {
                fuelbedDepthForFuelModel_[i] = surfaceFireSpread_.getFuelbedDepth();

                rosForFuelModel_[i] = surfaceFireSpread_.calculateForwardSpreadRate(fuelModelNumber_[i], hasDirectionOfInterest, directionOfInterest);

                reactionIntensityForFuelModel_[i] = surfaceFireSpread_.getReactionIntensity(HeatSourceAndReactionIntensityUnits.HeatSourceAndReactionIntensityUnitsEnum.BtusPerSquareFootPerMinute);
                dirMaxSpreadForFuelModel_[i] = surfaceFireSpread_.getDirectionOfMaxSpread();
                midFlameWindSpeedForFuelModel_[i] = surfaceFireSpread_.getMidflameWindSpeed();
                windAdjustmentFactorForFuelModel_[i] = surfaceFireSpread_.getWindAdjustmentFactor();
                effectiveWindSpeedForFuelModel_[i] = surfaceFireSpread_.getEffectiveWindSpeed();
                windSpeedLimitForFuelModel_[i] = surfaceFireSpread_.getWindSpeedLimit();
                windLimitExceededForFuelModel_[i] = surfaceFireSpread_.getIsWindLimitExceeded();
                firelineIntensityForFuelModel_[i] = surfaceFireSpread_.getFirelineIntensity();
                maxFlameLengthForFuelModel_[i] = surfaceFireSpread_.getMaxFlameLength();
                flameLengthForFuelModel_[i] = surfaceFireSpread_.getFlameLength();
                lengthToWidthRatioForFuelModel_[i] = surfaceFireSpread_.getFireLengthToWidthRatio();
                heatPerUnitAreaForFuelModel_[i] = surfaceFireSpread_.getHeatPerUnitArea();
            }
        }

        void calculateSpreadRateBasedOnMethod()
        {
            // If area weighted spread rate ...
            if (twoFuelModelsMethod_ == TwoFuelModelsMethod.Arithmetic)
            {
                spreadRate_ = (coverageForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST] * rosForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST]) +
                    (coverageForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.SECOND] * rosForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.SECOND]);
            }
            // else if harmonic mean spread rate...
            else if (twoFuelModelsMethod_ == TwoFuelModelsMethod.Harmonic)
            {
                if (rosForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST] > 0.000001 && rosForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.SECOND] > 0.000001)
                {
                    spreadRate_ = 1.0 / ((coverageForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST] / rosForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.FIRST]) +
                        (coverageForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.SECOND] / rosForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.SECOND]));
                }
            }
            // else if Finney's 2-dimensional spread rate...
            else if (twoFuelModelsMethod_ == TwoFuelModelsMethod.TwoFimensional)
            {
                //double lbRatio = lengthToWidthRatioForFuelModel_[TwoFuelModels.FIRST]; // get first fuel model's length-to-width ratio
                double lbRatio = lengthToWidthRatioForFuelModel_[(int)SurfaceInputs.TwoFuelModelsContants.SECOND]; // using fuel model's length-to-width ratio seems to agree with BehavePlus
                int samples = 2; // from behavePlus.xml
                int depth = 2; // from behavePlus.xml
                int laterals = 0; // from behavePlus.xml
                spreadRate_ = surfaceFireExpectedSpreadRate(ref rosForFuelModel_, ref coverageForFuelModel_, (int)SurfaceInputs.TwoFuelModelsContants.NUMBER_OF_MODELS, lbRatio, samples, depth, laterals);
            }
        }
    }
}

