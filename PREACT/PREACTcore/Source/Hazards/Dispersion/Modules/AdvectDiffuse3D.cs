//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
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
using System.Collections.Generic;
using PREACT.Math;

namespace PREACT.Smoke
{
    public class AdvectDiffuse3D : SmokeModule, IDisposable
    {
        const int READ = 0;
        const int WRITE = 1;
        MemoryBuffer1D<float, Stride1D.Dense>[] _speciesDensity = new MemoryBuffer1D<float, Stride1D.Dense>[2];
        MemoryBuffer1D<float, Stride1D.Dense> _injection;
        MemoryBuffer1D<float, Stride1D.Dense> _heightMap;
        MemoryBuffer1D<Vector3, Stride1D.Dense> _wind;
        List<MemoryBuffer1D<float, Stride1D.Dense>> _floatBuffers = new List<MemoryBuffer1D<float, Stride1D.Dense>>();
        Context _context;
        Accelerator _accelerator;
        Action<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, ArrayView<float>, GlobalData> _advectDiffuseKernel;
        int _3DbufferSize;
        float[] _sootOutput;

        //calculated from https://www.ready.noaa.gov/READYpgclass.php and from 
        //https://doi.org/10.1016/0004-6981(75)90066-9 which describes relation between K_z and K_y 
        //expand with https://en.wikipedia.org/wiki/Turner_stability_class?
        static readonly float[] KzClasses = { 260.0f, 215.0f, 125.0f, 125.0f, 39.0f, 10.5f, 3.0f };

        private struct GlobalData
        {
            public const float VON_KARMAN = 0.41f;
            public const float VON_KARMAN_INVERSE = 2.43902439f;
            public const float PI = 3.1415927410125732421875f;
            public const float PI_HALF = 1.57079637050628662109375f;
            public const float GRAVITY = 9.81f;
            public const float GRAVITY_INVERSE = 0.1019367992f;

            //these are set/calculated on CPU
            public float dt, windX, windY, windZ, windDirectionX, windDirectionY;
            public int xDim, yDim, zDim, xyDim;
            public float cellSizeX, cellSizeY, cellSizeZ, cellSizeXSq, cellSizeYSq, cellSizeZSq, invertedCellVolume, inverseCellSizeX, inverseCellSizeY, inverseCellSizeZ, inverseCellSizeXSq, inverseCellSizeYSq, inverseCellSizeZSq, cellArea, cellVolume;
            public float Kxy, Kz; //eddy diffusivity
            public float dtdx;
            public float mixingLayerHeight, mixingLayerHeightInverse;            
            
            public float L;// = -350.0; //Obukhov length
            public float L_inverse;// = -0.0028571429; //Obukhov length inversed
            public float u_star; //friction velocity
            public float u_star_squared; //friction velocity squared
            public float z_0;//= 1.0; //Davenport-Wierenga roughness length classification
            public float theta_zero; //ground level potential temperature
            public float theta_star; //scaling potential temperature

            public int loopCount;
        }
        GlobalData _globalData;


        public AdvectDiffuse3D(Simulation simulation) : base(simulation)
        {
            //initiate device to run on
            _context = Context.CreateDefault();// Context.Create(b => b.Default().EnableAlgorithms());
            int deviceIndex = -1;
            for (int i = 0; i < _context.Devices.Length; i++)
            {
                //WUIEngine.LOG(WUIEngine.LogType.Log, "ILGPU available accelerator: " + _context.Devices[i].Name);
                if (_context.Devices[i].Name.Contains("NVIDIA"))
                {
                    deviceIndex = i;
                    break;
                }
            }
            if (deviceIndex > -1)
            {
                _accelerator = _context.Devices[deviceIndex].CreateAccelerator(_context);
            }
            else
            {
                _accelerator = _context.GetPreferredDevice(false).CreateAccelerator(_context);
            }
            Engine.Message(null, Engine.LogType.Log, "ILGPU is using accelerator: " + _accelerator.Device.Name);

            //set up all buffers and data containers
            _globalData = new GlobalData();

            _globalData.xDim = _simulation.FireModule.GetCellCountX();
            _globalData.yDim = _simulation.FireModule.GetCellCountY();
            _globalData.zDim = (int)(0.5f + _simulation.Input.Smoke.AdvectDiffuseInput.MixingLayerHeight / 30.0f);
            _globalData.xyDim = _globalData.xDim * _globalData.yDim;

            Engine.Message(_simulation, Engine.LogType.Log, "AdvectDiffuse3D cell count [x,y,x]: " + _globalData.xDim + ", " + _globalData.yDim + ", " + _globalData.zDim);

            _globalData.cellSizeX = _simulation.FireModule.GetCellSizeX();
            _globalData.cellSizeY = _simulation.FireModule.GetCellSizeY();
            _globalData.cellSizeZ = _globalData.cellSizeX;

            Engine.Message(_simulation, Engine.LogType.Debug, "Cell sizes: " + _globalData.cellSizeX + ", " + _globalData.cellSizeY + ", " + _globalData.cellSizeZ);

            _globalData.inverseCellSizeX = 1f / _globalData.cellSizeX;
            _globalData.inverseCellSizeY = 1f / _globalData.cellSizeY;
            _globalData.inverseCellSizeZ = 1f / _globalData.cellSizeZ;

            _globalData.cellSizeXSq = _globalData.cellSizeX * _globalData.cellSizeX;
            _globalData.cellSizeYSq = _globalData.cellSizeY * _globalData.cellSizeY;
            _globalData.cellSizeZSq = _globalData.cellSizeZ * _globalData.cellSizeZ;

            _globalData.inverseCellSizeXSq = 1f / _globalData.cellSizeXSq;
            _globalData.inverseCellSizeYSq = 1f / _globalData.cellSizeYSq;
            _globalData.inverseCellSizeZSq = 1f / _globalData.cellSizeZSq;            

            _globalData.cellVolume = _globalData.cellSizeX * _globalData.cellSizeY * _globalData.cellSizeZ;
            _globalData.invertedCellVolume = 1f / _globalData.cellVolume;
            _globalData.dt = _simulation.Input.Simulation.DeltaTime;

            _globalData.mixingLayerHeight = _simulation.Input.Smoke.AdvectDiffuseInput.MixingLayerHeight;

            //3D data
            _3DbufferSize = _globalData.xDim * _globalData.yDim * _globalData.zDim;
            _speciesDensity[READ] = _accelerator.Allocate1D(new float[_3DbufferSize]);
            _speciesDensity[READ].MemSetToZero();
            _floatBuffers.Add(_speciesDensity[READ]);
            _speciesDensity[WRITE] = _accelerator.Allocate1D(new float[_3DbufferSize]);
            _speciesDensity[WRITE].MemSetToZero();
            _floatBuffers.Add(_speciesDensity[WRITE]);

            //2D data
            int _2DbufferSize = _globalData.xDim * _globalData.yDim;
            _injection = _accelerator.Allocate1D(new float[_2DbufferSize]);
            _injection.MemSetToZero();
            _floatBuffers.Add(_injection);

            _heightMap = _accelerator.Allocate1D(new float[_2DbufferSize]);
            _heightMap.MemSetToZero();
            _floatBuffers.Add(_heightMap);

            _wind = _accelerator.Allocate1D(new Vector3[_2DbufferSize]);
            _wind.MemSetToZero();

            //we need this to send data to WUIEngine and evaluative visibility
            _sootOutput = new float[_2DbufferSize];

            //compile kernel
            _advectDiffuseKernel = _accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, ArrayView<float>, GlobalData>(AdvectDiffuseFTU);            
        }

        ~AdvectDiffuse3D()
        {
            Dispose();
        }

        public void Dispose()
        {
            for (int i = 0; i < _floatBuffers.Count; i++)
            {
                _floatBuffers[i].Dispose();
            }
            _wind.Dispose();
            _accelerator.Dispose();
            _context.Dispose();
        }

        bool _lockOutput = false;
        public override void Step(float currentTime, float deltaTime)
        {
            _injection.CopyFromCPU(_simulation.FireModule.GetSootProduction());

            //update wind
            Fire.WindData windData = _simulation.Input.Fire.Data.WindInput.GetWindDataAtTime(currentTime);
            _globalData.windDirectionX = -Mathf.Sin(windData.direction * Mathf.Deg2Rad);
            _globalData.windDirectionY = -Mathf.Cos(windData.direction * Mathf.Deg2Rad);
            _globalData.windX = _globalData.windDirectionX * windData.speed;
            _globalData.windY = _globalData.windDirectionY * windData.speed;
            _globalData.windZ = 0f;

            https://www.ready.noaa.gov/READYpgclass.php
            int stability = 4; //0-6 represents stability class A-G
            _globalData.Kxy = KzClasses[stability];
            _globalData.Kz = KzClasses[stability];

            int subSteps = 5;
            _globalData.dt = deltaTime / subSteps;
            _globalData.dtdx = _globalData.dt / _globalData.cellSizeX;
            //run advection kernel
            for (int i = 0; i < subSteps; ++i)
            {
                _globalData.loopCount = i;
                _advectDiffuseKernel(_3DbufferSize, _speciesDensity[READ].View, _speciesDensity[WRITE].View, _heightMap.View, _injection.View, _globalData);
                _accelerator.Synchronize();
                Swap(_speciesDensity);
            }
            
            //transfer to CPU, not ideal as we render again on the GPU, but OK for now
            _lockOutput = true;
            _injection.CopyToCPU(_sootOutput); //injection works as input and output to save on memory
            _lockOutput = false;
        }

        void Swap(MemoryBuffer1D<float, Stride1D.Dense>[] buffer)
        {
            MemoryBuffer1D<float, Stride1D.Dense> tmp = buffer[READ];
            buffer[READ] = buffer[WRITE];
            buffer[WRITE] = tmp;
        }

        /// <summary>
        /// First order forward time, upwind first order advection, second order central difference diffusion. This one use dx = dy = dz meaning dA is same on all sides.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="read"></param>
        /// <param name="write"></param>
        /// <param name="sourceTerm"></param>
        /// <param name="globalData"></param>
        static void AdvectDiffuseFTU(Index1D i, ArrayView<float> read, ArrayView<float> write, ArrayView<float> heightMap, ArrayView<float> sourceTerm, GlobalData globalData)
        {
            int x = i % globalData.xDim;
            int y = (i / globalData.xDim) % globalData.yDim;
            int z = i / globalData.xyDim;

            if (x >= globalData.xDim || y >= globalData.yDim || z >= globalData.zDim)
            {
                return;
            }

            float C = read[i];
            float xNeg = C, xPos = C, yNeg = C, yPos = C, zNeg = C, zPos = C; //zero gradient diffusion
            if (x > 0)
            {
                xNeg = read[i - 1];
            }
            if (x < globalData.xDim - 1)
            {
                xPos = read[i + 1];
            }
            if (y > 0)
            {
                yNeg = read[i - globalData.xDim];
            }
            if (y < globalData.yDim - 1)
            {
                yPos = read[i + globalData.xDim];
            }
            if (z > 0)
            {
                zNeg = read[i - globalData.xyDim];
            }
            if (z < globalData.zDim - 1)
            {
                zPos = read[i + globalData.xyDim];
            }

            int twoDindex = i - z * globalData.xyDim;
            float heighAboveTerrain = z * globalData.cellSizeZ - heightMap[twoDindex];
            float windSpeed = get_wind_speed(heighAboveTerrain, globalData);

            //calculated from https://www.ready.noaa.gov/READYpgclass.php and from "Point source atmospheric diffusion model with variable wind and diffusivity profiles" which describes relation between K_z and K_y
            float Kz = 100f;// get_K_z(heighAboveTerrain, globalData);
            float Kxy = 5 * Kz;

            //TODO: need to use wind speed at cell faces
            //x
            float diffusion = Kxy * globalData.inverseCellSizeXSq * (xPos - 2 * C + xNeg);
            if(x == 0)
            {
                xNeg = 0;
            }
            if (x == globalData.xDim - 1)
            {
                xPos = 0;
            }
            float upwind = globalData.windX > 0 ? (C - xNeg) : (C - xPos);
            float advection = XMath.Abs(globalData.windX) * upwind * globalData.inverseCellSizeX;
            float xFlux = -advection + diffusion;

            //y
            diffusion = Kxy * globalData.inverseCellSizeYSq * (yPos - 2 * C + yNeg);
            if (y == 0)
            {
                yNeg = 0;
            }
            if (y == globalData.yDim - 1)
            {
                yPos = 0;
            }
            upwind = globalData.windY > 0 ? (C - yNeg) : (C - yPos);
            advection = XMath.Abs(globalData.windY) * upwind * globalData.inverseCellSizeY;
            float yFlux = -advection + diffusion;

            //z
            diffusion = Kz * globalData.inverseCellSizeZSq * (zPos - 2 * C + zNeg);
            //z == 0 not needed as zNeg = C by default meaning advection is 0 as desired (solid)
            if (z == globalData.zDim - 1)
            {
                zPos = 0;
            }
            upwind = globalData.windZ > 0 ? (C - zNeg) : (C - zPos);
            advection = XMath.Abs(globalData.windZ) * upwind * globalData.inverseCellSizeZ;
            float zFlux = -advection + diffusion;

            //injection
            float injection = 0;
            if(z == 4 && globalData.loopCount == 0)
            {
                injection = XMath.Max(0.0f, sourceTerm[twoDindex]) * globalData.invertedCellVolume; // injection should come in kg
            }

            float flux = xFlux + yFlux + zFlux;
            write[i] = C + injection + globalData.dt * flux;

            //density at ground/first cell
            if (z == 7)
            {
                sourceTerm[twoDindex] = write[i]; //save 
            }
        }


        static float get_wind_speed(float z, GlobalData data)
        {
            return data.u_star * GlobalData.VON_KARMAN_INVERSE * (XMath.Log(z / data.z_0) - psi_M(z, data));
        }

        //vertical diffusion coefficient
        static float get_K_z(float z, GlobalData data)
        {
            float a = (1.0f - z * data.mixingLayerHeightInverse);
            return GlobalData.VON_KARMAN * data.u_star * z * a * a / phi_H(z, data);
        }

        static float get_potential_temperature(float z, GlobalData data)
        {
            return data.theta_zero + data.theta_star * GlobalData.VON_KARMAN_INVERSE * (XMath.Log(z / data.z_0) - psi_H(z, data));
        }

        static float psi_stable(float z, GlobalData data)
        {
            return -5.0f * z * data.L_inverse;
        }

        //Momentum flux profile relationship
        static float psi_M(float z, GlobalData data)
        {
            float value;
            if (data.L < 0.0)
            {
                float zeta = z * data.L_inverse;
                float zeta_squared = zeta * zeta;
                value = 2.0f * XMath.Log((1.0f + zeta) * 0.5f) + XMath.Log((1.0f + zeta_squared) * 0.5f) - 2.0f * XMath.Atan(zeta) + GlobalData.PI_HALF;
            }
            else
            {
                value = psi_stable(z, data);
            }

            return value;
        }

        //Heat flux profile relationship
        static float psi_H(float z, GlobalData data)
        {
            float value;
            if (data.L < 0.0)
            {
                float zeta = XMath.Pow(1.0f - 16.0f * z * data.L_inverse, 0.25f);
                float zeta_squared = zeta * zeta;
                value = 2.0f * XMath.Log((1.0f + zeta_squared) * 0.5f);
            }
            else
            {
                value = psi_stable(z, data);
            }

            return value;
        }

        static float phi_stable(float z, GlobalData data)
        {
            return 1.0f + 5.0f * z * data.L_inverse;
        }

        //Heat flux profile relationship
        static float phi_H(float z, GlobalData data)
        {
            float value;
            if (data.L < 0.0)
            {
                float zeta = z * data.L_inverse;
                value = XMath.Rsqrt(1.0f - 16.0f * zeta);
            }
            else
            {
                value = phi_stable(z, data);
            }

            return value;
        }

        //Water vapour flux profile relationship
        static float phi_W(float z, GlobalData data)
        {
            return phi_H(z, data);
        }

        //Momentum flux profile relationship
        static float phi_M(float z, GlobalData data)
        {
            float value;
            if (data.L < 0.0)
            {
                float zeta = z * data.L_inverse;
                value = XMath.Sqrt(XMath.Rsqrt(1.0f - 16.0f * zeta));
            }
            else
            {
                value = phi_stable(z, data);
            }

            return value;
        }

        /// <summary>
        /// Forward time, upwind first order. This uses compressed z-direction, complicates calculation of cell face areas, see e.g. https://mpas-dev.github.io/atmosphere/atmosphere.html
        /// </summary>
        /// <param name="i"></param>
        /// <param name="read"></param>
        /// <param name="write"></param>
        /// <param name="injection"></param>
        /// <param name="globalData"></param>
        static void AdvectDiffuseFTU_compressed(Index1D i, ArrayView<float> read, ArrayView<float> write, ArrayView<float> heightMap, ArrayView<float> injection, GlobalData globalData, int anisotropic)
        {
            int x = i % globalData.xDim;
            int y = i / globalData.xDim;
            int z = i / globalData.xyDim;

            float C = read[i];
            float xNeg = 0, xPos = 0, yNeg = 0, yPos = 0, zNeg = 0, zPos = 0;
            float zHeightXNeg = heightMap[i];
            float zHeightXPos = heightMap[i];
            float zHeightYNeg = heightMap[i];
            float zHeightYPos = heightMap[i];
            //float L = C, R = C, D = C, U = C; //never diffuse to outside? creates problem of pulling in soot from boundary
            if (x > 0)
            {
                xNeg = read[i - 1];
                zHeightXNeg = 0.5f * (zHeightXNeg + heightMap[i - 1]);
            }
            if (x < globalData.xDim - 1)
            {
                xPos = read[i + 1];
                zHeightXPos = 0.5f * (zHeightXPos + heightMap[i + 1]);
            }
            if (y > 0)
            {
                yNeg = read[i - globalData.xDim];
                zHeightYNeg = 0.5f * (zHeightYNeg + heightMap[i - globalData.xDim]);
            }
            if (y < globalData.yDim - 1)
            {
                yPos = read[i + globalData.xDim];
                zHeightYPos = 0.5f * (zHeightYPos + heightMap[i + globalData.xDim]);
            }
            if (z > 0)
            {
                zNeg = read[i - globalData.xyDim];
            }
            if (z < globalData.zDim - 1)
            {
                zPos = read[i + globalData.xyDim];
            }

            float dzXNeg = (globalData.mixingLayerHeight - zHeightXNeg) / globalData.zDim;
            float dzXPos = (globalData.mixingLayerHeight - zHeightXPos) / globalData.zDim;
            float dzYNeg = (globalData.mixingLayerHeight - zHeightYNeg) / globalData.zDim;
            float dzYPos = (globalData.mixingLayerHeight - zHeightYPos) / globalData.zDim;

            float dAxNeg = dzXNeg * globalData.cellSizeX;
            float dAxPos = dzXPos * globalData.cellSizeX;
            float dAyNeg = dzYNeg * globalData.cellSizeY;
            float dAyPos = dzYPos * globalData.cellSizeY;
        }

        public override bool IsSimulationDone()
        {
            return false;
        }

        public override int GetCellsX()
        {
            return _globalData.xDim;
        }

        public override int GetCellsY()
        {
            return _globalData.yDim;
        }

        public override float[] GetExtinctionCoefficientDensity()
        {
            if (_lockOutput)
            {
                return null;
            }

            return _sootOutput;
        }

        public override float GetGroundExtinctionCoefficientAtWorldPos(Vector2d pos)
        {
            throw new System.NotImplementedException();
        }

        public override float GetGroundExtinctionCoefficientAtCoordinate(Vector2d latLon)
        {
            throw new System.NotImplementedException();
        }

        public override void Stop()
        {
            Dispose();
        }
    }
}

