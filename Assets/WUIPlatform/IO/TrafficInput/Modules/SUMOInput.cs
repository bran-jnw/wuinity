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
    public class SUMOInput
    {
        public string ConfigurationFile;
        public Vector2d UTMoffset;
        public enum DestinationChoiceEnum { Random, EvacGroup };
        public DestinationChoiceEnum DestinationChoice = DestinationChoiceEnum.EvacGroup;

        public static SUMOInput Parse(string[] inputLines, int startIndex)
        {
            int issues = 0;
            SUMOInput newInput = new SUMOInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);
            string input, userInput;

            input = nameof(ConfigurationFile);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                newInput.ConfigurationFile = userInput;
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            input = nameof(UTMoffset);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                string[] data = userInput.Split(',');
                double.TryParse(data[0], out newInput.UTMoffset.x);
                double.TryParse(data[1], out newInput.UTMoffset.y);
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            input = nameof(DestinationChoice);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                switch (userInput)
                {
                    case nameof(DestinationChoiceEnum.EvacGroup):
                        newInput.DestinationChoice = DestinationChoiceEnum.EvacGroup;
                        break;
                    case nameof(DestinationChoiceEnum.Random):
                        newInput.DestinationChoice = DestinationChoiceEnum.Random;
                        break;
                    default:
                        ++issues;
                        WUIEngine.LOG(WUIEngine.LogType.SimError, input + " was not recognized." + WUIEngineInput.pleaseCheckInput);
                        break;
                }
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            return newInput;
        }
    }
}
