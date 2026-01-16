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

        public EvacuationInput()
        {
            _data = new EvacuationData();
        }

        public void Parse(string[] inputLines, int startIndex, EventsInput eventsInput, PopulationInput population, PedestrianModuleInput pedestrianInput, TrafficModuleInput trafficInput, List<int> destinationLineIndices, List<int> responseCurveLineIndices, List<int> evacuationGroupLineIndices, string rootFolder, out bool success)
        {
            if (!pedestrianInput.Enabled && !trafficInput.Enabled)
            {
                success = true;
                return;
            }

            success = false;
            int issues = 0;            
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string nameOfInput, userInput;

            //critical
            EvacuationDestinationInputs = EvacuationDestinationInput.Parse(inputLines, destinationLineIndices, out success);
            if (!success)
            {
                return;
            }

            //critical
            ResponseCurves = ResponseCurve.Parse(inputLines, responseCurveLineIndices, out success);
            if (!success)
            {
                return;
            }

            //critical, must be done after response curves and destinations
            EvacuationGroupInputs = EvacuationGroupInput.Parse(inputLines, evacuationGroupLineIndices, EvacuationDestinationInputs, ResponseCurves, population, rootFolder, out success);
            if (!success)
            {
                return;
            }

            //not critical
            nameOfInput = nameof(EvacuationOrderStart);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                float.TryParse(userInput, out EvacuationOrderStart);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);            
            }

            //not critical
            nameOfInput = nameof(UseTriggerBufferEvacuation);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                bool.TryParse(userInput, out UseTriggerBufferEvacuation);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            //maybe critical
            if(UseTriggerBufferEvacuation)
            {
                nameOfInput = nameof(TriggerBufferFile);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    TriggerBufferFile = userInput;
                    PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success);
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, true);
                }
                if(!success)
                {
                    return;
                }
            }

            _data.LoadAll(rootFolder, out success);
        }

        
    }
}
