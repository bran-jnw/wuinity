using System;
using PREACT.Math;

namespace PREACT.Wildfire
{
    public class SpreadModelLookupROS : SpreadModel
    {
        private static readonly double _kmPerHourToMeterPerSecond = 1.0 / 3.6;

        LookupROSEntry _fuel;
        double _directionOfMaxSpread;
        bool _hasFuelLoad;
        double _eccentricity;
        double _forwardSpreadRate;

        public SpreadModelLookupROS(LandscapeCellData cellData, LookupROSTable lookUpTable)
        {
            _fuel = lookUpTable.GetSpreadRate(cellData.fuel_model, out bool success);
            if(success && _fuel.NoWindNoSlopeROS > 0.0)
            {
                _hasFuelLoad = true;
            }
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

            if(_fuel.WindCoefficient >= 0) //linear
            {
                double phiWind = windSpeed * _fuel.WindCoefficient;
                _forwardSpreadRate = _fuel.NoWindNoSlopeROS * (1.0 + phiWind);
            }
            else//exponential
            {
                double windMultiplier = Mathd.Exp(windSpeed * _fuel.WindCoefficient);
                _forwardSpreadRate = _fuel.NoWindNoSlopeROS * windMultiplier;
            }
            
            double lToW = LengthToWidth(windSpeed);
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
            return _forwardSpreadRate;
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

        //Alexander (1985), m/s
        private static double LengthToWidth(double U_10)
        {
            double LBR = 1 + 0.0189433521 * Mathd.Pow(U_10, 2.154); //coeff = 0.00120 (original) * 3.6 ^2.154 to get m/s input

            return Mathd.Min(6.5, LBR);
        }

        //m/s
        private static double LengthToWidthFarsite(double effectiveWindSpeed)
        {
            double LBR = 0.936 * Mathd.Exp(0.2566 *  effectiveWindSpeed) + 0.461 * Mathd.Exp(-0.1548 * effectiveWindSpeed) - 0.397;

            return Mathd.Min(8.0, LBR);
        }

        //m/s
        private static double LengthToWidthGrass(double U_10)
        {
            U_10 *= 3.6;
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
