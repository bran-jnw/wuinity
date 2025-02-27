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
    public class EventsInput
    {
        public string[] BlockGoalEventFiles;

        public EventsInput()
        {
            BlockGoalEventFiles = new string[0];
        }

        public static EventsInput Parse(string[] inputLines, int startIndex)
        {
            int issues = 0;
            EventsInput newInput = new EventsInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);
            string temp;            

            //TODO: get actual data
            if (inputToParse.TryGetValue(nameof(BlockGoalEventFiles), out temp))
            {
                string[] data = temp.Split(',');
                newInput.BlockGoalEventFiles = data;
            }
            else
            {
                ++issues;
            }

            return newInput;
        }
    }
}