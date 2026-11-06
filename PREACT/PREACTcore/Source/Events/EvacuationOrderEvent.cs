//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System;

namespace PREACT.Events
{
    public class EvacuationOrderEvent : PREACTEvent
    {        
        private List<string> _evacuationGroups;
        
        public List<string> EvacuationGroups { get => _evacuationGroups; }

        public EvacuationOrderEvent(string name, DateTime triggerTime, List<string> evacuationGroups) : base(name)
        {
            TriggerDateTime = triggerTime;
            Triggered = false;
            _evacuationGroups = evacuationGroups;
        }

        public override void TriggerEvent(Simulation simulation)
        {
            if (!Triggered)
            {
                Triggered = true;
                simulation.Evacuation.IssueEvacuationOrder(this);
            }
        }
    }
}
