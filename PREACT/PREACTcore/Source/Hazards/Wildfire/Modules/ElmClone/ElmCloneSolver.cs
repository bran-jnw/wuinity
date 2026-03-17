using PREACT.Math;
using System.Collections.Generic;

namespace PREACT.Wildfire
{
    public class ElmCloneSolver
    {
        private struct CellIndex
        {
            public int X;
            public int Y;

            public CellIndex(int x, int y)
            {
                X = x; 
                Y = y; 
            }
        }

        private double[,] _phi; // narrow-band, normalized [-1,1]
        double[,] _phiStar;
        double[,] rhs1;
        double[,] rhs2;
        private Vector2d[,] _ROS;
        private Vector2d[,] _ROS_star;
        private readonly int _bandThickness;
        private readonly bool[,] _ignited;

        private Dictionary<int, CellIndex> _tagged;
        private Dictionary<int, CellIndex> _everTagged;
        private List<CellIndex> _cellsToIgnite;

        private int _activeCells;
        public int ActiveCells { get => _activeCells; }

        public int Nx { get; }
        public int Ny { get; }
        public double dx { get; }
        public double dy { get; }

        private double _dx_inversed, _dy_inversed, _2dx_inversed, _2dy_inversed;
        

        private readonly double _cfl;
        private ElmClone _owner;

        public ElmCloneSolver(ElmClone owner, int xDim, int yDim, double dx, double dy, int bandThicknessCells = 2, double cfl = 0.5)
        {
            _owner = owner;
            Nx = xDim;
            Ny = yDim;
            this.dx = dx;
            this.dy = dy;

            _dx_inversed = 1.0 / dx;
            _dy_inversed = 1.0 / dy;
            _2dx_inversed = _dx_inversed * 0.5;
            _2dy_inversed = _dy_inversed * 0.5;

            _cfl = cfl;
            _bandThickness = bandThicknessCells;

            _tagged = new Dictionary<int, CellIndex>(Nx * Ny / 10);
            _everTagged = new Dictionary<int, CellIndex>(Nx * Ny / 5);
            _cellsToIgnite = new List<CellIndex>(Nx * Ny / 20);

            _phi = new double[xDim, yDim];
            _phiStar = new double[xDim, yDim];
            rhs1 = new double[xDim, yDim];
            rhs2 = new double[xDim, yDim];
            _ROS = new Vector2d[xDim, yDim];
            _ROS_star = new Vector2d[xDim, yDim];

            _ignited = new bool[xDim, yDim];

            //initialize
            for (int i = 0; i < Nx; i++)
            {
                for (int j = 0; j < Ny; j++)
                {
                    _phi[i, i] = 1;
                }
            }
        }

        public void Ignite(int ignIndexX, int ignIndexY, bool newIgnition)
        {
            if(newIgnition)
            {
                _phi[ignIndexX, ignIndexY] = -1f; // ignition
            }            
            _ignited[ignIndexX, ignIndexY] = true;
            AddTagged(ignIndexX, ignIndexY);
        }

        private void AddTagged(int x, int y)
        {
            int xMin = Mathd.Max(2, x - _bandThickness);
            int xMax = Mathd.Min(Nx - 2, x + _bandThickness);
            int yMin = Mathd.Max(2, y - _bandThickness);
            int yMax = Mathd.Min(Ny - 2, y + _bandThickness);


            for (int i = xMin; i < xMax; i++)
            {
                for (int j = yMin; j < yMax; j++)
                {
                    int index = i + j * Nx;
                    CellIndex c = new CellIndex(i, j);
                    if (_everTagged.TryAdd(index, c)) //has never been tagged
                    {
                        _tagged.TryAdd(index, c);
                    }
                }
            }                
        }

        private void RemoveTagged(int x, int y)
        {
            int index = x + y * Nx;
            _tagged.Remove(index);
        }

        private void CalculateROS(out double maxROS, double[,] phi, Vector2d[,] resultROS, WeatherManager weather, TimeManager time)
        {    
            maxROS = 0.0;

            foreach(CellIndex index in _tagged.Values)
            {
                int i = index.X;
                int j = index.Y;

                (double nxn, double nyn) = ComputeNormal(phi, i, j, dx, dy);
                Vector2d spreadVector = new Vector2d(nxn, nyn);
                double spreadDirection = Vector2d.Angle(Vector2d.up, spreadVector) * Mathd.Sign(Vector2d.Dot(Vector2d.right, spreadVector)); //relative to north
                if (spreadDirection < 0)
                {
                    spreadDirection += 360.0;
                }
                _owner.Spread[i, j].CalculateSpreadRate(weather, time);
                double ros = _owner.Spread[i, j].GetSpreadRateInDirection(spreadDirection);
                resultROS[i, j].x = ros * nxn;
                resultROS[i, j].y = ros * nyn;
                if (ros > maxROS)
                {
                    maxROS = ros;
                }
            }            
        }
        private (double normalX, double normalY) ComputeNormal(double[,] phi, int i, int j, double dx, double dy)
        {
            //von Neumann zero gradient at edges
            int xNeg = i > 0 ? i - 1 : i;
            int xPos = i < (Nx - 1) ? i + 1 : i;
            int yNeg = j > 0 ? j - 1 : j;
            int yPos = j < (Ny - 1) ? j + 1 : j;

            double dphidx = (phi[xPos, j] - phi[xNeg, j]) * _2dx_inversed;
            double dphidy = (phi[i, yPos] - phi[i, yNeg]) * _2dy_inversed;
            double inverseMagnitude = 1.0 / (Mathd.Sqrt(dphidx * dphidx + dphidy * dphidy) + 1e-9); //avoid zero division

            return (dphidx * inverseMagnitude, dphidy * inverseMagnitude);
        }


        public void Step(double deltaTime, WeatherManager weather, TimeManager time, out double internalDeltaTime)
        {
            CalculateROS(out double maxROS, _phi, _ROS, weather, time);            

            if (maxROS <= 0.0)
            {
                internalDeltaTime = deltaTime;
                return;
            }

            double dtCFL = _cfl * Mathd.Min(dx, dy) / maxROS;
            internalDeltaTime = Mathd.Min(deltaTime, dtCFL);

            // Gradient full step
            ComputeRhs(_phi, rhs1, _ROS);

            //intermediate
            foreach (CellIndex index in _tagged.Values)
            {
                int i = index.X;
                int j = index.Y;
                _phiStar[i, j] = _phi[i, j] - internalDeltaTime * rhs1[i, j];
            }

            // Gradient of intermediate step
            CalculateROS(out maxROS, _phiStar, _ROS_star, weather, time);
            ComputeRhs(_phiStar, rhs2, _ROS_star);

            //final
            _activeCells = 0;
            _cellsToIgnite.Clear();
            foreach (CellIndex index in _tagged.Values)
            {
                int i = index.X;
                int j = index.Y;
                _phi[i, j] = 0.5 * (_phi[i, j] + (_phiStar[i, j] - internalDeltaTime * rhs2[i, j]));

                if(_phi[i, j] <= 0 && !_ignited[i, j])
                {
                    _cellsToIgnite.Add(index);
                    _owner.UpdateCellData(i, j, (float)_owner.Spread[i, j].GetFireIntensity(), (float)_owner.Spread[i, j].GetMaxSpreadRate(), (float)_owner.Spread[i, j].GetDirectionOfMaxSpread());
                    _owner.SetTimeOfArrival(i, j, (float)(time.SimulationTime + internalDeltaTime));                    
                }
            }

            foreach(CellIndex c in _cellsToIgnite)
            {
                Ignite(c.X, c.Y, false);
            }
        }        

        /// <summary>
        /// Compute RHS = -U · grad(phi) using Godunov upwind derivatives, narrow-band only.
        /// </summary>
        private void ComputeRhs(double[,] phi, double[,] rhs, Vector2d[,] ros)
        {
            foreach (CellIndex index in _tagged.Values)
            {
                int i = index.X;
                int j = index.Y;
                Vector2d cellROS = ros[i, j];

                //von Neumann zero gradient at edges
                int xNeg = i > 0 ? i - 1 : i;
                int xPos = i < (Nx - 1) ? i + 1 : i;
                int yNeg = j > 0 ? j - 1 : j;
                int yPos = j < (Ny - 1) ? j + 1 : j;

                int xNeg2 = i > 1 ? i - 2 : xNeg;
                int xPos2 = i < (Nx - 2) ? i + 2 : xPos;
                int yNeg2 = j > 1 ? j - 2 : yNeg;
                int yPos2 = j < (Ny - 2) ? j + 2 : yPos;

                //local 
                double dphi_loc_west = phi[i - 1, j] - phi[i, j];
                double dphi_loc_east = phi[i + 1, j] - phi[i, j];
                double dphi_loc_south = phi[i, j - 1] - phi[i, j];
                double dphi_loc_north = phi[i, j + 1] - phi[i, j];

                double phi_west = phi[i, j], phi_east = phi[i, j];
                if (cellROS.x > 0f)
                {                    
                    double dphi_up_west = (phi[xNeg2, j] - phi[xNeg, j]);
                    double r_west = dphi_up_west / dphi_loc_west;
                    double B_r = Superbee(r_west);
                    phi_west = phi[xNeg, j] - 0.5 * B_r * dphi_loc_west;

                    double dphi_up_east = (phi[i, j] - phi[xNeg, j]);
                    double r_east = dphi_up_east / dphi_loc_east;
                    B_r = Superbee(r_east);
                    phi_east = phi[i, j] + 0.5 * B_r * dphi_loc_east;           
                }                    
                else
                {
                    double dphi_up_west = (phi[i, j] - phi[xPos, j]);
                    double r_west = dphi_up_west / dphi_loc_west;
                    double B_r = Superbee(r_west);
                    phi_west = phi[i, j] + 0.5 * B_r * dphi_loc_west;

                    double dphi_up_east = (phi[xPos2, j] - phi[xPos, j]);
                    double r_east = dphi_up_east / dphi_loc_east;
                    B_r = Superbee(r_east);
                    phi_east = phi[xPos, j] - 0.5 * B_r * dphi_loc_east;
                }

                double phi_south = phi[i, j], phi_north = phi[i, j];
                if (cellROS.y > 0f)
                {
                    double dphi_up_south = (phi[i, yNeg2] - phi[i, yNeg]);
                    double r_south = dphi_up_south / dphi_loc_south;
                    double B_r = Superbee(r_south);
                    phi_south = phi[i, yNeg] - 0.5 * B_r * dphi_loc_south;

                    double dphi_up_north = (phi[i, j] - phi[i, yNeg]);
                    double r_north = dphi_up_north / dphi_loc_north;
                    B_r = Superbee(r_north);
                    phi_north = phi[i, j] + 0.5 * B_r * dphi_loc_north;
                }                    
                else
                {
                    double dphi_up_south = (phi[i, j] - phi[i, yPos]);
                    double r_south = dphi_up_south / dphi_loc_south;
                    double B_r = Superbee(r_south);
                    phi_south = phi[i, j] + 0.5 * B_r * dphi_loc_south;

                    double dphi_up_north = (phi[i, yPos2] - phi[i, yPos]);
                    double r_north = dphi_up_north / dphi_loc_north;
                    B_r = Superbee(r_north);
                    phi_north = phi[i, yPos] - 0.5 * B_r * dphi_loc_north;
                }                    

                double dphidx = (phi_east - phi_west) * _dx_inversed;
                double dphidy = (phi_north - phi_south) * _dy_inversed;

                rhs[i, j] = (cellROS.x * dphidx + cellROS.y * dphidy);
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