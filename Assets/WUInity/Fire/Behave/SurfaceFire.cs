/******************************************************************************
*
* Project:  CodeBlocks
* Purpose:  Class for calculating values associated with surface fires used
*           in Rothermel Model
* Author:   William Chatham <wchatham@fs.fed.us>
* Credits:  Some of the code in the corresponding cpp file is, in part or in
*           whole, from BehavePlus5 source originally authored by Collin D.
*           Bevins and is used with or without modification.
*
*******************************************************************************
*
* THIS SOFTWARE WAS DEVELOPED AT THE ROCKY MOUNTAIN RESEARCH STATION (RMRS)
* MISSOULA FIRE SCIENCES LABORATORY BY EMPLOYEES OF THE FEDERAL GOVERNMENT
* IN THE COURSE OF THEIR OFFICIAL DUTIES. PURSUANT TO TITLE 17 SECTION 105
* OF THE UNITED STATES CODE, THIS SOFTWARE IS NOT SUBJECT TO COPYRIGHT
* PROTECTION AND IS IN THE PUBLIC DOMAIN. RMRS MISSOULA FIRE SCIENCES
* LABORATORY ASSUMES NO RESPONSIBILITY WHATSOEVER FOR ITS USE BY OTHER
* PARTIES,  AND MAKES NO GUARANTEES, EXPRESSED OR IMPLIED, ABOUT ITS QUALITY,
* RELIABILITY, OR ANY OTHER CHARACTERISTIC.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
* OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
* THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
* FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
* DEALINGS IN THE SOFTWARE.
*
******************************************************************************/
using static WUInity.Fire.MathWrap;
using static WUInity.Fire.BehaveUnits;

namespace WUInity.Fire
{
    public class SurfaceFire
    {
        // Pointers and references to other objects
        FuelModelSet fuelModelSet_;
        SurfaceInputs surfaceInputs_;
        FireSize size_;
        SurfaceFuelbedIntermediates surfaceFuelbedIntermediates_;
        SurfaceFireReactionIntensity surfaceFireReactionIntensity_;

        // Member variables
        bool isWindLimitExceeded_;
        double directionOfInterest_;
        double effectiveWindSpeed_;
        double windSpeedLimit_;
        double phiS_;                                           // Slope factor, Rothermel 1972, equation 51
        double phiW_;                                           // Wind factor, Rothermel 1972, equation 47
        double windB_;                                          // Rothermel 1972, Equation 49
        double windC_;                                          // Rothermel 1972, Equation 48
        double windE_;                                          // Rothermel 1972, Equation 50
        double directionOfMaxSpread_;                           // Direction of max fire spread in degrees clockwise from upslope
        double noWindNoSlopeSpreadRate_;                        // No-wind-no-slope fire spread rate, Rothermel 1972, equation 52
        public double forwardSpreadRate_;                       // Maximum rate of fire spread rate, Rothermel 1972, equation 52
        double spreadRateInDirectionOfInterest_;                // spreadRateInDirectionOfInterest
        double heatPerUnitArea_;                                // Heat per unit area (Btu/ft^2)
        double fireLengthToWidthRatio_;
        double residenceTime_;
        double reactionIntensity_;
        double firelineIntensity_;
        double maxFlameLength_;                                 // Flame length computed from spread rate in max direction, used in SAFETY
        double flameLength_;
        double backingSpreadRate_;

        double midflameWindSpeed_;
        double windAdjustmentFactor_;
        WindAdjustmentFactorShelterMethod windAdjustmentFactorShelterMethod_;
        double canopyCrownFraction_;

        double aspenMortality_;

        //bran-jnw
        /*SurfaceFire()
        : surfaceFireReactionIntensity_()
        {

        }*/
        public SurfaceFire()
        {
            surfaceFireReactionIntensity_ = new SurfaceFireReactionIntensity();
        }

        //bran-jnw
        /*SurfaceFire(const FuelModelSet& fuelModelSet, const SurfaceInputs& surfaceInputs, 
            FireSize& size)
        : surfaceFuelbedIntermediates_(fuelModelSet, surfaceInputs),
        surfaceFireReactionIntensity_(surfaceFuelbedIntermediates_)
        {
            fuelModelSet_ = &fuelModelSet;
            size_ = &size;
            surfaceInputs_ = &surfaceInputs;
            initializeMembers();
        }*/
        public SurfaceFire(FuelModelSet fuelModelSet, SurfaceInputs surfaceInputs, FireSize size)        
        {
            surfaceFuelbedIntermediates_ = new SurfaceFuelbedIntermediates(fuelModelSet, surfaceInputs);
            surfaceFireReactionIntensity_ = new SurfaceFireReactionIntensity(surfaceFuelbedIntermediates_);
            fuelModelSet_ = fuelModelSet;
            size_ = size;
            surfaceInputs_ = surfaceInputs;
            initializeMembers();
        }

        //bran-jnw
        // Copy Ctor
        /*SurfaceFire(const SurfaceFire& rhs)
    :   surfaceFireReactionIntensity_()
        {
            memberwiseCopyAssignment(rhs);
        }*/

        public SurfaceFire(SurfaceFire rhs)
        {
            surfaceFireReactionIntensity_ = new SurfaceFireReactionIntensity();
            memberwiseCopyAssignment(rhs);
        }

        /*SurfaceFire& operator=(const SurfaceFire& rhs)
        {
            if (this != &rhs)
            {
                    memberwiseCopyAssignment(rhs);
                }
            return *this;
        }*/

        void memberwiseCopyAssignment(SurfaceFire rhs)
        {
            surfaceFireReactionIntensity_ = rhs.surfaceFireReactionIntensity_;
            surfaceFuelbedIntermediates_ = rhs.surfaceFuelbedIntermediates_;

            isWindLimitExceeded_ = rhs.isWindLimitExceeded_;
            effectiveWindSpeed_ = rhs.effectiveWindSpeed_;
            windSpeedLimit_ = rhs.windSpeedLimit_;
            phiS_ = rhs.phiS_;
            phiW_ = rhs.phiW_;
            windB_ = rhs.windB_;
            windC_ = rhs.windC_;
            windE_ = rhs.windE_;

            directionOfMaxSpread_ = rhs.directionOfMaxSpread_;
            noWindNoSlopeSpreadRate_ = rhs.noWindNoSlopeSpreadRate_;
            forwardSpreadRate_ = rhs.forwardSpreadRate_;
            spreadRateInDirectionOfInterest_ = rhs.spreadRateInDirectionOfInterest_;
            heatPerUnitArea_ = rhs.heatPerUnitArea_;
            fireLengthToWidthRatio_ = rhs.fireLengthToWidthRatio_;
            residenceTime_ = rhs.residenceTime_;
            reactionIntensity_ = rhs.reactionIntensity_;
            firelineIntensity_ = rhs.firelineIntensity_;
            flameLength_ = rhs.flameLength_;
            maxFlameLength_ = rhs.maxFlameLength_;
            backingSpreadRate_ = rhs.backingSpreadRate_;

            midflameWindSpeed_ = rhs.midflameWindSpeed_;
            windAdjustmentFactor_ = rhs.windAdjustmentFactor_;
            windAdjustmentFactorShelterMethod_ = rhs.windAdjustmentFactorShelterMethod_;
            canopyCrownFraction_ = rhs.canopyCrownFraction_;

            aspenMortality_ = rhs.aspenMortality_;
        }

        public double calculateNoWindNoSlopeSpreadRate(double reactionIntensity, double propagatingFlux, double heatSink)
        {
            noWindNoSlopeSpreadRate_ = (heatSink < 1.0e-07)
                ? (0.0)
                : (reactionIntensity * propagatingFlux / heatSink);
            return noWindNoSlopeSpreadRate_;
        }

        void calculateResidenceTime()
        {
            double sigma = surfaceFuelbedIntermediates_.getSigma();
            residenceTime_ = ((sigma < 1.0e-07)
                ? (0.0)
                : (384.0 / sigma));
        }

        void calculateFireFirelineIntensity(double forwardSpreadRate)
        {
            double secondsPerMinute = 60.0; // for converting feet per minute to feet per second
            firelineIntensity_ = forwardSpreadRate * reactionIntensity_ * (residenceTime_ / secondsPerMinute);
        }

        public double calculateFlameLength(double firelineIntensity)
        {
            double flameLength = ((firelineIntensity < 1.0e-07)
                ? (0.0)
                : (0.45 * pow(firelineIntensity, 0.46)));
            return flameLength;
        }

        public void skipCalculationForZeroLoad()
        {
            initializeMembers();
        }

        void calculateFlameLength()
        {
            // Byram 1959, Albini 1976
            flameLength_ = ((firelineIntensity_ < 1.0e-07)
                ? (0.0)
                : (0.45 * pow(firelineIntensity_, 0.46)));
        }

        public double calculateForwardSpreadRate(int fuelModelNumber, bool hasDirectionOfInterest = false, double directionOfInterest = -1.0)
        {
            // Reset member variables to prepare for next calculation
            initializeMembers();

            // Calculate fuelbed intermediates
            surfaceFuelbedIntermediates_.calculateFuelbedIntermediates(fuelModelNumber);

            // Get needed fuelbed intermediates
            double propagatingFlux = surfaceFuelbedIntermediates_.getPropagatingFlux();
            double heatSink = surfaceFuelbedIntermediates_.getHeatSink();
            reactionIntensity_ = surfaceFireReactionIntensity_.calculateReactionIntensity();

            // Calculate Wind and Slope Factors
            calculateMidflameWindSpeed();
            calculateWindFactor();
            calculateSlopeFactor();

            // No-wind no-slope spread rate and parameters
            noWindNoSlopeSpreadRate_ = calculateNoWindNoSlopeSpreadRate(reactionIntensity_, propagatingFlux, heatSink);
            forwardSpreadRate_ = noWindNoSlopeSpreadRate_;

            // Slope and wind adjusted spread rate
            calculateWindSpeedLimit();

            forwardSpreadRate_ = noWindNoSlopeSpreadRate_ * (1.0 + phiW_ + phiS_);

            // Calculate spread rate in optimal direction.
            calculateDirectionOfMaxSpread();
            calculateEffectiveWindSpeed();
            // maximum windspeed effect on ros
            if (effectiveWindSpeed_ > windSpeedLimit_)
            {
                applyWindSpeedLimit();
            }

            effectiveWindSpeed_ = SpeedUnits.fromBaseUnits(effectiveWindSpeed_, SpeedUnits.SpeedUnitsEnum.MilesPerHour);
            calculateResidenceTime();

            double elapsedTime = surfaceInputs_.getElapsedTime();

            // Calculate fire ellipse and related properties
            size_.calculateFireBasicDimensions(effectiveWindSpeed_, SpeedUnits.SpeedUnitsEnum.MilesPerHour, forwardSpreadRate_, SpeedUnits.SpeedUnitsEnum.FeetPerMinute);

            fireLengthToWidthRatio_ = size_.getFireLengthToWidthRatio();

            backingSpreadRate_ = size_.getBackingSpreadRate(SpeedUnits.SpeedUnitsEnum.FeetPerMinute);

            calculateFireFirelineIntensity(forwardSpreadRate_);
            calculateFlameLength();
            maxFlameLength_ = getFlameLength(); // Used by SAFETY Module
            if (hasDirectionOfInterest) // If needed, calculate spread rate in arbitrary direction of interest
            {
                spreadRateInDirectionOfInterest_ = calculateSpreadRateAtVector(directionOfInterest);
                calculateFireFirelineIntensity(spreadRateInDirectionOfInterest_);
                calculateFlameLength();
            }

            calculateHeatPerUnitArea();

            return forwardSpreadRate_;
        }

        public double calculateSpreadRateAtVector(double directionOfInterest)
        {
            double rosVector = forwardSpreadRate_;
            double eccentricity_ = size_.getEccentricity();
            if (forwardSpreadRate_ != 0.0) // if forward spread rate is not zero
            {
                // Calculate the fire spread rate in this azimuth
                // if it deviates more than a tenth degree from the maximum azimuth

                // Calcualte beta: the angle between the direction of max spread and the direction of interest
                double beta = fabs(directionOfMaxSpread_ - directionOfInterest);

                // Calculate the fire spread rate in this azimuth
                // if it deviates more than a tenth degree from the maximum azimuth
                if (beta > 180.0)
                {
                    beta = (360.0 - beta);
                }
                if (fabs(beta) > 0.1)
                {
                    double radians = beta * M_PI / 180.0;
                    rosVector = forwardSpreadRate_ * (1.0 - eccentricity_) / (1.0 - eccentricity_ * cos(radians));
                }
            }
            return rosVector;
        }

        void applyWindSpeedLimit()
        {
            isWindLimitExceeded_ = true;
            effectiveWindSpeed_ = windSpeedLimit_;

            double relativePackingRatio = surfaceFuelbedIntermediates_.getRelativePackingRatio();
            double phiEffectiveWind = windC_ * pow(windSpeedLimit_, windB_) * pow(relativePackingRatio, -windE_);
            forwardSpreadRate_ = noWindNoSlopeSpreadRate_ * (1 + phiEffectiveWind);
        }

        void calculateEffectiveWindSpeed()
        {
            double phiEffectiveWind = forwardSpreadRate_ / noWindNoSlopeSpreadRate_ - 1.0;
            double relativePackingRatio = surfaceFuelbedIntermediates_.getRelativePackingRatio();
            effectiveWindSpeed_ = pow(((phiEffectiveWind * pow(relativePackingRatio, windE_)) / windC_), 1.0 / windB_);
        }

        void calculateDirectionOfMaxSpread()
        {
            //Calculate directional components (direction is clockwise from upslope)
            double correctedWindDirection = surfaceInputs_.getWindDirection();

            WindAndSpreadOrientationMode windAndSpreadOrientation = surfaceInputs_.getWindAndSpreadOrientationMode();
            if (windAndSpreadOrientation == WindAndSpreadOrientationMode.RelativeToNorth)
            {
                double aspect = surfaceInputs_.getAspect();
                correctedWindDirection -= aspect;
            }

            double windDirRadians = correctedWindDirection * M_PI / 180.0;

            // Calculate wind and slope rate
            double slopeRate = noWindNoSlopeSpreadRate_ * phiS_;
            double windRate = noWindNoSlopeSpreadRate_ * phiW_;

            // Calculate coordinate components
            double x = slopeRate + (windRate * cos(windDirRadians));
            double y = windRate * sin(windDirRadians);
            double rateVector = sqrt((x * x) + (y * y));

            // Apply wind and slope rate to spread rate
            forwardSpreadRate_ = noWindNoSlopeSpreadRate_ + rateVector;

            // Calculate azimuth
            double azimuth = 0.0;
            azimuth = atan2(y, x);

            // Recalculate azimuth in degrees
            azimuth *= 180.0 / M_PI;

            // If angle is negative, add 360 degrees
            if (azimuth < -1.0e-20)
            {
                azimuth += 360.0;
            }

            // Undocumented hack from BehavePlus code
            if (fabs(azimuth) < 0.5)
            {
                azimuth = 0.0;
            }

            // Convert azimuth to be relative to North if necessary
            if (windAndSpreadOrientation == WindAndSpreadOrientationMode.RelativeToNorth)
            {
                azimuth = convertDirectionOfSpreadToRelativeToNorth(azimuth);
                while (azimuth >= 360.0)
                {
                    azimuth -= 360.0;
                }
            }

            // Azimuth is the direction of maximum spread
            directionOfMaxSpread_ = azimuth;
        }

        void calculateHeatPerUnitArea()
        {
            heatPerUnitArea_ = reactionIntensity_ * residenceTime_;
        }

        void calculateWindSpeedLimit()
        {
            windSpeedLimit_ = 0.9 * reactionIntensity_;
            if (phiS_ > 0.0)
            {
                if (phiS_ > windSpeedLimit_)
                {
                    // Enforce wind speed limit
                    phiS_ = windSpeedLimit_;
                }
            }
        }

        void calculateWindFactor()
        {
            double sigma = surfaceFuelbedIntermediates_.getSigma();
            double relativePackingRatio = surfaceFuelbedIntermediates_.getRelativePackingRatio();

            windC_ = 7.47 * exp(-0.133 * pow(sigma, 0.55));
            windB_ = 0.02526 * pow(sigma, 0.54);
            windE_ = 0.715 * exp(-0.000359 * sigma);

            // midflameWindSpeed is in ft/min
            if (midflameWindSpeed_ < 1.0e-07)
            {
                phiW_ = 0.0;
            }
            else
            {
                phiW_ = pow(midflameWindSpeed_, windB_) * windC_ * pow(relativePackingRatio, -windE_);
            }
        }

        void calculateWindAdjustmentFactor()
        {
            WindAdjustmentFactor windAdjustmentFactor = new WindAdjustmentFactor();

            double canopyCover = surfaceInputs_.getCanopyCover();
            double canopyHeight = surfaceInputs_.getCanopyHeight();
            double crownRatio = surfaceInputs_.getCrownRatio();
            double fuelbedDepth = surfaceFuelbedIntermediates_.getFuelbedDepth();

            WindAdjustmentFactorCalculationMethod windAdjustmentFactorCalculationMethod =
                surfaceInputs_.getWindAdjustmentFactorCalculationMethod();
            if (windAdjustmentFactorCalculationMethod == WindAdjustmentFactorCalculationMethod.UseCrownRatio)
            {
                windAdjustmentFactor_ = windAdjustmentFactor.calculateWindAdjustmentFactorWithCrownRatio(canopyCover, canopyHeight, crownRatio, fuelbedDepth);
            }
            else if ((windAdjustmentFactorCalculationMethod == WindAdjustmentFactorCalculationMethod.DontUseCrownRatio))
            {
                windAdjustmentFactor_ = windAdjustmentFactor.calculateWindAdjustmentFactorWithoutCrownRatio(canopyCover, canopyHeight, fuelbedDepth);
            }
            windAdjustmentFactorShelterMethod_ = windAdjustmentFactor.getWindAdjustmentFactorShelterMethod();
        }

        public void calculateMidflameWindSpeed()
        {
            double windSpeed = surfaceInputs_.getWindSpeed();

            WindHeightInputMode windHeightInputMode = surfaceInputs_.getWindHeightInputMode();

            if (windHeightInputMode == WindHeightInputMode.DirectMidflame)
            {
                midflameWindSpeed_ = windSpeed;
            }
            else if (windHeightInputMode == WindHeightInputMode.TwentyFoot || windHeightInputMode == WindHeightInputMode.TenMeter)
            {
                if (windHeightInputMode == WindHeightInputMode.TenMeter)
                {
                    windSpeed /= 1.15;
                }
                WindAdjustmentFactorCalculationMethod windAdjustmentFactorCalculationMethod =
                    surfaceInputs_.getWindAdjustmentFactorCalculationMethod();
                if (windAdjustmentFactorCalculationMethod == WindAdjustmentFactorCalculationMethod.UserInput)
                {
                    windAdjustmentFactor_ = surfaceInputs_.getUserProvidedWindAdjustmentFactor();
                }
                else
                {
                    calculateWindAdjustmentFactor();
                }
                midflameWindSpeed_ = windAdjustmentFactor_ * windSpeed;
            }
        }

        void calculateSlopeFactor()
        {
            double packingRatio = surfaceFuelbedIntermediates_.getPackingRatio();
            // Slope factor
            double slope = surfaceInputs_.getSlope();
            double slopex = tan((double)slope / 180.0 * M_PI); // convert from degrees to tan
            phiS_ = 5.275 * pow(packingRatio, -0.3) * (slopex * slopex);
        }

        public double getFuelbedDepth()
        {
            int fuelModelNumber = surfaceInputs_.getFuelModelNumber();
            double fuelbedDepth = fuelModelSet_.getFuelbedDepth(fuelModelNumber, LengthUnits.LengthUnitsEnum.Feet);
            return fuelbedDepth;
        }

        public double getSpreadRate()
        {
            return forwardSpreadRate_;
        }

        public double getSpreadRateInDirectionOfInterest()
        {
            return spreadRateInDirectionOfInterest_;
        }

        public double getDirectionOfMaxSpread()
        {
            return directionOfMaxSpread_;
        }

        public double getEffectiveWindSpeed()
        {
            return effectiveWindSpeed_;
        }

        public double convertDirectionOfSpreadToRelativeToNorth(double directionOfMaxSpreadFromUpslope)
        {
            double dirMaxSpreadRelativeToNorth = directionOfMaxSpreadFromUpslope;
            double aspect = surfaceInputs_.getAspect();
            dirMaxSpreadRelativeToNorth += aspect + 180.0; // spread direction is now relative to north
            return dirMaxSpreadRelativeToNorth;
        }

        public double getFirelineIntensity()
        {
            return firelineIntensity_;
        }

        public double getFlameLength()
        {
            return flameLength_;
        }

        public double getMaxFlameLength()
        {
            return maxFlameLength_;
        }

        public double getFireLengthToWidthRatio()
        {
            return size_.getFireLengthToWidthRatio();
        }

        public double getFireEccentricity()
        {
            return  size_.getEccentricity();
        }

        public double getHeatPerUnitArea()
        {
            return heatPerUnitArea_;
        }

        public double getResidenceTime()
        {
            return residenceTime_;
        }

        public double getWindSpeedLimit()
        {
            return windSpeedLimit_;
        }

        public double getMidflameWindSpeed()
        {
            return midflameWindSpeed_;
        }

        public double getSlopeFactor()
        {
            return phiS_;
        }

        public double getHeatSink(HeatSinkUnits.HeatSinkUnitsEnum heatSinkUnits)
        {
            return HeatSinkUnits.fromBaseUnits(surfaceFuelbedIntermediates_.getHeatSink(), heatSinkUnits);
        }

        public double getBulkDensity()
        {
            return surfaceFuelbedIntermediates_.getBulkDensity();
        }

        public double getReactionIntensity(HeatSourceAndReactionIntensityUnits.HeatSourceAndReactionIntensityUnitsEnum reactiontionIntensityUnits)
        {
            return HeatSourceAndReactionIntensityUnits.fromBaseUnits(reactionIntensity_, reactiontionIntensityUnits);
        }

        public double getWindAdjustmentFactor()
        {
            return windAdjustmentFactor_;
        }

        public bool getIsWindLimitExceeded()
        {
            return isWindLimitExceeded_;
        }

        public void setDirectionOfMaxSpread(double directionOFMaxSpread)
        {
            directionOfMaxSpread_ = directionOFMaxSpread;
        }

        public void setEffectiveWindSpeed(double effectiveWindSpeed)
        {
            effectiveWindSpeed_ = effectiveWindSpeed;
        }

        public void setFirelineIntensity(double firelineIntensity)
        {
            firelineIntensity_ = firelineIntensity;
        }

        public void setFlameLength(double flameLength)
        {
            flameLength_ = flameLength;
        }

        public void setFireLengthToWidthRatio(double lengthToWidthRatio)
        {
            fireLengthToWidthRatio_ = lengthToWidthRatio;
        }

        public void setResidenceTime(double residenceTime)
        {
            residenceTime_ = residenceTime;
        }

        public void setWindSpeedLimit(double windSpeedLimit)
        {
            windSpeedLimit_ = windSpeedLimit;
        }

        public void setReactionIntensity(double reactionIntensity)
        {
            reactionIntensity_ = reactionIntensity;
        }

        public void setHeatPerUnitArea(double heatPerUnitArea)
        {
            heatPerUnitArea_ = heatPerUnitArea;
        }

        public void setIsWindLimitExceeded(bool isWindLimitExceeded)
        {
            isWindLimitExceeded_ = isWindLimitExceeded;
        }

        public void setWindAdjustmentFactor(double windAdjustmentFactor)
        {
            windAdjustmentFactor_ = windAdjustmentFactor;
        }

        public void setMidflameWindSpeed(double midflameWindSpeed)
        {
            midflameWindSpeed_ = midflameWindSpeed;
        }

        public void initializeMembers()
        {
            isWindLimitExceeded_ = false;
            effectiveWindSpeed_ = 0.0;
            windSpeedLimit_ = 0.0;
            phiS_ = 0.0;
            phiW_ = 0.0;
            windB_ = 0.0;
            windC_ = 0.0;
            windE_ = 0.0;
            directionOfInterest_ = 0.0;
            directionOfMaxSpread_ = 0.0;
            noWindNoSlopeSpreadRate_ = 0.0;
            forwardSpreadRate_ = 0.0;
            heatPerUnitArea_ = 0.0;
            fireLengthToWidthRatio_ = 1.0;
            residenceTime_ = 0.0;
            reactionIntensity_ = 0.0;
            firelineIntensity_ = 0.0;
            flameLength_ = 0.0;
            maxFlameLength_ = 0.0;
            backingSpreadRate_ = 0.0;

            midflameWindSpeed_ = 0.0;
            windAdjustmentFactor_ = 0.0;
            windAdjustmentFactorShelterMethod_ = WindAdjustmentFactorShelterMethod.Unsheltered;
            canopyCrownFraction_ = 0.0;

            aspenMortality_ = 0.0;
        }
    }
}

