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
        public string HouseholdsFile;
        public int minHouseholdSize = 1;
        public int maxHouseholdSize = 5;
        public bool allowMoreThanOneCar = true;
        public int maxCars = 2;
        public float maxCarsChance = 0.3f;

        const string householdsFileIn = "householdsFile";
        const string minHouseholdSizeIn = "minHouseholdSize";
        const string maxHouseholdSizeIn = "maxHouseholdSize";
        const string allowMoreThanOneCarIn = "allowMoreThanOneCar";
        const string maxCarsIn = "maxCars";
        const string maxCarsChanceIn = "maxCarsChance";

        public static PopulationInput Parse(string[] inputLines, int startIndex)
        {
            int issues = 0;
            PopulationInput newInput = new PopulationInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);

            string temp;
            if (inputToParse.TryGetValue(householdsFileIn, out temp))
            {
                newInput.HouseholdsFile = temp;
            }
            else
            {
                ++issues;
            }

            if (inputToParse.TryGetValue(minHouseholdSizeIn, out temp))
            {
                int.TryParse(temp, out newInput.minHouseholdSize);
            }
            else
            {
                ++issues;
            }

            if (inputToParse.TryGetValue(maxHouseholdSizeIn, out temp))
            {
                int.TryParse(temp, out newInput.maxHouseholdSize);
            }
            else
            {
                ++issues;
            }

            if (inputToParse.TryGetValue(allowMoreThanOneCarIn, out temp))
            {
                bool.TryParse(temp, out newInput.allowMoreThanOneCar);
            }
            else
            {
                ++issues;
            }

            if (inputToParse.TryGetValue(maxCarsIn, out temp))
            {
                int.TryParse(temp, out newInput.maxCars);
            }
            else
            {
                ++issues;
            }

            if (inputToParse.TryGetValue(maxCarsChanceIn, out temp))
            {
                float.TryParse(temp, out newInput.maxCarsChance);
            }
            else
            {
                ++issues;
            }

            return newInput;
        }
    }
}

