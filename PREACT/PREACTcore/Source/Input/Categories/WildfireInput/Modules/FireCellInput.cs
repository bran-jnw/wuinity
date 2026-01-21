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
        public enum SpreadRateModels { BehavePlus, CanadianFBP }
        public enum SpreadModes { FourDirections, EightDirections, SixteenDirections }

        public SpreadRateModels SpreadRateModel = SpreadRateModels.BehavePlus;
        public SpreadModes SpreadMode = SpreadModes.SixteenDirections;
        public string RootFolder = string.Empty;
        public string FuelModelsFile = string.Empty;
        public string InitialFuelMoistureFile = string.Empty;
        public string WeatherFile = string.Empty;
        public string WindFile = string.Empty;
        public string IgnitionPointsFile = string.Empty;
        public bool UseRandomIgnitionMap = false;
        public int RandomIgnitionPoints = 0;
        public bool UseInitialIgnitionMap = false;

        public FireCellInput()
        {

        }

        public void Parse(string[] inputLines, int startIndex, WildfireModuleInput fireInput, string rootFolder, out bool success)
        {
            success = false;
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string nameOfInput, userInput;

            //critical
            nameOfInput = nameof(SpreadRateModel);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                success = true;
                switch (userInput)
                {
                    case nameof(SpreadRateModels.BehavePlus):
                        SpreadRateModel = SpreadRateModels.BehavePlus;
                        break;
                    case nameof(SpreadRateModels.CanadianFBP):
                        SpreadRateModel = SpreadRateModels.CanadianFBP;
                        break;
                    default:
                        success = false;
                        PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                        break;
                }
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

            nameOfInput = nameof(SpreadMode);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                switch (userInput)
                {
                    case nameof(SpreadModes.FourDirections):
                        SpreadMode = SpreadModes.FourDirections;
                        break;
                    case nameof(SpreadModes.EightDirections):
                        SpreadMode = SpreadModes.EightDirections;
                        break;
                    case nameof(SpreadModes.SixteenDirections):
                        SpreadMode = SpreadModes.SixteenDirections;
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
                RootFolder = userInput;
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
                FuelModelsFile = userInput;
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
                InitialFuelMoistureFile = userInput;
                PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success);
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
            nameOfInput = nameof(WeatherFile);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                WeatherFile = userInput;
                PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success);
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
            nameOfInput = nameof(WindFile);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                WindFile = userInput;
                PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success);
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

            //maybe critical
            nameOfInput = nameof(IgnitionPointsFile);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                IgnitionPointsFile = userInput;
                PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }
            if (!success)
            {
                return;
            }

            //not critical
            nameOfInput = nameof(UseRandomIgnitionMap);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                bool.TryParse(userInput, out UseRandomIgnitionMap);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }

            //maybe critical                
            nameOfInput = nameof(RandomIgnitionPoints);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                int.TryParse(userInput, out RandomIgnitionPoints);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }
            if (RandomIgnitionPoints == 0 && UseRandomIgnitionMap)
            {
                return;
            }

            //might be critical if we have not loaded a proper map in fire input
            nameOfInput = nameof(UseInitialIgnitionMap);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                bool.TryParse(userInput, out UseInitialIgnitionMap);
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }
            if (UseInitialIgnitionMap && fireInput.GraphicalFireInputFile != string.Empty)
            {
                success = false;
                return;
            }

            success = true;
        }
    }   
}