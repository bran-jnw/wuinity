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
    public class FireInput
    {
        public enum FireModuleChoice { AscImport, FireCell, FireCell2, VectorCells, FarsiteDLL, PrometheusCOM }
        public FireModuleChoice FireModule = FireModuleChoice.AscImport;
        public string LcpFile;
        public string GraphicalFireInputFile;

        //module settings
        public AscImportInput AscImportInput;
        public FireCellInput FireCellInput;

        public static FireInput Parse(string[] inputLines, int startIndex, Dictionary<string, int> headerLineIndex)
        {
            int issues = 0;
            FireInput newInput = new FireInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);
            string input, userInput;

            input = nameof(FireModule);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                switch (userInput)
                {
                    case nameof(FireModuleChoice.AscImport):
                        newInput.FireModule = FireModuleChoice.AscImport;
                        break;
                    case nameof(FireModuleChoice.FireCell):
                        newInput.FireModule = FireModuleChoice.FireCell;
                        break;
                    case nameof(FireModuleChoice.FireCell2):
                        newInput.FireModule = FireModuleChoice.FireCell2;
                        break;
                    default:
                        ++issues;
                        WUIEngine.LOG(WUIEngine.LogType.SimError, input + " input " + userInput + " was not recognized." + WUIEngineInput.pleaseCheckInput);
                        break;
                }
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            if (inputToParse.TryGetValue(nameof(LcpFile), out userInput))
            {
                newInput.LcpFile = userInput;
            }
            else
            {
                ++issues;
                WUIEngine.LOG(WUIEngine.LogType.SimError, nameof(LcpFile) + " was not specified." + WUIEngineInput.pleaseCheckInput);
            }

            if (inputToParse.TryGetValue(nameof(GraphicalFireInputFile), out userInput))
            {
                newInput.GraphicalFireInputFile = userInput;
            }
            else
            {
                ++issues;
                WUIEngine.LOG(WUIEngine.LogType.SimError, nameof(GraphicalFireInputFile) + " was not specified." + WUIEngineInput.pleaseCheckInput);
            }

            //now check modules that have been selected
            if (newInput.FireModule == FireModuleChoice.AscImport)
            {
                input = nameof(FireModuleChoice.AscImport);
                WUIEngineInput.ReadingInputMessage(input);
                int lineindex;
                if (headerLineIndex.TryGetValue(input, out lineindex))
                {
                    newInput.AscImportInput = AscImportInput.Parse(inputLines, lineindex);
                }
                else
                {
                    //critical
                    WUIEngineInput.InputNotFoundMessage(input);
                    return null;
                }
            }
            else if (newInput.FireModule == FireModuleChoice.FireCell)
            {
                input = nameof(FireModuleChoice.FireCell);
                WUIEngineInput.ReadingInputMessage(input);
                int lineindex;
                if (headerLineIndex.TryGetValue(input, out lineindex))
                {
                    newInput.FireCellInput = FireCellInput.Parse(inputLines, lineindex);
                }
                else
                {
                    //critical
                    WUIEngineInput.InputNotFoundMessage(input);
                    return null;
                }
            }

            return newInput;
        }
    }
}  