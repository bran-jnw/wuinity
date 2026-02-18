using System;
using static System.Math;
using System.Collections.Generic;
using System.Text;

namespace PREACT.Source.Hazards.Wildfire.SpreadModels.AFDRS
{
    public static class KBDI
    {
        static readonly double _cToF = 9.0 / 5.0 + 32;
        const double _mmTiInches = 0.03937007874;

        public static void CalculateKBDI(double temp, double cumulativePRCP, in double currentKBDI, out double newKBDI)
        {
            double dQ = 0.001 * (800.0 - currentKBDI) * (0.968 * Exp(0.0486 * temp * _cToF) - 0.830) / (1.0 + 10.88 * Exp(-0.0441 * cumulativePRCP * _mmTiInches));
            newKBDI = currentKBDI + Max(0, dQ);
        }        
    }
}
