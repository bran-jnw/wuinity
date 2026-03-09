using PREACT.Math;

namespace PREACT.Wildfire
{
    public class NarrowBand
    {
        public bool[,] Active;     // true = update phi here
        public double _maxDistance;         // in grid cells

        public NarrowBand(int nx, int ny, double maxDistance = 240.0) //8*30.0
        {
            Active = new bool[nx, ny];
            _maxDistance = maxDistance;
        }

        public void Build(double[,] phi)
        {
            int nx = phi.GetLength(0);
            int ny = phi.GetLength(1);

            // Reset
            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    Active[i, j] = false;
                }
            }    

            for (int i = 1; i < nx - 1; i++)
            {
                for (int j = 1; j < ny - 1; j++)
                {
                    double val = phi[i, j];

                    // Only include cells near the front
                    if (Mathd.Abs(val) <= _maxDistance)
                    {
                        Active[i, j] = true;
                    }                       
                }
            }
        }
    }
}

