using System;
using PREACT.Math;

namespace PREACT.Wildfire
{
    public class SpreadModelLookUpTable : SpreadModel
    {
        double _noWindNoSlopeSpreadRate;
        double _directionOfMaxSpread;
        bool _hasFuelLoad;
        double _eccentricity;
        double _forwardSpreadRate;

        public SpreadModelLookUpTable(LandscapeCellData cellData, SpreadRateLookUpTable lookUpTable)
        {
            _noWindNoSlopeSpreadRate = lookUpTable.GetSpreadRate(cellData.fuel_model);
            _hasFuelLoad = _noWindNoSlopeSpreadRate > 0;
        }

        public override void CalculateSpreadRate(WeatherManager weather, TimeManager time)
        {
            weather.GetWind(out double windSpeed, out double windAzimuth);
            windAzimuth += 180;
            if(windAzimuth > 360)
            {
                windAzimuth -= 360;
            }
            _directionOfMaxSpread = windAzimuth;

            double windFactor = Mathd.Min(10, 0.05 * windSpeed);
            _forwardSpreadRate = _noWindNoSlopeSpreadRate * (1.0 + windFactor);
        }

        public override double GetDirectionOfMaxSpread()
        {
            return _directionOfMaxSpread;
        }

        public override double GetFireIntensity()
        {
            return 1000;
        }

        public override double GetMaxSpreadRate()
        {
            return _noWindNoSlopeSpreadRate;
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
                    double radians = beta * Mathd.Deg2Rad;
                    rosDirection = _forwardSpreadRate * (1.0 - _eccentricity) / (1.0 - _eccentricity * Mathd.Cos(radians));
                }
            }
            return rosDirection;
        }

        public override bool HasFuelLoad()
        {
            return _hasFuelLoad;
        }
    }
}
