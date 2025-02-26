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
        public float evacuationOrderStart = 0.0f;
        public string[] evacuationGoals;
        public string[] responseCurves;
        public string[] evacuationGroups;
        public float paintCellSize = 200f;

        /*const string EvacuationOrderStartIn = "EvacuationOrderStart";
        const string EvacuationGoalsIn = "EvacuationGoals";
        const string EvacGroupsIn = "EvacGroups";
        const string ResponseCurvesIn = "ResponseCurves";        
        const string PaintCellSizeIn = "PaintCellSize";*/


        public static EvacuationInput Parse(string[] inputLines, int startIndex)
        {
            int issues = 0;
            EvacuationInput newInput = new EvacuationInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);
            string temp;

            if (inputToParse.TryGetValue(nameof(evacuationOrderStart), out temp))
            {
                float.TryParse(temp, out newInput.evacuationOrderStart);
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, nameof(evacuationOrderStart) + " was not found, using default of " + newInput.evacuationOrderStart + " seconds.");             
            }

            //TODO: fix actual reading
            if (inputToParse.TryGetValue(nameof(evacuationGoals), out temp))
            {
                string[] data = temp.Split(',');
                newInput.evacuationGoals = data;
            }
            else
            {
                ++issues;
                WUIEngine.LOG(WUIEngine.LogType.Error, nameof(evacuationGoals) + " was not found." + WUIEngineInput.pleaseCheckInput);
            }

            //TODO: fix actual reading
            if (inputToParse.TryGetValue(nameof(evacuationGroups), out temp))
            {
                string[] data = temp.Split(',');
                newInput.evacuationGroups = data;
            }
            else
            {
                ++issues;
            }

            //TODO: fix actual reading
            if (inputToParse.TryGetValue(nameof(responseCurves), out temp))
            {
                string[] data = temp.Split(',');
                newInput.responseCurves = data;
            }
            else
            {
                ++issues;
            }

            if (inputToParse.TryGetValue(nameof(paintCellSize), out temp))
            {
                float.TryParse(temp, out newInput.paintCellSize);
            }
            else
            {
            }

            return newInput;
        }
    }
}
