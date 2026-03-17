//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace PREACT.Input
{
    [System.Serializable]
    public class AdvectDiffuseInput
    {
        public float MixingLayerHeight = 250.0f;

        public static AdvectDiffuseInput Parse(string[] inputLines, int startIndex, out bool success)
        {
            AdvectDiffuseInput newInput = new AdvectDiffuseInput();
            success = false;
            int issues = 0;            
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string nameOfInput, userInput;

            nameOfInput = nameof(MixingLayerHeight);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                issues += float.TryParse(userInput, out newInput.MixingLayerHeight) ? 0 : 1;
                if(issues > 0)
                {
                    PREACTInput.InputNotFoundMessage(nameOfInput, true);
                }
            }
            else
            {
                ++issues;
                PREACTInput.InputNotFoundMessage(nameOfInput, true);
            }
            if(issues > 0)
            {
                success = false;
                return newInput;
            }

            success = true;
            return newInput;
        }
    }
}

