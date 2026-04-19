using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;

public struct PuffSoA
{
    public ArrayView<float> X, Y, Z;
    public ArrayView<float> Q;
    public ArrayView<float> SigmaX, SigmaY, SigmaZ;
    public ArrayView<float> VX, VY, VZ;
    public ArrayView<float> Age;
    public ArrayView<float> Kx, Ky, Kz;
    public int Count;
}

public struct GridParams
{
    public int Nx, Ny, Nz;
    public float X0, Y0, Z0;
    public float Dx, Dy, Dz;
    public float NSigma;
}

public static class GaussianPuffKernels
{
    public static void AdvectPuffsKernel(
        Index1D index,
        PuffSoA p,
        float dt)
    {
        int i = index;
        if (i >= p.Count)
            return;

        float x = p.X[i];
        float y = p.Y[i];
        float z = p.Z[i];

        float vx = p.VX[i];
        float vy = p.VY[i];
        float vz = p.VZ[i];

        float sx = p.SigmaX[i];
        float sy = p.SigmaY[i];
        float sz = p.SigmaZ[i];

        float kx = p.Kx[i];
        float ky = p.Ky[i];
        float kz = p.Kz[i];

        x += vx * dt;
        y += vy * dt;
        z += vz * dt;

        sx = XMath.Sqrt(sx * sx + 2f * kx * dt);
        sy = XMath.Sqrt(sy * sy + 2f * ky * dt);
        sz = XMath.Sqrt(sz * sz + 2f * kz * dt);

        p.X[i] = x;
        p.Y[i] = y;
        p.Z[i] = z;

        p.SigmaX[i] = sx;
        p.SigmaY[i] = sy;
        p.SigmaZ[i] = sz;

        p.Age[i] += dt;
    }

    public static void CellPuffConcentrationKernel(
        Index1D index,
        ArrayView3D<float, Stride3D.DenseXY> conc,
        PuffSoA p,
        ArrayView<int> activeCells,
        ArrayView<int> cellOffsets,
        ArrayView<int> cellPuffIndices,
        GridParams gp)
    {
        int c = index;
        if (c >= activeCells.Length)
            return;

        int cellId = activeCells[c];

        int ix = cellId % gp.Nx;
        int iy = (cellId / gp.Nx) % gp.Ny;
        int iz = cellId / (gp.Nx * gp.Ny);

        float x = gp.X0 + ix * gp.Dx;
        float y = gp.Y0 + iy * gp.Dy;
        float z = gp.Z0 + iz * gp.Dz;

        int start = cellOffsets[c];
        int end = cellOffsets[c + 1];

        float cTotal = 0f;

        for (int k = start; k < end; k++)
        {
            int pi = cellPuffIndices[k];

            float px = p.X[pi];
            float py = p.Y[pi];
            float pz = p.Z[pi];

            float sx = p.SigmaX[pi];
            float sy = p.SigmaY[pi];
            float sz = p.SigmaZ[pi];

            float dxp = x - px;
            float dyp = y - py;
            float dzp = z - pz;

            float maxS = XMath.Max(sx, XMath.Max(sy, sz));
            float rMax = gp.NSigma * maxS;
            float r2Max = rMax * rMax;
            float r2 = dxp * dxp + dyp * dyp + dzp * dzp;

            if (r2 > r2Max)
                continue;

            float sx2 = sx * sx;
            float sy2 = sy * sy;
            float sz2 = sz * sz;

            float norm = p.Q[pi] /
                (XMath.Pow(2f * XMath.PI, 1.5f) * sx * sy * sz);

            float expo = -0.5f * (
                dxp * dxp / sx2 +
                dyp * dyp / sy2 +
                dzp * dzp / sz2);

            cTotal += norm * XMath.Exp(expo);
        }

        conc[new Index3D(ix, iy, iz)] = cTotal;
    }
}