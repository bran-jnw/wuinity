//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using PREACT.Evacuation;

namespace PREACT.IO
{
    [System.Serializable]
    public class EvacuationInput
    {
        private EvacuationData _data;

        public EvacuationData Data { get => _data; }
        public float EvacuationOrderStart = 0.0f;
        public Dictionary<string, EvacuationDestinationInput> EvacuationDestinationInputs;
        public Dictionary<string, ResponseCurve> ResponseCurves;        
        public Dictionary<string, EvacuationGroupInput> EvacuationGroupInputs;
        public bool UseTriggerBufferEvacuation = false;
        public string TriggerBufferFile = string.Empty;

        public EvacuationInput(SimulationInput simulationInput)
        {
            _data = new EvacuationData(simulationInput, this);
        }

        public static EvacuationInput Parse(string[] inputLines, int startIndex, SimulationInput simulationInput, EventsInput eventsInput, List<int> destinationLineIndices, List<int> responseCurveLineIndices, List<int> evacuationGroupLineIndices, string rootFolder, out bool success)
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

            //critical
            newInput.EvacuationDestinationInputs = EvacuationDestinationInput.Parse(inputLines, destinationLineIndices, out success);
            if (!success)
            {
                return newInput;
            }

            //critical
            newInput.ResponseCurves = ResponseCurve.Parse(inputLines, responseCurveLineIndices, out success);
            if (!success)
            {
                return newInput;
            }

            //critical, must be done after response curves and destinations
            newInput.EvacuationGroupInputs = EvacuationGroupInput.Parse(inputLines, simulationInput, evacuationGroupLineIndices, newInput.EvacuationDestinationInputs, newInput.ResponseCurves, rootFolder, out success);
            if (!success)
            {
                return newInput;
            }

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
