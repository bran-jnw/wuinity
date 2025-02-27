//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using WUIPlatform.IO;

namespace WUIPlatform.Runtime
{
    public class SmokeData
    {
        Traffic.OpticalDensityRamp _opticalDensityRamp;

        //private GlobalSmokeData globalSmokeData;

        public void LoadAll()
        {
            if(!WUIEngine.INPUT.Simulation.RunSmokeModule)
            {
                return;
            }

            if(WUIEngine.INPUT.Smoke.SmokeModule == SmokeInput.SmokeModuleChoice.GlobalSmoke)
            {
                LoadOpticalDensityFile(WUIEngine.INPUT.Smoke.GlobalSmokeInput.OpticalDensityFile, false);
            }            
        }

        private bool LoadOpticalDensityFile(string path, bool updateInputFile)
        {
            bool success = false;

            _opticalDensityRamp = new Traffic.OpticalDensityRamp();
            success = _opticalDensityRamp.LoadOpticalDensityRampFile(path);
            WUIEngine.DATA_STATUS.OpticalDensityLoaded = success;

            if (success && updateInputFile)
            {
                WUIEngineInput.SaveInput();
            }

            return success;
        }

        public float GetOpticalDensity(Vector2d worldPos, float currentTime)
        {
            if(WUIEngine.INPUT.Smoke.SmokeModule == SmokeInput.SmokeModuleChoice.GlobalSmoke)
            {
                _opticalDensityRamp.GetOpticalDensity(currentTime);
            }

            return 0f;
        }
    }

    /*public class GlobalSmokeData
    {
        Vector2[] data;

        public float GetGlobalDensity(float time)
        {
            return 0f;
        }
    }*/
}