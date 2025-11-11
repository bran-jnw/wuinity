//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace PREACT.IO
{
    [System.Serializable]
    public class WUIShowInput
    {
        public bool SendDataToWUIShow = false;
        public string WuiShowServerIP = "127.0.0.1";
        public int WuiShowServerPort = 9023;
        public float WuiShowDeltaTime = 1f;


        public WUIShowInput()
        {
        }

        public static WUIShowInput Parse(string[] inputLines, int startIndex, out bool success)
        {
            WUIShowInput newInput = new WUIShowInput();
            success = false;
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string nameOfInput, userInput;

            nameOfInput = nameof(SendDataToWUIShow);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                bool.TryParse(userInput, out newInput.SendDataToWUIShow);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            nameOfInput = nameof(WuiShowServerIP);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                newInput.WuiShowServerIP = userInput;
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            nameOfInput = nameof(WuiShowServerPort);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                int.TryParse(userInput, out newInput.WuiShowServerPort);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            nameOfInput = nameof(WuiShowDeltaTime);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                float.TryParse(userInput, out newInput.WuiShowDeltaTime);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            success = true;
            return newInput;
        }
    }
}