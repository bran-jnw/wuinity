using PREACT.Detection;
using static System.Math;
namespace PREACT.Source.Detection.Satellite
{
    public sealed class FireRadiativePowerEstimator
    {
        // Wavelength of VIIRS I04/M13 in microns (tune as needed)
        private readonly double _wavelengthMicron;
        // Empirical constant to convert excess radiance to FRP [MW]
        // You will tune this from ATBD / literature.
        private readonly double _frpGain;

        public FireRadiativePowerEstimator(double wavelengthMicron, double frpGain)
        {
            _wavelengthMicron = wavelengthMicron;
            _frpGain = frpGain;
        }

        public double EstimateFrpMw(
            double btPixelK,
            double btBackgroundK,
            double viewZenithDeg,
            double pixelAreaNadirM2)
        {
            //Convert BT to radiance
            double Lpix = Planck.BtToRadiance(btPixelK, _wavelengthMicron);
            double Lbg = Planck.BtToRadiance(btBackgroundK, _wavelengthMicron);

            //Excess radiance due to fire
            double dL = Lpix - Lbg;
            if (dL <= 0.0)
                return 0.0;

            //Adjust pixel area for off-nadir
            double thetaRad = viewZenithDeg * PI / 180.0;
            double cosTheta = Cos(thetaRad);
            if (cosTheta <= 0.0)
                cosTheta = 1e-3;

            //Projected area grows ~1/cos(theta); we want nadir-equivalent FRP
            double effectiveArea = pixelAreaNadirM2 / cosTheta;

            //FRP ~ gain * dL * area
            double frpMw = _frpGain * dL * effectiveArea * 1.0e-6; // W -> MW if gain is in W/(m^2·sr·µm)^-1

            if (frpMw < 0.0)
                frpMw = 0.0;

            return frpMw;
        }
    }
}
