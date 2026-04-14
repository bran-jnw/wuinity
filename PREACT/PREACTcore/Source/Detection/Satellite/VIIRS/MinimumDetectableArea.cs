using static System.Math;

namespace PREACT.Detection
{
    public sealed class MinimumDetectableFireArea
    {
        private readonly double _nedl;
        private readonly double _pixelAreaNadirM2;

        const double NEDL = 0.05;                 // W/(m²·sr·µm) — example
        const double PIXEL_AREA_NADIR = 375.0 * 375.0; // VIIRS M-band pixel area


        public MinimumDetectableFireArea(double nedl = NEDL, double pixelAreaNadirM2 = PIXEL_AREA_NADIR)
        {
            _nedl = nedl;
            _pixelAreaNadirM2 = pixelAreaNadirM2;
        }

        public double Compute(double fireTemperatureK, double backgroundTemperatureK, double scanAngleDeg)
        {
            double Lf = Planck4um.RadianceFromTemperature(fireTemperatureK);
            double Lbg = Planck4um.RadianceFromTemperature(backgroundTemperatureK);

            double dL = Lf - Lbg;
            if (dL <= 0.0)
                return double.PositiveInfinity;

            double thetaRad = scanAngleDeg * PI / 180.0;
            double cosTheta = Cos(thetaRad);
            if (cosTheta < 0.01)
                cosTheta = 0.01;

            double pixelArea = _pixelAreaNadirM2 / cosTheta;

            double pMin = _nedl / dL;

            double areaMin = pMin * pixelArea;
            return areaMin;
        }
    }
}
