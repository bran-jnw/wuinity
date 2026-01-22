//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using PREACT.Wildfire;

namespace PREACT.Visualization
{
    public abstract class FireDomainVisualizer
    {
        public enum LcpViewMode { FuelModel, Elevation, Slope, Aspect, TriggerBuffer }

        //LCP
        public abstract void SetAndDisplayLCP(LandscapeData lcpData, LcpViewMode lcpViewMode = LcpViewMode.FuelModel);
        public abstract void SetLCPViewMode(LcpViewMode lcpViewMode);
        public abstract void DisplayTriggerBuffer(float[,] data);        
        public abstract void SetVisibility(bool visible);
        public abstract bool ToggleVisibility();
        public abstract bool IsDataPlaneActive();

        //textures
    }
}

