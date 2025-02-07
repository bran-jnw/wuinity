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
using System.Collections.Generic;

namespace WUIPlatform.Smoke
{
    public class MixingLayerSmoke : SmokeModule, IDisposable
    {
        const int READ = 0;
        const int WRITE = 1;
        MemoryBuffer1D<float, Stride1D.Dense>[] _density = new MemoryBuffer1D<float, Stride1D.Dense>[2];
        MemoryBuffer1D<float, Stride1D.Dense> _injection;
        List<MemoryBuffer1D<float, Stride1D.Dense>> _allBuffers;
        Context _context;
        Accelerator _accelerator;
        Action<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, GlobalData> _advectKernel;
        Action<Index1D, ArrayView<float>, ArrayView<float>, GlobalData, int> _diffuseKernel;
        int bufferSize;
        float[] _sootOutput;

        //calculated from https://www.ready.noaa.gov/READYpgclass.php and from 
        //https://doi.org/10.1016/0004-6981(75)90066-9 which describes relation between K_z and K_y 
        //expand with https://en.wikipedia.org/wiki/Turner_stability_class?
        static readonly float[] eddyDiffusivity = { 260.0f, 215.0f, 125.0f, 125.0f, 39.0f, 10.5f, 3.0f };

        private struct GlobalData
        {
            public float deltaTime, windX, windY, windDirectionX, windDirectionY;
            public int cellsX, cellsY, cellsZ;
            public float cellSizeX, cellSizeY, cellSizeZ, cellSizeXSq, cellSizeYSq, cellSizeZSq, invertedCellVolume, inverseCellSizeX, inverseCellSizeY, inverseCellSizeZ, inverseCellSizeXSq, inverseCellSizeYSq, inverseCellSizeZSq,  cellArea, cellVolume;
            public float eddyDiffusivity;
        }
        GlobalData _globalData;

        public MixingLayerSmoke()
        {
            //initiate device to run on
            _context = Context.CreateDefault();
            int deviceIndex = -1;
            for (int i = 0; i < _context.Devices.Length; i++)
            {                
                //WUIEngine.LOG(WUIEngine.LogType.Log, "ILGPU available accelerator: " + _context.Devices[i].Name);
                if(_context.Devices[i].Name.Contains("NVIDIA"))
                {
                    deviceIndex = i;
                    break;
                }
            }
            if(deviceIndex > -1)
            {
                _accelerator = _context.Devices[deviceIndex].CreateAccelerator(_context);
            }
            else
            {
                _accelerator = _context.GetPreferredDevice(false).CreateAccelerator(_context);
            }           
            WUIEngine.LOG(WUIEngine.LogType.Log, "ILGPU is using accelerator: " + _accelerator.Device.Name);

            //set up all buffers and data containers
            _globalData = new GlobalData();
            _globalData.cellsX = WUIEngine.SIM.FireModule.GetCellCountX();
            _globalData.cellsY = WUIEngine.SIM.FireModule.GetCellCountY();
            int cellsZ = (int)(0.5f + WUIEngine.INPUT.Smoke.MixingLayerHeight / WUIEngine.SIM.FireModule.GetCellSizeX());
            _globalData.cellsZ = cellsZ;            

            //they should be square
            _globalData.cellSizeX = WUIEngine.SIM.FireModule.GetCellSizeX();
            _globalData.cellSizeY = WUIEngine.SIM.FireModule.GetCellSizeY();
            _globalData.cellSizeZ = WUIEngine.SIM.FireModule.GetCellSizeX();


            _globalData.cellSizeXSq = _globalData.cellSizeX * _globalData.cellSizeX;
            _globalData.cellSizeYSq = _globalData.cellSizeY * _globalData.cellSizeY;

            _globalData.inverseCellSizeXSq = 1f / _globalData.cellSizeXSq;
            _globalData.inverseCellSizeYSq = 1f / _globalData.cellSizeYSq;

            _globalData.inverseCellSizeX = 1f / _globalData.cellSizeX;
            _globalData.inverseCellSizeY = 1f / _globalData.cellSizeY;

            _globalData.cellVolume = _globalData.cellSizeX * _globalData.cellSizeY * _globalData.cellSizeZ;
            _globalData.invertedCellVolume = 1f / _globalData.cellVolume;
            _globalData.deltaTime = WUIEngine.INPUT.Simulation.DeltaTime;

            _allBuffers = new List<MemoryBuffer1D<float, Stride1D.Dense>>();
            bufferSize = _globalData.cellsX * _globalData.cellsY * _globalData.cellsZ;

            _density[READ] = _accelerator.Allocate1D(new float[bufferSize]);
            _density[READ].MemSetToZero();
            _allBuffers.Add(_density[READ]);
            _density[WRITE] = _accelerator.Allocate1D(new float[bufferSize]);
            _density[WRITE].MemSetToZero();
            _allBuffers.Add(_density[WRITE]);

            _injection = _accelerator.Allocate1D(new float[bufferSize]);
            _injection.MemSetToZero();
            _allBuffers.Add(_injection);

            //compile advection kernel
            _advectKernel = _accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, GlobalData>(Advect);
            _diffuseKernel = _accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<float>, ArrayView<float>, GlobalData, int>(Diffuse);
            //we need this to send data to WUIEngine and evaluative visibility
            _sootOutput = new float[bufferSize];
        }

        ~MixingLayerSmoke()
        {
            Dispose();
        }

        public void Dispose()
        {
            for (int i = 0; i < _allBuffers.Count; i++)
            {
                _allBuffers[i].Dispose();
            }
            _accelerator.Dispose();
            _context.Dispose();
        }

        bool _lockOutput = false;
        public override void Step(float currentTime, float deltaTime)
        {
            //if fire has been updated we need to update the injection buffer
            bool fireHasUpdated = true;
            if (fireHasUpdated)
            {
                _injection.CopyFromCPU(WUIEngine.SIM.FireModule.GetSootProduction());
            }

            //update wind
            Fire.WindData windData = WUIEngine.SIM.FireModule.GetCurrentWindData();
            _globalData.windDirectionX = -Mathf.Sin(windData.direction * Mathf.Deg2Rad);
            _globalData.windDirectionY = -Mathf.Cos(windData.direction * Mathf.Deg2Rad);
            _globalData.windX = _globalData.windDirectionX * windData.speed;
            _globalData.windY = _globalData.windDirectionY * windData.speed;

            https://www.ready.noaa.gov/READYpgclass.php
            int stability = 4; //0-6 represents stability class A-G
            _globalData.eddyDiffusivity = eddyDiffusivity[stability];            

            //run advection kernel
            _advectKernel(bufferSize, _density[READ].View, _density[WRITE].View, _injection.View, _globalData);            
            Swap(_density);
            //diffusion, implicit jacobian iterations
            for (int j = 0; j < 20; j++)
            {
                _diffuseKernel(bufferSize, _density[READ].View, _density[WRITE].View, _globalData, 1);
                Swap(_density);                
            }
            _accelerator.Synchronize();

            _lockOutput = true;
            _density[READ].CopyToCPU(_sootOutput);
            _lockOutput = false;
        }        

        void Swap(MemoryBuffer1D<float, Stride1D.Dense>[] buffer)
        {
            MemoryBuffer1D<float, Stride1D.Dense> tmp = buffer[READ];
            buffer[READ] = buffer[WRITE];
            buffer[WRITE] = tmp;
        }

        static void Advect(Index1D i, ArrayView<float> read, ArrayView<float> write, ArrayView<float> injection, GlobalData globalData)
        {
            int x = i % globalData.cellsX;
            int y = i / globalData.cellsX;
            //advection 
            Vector2 index = new Vector2(x, y);
            Vector2 advectedPos = GetAdvectedIndex(index, globalData);
            float newC = SampleBilinear(advectedPos, read, globalData);

            //injection
            float conc_dot = XMath.Max(0.0f, injection[i] * globalData.invertedCellVolume); // kg * s / m3, kg/s soot injection controlled/taken from firemesh

            //calculate new concentration (kg/m3)
            float c_delta = globalData.deltaTime * conc_dot;
            write[i] = newC + c_delta;
        }

        static void Diffuse(Index1D i, ArrayView<float> read, ArrayView<float> write, GlobalData globalData, int anisotropic)
        {
            int x = i % globalData.cellsX;
            int y = i / globalData.cellsX;

            float C = read[i];
            //float L = 0, R = 0, D = 0, U = 0;
            float L = C, R = C, D = C, U = C; //never diffuse to outside? creates problem of pulling in soot from boundary
            if (x > 0)
            {
                int idxL = x - 1 + y * globalData.cellsX;
                L = read[idxL];
            }
            if (x < globalData.cellsX - 1)
            {
                int idxR = x + 1 + y * globalData.cellsX;
                R = read[idxR];
            }
            if (y > 0)
            {
                int idxD = x + (y - 1) * globalData.cellsX;
                D = read[idxD];
            }
            if (y < globalData.cellsY - 1)
            {
                int idxU = x + (y + 1) * globalData.cellsX;
                U = read[idxU];
            }

            //isotropic diffusion
            float K = globalData.eddyDiffusivity;
            //dx^2 / v * dt
            float alpha = (globalData.cellSizeX * globalData.cellSizeY) / (K * globalData.deltaTime);
            float rBeta = 4.0f + alpha;
            write[i] = (C * alpha + L + R + D + U) / rBeta;

            if (anisotropic > 0)
            {
                //implicit jacobi
                //https://skill-lync.com/student-projects/solving-2d-heat-conduction-equation-using-various-iterative-solvers
                Vector2 K_y = GetCrosswindDiffusionCoefficient(globalData, K);
                float k1 = K_y.X * globalData.deltaTime * globalData.inverseCellSizeXSq;
                float k2 = K_y.Y * globalData.deltaTime * globalData.inverseCellSizeYSq;
                float term1 = 1f / (1f + 2f * k1 + 2f * k2);
                float term2 = k1 * term1;
                float term3 = k2 * term1;
                float h = L + R;
                float v = D + U;
                write[i] = C * term1 + term2 * h + term3 * v;
            }            
        }

        static Vector2 GetCrosswindDiffusionCoefficient(GlobalData globalData, float K)
        {
            //swap x and y since we want cross-wind, not along wind
            Vector2 K_y = new Vector2(XMath.Abs(globalData.windDirectionY), XMath.Abs(globalData.windDirectionX)); //can't have negative values...
            K_y = new Vector2(K_y.X * K + 0.001f, K_y.Y * K + 0.001f); // to avoid divison by zero

            return K_y;
        }

        static Vector2 GetAdvectedIndex(Vector2 startIndex, GlobalData constantData)
        {
            Vector2 step = constantData.deltaTime * new Vector2(constantData.windX * constantData.inverseCellSizeX, constantData.windY * constantData.inverseCellSizeY);
            startIndex -= step;
            return startIndex;
        }

        static float SampleBilinear(Vector2 advectedPos, ArrayView<float> read, GlobalData globalData)
        {
            //save the down-clamped position and the up-clamped position
            //floor is needed since we might have negative values and casting does not work then properly
            int x = (int)XMath.Floor(advectedPos.X);
            int y = (int)XMath.Floor(advectedPos.Y);
            int xp1 = x + 1;
            int yp1 = y + 1;

            //using remainder as fraction of actual position and integer snapped position
            float fx = advectedPos.X - x;
            float fy = advectedPos.Y - y;

            float c00 = 0f, c10 = 0f, c01 = 0f, c11 = 0f;
            if (x >= 0 && x < globalData.cellsX && y >= 0 && y < globalData.cellsY)
            {
                int idxC = x + y * globalData.cellsX;
                c00 = read[idxC];
            }
            if (xp1 >= 0 && xp1 < globalData.cellsX && y >= 0 && y < globalData.cellsY)
            {
                int idxR = xp1 + y * globalData.cellsX;
                c10 = read[idxR];
            }
            if (x >= 0 && x < globalData.cellsX && yp1 >= 0 && yp1 < globalData.cellsY)
            {
                int idxU = x + yp1 * globalData.cellsX;
                c01 = read[idxU];
            }
            if (xp1 >= 0 && xp1 < globalData.cellsX && yp1 >= 0 && yp1 < globalData.cellsY)
            {
                int idxRU = xp1 + yp1 * globalData.cellsX;
                c11 = read[idxRU];
            }

            float a = c00 * (1.0f - fx) + c10 * fx;
            float b = c01 * (1.0f - fx) + c11 * fx;

            return a * (1.0f - fy) + b * fy;
        }


        public override bool IsSimulationDone()
        {
            return false;
        }

        public override int GetCellsX()
        {
            return _globalData.cellsX;
        }

        public override int GetCellsY()
        {
            return _globalData.cellsY;
        }

        public override float[] GetGroundSoot()
        {
            if(_lockOutput)
            {
                return null;
            }

            return _sootOutput;
        }

        public override void Stop()
        {
            //throw new System.NotImplementedException();
        }
    }
}

