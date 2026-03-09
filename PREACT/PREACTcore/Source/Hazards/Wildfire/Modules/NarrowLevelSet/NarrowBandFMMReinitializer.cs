using System;
using System.Collections.Generic;
using PREACT.Math;

namespace PREACT.Wildfire
{
    public enum FmmState { Far, Narrow, Accepted }

    public class NarrowBandFMMReinitializer
    {
        private readonly double _dx;
        private readonly double _dy;
        private readonly double _maxDistance; // physical distance of band
        double[,] _dist;
        FmmState[,] _status;


        public NarrowBandFMMReinitializer(int nx, int ny, double dx, double dy, double maxDistance)
        {
            _dx = dx;
            _dy = dy;
            _maxDistance = maxDistance;
            _dist = new double[nx, ny];
            _status = new FmmState[nx, ny];
        }

        private class Node : IComparable<Node>
        {
            public int I, J;
            public double Dist;
            public int CompareTo(Node other) => Dist.CompareTo(other.Dist);
        }


        public void Reinitialize(NarrowBandLevelSetSolver solver)
        {
            int nx = solver.xDim;
            int ny = solver.yDim;
            double[,] phi = solver.Phi;           

            // Priority queue (min-heap)
            var heap = new SortedSet<Node>();

            double maxPhi = _maxDistance;

            // Init: mark band + FAR
            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    if (Mathd.Abs(phi[i, j]) <= maxPhi)
                    {
                        _dist[i, j] = double.PositiveInfinity;
                        _status[i, j] = FmmState.Far;
                    }
                    else
                    {
                        // outside band: we won't touch it
                        _dist[i, j] = phi[i, j];
                        _status[i, j] = FmmState.Accepted;
                    }
                }
            }

            //Seed NARROW band: cells near sign changes inside band
            for (int i = 1; i < nx - 1; i++)
            {
                for (int j = 1; j < ny - 1; j++)
                {
                    if (_status[i, j] != FmmState.Far)
                    {
                        continue;
                    }

                    double localPhi = phi[i, j];
                    if (localPhi == 0.0) //do not change this as this is the front
                    {
                        continue;
                    }

                    bool signChange =
                        (localPhi > 0 && (phi[i + 1, j] < 0 || phi[i - 1, j] < 0 || phi[i, j + 1] < 0 || phi[i, j - 1] < 0)) ||
                        (localPhi < 0 && (phi[i + 1, j] > 0 || phi[i - 1, j] > 0 || phi[i, j + 1] > 0 || phi[i, j - 1] > 0));

                    if (signChange)
                    {
                        _dist[i, j] = Mathd.Abs(localPhi);
                        _status[i, j] = FmmState.Narrow;
                        heap.Add(new Node { I = i, J = j, Dist = _dist[i, j] });
                    }
                }
            }

            // FMM inside band
            while (heap.Count > 0)
            {
                Node node = heap.Min;
                heap.Remove(node);

                int i = node.I;
                int j = node.J;

                if (_status[i, j] == FmmState.Accepted)
                {
                    continue;
                }                    

                _status[i, j] = FmmState.Accepted;

                TryUpdate(i + 1, j);
                TryUpdate(i - 1, j);
                TryUpdate(i, j + 1);
                TryUpdate(i, j - 1);
            }

            // Write back: inside band -> signed distance, outside band unchanged
            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    if (Mathd.Abs(phi[i, j]) <= maxPhi)
                    {
                        double s = Mathd.Sign(phi[i, j]);
                        if (s == 0.0) s = 1.0;
                        solver.Phi[i, j] = s * _dist[i, j];
                    }
                    else
                    {
                        // keep original phi
                        solver.Phi[i, j] = phi[i, j];
                    }
                }
            }

            // --- local helpers ---

            void TryUpdate(int ii, int jj)
            {
                if (ii < 0 || ii >= nx || jj < 0 || jj >= ny) return;
                if (_status[ii, jj] == FmmState.Accepted) return;
                if (Mathd.Abs(phi[ii, jj]) > maxPhi) return; // outside band

                double a1 = GetAccepted(ii - 1, jj);
                double a2 = GetAccepted(ii + 1, jj);
                double b1 = GetAccepted(ii, jj - 1);
                double b2 = GetAccepted(ii, jj + 1);

                double tx = Mathd.Min(a1, a2);
                double ty = Mathd.Min(b1, b2);

                double newVal = SolveEikonal(tx, ty, _dx, _dy);
                if (newVal < _dist[ii, jj])
                {
                    if (_status[ii, jj] == FmmState.Narrow)
                        heap.RemoveWhere(n => n.I == ii && n.J == jj);

                    _dist[ii, jj] = newVal;
                    _status[ii, jj] = FmmState.Narrow;
                    heap.Add(new Node { I = ii, J = jj, Dist = newVal });
                }
            }

            double GetAccepted(int ii, int jj)
            {
                if (ii < 0 || ii >= nx || jj < 0 || jj >= ny) return double.PositiveInfinity;
                return _status[ii, jj] == FmmState.Accepted ? _dist[ii, jj] : double.PositiveInfinity;
            }

        }

        private double SolveEikonal(double tx, double ty, double dx, double dy)
        {
            double a = tx;
            double b = ty;

            double dx2 = dx * dx;
            double dy2 = dy * dy;

            // Sort so a <= b
            if (a > b)
            {
                double tmp = a;
                a = b;
                b = tmp;
            }

            // Try 1D update
            double t = a + dx;
            if (t <= b)
            {
                return t;
            }

            // 2D update
            double A = 1.0 / dx2 + 1.0 / dy2;
            double B = -2.0 * (a / dx2 + b / dy2);
            double C = (a * a) / dx2 + (b * b) / dy2 - 1.0;

            double disc = B * B - 4.0 * A * C;
            if (disc < 0.0) disc = 0.0;

            return (-B + Mathd.Sqrt(disc)) / (2.0 * A);
        }
    }
}

