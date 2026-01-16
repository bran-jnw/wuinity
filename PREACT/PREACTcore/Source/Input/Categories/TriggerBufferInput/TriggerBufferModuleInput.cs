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
    public class TriggerBufferModuleInput
    {
        public enum TriggerBufferModules { None, kPERIL, BackwardsFireCell2 }

        private kPERILInput _kPERILInput;

        public bool Enabled = false;
        public TriggerBufferModules Module = TriggerBufferModules.None;
        public kPERILInput kPERILInput { get => _kPERILInput; }             
        
        

        public TriggerBufferModuleInput() 
        { 
            _kPERILInput = new kPERILInput();
        }

        public void Parse(string[] inputLines, int startIndex, Dictionary<string, int> headerLineIndex, string rootFolder, out bool success)
        {
            success = false;
            int issues = 0;             
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string nameOfInput, userInput;

            nameOfInput = nameof(Enabled);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                success = bool.TryParse(userInput, out Enabled);
            }
            else
            {
                success = false;
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }
            if(!success)
            {
                return;
            }

            if (Enabled)
            {
                nameOfInput = nameof(Module);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    switch (userInput)
                    {
                        case nameof(TriggerBufferModules.kPERIL):
                            Module = TriggerBufferModules.kPERIL;
                            break;
                        case nameof(TriggerBufferModules.BackwardsFireCell2):
                            Module = TriggerBufferModules.BackwardsFireCell2;
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
                    return;
                }

                //now check modules that have been selected
                if (Module == TriggerBufferModules.kPERIL)
                {
                    //critical
                    nameOfInput = nameof(TriggerBufferModules.kPERIL);
                    int lineindex;
                    if (headerLineIndex.TryGetValue(nameOfInput, out lineindex))
                    {
                        _kPERILInput = kPERILInput.Parse(inputLines, lineindex, rootFolder, out success);
                    }
                    else
                    {
                        success = false;
                        PREACTInput.InputNotFoundMessage(nameOfInput);
                    }
                    if(!success)
                    {
                        return;
                    }
                }
                else
                {
                    Engine.Message(null, Engine.LogType.Debug, "Trying to use non-implemented trigger buffer.");
                }
            }

            success = true;
        }
    }     
}