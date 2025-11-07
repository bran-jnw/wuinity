//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using PREACT.Utility.Math;
using PREACT.Runtime;

namespace PREACT.IO
{
    public class SimulationInput
    {
        private SimulationData _data;
        private Vector2d _lowerLeftLatLon;

        public SimulationData Data { get => _data; }
        public string Name;
        public Vector2d LowerLeftLatLon { get => _lowerLeftLatLon; set { _lowerLeftLatLon = value; _data.UpdateData(this); } }
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
            _data = new SimulationData(this);
        }

        public static SimulationInput Parse(string[] inputLines, int startIndex, out bool success)
        {
            success = false;
            int criticalIssues = 0;
            SimulationInput newInput = new SimulationInput();
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string input, userInput;

            //critical
            input = nameof(Name);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                newInput.Name = userInput;
                ++criticalIssues;
            }
            else
            {
                ++criticalIssues;
                PREACTInput.InputNotFoundMessage(input, true);
            }
            if (criticalIssues > 0)
            {
                success = false;
                return newInput;
            }

            //critical
            input = nameof(LowerLeftLatLon);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                string[] data = userInput.Split(',');
                criticalIssues += double.TryParse(data[0], out newInput._lowerLeftLatLon.x) ? 0 : 1;
                criticalIssues += double.TryParse(data[1], out newInput._lowerLeftLatLon.y) ? 0 : 1;
            }
            else
            {
                PREACTInput.InputNotFoundMessage(input, true);
            }
            if(criticalIssues > 0)
            {
                success = false;
                return newInput;
            }

            //critical
            input = nameof(DomainSize);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                string[] data = userInput.Split(',');
                criticalIssues += double.TryParse(data[0], out newInput.DomainSize.x) ? 0 : 1;
                criticalIssues += double.TryParse(data[1], out newInput.DomainSize.y) ? 0 : 1;
            }
            else
            {
                ++criticalIssues;
                PREACTInput.InputNotFoundMessage(input, true);
            }
            if (criticalIssues > 0)
            {
                success = false;
                return newInput;
            }

            input = nameof(DeltaTime);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                float.TryParse(userInput, out newInput.DeltaTime);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(input);
            }

            input = nameof(MaxSimTime);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                float.TryParse(userInput, out newInput.MaxSimTime);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(input);
            }

            input = nameof(RunPedestrianModule);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                bool.TryParse(userInput, out newInput.RunPedestrianModule);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(input);
            }

            input = nameof(RunTrafficModule);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                bool.TryParse(userInput, out newInput.RunTrafficModule);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(input);
            }

            input = nameof(RunFireModule);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                bool.TryParse(userInput, out newInput.RunFireModule);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(input);
            }

            input = nameof(RunSmokeModule);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                bool.TryParse(userInput, out newInput.RunSmokeModule);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(input);
            }

            input = nameof(StopWhenEvacuated);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                bool.TryParse(userInput, out newInput.StopWhenEvacuated);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(input);
            }

            newInput._data.UpdateData(newInput);
            success = true;
            return newInput;
        }
    }
}    