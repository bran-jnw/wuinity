//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Numerics;

namespace WUIPlatform.IO
{
    [System.Serializable]
    public class PedestrianInput
    {
        public enum PedestrianModuleChoice { MacroHouseholdSim, JupedSimSUMO }
        public PedestrianModuleChoice pedestrianModuleChoice = PedestrianModuleChoice.MacroHouseholdSim;

        //module inputs
        public MacroHouseholdSimInput macroHouseholdSimInput;

        public static PedestrianInput Parse(string[] inputLines, int startIndex, Dictionary<string, int> headerLineIndex)
        {
            int issues = 0;
            PedestrianInput newInput = new PedestrianInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);
            string temp;

            if (inputToParse.TryGetValue(nameof(pedestrianModuleChoice), out temp))
            {
                switch (temp)
                {
                    case nameof(PedestrianModuleChoice.MacroHouseholdSim):
                        newInput.pedestrianModuleChoice = PedestrianModuleChoice.MacroHouseholdSim;
                        break;
                    case nameof(PedestrianModuleChoice.JupedSimSUMO):
                        newInput.pedestrianModuleChoice = PedestrianModuleChoice.JupedSimSUMO;
                        break;
                    default:
                        newInput.pedestrianModuleChoice = PedestrianModuleChoice.MacroHouseholdSim;
                        break;
                }
            }
            else
            {
            }

            if(newInput.pedestrianModuleChoice == PedestrianModuleChoice.MacroHouseholdSim)
            {
                int lineIndex;
                if (headerLineIndex.TryGetValue(nameof(PedestrianModuleChoice.MacroHouseholdSim), out lineIndex))
                {
                    newInput.macroHouseholdSimInput = MacroHouseholdSimInput.Parse(inputLines, lineIndex);
                }
                else
                {
                    WUIEngine.LOG(WUIEngine.LogType.Warning, nameof(PedestrianModuleChoice.MacroHouseholdSim) + " input was not found, using defaults.");
                }
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Debug, "This should not happen, trying to use non-implemented pedestrian module.");
            }

            return newInput;
        }
    }

    public class MacroHouseholdSimInput
    {
        public float walkingDistanceModifier = 1.0f;
        public Vector2 walkingSpeedMinMax = new Vector2(0.7f, 1.0f);
        public float walkingSpeedModifier = 1.0f;

        public static MacroHouseholdSimInput Parse(string[] inputLines, int startIndex)
        {
            int issues = 0;
            MacroHouseholdSimInput newInput = new MacroHouseholdSimInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);
            string temp;

            if (inputToParse.TryGetValue(nameof(walkingDistanceModifier), out temp))
            {
                float.TryParse(temp, out newInput.walkingDistanceModifier);
            }

            if (inputToParse.TryGetValue(nameof(walkingSpeedMinMax), out temp))
            {
                string[] data = temp.Split(',');
                if(data.Length >= 2)
                {
                    float.TryParse(data[0], out newInput.walkingSpeedMinMax.X);
                    float.TryParse(data[1], out newInput.walkingSpeedMinMax.Y);
                }                
            }

            if (inputToParse.TryGetValue(nameof(walkingSpeedModifier), out temp))
            {
                float.TryParse(temp, out newInput.walkingSpeedModifier);
            }

            return newInput;
        }
    }
}
   