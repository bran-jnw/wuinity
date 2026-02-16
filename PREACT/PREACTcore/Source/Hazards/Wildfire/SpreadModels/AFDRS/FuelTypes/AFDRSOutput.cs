using System;
using System.Collections.Generic;
using System.Text;

namespace PREACT.Wildfire.AFDRS
{
    public struct AFDRSOutput
    {
        public double FMC;
        public double ROS;
        public double Direction;
        public double Intensity;
        public double FlameHeight;

        public AFDRSOutput(double fms, double ros, double direction, double intensity, double flameHeight)
        {
            FMC = fms;
            ROS = ros;
            Direction = direction;
            Intensity = intensity;
            FlameHeight = flameHeight;
        }
    }
}
