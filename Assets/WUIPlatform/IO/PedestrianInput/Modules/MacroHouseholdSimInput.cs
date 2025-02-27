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
    public class MacroHouseholdSimInput
    {
        public Vector2 WalkingSpeedMinMax = new Vector2(0.7f, 1.0f);
        public float WalkingSpeedModifier = 1.0f;
        public float WalkingDistanceModifier = 1.0f;               

        public static MacroHouseholdSimInput Parse(string[] inputLines, int startIndex)
        {
            int issues = 0;
            MacroHouseholdSimInput newInput = new MacroHouseholdSimInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);
            string input, userInput;

            input = nameof(WalkingDistanceModifier);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                float.TryParse(userInput, out newInput.WalkingDistanceModifier);
            }

            input = nameof(WalkingSpeedMinMax);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                string[] data = userInput.Split(',');
                if (data.Length >= 2)
                {
                    float.TryParse(data[0], out newInput.WalkingSpeedMinMax.X);
                    float.TryParse(data[1], out newInput.WalkingSpeedMinMax.Y);
                }
            }

            input = nameof(WalkingSpeedModifier);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                float.TryParse(userInput, out newInput.WalkingSpeedModifier);
            }

            return newInput;
        }
    }
}

    