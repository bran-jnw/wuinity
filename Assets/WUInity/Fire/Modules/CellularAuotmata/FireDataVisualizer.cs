using WUInity.Runtime;

namespace WUInity.Visualization
{
    public abstract class FireDataVisualizer
    {
        public enum LcpViewMode { FuelModel, Elevation, Slope, Aspect }

        public Runtime.FireData owner;

        public FireDataVisualizer(FireData owner)
        {
            this.owner = owner;
        }

        public abstract void SetLCPViewMode(LcpViewMode lcpViewMode);
        public abstract void ToggleLCPDataPlane();
        public abstract void SetLCPDataPlane(bool setActive);
    }
}

