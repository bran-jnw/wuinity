using System;
using System.Collections.Generic;
using PREACT.Math;

namespace PREACT.Wildfire
{
    public enum State { Far, Considered, Accepted } //Nodes are labeled as far (not yet visited), considered (visited and value tentatively assigned), and accepted (visited and value permanently assigned).

    public class NarrowBandFMMReinitializer
    {
        private readonly double _nx;
        private readonly double _ny;
        private readonly double _dx;
        private readonly double _dy;
        private readonly int _bandThicknessCells;
        private double _bandThicknessMeters;
        private double[,] _dist;
        State[,] _status;

        //https://en.wikipedia.org/wiki/Fast_marching_method
        public NarrowBandFMMReinitializer(int nx, int ny, double dx, double dy, int bandThicknessCells)
        {
            _nx = nx;
            _ny = ny;
            _dx = dx;
            _dy = dy;
            _bandThicknessCells = bandThicknessCells;
            _bandThicknessMeters = _bandThicknessCells * _dx;
            _dist = new double[nx, ny];
            _status = new State[nx, ny];
        }

        private class Node : IComparable<Node>
        {
            public int I, J;
            public double Dist;
            public int CompareTo(Node other) => Dist.CompareTo(other.Dist);
        }


        public void Reinitialize(double[,] phi, bool[,] activeFront)
        {
            // Priority queue (min-heap)
            SortedSet<Node> heap = new SortedSet<Node>();            

            //init all to far
            for (int i = 0; i < _nx; i++)
            {
                for (int j = 0; j < _ny; j++)
                {
                    _dist[i, j] = double.PositiveInfinity;
                    _status[i, j] = State.Far;
                }
            }

            //Identify interface nodes and seed them with dist = 0
            for (int i = 1; i < _nx - 1; i++)
            {
                for (int j = 1; j < _ny - 1; j++)
                {
                    bool front = false;
                    if (phi[i, j] == 0.0)
                    {
                        front = true;
                    }
                    else
                    {
                        bool signChange =
                        (phi[i, j] > 0 && (phi[i + 1, j] < 0 || phi[i - 1, j] < 0 || phi[i, j + 1] < 0 || phi[i, j - 1] < 0)) ||
                        (phi[i, j] < 0 && (phi[i + 1, j] > 0 || phi[i - 1, j] > 0 || phi[i, j + 1] > 0 || phi[i, j - 1] > 0));

                        front = signChange;
                    }                    

                    //this is out estimate of front
                    if (front)
                    {
                        _dist[i, j] = 0.0;
                        _status[i, j] = State.Considered;
                        heap.Add(new Node { I = i, J = j, Dist = 0.0 });
                    }
                }
            }

            //Fast Marching propagation
            while (heap.Count > 0)
            {
                Node node = heap.Min;
                heap.Remove(node);

                int i = node.I;
                int j = node.J;
                double d = node.Dist;


                if (_status[i, j] == State.Accepted)
                {
                    continue;
                }
                // narrow-band cutoff
                if (d > _bandThicknessMeters)
                {
                    continue; 
                }

                _status[i, j] = State.Accepted;

                Update(phi, heap, i + 1, j);
                Update(phi, heap, i - 1, j);
                Update(phi, heap, i, j + 1);
                Update(phi, heap, i, j - 1);

            }

            // Normalize to [-1,1] based on distance in cells
            for (int i = 0; i < _nx; i++)
            {
                for (int j = 0; j < _ny; j++)
                {
                    double d = _dist[i, j];
                    double s = Mathd.Sign(phi[i, j]);
                    if (s == 0.0)
                    {
                        s = 1.0;
                    }

                    if (d < double.PositiveInfinity)
                    {
                        double distCells = d / _dx;
                        double phiNorm = Mathd.Min(distCells / _bandThicknessCells, 1.0);
                        phi[i, j] = s * phiNorm;
                        activeFront[i, j] = true;
                    }
                    else
                    {
                        // outside band, clamp to +-1
                        phi[i, j] = s * 1.0;
                        activeFront[i, j] = false;
                    }
                }
            }            
        }

        void Update(double[,] phi, SortedSet<Node> heap, int i, int j)
        {
            //outside
            if (i < 0 || i >= _nx || j < 0 || j >= _ny)
            {
                return;
            }

            //already done
            if (_status[i, j] == State.Accepted)
            {
                return;
            }

            double a1 = GetAccepted(i - 1, j);
            double a2 = GetAccepted(i + 1, j);
            double b1 = GetAccepted(i, j - 1);
            double b2 = GetAccepted(i, j + 1);

            double tx = Mathd.Min(a1, a2);
            double ty = Mathd.Min(b1, b2);

            double newDistance = SolveEikonal(tx, ty, _dx, _dy);
            if (newDistance < _dist[i, j])
            {
                if (_status[i, j] == State.Considered)
                {
                    heap.RemoveWhere(n => n.I == i && n.J == j);
                }

                _dist[i, j] = newDistance;
                _status[i, j] = State.Considered;
                heap.Add(new Node { I = i, J = j, Dist = newDistance });
            }
        }

        double GetAccepted(int i, int j)
        {
            if (i < 0 || i >= _nx || j < 0 || j >= _ny)
            {
                return double.PositiveInfinity;
            }

            return _status[i, j] == State.Accepted ? _dist[i, j] : double.PositiveInfinity;
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
            if (disc < 0.0)
            {
                disc = 0.0;
            }

            return (-B + Mathd.Sqrt(disc)) / (2.0 * A);
        }
    }
}

