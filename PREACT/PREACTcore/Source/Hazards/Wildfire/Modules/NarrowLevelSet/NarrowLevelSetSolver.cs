using PREACT.Math;

namespace PREACT.Wildfire
{
    public class NarrowBandLevelSetSolver
    {
        private readonly double _cfl;
        private readonly NarrowBand _band;

        public NarrowBandLevelSetSolver(NarrowBand band, double cfl = 0.5)
        {
            _band = band;
            _cfl = cfl;
        }

        public void Step(NarrowLevelSet levelSet, double deltaTimeMax)
        {
            int nx = levelSet.xDim;
            int ny = levelSet.yDim;
            double dx = levelSet.Dx;
            double dy = levelSet.Dy;

            double[,] phi = levelSet.Phi;
            double[,] phiNew = new double[nx, ny];

            // Build/update narrow band
            _band.Build(phi, dx, dy);

            // Compute max ROS for CFL
            double maxRos = 0.0;
            double[,] spreadRates = new double[nx, ny];
            for (int i = 1; i < nx - 1; i++)
            {
                for (int j = 1; j < ny - 1; j++)
                {
                    if (!_band.Active[i, j])
                    {
                        continue;
                    }

                    (double nxn, double nyn) = ComputeNormal(phi, i, j, dx, dy);
                    //_spreadRateModel.GetRateOfSpread(i, j, nxn, nyn, state)
                    Vector2d spreadVector = new Vector2d(nxn, nyn);
                    double spreadDirection = Vector2d.Angle(Vector2d.up, spreadVector) * Mathd.Sign(Vector2d.Dot(Vector2d.right, spreadVector)); //relative to north
                    if (spreadDirection < 0)
                    {
                        spreadDirection += 360.0;
                    }
                    double ros = levelSet.Spread[i, j].GetSpreadRateInDirection(spreadDirection);
                    if (ros > maxRos)
                    {
                        maxRos = ros;
                    }
                    spreadRates[i, j] = ros;
                }
            }

            if (maxRos <= 0.0)
            {
                return;
            }

            double dtCfl = _cfl * Mathd.Min(dx, dy) / maxRos;
            double dt = Mathd.Min(deltaTimeMax, dtCfl);

            // Update only narrow band cells
            for (int i = 1; i < nx - 1; i++)
            {
                for (int j = 1; j < ny - 1; j++)
                {
                    if (!_band.Active[i, j])
                    {
                        phiNew[i, j] = phi[i, j];
                        continue;
                    }

                    // Upwind gradients
                    double phi_x_plus = (phi[i + 1, j] - phi[i, j]) / dx;
                    double phi_x_minus = (phi[i, j] - phi[i - 1, j]) / dx;
                    double phi_y_plus = (phi[i, j + 1] - phi[i, j]) / dy;
                    double phi_y_minus = (phi[i, j] - phi[i, j - 1]) / dy;

                    double gradPlus = Mathd.Sqrt(
                        Mathd.Max(phi_x_minus, 0.0) * Mathd.Max(phi_x_minus, 0.0) +
                        Mathd.Min(phi_x_plus, 0.0) * Mathd.Min(phi_x_plus, 0.0) +
                        Mathd.Max(phi_y_minus, 0.0) * Mathd.Max(phi_y_minus, 0.0) +
                        Mathd.Min(phi_y_plus, 0.0) * Mathd.Min(phi_y_plus, 0.0)
                    );

                    double rosLocal = spreadRates[i, j];

                    double dphi_dt = -rosLocal * gradPlus;
                    phiNew[i, j] = phi[i, j] + dt * dphi_dt;
                }
            }

            // Copy boundaries
            for (int i = 0; i < nx; i++)
            {
                phiNew[i, 0] = phi[i, 0];
                phiNew[i, ny - 1] = phi[i, ny - 1];
            }
            for (int j = 0; j < ny; j++)
            {
                phiNew[0, j] = phi[0, j];
                phiNew[nx - 1, j] = phi[nx - 1, j];
            }

            // Swap
            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    phi[i, j] = phiNew[i, j];
                }
            }                
        }

        private static (double nx, double ny) ComputeNormal(double[,] phi, int i, int j, double dx, double dy)
        {
            double dphidx = (phi[i + 1, j] - phi[i - 1, j]) / (2.0 * dx);
            double dphidy = (phi[i, j + 1] - phi[i, j - 1]) / (2.0 * dy);
            double mag = Mathd.Sqrt(dphidx * dphidx + dphidy * dphidy) + 1e-9;
            return (dphidx / mag, dphidy / mag);
        }
    }
}