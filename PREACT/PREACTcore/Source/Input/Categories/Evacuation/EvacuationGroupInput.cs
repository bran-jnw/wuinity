using PREACT.Input;
using PREACT.Math;
using System;
using System.Collections.Generic;
using PREACT.Utility.Math;

namespace PREACT.Evacuation
{
    public enum DestinationChoices { Random, ClosestEuclidean, EvacGroupProbability, EvacGroupClosestEuclidean };

    public class EvacuationGroupInput
    {
        public string Name = string.Empty;
        public PREACTColor Color = PREACTColor.white;
        public DestinationChoices DestinationChoice = DestinationChoices.EvacGroupProbability;
        public List<(string, double)> Destinations = new List<(string, double)>(8);
        public List<(string, double)> EvacuationBehaviors = new List<(string, double)>(8);
        public string Demographics = string.Empty;
        public string ShapeFile = string.Empty;
        public bool Default = false;

        public EvacuationGroupInput()
        {

        }

        public static void Parse(Dictionary<string, EvacuationGroupInput> newInputs, string[] inputLines, List<int> evacGroupLineIndices, 
            Dictionary<string, EvacuationDestinationInput> destinationInputs, Dictionary<string, ResponseCurve> responseCurves, Dictionary<string, EvacuationBehaviorInput> evacBehaviorInputs,
            SimulationInput simulation, PopulationInput population, string rootFolder, out bool success)
        {            
            success = false;
            newInputs.Clear();

            for (int i = 0; i < evacGroupLineIndices.Count; ++i)
            {
                EvacuationGroupInput newInput = new EvacuationGroupInput();
                success = false;
                int issues = 0;
                Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, evacGroupLineIndices[i]);
                string nameOfInput, userInput;

                //critical
                nameOfInput = nameof(Name);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    newInput.Name = userInput;
                    success = true;
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, true);
                }
                if (!success)
                {
                    break;
                }

                //critical
                nameOfInput = nameof(DestinationChoice);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    switch (userInput)
                    {
                        case nameof(DestinationChoices.Random):
                            newInput.DestinationChoice = DestinationChoices.Random;
                            break;
                        case nameof(DestinationChoices.ClosestEuclidean):
                            newInput.DestinationChoice = DestinationChoices.ClosestEuclidean;
                            break;
                        case nameof(DestinationChoices.EvacGroupProbability):
                            newInput.DestinationChoice = DestinationChoices.EvacGroupProbability;
                            break;
                        case nameof(DestinationChoices.EvacGroupClosestEuclidean):
                            newInput.DestinationChoice = DestinationChoices.EvacGroupClosestEuclidean;
                            break;
                        default:
                            ++issues;
                            Engine.Message(null, Engine.LogType.SimulationError, nameOfInput + " was not recognized." + PREACTInput.pleaseCheckInput);
                            break;
                    }
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput);
                }
                if (!success)
                {
                    break;
                }

                //not critical, uses default
                nameOfInput = nameof(Demographics);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    newInput.Demographics = userInput;
                    if (population.Demographics.ContainsKey(userInput))
                    {
                        success = true;
                    }
                    else
                    {
                        success = false;
                        PREACTInput.MissingReferenceToOtherInput(nameOfInput, userInput);
                    }
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, true);
                }
                if (!success)
                {
                    newInput.Demographics = string.Empty;
                }


                //maybe critical
                nameOfInput = nameof(Destinations);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    success = ReferenceProbabilityPairs.TryParse(userInput, newInput.Destinations);
                    if(success)
                    {
                        //validate data
                        for (int j = 0; j < newInput.Destinations.Count; ++j)
                        {
                            if (!destinationInputs.ContainsKey(newInput.Destinations[j].Item1))
                            {
                                success = false;
                                PREACTInput.MissingReferenceToOtherInput(nameOfInput, newInput.Destinations[j].Item1);
                            }
                        }
                    }
                    else
                    {
                        success = false;
                        PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                    }
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, true);
                }
                if (!success && (newInput.DestinationChoice == DestinationChoices.EvacGroupProbability || newInput.DestinationChoice == DestinationChoices.EvacGroupClosestEuclidean))
                {
                    break;
                }

                //critical
                nameOfInput = nameof(EvacuationBehaviors);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    success = ReferenceProbabilityPairs.TryParse(userInput, newInput.EvacuationBehaviors);
                    if (success)
                    {
                        //validate data
                        for (int j = 0; j < newInput.EvacuationBehaviors.Count; ++j)
                        {
                            if (!evacBehaviorInputs.ContainsKey(newInput.EvacuationBehaviors[j].Item1))
                            {
                                success = false;
                                PREACTInput.MissingReferenceToOtherInput(nameOfInput, newInput.EvacuationBehaviors[j].Item1);
                            }
                        }
                    }
                    else
                    {
                        success = false;
                        PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                    }
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, true);
                }
                if (!success)
                {
                    break;
                }

                //maybe critical
                nameOfInput = nameof(ShapeFile);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    newInput.ShapeFile = userInput;
                    PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success);
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, true);
                }
                if (!success && evacGroupLineIndices.Count > 1) //if only one then it is not critical
                {
                    break;
                }

                //not critical
                if (newInputs.Count == 0)
                {
                    newInput.Default = true;
                }
                else
                {
                    nameOfInput = nameof(Default);
                    if (inputToParse.TryGetValue(nameOfInput, out userInput))
                    {
                        success = bool.TryParse(userInput, out newInput.Default);
                    }
                    else
                    {
                        success = false;
                        PREACTInput.InputNotFoundMessage(nameOfInput);
                    }
                    if (!success)
                    {
                        newInput.Default = false;
                    }
                    else if (newInput.Default)
                    {
                        foreach (EvacuationGroupInput prevInput in newInputs.Values)
                        {
                            prevInput.Default = false;
                        }
                    }
                }

                //not critical
                nameOfInput = nameof(Color);
                success = true;
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    string[] data = userInput.Split(',');
                    if (data.Length == 3)
                    {
                        issues += float.TryParse(data[0], out newInput.Color.r) ? 0 : 1;
                        issues += float.TryParse(data[1], out newInput.Color.g) ? 0 : 1;
                        issues += float.TryParse(data[2], out newInput.Color.b) ? 0 : 1;
                    }
                    else
                    {
                        issues++;
                    }
                    if (issues > 0)
                    {
                        PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                    }
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput);
                }
                if (!success || issues > 0)
                {
                    newInput.Color = PREACTColor.Random();
                }

                newInputs.Add(newInput.Name, newInput);
            }
            
            if(newInputs.Count == evacGroupLineIndices.Count)
            {
                success = true;
            }
            else
            {
                Engine.Message(null, Engine.LogType.InputError, "Could not read all specified Destinations.");
            }
        }
    }
}
