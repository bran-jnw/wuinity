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
    public class EvacuationInput
    {
        public enum PedestrianModuleChoice { MacroHouseholdSim, JupedSimSUMO }
        public PedestrianModuleChoice pedestrianModuleChoice = PedestrianModuleChoice.MacroHouseholdSim;
        
        public float EvacuationOrderStart = 0.0f;
        public string[] ResponseCurves;
        public string[] EvacGroups;
        public float PaintCellSize = 200f;

        const string EvacuationOrderStartIn = "EvacuationOrderStart";
        const string ResponseCurvesIn = "ResponseCurves";
        const string EvacGroupsIn = "EvacGroups";
        const string PaintCellSizeIn = "PaintCellSize";


        public string[] BlockGoalEventFiles;

        public static EvacuationInput Parse(string[] inputLines, int startIndex)
        {
            int issues = 0;
            EvacuationInput newInput = new EvacuationInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);

            string temp;
            if (inputToParse.TryGetValue(EvacuationOrderStartIn, out temp))
            {
                float.TryParse(temp, out newInput.EvacuationOrderStart);
            }
            else
            {
                ++issues;
            }            

            return newInput;
        }
    }
}
