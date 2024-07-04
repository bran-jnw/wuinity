//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace WUIPlatform.Population
{
    public abstract class PopulationVisualizer
    {
        protected PopulationManager owner;

        public PopulationVisualizer(PopulationManager owner)
        {
            this.owner = owner;
        }

        public abstract void SetDataPlane(bool setActive);
        public abstract bool IsDataPlaneActive();
        public abstract object GetPopulationTexture();
        public abstract void CreateTexture();
        public abstract void CreateGPWTexture();
        public abstract bool ToggleLocalGPWVisibility();
    }
}


