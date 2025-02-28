//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace WUIPlatform.IO
{
    [System.Serializable]
    public class TriggerBufferInput
    {
        public bool CalculateTriggerBuffer = false;
        public enum TriggerBufferChoice { kPERIL, BackwardsFireCell2 }
        public TriggerBufferChoice TriggerBuffer = TriggerBufferChoice.kPERIL;

        public kPERILInput kPERILInput;

        public static TriggerBufferInput Parse(string[] inputLines, int startIndex, Dictionary<string, int> headerLineIndex)
        {
            int issues = 0;
            TriggerBufferInput newInput = new TriggerBufferInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);
            string input, userInput;

            input = nameof(CalculateTriggerBuffer);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                bool.TryParse(userInput, out newInput.CalculateTriggerBuffer);
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            if (newInput.CalculateTriggerBuffer)
            {
                if (inputToParse.TryGetValue(nameof(TriggerBuffer), out userInput))
                {
                    switch (userInput)
                    {
                        case nameof(TriggerBufferChoice.kPERIL):
                            newInput.TriggerBuffer = TriggerBufferChoice.kPERIL;
                            break;
                        case nameof(TriggerBufferChoice.BackwardsFireCell2):
                            newInput.TriggerBuffer = TriggerBufferChoice.BackwardsFireCell2;
                            break;
                        default:
                            ++issues;
                            WUIEngineInput.CouldNotInterpretInputMessage(input, userInput);
                            break;
                    }
                }
                else
                {
                    WUIEngine.LOG(WUIEngine.LogType.SimError, "No trigger buffer module was set, using " + newInput.TriggerBuffer.ToString() + ".");
                }

                //now check modules that have been selected
                if (newInput.TriggerBuffer == TriggerBufferChoice.kPERIL)
                {
                    input = nameof(TriggerBufferChoice.kPERIL);
                    WUIEngineInput.ReadingInputMessage(input);
                    int lineindex;
                    if (headerLineIndex.TryGetValue(input, out lineindex))
                    {
                        newInput.kPERILInput = kPERILInput.Parse(inputLines, lineindex);
                    }
                    else
                    {
                        //critical
                        ++issues;
                        WUIEngineInput.InputNotFoundMessage(input);
                    }
                }
                else
                {
                    WUIEngine.LOG(WUIEngine.LogType.Debug, "Trying to use non-implemented trigger buffer.");
                }
            }    

            return newInput;
        }
    }     
}