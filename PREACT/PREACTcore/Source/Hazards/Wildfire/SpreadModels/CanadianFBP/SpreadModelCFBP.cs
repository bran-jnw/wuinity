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

        public override void CalculateSpreadRate(WeatherManager weather, TimeManager time)
        {
            //wind
            double windSpeed, windDirection;
            weather.GetWind(out windSpeed, out windDirection);            
            _inputs.WindAzimuth = (int)windDirection;
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
            double theta = Mathd.Abs(_outputs.SpreadAzimuth - directionOfInterest) * Mathd.Deg2Rad;
            double ROS = _head.RateOfSpread;
            double BROS = _back.RateOfSpread;
            double FROS = _flank.RateOfSpread;            

            double c1 = Mathd.Cos(theta);
            if (c1 == 0.0)
            {
                c1 = Mathd.Cos(theta + .001);
            }
            double s1 = Mathd.Sin(theta);
            
            double ROStheta = (((ROS - BROS) / (2 * c1) + (ROS + BROS) / (2 * c1)) * ((FROS * c1 * Mathd.Sqrt(FROS * FROS * c1 * c1 + (ROS * BROS) * s1 * s1) - ((ROS * ROS - BROS * BROS) / 4) * s1 * s1) / (FROS * FROS * c1 * c1 + ((ROS + BROS) / 2) * ((ROS + BROS) / 2) * s1 * s1)));

            return ROStheta * _meterPerMinToMeterPerSecond;
        }

        public override bool HasFuelLoad()
        {
            return _hasFuelLoad;
        }

        public override double GetFirelineIntensity()
        {
            return 1000;// _head.FireIntensity * 1000;
        }
    }
}
