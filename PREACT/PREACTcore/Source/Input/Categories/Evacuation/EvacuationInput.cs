//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using PREACT.Evacuation;

namespace PREACT.Input
{
    [System.Serializable]
    public class EvacuationInput
    {
        private EvacuationData _data;

        public EvacuationData Data { get => _data; }
        public Dictionary<string, EvacuationDestinationInput> EvacuationDestinationInputs = new Dictionary<string, EvacuationDestinationInput>(5);
        public Dictionary<string, ResponseCurve> ResponseCurves = new Dictionary<string, ResponseCurve>(5);        
        public Dictionary<string, EvacuationGroupInput> EvacuationGroupInputs = new Dictionary<string, EvacuationGroupInput>(5);
        public bool UseTriggerBufferEvacuation = false;
        public string TriggerBufferFile = string.Empty;

        public EvacuationInput()
        {
            _data = new EvacuationData();
        }

        public void Parse(string[] inputLines, int startIndex, SimulationInput simulationInput, EventsInput eventsInput, PopulationInput population, PedestrianModuleInput pedestrianInput, TrafficModuleInput trafficInput, List<int> destinationLineIndices, List<int> responseCurveLineIndices, List<int> evacuationGroupLineIndices, string rootFolder, out bool success)
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
            EvacuationDestinationInput.Parse(EvacuationDestinationInputs, inputLines, destinationLineIndices, out success);
            if (!success)
            {
                return;
            }

            //critical
            ResponseCurve.Parse(ResponseCurves, inputLines, responseCurveLineIndices, simulationInput, out success);
            if (!success)
            {
                return;
            }

            //critical, must be done after response curves and destinations
            EvacuationGroupInput.Parse(EvacuationGroupInputs, inputLines, evacuationGroupLineIndices, EvacuationDestinationInputs, ResponseCurves, simulationInput, population, rootFolder, out success);
            if (!success)
            {
                return;
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
