//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using static WUIPlatform.Fire.Behave.BehaveUnits;
using static WUIPlatform.Fire.MathWrap;

namespace WUIPlatform.Fire.Behave
{
    struct FireType
    {
        public enum FireTypeEnum
        {
            Surface = 0,    // surface fire with no torching or crown fire spread.
            Torching = 1,   // surface fire with torching.
            ConditionalCrownFire = 2, // active crown fire possible if the fire transitions to the overstory        
            Crowning = 3    // active crown fire, fire is spreading through the canopy.
        }
    }

    public class Crown
    {
        FuelModelSet fuelModelSet_;
        CrownInputs crownInputs_;

        Surface surfaceFuel_;
        Surface crownFuel_;

        FireType.FireTypeEnum fireType_;               // Classification based on corwn fire active and transition ratios
        double surfaceFireHeatPerUnitArea_;             // Surface fire hpua used for parallel surface runs (Btu/ft^2)
        double surfaceFirelineIntensity_;               // Surface fireline intensity used for parallel surface runs
        double surfaceFireSpreadRate_;
        double surfaceFireFlameLength_;
        double surfaceFireCriticalSpreadRate_;
        double crownFuelLoad_;                          // Crown fire fuel load (lb / ft^2)
        double canopyHeatPerUnitArea_;                  // Canopy heat per unit area (Btu/ft^2)
        double crownFireHeatPerUnitArea_;               // Crown fire heat per unit area (Btu/ft^2)
        double crownFirelineIntensity_;                 // Crown fire fireline intensity (Btu / ft / s)
        double crownFlameLength_;                       // Crown fire flame length (ft)
        double crownFireSpreadRate_;
        double crownCriticalSurfaceFirelineIntensity_;  // Crown fire's critical surface fire intensity (Btu / ft / s)
        double crownCriticalFireSpreadRate_;            // Crown fire's critical crown fire spread rate (ft / min)
        double crownCriticalSurfaceFlameLength_;        // Crown fire's critical surface fire flame length (ft)
        double crownPowerOfFire_;                       // Crown fire 'power of the fire' ( ft*lb / s / ft^2)
        double crownPowerOfWind_;                       // Crown fire 'power of the wind' ( ft*lb / s / ft^2)
        double crownFirePowerRatio_;                    // Crown fire power ratio
        double crownFireActiveRatio_;                   // Crown fire active ratio
        double crownFireTransitionRatio_;               // Crown fire transition ratio
        double crownFireLengthToWidthRatio_;            // Crown fire transition ratio
        double crownFireActiveWindSpeed_;               // 20 ft windspeed at which active crowning is possible (ft/min)
        double crownFractionBurned_;
        double crowningSurfaceFireRos_;                 // Surface fire spread rate at which the active crown fire spread rate is fully achieved (ft/min)
        double windSpeedAtTwentyFeet_;

        double finalSpreadRate_;                        // "Actual" spread rate of the fire, depends on fire type
        double finalHeatPerUnitArea_;                   // "Actual" fire heat per unit area, depends on fire type
        double finalFirelineIntesity_;                  // "Actual" fireline intensity, depends on fire type
        double finalFlameLength_;                       // "Actual" flame length, depends on fire type

        double passiveCrownFireSpreadRate_;
        double passiveCrownFireHeatPerUnitArea_;
        double passiveCrownFireLineIntensity_;
        double passiveCrownFireFlameLength_;

        bool isSurfaceFire_;
        bool isPassiveCrownFire_;
        bool isActiveCrownFire_;
        bool isCrownFire_;

        //bran-jnw: added direction of surface fire max spread rate
        double surfaceFireDirectionOfMaxSpread_;
        double surfaceFireEccentricity_;

        public Crown(FuelModelSet fuelModelSet)
        {
            fuelModelSet_ = fuelModelSet;
            surfaceFuel_ = new Surface(fuelModelSet);
            crownFuel_ = new Surface(fuelModelSet);            
            initializeMembers();
        }

        ~Crown()
        {

        }

        public Crown(Crown rhs)
        {
            surfaceFuel_ = new Surface(rhs.fuelModelSet_);
            crownFuel_ = new Surface(rhs.fuelModelSet_);
            memberwiseCopyAssignment(rhs);
        }

        /*Crown operator=(Crown rhs)
        {
            if (this != rhs)
            {
                memberwiseCopyAssignment(rhs);
            }
            return this;
        }*/

        void memberwiseCopyAssignment(Crown rhs)
        {
            fuelModelSet_ = rhs.fuelModelSet_;
            surfaceFuel_ = rhs.surfaceFuel_;
            crownFuel_ = rhs.crownFuel_;
            crownInputs_ = rhs.crownInputs_;

            fireType_ = rhs.fireType_;
            surfaceFireHeatPerUnitArea_ = rhs.surfaceFireHeatPerUnitArea_;
            surfaceFirelineIntensity_ = rhs.surfaceFirelineIntensity_;
            crownFuelLoad_ = rhs.crownFuelLoad_;
            canopyHeatPerUnitArea_ = rhs.canopyHeatPerUnitArea_;
            crownFireHeatPerUnitArea_ = rhs.crownFireHeatPerUnitArea_;
            crownFirelineIntensity_ = rhs.crownFirelineIntensity_;
            crownFlameLength_ = rhs.crownFlameLength_;
            crownFireSpreadRate_ = rhs.crownFireSpreadRate_;
            crownCriticalSurfaceFirelineIntensity_ = rhs.crownCriticalSurfaceFirelineIntensity_;
            crownCriticalFireSpreadRate_ = rhs.crownCriticalFireSpreadRate_;
            crownCriticalSurfaceFlameLength_ = rhs.crownCriticalSurfaceFlameLength_;
            crownPowerOfFire_ = rhs.crownPowerOfFire_;
            crownPowerOfWind_ = rhs.crownPowerOfWind_;
            crownFirePowerRatio_ = rhs.crownFirePowerRatio_;
            crownFireActiveRatio_ = rhs.crownFireActiveRatio_;
            crownFireTransitionRatio_ = rhs.crownFireTransitionRatio_;
            windSpeedAtTwentyFeet_ = rhs.windSpeedAtTwentyFeet_;
            crownFireLengthToWidthRatio_ = rhs.crownFireLengthToWidthRatio_;

            surfaceFireSpreadRate_ = rhs.surfaceFireSpreadRate_;
            surfaceFireCriticalSpreadRate_ = rhs.surfaceFireCriticalSpreadRate_;

            passiveCrownFireSpreadRate_ = rhs.passiveCrownFireSpreadRate_;
            passiveCrownFireHeatPerUnitArea_ = rhs.passiveCrownFireHeatPerUnitArea_;
            passiveCrownFireLineIntensity_ = rhs.passiveCrownFireLineIntensity_;
            passiveCrownFireFlameLength_ = rhs.passiveCrownFireFlameLength_;

            finalSpreadRate_ = rhs.finalSpreadRate_;
            finalHeatPerUnitArea_ = rhs.finalHeatPerUnitArea_;
            finalFirelineIntesity_ = rhs.finalFirelineIntesity_;
            finalFlameLength_ = rhs.finalFlameLength_;

            isSurfaceFire_ = rhs.isSurfaceFire_;
            isPassiveCrownFire_ = rhs.isPassiveCrownFire_;
            isActiveCrownFire_ = rhs.isActiveCrownFire_;

            crownFireActiveWindSpeed_ = rhs.crownFireActiveWindSpeed_;
            crownFractionBurned_ = rhs.crownFractionBurned_;
        }

        public void doCrownRunRothermel()
        {
            // This method uses Rothermel's 1991 crown fire correlation to calculate Crown fire average spread rate (ft/min)

            double canopyHeight = surfaceFuel_.getCanopyHeight(LengthUnits.LengthUnitsEnum.Feet);
            double canopyBaseHeight = crownInputs_.getCanopyBaseHeight(LengthUnits.LengthUnitsEnum.Feet);
            double crownRatio = (canopyHeight - canopyBaseHeight) / canopyHeight;
            surfaceFuel_.setCrownRatio(crownRatio);

            // Step 1: Do surface run and store values needed for further calculations 
            surfaceFuel_.doSurfaceRunInDirectionOfMaxSpread(); // Crown ROS output given in direction of max spread 
            surfaceFireHeatPerUnitArea_ = surfaceFuel_.getHeatPerUnitArea();
            surfaceFirelineIntensity_ = surfaceFuel_.getFirelineIntensity(FirelineIntensityUnits.FirelineIntensityUnitsEnum.BtusPerFootPerSecond);

            //bran-jnw
            surfaceFireDirectionOfMaxSpread_ = surfaceFuel_.getDirectionOfMaxSpread();
            surfaceFireEccentricity_ = surfaceFuel_.getFireEccentricity();

            surfaceFuel_.setWindAdjustmentFactorCalculationMethod(WindAdjustmentFactorCalculationMethod.UserInput);
            double windAdjustmentFactor = 0.4; // Wind adjustment factor is assumed to be 0.4
            surfaceFuel_.setUserProvidedWindAdjustmentFactor(windAdjustmentFactor);

            // Step 2: Create the crown fuel model (fire behavior fuel model 10)
            surfaceFuel_.setFuelModelNumber(10); // Set the fuel model used to fuel model 10
            surfaceFuel_.setSlope(0.0, SlopeUnits.SlopeUnitsEnum.Degrees); // Slope is assumed to be zero
            surfaceFuel_.setWindAndSpreadOrientationMode(WindAndSpreadOrientationMode.RelativeToUpslope);
            surfaceFuel_.setWindDirection(0.0); // Wind direction is assumed to be upslope

            // Step 3: Determine crown fire behavior
            surfaceFuel_.doSurfaceRunInDirectionOfMaxSpread();
            crownFireSpreadRate_ = 3.34 * surfaceFuel_.getSpreadRate(SpeedUnits.SpeedUnitsEnum.FeetPerMinute); // Rothermel 1991

            // Step 4: Calculate remaining crown fire characteristics
            calculateCrownFuelLoad();
            calculateCanopyHeatPerUnitArea();
            calculateCrownFireHeatPerUnitArea();
            calculateCrownFirelineIntensity();
            calculateCrownFlameLength();

            calculateCrownCriticalFireSpreadRate();
            calculateCrownFireActiveRatio();

            calculateCrownCriticalSurfaceFireIntensity();
            calculateCrownCriticalSurfaceFlameLength();
            calculateCrownFireTransitionRatio();

            calculateCrownPowerOfFire();
            calculateWindSpeedAtTwentyFeet();
            calcuateCrownPowerOfWind();
            calculateCrownLengthToWidthRatio();
            calcualteCrownFirePowerRatio();

            // Determine if/what type of crown fire has occured
            calculateFireTypeRothermel();
        }

        public void doCrownRunScottAndReinhardt()
        {
            // Scott and Reinhardt (2001) linked models method for crown fire

            double canopyHeight = surfaceFuel_.getCanopyHeight(LengthUnits.LengthUnitsEnum.Feet);
            double canopyBaseHeight = crownInputs_.getCanopyBaseHeight(LengthUnits.LengthUnitsEnum.Feet);
            double crownRatio = (canopyHeight - canopyBaseHeight) / canopyHeight;

            surfaceFuel_.setCrownRatio(crownRatio);

            // Step 1: Do surface run and store values needed for further calculations
            surfaceFuel_.doSurfaceRunInDirectionOfMaxSpread();
            surfaceFireSpreadRate_ = surfaceFuel_.getSpreadRate(SpeedUnits.SpeedUnitsEnum.FeetPerMinute); // Rothermel 1991
            surfaceFireHeatPerUnitArea_ = surfaceFuel_.getHeatPerUnitArea();
            surfaceFirelineIntensity_ = surfaceFuel_.getFirelineIntensity(FirelineIntensityUnits.FirelineIntensityUnitsEnum.BtusPerFootPerSecond);
            surfaceFireFlameLength_ = surfaceFuel_.getFlameLength(LengthUnits.LengthUnitsEnum.Feet); // Byram

            //bran-jnw
            surfaceFireDirectionOfMaxSpread_ = surfaceFuel_.getDirectionOfMaxSpread();
            surfaceFireEccentricity_ = surfaceFuel_.getFireEccentricity();

            // Step 2: Create the crown fuel model (fire behavior fuel model 10)
            crownFuel_ = surfaceFuel_;
            crownFuel_.setFuelModelNumber(10); // Set the crown fuel model used to fuel model 10
            crownFuel_.setUserProvidedWindAdjustmentFactor(0.4); // Wind adjustment factor is assumed to be 0.4
            crownFuel_.setWindAdjustmentFactorCalculationMethod(WindAdjustmentFactorCalculationMethod.UserInput);
            crownFuel_.setSlope(0, SlopeUnits.SlopeUnitsEnum.Degrees);
            crownFuel_.setWindAndSpreadOrientationMode(WindAndSpreadOrientationMode.RelativeToUpslope);
            crownFuel_.setWindDirection(0.0); // Wind direction is assumed to be upslope
            crownFuel_.setWindSpeed(surfaceFuel_.getWindSpeed(SpeedUnits.SpeedUnitsEnum.FeetPerMinute, WindHeightInputMode.TwentyFoot), SpeedUnits.SpeedUnitsEnum.FeetPerMinute, WindHeightInputMode.TwentyFoot);

            // Step 3: Determine crown fire behavior
            crownFuel_.doSurfaceRunInDirectionOfMaxSpread();
            crownFireSpreadRate_ = 3.34 * crownFuel_.getSpreadRate(SpeedUnits.SpeedUnitsEnum.FeetPerMinute); // Rothermel 1991

            // Step 4: Calculate remaining crown fire characteristics
            calculateCrownFireActiveWindSpeed();
            calculateCrownFuelLoad();
            calculateCanopyHeatPerUnitArea();
            calculateCrownFireHeatPerUnitArea();
            calculateCrownFirelineIntensity();
            calculateCrownFlameLength();
            calculateCrownCriticalFireSpreadRate();
            calculateCrownCriticalSurfaceFireIntensity();
            calculateCrownCriticalSurfaceFlameLength();
            calculateCrownFireActiveRatio();
            calculateCrownFireTransitionRatio();

            calculateCrownPowerOfFire();
            calculateWindSpeedAtTwentyFeet();
            calcuateCrownPowerOfWind();
            calculateCrownLengthToWidthRatio();
            calcualteCrownFirePowerRatio();

            // Determine if/what type of crown fire has occured
            calculateFireTypeRothermel();
            calculateFireTypeScottAndReinhardt();

            // Scott & Reinhardt's critical surface fire spread rate (R'initiation) (ft/min)
            calculateSurfaceFireCriticalSpreadRateScottAndReinhardt();

            calculateCrowningSurfaceFireRateOfSpread();

            // Scott & Reinhardt crown fraction burned
            calculateCrownFractionBurned();

            // Scott & Reinhardt torching (passive crown) spread rate, hpua, fireline intensity
            passiveCrownFireSpreadRate_ = surfaceFireSpreadRate_ + crownFractionBurned_ * (crownFireSpreadRate_ - surfaceFireSpreadRate_);
            passiveCrownFireHeatPerUnitArea_ = surfaceFireHeatPerUnitArea_ + canopyHeatPerUnitArea_ * crownFractionBurned_;
            passiveCrownFireLineIntensity_ = passiveCrownFireHeatPerUnitArea_ * passiveCrownFireSpreadRate_ / 60.0;

            // Scott & Reinhardt torching (passive) flame length
            calculatePassiveCrownFlameLength();

            // Determine final fire behavior
            assignFinalFireBehaviorBasedOnFireType();
        }

        void calculateCrownFractionBurned()
        {
            // Calculates the crown fraction burned as per Scott & Reinhardt.
            // Using these parameters:
            // surfaceFireSpreadRate_: the "actual" surface fire spread rate (ft/min).
            // surfaceFireCriticalSpreadRate_: surface fire spread rate required to initiate torching/crowning (ft/min).
            // crowningSurfaceFireRos_: Surface fire spread rate at which the active crown fire spread rate is fully achieved 
            // and the crown fraction burned is 1.

            double numerator = surfaceFireSpreadRate_ - surfaceFireCriticalSpreadRate_;
            double denominator = crowningSurfaceFireRos_ - surfaceFireCriticalSpreadRate_;

            crownFractionBurned_ = (denominator > 1e-07) ? (numerator / denominator) : 0.0;
            crownFractionBurned_ = (crownFractionBurned_ > 1.0) ? 1.0 : crownFractionBurned_;
            crownFractionBurned_ = (crownFractionBurned_ < 0.0) ? 0.0 : crownFractionBurned_;
        }

        void assignFinalFireBehaviorBasedOnFireType()
        {
            if (isSurfaceFire_)
            {
                finalSpreadRate_ = surfaceFireSpreadRate_;
                finalHeatPerUnitArea_ = surfaceFireHeatPerUnitArea_;
                finalFirelineIntesity_ = surfaceFirelineIntensity_;
                finalFlameLength_ = surfaceFireFlameLength_;
            }
            else if (isPassiveCrownFire_)
            {
                finalSpreadRate_ = passiveCrownFireSpreadRate_;
                finalHeatPerUnitArea_ = passiveCrownFireHeatPerUnitArea_;
                finalFirelineIntesity_ = passiveCrownFireLineIntensity_;
                finalFlameLength_ = passiveCrownFireFlameLength_;
            }
            else if (isActiveCrownFire_)
            {
                finalSpreadRate_ = crownFireSpreadRate_;
                finalHeatPerUnitArea_ = crownFireHeatPerUnitArea_;
                finalFirelineIntesity_ = crownFirelineIntensity_;
                finalFlameLength_ = crownFlameLength_;
            }
        }

        void calculateCrownFireActiveWindSpeed()
        {
            // O'active is the 20-ft wind speed at which the crown canopy becomes fully available for active fire spread and:
            // the crown fraction burned approaches 1, Ractive == R'active, and the surface fire spread rate would equal R'sa.
            // See Scott & Reinhardt(2001) equation 20 on page 19.

            double cbd = 16.0185 * getCanopyBulkDensity(DensityUnits.DensityUnitsEnum.PoundsPerCubicFoot);
            double ractive = 3.28084 * (3.0 / cbd);         // R'active, ft/min
            double r10 = ractive / 3.34;                    // R'active = 3.324 * R10
            double propFlux = 0.048317062998571636;         // Fuel model 10 actual propagating flux ratio
            double reactionIntensity = crownFuel_.getReactionIntensity(HeatSourceAndReactionIntensityUnits.HeatSourceAndReactionIntensityUnitsEnum.BtusPerSquareFootPerMinute);
            double heatSink = crownFuel_.getHeatSink(HeatSinkUnits.HeatSinkUnitsEnum.BtusPerCubicFoot);
            double ros0 = reactionIntensity * propFlux / heatSink;
            double windB = 1.4308256324729873;              // Fuel model 10 actual wind factor B
            double windBInv = 1.0 / windB;                  // Fuel model 10 actual inverse of wind factor B
            double windK = 0.0016102128596515481;           // Fuel model 10 actual K = C*pow((beta/betOpt),-E)
            double slopeFactor = 0.0;
            double a = ((r10 / ros0) - 1.0 - slopeFactor) / windK;
            double uMid = pow(a, windBInv);                 // midflame wind speed (ft/min)
            crownFireActiveWindSpeed_ = uMid / 0.4;         // 20-ft wind speed (ft/min) for waf=0.4
        }

        public double getCrownFireSpreadRate(SpeedUnits.SpeedUnitsEnum spreadRateUnits)
        {
            return SpeedUnits.fromBaseUnits(crownFireSpreadRate_, spreadRateUnits);
        }

        public double getSurfaceFireSpreadRate(SpeedUnits.SpeedUnitsEnum spreadRateUnits)
        {
            return surfaceFuel_.getSpreadRate(spreadRateUnits);
        }

        public double getCrownFirelineIntensity()
        {
            return crownFirelineIntensity_;
        }

        public double getCrownFlameLength(LengthUnits.LengthUnitsEnum flameLengthUnits)
        {
            return LengthUnits.fromBaseUnits(crownFlameLength_, flameLengthUnits);
        }

        FireType.FireTypeEnum getFireType()
        {
            return fireType_;
        }

        public double getFinalSpreadRate(SpeedUnits.SpeedUnitsEnum spreadRateUnits)
        {
            return SpeedUnits.fromBaseUnits(finalSpreadRate_, spreadRateUnits);
        }

        public double getFinalHeatPerUnitArea()
        {
            return finalHeatPerUnitArea_;
        }

        public double getFinalFirelineIntesity(FirelineIntensityUnits.FirelineIntensityUnitsEnum firelineIntensityUnits)
        {
            return FirelineIntensityUnits.fromBaseUnits(finalFirelineIntesity_, firelineIntensityUnits);
        }

        public double getFinalFlameLength(LengthUnits.LengthUnitsEnum flameLengthUnits)
        {
            return LengthUnits.fromBaseUnits(finalFlameLength_, flameLengthUnits);
        }

        public double getCrownFireLengthToWidthRatio()
        {
            return crownFireLengthToWidthRatio_;
        }

        public double getCriticalOpenWindSpeed(SpeedUnits.SpeedUnitsEnum speedUnits)
        {
            return SpeedUnits.fromBaseUnits(crownFireActiveWindSpeed_, speedUnits);
        }

        //bran-jnw
        public double getSurfaceFireDirectionOfMaxSpreadRate()
        {
            return surfaceFireDirectionOfMaxSpread_;
        }
        //bran-jnw
        public double getSurfaceFireEccentricity()
        {
            return surfaceFireEccentricity_;
        }

        void initializeMembers()
        {
            fireType_ = FireType.FireTypeEnum.Surface;
            surfaceFireHeatPerUnitArea_ = 0.0;
            surfaceFirelineIntensity_ = 0.0;
            crownFuelLoad_ = 0.0;
            canopyHeatPerUnitArea_ = 0.0;
            crownFireHeatPerUnitArea_ = 0.0;
            crownFirelineIntensity_ = 0.0;
            crownFlameLength_ = 0.0;
            crownFireSpreadRate_ = 0.0;
            crownCriticalSurfaceFirelineIntensity_ = 0.0;
            crownCriticalFireSpreadRate_ = 0.0;
            crownCriticalSurfaceFlameLength_ = 0.0;
            crownPowerOfFire_ = 0.0;
            crownPowerOfWind_ = 0.0;
            crownFirePowerRatio_ = 0.0;
            crownFireActiveRatio_ = 0.0;
            crownFireTransitionRatio_ = 0.0;
            windSpeedAtTwentyFeet_ = 0.0; ;
            crownFireLengthToWidthRatio_ = 1.0;

            surfaceFireSpreadRate_ = 0.0;
            surfaceFireCriticalSpreadRate_ = 0.0;

            passiveCrownFireSpreadRate_ = 0.0;
            passiveCrownFireHeatPerUnitArea_ = 0.0;
            passiveCrownFireLineIntensity_ = 0.0;
            passiveCrownFireFlameLength_ = 0.0;

            isSurfaceFire_ = false;
            isPassiveCrownFire_ = false;
            isActiveCrownFire_ = false;

            crownFireActiveWindSpeed_ = 0.0;

            //bran-jnw: added creation of object
            crownInputs_ = new CrownInputs();
            crownInputs_.initializeMembers();
        }

        void calculateCanopyHeatPerUnitArea()
        {
            const double LOW_HEAT_OF_COMBUSTION = 8000.0; // Low heat of combustion (hard coded to 8000 Btu/lbs)
            canopyHeatPerUnitArea_ = crownFuelLoad_ * LOW_HEAT_OF_COMBUSTION;
        }

        void calculateCrownFireHeatPerUnitArea()
        {
            crownFireHeatPerUnitArea_ = surfaceFireHeatPerUnitArea_ + canopyHeatPerUnitArea_;
        }

        void calculateCrownFuelLoad()
        {
            double canopyBulkDensity = crownInputs_.getCanopyBulkDensity(DensityUnits.DensityUnitsEnum.PoundsPerCubicFoot);
            double canopyBaseHeight = crownInputs_.getCanopyBaseHeight(LengthUnits.LengthUnitsEnum.Feet);
            double canopyHeight = surfaceFuel_.getCanopyHeight(LengthUnits.LengthUnitsEnum.Feet);
            crownFuelLoad_ = canopyBulkDensity * (canopyHeight - canopyBaseHeight);
        }

        void calculateCrownFireTransitionRatio()
        {
            crownFireTransitionRatio_ = ((crownCriticalSurfaceFirelineIntensity_ < 1.0e-7)
                 ? (0.00)
                 : (surfaceFirelineIntensity_ / crownCriticalSurfaceFirelineIntensity_));
        }

        void calculateCrownFirelineIntensity()
        {
            crownFirelineIntensity_ = (crownFireSpreadRate_ / 60.0) * crownFireHeatPerUnitArea_;
        }

        void calculateCrownCriticalSurfaceFireIntensity()
        {
            // Get moisture content in percent and constrain lower limit
            double moistureFoliar = crownInputs_.getMoistureFoliar(MoistureUnits.MoistureUnitsEnum.Percent);
            moistureFoliar = (moistureFoliar < 30.0) ? 30.0 : moistureFoliar;

            // Convert crown base height to meters and constrain lower limit
            double crownBaseHeight = crownInputs_.getCanopyBaseHeight(LengthUnits.LengthUnitsEnum.Meters);
            crownBaseHeight = (crownBaseHeight < 0.1) ? 0.1 : crownBaseHeight;

            // Critical surface fireline intensity (kW/m)
            FirelineIntensityUnits.FirelineIntensityUnitsEnum firelineIntensityUnits = FirelineIntensityUnits.FirelineIntensityUnitsEnum.KilowattsPerMeter;
            crownCriticalSurfaceFirelineIntensity_ = pow((0.010 * crownBaseHeight * (460.0 + 25.9 * moistureFoliar)), 1.5);

            // Return as Btu/ft/s
            crownCriticalSurfaceFirelineIntensity_ = FirelineIntensityUnits.toBaseUnits(crownCriticalSurfaceFirelineIntensity_, firelineIntensityUnits);
        }

        void calculateCrownCriticalSurfaceFlameLength()
        {
            crownCriticalSurfaceFlameLength_ = surfaceFuel_.calculateFlameLength(crownCriticalSurfaceFirelineIntensity_);
        }

        void calculateCrownFlameLength()
        {
            // Uses Thomas's (1963) flame length (ft) given a fireline intensity (Btu/ft/s).
            if (crownFirelineIntensity_ <= 0.0)
            {
                crownFlameLength_ = 0.0;
            }
            else
            {
                crownFlameLength_ = 0.2 * pow(crownFirelineIntensity_, (2.0 / 3.0));
            }
        }

        void calculatePassiveCrownFlameLength()
        {
            // Uses Thomas's (1963) flame length (ft) given a fireline intensity (Btu/ft/s).
            if (passiveCrownFireLineIntensity_ <= 0.0)
            {
                passiveCrownFireFlameLength_ = 0.0;
            }
            else
            {
                passiveCrownFireFlameLength_ = 0.2 * pow(passiveCrownFireLineIntensity_, (2.0 / 3.0));
            }
        }

        void calculateCrownPowerOfFire()
        {
            crownPowerOfFire_ = crownFirelineIntensity_ / 129.0;
        }

        void calcuateCrownPowerOfWind()
        {
            const double SECONDS_PER_MINUTE = 60.0;

            double WindspeedMinusCrownROS = 0.0;

            // Eq. 7, Rothermel 1991
            WindspeedMinusCrownROS = (windSpeedAtTwentyFeet_ - crownFireSpreadRate_) / SECONDS_PER_MINUTE;
            WindspeedMinusCrownROS = (WindspeedMinusCrownROS < 1e-07) ? 0.0 : WindspeedMinusCrownROS;
            crownPowerOfWind_ = 0.00106 * (WindspeedMinusCrownROS * WindspeedMinusCrownROS * WindspeedMinusCrownROS);
        }

        void calcualteCrownFirePowerRatio()
        {
            crownFirePowerRatio_ = (crownPowerOfWind_ > 1e-07) ? (crownPowerOfFire_ / crownPowerOfWind_) : 0.0;
        }


        void calculateSurfaceFireCriticalSpreadRateScottAndReinhardt()
        {
            // Scott & Reinhardt's critical surface fire spread rate (R'initiation) (ft/min)
            surfaceFireCriticalSpreadRate_ = (60.0 * crownCriticalSurfaceFirelineIntensity_) / surfaceFireHeatPerUnitArea_;
        }

        void calculateCrownCriticalFireSpreadRate()
        {
            // Convert canopy bulk density to Kg/m3
            double convertedCanopyBulkDensity = crownInputs_.getCanopyBulkDensity(DensityUnits.DensityUnitsEnum.KilogramsPerCubicMeter);
            crownCriticalFireSpreadRate_ = (convertedCanopyBulkDensity < 1e-07) ? 0.00 : (3.0 / convertedCanopyBulkDensity);

            // Convert spread rate from m/min to ft/min
            crownCriticalFireSpreadRate_ = SpeedUnits.toBaseUnits(crownCriticalFireSpreadRate_, SpeedUnits.SpeedUnitsEnum.MetersPerMinute);
        }

        void calculateCrownFireActiveRatio()
        {
            crownFireActiveRatio_ = (crownCriticalFireSpreadRate_ < 1e-07)
                ? (0.00)
                : (crownFireSpreadRate_ / crownCriticalFireSpreadRate_);
        }

        void calculateWindSpeedAtTwentyFeet()
        {
            WindHeightInputMode windHeightInputMode;
            windHeightInputMode = surfaceFuel_.getWindHeightInputMode();

            if (windHeightInputMode == WindHeightInputMode.TwentyFoot)
            {
                windSpeedAtTwentyFeet_ = surfaceFuel_.getWindSpeed(SpeedUnits.SpeedUnitsEnum.FeetPerMinute, windHeightInputMode);
            }
            else if (windHeightInputMode == WindHeightInputMode.TenMeter)
            {
                //bran-jnw: changed this class to static, see class def
                //WindSpeedUtility windSpeedUtility = new WindSpeedUtility();
                double windSpeedAtTenMeters = surfaceFuel_.getWindSpeed(SpeedUnits.SpeedUnitsEnum.FeetPerMinute, windHeightInputMode);
                windSpeedAtTwentyFeet_ = WindSpeedUtility.windSpeedAtTwentyFeetFromTenMeter(windSpeedAtTenMeters); //windSpeedUtility.windSpeedAtTwentyFeetFromTenMeter(windSpeedAtTenMeters);
            }
        }

        void calculateCrownLengthToWidthRatio()
        {
            //Calculates the crown fire length-to-width ratio given the 20-ft wind speed (in mph)
            // (Rothermel 1991, Equation 10, p16)

            double windSpeed = SpeedUnits.fromBaseUnits(windSpeedAtTwentyFeet_, SpeedUnits.SpeedUnitsEnum.MilesPerHour);
            if (windSpeedAtTwentyFeet_ > 1.0e-07)
            {
                crownFireLengthToWidthRatio_ = 1.0 + 0.125 * windSpeed;
            }
            else
            {
                crownFireLengthToWidthRatio_ = 1.0;
            }
        }

        void calculateCrowningSurfaceFireRateOfSpread()
        {
            surfaceFuel_.setWindSpeed(crownFireActiveWindSpeed_, SpeedUnits.SpeedUnitsEnum.FeetPerMinute, WindHeightInputMode.TwentyFoot);
            surfaceFuel_.doSurfaceRunInDirectionOfMaxSpread(); // Do crown run with crowning fire active wind speed
            crowningSurfaceFireRos_ = surfaceFuel_.getSpreadRate(SpeedUnits.SpeedUnitsEnum.FeetPerMinute);
        }

        void calculateFireTypeRothermel()
        {
            fireType_ = FireType.FireTypeEnum.Surface;
            // If the fire CAN NOT transition to the crown ...
            if (crownFireTransitionRatio_ < 1.0)
            {
                if (crownFireActiveRatio_ < 1.0)
                {
                    fireType_ = FireType.FireTypeEnum.Surface; // Surface fire
                }
                else // crownFireActiveRatio_ >= 1.0 
                {
                    fireType_ = FireType.FireTypeEnum.ConditionalCrownFire; // Conditional crown fire
                }
            }
            // If the fire CAN transition to the crown ...
            else // crownFireTransitionRatio_ >= 1.0 )
            {
                if (crownFireActiveRatio_ < 1.0)
                {
                    fireType_ = FireType.FireTypeEnum.Torching; // Torching
                }
                else // crownFireActiveRatio_ >= 1.0
                {
                    fireType_ = FireType.FireTypeEnum.Crowning; // Crowning
                }
            }
        }

        void calculateFireTypeScottAndReinhardt()
        {
            // Final fire type
            isSurfaceFire_ = fireType_ == FireType.FireTypeEnum.Surface || fireType_ == FireType.FireTypeEnum.ConditionalCrownFire;
            isPassiveCrownFire_ = fireType_ == FireType.FireTypeEnum.Torching;
            isActiveCrownFire_ = fireType_ == FireType.FireTypeEnum.Crowning;
            isCrownFire_ = isActiveCrownFire_ || isPassiveCrownFire_;
        }

        public void updateCrownInputs(int fuelModelNumber, double moistureOneHour, double moistureTenHour, double moistureHundredHour,
            double moistureLiveHerbaceous, double moistureLiveWoody, double moistureFoliar, MoistureUnits.MoistureUnitsEnum moistureUnits,
            double windSpeed, SpeedUnits.SpeedUnitsEnum windSpeedUnits, WindHeightInputMode windHeightInputMode,
            double windDirection, WindAndSpreadOrientationMode windAndSpreadOrientationMode, double slope,
            SlopeUnits.SlopeUnitsEnum slopeUnits, double aspect, double canopyCover, CoverUnits.CoverUnitsEnum coverUnits, double canopyHeight,
            double canopyBaseHeight, LengthUnits.LengthUnitsEnum canopyHeightUnits, double crownRatio, double canopyBulkDensity,
            DensityUnits.DensityUnitsEnum densityUnits)
        {
            surfaceFuel_.updateSurfaceInputs(fuelModelNumber, moistureOneHour, moistureTenHour, moistureHundredHour, moistureLiveHerbaceous,
                moistureLiveWoody, moistureUnits, windSpeed, windSpeedUnits, windHeightInputMode, windDirection,
                windAndSpreadOrientationMode, slope, slopeUnits, aspect, canopyCover, coverUnits, canopyHeight, canopyHeightUnits, crownRatio);
            crownInputs_.updateCrownInputs(canopyBaseHeight, canopyHeightUnits, canopyBulkDensity, densityUnits, moistureFoliar, moistureUnits);
        }

        public void setCanopyBaseHeight(double canopyBaseHeight, LengthUnits.LengthUnitsEnum heightUnits)
        {
            crownInputs_.setCanopyBaseHeight(canopyBaseHeight, heightUnits);
        }

        public void setCanopyBulkDensity(double canopyBulkDensity, DensityUnits.DensityUnitsEnum densityUnits)
        {
            crownInputs_.setCanopyBulkDensity(canopyBulkDensity, densityUnits);
        }

        public void setMoistureFoliar(double moistureFoliar, MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            crownInputs_.setMoistureFoliar(moistureFoliar, moistureUnits);
        }

        public void updateCrownsSurfaceInputs(int fuelModelNumber, double moistureOneHour, double moistureTenHour,
            double moistureHundredHour, double moistureLiveHerbaceous, double moistureLiveWoody,
            MoistureUnits.MoistureUnitsEnum moistureUnits, double windSpeed,
            SpeedUnits.SpeedUnitsEnum windSpeedUnits, WindHeightInputMode windHeightInputMode,
            double windDirection, WindAndSpreadOrientationMode windAndSpreadOrientationMode,
            double slope, SlopeUnits.SlopeUnitsEnum slopeUnits, double aspect, double canopyCover, CoverUnits.CoverUnitsEnum coverUnits,
            double canopyHeight, LengthUnits.LengthUnitsEnum canopyHeightUnits, double crownRatio)
        {
            surfaceFuel_.updateSurfaceInputs(fuelModelNumber, moistureOneHour, moistureTenHour, moistureHundredHour,
                moistureLiveHerbaceous, moistureLiveWoody, moistureUnits, windSpeed, windSpeedUnits, windHeightInputMode,
                windDirection, windAndSpreadOrientationMode, slope, slopeUnits, aspect, canopyCover, coverUnits,
                canopyHeight, canopyHeightUnits, crownRatio);
        }

        public void setCanopyCover(double canopyCover, CoverUnits.CoverUnitsEnum coverUnits)
        {
            surfaceFuel_.setCanopyCover(canopyCover, coverUnits);
        }

        public void setCanopyHeight(double canopyHeight, LengthUnits.LengthUnitsEnum canopyHeightUnits)
        {
            surfaceFuel_.setCanopyHeight(canopyHeight, canopyHeightUnits);
        }

        public void setCrownRatio(double crownRatio)
        {
            surfaceFuel_.setCrownRatio(crownRatio);
        }

        public void setFuelModelSet(FuelModelSet fuelModelSet)
        {
            fuelModelSet_ = fuelModelSet;
        }


        public void setFuelModelNumber(int fuelModelNumber)
        {
            surfaceFuel_.setFuelModelNumber(fuelModelNumber);
        }

        public void setMoistureOneHour(double moistureOneHour, MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            surfaceFuel_.setMoistureOneHour(moistureOneHour, moistureUnits);
        }

        public void setMoistureTenHour(double moistureTenHour, MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            surfaceFuel_.setMoistureTenHour(moistureTenHour, moistureUnits);
        }

        public void setMoistureHundredHour(double moistureHundredHour, MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            surfaceFuel_.setMoistureHundredHour(moistureHundredHour, moistureUnits);
        }

        public void setMoistureLiveHerbaceous(double moistureLiveHerbaceous, MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            surfaceFuel_.setMoistureLiveHerbaceous(moistureLiveHerbaceous, moistureUnits);
        }

        public void setMoistureLiveWoody(double moistureLiveWoody, MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            surfaceFuel_.setMoistureLiveWoody(moistureLiveWoody, moistureUnits);
        }

        public void setSlope(double slope, SlopeUnits.SlopeUnitsEnum slopeUnits)
        {
            surfaceFuel_.setSlope(slope, slopeUnits);
        }

        public void setAspect(double aspect)
        {
            surfaceFuel_.setAspect(aspect);
        }

        public void setWindSpeed(double windSpeed, SpeedUnits.SpeedUnitsEnum windSpeedUnits, WindHeightInputMode windHeightInputMode)
        {
            surfaceFuel_.setWindSpeed(windSpeed, windSpeedUnits, windHeightInputMode);
        }

        public void setWindDirection(double windDirection)
        {
            surfaceFuel_.setWindDirection(windDirection);
        }

        public void setWindHeightInputMode(WindHeightInputMode windHeightInputMode)
        {
            surfaceFuel_.setWindHeightInputMode(windHeightInputMode);
        }

        public void setWindAndSpreadOrientationMode(WindAndSpreadOrientationMode windAndSpreadAngleMode)
        {
            surfaceFuel_.setWindAndSpreadOrientationMode(windAndSpreadAngleMode);
        }

        public void setUserProvidedWindAdjustmentFactor(double userProvidedWindAdjustmentFactor)
        {
            surfaceFuel_.setUserProvidedWindAdjustmentFactor(userProvidedWindAdjustmentFactor);
        }

        public void setWindAdjustmentFactorCalculationMethod(WindAdjustmentFactorCalculationMethod windAdjustmentFactorCalculationMethod)
        {
            surfaceFuel_.setWindAdjustmentFactorCalculationMethod(windAdjustmentFactorCalculationMethod);
        }

        public int getFuelModelNumber()
        {
            return surfaceFuel_.getFuelModelNumber();
        }

        public double getMoistureOneHour(MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            return surfaceFuel_.getMoistureOneHour(moistureUnits);
        }

        public double getMoistureTenHour(MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            return surfaceFuel_.getMoistureTenHour(moistureUnits);
        }

        public double getMoistureHundredHour(MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            return surfaceFuel_.getMoistureHundredHour(moistureUnits);
        }

        public double getMoistureLiveHerbaceous(MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            return surfaceFuel_.getMoistureLiveHerbaceous(moistureUnits);
        }

        public double getMoistureLiveWoody(MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            return surfaceFuel_.getMoistureLiveWoody(moistureUnits);
        }

        public double getWindSpeed(SpeedUnits.SpeedUnitsEnum windSpeedUnits, WindHeightInputMode windHeightInputMode)
        {
            return surfaceFuel_.getWindSpeed(windSpeedUnits, windHeightInputMode);
        }

        public double getWindDirection()
        {
            return surfaceFuel_.getWindDirection();
        }

        public double getSlope(SlopeUnits.SlopeUnitsEnum slopeUnits)
        {
            return surfaceFuel_.getSlope(slopeUnits);
        }

        public double getAspect()
        {
            return surfaceFuel_.getAspect();
        }

        public double getCanopyCover(CoverUnits.CoverUnitsEnum canopyCoverUnits)
        {
            return surfaceFuel_.getCanopyCover(canopyCoverUnits);
        }

        public double getCanopyHeight(LengthUnits.LengthUnitsEnum canopyHeighUnits)
        {
            return surfaceFuel_.getCanopyHeight(canopyHeighUnits);
        }

        public double getCrownRatio()
        {
            return surfaceFuel_.getCrownRatio();
        }

        public double getCanopyBaseHeight(LengthUnits.LengthUnitsEnum canopyHeightUnits)
        {
            return crownInputs_.getCanopyBaseHeight(canopyHeightUnits);
        }

        public double getCanopyBulkDensity(DensityUnits.DensityUnitsEnum canopyBulkDensityUnits)
        {
            return crownInputs_.getCanopyBulkDensity(canopyBulkDensityUnits);
        }

        public double getMoistureFoliar(MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            return crownInputs_.getMoistureFoliar(moistureUnits);
        }
    }
}

