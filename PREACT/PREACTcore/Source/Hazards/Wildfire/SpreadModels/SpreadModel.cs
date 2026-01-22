namespace PREACT.Wildfire
{
    public abstract class SpreadModel
    {
        public abstract void CalculateSpreadRate();
        public abstract double GetMaxSpreadRate();
        public abstract double GetSpreadRateInDirection(double directionOfInterest);
        public abstract double GetDirectionOfMaxSpread();
        public abstract double GetFirelineIntensity();
        public abstract void SetWind(double direction, double speed);
        public abstract void SetFuelMoisture(double oneHour, double tenHour, double hundredHour, double liveHerbaceous, double liveWoody, double foliar);
        public abstract bool HasFuelLoad();
    }
}
