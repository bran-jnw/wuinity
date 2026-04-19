using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;
using System;

/*using var context = Context.CreateDefault();

Device? cuda = null;
Device? opencl = null;
Device? cpu = null;

foreach (var device in context)
{
    switch (device.AcceleratorType)
    {
        case AcceleratorType.Cuda:
            cuda = device;
            break;

        case AcceleratorType.OpenCL:
            opencl = device;
            break;

        case AcceleratorType.CPU:
            cpu = device;
            break;
    }
}

Accelerator? accelerator = cuda?.CreateAccelerator(context) ?? opencl?.CreateAccelerator(context) ?? cpu?.CreateAccelerator(context);

Console.WriteLine($"Using {accelerator.AcceleratorType}: {accelerator.Name}");*/

public class GaussianPuff
{
    static void Main()
    {
        using var context = Context.CreateDefault();

        Device? cuda = null;
        Device? opencl = null;
        Device? cpu = null;

        foreach (var device in context)
        {
            switch (device.AcceleratorType)
            {
                case AcceleratorType.Cuda:
                    cuda = device;
                    break;

                case AcceleratorType.OpenCL:
                    opencl = device;
                    break;

                case AcceleratorType.CPU:
                    cpu = device;
                    break;
            }
        }

        Accelerator? accelerator = cuda?.CreateAccelerator(context) ?? opencl?.CreateAccelerator(context) ?? cpu?.CreateAccelerator(context);

        Console.WriteLine($"Using {accelerator.AcceleratorType}: {accelerator.Name}");

        int nx = 32, ny = 32, nz = 16;
        int numCells = nx * ny * nz;
        int puffCount = 100;

        // Puff SoA buffers
        var X = accelerator.Allocate1D<float>(puffCount);
        var Y = accelerator.Allocate1D<float>(puffCount);
        var Z = accelerator.Allocate1D<float>(puffCount);
        var Q = accelerator.Allocate1D<float>(puffCount);
        var SX = accelerator.Allocate1D<float>(puffCount);
        var SY = accelerator.Allocate1D<float>(puffCount);
        var SZ = accelerator.Allocate1D<float>(puffCount);
        var VX = accelerator.Allocate1D<float>(puffCount);
        var VY = accelerator.Allocate1D<float>(puffCount);
        var VZ = accelerator.Allocate1D<float>(puffCount);
        var Age = accelerator.Allocate1D<float>(puffCount);
        var Kx = accelerator.Allocate1D<float>(puffCount);
        var Ky = accelerator.Allocate1D<float>(puffCount);
        var Kz = accelerator.Allocate1D<float>(puffCount);

        // Host init
        var rnd = new Random(1);
        float[] hx = new float[puffCount];
        float[] hy = new float[puffCount];
        float[] hz = new float[puffCount];
        float[] hq = new float[puffCount];
        float[] hsx = new float[puffCount];
        float[] hsy = new float[puffCount];
        float[] hsz = new float[puffCount];
        float[] hvx = new float[puffCount];
        float[] hvy = new float[puffCount];
        float[] hvz = new float[puffCount];
        float[] hkx = new float[puffCount];
        float[] hky = new float[puffCount];
        float[] hkz = new float[puffCount];

        for (int i = 0; i < puffCount; i++)
        {
            hx[i] = rnd.Next(0, nx);
            hy[i] = rnd.Next(0, ny);
            hz[i] = rnd.Next(0, nz);

            hq[i] = 1.0f;
            hsx[i] = hsy[i] = hsz[i] = 1.0f;

            hvx[i] = 0.1f;
            hvy[i] = 0.0f;
            hvz[i] = 0.0f;

            hkx[i] = hky[i] = hkz[i] = 0.01f;
        }

        X.CopyFromCPU(hx);
        Y.CopyFromCPU(hy);
        Z.CopyFromCPU(hz);
        Q.CopyFromCPU(hq);
        SX.CopyFromCPU(hsx);
        SY.CopyFromCPU(hsy);
        SZ.CopyFromCPU(hsz);
        VX.CopyFromCPU(hvx);
        VY.CopyFromCPU(hvy);
        VZ.CopyFromCPU(hvz);
        Kx.CopyFromCPU(hkx);
        Ky.CopyFromCPU(hky);
        Kz.CopyFromCPU(hkz);

        var puffSoA = new PuffSoA
        {
            X = X.View,
            Y = Y.View,
            Z = Z.View,
            Q = Q.View,
            SigmaX = SX.View,
            SigmaY = SY.View,
            SigmaZ = SZ.View,
            VX = VX.View,
            VY = VY.View,
            VZ = VZ.View,
            Age = Age.View,
            Kx = Kx.View,
            Ky = Ky.View,
            Kz = Kz.View,
            Count = puffCount
        };

        // Concentration field
        var concBuffer = accelerator.Allocate1D<float>(numCells);

        var concView = new ArrayView3D<float, Stride3D.DenseXY>(
            concBuffer.View,
            new Index3D(nx, ny, nz),
            new Stride3D.DenseXY(nx, nx * ny));

        // Active cells
        int numActiveCells = numCells;
        var activeCells = accelerator.Allocate1D<int>(numActiveCells);
        int[] hActive = new int[numActiveCells];
        for (int i = 0; i < numActiveCells; i++) hActive[i] = i;
        activeCells.CopyFromCPU(hActive);

        // Per-cell puff lists (demo: all puffs in all cells)
        var cellOffsets = accelerator.Allocate1D<int>(numActiveCells + 1);
        var cellPuffIndices = accelerator.Allocate1D<int>(numActiveCells * puffCount);

        int[] hOffsets = new int[numActiveCells + 1];
        int[] hPuffIdx = new int[numActiveCells * puffCount];

        for (int c = 0; c < numActiveCells; c++)
        {
            hOffsets[c] = c * puffCount;
            for (int i = 0; i < puffCount; i++)
                hPuffIdx[c * puffCount + i] = i;
        }
        hOffsets[numActiveCells] = numActiveCells * puffCount;

        cellOffsets.CopyFromCPU(hOffsets);
        cellPuffIndices.CopyFromCPU(hPuffIdx);

        // Kernels
        var advectKernel = accelerator.LoadStreamKernel<
            Index1D, PuffSoA, float>(GaussianPuffKernels.AdvectPuffsKernel);

        var concKernel = accelerator.LoadStreamKernel<
            Index1D,
            ArrayView3D<float, Stride3D.DenseXY>,
            PuffSoA,
            ArrayView<int>,
            ArrayView<int>,
            ArrayView<int>,
            GridParams>(GaussianPuffKernels.CellPuffConcentrationKernel);

        // Launch config
        var groupDim = new Index1D(128);
        var gridDim = new Index1D(
            XMath.DivRoundUp(numActiveCells, groupDim.X));
        var config = new KernelConfig(gridDim, groupDim);

        float dt = 1.0f;

        var gp = new GridParams
        {
            Nx = nx,
            Ny = ny,
            Nz = nz,
            X0 = 0f,
            Y0 = 0f,
            Z0 = 0f,
            Dx = 1f,
            Dy = 1f,
            Dz = 1f,
            NSigma = 4.0f
        };

        // Advection: extent = puffCount
        advectKernel(
            config,
            new Index1D(puffCount),
            puffSoA,
            dt);

        // Concentration: extent = numActiveCells
        concKernel(
            config,
            new Index1D(numActiveCells),
            concView,
            puffSoA,
            activeCells.View,
            cellOffsets.View,
            cellPuffIndices.View,
            gp);


        accelerator.Synchronize();

        Console.WriteLine("Done.");
    }
}