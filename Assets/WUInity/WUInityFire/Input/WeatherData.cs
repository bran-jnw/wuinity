using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static WUInity.InterpolationLibrary;

namespace WUInity.Fire
{
    /// <summary>
    /// Precipitation is the daily rain amount specified in hundredths of an inch or millimeters (integer).
    /// Hour1 corresponds to the hour at which the minimum temperature was recorded(0-2400).
    /// Hour2 corresponds to the hour at which the maximum temperature was recorded(0-2400).
    /// Temperatures(Temp1 is minimum; Temp2 is maximum) are in degrees Fahrenheit or Celsius(integer).
    /// Humidities(Humid1 is maximum; Humid2 is minimum) are in percent, 0 to 99 (integer).
    /// Elevation is in feet or meters above sea level.NOTE: these units (feet or meters) do not have to be the same as the landscape elevation theme(integer).
    /// </summary>
    [System.Serializable]    
    public struct WeatherData
    {
        public int Month, Day, Precip, Hour1, Hour2, Temp1, Temp2, Humid1, Humid2, Elevation;


        public WeatherData(int Month, int Day, int Precip, int Hour1, int Hour2, int Temp1, int Temp2, int Humid1, int Humid2, int Elevation)
        {
            this.Month = Month;
            this.Day = Day;
            this.Precip = Precip;
            this.Hour1 = Hour1;
            this.Hour2 = Hour2;
            this.Temp1 = Temp1;
            this.Temp2 = Temp2;
            this.Humid1 = Humid1;
            this.Humid2 = Humid2;
            this.Elevation = Elevation;
        }

        public void UpdateData(int Month, int Day, int Precip, int Hour1, int Hour2, int Temp1, int Temp2, int Humid1, int Humid2, int Elevation)
        {
            this.Month = Month;
            this.Day = Day;
            this.Precip = Precip;
            this.Hour1 = Hour1;
            this.Hour2 = Hour2;
            this.Temp1 = Temp1;
            this.Temp2 = Temp2;
            this.Humid1 = Humid1;
            this.Humid2 = Humid2;
            this.Elevation = Elevation;
        }

        public void GetMinHourData(out int minHour, out int minTemp, out int minHumid)
        {
            minHour = Mathf.Max(Hour1, Hour2);
            if (minHour == Hour1)
            {
                minTemp = Temp1;
                minHumid = Humid1;
            }
            else
            {
                minTemp = Temp2;
                minHumid = Humid2;
            }
        }

        public void GetMaxHourData(out int maxHour, out int maxTemp, out int maxHumid)
        {
            maxHour = Mathf.Max(Hour1, Hour2);
            if (maxHour == Hour1)
            {
                maxTemp = Temp1;
                maxHumid = Humid1;
            }
            else
            {
                maxTemp = Temp2;
                maxHumid = Humid2;
            }
        }
    }

    [System.Serializable]
    public struct WeatherInput
    {
        [SerializeField] private WeatherData[] weatherInputs;

        public WeatherInput(WeatherData[] weatherInputs)
        {
            this.weatherInputs = weatherInputs;
        }

        //https://weatherspark.com/m/3550/7/Average-Weather-in-July-in-Roxborough-Park-Colorado-United-States#Sections-Humidity
        public static WeatherInput GetTemplate()
        {
            WeatherData[] data = new WeatherData[2];
            data[0] = new WeatherData(7, 15, 0, 4, 16, 13, 33, 10, 10, 1803);
            data[1] = new WeatherData(7, 16, 25, 4, 16, 10, 30, 18, 15, 1803);

            WeatherInput w = new WeatherInput(data);

            return w;
        }

        public WeatherPoint GetCurrentWeather(int currentMonth, int currentDay, int currentSecond)
        {
            WeatherPoint currentWeather = new WeatherPoint();

            float currentHour = 2400f * currentSecond / (3600f * 24);
            for (int i = 0; i < weatherInputs.Length; i++)
            {
                if(currentMonth == weatherInputs[i].Month && currentDay == weatherInputs[i].Day)
                {
                    currentWeather.Elevation = weatherInputs[i].Elevation;
                    currentWeather.Precipitation = weatherInputs[i].Precip;

                    int minHour = Mathf.Min(weatherInputs[i].Hour1, weatherInputs[i].Hour2);
                    int maxHour = Mathf.Max(weatherInputs[i].Hour1, weatherInputs[i].Hour2);
                    int temp1, temp2, humid1, humid2;
                    if(minHour == weatherInputs[i].Hour1)
                    {
                        temp1 = weatherInputs[i].Temp1;
                        temp2 = weatherInputs[i].Temp2;
                        humid1 = weatherInputs[i].Humid1;
                        humid2 = weatherInputs[i].Humid2;
                    }
                    else
                    {
                        temp1 = weatherInputs[i].Temp2;
                        temp2 = weatherInputs[i].Temp1;
                        humid1 = weatherInputs[i].Humid2;
                        humid2 = weatherInputs[i].Humid1;
                    }

                    if (currentHour >= minHour && currentHour <= maxHour)
                    {
                        float frac = (currentHour - minHour) / (maxHour - minHour);
                        currentWeather.Temperature = (int)CosineInterpolate(temp1, temp2, frac);
                        currentWeather.Humidity = (int)CosineInterpolate(humid1, humid2, frac);
                    }
                    else if(currentHour < minHour)
                    {
                        if(i == 0)
                        {
                            currentWeather.Temperature = temp1;
                            currentWeather.Humidity = humid1;
                        }
                        else
                        {
                            float frac = (currentHour - minHour) / (maxHour - minHour);
                            currentWeather.Temperature = (int)CosineInterpolate(temp1, temp2, frac);
                            currentWeather.Humidity = (int)CosineInterpolate(humid1, humid2, frac);
                        }
                    }
                    else if (currentHour > maxHour)
                    {
                        if (i == weatherInputs.Length - 1)
                        {
                            currentWeather.Temperature =temp2;
                            currentWeather.Humidity = humid2;
                        }
                        else
                        {

                        }
                    }
                }
            }

            return currentWeather;
        }
    }

    [System.Serializable]
    public struct WeatherPoint
    {
        public int Elevation, Temperature, Humidity, Precipitation;
    }
}