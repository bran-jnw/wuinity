//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using WUIPlatform.Runtime;

namespace WUIPlatform.Visualization
{
    public abstract class FireDataVisualizer
    {
        public enum LcpViewMode { FuelModel, Elevation, Slope, Aspect, TriggerBuffer }

        public Runtime.FireData owner;

        public FireDataVisualizer(FireData owner)
        {
            this.owner = owner;
        }

        public abstract void SetLCPViewMode(LcpViewMode lcpViewMode);
        public abstract void ToggleLCPDataPlane();
        public abstract void SetLCPDataPlane(bool setActive);
        public abstract void CreateTriggerBufferVisuals(float[,] data);
    }
}

