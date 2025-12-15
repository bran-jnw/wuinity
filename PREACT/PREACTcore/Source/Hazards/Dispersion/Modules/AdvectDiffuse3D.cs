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
using ILGPU.Algorithms.Random;

namespace PREACT.Smoke
{
    public class AdvectDiffuse3D : SmokeModule, IDisposable
    {
        const int READ = 0;
        const int WRITE = 1;
        MemoryBuffer1D<float, Stride1D.Dense>[] _speciesDensity = new MemoryBuffer1D<float, Stride1D.Dense>[2];
        MemoryBuffer1D<float, Stride1D.Dense> _cpuInjection;
        MemoryBuffer1D<uint, Stride1D.Dense> _groundIndex;
        MemoryBuffer1D<float, Stride1D.Dense> _result;
        MemoryBuffer1D<Vector3, Stride1D.Dense> _wind;
        List<MemoryBuffer> _allBuffers = new List<MemoryBuffer>();
        Context _context;
        Accelerator _accelerator;
        Action<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<uint>, ArrayView<float>, GlobalData> _advectDiffuseKernel;
        Action<Index1D, ArrayView<float>, ArrayView<uint>, ArrayView<float>, GlobalData, int, int> _injectionKernel;
        int _3DbufferSize;
        int _2DbufferSize;
        float[] _sootOutput;
        System.Random _cpuRandom;
        RNG<XorShift64Star> _gpuRandom;

        int _fireCellsX;
        int _fireCellsY;

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
            public float mixingLayerHeight, mixingLayerHeightInverse;            
            
            public float L;// = -350.0; //Obukhov length
            public float L_inverse;// = -0.0028571429; //Obukhov length inversed
            public float u_star; //friction velocity
            public float u_star_squared; //friction velocity squared
            public float z_0;//= 1.0; //Davenport-Wierenga roughness length classification
            public float theta_zero; //ground level potential temperature
            public float theta_star; //scaling potential temperature

            public int fireSmokeCellRatio;
            public int sampleIndex;
        }
        GlobalData _globalData;


        public AdvectDiffuse3D(Simulation simulation) : base(simulation)
        {
            try
            {
                Initialize();
            }
            catch (Exception e)
            {
                Dispose();
                Engine.Message(_simulation, Engine.LogType.SimulationError, e.Message);
            }
        }

        private void Initialize()
        {
            //initiate device to run on
            _context = Context.CreateDefault();// Context.Create(b => b.Default().EnableAlgorithms());
            _context.ClearCache(ClearCacheMode.Everything);
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
            _globalData.mixingLayerHeight = _simulation.Input.Smoke.AdvectDiffuseInput.MixingLayerHeight;
            _globalData.fireSmokeCellRatio = 2;
            float zCellSize = 20f;

            //cell size stuff
            _globalData.cellSizeX = _simulation.FireModule.GetCellSizeX() * _globalData.fireSmokeCellRatio;
            _globalData.cellSizeY = _simulation.FireModule.GetCellSizeY() * _globalData.fireSmokeCellRatio;
            _globalData.cellSizeZ = zCellSize;
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

            //cell count stuff
            _fireCellsX = _simulation.FireModule.GetCellCountX();
            _fireCellsY = _simulation.FireModule.GetCellCountY();
            _globalData.xDim = _fireCellsX / _globalData.fireSmokeCellRatio + _fireCellsX % _globalData.fireSmokeCellRatio;
            _globalData.yDim = _fireCellsY / _globalData.fireSmokeCellRatio + _fireCellsY % _globalData.fireSmokeCellRatio;
            _globalData.xyDim = _globalData.xDim * _globalData.yDim;
            //determine height of domain
            Vector2d elevationMinMax = _simulation.Input.Fire.Data.LCPData.GetElevationMinMax();
            float domainHeight = _simulation.Input.Smoke.AdvectDiffuseInput.MixingLayerHeight + (float)elevationMinMax.y - (float)elevationMinMax.x;
            _globalData.zDim = (int)(0.5f + domainHeight * _globalData.inverseCellSizeZ);

            //buffer sizes
            _3DbufferSize = _globalData.xDim * _globalData.yDim * _globalData.zDim;
            _2DbufferSize = _globalData.xDim * _globalData.yDim;
            Engine.Message(_simulation, Engine.LogType.Log, "AdvectDiffuse3D cell count [x,y,z, total]: " + _globalData.xDim + ", " + _globalData.yDim + ", " + _globalData.zDim + ", " + _3DbufferSize);

            //3D data            
            _speciesDensity[READ] = _accelerator.Allocate1D(new float[_3DbufferSize]);
            _speciesDensity[READ].MemSetToZero();
            _allBuffers.Add(_speciesDensity[READ]);

            _speciesDensity[WRITE] = _accelerator.Allocate1D(new float[_3DbufferSize]);
            _speciesDensity[WRITE].MemSetToZero();
            _allBuffers.Add(_speciesDensity[WRITE]);

            //2D data            
            _cpuInjection = _accelerator.Allocate1D(new float[_fireCellsX * _fireCellsY]);
            _cpuInjection.MemSetToZero();
            _allBuffers.Add(_cpuInjection);

            //down sample heightmap if needed
            _groundIndex = _accelerator.Allocate1D(new uint[_2DbufferSize]);
            uint[] heightMap = new uint[_2DbufferSize];
            if (_globalData.fireSmokeCellRatio != 1)
            {
                Fire.LandscapeData l = _simulation.Input.Fire.Data.LCPData;
                for (int y = 0; y < _globalData.yDim; ++y)
                {
                    for (int x = 0; x < _globalData.xDim; ++x)
                    {
                        int index = x + y * _globalData.xDim;
                        float elevation = (float)(l.GetElevationLocalPos((x + 0.5f) * _globalData.cellSizeX, (y + 0.5f) * _globalData.cellSizeY) - elevationMinMax.x);
                        heightMap[index] = (uint)(0.5f + elevation * _globalData.inverseCellSizeZ);
                    }
                }
            }
            else
            {
                float[] elevation = _simulation.Input.Fire.Data.LCPData.Get1DElevation();
                for (int i = 0; i < _2DbufferSize; ++i)
                {
                    heightMap[i] = (uint)(0.5f + elevation[i] * _globalData.inverseCellSizeZ);
                }                
            }
            _groundIndex.CopyFromCPU(heightMap); //build height map
            _allBuffers.Add(_groundIndex);

            _wind = _accelerator.Allocate1D(new Vector3[_2DbufferSize]);
            _wind.MemSetToZero();
            _allBuffers.Add(_wind);

            //we need this to send data to WUIEngine and evaluative visibility
            _result = _accelerator.Allocate1D(new float[_2DbufferSize]);
            _result.MemSetToZero();
            _allBuffers.Add(_result);
            _sootOutput = new float[_2DbufferSize];

            //compile kernels
            _advectDiffuseKernel = _accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<uint>, ArrayView<float>, GlobalData>(AdvectDiffuseFTUW);
            _injectionKernel = _accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<float>, ArrayView<uint>, ArrayView<float>, GlobalData, int, int>(Inject);

            _cpuRandom = new System.Random();
            _gpuRandom = RNG.Create<XorShift64Star>(_accelerator, _cpuRandom);
        }

        ~AdvectDiffuse3D()
        {
            Dispose();
        }

        public void Dispose()
        {
            for (int i = 0; i < _allBuffers.Count; i++)
            {
                if (_allBuffers[i] != null)
                {
                    _allBuffers[i].Dispose();
                }                
            }
            if(_gpuRandom != null)
            {
                _gpuRandom.Dispose();
            }
            if(_accelerator != null)
            {
                _accelerator.Dispose();
            }
            if(_context != null)
            {
                _context.Dispose();
            }                      
        }

        bool _lockOutput = false;
        public override void Step(float currentTime, float deltaTime)
        {      
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

            //inject soot
            _cpuInjection.CopyFromCPU(_simulation.FireModule.GetSootProduction());
            _injectionKernel(_fireCellsX * _fireCellsY, _cpuInjection.View, _groundIndex.View, _speciesDensity[READ].View, _globalData, _fireCellsX, _fireCellsY);
            _accelerator.Synchronize();

            //run advection kernel
            for (int i = 0; i < subSteps; ++i)
            {
                _advectDiffuseKernel(_3DbufferSize, _speciesDensity[READ].View, _speciesDensity[WRITE].View, _groundIndex.View, _result.View, _globalData);
                _accelerator.Synchronize();
                Swap(_speciesDensity);
            }
            
            //transfer to CPU, not ideal as we render again on the GPU, but OK for now
            _lockOutput = true;
            _result.CopyToCPU(_sootOutput); //injection works as input and output to save on memory
            _lockOutput = false;
        }

        void Swap(MemoryBuffer1D<float, Stride1D.Dense>[] buffer)
        {
            MemoryBuffer1D<float, Stride1D.Dense> tmp = buffer[READ];
            buffer[READ] = buffer[WRITE];
            buffer[WRITE] = tmp;
        }

        static void Inject(Index1D i, ArrayView<float> sourceTerm, ArrayView<uint> groundIndex, ArrayView<float> density, GlobalData globalData, int fireXDim, int fireYDim)
        {
            int fireX = i % fireXDim;
            int fireY = i / fireXDim;

            if (fireX > fireXDim - 1 || fireY > fireYDim - 1)
            {
                return;
            }

            float injection = sourceTerm[i];
            int smokeX = fireX / globalData.fireSmokeCellRatio;
            int smokeY = fireY / globalData.fireSmokeCellRatio;
            int smokeIndex2D = smokeX + smokeY * globalData.xDim;
            float injectionHeight = 200f; //TODO: calculate this due to lofting
            int injectionHeightIndex = (int)(injectionHeight * globalData.inverseCellSizeZ) + (int)groundIndex[smokeIndex2D];
            int injectionIndex3D = smokeIndex2D + injectionHeightIndex * globalData.xyDim;
            density[injectionIndex3D] += XMath.Max(0.0f, injection) * globalData.invertedCellVolume; // injection should come in kg
        }

        /// <summary>
        /// First order forward time, upwind first order advection, second order central difference diffusion. This one use dx = dy = dz meaning dA is same on all sides.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="read"></param>
        /// <param name="write"></param>
        /// <param name="sourceTerm"></param>
        /// <param name="globalData"></param>
        static void AdvectDiffuseFTUW(Index1D i, ArrayView<float> read, ArrayView<float> write, ArrayView<uint> groundIndex, ArrayView<float> result, GlobalData globalData)
        {
            int x = i % globalData.xDim;
            int y = (i / globalData.xDim) % globalData.yDim;
            int z = i / globalData.xyDim;
            int index2D = i - z * globalData.xyDim;

            //due to gpu warp size some cores will be outside, also check to see if we are inside terrain
            if (x > globalData.xDim - 1 || y > globalData.yDim - 1 || z > globalData.zDim - 1 || z < groundIndex[index2D])
            {
                return;
            }

            float C = read[i];
            float xNeg = C, xPos = C, yNeg = C, yPos = C, zNeg = C, zPos = C; //Von Neumann, zero gradient for diffusion on boundaries and solids
            float xNegAdv = 0, xPosAdv = 0, yNegAdv = 0, yPosAdv = 0, zNegAdv = C, zPosAdv = 0; //Dirichlet, so zero outside domain, zNegAdv is C as we also want zero gradient "pulling" from the ground
            if (x > 0)
            {
                xNegAdv = C;
                if (groundIndex[index2D - 1] <= z)
                {
                    xNeg = read[i - 1];
                    xNegAdv = xNeg;
                }
            }
            if (x < globalData.xDim - 1)
            {                
                xPosAdv = C;
                if (groundIndex[index2D + 1] <= z)
                {
                    xPos = read[i + 1];
                    xPosAdv = xPos;
                }
            }
            if (y > 0)
            {
                yNegAdv = C;
                if (groundIndex[index2D - globalData.xDim] <= z)
                {
                    yNeg = read[i - globalData.xDim];
                    yNegAdv = yNeg;
                }   
            }
            if (y < globalData.yDim - 1)
            {
                yPosAdv = C;
                if (groundIndex[index2D + globalData.xDim] <= z)
                {
                    yPos = read[i + globalData.xDim];
                    yPosAdv = yPos;
                }                
            }
            //no need to check boundaries in z-direction as heightmap is only 2D
            if (z > groundIndex[index2D])
            {
                zNeg = read[i - globalData.xyDim];
                zNegAdv = zNeg;
            }
            if (z < globalData.zDim - 1)
            {
                zPos = read[i + globalData.xyDim];
                zPosAdv = zPos;
            }
            
            float heighAboveTerrain = (z - groundIndex[index2D]) * globalData.cellSizeZ;
            float windSpeed = get_wind_speed(heighAboveTerrain, globalData);

            //calculated from https://www.ready.noaa.gov/READYpgclass.php and from "Point source atmospheric diffusion model with variable wind and diffusivity profiles" which describes relation between K_z and K_y
            float Kz = 100f;// get_K_z(heighAboveTerrain, globalData);
            float Kxy = 5 * Kz;

            //TODO: want to use wind speed at cell faces
            //x
            float areaToVolume = globalData.cellSizeY * globalData.cellSizeZ * globalData.invertedCellVolume;
            float diffusion = Kxy * areaToVolume * areaToVolume * (xPos - 2 * C + xNeg);
            float upwind = globalData.windX > 0 ? (C - xNegAdv) : (xPosAdv - C);
            float advection = globalData.windX * upwind * areaToVolume;
            float xFlux = -advection + diffusion;

            //y
            areaToVolume = globalData.cellSizeX * globalData.cellSizeZ * globalData.invertedCellVolume;
            diffusion = Kxy * areaToVolume * areaToVolume * (yPos - 2 * C + yNeg);
            upwind = globalData.windY > 0 ? (C - yNegAdv) : (yPosAdv - C);
            advection = globalData.windY * upwind * areaToVolume;
            float yFlux = -advection + diffusion;

            //z
            areaToVolume = globalData.cellSizeX * globalData.cellSizeY * globalData.invertedCellVolume;
            diffusion = Kz * areaToVolume * areaToVolume * (zPos - 2 * C + zNeg);
            upwind = globalData.windZ > 0 ? (C - zNegAdv) : (zPosAdv - C);
            advection = globalData.windZ * upwind * areaToVolume;
            float zFlux = -advection + diffusion;                       

            float flux = xFlux + yFlux + zFlux;
            write[i] = C + globalData.dt * flux;

            //density at ground/first cell
            if (z == globalData.sampleIndex)
            {
                result[index2D] = write[i]; //save 
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

        public override float[] GetSootDensity()
        {
            if (_lockOutput)
            {
                return null;
            }

            return _sootOutput;
        }

        public override float GetSootDensityAtPos(Vector2d pos)
        {
            throw new System.NotImplementedException();
        }

        public override float GetSootDensityAtCoordinate(Vector2d latLon)
        {
            throw new System.NotImplementedException();
        }

        public override void Stop()
        {
            Dispose();
        }

        public void IncreaseOutputHeight()
        {
            _globalData.sampleIndex = Mathf.Min(_globalData.sampleIndex + 1, _globalData.zDim - 1);
        }
        public void DecreaseOutputHeight()
        {
            _globalData.sampleIndex = Mathf.Max(_globalData.sampleIndex - 1, 0);
        }
    }
}

