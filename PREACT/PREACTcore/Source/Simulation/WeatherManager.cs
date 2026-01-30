using PREACT.Math;
using System;
using PREACT.Weather;

namespace PREACT
{    
    public class WeatherManager
    {
        private Simulation _simulation;
        private Wildfire.FireWeatherIndex _fwi;
        private bool _fwiNeedsUpdate = true;
        private Wildfire.FFMCHourly _ffmcHourly;
        private DateTime _lastDateTime;

        //need current values and upcoming values for interpolation, assuming hourly input
        private float _currentTemperature, _nextTemperature, _interpolatedTemperature;
        private float _currentRelativeHumidity, _nextRelativeHumidity, _interpolatedRelativeHumidity;
        private float _currentPrecipitation, _nextPrecipitation, _interpolatedPrecipitation;
        private float _currentWindSpeed, _nextWindSpeed, _interpolatedWindSpeed;
        private float _currentWindDirection, _nextWindDirection, _interpolatedWindDirection;
        private float _currentCloudCover, _nextCloudCover, _interpolatedCloudCover;

        //"temperature_2m", "precipitation", "relative_humidity_2m",  "wind_speed_10m", "wind_direction_10m", "cloud_cover", "direct_radiation", "boundary_layer_height" 
        readonly OpenMeteo.HourlyOptionsParameter[] _parameters = { OpenMeteo.HourlyOptionsParameter.temperature_2m, OpenMeteo.HourlyOptionsParameter.relativehumidity_2m, OpenMeteo.HourlyOptionsParameter.precipitation,
            OpenMeteo.HourlyOptionsParameter.windspeed_10m, OpenMeteo.HourlyOptionsParameter.winddirection_10m, OpenMeteo.HourlyOptionsParameter.cloudcover, OpenMeteo.HourlyOptionsParameter.direct_radiation};

        public Wildfire.FireWeatherIndex FWI { get => _fwi; }
        public double FFMCHourly { get => _ffmcHourly.Value; }


        public WeatherManager(Simulation simulation)
        {
            _simulation = simulation;
            _fwi = new Wildfire.FireWeatherIndex();
            _ffmcHourly = new Wildfire.FFMCHourly();                       
        }

        public void Initialize()
        {
            //TODO: check if needed, look for file first
            DownloadWeather();
        }

        private void DownloadWeather()
        {
            //if we do not have weather file/file is not complete for period we try to stream it in.
            OpenMeteo.OpenMeteoClient client = new OpenMeteo.OpenMeteoClient();

            //set options to download
            OpenMeteo.WeatherForecastOptions options = new OpenMeteo.WeatherForecastOptions((float)_simulation.Spatial.SimulationCenterLatLon.x, (float)_simulation.Spatial.SimulationCenterLatLon.y);
            options.Windspeed_Unit = OpenMeteo.WindspeedUnitType.ms;
            options.Start_date = _simulation.Time.StartDateISO8601;
            //canadian FBP needs all year data for FWI/BUI/FFMC etc
            if (_simulation.Input.WildfireModule.Enabled
                && _simulation.Input.WildfireModule.Module == IO.WildfireModuleInput.WildfireModules.CellParticleHybrid
                && _simulation.Input.WildfireModule.FireCellInput.SpreadRateModel == IO.FireCellInput.SpreadRateModels.CanadianFBP)
            {
                options.Start_date = new string($"{_simulation.Time.StartDateTime.Year}-01-01");
            }
            options.End_date = _simulation.Time.EndDateISO8601;
            options.Hourly.Add(_parameters);

            //do query
            OpenMeteo.WeatherForecast? weatherData = client.Query(options);
            if (weatherData != null)
            {
                //weatherData
            }
            else
            {
                Engine.Message(_simulation, Engine.LogType.SimulationError, "Could not download weather and no weather file has been supplied.");
            }
        }

        public void Step(float simulationTime, DateTime currentDateTime)
        {
            bool newMinute = _lastDateTime.Minute != currentDateTime.Minute;
            bool newHour = _lastDateTime.Hour != currentDateTime.Hour;
            bool newDay = _lastDateTime.DayOfYear != currentDateTime.DayOfYear;


            if (newMinute)
            {

            }

            if (newHour)
            {
                //TODO: read new values from weather input stream

                //now update hourly values
                _ffmcHourly.Calculate(_currentTemperature, _currentRelativeHumidity, _currentWindSpeed, _currentPrecipitation);
            }

            if (newDay)
            {
                _fwiNeedsUpdate = true;
            }

            if (_fwiNeedsUpdate &&  currentDateTime.Hour == 12)
            {
                _fwiNeedsUpdate = false;
                _fwi.CalculateDay(currentDateTime, _currentTemperature, _currentRelativeHumidity, _currentWindSpeed, _currentPrecipitation);
            }
                


            //TODO: update all relevant data
            float timeFraction = (currentDateTime.Minute * 60 + currentDateTime.Second) / 3600.0f;
            InterpolateData(timeFraction);
            

            //lastly just update 
            _lastDateTime = currentDateTime;
        }

        private void InterpolateData(float fraction)
        {
            _interpolatedTemperature = Interpolation.CosineInterpolate(_currentTemperature, _nextTemperature, fraction);
            _interpolatedRelativeHumidity = Interpolation.CosineInterpolate(_currentRelativeHumidity, _nextRelativeHumidity, fraction);
            _interpolatedPrecipitation = Interpolation.CosineInterpolate(_currentPrecipitation, _nextPrecipitation, fraction);
            _currentWindSpeed = Interpolation.CosineInterpolate(_currentWindSpeed, _nextWindSpeed, fraction);
            _currentWindDirection = Interpolation.CosineInterpolate(_currentWindDirection, _nextWindDirection, fraction);
        }

        public float GetTemperature(Vector3d simulationPosition)
        {
            return _interpolatedTemperature;
        }

        public float GetRelativeHumidity(Vector3d simulationPosition)
        {
            return _interpolatedRelativeHumidity;
        }

        public float GetPrecipitation(Vector3d simulationPosition)
        {
            return _interpolatedPrecipitation;
        }

        public void GetWind(out double speed, out double direction)
        {
            speed = _interpolatedWindSpeed;
            direction = _interpolatedWindDirection;
        }

        public void GetWind(Vector2d simulationPosition, out double speed, out double direction)
        {
            speed = _interpolatedWindSpeed;
            direction = _interpolatedWindDirection;
        }

        public void GetWind(Vector3d simulationPosition, out double speed, out double direction)
        {
            speed = _interpolatedWindSpeed;
            direction = _interpolatedWindDirection;
        }
    }
}
