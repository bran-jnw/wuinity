using PREACT.Math;
using System;

namespace PREACT
{    
    public class WeatherManager
    {
        private IO.PREACTInput _input;
        private Wildfire.FireWeatherIndex _fwi;
        private bool _fwiNeedsUpdate = true;
        private Wildfire.FFMCHourly _ffmcHourly;
        private DateTime _lastDateTime;

        //need current values and upcoming values for interpolation, assumin ghourly input
        private float _currentTemperature, _nextTemperature, _interpolatedTemperature;
        private float _currentRelativeHumidity, _nextRelativeHumidity, _interpolatedRelativeHumidity;
        private float _currentPrecipitation, _nextPrecipitation, _interpolatedPrecipitation;
        private float _currentWindSpeed, _nextWindSpeed, _interpolatedWindSpeed;
        private float _currentWindDirection, _nextWindDirection, _interpolatedWindDirection;

        public Wildfire.FireWeatherIndex FWI { get => _fwi; }
        public double FFMCHourly { get => _ffmcHourly.Value; }


        public WeatherManager(IO.PREACTInput input) 
        {
            _input = input;
            _fwi = new Wildfire.FireWeatherIndex();
            _ffmcHourly = new Wildfire.FFMCHourly();
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
