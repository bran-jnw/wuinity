namespace WUInity.Fire
{
    public static class WindSpeedUtility
    {
        //bran-jnw: made this class static since there seems to be no use of the actial member variables
        /*double windSpeedAtMidflame_;
        double windSpeedAtTenMeters_;
        double windSpeedAtTwentyFeet_;*/

        /*public WindSpeedUtility()
        {
            windSpeedAtTenMeters_ = 0;
            windSpeedAtTwentyFeet_ = 0;
            windSpeedAtMidflame_ = 0;
        }*/

        public static double windSpeedAtMidflame(double windSpeedAtTwentyFeet, double windAdjustmentFactor)
        {
            // Calculate results
            double windSpeedAtMidflame_ = windSpeedAtTwentyFeet * windAdjustmentFactor;
            return windSpeedAtMidflame_;
        }

        public static double windSpeedAtTwentyFeetFromTenMeter(double windSpeedAtTenMeters)
        {
            // Calculate results
            double windSpeedAt20Ft_ = windSpeedAtTenMeters / 1.15;
            return windSpeedAt20Ft_;
        }
    }
}

