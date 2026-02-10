using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace PREACT.Wildfire
{
    public class DeadFuelMoistureEngine
    {
        //initial guess of size, 7*8*10 multiplied by how many elevation bins there will be (assuming all aspects, slopes and canopy covers bins will be represented), in this case 3000 meters is represented
        private Dictionary<uint, DeadFuelMoistureBin> _deadFuelMoistureBins = new Dictionary<uint, DeadFuelMoistureBin>(8400);
        private bool _initialized = false;

        public DeadFuelMoistureEngine(int[,] elevation, int[,] slope, int[,] aspect, int[,] canopyCover, bool useSimpleOneHour, bool include1000hour = false) 
        {
            int xDim = elevation.GetLength(0);
            int yDim = elevation.GetLength(1);

            for (int y = 0; y < yDim; ++y)
            {
                for (int x = 0; x < yDim; ++x)
                {
                    uint hash = GetBinHash(elevation[x, y], slope[x, y], aspect[x, y], canopyCover[x, y]); //, 400, 45, 15, 20
                    if (!_deadFuelMoistureBins.ContainsKey(hash))
                    {
                        DeadFuelMoistureBin bin = new DeadFuelMoistureBin(useSimpleOneHour, include1000hour);
                        _deadFuelMoistureBins.Add(hash, bin);
                    }
                }
            }

            Console.WriteLine($"Number of bins are {_deadFuelMoistureBins.Count}.");
        }

        /// <summary>
        /// Needed to run.
        /// </summary>
        /// <param name="year">Observation year(4 digits).</param>
        /// <param name="month">Observation month(Jan==1, Dec==12)</param>
        /// <param name="day">Observation day-of-the-month[1..31]</param>
        /// <param name="hour">Observation elapsed hours in the day[0..23</param>
        /// <param name="minute">Observation elapsed minutes in the hour(0..59]</param>
        /// <param name="second">Observation elapsed seconds in the minute[0..59]</param>
        /// <param name="ta">Initial ambient air temperature(oC)</param>
        /// <param name="ha">Initial ambient air relative humidity(g/g).</param>
        /// <param name="sr">Initial solar radiation(W/m2).</param>
        /// <param name="rc">Initial cumulative rainfall amount(cm).</param>
        /// <param name="ti">Initial stick temperature(oC).</param>
        /// <param name="hi">Initial stick surface relative humidty(g/g).</param>
        /// <param name="wi">Initial stick fuel moisture fraction(g/g).</param>
        /// <param name="bp">Initial stick barometric pressure(cal/cm3).</param>
        public void InitializeEnvironment(int year, int month, int day, int hour, int minute, int second, double ta, double ha, double sr, double rc, double ti, double hi, double wi, double bp = 0.0218)
        {
            foreach (DeadFuelMoistureBin bin in _deadFuelMoistureBins.Values)
            {
                bin.InitializeEnvironment(year, month, day, hour, minute, second, ta, ha, sr, rc, ti, hi, wi, bp);
            }
            _initialized = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ta">Initial ambient air temperature (oC).</param>
        /// <param name="ha">Initial ambient air relative humidity (g/g).</param>
        /// <param name="sr">Initial solar radiation (W/m2).</param>
        /// <param name="rc">Initial cumulative rainfall amount (cm).</param>
        /// <param name="ti">Initial stick temperature (oC).</param>
        /// <param name="hi">Initial stick surface relative humidty (g/g).</param>
        /// <param name="wi">Initial stick fuel moisture fraction (g/g).</param>
        /// <param name="bp">Initial stick barometric pressure (cal/cm3).</param>
        public void InitializeEnvironment(double ta, double ha, double sr, double rc, double ti, double hi, double wi, double bp = 0.0218)
        {
            foreach(DeadFuelMoistureBin bin in _deadFuelMoistureBins.Values)
            {
                bin.InitializeEnvironment(ta, ha, sr, rc, ti, hi, wi, bp);
            }
            _initialized = true;
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
        public void UpdateDateTime(int year, int month, int day, int hour, int minute, int second, double at, double rh, double sW, double rcum, double bpr = 0.0218, bool prcpAsAmnt = false)
        {
            if(!_initialized)
            {
                return;
            }

            Parallel.ForEach(_deadFuelMoistureBins.Values, bin =>
            {
                bin.UpdateDateTime(year, month, day, hour, minute, second, at, rh, sW, rcum, bpr, prcpAsAmnt);
            });

            if (hour % 24 == 0)
            {
                Console.WriteLine($"Day number {(hour / 24)}.");
            }
        }


        // et   Elapsed time since the previous observation(h).
        // at   Current observation's ambient air temperature (oC).
        // rh   Current observation's ambient air relative humidity (g/g).
        // sW   Current observation's solar radiation (W/m2).
        // rcum Current observation's total cumulative rainfall amount (cm).
        public void UpdateHours(double et, double at, double rh, double sW, double rCum, double bpr = 0.0218, bool prcpAsAmnt = false)
        {
            if (!_initialized)
            {
                return;
            }

            Parallel.ForEach(_deadFuelMoistureBins.Values, bin =>
            {
                bin.UpdateHours(et, at, rh, sW, rCum, bpr, prcpAsAmnt);
            });

            _totalTime += et;
            int hour = (int)_totalTime;
            if ( hour % 24 == 0)
            {
                Console.WriteLine($"Day number {(hour / 24)}.");
            }
        }
        double _totalTime = 0;

        public void GetDeadFuelMoisture(int elevation, int slope, int aspect, int canopyCover, out double oneHour, out double tenHour, out double hundredHour, out double thousandHour)
        {    
            oneHour = -1;
            tenHour = -1;
            hundredHour = -1;
            thousandHour = -1;

            if (!_initialized)
            {
                return;
            }

            uint hash = GetBinHash(elevation, slope, aspect, canopyCover);
            DeadFuelMoistureBin bin;

            if (_deadFuelMoistureBins.TryGetValue(hash, out bin))
            {
                bin.GetMoisture(out oneHour, out tenHour, out hundredHour, out thousandHour);
            }
        }

        private static uint GetBinHash(int elevation, int slope, int aspect, int canopyCover, uint elevationBinWidth = 200, uint aspectBinWidth = 45, uint slopeBinWidth = 10, uint canopyCoverBinWidth = 15)
        {
            uint hash = (uint)canopyCover / canopyCoverBinWidth; //between 0-6 with default of 15
            hash += ((uint)aspect / aspectBinWidth) * 10; //between 0-7*10 with default of 45 degrees
            hash += ((uint)slope / slopeBinWidth) * 100; //between 0-9*100 with defaul tof 10 degrees, as physically any slope over 90 degrees cannot be represented with a 2D DEM
            hash += ((uint)System.Math.Max(0, elevation) / elevationBinWidth) * 1000; //at worst approx. 0-9000 m (highest point on earth), so approx from 0-45*1000 with default bin width of 200, below 0 is considered 0

            return hash;
        }
    }
}
