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
using System.Numerics;
using System.Runtime.InteropServices;

namespace WUIPlatform.Smoke
{
    public class LagrangianSmokeSim : SmokeModule, IDisposable
    {
        public MemoryBuffer1D<Vector3, Stride1D.Dense> positions;
        Context context;
        Accelerator device;


        public LagrangianSmokeSim() 
        {
            context = Context.CreateDefault();
            device = context.GetPreferredDevice(false).CreateAccelerator(context);
        }

        public override void Step(float currentTime, float deltaTime)
        {

        }

        ~LagrangianSmokeSim()
        {
            Dispose();
        }

        public void Dispose()
        {
            positions.Dispose();
            device.Dispose();
            context.Dispose();
        }

        public override int GetCellsX()
        {
            throw new NotImplementedException();
        }

        public override int GetCellsY()
        {
            throw new NotImplementedException();
        }

        public override bool IsSimulationDone()
        {
            throw new NotImplementedException();
        }

        public override float[] GetGroundSoot()
        {
            throw new NotImplementedException();
        }
    }
}

