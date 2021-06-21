using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public BlockGoalEvent(float startTime)
        {
            this.startTime = startTime;
            triggered = false;
        }

        public override void ApplyEffects()
        {
            if(!triggered)
            {
                triggered = true;
                WUInity.WUINITY_SIM.LogMessage("EVENT: Goal blocked: " + WUInity.WUINITY_IN.traffic.evacuationGoals[goalIndex].name);
                WUInity.WUINITY_SIM.BlockEvacGoal(goalIndex);
            }            
        }

        public static BlockGoalEvent[] GetDummy()
        {
            BlockGoalEvent[] wE = new BlockGoalEvent[1];
            wE[0] = new BlockGoalEvent(float.MaxValue);
            return wE;
        }
    }
}
