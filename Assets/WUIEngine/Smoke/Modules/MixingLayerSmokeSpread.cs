using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;
using ILGPU.Runtime.OpenCL;
using System;
using System.Numerics;
using System.Collections.Generic;
using ILGPU.Util;

namespace WUIEngine.Smoke
{
    public class MixingLayerSmokeSpread : SmokeModule, IDisposable
    {
        MemoryBuffer1D<float, Stride1D.Dense> _densityRead, _densityWrite, _injection;
        List<MemoryBuffer1D<float, Stride1D.Dense>> _allBuffers;
        Context _context;
        Accelerator _accelerator;

        Action<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, ConstantData> _advectKernel;
        int bufferSize;

        private struct ConstantData
        {
            public float _deltaTime, _windX, _windY;
            public int _cellsX, _cellsY;
            public float _invertedCellVolume, _cellHeight, _invertedCellSizeX, _invertedCellSizeY, _invertedCellSizeXSq, _invertedCellSizeYSq, _cellArea, _cellVolume;
        }
        ConstantData _constantData;

        public MixingLayerSmokeSpread()
        {
            _context = Context.CreateDefault();
            _accelerator = _context.GetPreferredDevice(false).CreateAccelerator(_context);

            _allBuffers = new List<MemoryBuffer1D<float, Stride1D.Dense>>();

            _constantData = new ConstantData();
            _constantData._cellsX = Engine.SIM.FireModule.GetCellCountX();
            _constantData._cellsY = Engine.SIM.FireModule.GetCellCountY();

            bufferSize = _constantData._cellsX * _constantData._cellsY;

            _densityRead = _accelerator.Allocate1D(new float[bufferSize]);
            _densityRead.MemSetToZero();
            _allBuffers.Add(_densityRead);
            _densityWrite = _accelerator.Allocate1D(new float[bufferSize]);
            _densityWrite.MemSetToZero();
            _allBuffers.Add(_densityWrite);

            _injection = _accelerator.Allocate1D(new float[bufferSize]);
            _injection.MemSetToZero();
            _allBuffers.Add(_injection);

            _advectKernel = _accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, ConstantData>(Advect);
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

        public override void Step(float currentTime, float deltaTime)
        {
            //if fire has been updated we need to update the injection buffer
            bool fireHasUpdated = true;
            if (fireHasUpdated)
            {
                //_injection = _accelerator (Engine.SIM.FireModule.GetSootProduction());
            }

            _advectKernel(bufferSize, _densityRead.View, _densityWrite.View, _injection.View, _constantData);
            _accelerator.Synchronize();

            Swap(_densityRead, _densityWrite);            
        }

        public void Swap(MemoryBuffer1D<float, Stride1D.Dense> read, MemoryBuffer1D<float, Stride1D.Dense> write)
        {
            MemoryBuffer1D<float, Stride1D.Dense> temp = read;
            read = write;
            write = read;
        }

        static void Advect(Index1D i, ArrayView<float> read, ArrayView<float> write, ArrayView<float> injection, ConstantData constants)
        {
            int x = i % constants._cellsX;
            int y = i / constants._cellsY;
            //advection 
            Vector2 pos = new Vector2(x, y);
            Vector2 advectedPos = GetAdvectedPos(pos, constants);
            float newC = SampleBilinear(advectedPos, read, constants);

            //injection
            float conc_dot = XMath.Max(0.0f, injection[i] * constants._invertedCellVolume); // kg * s / m3, kg/s soot injection controlled/taken from firemesh

            //calculate new concentration (kg/m3)
            float c_delta = constants._deltaTime * conc_dot;
            write[i] = newC + c_delta;
        }

        static Vector2 GetAdvectedPos(Vector2 pos, ConstantData constantData)
        {
            //when only using global wind value
            Vector2 step = constantData._deltaTime * new Vector2(constantData._windX * constantData._invertedCellSizeX, constantData._windY * constantData._invertedCellSizeY);
            pos -= step;
            return pos;
        }

        static float SampleBilinear(Vector2 advectedPos, ArrayView<float> read, ConstantData constants)
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
            if (x >= 0 && x < constants._cellsX && y >= 0 && y < constants._cellsY)
            {
                int idxC = x + y * constants._cellsX;
                c00 = read[idxC];
            }
            if (xp1 >= 0 && xp1 < constants._cellsX && y >= 0 && y < constants._cellsY)
            {
                int idxR = xp1 + y * constants._cellsX;
                c10 = read[idxR];
            }
            if (x >= 0 && x < constants._cellsX && yp1 >= 0 && yp1 < constants._cellsY)
            {
                int idxU = x + yp1 * constants._cellsX;
                c01 = read[idxU];
            }
            if (xp1 >= 0 && xp1 < constants._cellsX && yp1 >= 0 && yp1 < constants._cellsY)
            {
                int idxRU = xp1 + yp1 * constants._cellsX;
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
    }
}

