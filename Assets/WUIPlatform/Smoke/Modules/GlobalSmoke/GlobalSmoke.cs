//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace WUIPlatform.Smoke
{
    public class GlobalSmoke : SmokeModule
    {
        OpticalDensityRamp _opticalDensityRamp;
        float[] _opticalDensityOutput;

        public GlobalSmoke(string opticalDensityFile)
        {
            _opticalDensityRamp = new OpticalDensityRamp();
            if(!_opticalDensityRamp.LoadOpticalDensityRampFile(opticalDensityFile))
            {
                WUIEngine.LOG(WUIEngine.LogType.SimError, "Failed to initialize GlobalSmoke.");
            }
            _opticalDensityOutput = new float[1];
        }


        public override void Step(float currentTime, float deltaTime)
        {
            _opticalDensityOutput[0] = _opticalDensityRamp.GetOpticalDensity(currentTime);
        }      

        public override bool IsSimulationDone()
        {
            return false;
        }

        public override int GetCellsX()
        {
            return 1;
        }

        public override int GetCellsY()
        {
            return 1;
        }

        public override float[] GetGroundOpticalDensity()
        {
            return _opticalDensityOutput;
        }        

        public override float GetGroundOpticalDensityAtWorldPos(Vector2d pos)
        {
            return _opticalDensityOutput[0];
        }

        public override float GetGroundOpticalDensityAtCoordinate(Vector2d latLon)
        {
            return _opticalDensityOutput[0];
        }

        public override void Stop()
        {

        }
    }
}

