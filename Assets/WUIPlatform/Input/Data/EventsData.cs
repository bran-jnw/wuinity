//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.IO;
using System.Collections.Generic;

namespace PREACT.IO
{
    public class EventsData
    {
        private List<BlockDestinationEvent> _blockDestinationEvents;

        public List<BlockDestinationEvent> BlockDestinationEvents {  get => _blockDestinationEvents; }

        public EventsData()
        {
            _blockDestinationEvents = new List<BlockDestinationEvent>();
        }

        public void LoadAll(EventsInput eventsInput, string rootFolder, out bool success)
        {       
            int issues = 0;

            for(int i = 0; i < eventsInput.BlockGoalEventFiles.Count; ++i)
            {
                string filePath = Path.Combine(rootFolder, eventsInput.BlockGoalEventFiles[i]);
                BlockDestinationEvent.LoadBlockGoalEvent(filePath, out success);
                issues += success ? 0 : 1;
            }
            
            if (issues > 0)
            {
                success = false;
                return;
            }

            success = true;
        }

        public void LoadBlockGoalEvent(string filePath, out bool success)
        {
            BlockDestinationEvent e = BlockDestinationEvent.LoadBlockGoalEvent(filePath, out success);
            if(success)
            {
                _blockDestinationEvents.Add(e);
            }
        }
    }
}