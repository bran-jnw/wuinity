//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using PREACT.IO;

namespace PREACT.IO
{
    [System.Serializable]
    public class WildfireModuleInput
    {
        public enum WildfireModules { None, AscImport, FireCell, CellParticleHybrid, FarsiteDLL, PrometheusCOM }

        private WildfireData _data;
        private AscImportInput _ascImportInput;
        private FireCellInput _fireCellInput;

        public bool Enabled = false;
        public WildfireData Data { get => _data; }
        public AscImportInput AscImportInput { get => _ascImportInput; }
        public FireCellInput FireCellInput { get => _fireCellInput; }
        public WildfireModules Module = WildfireModules.None;        
        public string GraphicalFireInputFile = string.Empty;


        public WildfireModuleInput() 
        {
            _data = new WildfireData();
            _ascImportInput = new AscImportInput();
            _fireCellInput = new FireCellInput();
        }

        public void Parse(string[] inputLines, int startIndex, SimulationInput simulationInput, WeatherInput weatherInput, Dictionary<string, int> headerLineIndex, string rootFolder, out bool success)
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
            if (!success || !Enabled)
            {
                return;
            }

            if(!Enabled)
            {
                success = true;
                return;
            }            

            nameOfInput = nameof(Module);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                success = true;
                switch (userInput)
                {
                    case nameof(WildfireModules.AscImport):
                        Module = WildfireModules.AscImport;
                        break;
                    case nameof(WildfireModules.FireCell):
                        Module = WildfireModules.FireCell;
                        break;
                    case nameof(WildfireModules.CellParticleHybrid):
                        Module = WildfireModules.CellParticleHybrid;
                        break;
                    default:
                        success = false;
                        PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                        break;
                }
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput, true);
                success = false;
            }
            if (!success)
            {
                return;
            }

            //check if weather exists, may be critical
            /*if (weatherInput.WeatherFile == string.Empty && (Module != WildfireModules.AscImport || Module != WildfireModules.None))
            {
                success = false;
                PREACTInput.CriticalDependency(nameof(weatherInput.WeatherFile));
                return;
            }*/         

            //might be critical if using e.g. random ignition
            nameOfInput = nameof(GraphicalFireInputFile);
            if (inputToParse.TryGetValue(nameof(GraphicalFireInputFile), out userInput))
            {
                GraphicalFireInputFile = userInput;
                PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success);
                if(!success)
                {
                    GraphicalFireInputFile = string.Empty;
                }
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            //now check modules that have been selected
            if (Module == WildfireModules.AscImport)
            {
                nameOfInput = nameof(WildfireModules.AscImport);
                PREACTInput.ReadingInputMessage(nameOfInput);
                int lineindex;
                if (headerLineIndex.TryGetValue(nameOfInput, out lineindex))
                {
                    _ascImportInput = AscImportInput.Parse(inputLines, lineindex, rootFolder, out success);
                }
                else
                {
                    //critical
                    PREACTInput.InputNotFoundMessage(nameOfInput);
                    return;
                }
            }
            else if (Module == WildfireModules.FireCell)
            {
                nameOfInput = nameof(WildfireModules.FireCell);
                PREACTInput.ReadingInputMessage(nameOfInput);
                int lineindex;
                if (headerLineIndex.TryGetValue(nameOfInput, out lineindex))
                {
                    _fireCellInput.Parse(inputLines, lineindex, this, rootFolder, out success);
                }
                else
                {
                    //critical
                    PREACTInput.InputNotFoundMessage(nameOfInput);
                    return;
                }
            }
            else if (Module == WildfireModules.CellParticleHybrid)
            {
                nameOfInput = nameof(WildfireModules.CellParticleHybrid);
                PREACTInput.ReadingInputMessage(nameOfInput);
                int lineindex;
                if (headerLineIndex.TryGetValue(nameOfInput, out lineindex))
                {
                    _fireCellInput.Parse(inputLines, lineindex, this, rootFolder, out success);
                }
                else
                {
                    //critical
                    PREACTInput.InputNotFoundMessage(nameOfInput);
                    return;
                }
            }

            _data.LoadAll(simulationInput, this, rootFolder, out success);
        }
    }
}  