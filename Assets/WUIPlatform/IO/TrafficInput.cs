//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Numerics;
using WUIPlatform.Traffic;

namespace WUIPlatform.IO
{
    [System.Serializable]
    public class TrafficInput
    {
        public enum TrafficModuleChoice { SUMO, MacroTrafficSim }
        public TrafficModuleChoice trafficModuleChoice = TrafficModuleChoice.SUMO;      
        public bool visibilityAffectsSpeed = false;    
        public TrafficAccident[] trafficAccidents = TrafficAccident.GetDummy();
        public ReverseLanes[] reverseLanes = ReverseLanes.GetDummy();
        public TrafficInjection[] trafficInjections = TrafficInjection.GetTemplate();
        public TrafficProbe[] trafficProbes = TrafficProbe.GetTemplate();

        //module settings
        public SUMOInput sumoInput;
        public MacroTrafficSimInput macroTrafficSimInput;

        const string trafficModuleChoiceIn = nameof(trafficModuleChoice);
        const string visibilityAffectsSpeedIn = nameof(visibilityAffectsSpeed);

        public static TrafficInput Parse(string[] inputLines, int startIndex)
        {
            int issues = 0;
            TrafficInput newInput = new TrafficInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);
            string temp;

            if (inputToParse.TryGetValue(trafficModuleChoiceIn, out temp))
            {
                switch (temp)
                {
                    case nameof(TrafficModuleChoice.SUMO):
                        newInput.trafficModuleChoice = TrafficModuleChoice.SUMO;
                        break;
                    case nameof(TrafficModuleChoice.MacroTrafficSim):
                        newInput.trafficModuleChoice = TrafficModuleChoice.MacroTrafficSim;
                        break;
                    default:
                        newInput.trafficModuleChoice = TrafficModuleChoice.SUMO;
                        break;
                }
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, "No traffic module choice was set, using SUMO.");
            }

            if (inputToParse.TryGetValue(visibilityAffectsSpeedIn, out temp))
            {
                bool.TryParse(visibilityAffectsSpeedIn, out newInput.visibilityAffectsSpeed);
            }
            else
            {
                
            }

            return newInput;
        }
    }

    [System.Serializable]
    public class SUMOInput
    {
        public string inputFile;
        public Vector2d UTMoffset;
    }

    public class MacroTrafficSimInput
    {
        public float stallSpeed = 5f;
        public Vector2 backGroundDensityMinMax = Vector2.Zero;        
        public string roadTypesFile;
        public enum RouteChoice { Fastest, Closest, Random, EvacGroup };
        public RouteChoice routeChoice = RouteChoice.Closest;
    }
}
    