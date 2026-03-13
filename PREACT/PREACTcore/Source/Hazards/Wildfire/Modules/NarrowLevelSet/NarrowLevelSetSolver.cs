using PREACT.Math;

namespace PREACT.Wildfire
{
    public class NarrowBandLevelSetSolver
    {
        private double[,] _phi; // narrow-band, normalized [-1,1]
        double[,] phiStar;
        double[,] rhs1;
        double[,] rhs2;

        private bool[,] _activeFront;
        private bool[,] _burnt;

        private int _activeCells;
        public int ActiveCells { get => _activeCells; }

        const double _phiLimit = 1.0; // |phi| <= 1 is the band
        int _bandThicknessCells;

        public int nx { get; }
        public int ny { get; }
        public double dx { get; }
        public double dy { get; }
        private Vector2d[,] _spreadRateBuffer;

        int _stepCount = 0;

        private readonly double _cfl;
        private NarrowBandFMMReinitializer _fmm;

        public NarrowBandLevelSetSolver(int xDim, int yDim, double dx, double dy, int bandThicknessCells = 2, double cfl = 0.5)
        {
            nx = xDim;
            ny = yDim;
            this.dx = dx;
            this.dy = dy;

            _cfl = cfl;
            _bandThicknessCells = bandThicknessCells;
            _fmm = new NarrowBandFMMReinitializer(xDim, yDim, dx, dy, bandThicknessCells);

            _phi = new double[xDim, yDim];
            phiStar = new double[xDim, yDim];
            rhs1 = new double[xDim, yDim];
            rhs2 = new double[xDim, yDim];
            _spreadRateBuffer = new Vector2d[xDim, yDim];
            _burnt = new bool[xDim, yDim];            
            _activeFront = new bool[xDim, yDim];

            //initialize field
            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    _phi[i, j] = 1;  // unburned
                }
            }
        }

        public void SetIgnition(double x0, double y0)
        {
            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    double x = (i + 0.5) * dx;
                    double y = (j + 0.5) * dy;

                    double distMeters = Mathd.Sqrt((x - x0) * (x - x0) + (y - y0) * (y - y0));
                    double distCells = distMeters / Mathd.Min(dx, dy);

                    double phiNorm = Mathd.Min(distCells / _bandThicknessCells, 1.0);

                    if (distCells < 0.5)
                    {
                        _phi[i, j] = -1.0;      // burned core
                    }                        
                    else
                    {
                        _phi[i, j] = phiNorm;  // unburned
                    }   
                }
            }
        }

        public void Step(NarrowLevelSet levelSet, double deltaTime, WeatherManager weather, TimeManager time, out double internalDeltaTime)
        {
            // Compute max ROS for CFL
            double maxRos = 0;
            for (int i = 1; i < nx - 1; i++)
            {
                for (int j = 1; j < ny - 1; j++)
                {                    
                    if (!_activeFront[i, j])
                    {
                        continue;
                    }

                    (double nxn, double nyn) = ComputeNormal(_phi, i, j, dx, dy);
                    Vector2d spreadVector = new Vector2d(nxn, nyn);
                    double spreadDirection = Vector2d.Angle(Vector2d.up, spreadVector) * Mathd.Sign(Vector2d.Dot(Vector2d.right, spreadVector)); //relative to north
                    if (spreadDirection < 0)
                    {
                        spreadDirection += 360.0;
                    }
                    levelSet.Spread[i, j].CalculateSpreadRate(weather, time);
                    double ros = levelSet.Spread[i, j].GetSpreadRateInDirection(spreadDirection);
                    _spreadRateBuffer[i, j].x = ros * nxn;
                    _spreadRateBuffer[i, j].y = ros * nyn;
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

            double dtCfl = _cfl * Mathd.Min(dx, dy) / maxRos;
            internalDeltaTime = Mathd.Min(deltaTime, dtCfl);


            // 3. Stage 1: k1 = -U · grad(phi0)
            ComputeRhs(_phi, rhs1);
            // predictor: phi * = phi0 + dt * k1(only in band)
            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    if (_activeFront[i, j])
                    {                        
                        phiStar[i, j] = _phi[i, j] + internalDeltaTime * rhs1[i, j];
                    }                        
                    else
                    {
                        phiStar[i, j] = _phi[i, j];
                    }                        
                }
            }

            // 4. Stage 2: k2 = -U* · grad(phi*)
            ComputeRhs(phiStar, rhs2);
            // 5. Final update: phi^{n+1} = phi0 + dt/2 (k1 + k2) in band
            _activeCells = 0;
            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    if (Mathd.Abs(_phi[i, j]) <= _phiLimit)
                    {
                        _phi[i, j] = _phi[i, j] + 0.5 * internalDeltaTime * (rhs1[i, j] + rhs2[i, j]);
                        if (_phi[i, j] <= 0 && !_burnt[i, j])
                        {
                            levelSet.UpdateCellData(i, j, (float)levelSet.Spread[i, j].GetFireIntensity(), (float)levelSet.Spread[i, j].GetMaxSpreadRate(), (float)levelSet.Spread[i, j].GetDirectionOfMaxSpread());
                            levelSet.SetTimeOfArrival(i, j, time.SimulationTime);
                        }
                    }
                    /*else
                    {
                        _phi[i, j] = val;                        
                    }*/
                }
            }

        }

        private static (double nx, double ny) ComputeNormal(double[,] phi, int i, int j, double dx, double dy)
        {
            double dphidx = (phi[i + 1, j] - phi[i - 1, j]) / (2.0 * dx);
            double dphidy = (phi[i, j + 1] - phi[i, j - 1]) / (2.0 * dy);
            double inverseMagnitude = 1.0 / (Mathd.Sqrt(dphidx * dphidx + dphidy * dphidy) + 1e-9); //avoid zero division
            return (dphidx * inverseMagnitude, dphidy * inverseMagnitude);
        }

        /// <summary>
        /// Compute RHS = -U · grad(phi) using Godunov upwind derivatives, narrow-band only.
        /// </summary>
        private void ComputeRhs(double[,] phi, double[,] rhs)
        {
            SuperbeeGradients(phi, nx, ny, dx, dy, out var dphix, out var dphiy);

            rhs = new double[ny, nx];

            /*foreach (var c in band.Active)
            {
                int i = c.I;
                int j = c.J;

                double ux = Ux[j, i];
                double uy = Uy[j, i];

                rhs[j, i] = -(ux * dphix[j, i] + uy * dphiy[j, i]);
            }*/

        }

        private static void SuperbeeGradients(double[,] phi, int nx, int ny, double dx, double dy, out double[,] dphix, out double[,] dphiy)
        {
            dphix = new double[ny, nx];
            dphiy = new double[ny, nx];

            for (int j = 0; j < ny; j++)
            {
                for (int i = 0; i < nx; i++)
                {
                    int ip = (i + 1 < nx) ? i + 1 : i;
                    int im = (i - 1 >= 0) ? i - 1 : i;
                    int ipp = (i + 2 < nx) ? i + 2 : ip;
                    int imm = (i - 2 >= 0) ? i - 2 : im;

                    int jp = (j + 1 < ny) ? j + 1 : j;
                    int jm = (j - 1 >= 0) ? j - 1 : j;
                    int jpp = (j + 2 < ny) ? j + 2 : jp;
                    int jmm = (j - 2 >= 0) ? j - 2 : jm;

                    // X-direction
                    double dxF = (phi[j, ip] - phi[j, i]) / dx;
                    double dxB = (phi[j, i] - phi[j, im]) / dx;
                    double dxFF = (phi[j, ipp] - phi[j, ip]) / dx;
                    double dxBB = (phi[j, im] - phi[j, imm]) / dx;

                    double rx = (Mathd.Abs(dxF) > 1e-12) ? dxB / dxF : 0.0;
                    double rxB = (Mathd.Abs(dxB) > 1e-12) ? dxF / dxB : 0.0;

                    double sxF = Superbee(rx) * dxF;
                    double sxB = Superbee(rxB) * dxB;

                    // choose centered limited slope (Elmfire-style: symmetric)
                    dphix[j, i] = 0.5 * (sxF + sxB);

                    // Y-direction
                    double dyF = (phi[jp, i] - phi[j, i]) / dy;
                    double dyB = (phi[j, i] - phi[jm, i]) / dy;
                    double dyFF = (phi[jpp, i] - phi[jp, i]) / dy;
                    double dyBB = (phi[jm, i] - phi[jmm, i]) / dy;

                    double ry = (Mathd.Abs(dyF) > 1e-12) ? dyB / dyF : 0.0;
                    double ryB = (Mathd.Abs(dyB) > 1e-12) ? dyF / dyB : 0.0;

                    double syF = Superbee(ry) * dyF;
                    double syB = Superbee(ryB) * dyB;

                    dphiy[j, i] = 0.5 * (syF + syB);
                }
            }                
        }

        private static double Superbee(double r)
        {
            if (r <= 0.0)
            {
                return 0.0;
            }

            double a = Mathd.Min(2.0 * r, 1.0);
            double b = Mathd.Min(r, 2.0);

            return Mathd.Max(0.0, Mathd.Max(a, b));
        }
    }
}