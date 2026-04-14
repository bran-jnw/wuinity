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
        private Wildfire.HourlyFFMC _ffmcHourly;
        private DateTime _lastDateTime;
        private WeatherStream _weatherData;

        //values that are needed for something else
        int _hoursSinceRain;
        double _totalRainToday, _totalRainYesterday, _totalRain2DaysAgo, _totalRainSimulation;
        double _maxTemperatureToday, _maxTemperatureYesterday;
        DailyKBDI _DailyKBDI;

        private float _weatherReferenceElevation;

        private const float _inverseSecondsPerHour = 1.0f / 3600.0f;

        //need current values and upcoming values for interpolation, assuming hourly input
        /*private float _currentTemperature, _nextTemperature, _interpolatedTemperature;
        private float _currentRelativeHumidity, _nextRelativeHumidity, _interpolatedRelativeHumidity;
        private float _currentPrecipitation, _nextPrecipitation, _interpolatedPrecipitation;
        private float _currentWindSpeed, _nextWindSpeed, _interpolatedWindSpeed;
        private float _currentWindDirection, _nextWindDirection, _interpolatedWindDirection;
        private float _currentCloudCover, _nextCloudCover, _interpolatedCloudCover;*/

        private HourlyWeather _currentHourlyData, _nextHourlyData, _interpolatedHourlyData;


        //"temperature_2m", "relative_humidity_2m", "precipitation", "wind_speed_10m", "wind_direction_10m", "cloud_cover", "direct_radiation", "boundary_layer_height" 
        readonly OpenMeteo.HourlyOptionsParameter[] _parameters = { OpenMeteo.HourlyOptionsParameter.temperature_2m, OpenMeteo.HourlyOptionsParameter.relativehumidity_2m, OpenMeteo.HourlyOptionsParameter.precipitation,
            OpenMeteo.HourlyOptionsParameter.windspeed_10m, OpenMeteo.HourlyOptionsParameter.winddirection_10m, OpenMeteo.HourlyOptionsParameter.cloudcover, OpenMeteo.HourlyOptionsParameter.direct_radiation, OpenMeteo.HourlyOptionsParameter.boundary_layer_height};

        public Wildfire.FireWeatherIndex FWI { get => _fwi; }
        public double FFMCHourly { get => _ffmcHourly.Value; }
        public int HoursSinceRain { get => _hoursSinceRain; }
        public double KBDI { get => _DailyKBDI.KBDI; }


        public WeatherManager(Simulation simulation, TimeManager time)
        {
            _simulation = simulation;

            //todo: move fire specific stuff to where it is actually used?
            _fwi = new Wildfire.FireWeatherIndex(simulation.Input.WildfireModule.FireCellInput.StartFFMC, simulation.Input.WildfireModule.FireCellInput.StartDMC, simulation.Input.WildfireModule.FireCellInput.StartDC);
            _ffmcHourly = new Wildfire.HourlyFFMC(simulation.Input.WildfireModule.FireCellInput.StartHourlyFFMC);
            _DailyKBDI = new DailyKBDI(simulation.Input.WildfireModule.FireCellInput.StartKBDI, 1500); //TODO: user input

            LoadOrDownloadWeather(time);
            Update(time.StartDateTime, true);
        }

        private void Initialize(TimeManager timeManager)
        {
            //calculate FWI up until point of simulation start
            /*if(_weatherData.FirstEntry.Month == 1 && _weatherData.FirstEntry.Day == 1 && _weatherData.FirstEntry.Hour == 0)
            {
                _fwi.Reset();
                int days = (_simulation.Time.StartDateTime - _weatherData.FirstEntry).Days;
                if(_simulation.Time.StartDateTime.Hour < 12)
                {
                    days--; 
                }

                DateTime start = _weatherData.FirstEntry.AddHours(12); 
                for (int i = 0; i < days; ++i)
                {
                    HourlyWeatherData noonData = _weatherData.HourlyData[12 + 24 * i];
                    _fwi.CalculateDay(start, noonData._temp, noonData._rh, noonData._windSpeed, noonData._precip);
                    start.AddHours(24);
                }
            }*/
        }

        Wildfire.DeadFuelMoistureEngine _deadFuelMoistureEngine;
        public void DeadFuelMoistureRun(DateTime start, DateTime end)
        {
            if(_deadFuelMoistureEngine == null)
            {
                //_deadFuelMoistureEngine = new Wildfire.DeadFuelMoistureEngine()
            }
        }

        public void Update(DateTime currentDateTime, bool forceUpdate = false)
        {
            bool newMinute = _lastDateTime.Minute != currentDateTime.Minute;
            bool newHour = _lastDateTime.Hour != currentDateTime.Hour;
            bool newDay = _lastDateTime.DayOfYear != currentDateTime.DayOfYear;
            if(forceUpdate)
            {
                newMinute = true;
                newHour = true;
                newDay = true;
            }

            if (newDay)
            {
                _fwiNeedsUpdate = true;

                _maxTemperatureYesterday = _maxTemperatureToday;
                _maxTemperatureToday = -300.0;

                _totalRain2DaysAgo = _totalRainYesterday;
                _totalRainYesterday = _totalRainToday;
                _totalRainToday = 0;

                _DailyKBDI.CalculateDailyKBDI_Metric(_maxTemperatureYesterday, _totalRainYesterday);
            }

            if (newHour)
            {
                //read new values from weather input stream
                _weatherData.GetHourlyData(currentDateTime, out _currentHourlyData, out _nextHourlyData);
                _interpolatedHourlyData = _currentHourlyData;                

                //now update hourly values
                _ffmcHourly.Calculate(_currentHourlyData._temp, _currentHourlyData._rh, _currentHourlyData._windSpeed * 3.6, _currentHourlyData._precip);                          
                
                if(_currentHourlyData._temp > _maxTemperatureToday)
                {
                    _maxTemperatureToday = _currentHourlyData._temp;
                }

                _hoursSinceRain = _currentHourlyData._precip > 0 ? 0 : _hoursSinceRain + 1;
                _totalRainToday += _currentHourlyData._precip;
                _totalRainSimulation += _currentHourlyData._precip;
            }
            else
            {
                //update all relevant data
                float timeFraction = (currentDateTime.Minute * 60 + currentDateTime.Second) * _inverseSecondsPerHour;
                HourlyWeather.InterpolateData(_currentHourlyData, _nextHourlyData, timeFraction, ref _interpolatedHourlyData);
            }

            if (newMinute)
            {

            }

            if (_fwiNeedsUpdate && currentDateTime.Hour == 12)
            {
                _fwiNeedsUpdate = false;
                _fwi.CalculateDay(currentDateTime, _currentHourlyData._temp, _currentHourlyData._rh, _currentHourlyData._windSpeed * 3.6, _currentHourlyData._precip);
            }

            //lastly just update DateTime
            _lastDateTime = currentDateTime;
        }

        private void LoadOrDownloadWeather(TimeManager timeManager)
        {
            string filePath = Path.Combine(_simulation.Input.RootFolder, _simulation.Input.Weather.WeatherFile);
            bool success = false;
            bool haveCorrectWeather = false;
            bool fileExists = File.Exists(filePath);
            if (fileExists)
            {
                WeatherStream wD = WeatherStream.LoadFromFile(filePath, out success);
                Engine.Message(_simulation, Engine.LogType.Log, $"Weather data available from {wD.FirstEntry.ToString()} to {wD.LastEntry.ToString()}");
                if (success)
                {
                    if(DateTime.Compare(timeManager.StartDateTime, wD.FirstEntry) >= 0 && DateTime.Compare(timeManager.EndDateTime, wD.LastEntry) <= 0)
                    {
                        haveCorrectWeather = true;
                        _weatherData = wD;
                    }
                }
                else
                {
                    Engine.Message(_simulation, Engine.LogType.Log, $"Supplied weather data is outside of needed range.");
                }
            }     

            //check for default named file that would be saved by the downloader
            if(!fileExists)
            {
                filePath = Path.Combine(_simulation.Input.RootFolder, $"{_simulation.Input.Simulation.Name}_weather.csv");
                if(File.Exists(filePath))
                {
                    WeatherStream wD = WeatherStream.LoadFromFile(filePath, out success);
                    Engine.Message(_simulation, Engine.LogType.Log, $"Weather data available from {wD.FirstEntry.ToString()} to {wD.LastEntry.ToString()}");
                    if (success)
                    {
                        if (DateTime.Compare(timeManager.StartDateTime, wD.FirstEntry) >= 0 && DateTime.Compare(timeManager.EndDateTime, wD.LastEntry) <= 0)
                        {
                            haveCorrectWeather = true;
                            _weatherData = wD;
                        }
                        else
                        {
                            Engine.Message(_simulation, Engine.LogType.Log, $"Supplied weather data is outside of needed range.");
                        }
                    }
                }                
            }

            if (!haveCorrectWeather)
            {
                Engine.Message(_simulation, Engine.LogType.Log, $"Weather data file was either not found or did not contain needed time range, downloading weather.");
                DownloadWeather();
            }                             
        }

        //TODO: remove
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
                        OpenMeteo.Hourly hourly = weatherStream.Hourly;
                        if (hourly.Time != null)
                        {
                            //header
                            file.WriteLine($"{nameof(hourly.Time)},{nameof(hourly.Temperature_2m)} [{weatherStream.HourlyUnits.Temperature_2m}],{nameof(hourly.Relativehumidity_2m)} [{weatherStream.HourlyUnits.Relativehumidity_2m}],{nameof(hourly.Precipitation)} [{weatherStream.HourlyUnits.Precipitation}]," +
                                $"{nameof(hourly.Windspeed_10m)} [{weatherStream.HourlyUnits.Windspeed_10m}],{nameof(hourly.Winddirection_10m)} [{weatherStream.HourlyUnits.Winddirection_10m}],{nameof(hourly.Cloudcover)} [{weatherStream.HourlyUnits.Cloudcover}]," +
                                $"{nameof(hourly.Direct_radiation)} [{weatherStream.HourlyUnits.Direct_radiation}],{nameof(hourly.Boundary_layer_height)} [{weatherStream.HourlyUnits.Boundary_layer_height}],FFMC hourly [-],FFMC [-],DMC [-],DC [-],ISI [-],BUI [-],FWI [-]");

                            //save actual data to usable format in memory
                            HourlyWeather[] hourlyArray = new HourlyWeather[hourly.Time.Length];
                            DateTime startDateTime, endDateTime;
                            DateTime.TryParse(hourly.Time[0], out startDateTime);
                            DateTime.TryParse(hourly.Time[hourly.Time.Length - 1], out endDateTime);
                            _weatherData = new WeatherStream(weatherStream.Latitude, weatherStream.Longitude, weatherStream.Elevation, startDateTime, endDateTime, hourlyArray);

                            Wildfire.FireWeatherIndex fwi = new Wildfire.FireWeatherIndex();
                            Wildfire.HourlyFFMC ffmcHourly = new Wildfire.HourlyFFMC();

                            //loop through all data
                            for (int i = 0; i < hourly.Time.Length; i++)
                            {
                                DateTime.TryParse(hourly.Time[i], out DateTime dateTime);
                                if(dateTime != null && dateTime.Hour == 12)
                                {
                                    fwi.CalculateDay(dateTime, hourly.Temperature_2m[i] ?? 0, hourly.Relativehumidity_2m[i] ?? 0, hourly.Windspeed_10m[i] ?? 0, hourly.Precipitation[i] ?? 0);
                                }
                                ffmcHourly.Calculate(hourly.Temperature_2m[i] ?? 0, hourly.Relativehumidity_2m[i] ?? 0, hourly.Windspeed_10m[i] ?? 0, hourly.Precipitation[i] ?? 0);

                                file.WriteLine($"{hourly.Time[i]},{hourly.Temperature_2m[i]},{hourly.Relativehumidity_2m[i]},{hourly.Precipitation[i]},{hourly.Windspeed_10m[i]},{hourly.Winddirection_10m[i]},{hourly.Cloudcover[i]},{hourly.Direct_radiation[i]},{hourly.Boundary_layer_height[i]},{ffmcHourly.Value},{fwi.FFMC},{fwi.DMC},{fwi.DC},{fwi.ISI},{fwi.BUI},{fwi.FWI}");

                                _weatherData.HourlyData[i] = new HourlyWeather(hourly.Temperature_2m[i] ?? 0, hourly.Relativehumidity_2m[i] ?? 0,hourly.Precipitation[i] ?? 0, hourly.Windspeed_10m[i] ?? 0, hourly.Winddirection_10m[i] ?? 0, hourly.Cloudcover[i] ?? 0, hourly.Direct_radiation[i] ?? 0, hourly.Boundary_layer_height[i] ?? 0);
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

        public float GetHourlyPrecipitation()
        {
            return _currentHourlyData._precip;
        }

        public float GetHourlyPrecipitation(Vector3d simulationPosition)
        {
            return _currentHourlyData._precip;
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
