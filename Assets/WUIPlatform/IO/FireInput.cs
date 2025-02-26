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
        public FireModuleChoice fireModuleChoice = FireModuleChoice.AscImport;
        public string lcpFile;
        public string graphicalFireInputFile;

        public AscImportInput ascImportInput;
        public FireCellInput fireCellInput;        

        public static FireInput Parse(string[] inputLines, int startIndex)
        {
            int issues = 0;
            FireInput newInput = new FireInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);
            string temp;

            if (inputToParse.TryGetValue(nameof(fireModuleChoice), out temp))
            {
                switch (temp)
                {
                    case nameof(FireModuleChoice.AscImport):
                        newInput.fireModuleChoice = FireModuleChoice.AscImport;
                        break;
                    case nameof(FireModuleChoice.FireCell):
                        newInput.fireModuleChoice = FireModuleChoice.FireCell;
                        break;
                    case nameof(FireModuleChoice.FireCell2):
                        newInput.fireModuleChoice = FireModuleChoice.FireCell2;
                        break;
                    default:
                        newInput.fireModuleChoice = FireModuleChoice.AscImport;
                        break;
                }
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, "No fire module was set, using " + nameof(FireModuleChoice.AscImport) + ".");
            }            

            return newInput;
        }
    }

    [System.Serializable]
    public class AscImportInput
    {
        public string rootFolder;
        public string timeOfArrival;
        public string rateOfSpread;
        public string spreadDirection;
        public string firelineIntensity;
        public string weatherStream;
    }

    [System.Serializable]
    public class FireCellInput
    {
        public string rootFolder;
        public Fire.SpreadMode spreadMode = Fire.SpreadMode.SixteenDirections;
        public string fuelModelsFile = "default.fuel";
        public string initialFuelMoistureFile = "default.fmc";
        public string weatherFile = "default.wtr";
        public string windFile = "default.wnd";
        public string ignitionPointsFile = "default.ign";
        public float windMultiplier = 1f;
        public bool useRandomIgnitionMap = false;
        public int randomIgnitionPoints = 0;
        public bool useInitialIgnitionMap = false;
    }
}
   