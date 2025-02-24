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
        public string SimulationID = "new";
        public float DeltaTime = 1f;
        public float MaxSimTime = 0f;
        public bool StopWhenEvacuated = true;
        public bool StopAfterConverging = true;
        public Vector2d LowerLeftLatLon = new Vector2d(55.697354, 13.173808);
        public Vector2d DomainSize = new Vector2d(3000.0, 3000.0);
        public bool ScaleToWebMercator = false;
        public bool RunPedestrianModule = false;
        public bool RunTrafficModule = false;
        public bool RunFireModule = false;
        public bool RunSmokeModule = false;

        public static SimulationInput Parse(string[] inputLines, int startIndex)
        {
            int issues = 0;
            SimulationInput newInput = new SimulationInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetCategoryInput(inputLines, startIndex);

            string temp;
            if (inputToParse.TryGetValue("id", out temp))
            {
                newInput.SimulationID = temp;
            }
            if (inputToParse.TryGetValue("deltaTime", out temp))
            {
                float.TryParse(temp, out newInput.DeltaTime);
            }
            if (inputToParse.TryGetValue("maxSimTime", out temp))
            {
                float.TryParse(temp, out newInput.MaxSimTime);
            }
            if (inputToParse.TryGetValue("stopWhenEvacuated", out temp))
            {
                bool.TryParse(temp, out newInput.StopWhenEvacuated);
            }
            if (inputToParse.TryGetValue("stopAfterConverging", out temp))
            {
                bool.TryParse(temp, out newInput.StopAfterConverging);
            }
            if (inputToParse.TryGetValue("lowerLeftLatLon", out temp))
            {
                string[] data = temp.Split(',');
                double.TryParse(data[0], out newInput.LowerLeftLatLon.x);
                double.TryParse(data[1], out newInput.LowerLeftLatLon.y);
            }
            if (inputToParse.TryGetValue("domainSize", out temp))
            {
                string[] data = temp.Split(',');
                double.TryParse(data[0], out newInput.DomainSize.x);
                double.TryParse(data[1], out newInput.DomainSize.y);
            }
            if (inputToParse.TryGetValue("scaleToWebMercator", out temp))
            {
                bool.TryParse(temp, out newInput.ScaleToWebMercator);
            }
            if (inputToParse.TryGetValue("runPedestrianModule", out temp))
            {
                bool.TryParse(temp, out newInput.RunPedestrianModule);
            }
            if (inputToParse.TryGetValue("RunTrafficModule", out temp))
            {
                bool.TryParse(temp, out newInput.RunTrafficModule);
            }
            if (inputToParse.TryGetValue("RunFireModule", out temp))
            {
                bool.TryParse(temp, out newInput.RunFireModule);
            }
            if (inputToParse.TryGetValue("RunSmokeModule", out temp))
            {
                bool.TryParse(temp, out newInput.RunSmokeModule);
            }

            return newInput;
        }
    }
}    