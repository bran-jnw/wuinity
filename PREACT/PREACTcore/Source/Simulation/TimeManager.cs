using System;
using System.Collections.Generic;
using System.Text;

namespace PREACT
{    
    public  class TimeManager
    {
        DateTime _startDateTime;

        public TimeManager(IO.PREACTInput input)
        {
            //find the lowest DateTime and us ethat as starting time
            _startDateTime = DateTime.Now;
        }

        public double GetRelativeSimulationTime(DateTime dateTime)
        {
            TimeSpan delta = dateTime - _startDateTime;
            return delta.TotalSeconds;
        }
    }
}
