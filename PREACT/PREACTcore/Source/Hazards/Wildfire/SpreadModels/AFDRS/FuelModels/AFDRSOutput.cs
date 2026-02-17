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
        public double Eccentricity;

        public AFDRSOutput(double fms, double ros, double direction, double intensity, double flameHeight) //, double eccentricity
        {
            FMC = fms;
            ROS = ros;
            Direction = direction;
            Intensity = intensity;
            FlameHeight = flameHeight;
            Eccentricity = 1.0;// eccentricity;
        }
    }
}
