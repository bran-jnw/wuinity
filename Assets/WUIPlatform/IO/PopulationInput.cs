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
    public class PopulationInput
    {
        public string PopulationFile;
        public int MinHouseholdSize = 1;
        public int MaxHouseholdSize = 5;
        public bool AllowMoreThanOneCar = true;
        public int MaxCars = 2;
        public float MaxCarsProbability = 0.3f;

        public static PopulationInput Parse(string[] inputLines, int startIndex)
        {
            int issues = 0;
            PopulationInput newInput = new PopulationInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);
            string input, userInput;

            input = nameof(PopulationFile);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                newInput.PopulationFile = userInput;
            }
            else
            {
                ++issues;
            }

            input = nameof(MinHouseholdSize);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                int.TryParse(userInput, out newInput.MinHouseholdSize);
            }
            else
            {
                ++issues;
            }

            input = nameof(MaxHouseholdSize);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                int.TryParse(userInput, out newInput.MaxHouseholdSize);
            }
            else
            {
                ++issues;
            }

            input = nameof(AllowMoreThanOneCar);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                bool.TryParse(userInput, out newInput.AllowMoreThanOneCar);
            }
            else
            {
                ++issues;
            }

            input = nameof(MaxCars);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                int.TryParse(userInput, out newInput.MaxCars);
            }
            else
            {
                ++issues;
            }

            input = nameof(MaxCarsProbability);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                float.TryParse(userInput, out newInput.MaxCarsProbability);
            }
            else
            {
                ++issues;
            }

            return newInput;
        }
    }
}

