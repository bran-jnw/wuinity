//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace WUIPlatform.Fire.Behave
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

