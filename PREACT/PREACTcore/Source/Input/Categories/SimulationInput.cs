//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System;
using PREACT.Math;

namespace PREACT.Input
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
        public DateTime StartDateTime = DateTime.Now;
        public DateTime EndDateTime = DateTime.Now;
        public bool StopWhenEvacuated = false;

        public SimulationInput()
        {
            _data = new SimulationData(_lowerLeftLatLon);
        }

        public void Parse(string[] inputLines, int startIndex, out bool success)
        {
            success = false;
            int issues = 0;            
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string nameOfInput, userInput;

            //critical
            nameOfInput = nameof(Name);
            if(inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                Name = userInput;
                if(Name.Length == 0)
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
                return;
            }

            //critical
            nameOfInput = nameof(LowerLeftLatLon);
            if(inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                string[] data = userInput.Split(',');
                issues += double.TryParse(data[0], out _lowerLeftLatLon.x) ? 0 : 1;
                issues += double.TryParse(data[1], out _lowerLeftLatLon.y) ? 0 : 1;
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
                return;
            }

            //critical
            nameOfInput = nameof(DomainSize);
            if(inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                string[] data = userInput.Split(',');
                issues += double.TryParse(data[0], out DomainSize.x) ? 0 : 1;
                issues += double.TryParse(data[1], out DomainSize.y) ? 0 : 1;
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
                return;
            }

            //not critical
            nameOfInput = nameof(DeltaTime);
            if(inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                float.TryParse(userInput, out DeltaTime);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            nameOfInput = nameof(StartDateTime);
            if(inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                success = DateTime.TryParse(userInput, out StartDateTime);
                if(!success)
                {
                    PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                }
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

            nameOfInput = nameof(EndDateTime);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                success = DateTime.TryParse(userInput, out EndDateTime);
                if (!success)
                {
                    PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                }
            }
            else
            {
                success = false;
                PREACTInput.InputNotFoundMessage(nameOfInput, true);
            }
            if (!success)
            {
                return;
            }

            nameOfInput = nameof(StopWhenEvacuated);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                bool.TryParse(userInput, out StopWhenEvacuated);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            _data.UpdateData(LowerLeftLatLon);
            success = true;
        }
    }
}    