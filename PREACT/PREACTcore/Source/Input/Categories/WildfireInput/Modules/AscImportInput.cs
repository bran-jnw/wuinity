//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System;

namespace PREACT.IO
{

    [System.Serializable]
    public class AscImportInput
    {
        public DateTime StartDateTime;
        public string TimeOfArrivalFile = string.Empty;
        public string RateOfSpreadFile = string.Empty;
        public string SpreadDirectionFile = string.Empty;
        public string FirelineIntensityFile = string.Empty;

        public AscImportInput()
        {

        }

        public static AscImportInput Parse(string[] inputLines, int startIndex, string rootFolder, out bool success)
        {
            success = false;
            int issues = 0;            
            AscImportInput newInput = new AscImportInput();
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string nameOfInput, userInput;

            //critical
            nameOfInput = nameof(StartDateTime);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                 success = DateTime.TryParse(userInput, out newInput.StartDateTime);
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

            //critical
            nameOfInput = nameof(TimeOfArrivalFile);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                newInput.TimeOfArrivalFile = userInput;
                PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success);                
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

            //critical only for k-PERIL
            nameOfInput = nameof(RateOfSpreadFile);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                newInput.RateOfSpreadFile = userInput;
                PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success);                
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

            //critical only for k-PERIL
            nameOfInput = nameof(SpreadDirectionFile);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                newInput.SpreadDirectionFile = userInput;
                PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success);                
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

            //not critical
            nameOfInput = nameof(FirelineIntensityFile);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                newInput.FirelineIntensityFile = userInput;
                PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder,out success);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            success = true;
            return newInput;
        }
    }
}
