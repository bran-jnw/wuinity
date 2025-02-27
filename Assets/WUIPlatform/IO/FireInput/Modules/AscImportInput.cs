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
    public class AscImportInput
    {
        public string RootFolder;
        public string TimeOfArrivalFile;
        public string RateOfSpreadFile;
        public string SpreadDirectionFile;
        public string FirelineIntensityFile;
        public string WeatherStreamFile;

        public static AscImportInput Parse(string[] inputLines, int startIndex)
        {
            int issues = 0;
            AscImportInput newInput = new AscImportInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);
            string input, userInput;

            input = nameof(RootFolder);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                newInput.RootFolder = userInput;
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            input = nameof(TimeOfArrivalFile);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                newInput.TimeOfArrivalFile = userInput;
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            input = nameof(RateOfSpreadFile);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                newInput.RateOfSpreadFile = userInput;
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            input = nameof(SpreadDirectionFile);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                newInput.SpreadDirectionFile = userInput;
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            input = nameof(FirelineIntensityFile);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                newInput.FirelineIntensityFile = userInput;
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            input = nameof(WeatherStreamFile);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                newInput.WeatherStreamFile = userInput;
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
