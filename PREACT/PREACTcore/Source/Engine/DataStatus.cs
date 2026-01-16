//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using PREACT.IO;

namespace PREACT
{
    public class DataStatus
    {
        public bool HaveInput;

        private bool _populationLoaded;
        public bool PopulationLoaded { get => _populationLoaded; }

        public bool LcpLoaded, FuelModelsLoaded;

        public bool ResponseCurvesValid;

        public bool CanRunSimulation(PREACTInput input)
        {
            bool canRun = true;

            if (input.PedestrianModule.Enabled && !PopulationLoaded)
            {
                canRun = false;
                Engine.Message(null, Engine.LogType.SimulationError, "Population is not loaded but user has requested pedestrian model.");
            }

            if (input.WildfireModule.Enabled)
            {
                if (!LcpLoaded && input.WildfireModule.Module != WildfireModuleInput.WildfireModules.AscImport)
                {
                    canRun = false;
                    Engine.Message(null, Engine.LogType.SimulationError, "No LCP file loaded but fire spread is activated.");
                }
            }

            if (input.PedestrianModule.Enabled)
            {
                if (input.Evacuation.ResponseCurves == null)
                {
                    canRun = false;
                    Engine.Message(null, Engine.LogType.SimulationError, "No valid response curves have been loaded.");
                }

            }

            return canRun;
        }

        public void Reset()
        {
            //haveInput = false; //can never lose input after getting it once
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

