using System;
using PREACT.Math;

namespace PREACT.Wildfire.AFDRS
{
    public class SpreadModelAFDRS : SpreadModel
    {
        private CanadianFBPInputs _inputs;
        private MainOutputs _outputs;
        private SecondaryOutputs _secondaryOutputs;
        private FireData _head, _flank, _back;
        private CanadianFBPFuel _fuel;
        private bool _hasFuelLoad;

        private static double _meterPerMinToMeterPerSecond = 1.0 / 60.0;

        double _forwardSpreadRate, _directionOfMaxSpread, _eccentricity;

        public SpreadModelAFDRS(LandscapeCellData cellData, CanadianFBPLookupTable lookupTable, SpatialManager spatialManager)
        {
            if (cellData.fuel_model < 1)
            {
                //Engine.Message(null, Engine.LogType.Debug, $"1: No fuel, caused by fuel_model {cellData.fuel_model}.");
                _hasFuelLoad = false;
                return;
            }

            CanadianFBPLookupEntry lookupEntry = lookupTable.GetLookupEntry(cellData.fuel_model, out bool success);
            if (!success || lookupEntry.fuel_type.StartsWith("Non"))
            {
                //Engine.Message(null, Engine.LogType.Debug, $"2: No fuel, caused by fuel_model {cellData.fuel_model}.");
                _hasFuelLoad = false;
                return;
            }

            _inputs = new CanadianFBPInputs();
            _outputs = new MainOutputs();
            _secondaryOutputs = new SecondaryOutputs();
            _head = new FireData();
            _flank = new FireData();
            _back = new FireData();
            _fuel = new CanadianFBPFuel(lookupEntry);

            if (_fuel.FuelType == CanadianFBP.FuelTypes.NonFuel) //this should never happen
            {
                Engine.Message(null, Engine.LogType.Debug, $"3: No fuel, caused by fuel_model {cellData.fuel_model}.");
                _hasFuelLoad = false;
                return;
            }
            else
            {
                _hasFuelLoad = true;
            }

            //landscape
            _inputs.Elevation = (int)(0.5 + cellData.elevation);
            int percentSlope = (int)(0.5 + Mathd.Tan(Mathd.Deg2Rad * cellData.slope) * 100);
            _inputs.PercentSlope = percentSlope;
            _inputs.SlopeAzimuth = (int)(0.5 + cellData.aspect);

            _inputs.Lat = spatialManager.SimulationCenterLatLon.x;
            _inputs.Lon = spatialManager.SimulationCenterLatLon.y;

            _inputs.Pattern = 0; //lin = 1, point = 1
            _inputs.Time = 20;
            _inputs.JulianDayMin = -1; //this means it is calculated/updated first calc             

            _inputs.PercentCuring = 80;//TODO, read input
            _inputs.GrassFuelLoad = 0.35; //kg/m2
        }

        public override void CalculateSpreadRate(WeatherManager weather, TimeManager time)
        {
            //wind
            double windSpeed, windDirection;
            weather.GetWind(out windSpeed, out windDirection);
            _inputs.WindAzimuth = (int)windDirection + 180;
            if (_inputs.WindAzimuth > 360)
            {
                _inputs.WindAzimuth -= 360;
            }
            _inputs.WindSpeed = windSpeed * 3.6; //requires km/h, input is m/s

            //moisture related
            _inputs.FFMC = weather.FFMCHourly;
            _inputs.BUI = weather.FWI.BUI;

            _inputs.JulianDay = time.CurrentDateTime.DayOfYear;

            CanadianFBP.Calculate(_inputs, _fuel, _outputs, _secondaryOutputs, _head, _flank, _back);
            _inputs.JulianDayMin = _outputs.JulianDayMin; //so that we do not have to calculate it every time
        }

        public override double GetMaxSpreadRate()
        {
            return _head.RateOfSpread * _meterPerMinToMeterPerSecond;
        }

        public override double GetDirectionOfMaxSpread()
        {
            return _outputs.SpreadAzimuth;
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

        public override double GetFireIntensity()
        {
            return _outputs.SurfaceFireIntensity;
        }        
    }
}
