//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using PREACT.Math;

namespace PREACT.IO
{  
    [System.Serializable]
    public class SUMOInput
    {
        public enum DestinationChoiceEnum { Random, EvacGroup };
        public enum SmokeSpeedReductionModelEnum { Exponential, Smokanza};

        public string ConfigurationFile;
        public Vector2d UTMoffset;
        public double OutputRasterSize = 25.0;        
        public DestinationChoiceEnum DestinationChoice = DestinationChoiceEnum.EvacGroup;
        public float SmokeAlpha = 0f;
        public float SmokeBeta = 0f;

        public SUMOInput()
        {

        }

        public static SUMOInput Parse(string[] inputLines, int startIndex, string rootFolder, out bool success)
        {
            success = false;
            int issues = 0;
            SUMOInput newInput = new SUMOInput();
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string input, userInput;

            input = nameof(ConfigurationFile);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                newInput.ConfigurationFile = userInput;
                PREACTInput.CheckIfFileExist(input, userInput, rootFolder, out success);
            }
            else
            {
                ++issues;
                PREACTInput.InputNotFoundMessage(input, true);
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
                PREACTInput.InputNotFoundMessage(input, true);
            }

            input = nameof(OutputRasterSize);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                double.TryParse(userInput, out newInput.OutputRasterSize);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(input);
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
                        Engine.Message(null, Engine.LogType.SimulationError, input + " was not recognized." + PREACTInput.pleaseCheckInput);
                        break;
                }
            }
            else
            {
                ++issues;
                PREACTInput.InputNotFoundMessage(input);
            }

            input = nameof(SmokeAlpha);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                float.TryParse(userInput, out newInput.SmokeAlpha);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(input);
            }

            input = nameof(SmokeBeta);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                float.TryParse(userInput, out newInput.SmokeBeta);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(input);
            }

            if (issues == 0)
            {
                success = true;
            }
            return newInput;
        }
    }
}
