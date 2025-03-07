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
    public class WUIShowInput
    {
        public bool SendDataToWUIShow = false;
        public string WuiShowServerIP = "127.0.0.1";
        public int WuiShowServerPort = 9023;
        public float WuiShowDeltaTime = 1f;

        /*private static readonly string sendDataString = "sendDataToWUIShow";
        private static readonly string serverIPString = "wuiShowServerIP";
        private static readonly string serverPortString = "wuiShowServerPort";
        private static readonly string deltaTimeString = "wuiShowDeltaTime";*/

        public static WUIShowInput Parse(string[] inputLines, int startIndex)
        {
            WUIShowInput newInput = new WUIShowInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);
            string input, userInput;

            input = nameof(SendDataToWUIShow);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                bool.TryParse(userInput, out newInput.SendDataToWUIShow);
            }

            input = nameof(WuiShowServerIP);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                newInput.WuiShowServerIP = userInput;
            }

            input = nameof(WuiShowServerPort);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                int.TryParse(userInput, out newInput.WuiShowServerPort);
            }

            input = nameof(WuiShowDeltaTime);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                float.TryParse(userInput, out newInput.WuiShowDeltaTime);
            }

            return newInput;
        }
    }
}