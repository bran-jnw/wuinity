//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System;
using System.IO;
using PREACT.Math;

namespace PREACT.Weather
{
    public struct HourlyWeatherData
    {
        public float _temp, _rh, _precip, _windSpeed, _windDirection, _cloudCover, _directRadiation, _boundrayLayerHeight;

        //temperature_2m", "relative_humidity_2m", "precipitation", "wind_speed_10m", "wind_direction_10m", "cloud_cover", "direct_radiation", "boundary_layer_height" 
        public HourlyWeatherData(float temp, float rh, float precip, float windSpeed, float windDirection, float cloudCover, float directRadiation, float boundrayLayerHeight)   
        {
            _temp = temp;
            _rh = rh;
            _precip = precip;
            _windSpeed = windSpeed;
            _windDirection = windDirection;
            _cloudCover = cloudCover;
            _directRadiation = directRadiation;
            _boundrayLayerHeight = boundrayLayerHeight;
        }

        public static void InterpolateData(HourlyWeatherData hour1, HourlyWeatherData hour2, float fraction, ref HourlyWeatherData interpolatedHour)
        {
            interpolatedHour._temp = Interpolation.CosineInterpolate(hour1._temp, hour2._temp, fraction);
            interpolatedHour._rh = Interpolation.CosineInterpolate(hour1._rh, hour2._rh, fraction);
            interpolatedHour._precip = Interpolation.CosineInterpolate(hour1._precip, hour2._precip, fraction);
            interpolatedHour._windSpeed = Interpolation.CosineInterpolate(hour1._windSpeed, hour2._windSpeed, fraction);
            interpolatedHour._windDirection = Interpolation.CosineInterpolate(hour1._windDirection, hour2._windDirection, fraction);
            interpolatedHour._cloudCover = Interpolation.CosineInterpolate(hour1._cloudCover, hour2._cloudCover, fraction);
            interpolatedHour._boundrayLayerHeight = Interpolation.CosineInterpolate(hour1._boundrayLayerHeight, hour2._boundrayLayerHeight, fraction);
        }
    }

    public class WeatherData
    {
        private double _latitude, _longitude, _elevation;
        private HourlyWeatherData[] _hourlyData;
        private DateTime _dateTimeFirstEntry;
        private DateTime _dateTimeLastEntry;

        public HourlyWeatherData[] HourlyData { get => _hourlyData; }
        public DateTime FirstEntry { get => _dateTimeFirstEntry; }
        public DateTime LastEntry { get => _dateTimeLastEntry; }

        public WeatherData(double latitude, double longitude, double elevation, DateTime firstDateTime, DateTime lastDateTime, HourlyWeatherData[] hourlyData)
        {
            _latitude = latitude;
            _longitude = longitude;
            _elevation = elevation;
            _hourlyData = hourlyData;
            _dateTimeFirstEntry = firstDateTime;
            _dateTimeLastEntry = lastDateTime;
        }

        public void GetHourlyData(DateTime dateTime, out HourlyWeatherData current, out HourlyWeatherData next)
        {
            int index = (int)(dateTime - _dateTimeFirstEntry).TotalHours;
            current = _hourlyData[index];
            if(index < _hourlyData.Length - 2)
            {
                next = _hourlyData[index + 1];
            }
            else
            {
                next = current;
            }
        }

        public static WeatherData LoadFromFile(string filePath, out bool success)
        {
            success = false;
            WeatherData result = null;
            List<HourlyWeatherData> weatherData = new List<HourlyWeatherData>();

            bool fileExists = File.Exists(filePath);
            if (fileExists)
            {
                string[] dataLines = File.ReadAllLines(filePath);

                double latitude, longitude, elevation;
                DateTime first = DateTime.MinValue;
                DateTime last = DateTime.MaxValue;

                string[] data = dataLines[0].Split(',');
                double.TryParse(data[1], out latitude);
                data = dataLines[1].Split(',');
                double.TryParse(data[1], out longitude);
                data = dataLines[2].Split(',');
                double.TryParse(data[1], out elevation);

                for (int j = 4; j < dataLines.Length; j++)
                {
                    data = dataLines[j].Split(',');
                    if (data.Length >= 9)
                    {
                        if(j == 4)
                        {
                            DateTime.TryParse(data[0], out first);
                        }
                        else
                        {
                            DateTime.TryParse(data[0], out last);
                        }                            

                        float temp, rh, precip, windSpeed, windDirection, cloudCover, directRadiation, boundrayLayerHeight;

                        bool b1 = float.TryParse(data[1], out temp);
                        bool b2 = float.TryParse(data[2], out rh);
                        bool b3 = float.TryParse(data[3], out precip);
                        bool b4 = float.TryParse(data[4], out windSpeed);
                        bool b5 = float.TryParse(data[5], out windDirection);
                        bool b6 = float.TryParse(data[6], out cloudCover);
                        bool b7 = float.TryParse(data[7], out directRadiation);
                        bool b8 = float.TryParse(data[8], out boundrayLayerHeight);

                        HourlyWeatherData wD = new HourlyWeatherData(temp, rh, precip, windSpeed, windDirection, cloudCover, directRadiation, boundrayLayerHeight);
                        weatherData.Add(wD);
                    }                    
                }

                if (weatherData.Count > 0)
                {
                    result = new WeatherData(latitude, longitude, elevation, first, last, weatherData.ToArray());
                    success = true;
                    Engine.Message(null, Engine.LogType.Log, " Weather input file " + filePath + " was found, " + weatherData.Count + " valid data points were succesfully loaded.");
                }
                else if (fileExists)
                {
                    Engine.Message(null, Engine.LogType.Warning, "Weather input file " + filePath + " was found but did not contain any valid data, will not be able to do fire or smoke spread simulations.");
                }
            }
            else
            {
                Engine.Message(null, Engine.LogType.Warning, "Weather data file " + filePath + " not found, will not be able to do fire or smoke spread simulations.");
            }

            return result;
        }
    }    
}