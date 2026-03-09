using PREACT.Math;

namespace PREACT.Wildfire
{
    public class NarrowBandLevelSetSolver
    {
        public int xDim { get; }
        public int yDim { get; }
        public double Dx { get; }
        public double Dy { get; }
        public double[,] Phi { get; }
        public double[,] PhiNew { get; }
        public double[,] SpreadRateBuffer { get; }

        private readonly double _cfl;
        private NarrowBand _band;
        private NarrowBandFMMReinitializer _fmm;

        public NarrowBandLevelSetSolver(int xDim, int yDim, double dx, double dy, int bandCellWidth = 8, double cfl = 0.5)
        {
            this.xDim = xDim;
            this.yDim = yDim;
            Dx = dx;
            Dy = dy;

            _cfl = cfl;
            double bandDistance = bandCellWidth * Mathd.Min(Dx, Dy);
            _band = new NarrowBand(xDim, yDim, bandDistance);            
            _fmm = new NarrowBandFMMReinitializer(xDim, yDim, dx, dy, bandDistance);

            Phi = new double[xDim, yDim];
            PhiNew = new double[xDim, yDim];
            SpreadRateBuffer = new double[xDim, yDim];
        }

        public void SetInitialIgnition(double x0, double y0, double radius)
        {
            for (int i = 0; i < xDim; i++)
            {
                double xPos = (i + 0.5) * Dx;
                for (int j = 0; j < yDim; j++)
                {
                    double yPos = (j + 0.5) * Dy;
                    double dist = Mathd.Sqrt((xPos - x0) * (xPos - x0) + (yPos - y0) * (yPos - y0));
                    Phi[i, j] = dist - radius; // negative inside
                }
            }
        }

        public void Reinitialize()
        {
            _fmm.Reinitialize(this);
        }

        public void Step(NarrowLevelSet levelSet, double deltaTime, WeatherManager weather, TimeManager time, out double internalDeltaTime)
        {
            // Build/update narrow band
            _band.Build(Phi);

            // Compute max ROS for CFL
            double maxRos = 0;
            for (int i = 1; i < xDim - 1; i++)
            {
                for (int j = 1; j < yDim - 1; j++)
                {
                    if (!_band.Active[i, j])
                    {
                        continue;
                    }

                    (double nxn, double nyn) = ComputeNormal(Phi, i, j, Dx, Dy);
                    Vector2d spreadVector = new Vector2d(nxn, nyn);
                    double spreadDirection = Vector2d.Angle(Vector2d.up, spreadVector) * Mathd.Sign(Vector2d.Dot(Vector2d.right, spreadVector)); //relative to north
                    if (spreadDirection < 0)
                    {
                        spreadDirection += 360.0;
                    }

                    levelSet.Spread[i, j].CalculateSpreadRate(weather, time);
                    //levelSet.UpdateCellData(i, j, (float)levelSet.Spread[i, j].GetFireIntensity(), (float)levelSet.Spread[i, j].GetMaxSpreadRate(), (float)levelSet.Spread[i, j].GetDirectionOfMaxSpread());
                    double ros = levelSet.Spread[i, j].GetSpreadRateInDirection(spreadDirection);
                    SpreadRateBuffer[i, j] = ros;
                    if (ros > maxRos)
                    {
                        maxRos = ros;
                    }                    
                }
            }

            if (maxRos <= 0.0)
            {
                internalDeltaTime = deltaTime;
                return;
            }

            double dtCfl = _cfl * Mathd.Min(Dx, Dy) / maxRos;
            internalDeltaTime = Mathd.Min(deltaTime, dtCfl);

            // Update only narrow band cells
            for (int i = 1; i < xDim - 1; i++)
            {
                for (int j = 1; j < yDim - 1; j++)
                {
                    if (!_band.Active[i, j])
                    {
                        PhiNew[i, j] = Phi[i, j];
                        continue;
                    }

                    // Upwind gradients
                    double phi_x_plus = (Phi[i + 1, j] - Phi[i, j]) / Dx;
                    double phi_x_minus = (Phi[i, j] - Phi[i - 1, j]) / Dx;
                    double phi_y_plus = (Phi[i, j + 1] - Phi[i, j]) / Dy;
                    double phi_y_minus = (Phi[i, j] - Phi[i, j - 1]) / Dy;

                    double gradPlus = Mathd.Sqrt(
                        Mathd.Max(phi_x_minus, 0.0) * Mathd.Max(phi_x_minus, 0.0) +
                        Mathd.Min(phi_x_plus, 0.0) * Mathd.Min(phi_x_plus, 0.0) +
                        Mathd.Max(phi_y_minus, 0.0) * Mathd.Max(phi_y_minus, 0.0) +
                        Mathd.Min(phi_y_plus, 0.0) * Mathd.Min(phi_y_plus, 0.0)
                    );

                    double rosLocal = SpreadRateBuffer[i, j];
                    double dphi_dt = -rosLocal * gradPlus;
                    PhiNew[i, j] = Phi[i, j] + internalDeltaTime * dphi_dt;

                    if (PhiNew[i, j] == 0.0 || (PhiNew[i, j] < 0 && Phi[i, j] > 0))
                    {
                        levelSet.UpdateCellData(i, j, (float)levelSet.Spread[i, j].GetFireIntensity(), (float)levelSet.Spread[i, j].GetMaxSpreadRate(), (float)levelSet.Spread[i, j].GetDirectionOfMaxSpread());
                        levelSet.SetTimeOfArrival(i, j, time.SimulationTime);
                    }
                }
            }

            // Copy boundaries
            for (int i = 0; i < xDim; i++)
            {
                PhiNew[i, 0] = Phi[i, 0];
                PhiNew[i, yDim - 1] = Phi[i, yDim - 1];
            }
            for (int j = 0; j < yDim; j++)
            {
                PhiNew[0, j] = Phi[0, j];
                PhiNew[xDim - 1, j] = Phi[xDim - 1, j];
            }

            // Swap final result
            for (int i = 0; i < xDim; i++)
            {
                for (int j = 0; j < yDim; j++)
                {
                    Phi[i, j] = PhiNew[i, j];
                }
            }                
        }

        private static (double nx, double ny) ComputeNormal(double[,] phi, int i, int j, double dx, double dy)
        {
            double dphidx = (phi[i + 1, j] - phi[i - 1, j]) / (2.0 * dx);
            double dphidy = (phi[i, j + 1] - phi[i, j - 1]) / (2.0 * dy);
            double mag = Mathd.Sqrt(dphidx * dphidx + dphidy * dphidy) + 1e-9; //avoid zero division
            return (dphidx / mag, dphidy / mag);
        }
    }
}