//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using PREACT.Events;
using PREACT.Input;
using System;

namespace PREACT.Input
{
    public class EvacuationOrderEventInput : PREACTEventInput
    {
        public List<string> EvacuationGroups = new List<string>(5);

        public EvacuationOrderEventInput()
        {

        }

        public static EvacuationOrderEventInput Parse(string[] inputLines, int eventsLineIndex, out bool success)
        {
            EvacuationOrderEventInput newInput = new EvacuationOrderEventInput();

            success = false;
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, eventsLineIndex);
            string nameOfInput, userInput;
                    
            //General
            Parse(inputToParse, newInput, out success);
            if(!success)
            {
                return null;
            }

            //critical
            nameOfInput = nameof(EvacuationGroups);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                success = true;
                string[] data = userInput.Split(',');
                for (int j = 0; j < data.Length; ++j)
                {
                    newInput.EvacuationGroups.Add(data[j]);
                }
            }
            else
            {
                success = false;
                PREACTInput.InputNotFoundMessage(nameOfInput, true);
            }
            if (!success)
            {
                return null;
            }


            success = true;
            return newInput;
        }
    }
}



