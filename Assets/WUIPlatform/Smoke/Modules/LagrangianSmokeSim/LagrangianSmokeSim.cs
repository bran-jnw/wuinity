//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace WUIPlatform.Smoke
{
    public class LagrangianSmokeSim : SmokeModule
    {
        Context context;
        Accelerator device;
        Action<Index1D, CanvasData, ParticleSystem, float> particleProcessingKernel;
        int particleCount;
        CanvasData c;
        HostParticleSystem hostParticleSystem;

        public LagrangianSmokeSim(int xDim, int yDim, int particlesPerCell)
        {
            context = Context.Create(builder => builder.Default().EnableAlgorithms());
            device = context.GetPreferredDevice(preferCPU: false).CreateAccelerator(context);

            int width = xDim;
            int height = yDim;

            particleCount = particlesPerCell * xDim * yDim;

            byte[] h_bitmapData = new byte[width * height * 3];

            using MemoryBuffer2D<Vec3, Stride2D.DenseY> canvasData = device.Allocate2DDenseY<Vec3>(new Index2D(width, height));
            using MemoryBuffer1D<byte, Stride1D.Dense> d_bitmapData = device.Allocate1D<byte>(width * height * 3);

            c = new CanvasData(canvasData, d_bitmapData, width, height);

            hostParticleSystem = new HostParticleSystem(device, particleCount, width, height);

            var frameBufferToBitmap = device.LoadAutoGroupedStreamKernel<Index2D, CanvasData>(CanvasData.CanvasToBitmap);
            particleProcessingKernel = device.LoadAutoGroupedStreamKernel<Index1D, CanvasData, ParticleSystem, float>(ParticleSystem.particleKernel);

            frameBufferToBitmap(canvasData.Extent.ToIntIndex(), c);
            device.Synchronize();

            d_bitmapData.CopyToCPU(h_bitmapData);

            //bitmap magic that ignores bitmap striding, be careful some sizes will mess up the striding
            using Bitmap b = new Bitmap(width, height, width * 3, PixelFormat.Format24bppRgb, Marshal.UnsafeAddrOfPinnedArrayElement(h_bitmapData, 0));
            b.Save("out.bmp");
        }

        public override int GetCellsX()
        {
            throw new NotImplementedException();
        }

        public override int GetCellsY()
        {
            throw new NotImplementedException();
        }

        public override float[] GetGroundSoot()
        {
            throw new NotImplementedException();
        }

        public override void Step(float currentTime, float deltaTime)
        {
            particleProcessingKernel(particleCount, c, hostParticleSystem.deviceParticleSystem, deltaTime);
            device.Synchronize();
        }

        public override bool IsSimulationDone()
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            hostParticleSystem.Dispose();
        }
    }

    public struct CanvasData
    {
        public ArrayView2D<Vec3, Stride2D.DenseY> canvas;
        public ArrayView1D<byte, Stride1D.Dense> bitmapData;
        public int width;
        public int height;

        public CanvasData(ArrayView2D<Vec3, Stride2D.DenseY> canvas, ArrayView1D<byte, Stride1D.Dense> bitmapData, int width, int height)
        {
            this.canvas = canvas;
            this.bitmapData = bitmapData;
            this.width = width;
            this.height = height;
        }

        public void setColor(Index2D index, Vec3 c)
        {
            if ((index.X >= 0) && (index.X < canvas.IntExtent.X) && (index.Y >= 0) && (index.Y < canvas.IntExtent.Y))
            {
                canvas[index] = c;
            }
        }

        public static void CanvasToBitmap(Index2D index, CanvasData c)
        {
            Vec3 color = c.canvas[index];

            int bitmapIndex = ((index.Y * c.width) + index.X) * 3;

            c.bitmapData[bitmapIndex] = (byte)(255.99f * color.x);
            c.bitmapData[bitmapIndex + 1] = (byte)(255.99f * color.y);
            c.bitmapData[bitmapIndex + 2] = (byte)(255.99f * color.z);

            c.canvas[index] = new Vec3(0, 0, 0);
        }
    }

    public class HostParticleSystem : IDisposable
    {
        public int particleCount;
        public MemoryBuffer1D<Vec3, Stride1D.Dense> positions;
        public MemoryBuffer1D<Vec3, Stride1D.Dense> velocities;
        public MemoryBuffer1D<Vec3, Stride1D.Dense> accelerations;
        public ParticleSystem deviceParticleSystem;

        public HostParticleSystem(Accelerator device, int particleCount, int width, int height)
        {
            this.particleCount = particleCount;
            Vec3[] poses = new Vec3[particleCount];
            System.Random rng = new System.Random();

            for (int i = 0; i < particleCount; i++)
            {
                poses[i] = new Vec3((float)rng.NextDouble() * width, (float)rng.NextDouble() * height, 1);
            }

            positions = device.Allocate1D(poses);
            velocities = device.Allocate1D<Vec3>(particleCount);
            accelerations = device.Allocate1D<Vec3>(particleCount);

            velocities.MemSetToZero();
            accelerations.MemSetToZero();

            deviceParticleSystem = new ParticleSystem(positions, velocities, accelerations, width, height);
        }

        public void Dispose()
        {
            positions.Dispose();
            velocities.Dispose();
            accelerations.Dispose();
        }
    }

    public struct ParticleSystem
    {
        public ArrayView1D<Vec3, Stride1D.Dense> positions;
        public ArrayView1D<Vec3, Stride1D.Dense> velocities;
        public float gc;
        public Vec3 centerPos;
        public float centerMass;

        public ParticleSystem(ArrayView1D<Vec3, Stride1D.Dense> positions, ArrayView1D<Vec3, Stride1D.Dense> velocities, ArrayView1D<Vec3, Stride1D.Dense> accelerations, int width, int height)
        {
            this.positions = positions;
            this.velocities = velocities;
            gc = 0.001f;
            centerPos = new Vec3(0.5f * width, 0.5f * height, 0);
            centerMass = (float)positions.Length;
        }

        public static void particleKernel(Index1D index, CanvasData c, ParticleSystem p, float dt)
        {
            Vec3 pos = p.update(index, dt);
            Index2D position = new Index2D((int)pos.x, (int)pos.y);
            c.setColor(position, new Vec3(1, 1, 1));
        }

        private void updatePosition(int ID)
        {
            positions[ID] = positions[ID] + velocities[ID];
        }

        private void updateVelocity(int ID)
        {
            velocities[ID] = velocities[ID];
        }

        public Vec3 update(int ID, float dt)
        {
            updatePosition(ID);
            updateVelocity(ID);
            return positions[ID];
        }
    }

    public struct Vec3
    {
        public float x;
        public float y;
        public float z;

        public Vec3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vec3 operator +(Vec3 v1, Vec3 v2)
        {
            return new Vec3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }

        public static Vec3 operator -(Vec3 v1, Vec3 v2)
        {
            return new Vec3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }

        public static Vec3 operator *(Vec3 v1, float v)
        {
            return new Vec3(v1.x * v, v1.y * v, v1.z * v);
        }

        public float length()
        {
            return XMath.Sqrt(x * x + y * y + z * z);
        }
    }
}

