//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Numerics;
using System.Collections.Generic;

namespace WUIPlatform.IO
{
    [System.Serializable]
    public class AdvectDiffuseInput
    {
        public float MixingLayerHeight = 250.0f;

        public static AdvectDiffuseInput Parse(string[] inputLines, int startIndex)
        {
            int issues = 0;
            AdvectDiffuseInput newInput = new AdvectDiffuseInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);
            string input, userInput;

            input = nameof(MixingLayerHeight);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                float.TryParse(userInput, out newInput.MixingLayerHeight);
            }
            else
            {
                WUIEngineInput.InputNotFoundMessage(input);
            }

            return newInput;
        }
    }
}

