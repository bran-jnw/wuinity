//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using PREACT.Math;

namespace PREACT.IO
{  
    [System.Serializable]
    public class SUMOInput
    {
        public enum SmokeSpeedReductionModels { Exponential, Smokanza};

        public string ConfigurationFile = string.Empty;
        public Vector2d UTMoffset;
        public double OutputRasterSize = 25.0;        
        public float SmokeAlpha = 0f;
        public float SmokeBeta = 0f;

        public SUMOInput()
        {

        }

        public static SUMOInput Parse(string[] inputLines, int startIndex, string rootFolder, out bool success)
        {
            success = false;
            int issues = 0;
            SUMOInput newInput = new SUMOInput();
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string nameOfInput, userInput;

            nameOfInput = nameof(ConfigurationFile);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                newInput.ConfigurationFile = userInput;
                PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success);
            }
            else
            {
                success = false;
                PREACTInput.InputNotFoundMessage(nameOfInput, true);
            }
            if(!success)
            {
                return newInput;
            }

            nameOfInput = nameof(UTMoffset);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                string[] data = userInput.Split(',');
                double.TryParse(data[0], out newInput.UTMoffset.x);
                double.TryParse(data[1], out newInput.UTMoffset.y);
            }
            else
            {
                ++issues;
                PREACTInput.InputNotFoundMessage(nameOfInput, true);
            }

            nameOfInput = nameof(OutputRasterSize);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                double.TryParse(userInput, out newInput.OutputRasterSize);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            nameOfInput = nameof(SmokeAlpha);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                float.TryParse(userInput, out newInput.SmokeAlpha);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            nameOfInput = nameof(SmokeBeta);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                float.TryParse(userInput, out newInput.SmokeBeta);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            if (issues == 0)
            {
                success = true;
            }
            return newInput;
        }
    }
}
