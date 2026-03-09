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
        public enum CentroidModes { Center, Random, RandomCross, RandomCircle }
        public enum SpreadRateModels { Behave, CanadianFBP, LookupROS }
        public enum IgnitionTypes { Point, Polygon, Random }
        public enum SpreadModes { FourDirections, EightDirections, SixteenDirections }


        public SpreadRateModels SpreadRateModel = SpreadRateModels.Behave;

        public SpreadModes SpreadMode = SpreadModes.SixteenDirections;

        //behave
        public string FuelModelsFile = string.Empty;
        public string InitialFuelMoistureFile = string.Empty;

        //canadian
        public string FBPLookupTableFile = string.Empty;
        public double StartFFMC = 85.0;
        public double StartDMC = 6.0;
        public double StartDC = 15.0;
        public double StartHourlyFFMC = 85.0;

        //australian
        public string AFDRSLookupTableFile = string.Empty;
        public double StartKBDI = 100.0;
        public double MeanAnnualPrcp = 1000.0;

        //simple
        public string LookUpTableFile = string.Empty;

        //common
        public CentroidModes CentroidMode = CentroidModes.Random;
        public double RandomAmount = 0.5;
        public double ThetaLimit = 5.0;
        public string LandscapeFile = string.Empty;
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
            nameOfInput = nameof(LandscapeFile);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                LandscapeFile = userInput;
                PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success);
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

            //not critical
            nameOfInput = nameof(CentroidMode);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                success = true;
                switch (userInput)
                {
                    case nameof(CentroidModes.Center):
                        CentroidMode = CentroidModes.Center;
                        break;
                    case nameof(CentroidModes.Random):
                        CentroidMode = CentroidModes.Random;
                        break;
                    case nameof(CentroidModes.RandomCross):
                        CentroidMode = CentroidModes.RandomCross;
                        break;
                    case nameof(CentroidModes.RandomCircle):
                        CentroidMode = CentroidModes.RandomCircle;
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
                PREACTInput.InputNotFoundMessage(nameOfInput, false, CentroidMode.ToString());
            }
            success = true;

            //not critical, uses defaults
            nameOfInput = nameof(RandomAmount);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                success = double.TryParse(userInput, out RandomAmount);
                if (!success)
                {
                    PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                }
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput, false, RandomAmount.ToString());
            }
            success = true;

            //not critical, uses defaults
            nameOfInput = nameof(ThetaLimit);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                success = double.TryParse(userInput, out ThetaLimit);
                if (!success)
                {
                    PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                }
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput, false, RandomAmount.ToString());
            }
            success = true;

            //critical
            nameOfInput = nameof(SpreadRateModel);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                success = true;
                switch (userInput)
                {
                    case nameof(SpreadRateModels.Behave):
                        SpreadRateModel = SpreadRateModels.Behave;
                        break;
                    case nameof(SpreadRateModels.CanadianFBP):
                        SpreadRateModel = SpreadRateModels.CanadianFBP;
                        break;
                    case nameof(SpreadRateModels.LookupROS):
                        SpreadRateModel = SpreadRateModels.LookupROS;
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
                PREACTInput.InputNotFoundMessage(nameOfInput, true);
            }
            if(!success)
            {
                return;
            }

            if (SpreadRateModel == SpreadRateModels.Behave)
            {
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
            }
            else if (SpreadRateModel == SpreadRateModels.CanadianFBP)
            {
                nameOfInput = nameof(FBPLookupTableFile);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    FBPLookupTableFile = userInput;
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

                //not critical, uses defaults
                nameOfInput = nameof(StartDC);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    success = double.TryParse(userInput, out StartDC);
                    if (!success)
                    {
                        PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                    }
                }
                else
                {
                    PREACTInput.InputNotFoundMessage(nameOfInput);
                }

                //not critical, uses defaults
                nameOfInput = nameof(StartDMC);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    success = double.TryParse(userInput, out StartDMC);
                    if (!success)
                    {
                        PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                    }
                }
                else
                {
                    PREACTInput.InputNotFoundMessage(nameOfInput);
                }

                //not critical, uses defaults
                nameOfInput = nameof(StartFFMC);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    success = double.TryParse(userInput, out StartFFMC);
                    if (!success)
                    {
                        PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                    }
                }
                else
                {
                    PREACTInput.InputNotFoundMessage(nameOfInput);
                }

                //not critical, uses defaults
                nameOfInput = nameof(StartHourlyFFMC);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    success = double.TryParse(userInput, out StartHourlyFFMC);
                    if (!success)
                    {
                        PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                    }
                }
                else
                {
                    PREACTInput.InputNotFoundMessage(nameOfInput);
                }
            }
            else if (SpreadRateModel == SpreadRateModels.LookupROS)
            {
                //critical
                nameOfInput = nameof(LookUpTableFile);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    LookUpTableFile = userInput;
                    PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success, false);                    
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

            //maybe critical
            nameOfInput = nameof(IgnitionPointsFile);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                IgnitionPointsFile = userInput;
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