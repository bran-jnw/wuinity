//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using PREACT.Runtime;

namespace PREACT.IO
{
    [System.Serializable]
    public class FireInput
    {
        public enum FireModuleChoice { None, AscImport, FireCell, FireCell2, VectorCells, FarsiteDLL, PrometheusCOM }

        private FireData _data;
        private AscImportInput _ascImportInput;
        private FireCellInput _fireCellInput;

        public FireData FireData { get => _data; }
        public AscImportInput AscImportInput { get => _ascImportInput; }
        public FireCellInput FireCellInput { get => _fireCellInput; }
        public FireModuleChoice FireModule = FireModuleChoice.None;
        public string LcpFile = "";
        public string GraphicalFireInputFile = "";


        public FireInput() 
        {
            _data = new FireData();
            _ascImportInput = new AscImportInput();
            _fireCellInput = new FireCellInput();
        }

        public static FireInput Parse(string[] inputLines, int startIndex, Dictionary<string, int> headerLineIndex, SimulationInput simulationInput, string rootFolder, out bool success)
        {
            FireInput newInput = new FireInput();
            if (!simulationInput.RunFireModule)
            {
                success = true;
                return newInput;
            }

            success = false;          
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string inputName, userInput;

            inputName = nameof(FireModule);
            if (inputToParse.TryGetValue(inputName, out userInput))
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
                        PREACTInput.CouldNotInterpretInputMessage(inputName, userInput);
                        break;
                }
            }
            else
            {
                PREACTInput.InputNotFoundMessage(inputName, true);
                success = false;
            }
            if (!success)
            {
                return newInput;
            }

            //might not always need lcp file
            if (newInput.FireModule != FireModuleChoice.None || newInput.FireModule != FireModuleChoice.AscImport)
            {
                inputName = nameof(LcpFile);
                if (inputToParse.TryGetValue(inputName, out userInput))
                {
                    newInput.LcpFile = userInput;
                    PREACTInput.CheckIfFileExist(inputName, userInput, rootFolder, out success);
                }
                else
                {
                    PREACTInput.InputNotFoundMessage(inputName, true);
                    success = false;
                }
                if(!success)
                {
                    return newInput;
                }
            }            

            //might be critical
            inputName = nameof(GraphicalFireInputFile);
            if (inputToParse.TryGetValue(nameof(GraphicalFireInputFile), out userInput))
            {
                newInput.GraphicalFireInputFile = userInput;
                PREACTInput.CheckIfFileExist(inputName, userInput, rootFolder, out success);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(inputName);
            }

            //now check modules that have been selected
            if (newInput.FireModule == FireModuleChoice.AscImport)
            {
                inputName = nameof(FireModuleChoice.AscImport);
                PREACTInput.ReadingInputMessage(inputName);
                int lineindex;
                if (headerLineIndex.TryGetValue(inputName, out lineindex))
                {
                    newInput._ascImportInput = AscImportInput.Parse(inputLines, lineindex, rootFolder, out success);
                }
                else
                {
                    //critical
                    PREACTInput.InputNotFoundMessage(inputName);
                    return null;
                }
            }
            else if (newInput.FireModule == FireModuleChoice.FireCell)
            {
                inputName = nameof(FireModuleChoice.FireCell);
                PREACTInput.ReadingInputMessage(inputName);
                int lineindex;
                if (headerLineIndex.TryGetValue(inputName, out lineindex))
                {
                    newInput._fireCellInput = FireCellInput.Parse(inputLines, lineindex, newInput, rootFolder, out success);
                }
                else
                {
                    //critical
                    PREACTInput.InputNotFoundMessage(inputName);
                    return null;
                }
            }

            newInput._data.LoadAll(simulationInput, newInput, rootFolder, out success);

            return newInput;
        }
    }
}  