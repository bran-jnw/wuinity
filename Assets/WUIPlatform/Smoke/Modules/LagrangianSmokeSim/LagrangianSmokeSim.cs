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
using System.Numerics;
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

        private int xDim, yDim;

        const float VON_KARMAN = 0.41f;
        const float VON_KARMAN_INVERSE = 2.43902439f;
        const float PI = 3.1415927410125732421875f;
        const float PI_HALF = 1.57079637050628662109375f;
        const float GRAVITY = 9.81f;
        const float GRAVITY_INVERSE = 0.1019367992f;

        //these are set/calculated on CPU
        float L;// = -350.0; //Obukhov length
        float L_inverse;// = -0.0028571429; //Obukhov length inversed
        float u_star; //friction velocity
        float u_star_squared; //friction velocity squared
        float z_0;//= 1.0; //Davenport-Wierenga roughness length classification
        float theta_zero; //ground level potential temperature

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
            using MemoryBuffer2D<float, Stride2D.DenseX> heightmap = device.Allocate2DDenseX<float>(new Index2D(width, height));

            c = new CanvasData(canvasData, d_bitmapData, width, height);

            hostParticleSystem = new HostParticleSystem(device, particleCount, width, height, heightmap);

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
            return xDim;
        }

        public override int GetCellsY()
        {
            return yDim;
        }

        public override float[] GetExtinctionCoefficientDensity()
        {
            throw new NotImplementedException();
        }

        public override float GetGroundExtinctionCoefficientAtWorldPos(Vector2d pos)
        {
            throw new System.NotImplementedException();
        }

        public override float GetGroundExtinctionCoefficientAtCoordinate(Vector2d latLon)
        {
            throw new System.NotImplementedException();
        }

        //move to CPU
        //friction velocity
        float get_u_star(float u_ref, float z_ref, float z0)
        {
            //FDS users guide
            return VON_KARMAN * u_ref / XMath.Log(z_ref / z_0);
        }

        //scaling potential temperature
        float theta_star()
        {
            return u_star_squared * theta_zero * GRAVITY_INVERSE * VON_KARMAN_INVERSE * L_inverse;
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
        public MemoryBuffer2D<float, Stride2D.DenseX> _heightMap;
        public ParticleSystem deviceParticleSystem;

        public HostParticleSystem(Accelerator device, int particleCount, int width, int height, MemoryBuffer2D<float, Stride2D.DenseX> heightMap)
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
            _heightMap = heightMap;

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

        const float VON_KARMAN = 0.41f;
        const float VON_KARMAN_INVERSE = 2.43902439f;
        const float PI = 3.1415927410125732421875f;
        const float PI_HALF =  1.57079637050628662109375f;
        const float GRAVITY = 9.81f;
        const float GRAVITY_INVERSE = 0.1019367992f;

        //these are set/calculated on CPU
        float L;// = -350.0; //Obukhov length
        float L_inverse;// = -0.0028571429; //Obukhov length inversed
        float u_star; //friction velocity
        float u_star_squared; //friction velocity squared
        float z_0;//= 1.0; //Davenport-Wierenga roughness length classification
        float theta_zero; //ground level potential temperature
        float theta_star; //scaling potential temperature
        float mixing_layer_height;
        float mixing_layer_height_inverse;

        public ParticleSystem(ArrayView1D<Vec3, Stride1D.Dense> positions, ArrayView1D<Vec3, Stride1D.Dense> velocities, ArrayView1D<Vec3, Stride1D.Dense> accelerations, int width, int height)
        {
            this.positions = positions;
            this.velocities = velocities;

            L = 350f;
            L_inverse = 1f / L;
            u_star = 0f;
            u_star_squared = u_star * u_star;
            z_0 = 0f;
            theta_zero = 0f;
            theta_star = 0f;
            mixing_layer_height = 0f;
            mixing_layer_height_inverse = 1f / mixing_layer_height;
        }

        public static void particleKernel(Index1D index, CanvasData c, ParticleSystem p, float dt)
        {
            Vec3 pos = p.update(index, dt);
            Index2D position = new Index2D((int)pos.x, (int)pos.y);
            c.setColor(position, new Vec3(1, 1, 1));
        }

        private void updatePosition(int ID, float dt)
        {
            positions[ID] = positions[ID] + velocities[ID] * dt;
        }

        private void updateVelocity(int ID)
        {
            velocities[ID] = velocities[ID];
        }

        public Vec3 update(int ID, float dt)
        {
            updatePosition(ID, dt);
            updateVelocity(ID);
            return positions[ID];
        }

        float psi_stable(float z)
        {
            return -5.0f * z * L_inverse;
        }

        //Momentum flux profile relationship
        float psi_M(float z)
        {
            float value;
            if (L < 0.0)
            {
                float zeta = z * L_inverse;
                float zeta_squared = zeta * zeta;
                value = 2.0f * XMath.Log((1.0f + zeta) * 0.5f) + XMath.Log((1.0f + zeta_squared) * 0.5f) - 2.0f * XMath.Atan(zeta) + PI_HALF;
            }
            else
            {
                value = psi_stable(z);
            }

            return value;
        }

        //Heat flux profile relationship
        float psi_H(float z)
        {
            float value;
            if (L < 0.0)
            {
                float zeta = XMath.Pow(1.0f - 16.0f * z * L_inverse, 0.25f);
                float zeta_squared = zeta * zeta;
                value = 2.0f * XMath.Log((1.0f + zeta_squared) * 0.5f);
            }
            else
            {
                value = psi_stable(z);
            }

            return value;
        }

        float phi_stable(float z)
        {
            return 1.0f + 5.0f * z * L_inverse;
        }

        //Heat flux profile relationship
        float phi_H(float z)
        {
            float value;
            if (L < 0.0)
            {
                float zeta = z * L_inverse;
                value = XMath.Rsqrt(1.0f - 16.0f * zeta);
            }
            else
            {
                value = phi_stable(z);
            }

            return value;
        }

        //Water vapour flux profile relationship
        float phi_W(float z)
        {
            return phi_H(z);
        }

        //Momentum flux profile relationship
        float phi_M(float z)
        {
            float value;
            if (L < 0.0)
            {
                float zeta = z * L_inverse;
                value = XMath.Sqrt(XMath.Rsqrt(1.0f - 16.0f * zeta));
            }
            else
            {
                value = phi_stable(z);
            }

            return value;
        }        

        float get_wind_speed(float z)
        {
            return u_star * VON_KARMAN_INVERSE * (XMath.Log(z / z_0) - psi_M(z));
        }

        float get_potential_temperature(float z)
        {
            return theta_zero + theta_star * VON_KARMAN_INVERSE * (XMath.Log(z / z_0) - psi_H(z));
        }

        //vertical diffusion coefficient
        float get_K_z(float z)
        {
            float a = (1.0f - z * mixing_layer_height_inverse);
            return VON_KARMAN * u_star * z * a * a / phi_H(z);
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

