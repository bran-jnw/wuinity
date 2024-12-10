//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using static WUIPlatform.Fire.MathWrap;

namespace WUIPlatform
{
    public static class InterpolationLibrary
    {
        /// <summary>
        /// Only works in one dimension as in 2D it reverts to being linear.
        /// </summary>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        /// <param name="fraction"></param>
        /// <returns></returns>
        /// 
        //http://paulbourke.net/miscellaneous/interpolation/#:~:text=Often%20a%20smoother%20interpolating%20function,smooth%20transition%20between%20adjacent%20segments.&text=Cubic%20interpolation%20is%20the%20simplest,true%20continuity%20between%20the%20segments.
        public static double CosineInterpolate(double y1, double y2, double fraction)
        {
            double frac;
            //range fom 0-1 over half circle
            frac = (1.0 - cos(fraction * PI)) / 2.0;

            //return weighted value between y1 and y2 based on fraction
            return (y1 * (1 - frac) + y2 * frac);
        }
    }
}

