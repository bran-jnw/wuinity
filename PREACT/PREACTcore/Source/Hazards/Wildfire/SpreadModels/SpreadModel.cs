namespace PREACT.Wildfire
{
    public abstract class SpreadModel
    {
        public abstract void CalculateSpreadRate(WeatherManager weather, TimeManager timeManager);
        public abstract double GetMaxSpreadRate();
        public abstract double GetSpreadRateInDirection(double directionOfInterest);
        public abstract double GetDirectionOfMaxSpread();
        public abstract double GetFirelineIntensity();
        public abstract bool HasFuelLoad();
    }
}
