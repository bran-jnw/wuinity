//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using PREACT.Runtime;

namespace PREACT.IO
{
    [System.Serializable]
    public class EvacuationInput
    {
        private EvacuationData _data;

        EvacuationData Data { get => _data; }
        public float EvacuationOrderStart = 0.0f;
        public string[] EvacuationDestinationFiles;
        public string[] ResponseCurveFiles;        
        public string[] EvacuationGroupFiles;        
        public string EvacuationGroupsMapFile;
        public float PaintCellSize = 200f;
        public bool UseTriggerBufferEvacuation = false;
        public string TriggerBufferFile;

        public EvacuationInput(SimulationInput simulationInput, EventsInput eventsInput)
        {
            _data = new EvacuationData(simulationInput, this, eventsInput);
        }

        public static EvacuationInput Parse(string[] inputLines, int startIndex, SimulationInput simulationInput, EventsInput eventsInput, string rootFolder, out bool success)
        {
            int issues = 0;
            EvacuationInput newInput = new EvacuationInput(simulationInput, eventsInput);
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string input, userInput;

            input = nameof(EvacuationOrderStart);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                float.TryParse(userInput, out newInput.EvacuationOrderStart);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(input);            
            }

            input = nameof(EvacuationDestinationFiles);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                string[] data = userInput.Split(',');
                newInput.EvacuationDestinationFiles = data;
                PREACTInput.CheckIfFilesExists(input, data, rootFolder, ref issues, out success);
            }
            else
            {
                ++issues;
                PREACTInput.InputNotFoundMessage(input);
            }

            input = nameof(EvacuationGroupFiles);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                string[] data = userInput.Split(',');
                newInput.EvacuationGroupFiles = data;
                PREACTInput.CheckIfFilesExists(input, data, rootFolder, ref issues, out success);
            }
            else
            {
                ++issues;
                PREACTInput.InputNotFoundMessage(input);
            }

            input = nameof(EvacuationGroupsMapFile);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                newInput.EvacuationGroupsMapFile = userInput;
                PREACTInput.CheckIfFileExist(input, userInput, rootFolder, ref issues, out success);
            }
            else
            {
            }

            input = nameof(ResponseCurveFiles);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                string[] data = userInput.Split(',');
                newInput.ResponseCurveFiles = data;
                PREACTInput.CheckIfFilesExists(input, data, rootFolder, ref issues, out success);
            }
            else
            {
                ++issues;
                PREACTInput.InputNotFoundMessage(input);
            }

            input = nameof(PaintCellSize);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                float.TryParse(userInput, out newInput.PaintCellSize);
            }
            else
            {
            }

            input = nameof(UseTriggerBufferEvacuation);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                bool.TryParse(userInput, out newInput.UseTriggerBufferEvacuation);
            }
            else
            {
            }

            input = nameof(TriggerBufferFile);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                newInput.TriggerBufferFile = userInput;
            }
            else
            {
            }

            newInput._data.LoadAll(rootFolder, out success);

            return newInput;
        }

        
    }
}
