//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace PREACT.IO
{
    [System.Serializable]
    public class kPERILInput
    {
        public float MidflameWindspeed = 0f;
        public bool CalculateROSFromBehave = true;
        public string InitialFuelMoistureFile = string.Empty;
        public string OutputName = string.Empty;

        public kPERILInput()
        {
        }

        public static kPERILInput Parse(string[] inputLines, int startIndex, string rootFolder, out bool success)
        {
            kPERILInput newInput = new kPERILInput();
            success = false;
            int issues = 0;            
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string nameOfInput, userInput;

            //critical
            nameOfInput = nameof(MidflameWindspeed);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                issues += float.TryParse(userInput, out newInput.MidflameWindspeed) ? 0 : 1;
                if(issues > 0)
                {
                    PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                }
            }
            else
            {
                ++issues;
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }
            if(issues > 0)
            {
                success = false;
                return newInput;
            }

            //not critical
            nameOfInput = nameof(CalculateROSFromBehave);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                issues += bool.TryParse(userInput, out newInput.CalculateROSFromBehave) ? 0 : 1;
                if (issues > 0)
                {
                    PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                }
            }
            else
            {
                Engine.Message(null, Engine.LogType.Warning, nameOfInput + " was not found, defaulting to " + newInput.CalculateROSFromBehave.ToString() + ".");
            }


            nameOfInput = nameof(InitialFuelMoistureFile);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                newInput.InitialFuelMoistureFile = userInput;
                PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success);
            }
            else
            {
                success = false;
                PREACTInput.InputNotFoundMessage(nameOfInput, true);
            }
            //critical
            if(!success && newInput.CalculateROSFromBehave)
            {
                return newInput;
            }

            //critical
            nameOfInput = nameof(OutputName);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                newInput.OutputName = userInput;
            }
            else
            {
                success = false;
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }
            if (!success)
            {
                return newInput;
            }

            success = true;
            return newInput;
        }

    }
}