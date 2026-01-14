//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace PREACT.IO
{
    public class PopulationInput
    {
        private PopulationData _data;

        public PopulationData Data { get => _data; }
        public string PopulationFile = string.Empty;
        public Dictionary<string, Evacuation.Demographics> Demographics;

        public PopulationInput()
        {
            _data = new PopulationData();
        }

        public static PopulationInput Parse(string[] inputLines, int startIndex, List<int> demographicsLinesIndices, SimulationInput simulationInput, string rootFolder, out bool success)
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

            newInput.Demographics = Evacuation.Demographics.Parse(inputLines, demographicsLinesIndices, out success);
            if(!success)
            {
                return null;
            }

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

            newInput.Data.LoadAll(simulationInput, newInput, rootFolder, out success);
            return newInput;
        }
    }
}

