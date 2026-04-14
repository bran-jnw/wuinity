using static System.Math;

namespace PREACT.Detection
{
    public static class Planck4um
    {
        private const double H = 6.62607015e-34;
        private const double C = 2.99792458e8;
        private const double K = 1.380649e-23;

        private const double WavelengthMicron = 4.0;
        private const double WavelengthMeter = WavelengthMicron * 1e-6;

        public static double RadianceFromTemperature(double temperatureK)
        {
            double c1 = 2.0 * H * C * C;
            double c2 = H * C / K;

            double lambda5 = Pow(WavelengthMeter, 5.0);
            double exponent = c2 / (WavelengthMeter * temperatureK);
            double denom = Exp(exponent) - 1.0;

            double L = c1 / (lambda5 * denom);
            return L * 1e-6; // per µm
        }
    }

    public static class Planck
    {
        // Planck constants in SI
        private const double H = 6.62607015e-34;   // J·s
        private const double C = 2.99792458e8;     // m/s
        private const double K = 1.380649e-23;     // J/K

        // Convert brightness temperature [K] to spectral radiance [W/(m^2·sr·µm)]
        public static double BtToRadiance(double temperatureK, double wavelengthMicron)
        {
            double lambda = wavelengthMicron * 1.0e-6; // m
            double c1 = 2.0 * H * C * C;
            double c2 = H * C / K;

            double lambda5 = Pow(lambda, 5.0);
            double exponent = c2 / (lambda * temperatureK);
            double denom = Exp(exponent) - 1.0;

            double L = (c1 / (lambda5 * denom)); // W/(m^2·sr·m)
                                                 // Convert per meter to per micron
            return L * 1.0e-6;
        }

        // Inverse Planck: radiance -> brightness temperature [K]
        public static double RadianceToBt(double radiance, double wavelengthMicron)
        {
            double lambda = wavelengthMicron * 1.0e-6; // m
            double c1 = 2.0 * H * C * C;
            double c2 = H * C / K;

            double Lm = radiance * 1.0e6; // back to per meter
            double term = c1 / (lambda * lambda * lambda * lambda * lambda * Lm) + 1.0;
            double exponent = Log(term);

            double temperatureK = c2 / (lambda * exponent);
            return temperatureK;
        }
    }
}
