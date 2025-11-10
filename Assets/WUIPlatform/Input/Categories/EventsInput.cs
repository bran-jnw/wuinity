//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using PREACT.IO;

namespace PREACT.IO
{
    [System.Serializable]
    public class EventsInput
    {
        private EventsData _data;

        public EventsData EventsData { get => _data; }
        public List<string> BlockGoalEventFiles = new List<string>();

        public EventsInput()
        {
            _data = new EventsData();
        }

        public static EventsInput Parse(string[] inputLines, int startIndex, string rootFolder, out bool success)
        {
            EventsInput newInput = new EventsInput();
            success = false;
            int issues = 0;                     
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string nameOfInput, userInput;

            //not critical
            nameOfInput = nameof(BlockGoalEventFiles);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                string[] data = userInput.Split(',');
                newInput.BlockGoalEventFiles.AddRange(data);
                PREACTInput.CheckIfFilesExists(nameOfInput, data, rootFolder, out success);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            newInput._data.LoadAll(newInput, rootFolder, out success);
            return newInput;
        }
    }
}