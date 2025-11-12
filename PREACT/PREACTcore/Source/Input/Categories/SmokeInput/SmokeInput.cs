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
        public enum SmokeModuleChoice { None, GlobalSmoke, BoxModel, AdvectDiffuseMixingLayer, AdvectDiffuse3D, Lagrangian, GaussianPuff, GaussianPlume, FFD }

        private SmokeData _data;
        private GlobalSmokeInput _globalSmokeInput;
        private AdvectDiffuseInput _advectDiffuseInput;
        private LagrangianInput _lagrangianInput;

        public SmokeData Data { get { return _data; } } 
        public SmokeModuleChoice SmokeModule = SmokeModuleChoice.None;
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

        public static SmokeInput Parse(string[] inputLines, int startIndex, Dictionary<string, int> headerLineIndex, SimulationInput simulationInput, string rootFolder, out bool success)
        {
            SmokeInput newInput = new SmokeInput();
            if(!simulationInput.RunSmokeModule)
            {
                success = true;
                return newInput;
            }

            success = false;
            int issues = 0;            
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string nameOfInput, userInput;

            //critical
            nameOfInput = nameof(SmokeModule);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                switch (userInput)
                {
                    case nameof(SmokeModuleChoice.GlobalSmoke):
                        newInput.SmokeModule = SmokeModuleChoice.GlobalSmoke;
                        break;
                    case nameof(SmokeModuleChoice.AdvectDiffuseMixingLayer):
                        newInput.SmokeModule = SmokeModuleChoice.AdvectDiffuseMixingLayer;
                        break;
                    case nameof(SmokeModuleChoice.AdvectDiffuse3D):
                        newInput.SmokeModule = SmokeModuleChoice.AdvectDiffuse3D;
                        break;
                    case nameof(SmokeModuleChoice.Lagrangian):
                        newInput.SmokeModule = SmokeModuleChoice.Lagrangian;
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
                return newInput;
            }
                        
            //critical
            if (newInput.SmokeModule == SmokeModuleChoice.GlobalSmoke)
            {
                int lineIndex;
                nameOfInput = nameof(SmokeModuleChoice.GlobalSmoke);
                if (headerLineIndex.TryGetValue(nameOfInput, out lineIndex))
                {
                    PREACTInput.ReadingInputMessage(nameOfInput);
                    newInput._globalSmokeInput = GlobalSmokeInput.Parse(inputLines, lineIndex, rootFolder, newInput, out success);
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, true);
                }
                if (!success)
                {
                    return newInput;
                }
            }

            //critical
            if (newInput.SmokeModule == SmokeModuleChoice.AdvectDiffuseMixingLayer)
            {
                int lineIndex;
                nameOfInput = nameof(SmokeModuleChoice.AdvectDiffuseMixingLayer);
                if (headerLineIndex.TryGetValue(nameOfInput, out lineIndex))
                {
                    PREACTInput.ReadingInputMessage(nameOfInput);
                    newInput._advectDiffuseInput = AdvectDiffuseInput.Parse(inputLines, lineIndex, out success);
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, true);
                }
                if (!success)
                {
                    return newInput;
                }
            }

            ///critical
            if (newInput.SmokeModule == SmokeModuleChoice.Lagrangian)
            {
                int lineIndex;
                nameOfInput = nameof(SmokeModuleChoice.Lagrangian);
                if (headerLineIndex.TryGetValue(nameOfInput, out lineIndex))
                {
                    PREACTInput.ReadingInputMessage(nameOfInput);
                    newInput._lagrangianInput = LagrangianInput.Parse(inputLines, lineIndex, out success);
                }
                else
                {                    
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, true);
                }
                if (!success)
                {
                    return newInput;
                }
            }

            newInput._data.LoadAll(simulationInput, newInput, rootFolder, out success);
            return newInput;
        }
    }    
}

    