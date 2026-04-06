//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace PREACT.Input
{
    [System.Serializable]
    public class TrafficModuleInput
    {
        public enum TrafficModules { None, SUMO, MacroTrafficSim, CityFlow }

        private TrafficData _data;
        private SUMOInput _sumoInput;
        private MacroTrafficSimInput _macroTrafficSimInput;
        private CityFlowInput _cityFlowInput;

        public bool Enabled = false;
        public TrafficData Data { get => _data; }
        public SUMOInput SumoInput { get { return _sumoInput; } }
        public MacroTrafficSimInput MacroTrafficSimInput { get => _macroTrafficSimInput; }
        public CityFlowInput CityFlowInput { get => _cityFlowInput; }
        public TrafficModules Module = TrafficModules.SUMO;
        public bool VisibilityAffectsSpeed = false;     


        public TrafficModuleInput() 
        {
            _data = new TrafficData();
            _sumoInput = new SUMOInput();
            _macroTrafficSimInput = new MacroTrafficSimInput();
            _cityFlowInput = new CityFlowInput();
        }

        public void Parse(string[] inputLines, int startIndex, Dictionary<string, int> headerLineIndex, string rootFolder, out bool success)
        {     
            success = false;
            int issues = 0;
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string nameOfInput, userInput;

            nameOfInput = nameof(Enabled);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                success = bool.TryParse(userInput, out Enabled);
            }
            else
            {
                success = false;
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }
            if (!success || !Enabled)
            {
                return;
            }

            //critical
            nameOfInput = nameof(Module);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                switch (userInput)
                {
                    case nameof(TrafficModules.SUMO):
                        Module = TrafficModules.SUMO;
                        break;
                    case nameof(TrafficModules.MacroTrafficSim):
                        Module = TrafficModules.MacroTrafficSim;
                        break;
                    default:
                        ++issues;
                        PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
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
                return;
            }

            nameOfInput = nameof(VisibilityAffectsSpeed);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                bool.TryParse(userInput, out VisibilityAffectsSpeed);
            }
            else
            {                
            }

            //load correct module
            if(Module == TrafficModules.SUMO)
            {
                int lineIndex;
                nameOfInput = nameof(TrafficModules.SUMO);
                if (headerLineIndex.TryGetValue(nameOfInput, out lineIndex))
                {
                    PREACTInput.ReadingInputMessage(nameOfInput);
                    _sumoInput = SUMOInput.Parse(inputLines, lineIndex, rootFolder, out success);
                }
                else
                {
                    //critical
                    Engine.Message(null, Engine.LogType.SimulationError, nameof(Simulation) + " header not found." + PREACTInput.pleaseCheckInput);
                    return;
                }
            }
            else if(Module == TrafficModules.MacroTrafficSim)
            {

            }
            else if(Module == TrafficModules.CityFlow)
            {
                int lineIndex;
                nameOfInput = nameof(TrafficModules.CityFlow);
                if (headerLineIndex.TryGetValue(nameOfInput, out lineIndex))
                {
                    PREACTInput.ReadingInputMessage(nameOfInput);
                    _cityFlowInput = CityFlowInput.Parse(inputLines, lineIndex, rootFolder, out success);
                }
                else
                {
                    //critical
                    Engine.Message(null, Engine.LogType.SimulationError, nameof(Simulation) + " header not found." + PREACTInput.pleaseCheckInput);
                    return;
                }
            }
            else
            {
                Engine.Message(null, Engine.LogType.SimulationError, "Unknown traffic module has been specified.");
            }

            _data.LoadAll(this, rootFolder, out success);
        }
    }
}
    