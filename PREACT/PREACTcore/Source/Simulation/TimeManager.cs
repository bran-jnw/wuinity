using System;
using System.Collections.Generic;
using System.Text;

namespace PREACT
{    
    public  class TimeManager
    {
        DateTime _startDateTime;
        DateTime _currentDateTime;
        float _currentTime;
        int _dayOfYear;

        public float CurrentTime { get => _currentTime; }
        public int DayOfYear { get => _dayOfYear; }

        public TimeManager(IO.PREACTInput input)
        {
            //TODO: find the lowest DateTime and use that as starting time
            _startDateTime = DateTime.Now;
            _currentDateTime = _startDateTime;
        }

        public void Step(float deltaTime)
        {
            _currentTime += deltaTime;
            _currentDateTime = _currentDateTime.AddSeconds(deltaTime);
            _dayOfYear = _currentDateTime.DayOfYear;
        }       

        public double GetRelativeSimulationTime(DateTime dateTime)
        {
            TimeSpan delta = dateTime - _startDateTime;
            return delta.TotalSeconds;
        }

        public int GetJulianDay(float currentTime)
        {
            return _dayOfYear;
        }
    }
}
