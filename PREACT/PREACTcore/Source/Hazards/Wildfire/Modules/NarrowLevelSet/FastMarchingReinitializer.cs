using System;
using System.Collections.Generic;
using PREACT.Math;

namespace PREACT.Wildfire
{
    public enum FmmState { Far, Narrow, Accepted }

    public class FastMarchingReinitializer
    {
        private readonly double _dx;
        private readonly double _dy;

        public FastMarchingReinitializer(double dx, double dy)
        {
            _dx = dx;
            _dy = dy;
        }

        private class Node : IComparable<Node>
        {
            public int I, J;
            public double Phi;

            public int CompareTo(Node other)
            {
                return Phi.CompareTo(other.Phi);
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
            if (t <= b) return t;

            // 2D update
            double A = 1.0 / dx2 + 1.0 / dy2;
            double B = -2.0 * (a / dx2 + b / dy2);
            double C = (a * a) / dx2 + (b * b) / dy2 - 1.0;

            double disc = B * B - 4.0 * A * C;
            if (disc < 0.0) disc = 0.0;

            return (-B + Mathd.Sqrt(disc)) / (2.0 * A);
        }

        public void Reinitialize(NarrowLevelSet levelSet)
        {
            int nx = levelSet.xDim;
            int ny = levelSet.yDim;

            double[,] phi = levelSet.Phi;
            double[,] dist = new double[nx, ny];
            FmmState[,] status = new FmmState[nx, ny];

            // Priority queue (min-heap)
            var heap = new SortedSet<Node>();

            // 1. Initialize all nodes as FAR
            for (int i = 0; i < nx; i++)
                for (int j = 0; j < ny; j++)
                {
                    dist[i, j] = double.PositiveInfinity;
                    status[i, j] = FmmState.Far;
                }

            // 2. Identify zero-crossing neighbors → initialize NARROW band
            for (int i = 1; i < nx - 1; i++)
            {
                for (int j = 1; j < ny - 1; j++)
                {
                    if (phi[i, j] == 0.0) continue;

                    bool signChange =
                        (phi[i, j] > 0 && (phi[i + 1, j] < 0 || phi[i - 1, j] < 0 || phi[i, j + 1] < 0 || phi[i, j - 1] < 0)) ||
                        (phi[i, j] < 0 && (phi[i + 1, j] > 0 || phi[i - 1, j] > 0 || phi[i, j + 1] > 0 || phi[i, j - 1] > 0));

                    if (signChange)
                    {
                        dist[i, j] = Mathd.Abs(phi[i, j]);
                        status[i, j] = FmmState.Narrow;
                        heap.Add(new Node { I = i, J = j, Phi = dist[i, j] });
                    }
                }
            }

            // 3. Fast Marching loop
            while (heap.Count > 0)
            {
                Node node = heap.Min;
                heap.Remove(node);

                int i = node.I;
                int j = node.J;

                status[i, j] = FmmState.Accepted;

                // Update neighbors
                TryUpdate(i + 1, j, i, j);
                TryUpdate(i - 1, j, i, j);
                TryUpdate(i, j + 1, i, j);
                TryUpdate(i, j - 1, i, j);
            }

            // 4. Restore sign of original phi
            for (int i = 0; i < nx; i++)
                for (int j = 0; j < ny; j++)
                    levelSet.Phi[i, j] = Mathd.Sign(phi[i, j]) * dist[i, j];

            // --- Local function: update neighbor ---
            void TryUpdate(int ii, int jj, int i0, int j0)
            {
                if (ii < 0 || ii >= nx || jj < 0 || jj >= ny) return;
                if (status[ii, jj] == FmmState.Accepted) return;

                double a = GetAcceptedNeighbor(ii - 1, jj);
                double b = GetAcceptedNeighbor(ii + 1, jj);
                double c = GetAcceptedNeighbor(ii, jj - 1);
                double d = GetAcceptedNeighbor(ii, jj + 1);

                double dx = _dx;
                double dy = _dy;

                double tx = Mathd.Min(a, b);
                double ty = Mathd.Min(c, d);

                double newVal = SolveEikonal(tx, ty, dx, dy);

                if (newVal < dist[ii, jj])
                {
                    // Remove old entry if exists
                    if (status[ii, jj] == FmmState.Narrow)
                    {
                        heap.RemoveWhere(n => n.I == ii && n.J == jj);
                    }

                    dist[ii, jj] = newVal;
                    status[ii, jj] = FmmState.Narrow;

                    heap.Add(new Node { I = ii, J = jj, Phi = newVal });
                }
            }

            // --- Local helper: get accepted neighbor distance ---
            double GetAcceptedNeighbor(int ii, int jj)
            {
                if (ii < 0 || ii >= nx || jj < 0 || jj >= ny) return double.PositiveInfinity;
                return status[ii, jj] == FmmState.Accepted ? dist[ii, jj] : double.PositiveInfinity;
            }
        }
    }
}

