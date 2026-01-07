using System;
using PREACT.Math;

namespace PREACT.Fire
{
    public class SpreadModelCFBP : SpreadModel
    {
        private float _eccentricity;

        private CFBPInputs _inputs;
        private MainOutputs _outputs;
        private SecondaryOutputs _secondaryOutputs;
        private FireData _head, _flank, _back;
        private CFBPFuel _fuel;

        public SpreadModelCFBP()
        {
            _inputs = new CFBPInputs();
            _outputs = new MainOutputs();
            _secondaryOutputs = new SecondaryOutputs();
            _head = new FireData();
            _flank = new FireData();
            _back = new FireData();
        }

        public override void DoRun()
        {
            CanadianFBP.Calculate(_inputs, _fuel, _outputs, _secondaryOutputs, _head, _flank, _back);
            _eccentricity = (float)Mathd.Sqrt(1.0 - _secondaryOutputs.LengthToBreadth * _secondaryOutputs.LengthToBreadth);
        }

        public override double GetMaxSpreadRate()
        {
            return (float)_head.RateOfSpread;
        }

        public override double GetMaxSpreadRateDirection()
        {
            return (float)_outputs.SpreadAzimuth;
        }

        public override double GetSpreadRateInDirection(double directionOfInterest)
        {
            double theta = Mathd.Abs(_outputs.SpreadAzimuth - directionOfInterest) * Mathd.Deg2Rad;
            double ROS = _head.RateOfSpread;
            double BROS = _back.RateOfSpread;
            double FROS = _flank.RateOfSpread;            

            double c1 = Mathd.Cos(theta);
            if (c1 == 0.0)
            {
                c1 = Mathd.Cos(theta + .001);
            }
            double s1 = Mathd.Sin(theta);
            
            double ROStheta = (((ROS - BROS) / (2 * c1) + (ROS + BROS) / (2 * c1)) * ((FROS * c1 * Mathd.Sqrt(FROS * FROS * c1 * c1 + (ROS * BROS) * s1 * s1) - ((ROS * ROS - BROS * BROS) / 4) * s1 * s1) / (FROS * FROS * c1 * c1 + ((ROS + BROS) / 2) * ((ROS + BROS) / 2) * s1 * s1)));
            
            return ROStheta;

            /* FARSITE approach
            double rosDirection = _head.RateOfSpread;
            if (_head.RateOfSpread != 0.0) // if forward spread rate is not zero
            {
                // Calculate the fire spread rate in this azimuth
                // if it deviates more than a tenth degree from the maximum azimuth

                // Calculate beta: the angle between the direction of max spread and the direction of interest
                double beta = Mathd.Abs(_outputs.SpreadAzimuth - directionOfInterest);

                // Calculate the fire spread rate in this azimuth
                // if it deviates more than a tenth degree from the maximum azimuth
                if (beta > 180.0)
                {
                    beta = (360.0 - beta);
                }
                if (Mathd.Abs(beta) > 0.1)
                {
                    double radians = beta * Mathd.PI / 180.0;
                    rosDirection = _head.RateOfSpread * (1.0 - _eccentricity) / (1.0 - _eccentricity * Mathd.Cos(radians));
                }
            }
            
            return rosDirection; */

        }

        public override void SetWind(double direction, double speed)
        {
            _inputs.WindAzimuth = (int)direction;
            _inputs.WindSpeed = speed;            
        }
    }
}
