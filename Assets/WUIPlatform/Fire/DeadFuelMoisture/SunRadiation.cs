//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using static WUIPlatform.Fire.CDT_SolarRadiationLibrary;

namespace WUIPlatform.Fire
{
    public static class SunRadiation
    {

        /// <summary>
        /// Adapted from Farsite FMC-FE2.cpp, returns W/m2. Added latitude and longitide as that was not included (lat got it through reference, long was set to 0 for some reason, equator?)
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="date"></param>
        /// <param name="hour"></param>
        /// <param name="cloud"></param>
        /// <param name="elev"></param>
        /// <param name="slope"></param>
        /// <param name="aspect"></param>
        /// <param name="cover"></param>
        /// <returns></returns>
        public static double SimpleRadiation(double latitude, double longitude, long date, double hour, long cloud, long elev, long slope, long aspect, long cover)
        {

            // calculates solar radiation (W/m2) using Collin Bevins model
            double cloudTransmittance = 1.0 - (double)cloud / 100.0;
            double Rad, jdate;// latitude = (double)a_CI->GetLatitude();
            double atmTransparency = 0.7;
            double canopyTransmittance = 1.0 - (double)cover / 100.0;
            long i, month = 0, pdays = 0, days = 0;

            for (i = 1; i <= 13; i++)
            {
                switch (i)
                {
                    case 1: days = 0; break;            // cumulative days to begin of month
                    case 2: days = 31; pdays = 0; break;           // except ignores leapyear, but who cares anyway,
                    case 3: days = 59; pdays = 31; break;
                    case 4: days = 90; pdays = 59; break;
                    case 5: days = 120; pdays = 90; break;
                    case 6: days = 151; pdays = 120; break;
                    case 7: days = 181; pdays = 151; break;
                    case 8: days = 212; pdays = 181; break;
                    case 9: days = 243; pdays = 212; break;
                    case 10: days = 273; pdays = 243; break;
                    case 11: days = 304; pdays = 273; break;
                    case 12: days = 334; pdays = 304; break;
                    default: days = 367; pdays = 334; break;
                }
                if (date < days)
                {
                    month = i - 1;
                    days = date - pdays;
                    break;
                }
            }

            jdate = CDT_JulianDate(2000, (int)month, (int)days, (int)hour / 100, 0, 0, 0);
            Rad = CDT_SolarRadiation(jdate, longitude, latitude, 0.0, (double)slope, (double)aspect, (double)elev / 3.2808, atmTransparency, cloudTransmittance, canopyTransmittance);

            return Rad *= 1370.0;       //W/m2
        }
    }
}

