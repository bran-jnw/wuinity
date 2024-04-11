using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;
using ILGPU.Runtime.OpenCL;
using System;
using System.Numerics;
using System.Collections.Generic;
using ILGPU.Util;

namespace WUIPlatform.Smoke
{
    public class MixingLayerSmokeSpread : SmokeModule, IDisposable
    {
        MemoryBuffer1D<float, Stride1D.Dense> _densityRead, _densityWrite, _injection;
        List<MemoryBuffer1D<float, Stride1D.Dense>> _allBuffers;
        Context _context;
        Accelerator _accelerator;

        Action<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, GlobalData> _advectKernel;
        int bufferSize;

        private struct GlobalData
        {
            public float deltaTime, windX, windY;
            public int cellsX, cellsY;
            public float cellSizeX, cellSizeY, invertedCellVolume, cellHeight, invertedCellSizeX, invertedCellSizeY, invertedCellSizeXSq, invertedCellSizeYSq, cellArea, cellVolume;
        }
        GlobalData _globalData;

        public MixingLayerSmokeSpread()
        {
            //initiate device to run on
            _context = Context.CreateDefault();
            _accelerator = _context.GetPreferredDevice(false).CreateAccelerator(_context);

            //set up all buffers and data containers
            _globalData = new GlobalData();
            _globalData.cellsX = WUIEngine.SIM.FireModule.GetCellCountX();
            _globalData.cellsY = WUIEngine.SIM.FireModule.GetCellCountY();
            _globalData.cellSizeX = WUIEngine.SIM.FireModule.GetCellSizeX();
            _globalData.cellSizeY = WUIEngine.SIM.FireModule.GetCellSizeY();
            _globalData.invertedCellSizeX = 1f / _globalData.cellSizeX;
            _globalData.invertedCellSizeY = 1f / _globalData.cellSizeY;
            _globalData.cellHeight = WUIEngine.INPUT.Smoke.MixingLayerHeight;
            _globalData.cellVolume = _globalData.cellHeight * _globalData.cellSizeX * _globalData.cellSizeY;
            _globalData.deltaTime = WUIEngine.INPUT.Simulation.DeltaTime;

            _allBuffers = new List<MemoryBuffer1D<float, Stride1D.Dense>>();
            bufferSize = _globalData.cellsX * _globalData.cellsY;

            _densityRead = _accelerator.Allocate1D(new float[bufferSize]);
            _densityRead.MemSetToZero();
            _allBuffers.Add(_densityRead);
            _densityWrite = _accelerator.Allocate1D(new float[bufferSize]);
            _densityWrite.MemSetToZero();
            _allBuffers.Add(_densityWrite);

            _injection = _accelerator.Allocate1D(new float[bufferSize]);
            _injection.MemSetToZero();
            _allBuffers.Add(_injection);

            //compile kernel
            _advectKernel = _accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, GlobalData>(Advect);

            sootOutput = new float[bufferSize];
        }

        ~MixingLayerSmokeSpread()
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

        float[] sootOutput;
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
            float windX = Mathf.Sin(windData.direction * Mathf.Deg2Rad) * windData.speed;
            float windY = Mathf.Cos(windData.direction * Mathf.Deg2Rad) * windData.speed;
            _globalData.windX = windX;
            _globalData.windY = windY;

            //run kernel
            _advectKernel(bufferSize, _densityRead.View, _densityWrite.View, _injection.View, _globalData);
            _accelerator.Synchronize();

            //swap buffers so that they are correct next step
            Swap(_densityRead, _densityWrite);

            _densityRead.CopyToCPU(sootOutput);
        }

        public void Swap(MemoryBuffer1D<float, Stride1D.Dense> read, MemoryBuffer1D<float, Stride1D.Dense> write)
        {
            MemoryBuffer1D<float, Stride1D.Dense> temp = read;
            read = write;
            write = read;
        }

        static void Advect(Index1D i, ArrayView<float> read, ArrayView<float> write, ArrayView<float> injection, GlobalData globalData)
        {
            int x = i % globalData.cellsX;
            int y = i / globalData.cellsY;
            //advection 
            Vector2 pos = new Vector2(x, y);
            Vector2 advectedPos = GetAdvectedPos(pos, globalData);
            float newC = SampleBilinear(advectedPos, read, globalData);

            //injection
            float conc_dot = XMath.Max(0.0f, injection[i] * globalData.invertedCellVolume); // kg * s / m3, kg/s soot injection controlled/taken from firemesh

            //calculate new concentration (kg/m3)
            float c_delta = globalData.deltaTime * conc_dot;
            write[i] = newC + c_delta;
        }

        static Vector2 GetAdvectedPos(Vector2 pos, GlobalData constantData)
        {
            //when only using global wind value
            Vector2 step = constantData.deltaTime * new Vector2(constantData.windX * constantData.invertedCellSizeX, constantData.windY * constantData.invertedCellSizeY);
            pos -= step;
            return pos;
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
            return sootOutput;
        }
    }
}

