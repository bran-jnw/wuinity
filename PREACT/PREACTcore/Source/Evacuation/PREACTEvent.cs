//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using PREACT.Evacuation;
using PREACT.Input;

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
        public string DestinationName;
        public BlockDestinationEvent(float startTime, string destinationIndex)
        {
            StartTime = startTime;
            DestinationName = destinationIndex;
            Triggered = false;
        }

        public override void ApplyEffects()
        {
            if(!Triggered)
            {
                Triggered = true;                
                _simulation.Evacuation.TryBlockDestination(DestinationName);
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

                blockDestinationEvent = new BlockDestinationEvent(0, string.Empty);
                success = true;
            }
            else
            {
                Engine.Message(null, Engine.LogType.Warning, "Goal blocking event file not found in " + filePath + " and could not be loaded");
            }
            
            return blockDestinationEvent;
        }
    }
}
