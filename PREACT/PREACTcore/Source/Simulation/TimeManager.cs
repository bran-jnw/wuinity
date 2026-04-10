using GeoTimeZone;
using System;

namespace PREACT
{    
    public  class TimeManager
    {
        DateTime _startDateTime;
        DateTime _endDateTime;
        DateTime _currentDateTime;
        DateTime _currentUTCDateTime;
        float _simulationTime;
        float _simulationEndTime;
        //string _startDateISO8601;
        //string _endDateISO8601;

        public float SimulationTime { get => _simulationTime; }
        public float SimulationEndTime { get => _simulationEndTime; }
        public DateTime StartDateTime { get => _startDateTime; }
        public DateTime EndDateTime { get => _endDateTime; }
        public DateTime CurrentDateTime { get => _currentDateTime; }
        public DateTime CurrentUTCDateTime { get => _currentUTCDateTime; }
        //public string StartDateISO8601 { get => _startDateISO8601; }
        //public string EndDateISO8601 { get => _endDateISO8601; }

        public TimeManager(Input.PREACTInput input, Simulation simulation)
        {
            _simulationTime = 0;

            TimeZoneResult iana = TimeZoneLookup.GetTimeZone(simulation.Input.Simulation.LowerLeftLatLon.x, simulation.Input.Simulation.LowerLeftLatLon.y);
            string windows = TimeZoneConverter.TZConvert.IanaToWindows(iana.Result);
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(windows);
            DateTimeOffset dto = new DateTimeOffset(input.Simulation.StartDateTime, tz.GetUtcOffset(input.Simulation.StartDateTime));

            _startDateTime = dto.DateTime;
            _currentDateTime = _startDateTime;
            _currentUTCDateTime = dto.UtcDateTime;
            _endDateTime = input.Simulation.EndDateTime.ToLocalTime();
            _simulationEndTime = (float)(_endDateTime - _startDateTime).TotalSeconds;

            Engine.Message(simulation, Engine.LogType.Debug, $"Simulation will run between {_startDateTime.ToString()} and {_endDateTime.ToString()} for a total of {_simulationEndTime} seconds (unless user has specified to exit early once evacuated.)");

            //_startDateISO8601 = new string($"{_startDateTime.Year}-{_startDateTime.Month}-{_startDateTime.Day}");
            //_endDateISO8601 = new string($"{_endDateTime.Year}-{_endDateTime.Month}-{_endDateTime.Day}");
        }

        public void Step(float deltaTime)
        {
            _simulationTime += deltaTime;
            _currentDateTime = _currentDateTime.AddSeconds(deltaTime);
            _currentUTCDateTime = _currentUTCDateTime.AddSeconds(deltaTime);
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
