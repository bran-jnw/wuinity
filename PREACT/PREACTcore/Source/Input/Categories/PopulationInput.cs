//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using PREACT.IO;

namespace PREACT.IO
{

    [System.Serializable]
    public class PopulationInput
    {
        private PopulationData _data;

        public PopulationData Data { get => _data; }
        public string PopulationFile = string.Empty;
        public int MinHouseholdSize = 1;
        public int MaxHouseholdSize = 5;
        public bool AllowMoreThanOneCar = true;
        public int MaxCars = 2;
        public float MaxCarsProbability = 0.3f;

        public PopulationInput()
        {
            _data = new PopulationData();
        }

        public static PopulationInput Parse(string[] inputLines, int startIndex, SimulationInput simulationInput, string rootFolder, out bool success)
        {
            PopulationInput newInput = new PopulationInput();

            if (!simulationInput.RunPedestrianModule)
            {
                success = true;
                return newInput;
            }

            int issues = 0;            
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string nameOfInput, userInput;

            nameOfInput = nameof(PopulationFile);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                newInput.PopulationFile = userInput;
                PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            nameOfInput = nameof(MinHouseholdSize);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                int.TryParse(userInput, out newInput.MinHouseholdSize);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            nameOfInput = nameof(MaxHouseholdSize);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                int.TryParse(userInput, out newInput.MaxHouseholdSize);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            nameOfInput = nameof(AllowMoreThanOneCar);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                bool.TryParse(userInput, out newInput.AllowMoreThanOneCar);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            nameOfInput = nameof(MaxCars);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                int.TryParse(userInput, out newInput.MaxCars);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            nameOfInput = nameof(MaxCarsProbability);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                float.TryParse(userInput, out newInput.MaxCarsProbability);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            newInput.Data.LoadAll(simulationInput, newInput, rootFolder, out success);
            return newInput;
        }
    }
}

