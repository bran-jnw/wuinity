using System.Collections.Generic;
using PREACT.Math;

namespace PREACT.Wildfire.AFDRS
{
    public class SpreadModelAFDRS : SpreadModel
    {
        public enum FuelModels { Forest, Grassland, Savannah, Spinifex, Heathland, MalleeHeath, Buttongrass, Pine };

        private FuelModels _fuelModel;
        private AFDRSInput _input;
        private AFDRSOutput _output;
        
        private bool _hasFuelLoad;

        private static readonly double _kmPerHourToMeterPerSecond = 1.0 / 3.6;
        private static readonly double _meterPerHourToMeterPerSecond = 1.0 / 3600.0;

        double _eccentricity;

        AFDRSFuelModel _fuelModelCode;

        public SpreadModelAFDRS(LandscapeCellData cellData, SpatialManager spatialManager)
        {
            if (cellData.fuel_model < 1)
            {
                _hasFuelLoad = false;
                return;
            }

            double percentSlope = Mathd.Tan(Mathd.Deg2Rad * cellData.slope) * 100;
            double slopeAzimuth = cellData.aspect;

            if (cellData.fuel_model <= 230)
            {
                _fuelModel = FuelModels.Forest;
                _fuelModelCode = new Forest();
                //set all the input needed
                //_input.SetForestInput();
            }            
            else if(true)
            {
                _fuelModel = FuelModels.Grassland;
                _fuelModelCode = new Grassland();
            }
            else if (true)
            {
                _fuelModel = FuelModels.Savannah;
                _fuelModelCode = new Savanna();
            }
            else if (true)
            {
                _fuelModel = FuelModels.Spinifex;
                _fuelModelCode = new Spinifex();
            }
            else if (true)
            {
                _fuelModel = FuelModels.Heathland;
                _fuelModelCode = new Heathland();
            }
            else if (true)
            {
                _fuelModel = FuelModels.MalleeHeath;
                _fuelModelCode = new MalleeHeath();
            }
            else if (true)
            {
                _fuelModel = FuelModels.Buttongrass;
                _fuelModelCode = new Buttongrass();
            }
            else if (true)
            {
                _fuelModel = FuelModels.Pine;
                _fuelModelCode = new Pine();
            }
        }

        public override void CalculateSpreadRate(WeatherManager weather, TimeManager time)
        {
            //wind
            double windSpeed, windAzimuth;
            weather.GetWind(out windSpeed, out windAzimuth);
            windAzimuth += 180; //azimuth is direction wind travels, direction is where it travels from
            if (windAzimuth > 360)
            {
                windAzimuth -= 360;
            }
            windSpeed *= 3.6; //requires km/h, input is m/s

            //update transient data
            _input.UpdateTransientData(time.CurrentDateTime, weather.GetTemperature(), weather.GetRelativeHumidity(), windSpeed, windAzimuth, 0, weather.HoursSinceRain, 0, weather.KBDI);

            //calculate
            _output = _fuelModelCode.Calculate(_input);
            _output.ROS *= _meterPerHourToMeterPerSecond;
            _eccentricity = CalculateEccentricity(_output.LengthToWidth);
        }

        public override double GetMaxSpreadRate()
        {
            return _output.ROS;
        }

        public override double GetDirectionOfMaxSpread()
        {
            return _output.Direction;
        }

        public override double GetSpreadRateInDirection(double directionOfInterest)
        {
            double rosDirection = _output.ROS;
            if (rosDirection != 0.0) // if forward spread rate is not zero
            {
                // Calculate beta: the angle between the direction of max spread and the direction of interest
                double beta = Mathd.Abs(_output.Direction - directionOfInterest);

                // Calculate the fire spread rate in this azimuth
                // if it deviates more than a tenth degree from the maximum azimuth
                if (beta > 180.0)
                {
                    beta = (360.0 - beta);
                }
                if (Mathd.Abs(beta) > 0.1)
                {
                    double radians = beta * Mathd.Deg2Rad;
                    rosDirection = rosDirection * (1.0 - _eccentricity) / (1.0 - _eccentricity * Mathd.Cos(radians));
                }
            }

            return rosDirection;
        }

        public override bool HasFuelLoad()
        {
            return _hasFuelLoad;
        }

        public override double GetFireIntensity()
        {
            return _output.Intensity;
        }

        //Van Wagner 1977, taken from Canadian FBP system
        public static double SlopeFactorFBP(double percentSlope)
        {
            return Mathd.Min(10, Mathd.Exp(3.533 * Mathd.Pow(percentSlope * 0.01, 1.2)));
        }

        //idea from https://research.csiro.au/spark/resources/model-library/slope-effects/
        public static double SlopeFactorCSIRO(double percentSlope, double slopeAzimuth, double windAzimuth)
        {
            slopeAzimuth -= 180.0; //upslope
            double slopeRad = slopeAzimuth * Mathd.Deg2Rad;
            double windRad = windAzimuth * Mathd.Deg2Rad;
            Vector2d upSlopeVector = new Vector2d(Mathd.Sin(slopeRad), Mathd.Cos(slopeRad));
            Vector2d windVector = new Vector2d(Mathd.Sin(windRad), Mathd.Cos(windRad));
            double alignment = Vector2d.Dot(upSlopeVector, windVector);
            double degreeSlope = Mathd.Atan(percentSlope * 0.01) * Mathd.Rad2Deg;
            double alignedDegreeSlope = degreeSlope * alignment;

            // Capping slopes at 20 degrees for largest speed increases or decreases in this example
            alignedDegreeSlope = Mathd.Clamp(alignedDegreeSlope, -20, 20);
            // Using McArthur's rule of thumb to double the speed of the fire for every 10 degrees up-slope.
            double slopeFactor = Mathd.Pow(2.0, 0.1 * Mathd.Abs(alignedDegreeSlope));

            // The CSIRO Kataburn model is implemented here for negative slopes (fire spreading down hill)
            if (alignedDegreeSlope < 0)
            {
                slopeFactor = slopeFactor / (2 * slopeFactor - 1.0);
            }                

            return slopeFactor;
        }

        //Adapted from Behave
        public static void CalculateDirectionOfMaxSpread(double windAzimuth, double slopeAzimuth, double noWindNoSlopeROS, double windROS, double slopeROS, out double forwardROS, out double spreadDirection)
        {
            //Calculate directional components (direction is clockwise from upslope)
            double correctedWindDirection = windAzimuth + 180; //back to wind direction from wind azimuth
            if (correctedWindDirection >= 360)
            {
                correctedWindDirection -= 360;
            }
            correctedWindDirection -= slopeAzimuth; //wind direction relative to aspect

            double windDirRadians = correctedWindDirection * Mathd.Deg2Rad;

            // Calculate coordinate components
            double x = slopeROS + (windROS * Mathd.Cos(windDirRadians));
            double y = windROS * Mathd.Sin(windDirRadians);
            double rateVector = Mathd.Sqrt((x * x) + (y * y)); //this is equivalent to WSV

            // Apply wind and slope rate to spread rate
            forwardROS = noWindNoSlopeROS + rateVector;

            // Calculate azimuth
            double azimuth = Mathd.Atan2(y, x);
            // Recalculate azimuth to degrees
            azimuth *= Mathd.Rad2Deg;
            // If angle is negative, add 360 degrees
            if (azimuth < 0)
            {
                azimuth += 360.0;
            }

            // Convert azimuth to be relative to North
            double dirMaxSpreadRelativeToNorth = azimuth;
            dirMaxSpreadRelativeToNorth += slopeAzimuth + 180.0; // spread direction is now relative to north
            while (dirMaxSpreadRelativeToNorth >= 360.0)
            {
                dirMaxSpreadRelativeToNorth -= 360.0;
            }

            spreadDirection = dirMaxSpreadRelativeToNorth;
        }

        public static double CalculateEccentricity(double fireLengthToWidthRatio)
        {
            double eccentricity = 0.0;
            double x = (fireLengthToWidthRatio * fireLengthToWidthRatio) - 1.0;
            if (x > 0.0)
            {
                eccentricity = Mathd.Sqrt(x) / fireLengthToWidthRatio;
            }

            return eccentricity;
        }

        /// <summary>
        /// Listed for Spinifex, grassland, buttongrass, heathland (https://research.csiro.au/spark/resources/model-library/)
        /// </summary>
        /// <param name="U_10">wind speed, km/h</param>
        /// <returns></returns>
        public static double GrasslandLengthToWidth(double U_10)
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

            return LBR;
        }

        /// <summary>
        /// Listed for Eucalypt (dry/wet), Mallee heath (https://research.csiro.au/spark/resources/model-library/)
        /// </summary>
        /// <param name="U_10">wind speed, km/h</param>
        /// <returns></returns>
        public static double ForestLengthToWidth(double U_10)
        {
            double LBR = 1.0;
            if (U_10 < 5)
            {
                LBR = 1.0;
            }
            else if (U_10 < 25)
            {
                LBR = 0.9286 * Mathd.Exp(0.0505 * U_10);
            }
            else
            {
                LBR = 0.1143 * U_10 + 0.4143;
            }

            return LBR;
        }

        /*public static double CalculateSurfaceFireLengthToWidthRatio(double U_10)
        {
            double lengthToWidthRatio;

            //km/h to m/s
            U_10 *= _kmPerHourToMeterPerSecond;

            if (U_10 > 1.0e-07)
            {
                //coefficients from Farsite manual as they are in m/s
                lengthToWidthRatio = .936 * Mathd.Exp(0.2566 * U_10) + 0.461 * Mathd.Exp(-0.1548 * U_10) - .397;
                // maximum eccentricity
                if (lengthToWidthRatio > 8.0)
                {
                    lengthToWidthRatio = 8.0;
                }
            }
            else
            {
                lengthToWidthRatio = 1.0;
            }

            return lengthToWidthRatio;
        }

        public static double CalculateCrownFireLengthToWidthRatio(double U_10)
        {
            double lengthToWidthRatio;
            //Calculates the crown fire length-to-width ratio given the 20-ft wind speed (in mph)
            // (Rothermel 1991, Equation 10, p16)            
            if (U_10 > 1.0e-07)
            {
                U_10 *= 0.621371192;
                lengthToWidthRatio = 1.0 + 0.125 * U_10;
            }
            else
            {
                lengthToWidthRatio = 1.0;
            }

            return lengthToWidthRatio;
        }*/
    }
}
        