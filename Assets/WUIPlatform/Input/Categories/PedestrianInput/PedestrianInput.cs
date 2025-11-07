//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace PREACT.IO
{
    [System.Serializable]
    public class PedestrianInput
    {
        private MacroHouseholdSimInput _macroHouseholdSimInput;

        public enum PedestrianModuleChoice { MacroHouseholdSim, JupedSimSUMO }
        public PedestrianModuleChoice PedestrianModule = PedestrianModuleChoice.MacroHouseholdSim;

        //module inputs
        public MacroHouseholdSimInput MacroHouseholdSimInput { get => _macroHouseholdSimInput; }

        public PedestrianInput()
        {
            _macroHouseholdSimInput = new MacroHouseholdSimInput();
        }

        public static PedestrianInput Parse(string[] inputLines, int startIndex, Dictionary<string, int> headerLineIndex, SimulationInput simulationInput, out bool success)
        {
            PedestrianInput newInput = new PedestrianInput();
            if (!simulationInput.RunPedestrianModule)
            {
                success = true;
                return newInput;
            }

            success = false;
            int issues = 0;            
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string input, userInput;

            

            input = nameof(PedestrianModule);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                switch (userInput)
                {
                    case nameof(PedestrianModuleChoice.MacroHouseholdSim):
                        newInput.PedestrianModule = PedestrianModuleChoice.MacroHouseholdSim;
                        break;
                    case nameof(PedestrianModuleChoice.JupedSimSUMO):
                        newInput.PedestrianModule = PedestrianModuleChoice.JupedSimSUMO;
                        break;
                    default:
                        ++issues;
                        PREACTInput.CouldNotInterpretInputMessage(input, userInput);
                        break;
                }
            }
            else
            {
                PREACTInput.InputNotFoundMessage(input);
            }

            if(newInput.PedestrianModule == PedestrianModuleChoice.MacroHouseholdSim)
            {
                int lineIndex;
                if (headerLineIndex.TryGetValue(nameof(PedestrianModuleChoice.MacroHouseholdSim), out lineIndex))
                {
                    newInput._macroHouseholdSimInput = MacroHouseholdSimInput.Parse(inputLines, lineIndex, out success);
                }
                else
                {
                    Engine.MESSAGE(null, Engine.LogType.Warning, nameof(PedestrianModuleChoice.MacroHouseholdSim) + " input was not found, using defaults.");
                }
            }
            else
            {
                Engine.MESSAGE(null, Engine.LogType.Debug, "This should not happen, trying to use non-implemented pedestrian module.");
            }

            success = true;
            return newInput;
        }
    }
}
   