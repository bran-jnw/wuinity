namespace PREACT.Fire
{
    public abstract class SpreadModel
    {
        public abstract void DoRun();
        public abstract double GetMaxSpreadRate();
        public abstract double GetSpreadRateInDirection(double direction);
        public abstract double GetMaxSpreadRateDirection();
        public abstract void SetWind(double direction, double speed);
    }
}
