using System;
using System.Collections.Generic;
using System.Text;

namespace PREACT.Wildfire
{
    public class SpreadModelLookUpTable : SpreadModel
    {
        double _spreadRate;
        bool _hasFuelLoad;

        public SpreadModelLookUpTable(LandscapeCellData cellData, SpreadRateLookUpTable lookUpTable)
        {
            _spreadRate = lookUpTable.GetSpreadRate(cellData.fuel_model);
            _hasFuelLoad = _spreadRate > 0;
        }

        public override void CalculateSpreadRate(WeatherManager weather, TimeManager time)
        {
            //do nothing
        }

        public override double GetDirectionOfMaxSpread()
        {
            return 0;
        }

        public override double GetFireIntensity()
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
            return _hasFuelLoad;
        }
    }
}
