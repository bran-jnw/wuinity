/******************************************************************************
*
* Project:  CodeBlocks
* Purpose:  Class for handling surface fire behavior based on the Facade OOP 
*           Design Pattern and using the Rothermel spread model
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static WUInity.Fire.BehaveUnits;

namespace WUInity.Fire
{
    public class Surface
    {
        FuelModelSet fuelModelSet_;

        // Surface Module components
        SurfaceInputs surfaceInputs_;
        SurfaceFire surfaceFire_;

        // Size Module
        FireSize size_;

        /*Surface(const FuelModelSet& fuelModelSet)
            : surfaceInputs_(),
            surfaceFire_(fuelModelSet, surfaceInputs_, size_)
        {
            fuelModelSet_ = &fuelModelSet;
        }*/
        //bran-jnw
        public Surface(FuelModelSet fuelModelSet)
        {
            surfaceInputs_ = new SurfaceInputs();
            size_ = new FireSize(); //bran-jnw added, does not get created otherwise?
            surfaceFire_ = new SurfaceFire(fuelModelSet, surfaceInputs_, size_);

            fuelModelSet_ = fuelModelSet;
        }


        /*
        // Copy Ctor
        Surface(const Surface& rhs)
            : surfaceFire_()
        {
            memberwiseCopyAssignment(rhs);
        }*/
        public Surface(Surface rhs) 
        {
            surfaceFire_ = new SurfaceFire();
            memberwiseCopyAssignment(rhs);
        }

        /*Surface& operator=(const Surface& rhs)
        {
            if (this != &rhs)
            {
                memberwiseCopyAssignment(rhs);
            }
            return *this;
        }*/

        void memberwiseCopyAssignment(Surface rhs)
        {
            surfaceInputs_ = rhs.surfaceInputs_;
            surfaceFire_ = rhs.surfaceFire_;
            size_ = rhs.size_;
        }

        public bool isAllFuelLoadZero(int fuelModelNumber)
        {
            //bran-jnw: added fix for LCP files that contains -9999 and similar non-defined behavior.
            if(!fuelModelSet_.isFuelModelDefined(fuelModelNumber))
            {
                return true;
            }

            // if  all loads are zero, skip calculations
            /*bool isNonZeroLoad = fuelModelSet_.getFuelLoadOneHour(fuelModelNumber, LoadingUnits.LoadingUnitsEnum.PoundsPerSquareFoot)
                    || fuelModelSet_.getFuelLoadTenHour(fuelModelNumber, LoadingUnits.LoadingUnitsEnum.PoundsPerSquareFoot)
                    || fuelModelSet_.getFuelLoadHundredHour(fuelModelNumber, LoadingUnits.LoadingUnitsEnum.PoundsPerSquareFoot)
                    || fuelModelSet_.getFuelLoadLiveHerbaceous(fuelModelNumber, LoadingUnits.LoadingUnitsEnum.PoundsPerSquareFoot)
                    || fuelModelSet_.getFuelLoadLiveWoody(fuelModelNumber, LoadingUnits.LoadingUnitsEnum.PoundsPerSquareFoot);*/
            //bran-jnw
            if(fuelModelSet_.getFuelLoadOneHour(fuelModelNumber, LoadingUnits.LoadingUnitsEnum.PoundsPerSquareFoot) == 0.0
                    && fuelModelSet_.getFuelLoadTenHour(fuelModelNumber, LoadingUnits.LoadingUnitsEnum.PoundsPerSquareFoot) == 0.0
                    && fuelModelSet_.getFuelLoadHundredHour(fuelModelNumber, LoadingUnits.LoadingUnitsEnum.PoundsPerSquareFoot) == 0.0
                    && fuelModelSet_.getFuelLoadLiveHerbaceous(fuelModelNumber, LoadingUnits.LoadingUnitsEnum.PoundsPerSquareFoot) == 0.0
                    && fuelModelSet_.getFuelLoadLiveWoody(fuelModelNumber, LoadingUnits.LoadingUnitsEnum.PoundsPerSquareFoot) == 0.0)
            {
                return true;
            }

            return false;

            //bool isZeroLoad = !isNonZeroLoad;
            //return isZeroLoad;
        }

        public void doSurfaceRunInDirectionOfMaxSpread()
        {
            double directionOfInterest = -1; // dummy value
            bool hasDirectionOfInterest = false;
            if (isUsingTwoFuelModels())
            {
                // Calculate spread rate for Two Fuel Models
                SurfaceTwoFuelModels surfaceTwoFuelModels = new SurfaceTwoFuelModels(surfaceFire_);
                TwoFuelModelsMethod twoFuelModelsMethod = surfaceInputs_.getTwoFuelModelsMethod();
                int firstFuelModelNumber = surfaceInputs_.getFirstFuelModelNumber();
                double firstFuelModelCoverage = surfaceInputs_.getFirstFuelModelCoverage();
                int secondFuelModelNumber = surfaceInputs_.getSecondFuelModelNumber();
                surfaceTwoFuelModels.calculateWeightedSpreadRate(twoFuelModelsMethod, firstFuelModelNumber, firstFuelModelCoverage,
                    secondFuelModelNumber, hasDirectionOfInterest, directionOfInterest);
            }
            else // Use only one fuel model
            {
                // Calculate spread rate
                int fuelModelNumber = surfaceInputs_.getFuelModelNumber();
                if (isAllFuelLoadZero(fuelModelNumber) || !fuelModelSet_.isFuelModelDefined(fuelModelNumber))
                {
                    // No fuel to burn, spread rate is zero
                    surfaceFire_.skipCalculationForZeroLoad();
                }
                else
                {
                    // Calculate spread rate
                    surfaceFire_.calculateForwardSpreadRate(fuelModelNumber, hasDirectionOfInterest, directionOfInterest);
                }
            }
        }

        public void doSurfaceRunInDirectionOfInterest(double directionOfInterest)
        {
            bool hasDirectionOfInterest = true;
            if (isUsingTwoFuelModels())
            {
                // Calculate spread rate for Two Fuel Models
                SurfaceTwoFuelModels surfaceTwoFuelModels = new SurfaceTwoFuelModels(surfaceFire_);
                TwoFuelModelsMethod twoFuelModelsMethod = surfaceInputs_.getTwoFuelModelsMethod();
                int firstFuelModelNumber = surfaceInputs_.getFirstFuelModelNumber();
                double firstFuelModelCoverage = surfaceInputs_.getFirstFuelModelCoverage();
                int secondFuelModelNumber = surfaceInputs_.getSecondFuelModelNumber();
                surfaceTwoFuelModels.calculateWeightedSpreadRate(twoFuelModelsMethod, firstFuelModelNumber, firstFuelModelCoverage,
                    secondFuelModelNumber, hasDirectionOfInterest, directionOfInterest);
            }
            else // Use only one fuel model
            {
                int fuelModelNumber = surfaceInputs_.getFuelModelNumber();
                if (isAllFuelLoadZero(fuelModelNumber) || !fuelModelSet_.isFuelModelDefined(fuelModelNumber))
                {
                    // No fuel to burn, spread rate is zero
                    surfaceFire_.skipCalculationForZeroLoad();
                }
                else
                {
                    // Calculate spread rate
                    surfaceFire_.calculateForwardSpreadRate(fuelModelNumber, hasDirectionOfInterest, directionOfInterest);
                }
            }
        }

        public double calculateFlameLength(double firelineIntensity)
        {
            return surfaceFire_.calculateFlameLength(firelineIntensity);
        }

        public void setFuelModelSet(FuelModelSet fuelModelSet)
        {
            fuelModelSet_ = fuelModelSet;
        }

        public void initializeMembers()
        {
            surfaceFire_.initializeMembers();
            surfaceInputs_.initializeMembers();
        }

        public double calculateSpreadRateAtVector(double directionOfinterest)
        {
            return surfaceFire_.calculateSpreadRateAtVector(directionOfinterest);
        }

        public double getSpreadRate(SpeedUnits.SpeedUnitsEnum spreadRateUnits)
        {
            return SpeedUnits.fromBaseUnits(surfaceFire_.getSpreadRate(), spreadRateUnits);
        }

        public double getSpreadRateInDirectionOfInterest(SpeedUnits.SpeedUnitsEnum spreadRateUnits)
        {
            return SpeedUnits.fromBaseUnits(surfaceFire_.getSpreadRateInDirectionOfInterest(), spreadRateUnits);
        }

        public double getDirectionOfMaxSpread()
        {
            double directionOfMaxSpread = surfaceFire_.getDirectionOfMaxSpread();
            return directionOfMaxSpread;
        }

        public double getFlameLength(LengthUnits.LengthUnitsEnum flameLengthUnits)
        {
            return LengthUnits.fromBaseUnits(surfaceFire_.getFlameLength(), flameLengthUnits);
        }

        public double getFireLengthToWidthRatio()
        {
            return size_.getFireLengthToWidthRatio();
        }

        public double getFireEccentricity()
        {
            return size_.getEccentricity();
        }

        public double getFirelineIntensity(FirelineIntensityUnits.FirelineIntensityUnitsEnum firelineIntensityUnits)
        {
            return FirelineIntensityUnits.fromBaseUnits(surfaceFire_.getFirelineIntensity(), firelineIntensityUnits);
        }

        public double getHeatPerUnitArea()
        {
            return surfaceFire_.getHeatPerUnitArea();
        }

        public double getResidenceTime()
        {
            return surfaceFire_.getResidenceTime();
        }

        public double getReactionIntensity(HeatSourceAndReactionIntensityUnits.HeatSourceAndReactionIntensityUnitsEnum reactiontionIntensityUnits)
        {
            return surfaceFire_.getReactionIntensity(reactiontionIntensityUnits);
        }

        public double getMidflameWindspeed()
        {
            return surfaceFire_.getMidflameWindSpeed();
        }

        public double getEllipticalA(LengthUnits.LengthUnitsEnum lengthUnits, double elapsedTime, TimeUnits.TimeUnitsEnum timeUnits)
        {
            return size_.getEllipticalA(lengthUnits, elapsedTime, timeUnits);
        }

        public double getEllipticalB(LengthUnits.LengthUnitsEnum lengthUnits, double elapsedTime, TimeUnits.TimeUnitsEnum timeUnits)
        {
            return size_.getEllipticalB(lengthUnits, elapsedTime, timeUnits);
        }

        public double getEllipticalC(LengthUnits.LengthUnitsEnum lengthUnits, double elapsedTime, TimeUnits.TimeUnitsEnum timeUnits)
        {
            return size_.getEllipticalC(lengthUnits, elapsedTime, timeUnits);
        }

        public double getSlopeFactor()
        {
            return surfaceFire_.getSlopeFactor();
        }

        public double getBulkDensity(DensityUnits.DensityUnitsEnum densityUnits)
        {
            return DensityUnits.fromBaseUnits(surfaceFire_.getBulkDensity(), densityUnits);
        }

        public double getHeatSink(HeatSinkUnits.HeatSinkUnitsEnum heatSinkUnits)
        {
            return surfaceFire_.getHeatSink(heatSinkUnits);
        }

        public double getFirePerimeter(LengthUnits.LengthUnitsEnum lengthUnits, double elapsedTime, TimeUnits.TimeUnitsEnum timeUnits)
        {
            return size_.getFirePerimeter(lengthUnits, elapsedTime, timeUnits);
        }

        public double getFireArea(AreaUnits.AreaUnitsEnum areaUnits, double elapsedTime, TimeUnits.TimeUnitsEnum timeUnits)
        {
            return size_.getFireArea(areaUnits, elapsedTime, timeUnits);
        }

        public void setCanopyCover(double canopyCover, CoverUnits.CoverUnitsEnum coverUnits)
        {
            surfaceInputs_.setCanopyCover(canopyCover, coverUnits);
        }

        public void setCanopyHeight(double canopyHeight, LengthUnits.LengthUnitsEnum canopyHeightUnits)
        {
            surfaceInputs_.setCanopyHeight(canopyHeight, canopyHeightUnits);
        }

        public void setCrownRatio(double crownRatio)
        {
            surfaceInputs_.setCrownRatio(crownRatio);
        }

        public bool isUsingTwoFuelModels()
        {
            return surfaceInputs_.isUsingTwoFuelModels();
        }

        public int getFuelModelNumber()
        {
	        return surfaceInputs_.getFuelModelNumber();
        }

        public double getMoistureOneHour(MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            return surfaceInputs_.getMoistureOneHour(moistureUnits);
        }

        public double getMoistureTenHour(MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            return surfaceInputs_.getMoistureTenHour(moistureUnits);
        }

        public double getMoistureHundredHour(MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            return surfaceInputs_.getMoistureHundredHour(moistureUnits);
        }

        public double getMoistureLiveHerbaceous(MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            return surfaceInputs_.getMoistureLiveHerbaceous(moistureUnits);
        }

        public double getMoistureLiveWoody(MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            return surfaceInputs_.getMoistureLiveWoody(moistureUnits);
        }

        public double getCanopyCover(CoverUnits.CoverUnitsEnum coverUnits)
        {
            return CoverUnits.fromBaseUnits(surfaceInputs_.getCanopyCover(), coverUnits);
        }

        public double getCanopyHeight(LengthUnits.LengthUnitsEnum canopyHeightUnits)
        {
            return LengthUnits.fromBaseUnits(surfaceInputs_.getCanopyHeight(), canopyHeightUnits);
        }

        public double getCrownRatio()
        {
            return surfaceInputs_.getCrownRatio();
        }

        public WindAndSpreadOrientationMode getWindAndSpreadOrientationMode()
        {
            return surfaceInputs_.getWindAndSpreadOrientationMode();
        }

        public WindHeightInputMode getWindHeightInputMode()
        {
            return surfaceInputs_.getWindHeightInputMode();
        }

        public WindAdjustmentFactorCalculationMethod getWindAdjustmentFactorCalculationMethod()
        {
            return surfaceInputs_.getWindAdjustmentFactorCalculationMethod();
        }

        public double getWindSpeed(SpeedUnits.SpeedUnitsEnum windSpeedUnits, WindHeightInputMode windHeightInputMode)
        {
            double midFlameWindSpeed = surfaceFire_.getMidflameWindSpeed();
            double windSpeed = midFlameWindSpeed;
            if (windHeightInputMode == WindHeightInputMode.DirectMidflame)
            {
                windSpeed = midFlameWindSpeed;
            }
            else 
            {
                double windAdjustmentFactor = surfaceFire_.getWindAdjustmentFactor();
    
                if ((windHeightInputMode == WindHeightInputMode.TwentyFoot) && (windAdjustmentFactor > 0.0))
                {
                    windSpeed = midFlameWindSpeed / windAdjustmentFactor;
                }
                else // Ten Meter
                {
                    if (windAdjustmentFactor > 0.0)
                    {
                        windSpeed = (midFlameWindSpeed / windAdjustmentFactor) * 1.15;
                    }
                }
            }
            return SpeedUnits.fromBaseUnits(windSpeed, windSpeedUnits);
        }

        public double getWindDirection()
        {
            return surfaceInputs_.getWindDirection();
        }

        public double getSlope(SlopeUnits.SlopeUnitsEnum slopeUnits)
        {
            return SlopeUnits.fromBaseUnits(surfaceInputs_.getSlope(), slopeUnits);
        }

        public double getAspect()
        {
            return surfaceInputs_.getAspect();
        }

        public void setFuelModelNumber(int fuelModelNumber)
        {
            surfaceInputs_.setFuelModelNumber(fuelModelNumber);
        }

        public void setMoistureOneHour(double moistureOneHour, MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            surfaceInputs_.setMoistureOneHour(moistureOneHour, moistureUnits);
        }

        public void setMoistureTenHour(double moistureTenHour, MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            surfaceInputs_.setMoistureTenHour(moistureTenHour, moistureUnits);
        }

        public void setMoistureHundredHour(double moistureHundredHour, MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            surfaceInputs_.setMoistureHundredHour(moistureHundredHour, moistureUnits);
        }

        public void setMoistureLiveHerbaceous(double moistureLiveHerbaceous, MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            surfaceInputs_.setMoistureLiveHerbaceous(moistureLiveHerbaceous, moistureUnits);
        }

        public void setMoistureLiveWoody(double moistureLiveWoody, MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            surfaceInputs_.setMoistureLiveWoody(moistureLiveWoody, moistureUnits);
        }

        public void setSlope(double slope, SlopeUnits.SlopeUnitsEnum slopeUnits)
        {
            surfaceInputs_.setSlope(slope, slopeUnits);
        }

        public void setAspect(double aspect)
        {
            surfaceInputs_.setAspect(aspect);
        }

        public void setWindSpeed(double windSpeed, SpeedUnits.SpeedUnitsEnum windSpeedUnits, WindHeightInputMode windHeightInputMode)
        {
            surfaceInputs_.setWindSpeed(windSpeed, windSpeedUnits, windHeightInputMode);
            surfaceFire_.calculateMidflameWindSpeed();
        }

        public void setUserProvidedWindAdjustmentFactor(double userProvidedWindAdjustmentFactor)
        {
            surfaceInputs_.setUserProvidedWindAdjustmentFactor(userProvidedWindAdjustmentFactor);
        }

        public void setWindDirection(double windDirection)
        {
            surfaceInputs_.setWindDirection(windDirection);
        }

        public void setWindAndSpreadOrientationMode(WindAndSpreadOrientationMode windAndSpreadOrientationMode)
        {
            surfaceInputs_.setWindAndSpreadOrientationMode(windAndSpreadOrientationMode);
        }

        public void setWindHeightInputMode(WindHeightInputMode windHeightInputMode)
        {
            surfaceInputs_.setWindHeightInputMode(windHeightInputMode);
        }

        public void setFirstFuelModelNumber(int firstFuelModelNumber)
        {
            surfaceInputs_.setFirstFuelModelNumber(firstFuelModelNumber);
        }

        public void setSecondFuelModelNumber(int secondFuelModelNumber)
        {
            surfaceInputs_.setSecondFuelModelNumber(secondFuelModelNumber);
        }

        public void setTwoFuelModelsMethod(TwoFuelModelsMethod twoFuelModelsMethod)
        {
            surfaceInputs_.setTwoFuelModelsMethod(twoFuelModelsMethod);
        }

        public void setTwoFuelModelsFirstFuelModelCoverage(double firstFuelModelCoverage, CoverUnits.CoverUnitsEnum coverUnits)
        {
            surfaceInputs_.setTwoFuelModelsFirstFuelModelCoverage(firstFuelModelCoverage, coverUnits);
        }

        public void setWindAdjustmentFactorCalculationMethod(WindAdjustmentFactorCalculationMethod windAdjustmentFactorCalculationMethod)
        {
            surfaceInputs_.setWindAdjustmentFactorCalculationMethod(windAdjustmentFactorCalculationMethod);
        }

        public void updateSurfaceInputs(int fuelModelNumber, double moistureOneHour, double moistureTenHour, double moistureHundredHour,
            double moistureLiveHerbaceous, double moistureLiveWoody, MoistureUnits.MoistureUnitsEnum moistureUnits, double windSpeed,
            SpeedUnits.SpeedUnitsEnum windSpeedUnits, WindHeightInputMode windHeightInputMode,
            double windDirection, WindAndSpreadOrientationMode windAndSpreadOrientationMode,
            double slope, SlopeUnits.SlopeUnitsEnum slopeUnits, double aspect, double canopyCover, CoverUnits.CoverUnitsEnum coverUnits, double canopyHeight,
            LengthUnits.LengthUnitsEnum canopyHeightUnits, double crownRatio)
        {
            surfaceInputs_.updateSurfaceInputs(fuelModelNumber, moistureOneHour, moistureTenHour, moistureHundredHour, moistureLiveHerbaceous,
                moistureLiveWoody, moistureUnits, windSpeed, windSpeedUnits, windHeightInputMode, windDirection, windAndSpreadOrientationMode,
                slope, slopeUnits, aspect, canopyCover, coverUnits, canopyHeight, canopyHeightUnits, crownRatio);
            surfaceFire_.calculateMidflameWindSpeed();
        }

        public void updateSurfaceInputsForTwoFuelModels(int firstfuelModelNumber, int secondFuelModelNumber, double moistureOneHour,
            double moistureTenHour, double moistureHundredHour, double moistureLiveHerbaceous, double moistureLiveWoody,
            MoistureUnits.MoistureUnitsEnum moistureUnits, double windSpeed, SpeedUnits.SpeedUnitsEnum windSpeedUnits,
            WindHeightInputMode windHeightInputMode, double windDirection,
            WindAndSpreadOrientationMode windAndSpreadOrientationMode, double firstFuelModelCoverage,
            CoverUnits.CoverUnitsEnum firstFuelModelCoverageUnits, TwoFuelModelsMethod twoFuelModelsMethod,
            double slope, SlopeUnits.SlopeUnitsEnum slopeUnits, double aspect, double canopyCover,
            CoverUnits.CoverUnitsEnum canopyCoverUnits, double canopyHeight, LengthUnits.LengthUnitsEnum canopyHeightUnits, double crownRatio)
        {
            surfaceInputs_.updateSurfaceInputsForTwoFuelModels(firstfuelModelNumber, secondFuelModelNumber, moistureOneHour, moistureTenHour,
                moistureHundredHour, moistureLiveHerbaceous, moistureLiveWoody, moistureUnits, windSpeed, windSpeedUnits, windHeightInputMode,
                windDirection, windAndSpreadOrientationMode, firstFuelModelCoverage, firstFuelModelCoverageUnits, twoFuelModelsMethod, slope,
                slopeUnits, aspect, canopyCover, canopyCoverUnits, canopyHeight, canopyHeightUnits, crownRatio);
            surfaceFire_.calculateMidflameWindSpeed();
        }

        public void updateSurfaceInputsForPalmettoGallbery(double moistureOneHour, double moistureTenHour, double moistureHundredHour,
            double moistureLiveHerbaceous, double moistureLiveWoody, MoistureUnits.MoistureUnitsEnum moistureUnits, double windSpeed,
            SpeedUnits.SpeedUnitsEnum windSpeedUnits, WindHeightInputMode windHeightInputMode, double windDirection,
            WindAndSpreadOrientationMode windAndSpreadOrientationMode, double ageOfRough,
            double heightOfUnderstory, double palmettoCoverage, double overstoryBasalArea, double slope, SlopeUnits.SlopeUnitsEnum slopeUnits,
            double aspect, double canopyCover, CoverUnits.CoverUnitsEnum coverUnits, double canopyHeight, LengthUnits.LengthUnitsEnum canopyHeightUnits, double crownRatio)
        {
            surfaceInputs_.updateSurfaceInputsForPalmettoGallbery(moistureOneHour, moistureTenHour, moistureHundredHour, moistureLiveHerbaceous,
                moistureLiveWoody, moistureUnits, windSpeed, windSpeedUnits, windHeightInputMode, windDirection, windAndSpreadOrientationMode,
                ageOfRough, heightOfUnderstory, palmettoCoverage, overstoryBasalArea, slope, slopeUnits, aspect, canopyCover, coverUnits,
                canopyHeight, canopyHeightUnits, crownRatio);
            surfaceFire_.calculateMidflameWindSpeed();
        }

        public void updateSurfaceInputsForWesternAspen(int aspenFuelModelNumber, double aspenCuringLevel,
            AspenFireSeverity aspenFireSeverity, double DBH, double moistureOneHour, double moistureTenHour,
            double moistureHundredHour, double moistureLiveHerbaceous, double moistureLiveWoody, MoistureUnits.MoistureUnitsEnum moistureUnits,
            double windSpeed, SpeedUnits.SpeedUnitsEnum windSpeedUnits, WindHeightInputMode windHeightInputMode,
            double windDirection, WindAndSpreadOrientationMode windAndSpreadOrientationMode, double slope,
            SlopeUnits.SlopeUnitsEnum slopeUnits, double aspect, double canopyCover, CoverUnits.CoverUnitsEnum coverUnits, double canopyHeight,
            LengthUnits.LengthUnitsEnum canopyHeightUnits, double crownRatio)
        {
            surfaceInputs_.updateSurfaceInputsForWesternAspen(aspenFuelModelNumber, aspenCuringLevel, aspenFireSeverity, DBH, moistureOneHour,
                moistureTenHour, moistureHundredHour, moistureLiveHerbaceous, moistureLiveWoody, moistureUnits, windSpeed, windSpeedUnits,
                windHeightInputMode, windDirection, windAndSpreadOrientationMode, slope, slopeUnits, aspect, canopyCover, coverUnits,
                canopyHeight, canopyHeightUnits, crownRatio);
            surfaceFire_.calculateMidflameWindSpeed();
        }
    }
}

