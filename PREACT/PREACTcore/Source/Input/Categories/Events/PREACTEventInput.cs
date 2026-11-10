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
    public class PREACTEventInput
    {
        public enum EventType
        {
            BlockDestination,
            EvacuationOrder
        }

        public string Name;
        public EventType Type;        
        public DateTime TriggerDateTime;


        public PREACTEventInput()
        {

        }

        //The common part of events
        public static void Parse(Dictionary<string, string> inputToParse, PREACTEventInput newInput, out bool success)
        {
            success = false;
            string nameOfInput, userInput;

            //critical
            nameOfInput = nameof(Name);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
            success = true;
                newInput.Name = userInput;
            }
            else
            {
                success = false;
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }
            if(!success)
            {
                return;
            }

            nameOfInput = nameof(TriggerDateTime);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                success = DateTime.TryParse(userInput, out newInput.TriggerDateTime);
                if (!success)
                {
                    PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                }
            }
            else
            {
                success = false;
                PREACTInput.InputNotFoundMessage(nameOfInput, true);
            }
            if (!success)
            {
                return;
            }

            nameOfInput = nameof(Type);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                success = Enum.TryParse(userInput, out newInput.Type);
                if (!success)
                {
                    PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                }
            }
            else
            {
                success = false;
                PREACTInput.InputNotFoundMessage(nameOfInput, true);
            }
            if (!success)
            {
                return;
            }

            success = true;
        }
    }
}



