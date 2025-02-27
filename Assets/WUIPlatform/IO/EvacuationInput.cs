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
        public float EvacuationOrderStart = 0.0f;
        public string[] EvacuationGoalFiles;
        public string[] ResponseCurveFiles;
        public string[] EvacuationGroupFiles;
        public float PaintCellSize = 200f;

        public static EvacuationInput Parse(string[] inputLines, int startIndex)
        {
            int issues = 0;
            EvacuationInput newInput = new EvacuationInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);
            string input, userInput;

            input = nameof(EvacuationOrderStart);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                float.TryParse(userInput, out newInput.EvacuationOrderStart);
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, nameof(EvacuationOrderStart) + " was not found, using default of " + newInput.EvacuationOrderStart + " seconds.");             
            }

            //TODO: fix actual reading
            input = nameof(EvacuationGoalFiles);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                string[] data = userInput.Split(',');
                newInput.EvacuationGoalFiles = data;
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            //TODO: fix actual reading
            input = nameof(EvacuationGroupFiles);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                string[] data = userInput.Split(',');
                newInput.EvacuationGroupFiles = data;
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            //TODO: fix actual reading
            input = nameof(ResponseCurveFiles);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                string[] data = userInput.Split(',');
                newInput.ResponseCurveFiles = data;
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            input = nameof(PaintCellSize);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                float.TryParse(userInput, out newInput.PaintCellSize);
            }
            else
            {
            }

            return newInput;
        }
    }
}
