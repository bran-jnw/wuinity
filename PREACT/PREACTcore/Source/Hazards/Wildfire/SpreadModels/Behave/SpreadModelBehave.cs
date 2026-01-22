using System;
using PREACT.Math;

namespace PREACT.Wildfire
{
    public class SpreadModelBehave : SpreadModel
    {
        public static readonly BehaveCore.TwoFuelModelsMethod.TwoFuelModelsMethodEnum TwoFuelModelsMethod = BehaveCore.TwoFuelModelsMethod.TwoFuelModelsMethodEnum.NoMethod;
        public static readonly BehaveCore.FractionUnits.FractionUnitsEnum MoistureUnits = BehaveCore.FractionUnits.FractionUnitsEnum.Percent;
        public static readonly BehaveCore.WindHeightInputMode.WindHeightInputModeEnum WindHeightInputMode = BehaveCore.WindHeightInputMode.WindHeightInputModeEnum.TenMeter;
        public static readonly BehaveCore.SlopeUnits.SlopeUnitsEnum SlopeUnits = BehaveCore.SlopeUnits.SlopeUnitsEnum.Degrees;
        public static readonly BehaveCore.FractionUnits.FractionUnitsEnum FractionUnits = BehaveCore.FractionUnits.FractionUnitsEnum.Fraction;
        public static readonly BehaveCore.LengthUnits.LengthUnitsEnum LengthUnits = BehaveCore.LengthUnits.LengthUnitsEnum.Meters;
        public static readonly BehaveCore.SpeedUnits.SpeedUnitsEnum WindSpeedUnits = BehaveCore.SpeedUnits.SpeedUnitsEnum.MetersPerSecond;
        public static readonly BehaveCore.WindAndSpreadOrientationMode.WindAndSpreadOrientationModeEnum WindAndSpreadOrientationMode = BehaveCore.WindAndSpreadOrientationMode.WindAndSpreadOrientationModeEnum.RelativeToNorth;
        public static readonly BehaveCore.DensityUnits.DensityUnitsEnum DensityUnits = BehaveCore.DensityUnits.DensityUnitsEnum.KilogramsPerCubicMeter;

        private double _forwardSpreadRate;
        private double _directionOfMaxSpread;
        private double _eccentricity;
        private BehaveCore.Crown _crownBehave;
        private double _firelineIntensity;

        private int _fuelModelNumber;
        private LandscapeCellData _cellData;

        public SpreadModelBehave(BehaveCore.FuelModels fuelModel, LandscapeCellData cellData, InitialFuelMoisture moisture)
        {
            _crownBehave = new BehaveCore.Crown(fuelModel);
            _cellData = cellData;

            double crownRatio = 1.0; //This can be whatever as Behave calculates it internally each time anyway, so not sure why it is an input
            double moistureFoliar = 0;
            _crownBehave.updateCrownInputs(_cellData.fuel_model, 
                moisture.OneHour, moisture.TenHour, moisture.HundredHour, moisture.LiveHerbaceous, moisture.LiveWoody, moistureFoliar, MoistureUnits,
                0, WindSpeedUnits, WindHeightInputMode, 0, WindAndSpreadOrientationMode, 
                _cellData.slope, SlopeUnits,
                _cellData.aspect, _cellData.canopy_cover, FractionUnits, _cellData.crown_canopy_height, _cellData.crown_base, LengthUnits, crownRatio, FractionUnits, _cellData.crown_bulk_density, DensityUnits);
        }

        public override void CalculateSpreadRate()
        {
            _crownBehave.doCrownRunRothermel();
            _forwardSpreadRate = _crownBehave.getFinalSpreadRate(BehaveCore.SpeedUnits.SpeedUnitsEnum.MetersPerSecond);
            _eccentricity = _crownBehave.getFireEccentricity();
            _directionOfMaxSpread = _crownBehave.getDirectionOfMaxSpread();
            _firelineIntensity = _crownBehave.getFinalFirelineIntesity(BehaveCore.FirelineIntensityUnits.FirelineIntensityUnitsEnum.KilowattsPerMeter);
        }

        public override double GetMaxSpreadRate()
        {
            return _forwardSpreadRate;
        }

        public override double GetDirectionOfMaxSpread()
        {
            return _directionOfMaxSpread;
        }

        public override double GetSpreadRateInDirection(double directionOfInterest)
        {
            double rosDirection = _forwardSpreadRate;
            if (_forwardSpreadRate != 0.0) // if forward spread rate is not zero
            {
                // Calculate the fire spread rate in this azimuth
                // if it deviates more than a tenth degree from the maximum azimuth

                // Calculate beta: the angle between the direction of max spread and the direction of interest
                double beta = Mathd.Abs(_directionOfMaxSpread - directionOfInterest);

                // Calculate the fire spread rate in this azimuth
                // if it deviates more than a tenth degree from the maximum azimuth
                if (beta > 180.0)
                {
                    beta = (360.0 - beta);
                }
                if (Mathd.Abs(beta) > 0.1)
                {
                    double radians = beta * Mathd.PI / 180.0;
                    rosDirection = _forwardSpreadRate * (1.0 - _eccentricity) / (1.0 - _eccentricity * Mathd.Cos(radians));
                }
            }
            return rosDirection;
        }

        public override double GetFirelineIntensity()
        {
            return _firelineIntensity;
        }

        public override void SetWind(double direction, double speedMetersPerSecond)
        {
            _crownBehave.setWindDirection(direction);
            _crownBehave.setWindSpeed(speedMetersPerSecond, WindSpeedUnits, WindHeightInputMode);
        }

        public override void SetFuelMoisture(double oneHour, double tenHour, double hundredHour, double liveHerbaceous, double liveWoody, double foliar)
        {
            _crownBehave.setMoistureOneHour(oneHour, MoistureUnits);
            _crownBehave.setMoistureTenHour(tenHour, MoistureUnits);
            _crownBehave.setMoistureHundredHour(hundredHour, MoistureUnits);
            _crownBehave.setMoistureLiveHerbaceous(liveHerbaceous, MoistureUnits);
            _crownBehave.setMoistureLiveWoody(liveWoody, MoistureUnits);
            _crownBehave.setMoistureFoliar(foliar, MoistureUnits);
        }

        public override bool HasFuelLoad()
        {
            return !_crownBehave.isAllFuelLoadZero(_fuelModelNumber);
        }
    }
}
