//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using PREACT.IO;
using PREACT.Math;

namespace PREACT.IO
{
    public class SimulationInput
    {
        private SimulationData _data;
        private Vector2d _lowerLeftLatLon;

        public SimulationData Data { get => _data; }
        public string Name = string.Empty;
        public Vector2d LowerLeftLatLon { get => _lowerLeftLatLon; set { _lowerLeftLatLon = value; _data.UpdateData(LowerLeftLatLon); } }
        public Vector2d DomainSize;
        public float DeltaTime = 1.0f;
        public float MaxSimTime= float.MaxValue;
        public bool RunPedestrianModule = false;
        public bool RunTrafficModule = false;
        public bool RunFireModule = false;
        public bool RunSmokeModule = false;
        public bool StopWhenEvacuated = true;

        public SimulationInput()
        {
            _data = new SimulationData(_lowerLeftLatLon);
        }

        public static SimulationInput Parse(string[] inputLines, int startIndex, out bool success)
        {
            SimulationInput newInput = new SimulationInput();
            success = false;
            int issues = 0;            
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string nameOfInput, userInput;

            //critical
            nameOfInput = nameof(Name);
            if(inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                newInput.Name = userInput;
                if(newInput.Name.Length == 0)
                {
                    PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                    ++issues;
                }                
            }
            else
            {
                ++issues;
                PREACTInput.InputNotFoundMessage(nameOfInput, true);
            }
            if (issues > 0)
            {
                success = false;
                return newInput;
            }

            //critical
            nameOfInput = nameof(LowerLeftLatLon);
            if(inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                string[] data = userInput.Split(',');
                issues += double.TryParse(data[0], out newInput._lowerLeftLatLon.x) ? 0 : 1;
                issues += double.TryParse(data[1], out newInput._lowerLeftLatLon.y) ? 0 : 1;
                if(issues > 0)
                {
                    PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                }
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput, true);
            }
            if(issues > 0)
            {
                success = false;
                return newInput;
            }

            //critical
            nameOfInput = nameof(DomainSize);
            if(inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                string[] data = userInput.Split(',');
                issues += double.TryParse(data[0], out newInput.DomainSize.x) ? 0 : 1;
                issues += double.TryParse(data[1], out newInput.DomainSize.y) ? 0 : 1;
                if (issues > 0)
                {
                    PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                }
            }
            else
            {
                ++issues;
                PREACTInput.InputNotFoundMessage(nameOfInput, true);
            }
            if (issues > 0)
            {
                success = false;
                return newInput;
            }

            //not critical
            nameOfInput = nameof(DeltaTime);
            if(inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                float.TryParse(userInput, out newInput.DeltaTime);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            nameOfInput = nameof(MaxSimTime);
            if(inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                float.TryParse(userInput, out newInput.MaxSimTime);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            nameOfInput = nameof(RunPedestrianModule);
            if(inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                bool.TryParse(userInput, out newInput.RunPedestrianModule);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            nameOfInput = nameof(RunTrafficModule);
            if(inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                bool.TryParse(userInput, out newInput.RunTrafficModule);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            nameOfInput = nameof(RunFireModule);
            if(inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                bool.TryParse(userInput, out newInput.RunFireModule);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            nameOfInput = nameof(RunSmokeModule);
            if(inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                bool.TryParse(userInput, out newInput.RunSmokeModule);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            nameOfInput = nameof(StopWhenEvacuated);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                bool.TryParse(userInput, out newInput.StopWhenEvacuated);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            newInput._data.UpdateData(newInput.LowerLeftLatLon);
            success = true;
            return newInput;
        }
    }
}    