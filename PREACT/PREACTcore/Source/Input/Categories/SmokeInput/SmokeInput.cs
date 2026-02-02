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
    public class SmokeInput
    {
        public enum SmokeModules { None, GlobalSmoke, BoxModel, AdvectDiffuseMixingLayer, AdvectDiffuse3D, Lagrangian, GaussianPuff, GaussianPlume, FFD }

        private SmokeData _data;
        private GlobalSmokeInput _globalSmokeInput;
        private AdvectDiffuseInput _advectDiffuseInput;
        private LagrangianInput _lagrangianInput;

        public bool Enabled = false;
        public SmokeData Data { get =>  _data; } 
        public SmokeModules Module = SmokeModules.None;
        public GlobalSmokeInput GlobalSmokeInput { get => _globalSmokeInput; }
        public AdvectDiffuseInput AdvectDiffuseInput { get => _advectDiffuseInput; }
        public LagrangianInput LagrangianInput {  get => _lagrangianInput; }

        public SmokeInput()
        {
            _data = new SmokeData();
            _globalSmokeInput = new GlobalSmokeInput();
            _advectDiffuseInput = new AdvectDiffuseInput();
            _lagrangianInput = new LagrangianInput();
        }

        public void Parse(string[] inputLines, int startIndex, Dictionary<string, int> headerLineIndex, WeatherInput weatherInput, string rootFolder, out bool success)
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
            if (!success)
            {
                return;
            }

            //critical
            nameOfInput = nameof(Module);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                switch (userInput)
                {
                    case nameof(SmokeModules.GlobalSmoke):
                        Module = SmokeModules.GlobalSmoke;
                        break;
                    case nameof(SmokeModules.AdvectDiffuseMixingLayer):
                        Module = SmokeModules.AdvectDiffuseMixingLayer;
                        break;
                    case nameof(SmokeModules.AdvectDiffuse3D):
                        Module = SmokeModules.AdvectDiffuse3D;
                        break;
                    case nameof(SmokeModules.Lagrangian):
                        Module = SmokeModules.Lagrangian;
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
                PREACTInput.InputNotFoundMessage(nameOfInput, true);
            }
            if(issues > 0)
            {
                success = false;
                return;
            }

            //check if weather exists
            if (weatherInput.WeatherFile == string.Empty && (Module != SmokeModules.None || Module != SmokeModules.GlobalSmoke))
            {
                success = false;
                PREACTInput.CriticalDependency(nameof(weatherInput.WeatherFile));
                return;
            }

            //critical
            if (Module == SmokeModules.GlobalSmoke)
            {
                int lineIndex;
                nameOfInput = nameof(SmokeModules.GlobalSmoke);
                if (headerLineIndex.TryGetValue(nameOfInput, out lineIndex))
                {
                    PREACTInput.ReadingInputMessage(nameOfInput);
                    _globalSmokeInput = GlobalSmokeInput.Parse(inputLines, lineIndex, rootFolder, this, out success);
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, true);
                }
                if (!success)
                {
                    return;
                }
            }

            //critical
            if (Module == SmokeModules.AdvectDiffuseMixingLayer)
            {
                int lineIndex;
                nameOfInput = nameof(SmokeModules.AdvectDiffuseMixingLayer);
                if (headerLineIndex.TryGetValue(nameOfInput, out lineIndex))
                {
                    PREACTInput.ReadingInputMessage(nameOfInput);
                    _advectDiffuseInput = AdvectDiffuseInput.Parse(inputLines, lineIndex, out success);
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, true);
                }
                if (!success)
                {
                    return;
                }
            }

            if (Module == SmokeModules.AdvectDiffuse3D)
            {
                int lineIndex;
                nameOfInput = nameof(SmokeModules.AdvectDiffuse3D);
                if (headerLineIndex.TryGetValue(nameOfInput, out lineIndex))
                {
                    PREACTInput.ReadingInputMessage(nameOfInput);
                    _advectDiffuseInput = AdvectDiffuseInput.Parse(inputLines, lineIndex, out success);
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, true);
                }
                if (!success)
                {
                    Engine.Message(null, Engine.LogType.InputError, "The AdvectDiffuse3D model cannot find all needed input parameters.");
                    return;
                }
            }

            //critical
            if (Module == SmokeModules.Lagrangian)
            {
                int lineIndex;
                nameOfInput = nameof(SmokeModules.Lagrangian);
                if (headerLineIndex.TryGetValue(nameOfInput, out lineIndex))
                {
                    PREACTInput.ReadingInputMessage(nameOfInput);
                    _lagrangianInput = LagrangianInput.Parse(inputLines, lineIndex, out success);
                }
                else
                {                    
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, true);
                }
                if (!success)
                {
                    return;
                }
            }

            _data.LoadAll(this, rootFolder, out success);
            return;
        }
    }    
}

    