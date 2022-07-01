using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace WUInity
{
    public abstract class WUInityEvent
    {
        public float startTime;
        [System.NonSerialized] public bool triggered;

        public abstract void ApplyEffects();
    }

    [System.Serializable]
    public class BlockGoalEvent : WUInityEvent
    {
        public int goalIndex;
        public BlockGoalEvent(float startTime, int goalIndex)
        {
            this.startTime = startTime;
            this.goalIndex = goalIndex;
            triggered = false;
        }

        public override void ApplyEffects()
        {
            if(!triggered)
            {
                triggered = true;
                WUInity.LOG("EVENT: Goal blocked: " + WUInity.RUNTIME_DATA.EvacuationGoals[goalIndex].name);
                WUInity.SIM.BlockEvacGoal(goalIndex);
            }            
        }

        public static BlockGoalEvent[] GetDummy()
        {
            BlockGoalEvent[] wE = new BlockGoalEvent[1];
            wE[0] = new BlockGoalEvent(float.MaxValue, 0);
            return wE;
        }

        public static BlockGoalEvent[] LoadBlockGoalEvents()
        {
            List<BlockGoalEvent> blockGoalEvents = new List<BlockGoalEvent>();
            for (int i = 0; i < WUInity.INPUT.Evacuation.GoalEventFiles.Length; i++)
            {
                string path = Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.Evacuation.GoalEventFiles[i] + ".bge");
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
                        //responseCurves.Add(new ResponseCurve(dataPoints, WUInity.INPUT.Evacuation.responseCurveFiles[i]));
                        WUInity.LOG("LOG: Loaded goal blocking event from " + path);
                    }
                }
                else
                {
                    WUInity.LOG("WARNING: Goalblocking event file not found in " + path + " and could not be loaded");
                }
            }

            if (blockGoalEvents.Count > 0)
            {
                BlockGoalEvent[] gbe = blockGoalEvents.ToArray();
                return gbe;
            }
            else
            {
                WUInity.LOG("ERROR: No response curves could be loaded, simulation will stall.");
                return null;
            }
        }
    }
}
