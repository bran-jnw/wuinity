using PREACT.Math;

namespace PREACT.Wildfire
{
    public class NarrowBand
    {
        public bool[,] Active;     // true = update phi here
        public int Radius;         // in grid cells

        public NarrowBand(int nx, int ny, int radius)
        {
            Active = new bool[nx, ny];
            Radius = radius;
        }

        public void Build(double[,] phi, double dx, double dy)
        {
            int nx = phi.GetLength(0);
            int ny = phi.GetLength(1);

            // Reset
            for (int i = 0; i < nx; i++)
                for (int j = 0; j < ny; j++)
                    Active[i, j] = false;

            double maxDist = Radius * Mathd.Min(dx, dy);

            for (int i = 1; i < nx - 1; i++)
            {
                for (int j = 1; j < ny - 1; j++)
                {
                    double val = phi[i, j];

                    // Only include cells near the front
                    if (Mathd.Abs(val) <= maxDist)
                        Active[i, j] = true;
                }
            }
        }
    }
}

