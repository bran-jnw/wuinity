//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace WUIPlatform.Traffic
{
    public abstract class TrafficEvent
    {
        public float startTime;
        public float endTime;
        public bool isActive;

        public abstract void ApplyEffects(MacroTrafficSim mTS);
        public abstract void RemoveEffects(MacroTrafficSim mTS);
        public abstract void ResetEvent();
    }

    [System.Serializable]
    public class ReverseLanes : TrafficEvent
    {
        public ReverseLanes(float startTime, float endTime)
        {
            this.startTime = startTime;
            this.endTime = endTime;
            isActive = false;
        }

        public override void ApplyEffects(MacroTrafficSim mTS)
        {
            if (mTS == null)
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, "Error, no mCS set.");
                return;
            }

            mTS.reverseLanes = true;
            //mCS.stallBigRoads = true;
            isActive = true;
        }

        public override void RemoveEffects(MacroTrafficSim mTS)
        {
            if (mTS == null)
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, "Error, no mCS set.");
                return;
            }

            mTS.reverseLanes = false;
            //mCS.stallBigRoads = false;
            isActive = false;
        }

        public override void ResetEvent()
        {
            isActive = false;
        }

        public static ReverseLanes[] GetDummy()
        {
            ReverseLanes[] rLs = new ReverseLanes[1];
            rLs[0] = new ReverseLanes(float.MaxValue - 1f, float.MaxValue);
            return rLs;
        }
    }

    [System.Serializable]
    public class TrafficAccident : TrafficEvent
    {
        public TrafficAccident(float startTime, float endTime)
        {
            this.startTime = startTime;
            this.endTime = endTime;
            isActive = false;
        }

        public override void ApplyEffects(MacroTrafficSim mTS)
        {
            if (mTS == null)
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, "Error, no mCS set.");
                return;
            }

            mTS.stallBigRoads = true;
            isActive = true;
        }

        public override void RemoveEffects(MacroTrafficSim mTS)
        {
            if (mTS == null)
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, "Error, no mCS set.");
                return;
            }

            mTS.stallBigRoads = false;
            isActive = false;
        }

        public override void ResetEvent()
        {
            isActive = false;
        }

        public static TrafficAccident[] GetDummy()
        {
            TrafficAccident[] tAs = new TrafficAccident[1];
            tAs[0] = new TrafficAccident(float.MaxValue - 1f, float.MaxValue);
            return tAs;
        }
    }
}
