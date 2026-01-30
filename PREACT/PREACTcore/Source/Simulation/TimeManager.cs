using System;
using System.Collections.Generic;
using System.Text;

namespace PREACT
{    
    public  class TimeManager
    {
        DateTime _startDateTime;
        DateTime _endDateTime;
        DateTime _currentDateTime;
        float _simulationTime;
        string _startDateISO8601;
        string _endDateISO8601;

        public float SimulationTime { get => _simulationTime; }
        public DateTime StartDateTime { get => _startDateTime; }
        public DateTime CurrentDateTime { get => _currentDateTime; }
        public string StartDateISO8601 { get => _startDateISO8601; }
        public string EndDateISO8601 { get => _endDateISO8601; }

        public TimeManager(IO.PREACTInput input)
        {
            //TODO: find the lowest DateTime and use that as starting time
            _simulationTime = 0;
            _startDateTime = DateTime.Now;
            _currentDateTime = _startDateTime;

            _startDateISO8601 = new string($"{_startDateTime.Year}-{_startDateTime.Month}-{_startDateTime.Day}");
            _endDateISO8601 = new string($"{_endDateTime.Year}-{_endDateTime.Month}-{_endDateTime.Day}");
        }

        public void Step(float deltaTime)
        {
            _simulationTime += deltaTime;
            _currentDateTime = _currentDateTime.AddSeconds(deltaTime);
        }

        /// <summary>
        /// Return the simulation time in seconds between start DateTime and requested DateTime.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public double GetSimulationTime(DateTime dateTime)
        {
            TimeSpan delta = dateTime - _startDateTime;
            return delta.TotalSeconds;
        }
    }
}
