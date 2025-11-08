//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using PREACT.Evacuation;
using PREACT.IO;

namespace PREACT
{
    public abstract class PREACTEvent
    {
        public float StartTime;
        public bool Triggered;
        protected Simulation _simulation;

        public abstract void ApplyEffects();
    }

    public class BlockDestinationEvent : PREACTEvent
    {
        public int GoalIndex;
        public BlockDestinationEvent(float startTime, int goalIndex)
        {
            StartTime = startTime;
            GoalIndex = goalIndex;
            Triggered = false;
        }

        public override void ApplyEffects()
        {
            if(!Triggered)
            {
                Triggered = true;
                Engine.MESSAGE(null, Engine.LogType.Event, "Goal blocked: " + _simulation.Destinations[GoalIndex].Name);
                _simulation.BlockEvacGoal(GoalIndex);
            }            
        }

        public static BlockDestinationEvent LoadBlockGoalEvent(string filePath, out bool success)
        {
            success = false;
            BlockDestinationEvent blockDestinationEvent = null;

            if (File.Exists(filePath))
            {                
                string[] dataLines = File.ReadAllLines(filePath);
                //skip first line (header)
                for (int j = 1; j < dataLines.Length; j++)
                {
                    //TODO:                    
                }

                blockDestinationEvent = new BlockDestinationEvent(0, -1);
                success = true;
            }
            else
            {
                Engine.MESSAGE(null, Engine.LogType.Warning, "Goal blocking event file not found in " + filePath + " and could not be loaded");
            }
            
            return blockDestinationEvent;
        }
    }
}
