//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace PREACT.Input
{
    [System.Serializable]
    public class PedestrianModuleInput
    {
        private MacroHouseholdSimInput _macroHouseholdSimInput;

        public bool Enabled = false;
        public enum PedestrianModules { None, MacroHouseholdSim, JupedSimSUMO }
        public PedestrianModules Module = PedestrianModules.MacroHouseholdSim;

        //module inputs
        public MacroHouseholdSimInput MacroHouseholdSimInput { get => _macroHouseholdSimInput; }

        public PedestrianModuleInput()
        {
            _macroHouseholdSimInput = new MacroHouseholdSimInput();
        }

        public void Parse(string[] inputLines, int startIndex, Dictionary<string, int> headerLineIndex, out bool success)
        {
            success = false;
            int issues = 0;            
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string nameOfInput, userInput;

            nameOfInput = nameof(Enabled);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                success = bool.TryParse(userInput, out Enabled);
            }
            else
            {
                success = false;
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }
            if (!success || !Enabled)
            {
                return;
            }

            nameOfInput = nameof(Module);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                switch (userInput)
                {
                    case nameof(PedestrianModules.MacroHouseholdSim):
                        Module = PedestrianModules.MacroHouseholdSim;
                        break;
                    case nameof(PedestrianModules.JupedSimSUMO):
                        Module = PedestrianModules.JupedSimSUMO;
                        break;
                    default:
                        ++issues;
                        PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                        break;
                }
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            if(Module == PedestrianModules.MacroHouseholdSim)
            {
                int lineIndex;
                if (headerLineIndex.TryGetValue(nameof(PedestrianModules.MacroHouseholdSim), out lineIndex))
                {
                    _macroHouseholdSimInput = MacroHouseholdSimInput.Parse(inputLines, lineIndex, out success);
                }
                else
                {
                    Engine.Message(null, Engine.LogType.Warning, nameof(PedestrianModules.MacroHouseholdSim) + " input was not found, using defaults.");
                }
            }
            else
            {
                Engine.Message(null, Engine.LogType.Debug, "This should not happen, trying to use non-implemented pedestrian module.");
            }

            success = true;
        }
    }
}
   