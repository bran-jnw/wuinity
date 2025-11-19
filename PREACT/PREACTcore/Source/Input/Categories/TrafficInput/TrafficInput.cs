//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace PREACT.IO
{
    [System.Serializable]
    public class TrafficInput
    {
        public enum TrafficModuleChoice { SUMO, MacroTrafficSim, CityFlow }

        private TrafficData _data;
        private SUMOInput _sumoInput;
        private MacroTrafficSimInput _macroTrafficSimInput;
        private CityFlowInput _cityFlowInput;

        public TrafficData Data { get => _data; }
        public SUMOInput SumoInput { get { return _sumoInput; } }
        public MacroTrafficSimInput MacroTrafficSimInput { get => _macroTrafficSimInput; }
        public CityFlowInput CityFlowInput { get => _cityFlowInput; }
        public TrafficModuleChoice TrafficModule = TrafficModuleChoice.SUMO;      
        public bool VisibilityAffectsSpeed = false;     


        public TrafficInput() 
        {
            _data = new TrafficData();
            _sumoInput = new SUMOInput();
            _macroTrafficSimInput = new MacroTrafficSimInput();
            _cityFlowInput = new CityFlowInput();
        }

        public static TrafficInput Parse(string[] inputLines, int startIndex, Dictionary<string, int> headerLineIndex, SimulationInput simulationInput, string rootFolder, out bool success)
        {            
            TrafficInput newInput = new TrafficInput();
            if (!simulationInput.RunTrafficModule)
            {
                success = true;
                return newInput;
            }

            success = false;
            int issues = 0;
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string input, userInput;

            //critical
            input = nameof(TrafficModule);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                switch (userInput)
                {
                    case nameof(TrafficModuleChoice.SUMO):
                        newInput.TrafficModule = TrafficModuleChoice.SUMO;
                        break;
                    case nameof(TrafficModuleChoice.MacroTrafficSim):
                        newInput.TrafficModule = TrafficModuleChoice.MacroTrafficSim;
                        break;
                    default:
                        ++issues;
                        PREACTInput.CouldNotInterpretInputMessage(input, userInput);
                        break;
                }
            }
            else
            {
                ++issues;
                Engine.Message(null, Engine.LogType.SimulationError, "No traffic module choice was set.");
            }
            if(issues > 0)
            {
                return newInput;
            }

            input = nameof(VisibilityAffectsSpeed);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                bool.TryParse(userInput, out newInput.VisibilityAffectsSpeed);
            }
            else
            {                
            }

            //load correct module
            if(newInput.TrafficModule == TrafficModuleChoice.SUMO)
            {
                int lineIndex;
                input = nameof(TrafficModuleChoice.SUMO);
                if (headerLineIndex.TryGetValue(input, out lineIndex))
                {
                    PREACTInput.ReadingInputMessage(input);
                    newInput._sumoInput = SUMOInput.Parse(inputLines, lineIndex, rootFolder, out success);
                }
                else
                {
                    //critical
                    Engine.Message(null, Engine.LogType.SimulationError, nameof(Simulation) + " header not found." + PREACTInput.pleaseCheckInput);
                    return newInput;
                }
            }
            else if(newInput.TrafficModule == TrafficModuleChoice.MacroTrafficSim)
            {

            }
            else if(newInput.TrafficModule == TrafficModuleChoice.CityFlow)
            {
                int lineIndex;
                input = nameof(TrafficModuleChoice.CityFlow);
                if (headerLineIndex.TryGetValue(input, out lineIndex))
                {
                    PREACTInput.ReadingInputMessage(input);
                    newInput._cityFlowInput = CityFlowInput.Parse(inputLines, lineIndex, rootFolder, out success);
                }
                else
                {
                    //critical
                    Engine.Message(null, Engine.LogType.SimulationError, nameof(Simulation) + " header not found." + PREACTInput.pleaseCheckInput);
                    return newInput;
                }
            }
            else
            {
                Engine.Message(null, Engine.LogType.SimulationError, "Unknown traffic module has been specified.");
            }

            newInput._data.LoadAll(newInput, rootFolder, out success);
            return newInput;
        }
    }
}
    