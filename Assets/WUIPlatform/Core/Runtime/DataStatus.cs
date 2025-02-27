//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace WUIPlatform
{
    public class DataStatus
    {
        public bool HaveInput;

        public bool MapLoaded;

        private bool _populationLoaded;
        public bool PopulationLoaded { get => _populationLoaded; }

        public bool LcpLoaded, FuelModelsLoaded;

        public bool ResponseCurvesValid;

        public bool CanRunSimulation()
        {
            bool canRun = true;
            if (!MapLoaded)
            {
                canRun = false;
                WUIEngine.LOG(WUIEngine.LogType.SimError, "Map is not loaded.");
            }

            if (WUIEngine.INPUT.Simulation.RunPedestrianModule && !PopulationLoaded)
            {
                canRun = false;
                WUIEngine.LOG(WUIEngine.LogType.SimError, "Population is not loaded but user has requested pedestrian model.");
            }

            if (WUIEngine.INPUT.Simulation.RunFireModule)
            {
                if (!LcpLoaded)
                {
                    canRun = false;
                    WUIEngine.LOG(WUIEngine.LogType.SimError, "No LCP file loaded but fire spread is activated.");
                }
            }

            if (WUIEngine.INPUT.Simulation.RunPedestrianModule)
            {
                if (WUIEngine.RUNTIME_DATA.Evacuation.ResponseCurves == null)
                {
                    canRun = false;
                    WUIEngine.LOG(WUIEngine.LogType.SimError, "No valid response curves have been loaded.");
                }

            }

            return canRun;
        }

        public void Reset()
        {
            //haveInput = false; //can never lose input after getting it once
            MapLoaded = false;
            _populationLoaded = false;
            LcpLoaded = false;
            FuelModelsLoaded = false;
        }

        public void SetPopulation(bool status)
        {
            _populationLoaded = status;
        }
    }
}

