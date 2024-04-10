using System.Collections.Generic;
using System.IO;

namespace WUIPlatform
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
                WUIEngine.LOG(WUIEngine.LogType.Event, "Goal blocked: " + WUIEngine.RUNTIME_DATA.Evacuation.EvacuationGoals[goalIndex].name);
                WUIEngine.SIM.BlockEvacGoal(goalIndex);
            }            
        }

        public static BlockGoalEvent[] GetDummy()
        {
            BlockGoalEvent[] wE = new BlockGoalEvent[1];
            wE[0] = new BlockGoalEvent(float.MaxValue, 0);
            return wE;
        }

        public static BlockGoalEvent[] LoadBlockGoalEvents(out bool success)
        {
            success = false;
            List<BlockGoalEvent> blockGoalEvents = new List<BlockGoalEvent>();
            for (int i = 0; i < WUIEngine.INPUT.Evacuation.BlockGoalEventFiles.Length; i++)
            {
                string path = Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Evacuation.BlockGoalEventFiles[i] + ".bge");
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
                        WUIEngine.LOG(WUIEngine.LogType.Log, " Loaded goal blocking event from " + path);
                    }
                }
                else
                {
                    WUIEngine.LOG(WUIEngine.LogType.Warning, "Goal blocking event file not found in " + path + " and could not be loaded");
                }
            }

            if (blockGoalEvents.Count > 0)
            {
                BlockGoalEvent[] gbe = blockGoalEvents.ToArray();
                success = true;
                return gbe;
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, "No valid goal blocking events could be loaded.");
                return null;
            }
        }
    }
}
