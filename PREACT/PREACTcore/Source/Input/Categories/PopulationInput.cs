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
        public Dictionary<string, Evacuation.DemographicsInput> Demographics = new Dictionary<string, Evacuation.DemographicsInput>(5);
        public bool CullOutsideGroups = false;

        public PopulationInput()
        {
            _data = new PopulationData();
        }

        public void Parse(string[] inputLines, int startIndex, List<int> demographicsLinesIndices, PedestrianModuleInput pedestrianInput, string rootFolder, out bool success)
        {
            if (!pedestrianInput.Enabled)
            {
                success = true;
                return;
            }

            int issues = 0;            
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string nameOfInput, userInput;

            Evacuation.DemographicsInput.Parse(Demographics, inputLines, demographicsLinesIndices, out success);
            if(!success)
            {
                return;
            }

            nameOfInput = nameof(PopulationFile);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                PopulationFile = userInput;
                PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            nameOfInput = nameof(CullOutsideGroups);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                success = bool.TryParse(userInput, out CullOutsideGroups);
            }
            else
            {
                success = false;
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }
            if(!success)
            {
                CullOutsideGroups = false;
            }

            Data.LoadAll(pedestrianInput, this, rootFolder, out success);
        }
    }
}

