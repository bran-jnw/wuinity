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
    public abstract class WUInityEvent
    {
        public float StartTime;
        public bool Triggered;
        protected Simulation _simulation;

        public abstract void ApplyEffects();
    }

    [System.Serializable]
    public class BlockDestinationEvent : WUInityEvent
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

        public static BlockDestinationEvent[] LoadBlockGoalEvents(Input input, string rootFolder, out bool success)
        {
            success = false;
            List<BlockDestinationEvent> blockGoalEvents = new List<BlockDestinationEvent>();
            for (int i = 0; i < input.Events.BlockGoalEventFiles.Length; i++)
            {
                string path = Path.Combine(rootFolder, input.Events.BlockGoalEventFiles[i] + ".bge");
                if (File.Exists(path))
                {
                    string[] dataLines = File.ReadAllLines(path);
                    List<ResponseDataPoint> dataPoints = new List<ResponseDataPoint>();
                    //skip first line (header)
                    for (int j = 1; j < dataLines.Length; j++)
                    {
                        //TODO:
                        /*string[] data = dataLines[j].Split(',');

                        if (data.Length >= 2)
                        {
                            float time, probability;

                            bool timeRead = float.TryParse(data[0], out time);
                            bool probabilityRead = float.TryParse(data[1], out probability);
                            if (timeRead && probabilityRead)
                            {
                                ResponseDataPoint dataPoint = new ResponseDataPoint(time, probability);
                                dataPoints.Add(dataPoint);
                            }
                        }*/
                    }

                    //need at least two to make a curve
                    if (dataPoints.Count >= 2)
                    {
                        //responseCurves.Add(new ResponseCurve(dataPoints, WUIEngine.Input.Evacuation.responseCurveFiles[i]));
                        Engine.MESSAGE(null, Engine.LogType.Log, " Loaded goal blocking event from " + path);
                    }
                }
                else
                {
                    Engine.MESSAGE(null, Engine.LogType.Warning, "Goal blocking event file not found in " + path + " and could not be loaded");
                }
            }

            if (blockGoalEvents.Count > 0)
            {
                BlockDestinationEvent[] gbe = blockGoalEvents.ToArray();
                success = true;
                return gbe;
            }
            else
            {
                Engine.MESSAGE(null, Engine.LogType.Log, "No valid goal blocking events were loaded.");
                return null;
            }
        }
    }
}
