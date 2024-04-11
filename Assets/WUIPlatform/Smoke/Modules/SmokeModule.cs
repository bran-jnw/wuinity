namespace WUIPlatform.Smoke
{
    public abstract class SmokeModule : SimulationModule
    {
        public abstract int GetCellsX();
        public abstract int GetCellsY();
        public abstract float[] GetGroundSoot();
    }

}

