//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using WUIPlatform.Runtime;

namespace WUIPlatform.Population
{
    public abstract class PopulationVisualizer
    {
        protected PopulationData _owner;

        public PopulationVisualizer(PopulationData owner)
        {
            _owner = owner;
        }

        public abstract void SetDataPlane(bool setActive);
        public abstract bool IsDataPlaneActive();
        public abstract object GetPopulationTexture();
        public abstract object GetPopulationMaskTexture();
        public abstract void CreatePopulationMapTexture(PopulationMap data);
        public abstract void CreatePopulationMapMaskTexture(PopulationMap data);
        public abstract void CreateGPWTexture(LocalGPWData data);
        public abstract bool ToggleLocalGPWVisibility();

        //colors from GPW website
        static WUIEngineColor c0 = new WUIEngineColor(190f / 255f, 232f / 255f, 255f / 255f);
        static WUIEngineColor c1 = new WUIEngineColor(1.0f, 241f / 255f, 208f / 255f);
        static WUIEngineColor c2 = new WUIEngineColor(1.0f, 218f / 255f, 165f / 255f);
        static WUIEngineColor c3 = new WUIEngineColor(252f / 255f, 183f / 255f, 82f / 255f);
        static WUIEngineColor c4 = new WUIEngineColor(1.0f, 137f / 255f, 63f / 255f);
        static WUIEngineColor c5 = new WUIEngineColor(238f / 255f, 60f / 255f, 30f / 255f);
        static WUIEngineColor c6 = new WUIEngineColor(191f / 255f, 1f / 255f, 39f / 255f);

        public static WUIEngineColor GetGPWColor(float density)
        {
            WUIEngineColor color;
            if (density < 0.0f)
            {
                color = c0;
            }
            else if (density < 1.0f)
            {
                color = c1;
            }
            else if (density <= 5.0f)
            {
                color = c2;
            }
            else if (density <= 25.0f)
            {
                color = c3;
            }
            else if (density <= 250.0f)
            {
                color = c4;
            }
            else if (density <= 1000.0f)
            {
                color = c5;
            }
            else
            {
                color = c6;
            }
            color.a = 0.5f;
            return color;
        }
    }
}


