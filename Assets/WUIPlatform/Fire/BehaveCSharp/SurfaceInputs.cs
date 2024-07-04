//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using static WUIPlatform.Fire.BehaveUnits;

namespace WUIPlatform.Fire
{    
    public enum AspenFireSeverity
    {
        Low = 0,
        Mmoderate = 1
    }

    
    public enum TwoFuelModelsMethod
    {
        NoMethod = 0,          // Don't use TwoFuel Models method
        Arithmetic = 1,         // Use arithmetic mean
        Harmonic = 2,           // Use harmoic mean
        TwoFimensional = 3     // Use Finney's two dimensional method
    }

    public enum WindAdjustmentFactorShelterMethod
    {
        Unsheltered = 0,            // Wind adjustment factor was calculated using unsheltered method
        Sheltered = 1,              // Wind adjustment factor was calculated using sheltered method
    }
    
    public enum WindAdjustmentFactorCalculationMethod
    {
        UserInput = 0,             // User enters wind adjustment factor directly
        UseCrownRatio = 1,        // Use crown ratio when calculating wind adjustment factor
        DontUseCrownRatio = 2    // Don't use crown ratio when calculating wind adjustment factor
    }

    
    public enum WindAndSpreadOrientationMode
    {
        RelativeToUpslope = 0,    // Wind and spread angles I/O are clockwise relative to upslope
        RelativeToNorth = 1       // Wind direction angles I/O are clockwise relative to compass north
    }
    
    public enum WindHeightInputMode
    {
        DirectMidflame = 0,    // User enters midflame wind speed directy
        TwentyFoot = 1,        // User enters the 20 foot wind speed
        TenMeter = 2           // User enters the 10 meter wind speed
    }

    public class SurfaceInputs
    {
        
        public enum FuelConstants
        {
            DEAD = 0,                   // Index associated with dead fuels
            LIVE = 1,                   // Index associated with live fuels
            MAX_LIFE_STATES = 2,        // Number of life states, live and dead
            MAX_LIVE_SIZE_CLASSES = 3,  // Maximum number of live size classes
            MAX_DEAD_SIZE_CLASSES = 4,  // Maximum number of dead size classes
            MAX_PARTICLES = 4,          // Maximum number of size classes within a life state (dead/live)
            MAX_SAVR_SIZE_CLASSES = 5,  // Maximum number of SAVR size classes
            NUM_FUEL_MODELS = 267       // Maximum number of fuel models
        }
        
        public enum TwoFuelModelsContants
        {
            FIRST = 0,              // Index of the first fuel model
            SECOND = 1,             // Index of the second fuel model
            NUMBER_OF_MODELS = 2,   // Numbe of fuel models used in TwoFuel Models method
        }

        // Main Suface module inputs
        int fuelModelNumber_;               // 1 to 256
        double moistureOneHour_;            // 1% to 60%
        double moistureTenHour_;            // 1% to 60%		
        double moistureHundredHour_;        // 1% to 60%
        double moistureLiveHerbaceous_;     // 30% to 300%
        double moistureLiveWoody_;          // 30% to 300%
        double windSpeed_;                  // measured wind speed in feet per minute
        double windDirection_;              // degrees, 0-360
        double slope_;                      // gradient 0-600 or degrees 0-80  
        double aspect_;                     // aspect of slope in degrees, 0-360

        // Two Fuel Models inputs
        bool isUsingTwoFuelModels_;         // Whether fire spread calculation is using Two Fuel Models
        int secondFuelModelNumber_;         // 1 to 256, second fuel used in Two Fuel Models
        double firstFuelModelCoverage_;     // percent of landscape occupied by first fuel in Two Fuel Models

        // Palmetto-Gallberry inputs
        bool isUsingPalmettoGallberry_;
        double ageOfRough_;
        double heightOfUnderstory_;
        double palmettoCoverage_;
        double overstoryBasalArea_;

        // Western Aspen inputs
        bool isUsingWesternAspen_;
        int aspenFuelModelNumber_;
        double aspenCuringLevel_;
        double DBH_;
        AspenFireSeverity aspenFireSeverity_;

        // For Size Module
        double elapsedTime_;

        // Wind Adjustment Factor Parameters
        double canopyCover_;
        double canopyHeight_;
        double crownRatio_;
        double userProvidedWindAdjustmentFactor_;

        // Input Modes
        TwoFuelModelsMethod twoFuelModelsMethod_;
        WindHeightInputMode windHeightInputMode_;
        WindAndSpreadOrientationMode windAndSpreadOrientationMode_;
        WindAdjustmentFactorCalculationMethod windAdjustmentFactorCalculationMethod_;

        // Default Ctor
        public SurfaceInputs()
        {
            initializeMembers();
        }

        /*SurfaceInputs & operator=(const SurfaceInputs & rhs)
        {
            if (this != &rhs)
            {
                memberwiseCopyAssignment(rhs);
            }
            return *this;
        }*/

        public void initializeMembers()
        {
            fuelModelNumber_ = 0;
            secondFuelModelNumber_ = 0;
            moistureOneHour_ = 0.0;
            moistureTenHour_ = 0.0;
            moistureHundredHour_ = 0.0;
            moistureLiveHerbaceous_ = 0.0;
            moistureLiveWoody_ = 0.0;
            slope_ = 0.0;
            aspect_ = 0.0;
            windSpeed_ = 0.0;
            windDirection_ = 0.0;

            isUsingTwoFuelModels_ = false;
            isUsingPalmettoGallberry_ = false;
            isUsingWesternAspen_ = false;

            windAndSpreadOrientationMode_ = WindAndSpreadOrientationMode.RelativeToUpslope;
            windHeightInputMode_ = WindHeightInputMode.DirectMidflame;
            twoFuelModelsMethod_ = TwoFuelModelsMethod.NoMethod;
            windAdjustmentFactorCalculationMethod_ = WindAdjustmentFactorCalculationMethod.UseCrownRatio;

            firstFuelModelCoverage_ = 0.0;

            ageOfRough_ = 0.0;
            heightOfUnderstory_ = 0.0;
            palmettoCoverage_ = 0.0;
            overstoryBasalArea_ = 0.0;

            canopyCover_ = 0.0;
            canopyHeight_ = 0.0;
            crownRatio_ = 0.0;

            aspenFuelModelNumber_ = -1;
            aspenCuringLevel_ = 0.0;
            aspenFireSeverity_ = AspenFireSeverity.Low;
            DBH_ = 0.0;

            elapsedTime_ = TimeUnits.toBaseUnits(1, TimeUnits.TimeUnitsEnum.Hours);

            userProvidedWindAdjustmentFactor_ = -1.0;
        }

        public void updateSurfaceInputs(int fuelModelNumber, double moistureOneHour, double moistureTenHour,
            double moistureHundredHour, double moistureLiveHerbaceous, double moistureLiveWoody,
            MoistureUnits.MoistureUnitsEnum moistureUnits, double windSpeed, SpeedUnits.SpeedUnitsEnum windSpeedUnits,
            WindHeightInputMode windHeightInputMode, double windDirection,
            WindAndSpreadOrientationMode windAndSpreadOrientationMode, double slope,
            SlopeUnits.SlopeUnitsEnum slopeUnits, double aspect, double canopyCover, CoverUnits.CoverUnitsEnum coverUnits,
            double canopyHeight, LengthUnits.LengthUnitsEnum canopyHeightUnits, double crownRatio)
        {
            setSlope(slope, slopeUnits);
            aspect_ = aspect;



            setFuelModelNumber(fuelModelNumber);

            setMoistureOneHour(moistureOneHour, moistureUnits);
            setMoistureTenHour(moistureTenHour, moistureUnits);
            setMoistureHundredHour(moistureHundredHour, moistureUnits);
            setMoistureLiveHerbaceous(moistureLiveHerbaceous, moistureUnits);
            setMoistureLiveWoody(moistureLiveWoody, moistureUnits);

            setWindSpeed(windSpeed, windSpeedUnits, windHeightInputMode);
            setWindHeightInputMode(windHeightInputMode);

            if (windDirection < 0.0)
            {
                windDirection += 360.0;
            }
            while (windDirection >= 360.0)
            {
                windDirection -= 360.0;
            }

            setWindDirection(windDirection);
            setWindAndSpreadOrientationMode(windAndSpreadOrientationMode);
            isUsingTwoFuelModels_ = false;
            setTwoFuelModelsMethod(TwoFuelModelsMethod.NoMethod);

            isUsingPalmettoGallberry_ = false;
            isUsingWesternAspen_ = false;

            setCanopyCover(canopyCover, coverUnits);
            setCanopyHeight(canopyHeight, canopyHeightUnits);
            setCrownRatio(crownRatio);
        }

        public void updateSurfaceInputsForTwoFuelModels(int firstfuelModelNumber, int secondFuelModelNumber,
            double moistureOneHour, double moistureTenHour, double moistureHundredHour, double moistureLiveHerbaceous,
            double moistureLiveWoody, MoistureUnits.MoistureUnitsEnum moistureUnits, double windSpeed, SpeedUnits.SpeedUnitsEnum windSpeedUnits,
            WindHeightInputMode windHeightInputMode, double windDirection,
            WindAndSpreadOrientationMode windAndSpreadOrientationMode, double firstFuelModelCoverage,
            CoverUnits.CoverUnitsEnum firstFuelModelCoverageUnits, TwoFuelModelsMethod twoFuelModelsMethod, double slope, SlopeUnits.SlopeUnitsEnum slopeUnits,
            double aspect, double canopyCover, CoverUnits.CoverUnitsEnum canopyCoverUnits, double canopyHeight, LengthUnits.LengthUnitsEnum canopyHeightUnits, double crownRatio)
        {
            int fuelModelNumber = firstfuelModelNumber;
            updateSurfaceInputs(fuelModelNumber, moistureOneHour, moistureTenHour, moistureHundredHour, moistureLiveHerbaceous,
                moistureLiveWoody, moistureUnits, windSpeed, windSpeedUnits, windHeightInputMode, windDirection, windAndSpreadOrientationMode,
                slope, slopeUnits,
                aspect, canopyCover, canopyCoverUnits, canopyHeight, canopyHeightUnits, crownRatio);
            setSecondFuelModelNumber(secondFuelModelNumber);
            setTwoFuelModelsFirstFuelModelCoverage(firstFuelModelCoverage, firstFuelModelCoverageUnits);
            isUsingTwoFuelModels_ = true;
            setTwoFuelModelsMethod(twoFuelModelsMethod);
        }

        public void updateSurfaceInputsForPalmettoGallbery(double moistureOneHour, double moistureTenHour,
            double moistureHundredHour, double moistureLiveHerbaceous, double moistureLiveWoody, MoistureUnits.MoistureUnitsEnum moistureUnits,
            double windSpeed, SpeedUnits.SpeedUnitsEnum windSpeedUnits, WindHeightInputMode windHeightInputMode,
            double windDirection, WindAndSpreadOrientationMode windAndSpreadOrientationMode,
            double ageOfRough, double heightOfUnderstory, double palmettoCoverage, double overstoryBasalArea,
            double slope, SlopeUnits.SlopeUnitsEnum slopeUnits, double aspect, double canopyCover, CoverUnits.CoverUnitsEnum coverUnits, double canopyHeight,
            LengthUnits.LengthUnitsEnum canopyHeightUnits, double crownRatio)
        {
            updateSurfaceInputs(0, moistureOneHour, moistureTenHour, moistureHundredHour, moistureLiveHerbaceous,
                moistureLiveWoody, moistureUnits, windSpeed, windSpeedUnits, windHeightInputMode, windDirection,
                windAndSpreadOrientationMode, slope, slopeUnits, aspect, canopyCover, coverUnits, canopyHeight, canopyHeightUnits,
                crownRatio);

            setAgeOfRough(ageOfRough);
            setHeightOfUnderstory(heightOfUnderstory);
            setPalmettoCoverage(palmettoCoverage);
            setOverstoryBasalArea(overstoryBasalArea);
            isUsingPalmettoGallberry_ = true;
        }

        public void updateSurfaceInputsForWesternAspen(int aspenFuelModelNumber, double aspenCuringLevel,
            AspenFireSeverity aspenFireSeverity, double DBH, double moistureOneHour, double moistureTenHour,
            double moistureHundredHour, double moistureLiveHerbaceous, double moistureLiveWoody, MoistureUnits.MoistureUnitsEnum moistureUnits,
            double windSpeed, SpeedUnits.SpeedUnitsEnum windSpeedUnits, WindHeightInputMode windHeightInputMode,
            double windDirection, WindAndSpreadOrientationMode windAndSpreadOrientationMode,
            double slope, SlopeUnits.SlopeUnitsEnum slopeUnits, double aspect, double canopyCover, CoverUnits.CoverUnitsEnum coverUnits, double canopyHeight,
            LengthUnits.LengthUnitsEnum canopyHeightUnits, double crownRatio)
        {
            updateSurfaceInputs(0, moistureOneHour, moistureTenHour, moistureHundredHour, moistureLiveHerbaceous,
                moistureLiveWoody, moistureUnits, windSpeed, windSpeedUnits, windHeightInputMode, windDirection,
                windAndSpreadOrientationMode, slope, slopeUnits, aspect, canopyCover, coverUnits, canopyHeight, canopyHeightUnits,
                crownRatio);

            setAspenFuelModelNumber(aspenFuelModelNumber);
            setAspenCuringLevel(aspenCuringLevel);
            setAspenFireSeverity(aspenFireSeverity);
            setAspenDBH(DBH);
            isUsingWesternAspen_ = true;
        }

        public void setAspenFuelModelNumber(int aspenFuelModelNumber)
        {
            aspenFuelModelNumber_ = aspenFuelModelNumber;
        }

        public void setAspenCuringLevel(double aspenCuringLevel)
        {
            aspenCuringLevel_ = aspenCuringLevel;
        }

        public void setAspenDBH(double DBH)
        {
            DBH_ = DBH;
        }

        public void setAspenFireSeverity(AspenFireSeverity aspenFireSeverity)
        {
            aspenFireSeverity_ = aspenFireSeverity;
        }

        public void setCanopyCover(double canopyCover, CoverUnits.CoverUnitsEnum coverUnits)
        {
            canopyCover_ = CoverUnits.toBaseUnits(canopyCover, coverUnits);
        }

        public void setCanopyHeight(double canopyHeight, LengthUnits.LengthUnitsEnum canopyHeightUnits)
        {
            canopyHeight_ = LengthUnits.toBaseUnits(canopyHeight, canopyHeightUnits);
        }

        public void setCrownRatio(double crownRatio)
        {
            crownRatio_ = crownRatio;
        }

        public void setWindAndSpreadOrientationMode(WindAndSpreadOrientationMode windAndSpreadOrientationMode)
        {
            windAndSpreadOrientationMode_ = windAndSpreadOrientationMode;
        }

        public void setWindHeightInputMode(WindHeightInputMode windHeightInputMode)
        {
            windHeightInputMode_ = windHeightInputMode;
        }

        public void setFuelModelNumber(int fuelModelNumber)
        {
            fuelModelNumber_ = fuelModelNumber;
        }

        public void setMoistureOneHour(double moistureOneHour, MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            moistureOneHour_ = MoistureUnits.toBaseUnits(moistureOneHour, moistureUnits);
        }

        public void setMoistureTenHour(double moistureTenHour, MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            moistureTenHour_ = MoistureUnits.toBaseUnits(moistureTenHour, moistureUnits);
        }

        public void setMoistureHundredHour(double moistureHundredHour, MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            moistureHundredHour_ = MoistureUnits.toBaseUnits(moistureHundredHour, moistureUnits);
        }

        public void setMoistureLiveHerbaceous(double moistureLiveHerbaceous, MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            moistureLiveHerbaceous_ = MoistureUnits.toBaseUnits(moistureLiveHerbaceous, moistureUnits);
        }

        public void setMoistureLiveWoody(double moistureLiveWoody, MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            moistureLiveWoody_ = MoistureUnits.toBaseUnits(moistureLiveWoody, moistureUnits);
        }

        public void setSlope(double slope, SlopeUnits.SlopeUnitsEnum slopeUnits)
        {
            slope_ = SlopeUnits.toBaseUnits(slope, slopeUnits);
        }

        public void setAspect(double aspect)
        {
            aspect_ = aspect;
        }

        public void setTwoFuelModelsMethod(TwoFuelModelsMethod twoFuelModelsMethod)
        {
            twoFuelModelsMethod_ = twoFuelModelsMethod;
        }

        public void setTwoFuelModelsFirstFuelModelCoverage(double firstFuelModelCoverage, CoverUnits.CoverUnitsEnum coverUnits)
        {
            firstFuelModelCoverage_ = CoverUnits.toBaseUnits(firstFuelModelCoverage, coverUnits);
        }

        public void setWindSpeed(double windSpeed, SpeedUnits.SpeedUnitsEnum windSpeedUnits, WindHeightInputMode windHeightInputMode)
        {
            windHeightInputMode_ = windHeightInputMode;
            windSpeed_ = SpeedUnits.toBaseUnits(windSpeed, windSpeedUnits);
        }

        public void setWindDirection(double windDirection)
        {
            windDirection_ = windDirection;
        }

        public void setFirstFuelModelNumber(int firstFuelModelNumber)
        {
            fuelModelNumber_ = firstFuelModelNumber;
        }

        public int getFirstFuelModelNumber()
        {
            return fuelModelNumber_;
        }

        public int getSecondFuelModelNumber()
        {
            return secondFuelModelNumber_;
        }

        public void setSecondFuelModelNumber(int secondFuelModelNumber)
        {
            secondFuelModelNumber_ = secondFuelModelNumber;
        }

        public int getFuelModelNumber()
        {
            return fuelModelNumber_;
        }

        public double getSlope()
        {
            return slope_;
        }

        public double getAspect()
        {
            return aspect_;
        }

        public double getFirstFuelModelCoverage()
        {
            return firstFuelModelCoverage_;
        }

        public TwoFuelModelsMethod getTwoFuelModelsMethod()
        {
            return twoFuelModelsMethod_;
        }

        public bool isUsingTwoFuelModels()
        {
            return isUsingTwoFuelModels_;
        }

        public bool isUsingPalmettoGallberry()
        {
            return isUsingPalmettoGallberry_;
        }

        public WindHeightInputMode getWindHeightInputMode()
        {
            return windHeightInputMode_;
        }

        public WindAndSpreadOrientationMode getWindAndSpreadOrientationMode()
        {
            return windAndSpreadOrientationMode_;
        }

        public double getWindDirection()
        {
            return windDirection_;
        }

        public double getWindSpeed()
        {
            return windSpeed_;
        }

        public double getMoistureOneHour(MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            return MoistureUnits.fromBaseUnits(moistureOneHour_, moistureUnits);
        }

        public double getMoistureTenHour(MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
             return MoistureUnits.fromBaseUnits(moistureTenHour_, moistureUnits);
        }

        public double getMoistureHundredHour(MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            return MoistureUnits.fromBaseUnits(moistureHundredHour_, moistureUnits);
        }

        public double getMoistureLiveHerbaceous(MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            return MoistureUnits.fromBaseUnits(moistureLiveHerbaceous_, moistureUnits);
        }

        public double getMoistureLiveWoody(MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            return MoistureUnits.fromBaseUnits(moistureLiveWoody_, moistureUnits);
        }

        public void setAgeOfRough(double ageOfRough)
        {
            ageOfRough_ = ageOfRough;
        }

        public double getAgeOfRough()
        {
            return ageOfRough_;
        }

        public void setHeightOfUnderstory(double heightOfUnderstory)
        {
            heightOfUnderstory_ = heightOfUnderstory;
        }

        public double getHeightOfUnderstory()
        {
            return heightOfUnderstory_;
        }

        public void setPalmettoCoverage(double palmettoCoverage)
        {
            palmettoCoverage_ = palmettoCoverage;
        }

        public double getPalmettoCoverage()
        {
            return palmettoCoverage_;
        }

        public void setOverstoryBasalArea(double overstoryBasalArea)
        {
            overstoryBasalArea_ = overstoryBasalArea;
        }

        public double getOverstoryBasalArea()
        {
            return overstoryBasalArea_;
        }

        public double getCanopyCover()
        {
            return canopyCover_;
        }

        public double getCanopyHeight()
        {
            return canopyHeight_;
        }

        public double getCrownRatio()
        {
            return crownRatio_;
        }

        public bool isUsingWesternAspen()
        {
            return isUsingWesternAspen_;
        }

        public int getAspenFuelModelNumber()
        {
            return aspenFuelModelNumber_;
        }

        public double getAspenCuringLevel()
        {
            return aspenCuringLevel_;
        }

        public double getAspenDBH()
        {
            return DBH_;
        }

        public AspenFireSeverity getAspenFireSeverity()
        {
            return aspenFireSeverity_;
        }

        public void memberwiseCopyAssignment(SurfaceInputs rhs)
        {
            fuelModelNumber_ = rhs.fuelModelNumber_;
            moistureOneHour_ = rhs.moistureOneHour_;
            moistureTenHour_ = rhs.moistureTenHour_;
            moistureHundredHour_ = rhs.moistureHundredHour_;
            moistureLiveHerbaceous_ = rhs.moistureLiveHerbaceous_;
            moistureLiveWoody_ = rhs.moistureLiveWoody_;
            windSpeed_ = rhs.windSpeed_;
            windDirection_ = rhs.windDirection_;
            slope_ = rhs.slope_;
            aspect_ = rhs.aspect_;

            isUsingTwoFuelModels_ = rhs.isUsingTwoFuelModels_;
            secondFuelModelNumber_ = rhs.secondFuelModelNumber_;
            firstFuelModelCoverage_ = rhs.firstFuelModelCoverage_;

            isUsingPalmettoGallberry_ = rhs.isUsingPalmettoGallberry_;
            ageOfRough_ = rhs.ageOfRough_;
            heightOfUnderstory_ = rhs.heightOfUnderstory_;
            palmettoCoverage_ = rhs.palmettoCoverage_;
            overstoryBasalArea_ = rhs.overstoryBasalArea_;

            isUsingWesternAspen_ = rhs.isUsingWesternAspen_;
            aspenFuelModelNumber_ = rhs.aspenFuelModelNumber_;
            aspenCuringLevel_ = rhs.aspenCuringLevel_;
            DBH_ = rhs.DBH_;
            aspenFireSeverity_ = rhs.aspenFireSeverity_;

            elapsedTime_ = rhs.elapsedTime_;

            canopyCover_ = rhs.canopyCover_;
            canopyHeight_ = rhs.canopyHeight_;
            crownRatio_ = rhs.crownRatio_;
            userProvidedWindAdjustmentFactor_ = rhs.userProvidedWindAdjustmentFactor_;

            twoFuelModelsMethod_ = rhs.twoFuelModelsMethod_;
            windHeightInputMode_ = rhs.windHeightInputMode_;
            windAndSpreadOrientationMode_ = rhs.windAndSpreadOrientationMode_;
            windAdjustmentFactorCalculationMethod_ = rhs.windAdjustmentFactorCalculationMethod_;
        }

        public void setUserProvidedWindAdjustmentFactor(double userProvidedWindAdjustmentFactor)
        {
            userProvidedWindAdjustmentFactor_ = userProvidedWindAdjustmentFactor;
        }

        public void setWindAdjustmentFactorCalculationMethod(WindAdjustmentFactorCalculationMethod windAdjustmentFactorCalculationMethod)
        {
            windAdjustmentFactorCalculationMethod_ = windAdjustmentFactorCalculationMethod;
        }

        public void setElapsedTime(double elapsedTime, TimeUnits.TimeUnitsEnum timeUnits)
        {
            elapsedTime_ = TimeUnits.toBaseUnits(elapsedTime, timeUnits);
        }

        public double getUserProvidedWindAdjustmentFactor()
        {
            return userProvidedWindAdjustmentFactor_;
        }

        public WindAdjustmentFactorCalculationMethod getWindAdjustmentFactorCalculationMethod()
        {
            return windAdjustmentFactorCalculationMethod_;
        }

        public double getElapsedTime()
        {
            return elapsedTime_;
        }
    }
}

