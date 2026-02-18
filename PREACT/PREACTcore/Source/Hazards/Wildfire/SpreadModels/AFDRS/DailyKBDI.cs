using System;
using static System.Math;
using System.Collections.Generic;
using System.Text;

namespace PREACT.Wildfire.AFDRS
{
    //https://wikifire.wsl.ch/tiki-index908f.html?page=Keetch-Byram+drought+index
    public class DailyKBDI
    {
        static readonly double _cToF = 9.0 / 5.0 + 32;
        const double _mmToInches = 0.03937007874;
        const double P_lim_metric = 5; //mm

        double _cumulativeRain;
        double _oldKBDI, _KBDI;
        double _meanAnnualPrcp;

        public double KBDI { get => _KBDI; }

        public DailyKBDI(double startKBDI, double meanAnnualPrcp)
        {
            _oldKBDI = startKBDI;
            _meanAnnualPrcp = meanAnnualPrcp;
        }

        /*public void CalculateDailyKBDI_Imperial(double tempF, double cumulativePRCP_hundredthInch, ref double KBDI)
        {
            double dQ = 0.001 * (800.0 - KBDI) * (0.968 * Exp(0.0486 * tempF) - 0.830) / (1.0 + 10.88 * Exp(-0.0441 * cumulativePRCP_hundredthInch));
            KBDI = KBDI + Max(0, dQ);
        }*/
        
        public void CalculateDailyKBDI_Metric(double tempC, double dailyPrcp)
        {
            if(dailyPrcp > 0)
            {
                _cumulativeRain += dailyPrcp;
            }
            else
            {
                _cumulativeRain = 0;
            }

            double P_net = Max(0, dailyPrcp - Max(0, P_lim_metric - _cumulativeRain));
            double Q_SI = (_oldKBDI - P_net);
            _oldKBDI = _KBDI;
            _KBDI = Q_SI + 0.001 * (203.2 - Q_SI) * (0.968 * Exp(0.875 * tempC + 1.5552) - 8.30) / (1.0 + 10.88 * Exp(-0.001736 * _meanAnnualPrcp));

        }
    }
}
