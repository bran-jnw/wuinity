//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Numerics;
using System.Collections.Generic;

namespace WUIPlatform.IO
{
    [System.Serializable]
    public class SmokeInput
    {
        public enum SmokeModuleChoice { GlobalSmoke, AdvectDiffuse, BoxModel, Lagrangian, GaussianPuff, GaussianPlume, FFD }
        public SmokeModuleChoice SmokeModule = SmokeModuleChoice.GlobalSmoke;

        public GlobalSmokeInput GlobalSmokeInput;
        public AdvectDiffuseInput AdvectDiffuseInput;
        public LagrangianInput LagrangianInput;

        public static SmokeInput Parse(string[] inputLines, int startIndex, Dictionary<string, int> headerLineIndex)
        {
            int issues = 0;
            SmokeInput newInput = new SmokeInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);
            string input, userInput;

            input = nameof(SmokeModule);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                switch (userInput)
                {
                    case nameof(SmokeModuleChoice.GlobalSmoke):
                        newInput.SmokeModule = SmokeModuleChoice.GlobalSmoke;
                        break;
                    case nameof(SmokeModuleChoice.AdvectDiffuse):
                        newInput.SmokeModule = SmokeModuleChoice.AdvectDiffuse;
                        break;
                    case nameof(SmokeModuleChoice.Lagrangian):
                        newInput.SmokeModule = SmokeModuleChoice.Lagrangian;
                        break;
                    default:
                        ++issues;
                        WUIEngine.LOG(WUIEngine.LogType.Warning, input + " was not recognized." + WUIEngineInput.pleaseCheckInput);
                        break;
                }
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            if(newInput.SmokeModule == SmokeModuleChoice.GlobalSmoke)
            {
                int lineIndex;
                input = nameof(SmokeModuleChoice.GlobalSmoke);
                if (headerLineIndex.TryGetValue(input, out lineIndex))
                {
                    WUIEngineInput.ReadingInputMessage(input);
                    newInput.GlobalSmokeInput = GlobalSmokeInput.Parse(inputLines, lineIndex);
                }
                else
                {
                    //critical
                    WUIEngine.LOG(WUIEngine.LogType.SimError, nameof(SmokeModuleChoice.GlobalSmoke) + " header not found but user has requested this smoke module." + WUIEngineInput.pleaseCheckInput);
                    return null;
                }
            }

            return newInput;
        }
    }

    public class AdvectDiffuseInput
    {
        public float MixingLayerHeight = 250.0f;
    }

    public class LagrangianInput
    {
        public uint particlesPerFireCell = 50;
    }
}

    