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
    public class FireCellInput
    {
        public enum SpreadModeEnum { FourDirections, EightDirections, SixteenDirections }
        public SpreadModeEnum SpreadMode = SpreadModeEnum.SixteenDirections;
        public string RootFolder;
        public string FuelModelsFile;
        public string InitialFuelMoistureFile;
        public string WeatherFile;
        public string WindFile;
        public string IgnitionPointsFile;
        public float WindMultiplier = 1f;
        public bool UseRandomIgnitionMap = false;
        public int RandomIgnitionPoints = 0;
        public bool UseInitialIgnitionMap = false;

        public static FireCellInput Parse(string[] inputLines, int startIndex)
        {
            int issues = 0;
            FireCellInput newInput = new FireCellInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);
            string input, userInput;

            input = nameof(SpreadMode);
            if (inputToParse.TryGetValue(input, out userInput))
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
                        ++issues;
                        WUIEngine.LOG(WUIEngine.LogType.SimError, input + " was not recognized." + WUIEngineInput.pleaseCheckInput);
                        break;
                }
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            input = nameof(RootFolder);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                newInput.RootFolder = userInput;
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            input = nameof(FuelModelsFile);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                newInput.FuelModelsFile = userInput;
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            input = nameof(InitialFuelMoistureFile);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                newInput.FuelModelsFile = userInput;
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            input = nameof(WeatherFile);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                newInput.FuelModelsFile = userInput;
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            input = nameof(WindFile);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                newInput.FuelModelsFile = userInput;
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            input = nameof(IgnitionPointsFile);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                newInput.FuelModelsFile = userInput;
            }
            else
            {
                ++issues;
                WUIEngineInput.InputNotFoundMessage(input);
            }

            input = nameof(WindMultiplier);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                float.TryParse(userInput, out newInput.WindMultiplier);
            }
            else
            {
                WUIEngineInput.InputNotFoundMessage(input);
            }

            input = nameof(UseRandomIgnitionMap);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                bool.TryParse(userInput, out newInput.UseRandomIgnitionMap);
            }
            else
            {
                WUIEngineInput.InputNotFoundMessage(input);
            }

            input = nameof(RandomIgnitionPoints);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                int.TryParse(userInput, out newInput.RandomIgnitionPoints);
            }
            else
            {
                WUIEngineInput.InputNotFoundMessage(input);
            }

            input = nameof(UseInitialIgnitionMap);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                bool.TryParse(userInput, out newInput.UseRandomIgnitionMap);
            }
            else
            {
                WUIEngineInput.InputNotFoundMessage(input);
            }

            return newInput;
        }
    }   
}