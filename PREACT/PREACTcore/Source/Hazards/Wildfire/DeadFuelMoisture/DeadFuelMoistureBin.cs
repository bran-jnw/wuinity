using NFDRS4;

namespace PREACT.Wildfire
{
    public enum DeadFuelMoistureSizes { One, Ten, Hundred, Thousand };

    public class DeadFuelMoistureBin
    {
        DeadFuelMoisture _oneHour, _tenHour, _hundredHour, _thousandHour;
        bool _useSimpleOneHour, _include1000hour;
        double _at, _rh;

        public DeadFuelMoistureBin(bool useSimpleOneHour, bool include1000hour = false)
        {
            _useSimpleOneHour = useSimpleOneHour;
            _include1000hour = include1000hour;
            _oneHour = DeadFuelMoisture.createDeadFuelMoisture1("OneHour");        
            _tenHour = DeadFuelMoisture.createDeadFuelMoisture10("TenHour");
            _hundredHour = DeadFuelMoisture.createDeadFuelMoisture10("HundredHour");
            if(_include1000hour)
            {
                _thousandHour = DeadFuelMoisture.createDeadFuelMoisture1000("ThousandHour");
            }
        }

        // year     Observation year(4 digits).
        // month    Observation month(Jan==1, Dec==12).
        // day      Observation day-of-the-month[1..31].
        // hour     Observation elapsed hours in the day[0..23].
        // minute   Observation elapsed minutes in the hour(0..59].
        // second   Observation elapsed seconds in the minute[0..59].
        // ta Initial ambient air temperature(oC).
        // ha Initial ambient air relative humidity(g/g).
        // sr Initial solar radiation(W/m2).
        // rc Initial cumulative rainfall amount(cm).
        // ti Initial stick temperature(oC).
        // hi Initial stick surface relative humidty(g/g).
        // wi Initial stick fuel moisture fraction(g/g).
        // bp Initial stick barometric pressure(cal/cm3).
        public void InitializeEnvironment(int year, int month, int day, int hour, int minute, int second, double ta, double ha, double sr, double rc, double ti, double hi, double wi, double bp = 0.0218)
        {
            if(!_useSimpleOneHour)
            {
                _oneHour.initializeEnvironment(year, month, day, hour, minute, second, ta, ha, sr, rc, ti, hi, wi, bp);
            }            
            _tenHour.initializeEnvironment(year, month, day, hour, minute, second, ta, ha, sr, rc, ti, hi, wi, bp);
            _hundredHour.initializeEnvironment(year, month, day, hour, minute, second, ta, ha, sr, rc, ti, hi, wi, bp);
            if(_include1000hour)
            {
                _thousandHour.initializeEnvironment(year, month, day, hour, minute, second, ta, ha, sr, rc, ti, hi, wi, bp);
            }
        }

        // ta Initial ambient air temperature (oC).
        // ha Initial ambient air relative humidity (g/g).
        // sr Initial solar radiation (W/m2).
        // rc Initial cumulative rainfall amount (cm).
        // ti Initial stick temperature (oC).
        // hi Initial stick surface relative humidty (g/g).
        // wi Initial stick fuel moisture fraction (g/g).
        // bp Initial stick barometric pressure (cal/cm3).
        public void InitializeEnvironment(double ta, double ha, double sr, double rc, double ti, double hi, double wi, double bp = 0.0218)
        {
            if(!_useSimpleOneHour)
            {
                _oneHour.initializeEnvironment(ta, ha, sr, rc, ti, hi, wi, bp);
            }            
            _tenHour.initializeEnvironment(ta, ha, sr, rc, ti, hi, wi, bp);
            _hundredHour.initializeEnvironment(ta, ha, sr, rc, ti, hi, wi, bp);
            if(_include1000hour)
            {
                _thousandHour.initializeEnvironment(ta, ha, sr, rc, ti, hi, wi, bp);
            }
        }

        // year     Observation year(4 digits).
        // month    Observation month(Jan==1, Dec==12).
        // day      Observation day-of-the-month[1..31].
        // hour     Observation elapsed hours in the day[0..23].
        // minute   Observation elapsed minutes in the hour(0..59].
        // second   Observation elapsed seconds in the minute[0..59].
        // at   Current observation's ambient air temperature (oC).
        // rh   Current observation's ambient air relative humidity (g/g).
        // sW   Current observation's solar radiation (W/m2).
        public void UpdateDateTime(int year, int month, int day, int hour, int minute, int second, double at, double rh, double sW, double rcum, double bpr= 0.0218, bool prcpAsAmnt = false)
        {
            _at = at;
            _rh = rh;
            if (!_useSimpleOneHour)
            {
                _oneHour.update(year, month, day, hour, minute, second, at, rh, sW, rcum, bpr, prcpAsAmnt);
            }            
            _tenHour.update(year, month, day, hour, minute, second, at, rh, sW, rcum, bpr, prcpAsAmnt);
            _hundredHour.update(year, month, day, hour, minute, second, at, rh, sW, rcum, bpr, prcpAsAmnt);
            if (_include1000hour)
            {
                _thousandHour.update(year, month, day, hour, minute, second, at, rh, sW, rcum, bpr, prcpAsAmnt);
            }
        }


        // et   Elapsed time since the previous observation(h).
        // at   Current observation's ambient air temperature (oC).
        // rh   Current observation's ambient air relative humidity (g/g).
        // sW   Current observation's solar radiation (W/m2).
        // rcum Current observation's total cumulative rainfall amount (cm).
        public void UpdateHours(double et, double at, double rh, double sW, double rCum, double bpr = 0.0218, bool prcpAsAmnt = false)
        {
            _at = at;
            _rh = rh;
            if (!_useSimpleOneHour)
            {
                _oneHour.update(et, at, rh, sW, rCum, bpr, prcpAsAmnt);
            }
            _tenHour.update(et, at, rh, sW, rCum, bpr, prcpAsAmnt);
            _hundredHour.update(et, at, rh, sW, rCum, bpr, prcpAsAmnt);
            if(_include1000hour)
            {
                _thousandHour.update(et, at, rh, sW, rCum, bpr, prcpAsAmnt);
            }
        }

        public void GetMoisture(out double oneHour, out double tenHour, out double hundredHour, out double thousandHour)
        {
            if(_useSimpleOneHour)
            {
                oneHour = _oneHour.eqmc(_at, _rh);
            }
            else
            {
                oneHour = _oneHour.meanWtdMoisture();
            }                
            tenHour = _tenHour.meanWtdMoisture();
            hundredHour = _hundredHour.meanWtdMoisture();
            thousandHour = -1;
            if (_include1000hour)
            {
                thousandHour = _thousandHour.meanWtdMoisture();
            }
        }        
    }
}
