//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace WUIPlatform.IO
{
    [System.Serializable]
    public class PedestrianInput
    {
        public enum PedestrianModuleChoice { MacroHouseholdSim, JupedSimSUMO }
        public PedestrianModuleChoice PedestrianModule = PedestrianModuleChoice.MacroHouseholdSim;

        //module inputs
        public MacroHouseholdSimInput macroHouseholdSimInput;

        public static PedestrianInput Parse(string[] inputLines, int startIndex, Dictionary<string, int> headerLineIndex)
        {
            int issues = 0;
            PedestrianInput newInput = new PedestrianInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);
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
                        WUIEngineInput.CouldNotInterpretInputMessage(input, userInput);
                        break;
                }
            }
            else
            {
                WUIEngineInput.InputNotFoundMessage(input);
            }

            if(newInput.PedestrianModule == PedestrianModuleChoice.MacroHouseholdSim)
            {
                int lineIndex;
                if (headerLineIndex.TryGetValue(nameof(PedestrianModuleChoice.MacroHouseholdSim), out lineIndex))
                {
                    newInput.macroHouseholdSimInput = MacroHouseholdSimInput.Parse(inputLines, lineIndex);
                }
                else
                {
                    WUIEngine.LOG(WUIEngine.LogType.Warning, nameof(PedestrianModuleChoice.MacroHouseholdSim) + " input was not found, using defaults.");
                }
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Debug, "This should not happen, trying to use non-implemented pedestrian module.");
            }

            return newInput;
        }
    }
}
   