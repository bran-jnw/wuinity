//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using PREACT.Math;

namespace PREACT.Dispersion
{
    public abstract class SmokeModule : SimulationModule
    {
        public SmokeModule(Simulation simulation) : base(simulation)
        {

        }
        public abstract int GetCellsX();
        public abstract int GetCellsY();

        /// <summary>
        /// Returns a linearized (x-axis leading) 2D map at ground level (or level of interest if available) of soot density in kg/m3.
        /// </summary>
        /// <returns></returns>
        public abstract float[] GetSootDensity();

        /// <summary>
        /// Returns soot density in kg/m3 at specified simulation position. Could return interpolated or point wise value (depends on module implementation).
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public abstract float GetSootDensityAtPos(Vector2d pos);

        /// <summary>
        /// Returns soot density in kg/m3 at specified WGS84 coordinate. Could return interpolated or point wise value (depends on module implementation).
        /// </summary>
        /// <param name="latLon"></param>
        /// <returns></returns>
        public abstract float GetSootDensityAtCoordinate(Vector2d latLon);
    }

}

