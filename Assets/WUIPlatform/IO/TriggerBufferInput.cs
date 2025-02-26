//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using ScottPlot.Drawing.Colormaps;
using System.Collections.Generic;
using static WUIPlatform.IO.FireInput;

namespace WUIPlatform.IO
{
    [System.Serializable]
    public class TriggerBufferInput
    {
        public bool calculateTriggerBuffer = false;
        public enum TriggerBufferChoice { kPERIL, BackwardsFireCell2 }
        public TriggerBufferChoice triggerBufferChoice = TriggerBufferChoice.kPERIL;

        public kPERILInput kPERILInput;

        public static TriggerBufferInput Parse(string[] inputLines, int startIndex)
        {
            int issues = 0;
            TriggerBufferInput newInput = new TriggerBufferInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);
            string temp;

            if (inputToParse.TryGetValue(nameof(calculateTriggerBuffer), out temp))
            {
                bool.TryParse(temp, out newInput.calculateTriggerBuffer);
            }
            else
            {
            }

            if (newInput.calculateTriggerBuffer)
            {
                if (inputToParse.TryGetValue(nameof(triggerBufferChoice), out temp))
                {
                    switch (temp)
                    {
                        case nameof(TriggerBufferChoice.kPERIL):
                            newInput.triggerBufferChoice = TriggerBufferChoice.kPERIL;
                            break;
                        case nameof(TriggerBufferChoice.BackwardsFireCell2):
                            newInput.triggerBufferChoice = TriggerBufferChoice.BackwardsFireCell2;
                            break;
                        default:
                            newInput.triggerBufferChoice = TriggerBufferChoice.kPERIL;
                            break;
                    }
                }
                else
                {
                    WUIEngine.LOG(WUIEngine.LogType.Error, "No trigger buffer module was set, using " + newInput.triggerBufferChoice.ToString() + ".");
                }
            }    
            
            if(newInput.triggerBufferChoice == TriggerBufferChoice.kPERIL)
            {
                //newInput.kPERILInput = kPERILInput.Parse()
            }

            return newInput;
        }
    }

    public class kPERILInput
    {
        public float midflameWindspeed = 0f;
        public bool calculateROSFromBehave = true;

        public static kPERILInput Parse(string[] inputLines, int startIndex)
        {
            int issues = 0;
            kPERILInput newInput = new kPERILInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);
            string temp;

            if (inputToParse.TryGetValue(nameof(midflameWindspeed), out temp))
            {
                float.TryParse(temp, out newInput.midflameWindspeed);
            }
            else
            {
                ++issues;
                WUIEngine.LOG(WUIEngine.LogType.Error, "No midflame wind speed was set." + WUIEngineInput.pleaseCheckInput);
            }

            if (inputToParse.TryGetValue(nameof(calculateROSFromBehave), out temp))
            {
                bool.TryParse(temp, out newInput.calculateROSFromBehave);
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, nameof(calculateROSFromBehave) + " was not found, defaulting to " + newInput.calculateROSFromBehave.ToString() + ".");
            }

            return newInput;
        }
        
                
    }
}