//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using PREACT.Utility.Math;

namespace PREACT.IO
{
    public struct SimulationInput
    {
        public string Name;
        public Vector2d LowerLeftLatLon;
        public Vector2d DomainSize;
        public float DeltaTime;
        public float MaxSimTime;              
        public bool RunPedestrianModule;
        public bool RunTrafficModule;
        public bool RunFireModule;
        public bool RunSmokeModule;
        public bool StopWhenEvacuated;        

        public static SimulationInput Parse(string[] inputLines, int startIndex, out uint criticalIssues)
        {
            criticalIssues = 0;
            SimulationInput newInput = new SimulationInput();
            Dictionary<string, string> inputToParse = Input.GetHeaderInput(inputLines, startIndex);
            string input, userInput;

            input = nameof(Name);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                newInput.Name = userInput;
            }
            else
            {
                ++criticalIssues;
                Input.InputNotFoundMessage(input);
            }

            input = nameof(LowerLeftLatLon);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                string[] data = userInput.Split(',');
                double.TryParse(data[0], out newInput.LowerLeftLatLon.x);
                double.TryParse(data[1], out newInput.LowerLeftLatLon.y);
            }
            else
            {
                ++criticalIssues;
                Input.InputNotFoundMessage(input);
            }

            input = nameof(DomainSize);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                string[] data = userInput.Split(',');
                double.TryParse(data[0], out newInput.DomainSize.x);
                double.TryParse(data[1], out newInput.DomainSize.y);
            }
            else
            {
                ++criticalIssues;
                Input.InputNotFoundMessage(input);
            }

            input = nameof(DeltaTime);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                float.TryParse(userInput, out newInput.DeltaTime);
            }
            else
            {
                ++criticalIssues;
                Input.InputNotFoundMessage(input);
            }

            input = nameof(MaxSimTime);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                float.TryParse(userInput, out newInput.MaxSimTime);
            }
            else
            {
                ++criticalIssues;
                Input.InputNotFoundMessage(input);
            }

            input = nameof(RunPedestrianModule);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                bool.TryParse(userInput, out newInput.RunPedestrianModule);
            }
            else
            {
                ++criticalIssues;
                Input.InputNotFoundMessage(input);
            }

            input = nameof(RunTrafficModule);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                bool.TryParse(userInput, out newInput.RunTrafficModule);
            }
            else
            {
                ++criticalIssues;
                Input.InputNotFoundMessage(input);
            }

            input = nameof(RunFireModule);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                bool.TryParse(userInput, out newInput.RunFireModule);
            }
            else
            {
                ++criticalIssues;
                Input.InputNotFoundMessage(input);
            }

            input = nameof(RunSmokeModule);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                bool.TryParse(userInput, out newInput.RunSmokeModule);
            }
            else
            {
                ++criticalIssues;
                Input.InputNotFoundMessage(input);
            }

            input = nameof(StopWhenEvacuated);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                bool.TryParse(userInput, out newInput.StopWhenEvacuated);
            }
            else
            {
                ++criticalIssues;
                Input.InputNotFoundMessage(input);
            }

            /*input = nameof(StopAfterConverging);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                bool.TryParse(userInput, out newInput.StopAfterConverging);
            }
            else
            {
                ++issues;
                Input.InputNotFoundMessage(input);
            }*/

            return newInput;
        }
    }
}    