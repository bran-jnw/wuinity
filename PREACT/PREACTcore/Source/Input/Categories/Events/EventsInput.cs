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
    [System.Serializable]
    public class EventsInput
    {
        //private EventsData _data;

        //public EventsData Data { get => _data; }

        public List<EvacuationOrderEventInput> EvacuationOrderEventInputs = new List<EvacuationOrderEventInput>(5);
        public List<BlockDestinationEventInput> BlockDestinationEventInputs = new List<BlockDestinationEventInput>(5);
        

        public EventsInput()
        {
            //_data = new EventsData();
        }

        public void Parse(string[] inputLines, List<int> eventsLineIndices, out bool success)
        {
            success = false;
            EvacuationOrderEventInputs.Clear();
            BlockDestinationEventInputs.Clear();
            PREACTEventInput newEvent = new PREACTEventInput();

            for (int i = 0; i < eventsLineIndices.Count; ++i)
            {
                int issues = 0;
                Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, eventsLineIndices[i]);
                string nameOfInput, userInput;

                //General
                PREACTEventInput.Parse(inputToParse, newEvent, out success);
                if (!success)
                {
                    return;
                }

                if(newEvent.Type == PREACTEventInput.EventType.EvacuationOrder)
                {
                    EvacuationOrderEventInput evacuationOrderEventInput = EvacuationOrderEventInput.Parse(inputLines, eventsLineIndices[i], out success);
                    if (success)
                    {
                        EvacuationOrderEventInputs.Add(evacuationOrderEventInput);
                    }
                }
                else if(newEvent.Type == PREACTEventInput.EventType.BlockDestination)
                {
                    BlockDestinationEventInput blockDestinationEventInput = BlockDestinationEventInput.Parse(inputLines, eventsLineIndices[i], out success);
                    if (success)
                    {
                        BlockDestinationEventInputs.Add(blockDestinationEventInput);
                    }
                }
                if(!success)
                {
                    return;
                }   
            }

            //_data.LoadAll(this, rootFolder, out success);
        }
    }
}