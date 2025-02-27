//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Numerics;
using WUIPlatform.Traffic;

namespace WUIPlatform.IO
{
    [System.Serializable]
    public class TrafficInput
    {
        public enum TrafficModuleChoice { SUMO, MacroTrafficSim }
        public TrafficModuleChoice TrafficModule = TrafficModuleChoice.SUMO;      
        public bool VisibilityAffectsSpeed = false;            

        //module settings
        public SUMOInput SumoInput;
        public MacroTrafficSimInput MacroTrafficSimInput;

        public static TrafficInput Parse(string[] inputLines, int startIndex, Dictionary<string, int> headerLineIndex)
        {
            int issues = 0;
            TrafficInput newInput = new TrafficInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);
            string input, userInput;

            input = nameof(TrafficModule);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                switch (userInput)
                {
                    case nameof(TrafficModuleChoice.SUMO):
                        newInput.TrafficModule = TrafficModuleChoice.SUMO;
                        break;
                    case nameof(TrafficModuleChoice.MacroTrafficSim):
                        newInput.TrafficModule = TrafficModuleChoice.MacroTrafficSim;
                        break;
                    default:
                        WUIEngineInput.CouldNotInterpretInputMessage(input, userInput);
                        break;
                }
            }
            else
            {
                ++issues;
                WUIEngine.LOG(WUIEngine.LogType.SimError, "No traffic module choice was set.");
            }

            input = nameof(VisibilityAffectsSpeed);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                bool.TryParse(userInput, out newInput.VisibilityAffectsSpeed);
            }
            else
            {                
            }

            //load correct module
            if(newInput.TrafficModule == TrafficModuleChoice.SUMO)
            {
                int lineIndex;
                input = nameof(TrafficModuleChoice.SUMO);
                if (headerLineIndex.TryGetValue(input, out lineIndex))
                {
                    WUIEngineInput.ReadingInputMessage(input);
                    newInput.SumoInput = SUMOInput.Parse(inputLines, lineIndex);
                }
                else
                {
                    //critical
                    WUIEngine.LOG(WUIEngine.LogType.SimError, nameof(Simulation) + " header not found." + WUIEngineInput.pleaseCheckInput);
                    return null;
                }
            }
            else if(newInput.TrafficModule == TrafficModuleChoice.MacroTrafficSim)
            {

            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.SimError, "Unknown traffic module has been specified.");
            }


            return newInput;
        }
    }
}
    