using PREACT.Math;
using System;
using PREACT.Weather;
using System.IO;

namespace PREACT
{    
    public class WeatherManager
    {
        private Simulation _simulation;
        private Wildfire.FireWeatherIndex _fwi;
        private bool _fwiNeedsUpdate = true;
        private Wildfire.FFMCHourly _ffmcHourly;
        private DateTime _lastDateTime;
        private WeatherData _weatherData;

        private float _weatherReferenceElevation;

        //need current values and upcoming values for interpolation, assuming hourly input
        /*private float _currentTemperature, _nextTemperature, _interpolatedTemperature;
        private float _currentRelativeHumidity, _nextRelativeHumidity, _interpolatedRelativeHumidity;
        private float _currentPrecipitation, _nextPrecipitation, _interpolatedPrecipitation;
        private float _currentWindSpeed, _nextWindSpeed, _interpolatedWindSpeed;
        private float _currentWindDirection, _nextWindDirection, _interpolatedWindDirection;
        private float _currentCloudCover, _nextCloudCover, _interpolatedCloudCover;*/

        private HourlyWeatherData _lastHourlyData, _nextHourlyData, _interpolatedHourlyData;


        //"temperature_2m", "relative_humidity_2m", "precipitation", "wind_speed_10m", "wind_direction_10m", "cloud_cover", "direct_radiation", "boundary_layer_height" 
        readonly OpenMeteo.HourlyOptionsParameter[] _parameters = { OpenMeteo.HourlyOptionsParameter.temperature_2m, OpenMeteo.HourlyOptionsParameter.relativehumidity_2m, OpenMeteo.HourlyOptionsParameter.precipitation,
            OpenMeteo.HourlyOptionsParameter.windspeed_10m, OpenMeteo.HourlyOptionsParameter.winddirection_10m, OpenMeteo.HourlyOptionsParameter.cloudcover, OpenMeteo.HourlyOptionsParameter.direct_radiation, OpenMeteo.HourlyOptionsParameter.boundary_layer_height};

        public Wildfire.FireWeatherIndex FWI { get => _fwi; }
        public double FFMCHourly { get => _ffmcHourly.Value; }


        public WeatherManager(Simulation simulation)
        {
            _simulation = simulation;
            _fwi = new Wildfire.FireWeatherIndex();
            _ffmcHourly = new Wildfire.FFMCHourly();                       
        }

        public void Initialize(TimeManager timeManager)
        {
            InitializeWeather(timeManager);

            _weatherData.GetHourlyData(_simulation.Time.StartDateTime, out _lastHourlyData, out _nextHourlyData);
            _interpolatedHourlyData = _lastHourlyData;
        }

        private void InitializeWeather(TimeManager timeManager)
        {
            string filePath = Path.Combine(_simulation.Input.RootFolder, _simulation.Input.Weather.WeatherFile);
            bool success = false;
            bool haveCorrectWeather = false;
            bool fileExists = File.Exists(filePath);
            if (fileExists)
            {
                WeatherData wD = WeatherData.LoadFromFile(filePath, out success);
                Engine.Message(_simulation, Engine.LogType.Log, $"Weather data available from {wD.FirstEntry.ToString()} to {wD.LastEntry.ToString()}");
                if (success)
                {
                    if(DateTime.Compare(timeManager.StartDateTime, wD.FirstEntry) < 0 && DateTime.Compare(timeManager.EndDateTime, wD.LastEntry) > 0)
                    {
                        haveCorrectWeather = true;
                        _weatherData = wD;
                    }
                }
            }     

            //check for default named file that would be saved by the donwloader
            if(!fileExists)
            {
                filePath = Path.Combine(_simulation.Input.RootFolder, $"{_simulation.Input.Simulation.Name}_weather.csv");
                if(File.Exists(filePath))
                {
                    WeatherData wD = WeatherData.LoadFromFile(filePath, out success);
                    Engine.Message(_simulation, Engine.LogType.Log, $"Weather data available from {wD.FirstEntry.ToString()} to {wD.LastEntry.ToString()}");
                    if (success)
                    {
                        if (timeManager.StartDateTime > wD.FirstEntry && timeManager.EndDateTime < wD.LastEntry)
                        {
                            haveCorrectWeather = true;
                            _weatherData = wD;
                        }
                    }
                }                
            }

            if (!haveCorrectWeather)
            {
                DownloadWeather();
            }                             
        }

        private void DownloadWeather()
        {
            //if we do not have weather file/file is not complete for period we try to stream it in.
            OpenMeteo.OpenMeteoClient client = new OpenMeteo.OpenMeteoClient(true);
            //set options to download
            OpenMeteo.WeatherForecastOptions options = new OpenMeteo.WeatherForecastOptions((float)_simulation.Spatial.SimulationCenterLatLon.x, (float)_simulation.Spatial.SimulationCenterLatLon.y);
            options.Windspeed_Unit = OpenMeteo.WindspeedUnitType.ms;
            //canadian FBP needs all year data for FWI/BUI/FFMC etc
            options.Start_date = new string($"{_simulation.Time.StartDateTime.Year}-01-01");
            options.End_date = new string($"{_simulation.Time.EndDateTime.Year}-12-31");
            options.Hourly.Add(_parameters);

            //do query
            OpenMeteo.WeatherForecast? weatherStream = client.Query(options);
            if (weatherStream != null)
            {
                _weatherReferenceElevation = weatherStream.Elevation;

                string filePath = Path.Combine(_simulation.Input.RootFolder, $"{_simulation.Input.Simulation.Name}_weather.csv");
                using (StreamWriter file = new StreamWriter(filePath))
                {
                    file.WriteLine($"Latitide,{weatherStream.Latitude}");
                    file.WriteLine($"Longitude,{weatherStream.Longitude}");
                    file.WriteLine($"Elevation,{weatherStream.Elevation}");                    

                    if (weatherStream.Hourly != null)
                    {
                        OpenMeteo.Hourly hour = weatherStream.Hourly;
                        if (hour.Time != null)
                        {
                            //header
                            file.WriteLine($"{nameof(hour.Time)},{nameof(hour.Temperature_2m)} [{weatherStream.HourlyUnits.Temperature_2m}],{nameof(hour.Relativehumidity_2m)} [{weatherStream.HourlyUnits.Relativehumidity_2m}],{nameof(hour.Precipitation)} [{weatherStream.HourlyUnits.Precipitation}]," +
                                $"{nameof(hour.Windspeed_10m)} [{weatherStream.HourlyUnits.Windspeed_10m}],{nameof(hour.Winddirection_10m)} [{weatherStream.HourlyUnits.Winddirection_10m}],{nameof(hour.Cloudcover)} [{weatherStream.HourlyUnits.Cloudcover}]," +
                                $"{nameof(hour.Direct_radiation)} [{weatherStream.HourlyUnits.Direct_radiation}],{nameof(hour.Boundary_layer_height)} [{weatherStream.HourlyUnits.Boundary_layer_height}]");

                            //save actual data to usable format
                            HourlyWeatherData[] hourlyArray = new HourlyWeatherData[hour.Time.Length];
                            DateTime startDateTime, endDateTime;
                            DateTime.TryParse(hour.Time[0], out startDateTime);
                            DateTime.TryParse(hour.Time[hour.Time.Length - 1], out endDateTime);
                            _weatherData = new WeatherData(weatherStream.Latitude, weatherStream.Longitude, weatherStream.Elevation, startDateTime, endDateTime, hourlyArray);

                            //loop through all data
                            for (int i = 0; i < hour.Time.Length; i++)
                            {
                                file.WriteLine($"{hour.Time[i]},{hour.Temperature_2m[i]},{hour.Relativehumidity_2m[i]},{hour.Precipitation[i]},{hour.Windspeed_10m[i]},{hour.Winddirection_10m[i]},{hour.Cloudcover[i]},{hour.Direct_radiation[i]},{hour.Boundary_layer_height[i]}");

                                _weatherData.HourlyData[i] = new HourlyWeatherData(hour.Temperature_2m[i] ?? 0, hour.Relativehumidity_2m[i] ?? 0,hour.Precipitation[i] ?? 0, hour.Windspeed_10m[i] ?? 0, hour.Winddirection_10m[i] ?? 0, hour.Cloudcover[i] ?? 0, hour.Direct_radiation[i] ?? 0, hour.Boundary_layer_height[i] ?? 0);
                            }
                        }
                    }
                }
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
                //read new values from weather input stream
                _weatherData.GetHourlyData(currentDateTime, out _lastHourlyData, out _nextHourlyData);
                _interpolatedHourlyData = _lastHourlyData;

                //now update hourly values
                _ffmcHourly.Calculate(_lastHourlyData._temp, _lastHourlyData._rh, _lastHourlyData._windSpeed, _lastHourlyData._precip);
            }
            else
            {
                //update all relevant data
                float timeFraction = (currentDateTime.Minute * 60 + currentDateTime.Second) / 3600.0f;
                HourlyWeatherData.InterpolateData(_lastHourlyData, _nextHourlyData, timeFraction, ref _interpolatedHourlyData);
            }

            if (newDay)
            {
                _fwiNeedsUpdate = true;
            }

            if (_fwiNeedsUpdate &&  currentDateTime.Hour == 12)
            {
                _fwiNeedsUpdate = false;
                _fwi.CalculateDay(currentDateTime, _lastHourlyData._temp, _lastHourlyData._rh, _lastHourlyData._windSpeed, _lastHourlyData._precip);
            }    

                      

            //lastly just update 
            _lastDateTime = currentDateTime;
        }

        public float GetTemperature()
        {
            return _interpolatedHourlyData._temp;
        }

        public float GetTemperature(Vector3d simulationPosition, float elevation)
        {           
            return _interpolatedHourlyData._temp + 0.0065f * (_weatherReferenceElevation - elevation); //6.5 deg C / 1000 meters lapse rate https://en.wikipedia.org/wiki/Lapse_rate
        }

        public float GetRelativeHumidity()
        {
            return _interpolatedHourlyData._rh;
        }

        public float GetRelativeHumidity(Vector3d simulationPosition)
        {
            return _interpolatedHourlyData._rh;
        }

        public float GetPrecipitation()
        {
            return _interpolatedHourlyData._precip;
        }

        public float GetPrecipitation(Vector3d simulationPosition)
        {
            return _interpolatedHourlyData._precip;
        }

        public float WindSpeed { get => _interpolatedHourlyData._windSpeed; }
        public float WindDirection { get => _interpolatedHourlyData._windDirection; }

        public void GetWind(out double speed, out double direction)
        {
            speed = _interpolatedHourlyData._windSpeed;
            direction = _interpolatedHourlyData._windDirection;
        }

        public void GetWind(Vector2d simulationPosition, out double speed, out double direction)
        {
            speed = _interpolatedHourlyData._windSpeed;
            direction = _interpolatedHourlyData._windDirection;
        }

        public void GetWind(Vector3d simulationPosition, out double speed, out double direction)
        {
            speed = _interpolatedHourlyData._windSpeed;
            direction = _interpolatedHourlyData._windDirection;
        }

        public float GetCloudcover(Vector3d simulationPosition)
        {
            return _interpolatedHourlyData._cloudCover;
        }
    }
}
