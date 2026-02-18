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
        public double LengthToWidth;

        public AFDRSOutput(double fms, double ros, double direction, double intensity, double flameHeight, double lengthToWidth) 
        {
            FMC = fms;
            ROS = ros;
            Direction = direction;
            Intensity = intensity;
            FlameHeight = flameHeight;
            LengthToWidth = lengthToWidth;
        }
    }
}
