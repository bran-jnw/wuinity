//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Numerics;
using WUIPlatform.Traffic;
using System.Collections.Generic;
using System.IO;

namespace WUIPlatform.IO
{
    [System.Serializable]
    public class SmokeInput
    {
        public enum SmokeModuleChoice { GlobalSmoke, AdvectDiffuse, BoxModel, Lagrangian, GaussianPuff, GaussianPlume, FFD }
        public SmokeModuleChoice smokeModuleChoice = SmokeModuleChoice.GlobalSmoke;

        public GlobalSmokeInput globalSmokeInput;
        public AdvectDiffuseInput advectDiffuseInput;
        public LagrangianInput lagrangianInput;

        public static SmokeInput Parse(string[] inputLines, int startIndex)
        {
            int issues = 0;
            SmokeInput newInput = new SmokeInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);
            string temp;

            if (inputToParse.TryGetValue(nameof(smokeModuleChoice), out temp))
            {
                switch (temp)
                {
                    case nameof(SmokeModuleChoice.GlobalSmoke):
                        newInput.smokeModuleChoice = SmokeModuleChoice.GlobalSmoke;
                        break;
                    case nameof(SmokeModuleChoice.AdvectDiffuse):
                        newInput.smokeModuleChoice = SmokeModuleChoice.AdvectDiffuse;
                        break;
                    case nameof(SmokeModuleChoice.Lagrangian):
                        newInput.smokeModuleChoice = SmokeModuleChoice.Lagrangian;
                        break;
                    default:
                        newInput.smokeModuleChoice = SmokeModuleChoice.GlobalSmoke;
                        break;
                }
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, "No smoke module was selected, using " + newInput.smokeModuleChoice.ToString());
            }

            return newInput;
        }
    }

    public class GlobalSmokeInput
    {
        public Vector2[] timeSeries;
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

    