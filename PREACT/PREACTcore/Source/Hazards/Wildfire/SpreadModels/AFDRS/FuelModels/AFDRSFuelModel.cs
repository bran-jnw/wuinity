using System;
using System.Collections.Generic;
using System.Text;

namespace PREACT.Wildfire.AFDRS
{
    public abstract class AFDRSFuelModel
    {
        public abstract AFDRSOutput Calculate(AFDRSInput input);
    }
}
