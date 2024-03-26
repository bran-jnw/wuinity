using ILGPU;

namespace WUInity.Smoke
{
    public class GaussianPuffSim : SmokeModule
    {
        float[] xPositions, yPositions;
        uint puffCount;
        Context context;


        public GaussianPuffSim() 
        {
            context = Context.CreateDefault();
        }

        ~GaussianPuffSim()
        {
            context.Dispose();
        }

        public override void Update(float currentTime, float deltaTime)
        {
            for (int i = 0; i < puffCount; i++)
            {

            }
        }
    }
}

