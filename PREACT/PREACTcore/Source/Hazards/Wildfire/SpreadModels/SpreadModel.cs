namespace PREACT.Wildfire
{
    public abstract class SpreadModel
    {
        /// <summary>
        /// Does a run to update the results for the implemented spread model.
        /// </summary>
        /// <param name="weather"></param>
        /// <param name="time"></param>
        public abstract void CalculateSpreadRate(WeatherManager weather, TimeManager time);

        /// <summary>
        /// Returns maximum spread rate in [m/s].
        /// </summary>
        /// <returns></returns>
        public abstract double GetMaxSpreadRate();

        /// <summary>
        /// Returns spread rate in specified direction [m/s].
        /// </summary>
        /// <param name="directionOfInterest">Azimuth angle (related to north)</param>
        /// <returns></returns>
        public abstract double GetSpreadRateInDirection(double directionOfInterest);
        
        /// <summary>
        /// Returns azimuth angle (related to north).
        /// </summary>
        /// <returns></returns>
        public abstract double GetDirectionOfMaxSpread();

        /// <summary>
        /// Returns the fire intensity [kW/m].
        /// </summary>
        /// <returns></returns>
        public abstract double GetFirelineIntensity();
        
        /// <summary>
        /// Returns if a fuel that can burn is present.
        /// </summary>
        /// <returns></returns>
        public abstract bool HasFuelLoad();
    }
}
