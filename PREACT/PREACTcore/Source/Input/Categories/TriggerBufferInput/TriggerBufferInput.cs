//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace PREACT.IO
{
    [System.Serializable]
    public class TriggerBufferInput
    {
        public enum TriggerBufferChoice { None, kPERIL, BackwardsFireCell2 }

        public kPERILInput _kPERILInput;

        public kPERILInput kPERILInput { get => _kPERILInput; }
        public bool CalculateTriggerBuffer = false;        
        public TriggerBufferChoice TriggerBuffer = TriggerBufferChoice.None;
        

        public TriggerBufferInput() 
        { 
            _kPERILInput = new kPERILInput();
        }

        public static TriggerBufferInput Parse(string[] inputLines, int startIndex, Dictionary<string, int> headerLineIndex, string rootFolder, out bool success)
        {
            TriggerBufferInput newInput = new TriggerBufferInput();
            success = false;
            int issues = 0;             
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string nameOfInput, userInput;

            nameOfInput = nameof(CalculateTriggerBuffer);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                bool.TryParse(userInput, out newInput.CalculateTriggerBuffer);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            if (newInput.CalculateTriggerBuffer)
            {
                nameOfInput = nameof(TriggerBuffer);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
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
                            PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                            break;
                    }
                }
                else
                {
                    ++issues;
                    PREACTInput.InputNotFoundMessage(nameOfInput);
                }
                if(issues > 0)
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, true);
                    return newInput;
                }

                //now check modules that have been selected
                if (newInput.TriggerBuffer == TriggerBufferChoice.kPERIL)
                {
                    //critical
                    nameOfInput = nameof(TriggerBufferChoice.kPERIL);
                    int lineindex;
                    if (headerLineIndex.TryGetValue(nameOfInput, out lineindex))
                    {
                        newInput._kPERILInput = kPERILInput.Parse(inputLines, lineindex, rootFolder, out success);
                    }
                    else
                    {
                        success = false;
                        PREACTInput.InputNotFoundMessage(nameOfInput);
                    }
                    if(!success)
                    {
                        return newInput;
                    }
                }
                else
                {
                    Engine.Message(null, Engine.LogType.Debug, "Trying to use non-implemented trigger buffer.");
                }
            }

            success = true;
            return newInput;
        }
    }     
}