//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using PREACT.Math;

namespace PREACT.Wildfire
{
    public abstract class WildfireModule : SimulationModule
    {
        protected double _internalDeltaTime;

        protected double _currentBurnArea;
        public double CurrentBurnArea { get => _currentBurnArea; }

        public WildfireModule(Simulation simulation) : base(simulation)
        {

        }

        /// <summary>
        /// The fire module might be able to take longer time steps compared to other modules, so this information is needed if only doing fire simulation.
        /// </summary>
        /// <returns></returns>
        public abstract double GetInternalDeltaTime();

        /// <summary>
        /// Get the maximum rate of spread (any direction, so azimuth is also needed to back calculate eliipse)
        /// </summary>
        public abstract float[,] GetMaxROS();
        /// <summary>
        /// Get the spread direction of the maximum rate of spread, 0 degrees is North and then clockwise
        /// </summary>
        public abstract float[,] GetMaxROSAzimuth();

        public abstract int GetCellCountX();
        public abstract int GetCellCountY();
        public abstract float GetCellSizeX();
        public abstract float GetCellSizeY();
        public abstract float[] GetFireLineIntensityData();
        public abstract float[] GetFuelModelNumberData();
        public abstract float[] GetSootProduction();
        public abstract int GetActiveCellCount();
        public abstract List<Vector2int> GetIgnitedFireCells();
        public abstract void ConsumeIgnitedFireCells();
        public abstract bool Ignited();

        public abstract void GetOffsetAndSize(out Vector2d offset, out Vector2d size);

        /// <summary>
        /// Returns state of cell on mesh based on simulation position. Returns dead if outside of mesh.
        /// </summary>
        /// <param name="simulationPos"></param>
        /// <returns></returns>
        public abstract FireCellState GetFireCellState(Vector2d simulationPos);

        public abstract Vector2int SimulationPosToCellIndex(Vector2d simulationPos, out bool inside);
    }
}

