//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;

namespace PREACT.IO
{
    [System.Serializable]
    public class FireCellInput
    {
        public enum SpreadModeEnum { FourDirections, EightDirections, SixteenDirections }

        public SpreadModeEnum SpreadMode = SpreadModeEnum.SixteenDirections;
        public string RootFolder = string.Empty;
        public string FuelModelsFile = string.Empty;
        public string InitialFuelMoistureFile = string.Empty;
        public string WeatherFile = string.Empty;
        public string WindFile = string.Empty;
        public string IgnitionPointsFile = string.Empty;
        public float WindMultiplier = 1f;
        public bool UseRandomIgnitionMap = false;
        public int RandomIgnitionPoints = 0;
        public bool UseInitialIgnitionMap = false;

        public FireCellInput()
        {

        }

        public static FireCellInput Parse(string[] inputLines, int startIndex, FireInput fireInput, string rootFolder, out bool success)
        {
            success = false;
            FireCellInput newInput = new FireCellInput();
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string nameOfInput, userInput;

            nameOfInput = nameof(SpreadMode);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                switch (userInput)
                {
                    case nameof(SpreadModeEnum.FourDirections):
                        newInput.SpreadMode = SpreadModeEnum.FourDirections;
                        break;
                    case nameof(SpreadModeEnum.EightDirections):
                        newInput.SpreadMode = SpreadModeEnum.EightDirections;
                        break;
                    case nameof(SpreadModeEnum.SixteenDirections):
                        newInput.SpreadMode = SpreadModeEnum.SixteenDirections;
                        break;
                    default:
                        Engine.Message(null, Engine.LogType.SimulationError, nameOfInput + " was not recognized." + PREACTInput.pleaseCheckInput);
                        break;
                }
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            //not critical as it can be empty
            nameOfInput = nameof(RootFolder);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                newInput.RootFolder = userInput;
                rootFolder = Path.Combine(rootFolder, userInput);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }            

            //not critical, uses defaults
            nameOfInput = nameof(FuelModelsFile);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                newInput.FuelModelsFile = userInput;
                PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            //critical
            nameOfInput = nameof(InitialFuelMoistureFile);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                newInput.InitialFuelMoistureFile = userInput;
                PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success);
            }
            else
            {
                success = false;
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }
            if (!success)
            {
                return newInput;
            }

            //critical
            nameOfInput = nameof(WeatherFile);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                newInput.WeatherFile = userInput;
                PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success);
            }
            else
            {
                success = false;
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }
            if (!success)
            {
                return newInput;
            }

            //critical
            nameOfInput = nameof(WindFile);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                newInput.WindFile = userInput;
                PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success);
            }
            else
            {
                success = false;
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }
            if (!success)
            {
                return newInput;
            }

            //maybe critical
            nameOfInput = nameof(IgnitionPointsFile);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                newInput.IgnitionPointsFile = userInput;
                PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }
            if (!success)
            {
                return newInput;
            }

            //not critical
            nameOfInput = nameof(WindMultiplier);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                float.TryParse(userInput, out newInput.WindMultiplier);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            //not critical
            nameOfInput = nameof(UseRandomIgnitionMap);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                bool.TryParse(userInput, out newInput.UseRandomIgnitionMap);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            //maybe critical                
            nameOfInput = nameof(RandomIgnitionPoints);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                int.TryParse(userInput, out newInput.RandomIgnitionPoints);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }
            if (newInput.RandomIgnitionPoints == 0 && newInput.UseRandomIgnitionMap)
            {
                return newInput;
            }

            //might be critical if we have not loaded a proper map in fire input
            nameOfInput = nameof(UseInitialIgnitionMap);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                bool.TryParse(userInput, out newInput.UseInitialIgnitionMap);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }
            if (newInput.UseInitialIgnitionMap && fireInput.GraphicalFireInputFile != string.Empty)
            {
                success = false;
                return newInput;
            }

            success = true;
            return newInput;
        }
    }   
}