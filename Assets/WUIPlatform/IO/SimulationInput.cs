//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace WUIPlatform.IO
{
    [System.Serializable]
    public class SimulationInput
    {
        public string Id = "new";
        public Vector2d LowerLeftLatLon = new Vector2d(55.697354, 13.173808);
        public Vector2d DomainSize = new Vector2d(3000.0, 3000.0);
        public float DeltaTime = 1f;
        public float MaxSimTime = 0f;              
        public bool RunPedestrianModule = false;
        public bool RunTrafficModule = false;
        public bool RunFireModule = false;
        public bool RunSmokeModule = false;
        public bool StopWhenEvacuated = true;
        public bool StopAfterConverging = false;

        public static SimulationInput Parse(string[] inputLines, int startIndex)
        {
            int issues = 0;
            SimulationInput newInput = new SimulationInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);
            string input, userInput;

            input = nameof(Id);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                newInput.Id = userInput;
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
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
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
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
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            input = nameof(DeltaTime);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                float.TryParse(userInput, out newInput.DeltaTime);
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            input = nameof(MaxSimTime);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                float.TryParse(userInput, out newInput.MaxSimTime);
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            input = nameof(RunPedestrianModule);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                bool.TryParse(userInput, out newInput.RunPedestrianModule);
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            input = nameof(RunTrafficModule);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                bool.TryParse(userInput, out newInput.RunTrafficModule);
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            input = nameof(RunFireModule);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                bool.TryParse(userInput, out newInput.RunFireModule);
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            input = nameof(RunSmokeModule);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                bool.TryParse(userInput, out newInput.RunSmokeModule);
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            input = nameof(StopWhenEvacuated);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                bool.TryParse(userInput, out newInput.StopWhenEvacuated);
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            input = nameof(StopAfterConverging);
            if(inputToParse.TryGetValue(input, out userInput))
            {
                bool.TryParse(userInput, out newInput.StopAfterConverging);
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            if (issues > 0)
            {
                newInput = null;
            }

            return newInput;
        }
    }
}    