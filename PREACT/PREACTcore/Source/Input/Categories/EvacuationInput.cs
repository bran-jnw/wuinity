//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using PREACT.IO;

namespace PREACT.IO
{
    [System.Serializable]
    public class EvacuationInput
    {
        private EvacuationData _data;

        public EvacuationData Data { get => _data; }
        public float EvacuationOrderStart = 0.0f;
        public List<string> EvacuationDestinationFiles = new List<string>();
        public List<string> ResponseCurveFiles = new List<string>();      
        public List<string> EvacuationGroupFiles = new List<string>();        
        public string EvacuationGroupsMapFile = string.Empty;
        public float PaintCellSize = 200f;
        public bool UseTriggerBufferEvacuation = false;
        public string TriggerBufferFile = string.Empty;

        public EvacuationInput(SimulationInput simulationInput)
        {
            _data = new EvacuationData(simulationInput, this);
        }

        public static EvacuationInput Parse(string[] inputLines, int startIndex, SimulationInput simulationInput, EventsInput eventsInput, string rootFolder, out bool success)
        {
            EvacuationInput newInput = new EvacuationInput(simulationInput);
            if (!simulationInput.RunPedestrianModule && !simulationInput.RunTrafficModule)
            {
                success = true;
                return newInput;
            }

            success = false;
            int issues = 0;            
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string nameOfInput, userInput;

            //not critical
            nameOfInput = nameof(EvacuationOrderStart);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                float.TryParse(userInput, out newInput.EvacuationOrderStart);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);            
            }

            //critical
            nameOfInput = nameof(EvacuationDestinationFiles);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                string[] data = userInput.Split(',');
                newInput.EvacuationDestinationFiles.AddRange(data);
                PREACTInput.CheckIfFilesExists(nameOfInput, data, rootFolder, out success);
            }
            else
            {
                success = false;
                PREACTInput.InputNotFoundMessage(nameOfInput, true);
            }
            if(!success)
            {
                return newInput;
            }

            //critical
            nameOfInput = nameof(EvacuationGroupFiles);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                string[] data = userInput.Split(',');
                newInput.EvacuationGroupFiles.AddRange(data);
                PREACTInput.CheckIfFilesExists(nameOfInput, data, rootFolder, out success);
            }
            else
            {
                success = false;
                PREACTInput.InputNotFoundMessage(nameOfInput, true);
            }
            if(!success)
            {
                return newInput;
            }

            //critical sometimes
            nameOfInput = nameof(EvacuationGroupsMapFile);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                newInput.EvacuationGroupsMapFile = userInput;
                PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success);
            }
            else
            {
                success = false;
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }
            if(!success && newInput.EvacuationGroupFiles.Count > 1)
            {
                return newInput;
            }

            //critical
            nameOfInput = nameof(ResponseCurveFiles);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                string[] data = userInput.Split(',');
                newInput.ResponseCurveFiles.AddRange(data);
                PREACTInput.CheckIfFilesExists(nameOfInput, data, rootFolder, out success);
            }
            else
            {
                success = false;
                PREACTInput.InputNotFoundMessage(nameOfInput, true);
            }
            if(!success)
            {
                return newInput;
            }

            nameOfInput = nameof(PaintCellSize);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                float.TryParse(userInput, out newInput.PaintCellSize);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            //not critical
            nameOfInput = nameof(UseTriggerBufferEvacuation);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                bool.TryParse(userInput, out newInput.UseTriggerBufferEvacuation);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            //maybe critical
            if(newInput.UseTriggerBufferEvacuation)
            {
                nameOfInput = nameof(TriggerBufferFile);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    newInput.TriggerBufferFile = userInput;
                    PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success);
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, true);
                }
                if(!success)
                {
                    return newInput;
                }

            }

            newInput._data.LoadAll(rootFolder, out success);
            return newInput;
        }

        
    }
}
