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
        public Vector2d LowerLeftLatLon = new Vector2d(55.697354, 13.173808);
        public Vector2d DomainSize = new Vector2d(3000.0, 3000.0);
        public float DeltaTime = 1f;
        public float MaxSimTime = 0f;
        public bool StopWhenEvacuated = true;        
        public bool RunPedestrianModule = false;
        public bool RunTrafficModule = false;
        public bool RunFireModule = false;
        public bool RunSmokeModule = false;

        public bool StopAfterConverging = true;

        const string idIn = "id";
        const string lowerLeftLatLonIn = "lowerLeftLatLon";
        const string domainSizeIn = "domainSize";
        const string deltaTimeIn = "deltaTime";
        const string maxSimTimeIn = "maxSimTime";
        const string stopWhenEvacuatedIn = "stopWhenEvacuated";        
        const string runPedestrianModuleIn = "runPedestrianModule";
        const string runTrafficModuleIn = "runTrafficModule";
        const string runFireModuleIn = "runFireModule";
        const string runSmokeModuleIn = "runSmokeModule";

        const string stopAfterConvergingIn = "stopAfterConverging";

        public static SimulationInput Parse(string[] inputLines, int startIndex)
        {
            int issues = 0;
            SimulationInput newInput = new SimulationInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);

            string temp;
            if (inputToParse.TryGetValue(idIn, out temp))
            {
                newInput.SimulationID = temp;
            }
            else
            {
                ++issues;
            }

            if (inputToParse.TryGetValue(lowerLeftLatLonIn, out temp))
            {
                string[] data = temp.Split(',');
                double.TryParse(data[0], out newInput.LowerLeftLatLon.x);
                double.TryParse(data[1], out newInput.LowerLeftLatLon.y);
            }
            else
            {
                ++issues;
            }

            if (inputToParse.TryGetValue(domainSizeIn, out temp))
            {
                string[] data = temp.Split(',');
                double.TryParse(data[0], out newInput.DomainSize.x);
                double.TryParse(data[1], out newInput.DomainSize.y);
            }
            else
            {
                ++issues;
            }

            if (inputToParse.TryGetValue(deltaTimeIn, out temp))
            {
                float.TryParse(temp, out newInput.DeltaTime);
            }
            else
            {
                ++issues;
            }

            if (inputToParse.TryGetValue(maxSimTimeIn, out temp))
            {
                float.TryParse(temp, out newInput.MaxSimTime);
            }
            else
            {
                ++issues;
            }

            if (inputToParse.TryGetValue(stopWhenEvacuatedIn, out temp))
            {
                bool.TryParse(temp, out newInput.StopWhenEvacuated);
            }
            else
            {
                ++issues;
            }   

            if (inputToParse.TryGetValue(runPedestrianModuleIn, out temp))
            {
                bool.TryParse(temp, out newInput.RunPedestrianModule);
            }
            else
            {
                ++issues;
            }

            if (inputToParse.TryGetValue(runTrafficModuleIn, out temp))
            {
                bool.TryParse(temp, out newInput.RunTrafficModule);
            }
            else
            {
                ++issues;
            }

            if (inputToParse.TryGetValue(runFireModuleIn, out temp))
            {
                bool.TryParse(temp, out newInput.RunFireModule);
            }
            else
            {
                ++issues;
            }

            if (inputToParse.TryGetValue(runSmokeModuleIn, out temp))
            {
                bool.TryParse(temp, out newInput.RunSmokeModule);
            }
            else
            {
                ++issues;
            }

            if (inputToParse.TryGetValue(stopAfterConvergingIn, out temp))
            {
                bool.TryParse(temp, out newInput.StopAfterConverging);
            }
            else
            {
                ++issues;
            }

            if (issues > 0)
            {
                newInput = null;
            }

            return newInput;
        }
    }
}    