using System;
using System.Collections.Generic;
using System.Text;

namespace PREACT.Wildfire
{
    public class SpreadModelConstant : SpreadModel
    {
        double _spreadRate;

        public SpreadModelConstant(LandscapeCellData cellData)
        {
            //TODO: have database to read from
        }

        public override void CalculateSpreadRate(WeatherManager weather, TimeManager time)
        {
            //do nothing
        }

        public override double GetDirectionOfMaxSpread()
        {
            return 0;
        }

        public override double GetFirelineIntensity()
        {
            return 1000;
        }

        public override double GetMaxSpreadRate()
        {
            return _spreadRate;
        }

        public override double GetSpreadRateInDirection(double directionOfInterest)
        {
            return _spreadRate;
        }

        public override bool HasFuelLoad()
        {
            return true;
        }
    }
}
