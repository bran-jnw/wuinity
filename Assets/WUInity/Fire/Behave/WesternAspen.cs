using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static WUInity.Fire.MathWrap;

namespace WUInity.Fire
{
    public class WesternAspen
    {
        // Member variables
        double DBH_;
        double mortality_;

        public WesternAspen()
        {

        }

        ~WesternAspen()
        {

        }

        public void initializeMembers()
        {
            DBH_ = 0.0;
            mortality_ = 0.0;
        }

        public double getAspenDBH()
        {
            return DBH_;
        }

        public double getAspenMortality()
        {
            return mortality_;
        }

        double aspenInterpolate(double curing, ref double[] valueArray)
        {
            double[] curingArray = { 0.0, 0.3, 0.5, 0.7, 0.9, 1.000000001 };
            curing = (curing < 0.0) ? 0.0 : curing;
            curing = (curing > 1.0) ? 1.0 : curing;
            double fraction = 0.0;
            int i = 1;
            for (i = 1; i < curingArray.Length - 1; i++)
            {
                if (curing < curingArray[i])
                {
                    fraction = 1.0 - (curingArray[i] - curing) / (curingArray[i] - curingArray[i - 1]);
                    break;
                }
            }
            double value = valueArray[i - 1] + fraction * (valueArray[i] - valueArray[i - 1]);
            return value;
        }

        public double getAspenFuelBedDepth(int aspenFuelModelNumber)
        {
            int aspenFuelModelIndex = aspenFuelModelNumber - 1;
            double[] Depth = { 0.65, 0.30, 0.18, 0.50, 0.18 };
            return Depth[aspenFuelModelIndex];
        }

        public double getAspenMoistureOfExtinctionDead()
        {
            return 0.25;
        }

        public double getAspenHeatOfCombustionDead()
        {
            return 8000.0;
        }

        public double getAspenHeatOfCombustionLive()
        {
            return 8000.0;
        }

        public double getAspenLoadDeadOneHour(int aspenFuelModelNumber, double aspenCuringLevel)
        {
            int aspenFuelModelIndex = aspenFuelModelNumber - 1;
            double[][] Load = new double[][]
            {
                new double[]{ 0.800, 0.893, 1.056, 1.218, 1.379, 1.4595 },
                new double[]{ 0.738, 0.930, 1.056, 1.183, 1.309, 1.3720 },
                new double[]{ 0.601, 0.645, 0.671, 0.699, 0.730, 0.7455 },
                new double[]{ 0.880, 0.906, 1.037, 1.167, 1.300, 1.3665 },
                new double[]{ 0.754, 0.797, 0.825, 0.854, 0.884, 0.8990 }
            };
            double load = 0.0;
            if (aspenFuelModelIndex >= 0 && aspenFuelModelIndex< 5)
            {
                load = aspenInterpolate(aspenCuringLevel, ref Load[aspenFuelModelIndex]);
            }
            return load* 2000.0 / 43560.0;
        }

        public double getAspenLoadDeadTenHour(int aspenFuelModelNumber)
        {
            int aspenFuelModelIndex = aspenFuelModelNumber - 1;
            double[] Load = { 0.975, 0.475, 1.035, 1.340, 1.115 };
            double load = 0.0;
            if (aspenFuelModelIndex >= 0 && aspenFuelModelIndex < 5)
            {
                load = Load[aspenFuelModelIndex];
            }
            return load * 2000.0 / 43560.0;
        }

        public double getAspenLoadLiveHerbaceous(int aspenFuelModelNumber, double aspenCuringLevel)
        {
            int aspenFuelModelIndex = aspenFuelModelNumber - 1;
            double[][] Load = new double [][]
            {
                new double[]{ 0.335, 0.234, 0.167, 0.100, 0.033, 0.000 },
                new double[]{ 0.665, 0.465, 0.332, 0.199, 0.067, 0.000 },
                new double[]{ 0.150, 0.105, 0.075, 0.045, 0.015, 0.000 },
                new double[]{ 0.100, 0.070, 0.050, 0.030, 0.010, 0.000 },
                new double[]{ 0.150, 0.105, 0.075, 0.045, 0.015, 0.000 }
            };
            double load = 0.0;
            if (aspenFuelModelIndex >= 0 && aspenFuelModelIndex< 5)
            {
                load = aspenInterpolate(aspenCuringLevel, ref Load[aspenFuelModelIndex]);
            }
            return load* 2000.0 / 43560.0;
        }

        public double getAspenLoadLiveWoody(int aspenFuelModelNumber, double aspenCuringLevel)
        {
            int aspenFuelModelIndex = aspenFuelModelNumber - 1;
            double[][] Load = new double[][]
            {
                new double []{ 0.403, 0.403, 0.333, 0.283, 0.277, 0.2740 },
                new double []{ 0.000, 0.000, 0.000, 0.000, 0.000, 0.0000 },
                new double []{ 0.000, 0.000, 0.000, 0.000, 0.000, 0.0000 },
                new double []{ 0.455, 0.455, 0.364, 0.290, 0.261, 0.2465 },
                new double []{ 0.000, 0.000, 0.000, 0.000, 0.000, 0.0000 }
            };
            double load = 0.0;
            if (aspenFuelModelIndex >= 0 && aspenFuelModelIndex< 5)
            {
                load = aspenInterpolate(aspenCuringLevel, ref Load[aspenFuelModelIndex]);
            }
            return load * 2000.0 / 43560.0;
        }

        public double calculateAspenMortality(int severity, double flameLength, double DBH)
        {
            double mortality = 1.0;
            double charHeight = flameLength / 1.8;
            if (severity == 0)
            {
                mortality = 1.0 / (1.0 + exp(-4.407 + 0.638 * DBH - 2.134 * charHeight));
            }
            else if (severity == 1)
            {
                mortality = 1.0 / (1.0 + exp(-2.157 + 0.218 * DBH - 3.600 * charHeight));
            }
            mortality_ = (mortality < 0.0) ? 0.0 : mortality;
            mortality_ = (mortality > 1.0) ? 1.0 : mortality;
            return mortality_;
        }

        public double getAspenSavrDeadOneHour(int aspenFuelModelNumber, double aspenCuringLevel)
        {
            int aspenFuelModelIndex = aspenFuelModelNumber - 1;
            double[][] Savr = new double [][]
            {
                new double[]{ 1440.0, 1620.0, 1910.0, 2090.0, 2220.0, 2285.0 },
                new double[]{ 1480.0, 1890.0, 2050.0, 2160.0, 2240.0, 2280.0 },
                new double[]{ 1400.0, 1540.0, 1620.0, 1690.0, 1750.0, 1780.0 },
                new double[]{ 1350.0, 1420.0, 1710.0, 1910.0, 2060.0, 2135.0 },
                new double[]{ 1420.0, 1540.0, 1610.0, 1670.0, 1720.0, 1745.0 }
            };
            double savr = 1440.0;
            if (aspenFuelModelIndex >= 0 && aspenFuelModelIndex< 5)
            {
                savr = aspenInterpolate(aspenCuringLevel, ref Savr[aspenFuelModelIndex]);
            }
            return savr;
        }

        public double getAspenSavrDeadTenHour()
        {
            return 109.0;
        }

        public double getAspenSavrLiveHerbaceous()
        {
            return 2800.0;
        }

        public double getAspenSavrLiveWoody(int aspenFuelModelNumber, double aspenCuringLevel)
        {
            int aspenFuelModelIndex = aspenFuelModelNumber - 1;
            double[][] Savr = new double[][]
            {
                new double[]{ 2440.0, 2440.0, 2310.0, 2090.0, 1670.0, 1670.0 },
                new double[]{ 2440.0, 2440.0, 2440.0, 2440.0, 2440.0, 2440.0 },
                new double[]{ 2440.0, 2440.0, 2440.0, 2440.0, 2440.0, 2440.0 },
                new double[]{ 2530.0, 2530.0, 2410.0, 2210.0, 1800.0, 1800.0 },
                new double[]{ 2440.0, 2440.0, 2440.0, 2440.0, 2440.0, 2440.0 }
            };
            double savr = 2440.0;
            if (aspenFuelModelIndex >= 0 && aspenFuelModelIndex< 5)
            {
                savr = aspenInterpolate(aspenCuringLevel, ref Savr[aspenFuelModelIndex]);
            }
            return savr;
        }
    }
}

