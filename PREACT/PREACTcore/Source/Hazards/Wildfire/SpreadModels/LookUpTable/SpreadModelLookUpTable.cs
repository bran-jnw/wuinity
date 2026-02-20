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

            double windFactor = Mathd.Min(10, 0.01 * windSpeed);
            _forwardSpreadRate = _noWindNoSlopeSpreadRate * (1.0 + windFactor);
            double lToW = LengthToWidth(windSpeed * 3.6);
            _eccentricity = CalculateEccentricity(lToW);
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

        private static double LengthToWidth(double U_10)
        {
            double LBR = 1.0;
            if (U_10 < 5)
            {
                LBR = 1.0;
            }
            else
            {
                LBR = 1.1 * Mathd.Pow(U_10, 0.464);
            }

            return Mathd.Min(8.0, LBR);
        }

        private static double CalculateEccentricity(double fireLengthToWidthRatio)
        {
            double eccentricity = 0.0;
            double x = (fireLengthToWidthRatio * fireLengthToWidthRatio) - 1.0;
            if (x > 0.0)
            {
                eccentricity = Mathd.Sqrt(x) / fireLengthToWidthRatio;
            }

            return eccentricity;
        }

        public override bool HasFuelLoad()
        {
            return _hasFuelLoad;
        }
    }
}
