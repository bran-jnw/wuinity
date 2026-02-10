using System;
using PREACT.Math;

namespace PREACT.Wildfire
{
    public class SpreadModelCFBP : SpreadModel
    {
        private CanadianFBPInputs _inputs;
        private MainOutputs _outputs;
        private SecondaryOutputs _secondaryOutputs;
        private FireData _head, _flank, _back;
        private CanadianFBPFuel _fuel;
        private bool _hasFuelLoad;

        private static double _meterPerMinToMeterPerSecond = 1.0 / 60.0;

        public SpreadModelCFBP(LandscapeCellData cellData, CanadianFBPLookupTable lookupTable, SpatialManager spatialManager)
        {
            if(cellData.fuel_model < 1)
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

            if(_fuel.FuelType == CanadianFBP.FuelTypes.NonFuel) //this should never happen
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

        //for testing outside of PREACT       
        public SpreadModelCFBP(CanadianFBPFuel fuel, int elevation, int percentSlope, int slopeAzimuth, double lat, double lon, int pattern = 0, int time = 0, int percentCuring = 80, double grassFuelLoad = 0.35)
        {
            _inputs = new CanadianFBPInputs();
            _outputs = new MainOutputs();
            _secondaryOutputs = new SecondaryOutputs();
            _head = new FireData();
            _flank = new FireData();
            _back = new FireData();
            _fuel = fuel;

            //landscape
            _inputs.Elevation = elevation;
            _inputs.PercentSlope = percentSlope;
            _inputs.SlopeAzimuth = slopeAzimuth;

            _inputs.Lat = lat;
            _inputs.Lon = lon;

            _inputs.Pattern = pattern; //lin = 1, point = 1
            _inputs.Time = 20;
            _inputs.JulianDayMin = -1; //this means it is calculated/updated first calc             

            _inputs.PercentCuring = 80;//TODO, read input
            _inputs.GrassFuelLoad = 0.35; //kg/m2
        }

        public void CalculateSpreadRate(double windSpeed, int windDirection, double FFMC, double BUI, int julianDay)
        {
            _inputs.WindAzimuth = windDirection + 180;
            if(_inputs.WindAzimuth > 360)
            {
                _inputs.WindAzimuth -= 360;
            }
            _inputs.WindSpeed = windSpeed * 3.6; //requires km/h, input is m/s

            //moisture related
            _inputs.FFMC = FFMC;
            _inputs.BUI = BUI;

            _inputs.JulianDay = julianDay;

            CanadianFBP.Calculate(_inputs, _fuel, _outputs, _secondaryOutputs, _head, _flank, _back);
            _inputs.JulianDayMin = _outputs.JulianDayMin; //so that we do not have to calculate it every time
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

        public double GetHeadISI()
        {
            return _outputs.ISI;
        }

        public override double GetMaxSpreadRate()
        {
            return _head.RateOfSpread * _meterPerMinToMeterPerSecond;
        }

        public double GetFlankSpreadRate()
        {
            return _flank.RateOfSpread * _meterPerMinToMeterPerSecond;
        }

        public double GetBackSpreadRate()
        {
            return _back.RateOfSpread * _meterPerMinToMeterPerSecond;
        }

        public double GetLengthToBreadth()
        {
            return _secondaryOutputs.LengthToBreadth;
        }

        public override double GetDirectionOfMaxSpread()
        {
            return _outputs.SpreadAzimuth;
        }

        public override double GetSpreadRateInDirection(double directionOfInterest)
        {      
            double theta = Mathd.Abs(_outputs.SpreadAzimuth - directionOfInterest);
            if (theta == 90.0)
            {
                theta += 0.001;
            }
            theta *= Mathd.Deg2Rad;

            double ROS = _head.RateOfSpread;
            double BROS = _back.RateOfSpread;
            double FROS = _flank.RateOfSpread;   

            double cosTheta = Mathd.Cos(theta);
            double sinTheta = Mathd.Sin(theta);

            double p1 = (ROS - BROS) / (2 * cosTheta);
            double p2 = (ROS + BROS) / (2 * cosTheta);
            double nom1 = FROS * cosTheta * Mathd.Sqrt(FROS * FROS * cosTheta * cosTheta + (ROS * BROS) * sinTheta * sinTheta);
            double nom2 = (ROS * ROS - BROS * BROS) * 0.25 * sinTheta * sinTheta;
            double denom = FROS * FROS * cosTheta * cosTheta + ((ROS + BROS) * 0.5) * ((ROS + BROS) * 0.5) * sinTheta * sinTheta;
            double ROStheta = p1 + p2 * ((nom1 - nom2) / denom); 

            return ROStheta * _meterPerMinToMeterPerSecond;
        }

        public override bool HasFuelLoad()
        {
            return _hasFuelLoad;
        }

        public override double GetFirelineIntensity()
        {
            return _outputs.SurfaceFireIntensity;
        }
    }
}
