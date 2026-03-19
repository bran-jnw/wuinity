using PREACT.Math;
using System.Collections.Generic;

namespace PREACT.Wildfire
{
    public class ElmCloneSolver
    {
        private const double EPSILON = 1.0e-30;
        private const double BIG = 3e4;

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
        double[,] _phi_star;
        double[,] rhs;
        private Vector2d[,] _ROS;
        private Vector2d[,] _ROS_star;
        private readonly int _bandThickness;
        private readonly bool[,] _ignited;

        private Dictionary<int, CellIndex> _tagged;
        private Dictionary<int, CellIndex> _everTagged;
        private List<CellIndex> _cellsToIgnite;
        private List<int> _cellsToRemove;

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
            _cellsToRemove = new List<int>(Nx * Ny / 20);

            _phi = new double[xDim, yDim];
            _phi_star = new double[xDim, yDim];
            rhs = new double[xDim, yDim];
            _ROS = new Vector2d[xDim, yDim];
            _ROS_star = new Vector2d[xDim, yDim];

            _ignited = new bool[xDim, yDim];

            //initialize
            for (int i = 0; i < Nx; i++)
            {
                for (int j = 0; j < Ny; j++)
                {
                    _phi[i, j] = 1.0;
                    _phi_star[i, j] = 1.0;
                }
            }
        }

        public void Ignite(int ignIndexX, int ignIndexY, bool newIgnition)
        {
            if(newIgnition)
            {
                _phi[ignIndexX, ignIndexY] = -1.0; // ignition
                _phi_star[ignIndexX, ignIndexY] = -1.0;
            }            
            _ignited[ignIndexX, ignIndexY] = true;
            ExpandTagged(ignIndexX, ignIndexY);
        }

        private void ExpandTagged(int x, int y)
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

        int _untagCounter = 0;
        private void UpdateTagged()
        {
            _untagCounter += 1;            
            if(_untagCounter % 5 != 0 || _tagged.Count < 100)
            {
                return;
            }

            foreach (CellIndex index in _tagged.Values)
            {
                int x = index.X;
                int y = index.Y;

                bool untagBecauseBurned = true;
                for(int i = x - _bandThickness; i <= x + _bandThickness; ++i)
                {
                    if (_phi[i, y] > 0)
                    {
                        untagBecauseBurned = false;
                        break;
                    }
                }

                if(untagBecauseBurned)
                {
                    for (int j = y - _bandThickness; j <= y + _bandThickness; ++j)
                    {
                        if (_phi[x, j] > 0)
                        {
                            untagBecauseBurned = false;
                            break;
                        }
                    }
                }

                if(untagBecauseBurned)
                {
                    _cellsToRemove.Add(x + y * Nx);
                }                
            }

            foreach(int index in _cellsToRemove)
            {
                _tagged.Remove(index);
            }
            _cellsToRemove.Clear();
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
            double dphidx = Mathd.Clamp((phi[i + 1, j] - phi[i - 1, j]) * _2dx_inversed, -BIG, BIG);
            double dphidy = Mathd.Clamp((phi[i, j + 1] - phi[i, j - 1]) * _2dy_inversed, -BIG, BIG);
            double inverseMagnitude = 1.0 / (Mathd.Max(Mathd.Sqrt(dphidx * dphidx + dphidy * dphidy), EPSILON)); //avoid zero division

            return (dphidx * inverseMagnitude, dphidy * inverseMagnitude);
        }


        public void Step(double deltaTime, WeatherManager weather, TimeManager time, out double internalDeltaTime)
        {
            //step 1
            CalculateROS(out double maxROS, _phi, _ROS, weather, time);  
            if (maxROS <= 0.0)
            {
                internalDeltaTime = deltaTime;
                return;
            }
            double dtCFL = _cfl * Mathd.Min(dx, dy) / maxROS;
            internalDeltaTime = Mathd.Min(deltaTime, dtCFL);
            ComputeRhs(_phi, rhs, _ROS);
            foreach (CellIndex index in _tagged.Values)
            {
                int i = index.X;
                int j = index.Y;
                _phi_star[i, j] = Mathd.Clamp(_phi[i, j] - internalDeltaTime * rhs[i, j], -100.0, 100.0);
            }

            //Step 2
            CalculateROS(out maxROS, _phi_star, _ROS_star, weather, time);
            ComputeRhs(_phi_star, rhs, _ROS_star);
            _activeCells = 0;            
            foreach (CellIndex index in _tagged.Values)
            {
                int i = index.X;
                int j = index.Y;
                _phi[i, j] = 0.5 * (_phi[i, j] + (_phi_star[i, j] - internalDeltaTime * rhs[i, j]));

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
            _cellsToIgnite.Clear();

            UpdateTagged();
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

                double PHIEAST = 1.0, PHIWEST = 1.0, PHINORTH = 1.0, PHISOUTH = 1.0, DELTAUP, DELTALOC;

                //Apply flux limiter
                if(cellROS.x > 0.0)
                {
                    //east
                    DELTAUP = _phi[i, j] - _phi[i - 1, j];
                    DELTALOC = _phi[i + 1, j] - _phi[i, j];
                    if (Mathd.Abs(DELTALOC) > EPSILON)
                    {
                        PHIEAST = _phi[i, j] + HalfSuperbee(DELTAUP / DELTALOC) * DELTALOC;
                    }

                    //west
                    DELTALOC = -DELTAUP;
                    if(Mathd.Abs(DELTALOC) > EPSILON)
                    {
                        DELTAUP = _phi[i - 2, j] - _phi[i - 1, j];
                        PHIWEST = _phi[i - 1, j] - HalfSuperbee(DELTAUP / DELTALOC) * DELTALOC;
                    }
                }
                else
                {
                    //east
                    DELTALOC = _phi[i + 1, j] - _phi[i, j];
                    if(Mathd.Abs(DELTALOC) > EPSILON)
                    {
                        DELTAUP = _phi[i + 2, j] - _phi[i + 1, j];
                        PHIEAST = _phi[i + 1, j] - HalfSuperbee(DELTAUP / DELTALOC) * DELTALOC;
                    }

                    //west
                    DELTAUP = -DELTALOC;
                    DELTALOC = _phi[i - 1, j] - _phi[i, j];
                    if(Mathd.Abs(DELTALOC) > EPSILON)
                    {
                        PHIWEST = _phi[i, j] + HalfSuperbee(DELTAUP / DELTALOC) * DELTALOC;
                    }
                }

                double DPHIDX_LIMITED = (PHIEAST - PHIWEST) * _dx_inversed;
                DPHIDX_LIMITED = Mathd.Clamp(DPHIDX_LIMITED, -1000.0, 1000.0);

                if (cellROS.y > 0.0)
                {
                    // PHINORTH
                    DELTAUP = _phi[i, j] - _phi[i, j - 1];
                    DELTALOC = _phi[i, j + 1] - _phi[i, j];
                    if (Mathd.Abs(DELTALOC) > EPSILON)
                    {
                        PHINORTH = _phi[i, j] + HalfSuperbee(DELTAUP / DELTALOC) * DELTALOC;
                    }

                    // PHISOUTH
                    DELTALOC = -DELTAUP;
                    if (Mathd.Abs(DELTALOC) > EPSILON)
                    {
                        DELTAUP = _phi[i, j - 2] - _phi[i, j - 1];
                        PHISOUTH = _phi[i, j - 1] - HalfSuperbee(DELTAUP / DELTALOC) * DELTALOC;
                    }
                }
                else //UY.LT. 0
                {
                    // PHINORTH
                    DELTALOC = _phi[i, j + 1] - _phi[i, j];
                    if (Mathd.Abs(DELTALOC) > EPSILON)
                    {
                        DELTAUP = _phi[i, j + 2] - _phi[i, j + 1];
                        PHINORTH = _phi[i, j + 1] - HalfSuperbee(DELTAUP / DELTALOC) * DELTALOC;
                    }

                    // PHISOUTH
                    DELTAUP = -DELTALOC;
                    DELTALOC = _phi[i, j - 1] - _phi[i, j];
                    if (Mathd.Abs(DELTALOC) > EPSILON)
                    {
                        PHISOUTH = _phi[i, j] + HalfSuperbee(DELTAUP / DELTALOC) * DELTALOC;
                    }
                }

                double DPHIDY_LIMITED = (PHINORTH - PHISOUTH) * _dx_inversed;
                DPHIDY_LIMITED = Mathd.Clamp(DPHIDY_LIMITED, -1000.0, 1000.0);

                rhs[i, j] = (cellROS.x * DPHIDX_LIMITED + cellROS.y * DPHIDY_LIMITED);
            }
        }

        private static double HalfSuperbee(double r)
        {
            if (r <= 0.0)
            {
                return 0.0;
            }

            double a = Mathd.Min(0.5 * r, 1.0);
            double b = Mathd.Min(r, 0.5);

            return Mathd.Max(0.0, Mathd.Max(a, b));
        }
    }
}