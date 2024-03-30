using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;

namespace WUInity.Smoke
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

        public override bool SimulationDone()
        {
            throw new NotImplementedException();
        }

        public override void Step(float currentTime, float deltaTime)
        {
            
        }
    }
}

