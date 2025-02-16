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
        public bool sendDataToWUIShow = false;
        public string wuiShowServerIP = "127.0.0.1";
        public int wuiShowServerPort = 9023;
        public float wuiShowDeltaTime = 1f;

        private static readonly string sendDataString = "sendDataToWUIShow";
        private static readonly string serverIPString = "wuiShowServerIP";
        private static readonly string serverPortString = "wuiShowServerPort";
        private static readonly string deltaTimeString = "wuiShowDeltaTime";

        public static WUIShowInput Parse(string[] inputLines, int startIndex)
        {
            WUIShowInput newInput = new WUIShowInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetCategoryInput(inputLines, startIndex);

            string temp;
            if (inputToParse.TryGetValue(sendDataString, out temp))
            {
                bool.TryParse(temp, out newInput.sendDataToWUIShow);
            }
            if (inputToParse.TryGetValue(serverIPString, out temp))
            {
                newInput.wuiShowServerIP = temp;
            }
            if (inputToParse.TryGetValue(serverPortString, out temp))
            {
                int.TryParse(temp, out newInput.wuiShowServerPort);
            }
            if (inputToParse.TryGetValue(deltaTimeString, out temp))
            {
                float.TryParse(temp, out newInput.wuiShowDeltaTime);
            }

            return newInput;
        }

        public List<string> GetInputString()
        {
            List<string> lines = new List<string>();

            lines.Add("[WUIShow]");
            lines.Add(sendDataString + "=" + sendDataToWUIShow.ToString().ToLower());
            lines.Add(serverIPString + "=" + wuiShowServerPort);
            lines.Add(serverPortString + "=" + wuiShowServerPort);
            lines.Add(deltaTimeString + "=" + wuiShowDeltaTime);

            return lines;
        }
    }
}