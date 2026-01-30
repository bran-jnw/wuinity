using System;
using System.Collections.Generic;
using System.Text;

namespace PREACT
{    
    public  class TimeManager
    {
        DateTime _startDateTime;
        DateTime _currentDateTime;
        float _simulationTime;

        public float SimulationTime { get => _simulationTime; }
        public DateTime CurrentDateTime { get => _currentDateTime; }

        public TimeManager(IO.PREACTInput input)
        {
            //TODO: find the lowest DateTime and use that as starting time
            _simulationTime = 0;
            _startDateTime = DateTime.Now;
            _currentDateTime = _startDateTime;
        }

        public void Step(float deltaTime)
        {
            _simulationTime += deltaTime;
            _currentDateTime = _currentDateTime.AddSeconds(deltaTime);
        }

        /// <summary>
        /// Return the relative simulation time in seconds between start DateTime and requested DateTime.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public double GetRelativeSimulationTime(DateTime dateTime)
        {
            TimeSpan delta = dateTime - _startDateTime;
            return delta.TotalSeconds;
        }
    }
}
