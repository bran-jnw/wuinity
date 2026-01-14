using System.Collections.Generic;
using System.IO;
using PREACT.Math;
using PREACT.IO;

namespace PREACT.Evacuation
{
    public enum DestinationChoices { Random, ClosestEuclidean, EvacGroupWeighted, EvacGroupClosestEuclidean };

    public class EvacuationGroupInput
    {
        public string Name = string.Empty;
        public PREACTColor Color = PREACTColor.white;
        public DestinationChoices DestinationChoice = DestinationChoices.EvacGroupWeighted;
        public List<string> Destinations = new List<string>(16);
        public List<double> DestinationsCDF = new List<double>(16);
        public List<string> ResponseCurves = new List<string>(16);
        public List<double> ResponseCurvesCDF = new List<double>(16);
        public string Demographics = string.Empty;
        public string ShapeFile = string.Empty;
        public bool Default = false;

        public EvacuationGroupInput()
        {

        }

        public static Dictionary<string, EvacuationGroupInput> Parse(string[] inputLines, SimulationInput simulationInput, List<int> evacGroupLineIndices, 
            Dictionary<string, EvacuationDestinationInput> destinationInputs, Dictionary<string, ResponseCurve> responseCurves, PopulationInput population, string rootFolder, out bool success)
        {
            Dictionary<string, EvacuationGroupInput> newInputs = new Dictionary<string, EvacuationGroupInput>();
            success = false;

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
                        case nameof(DestinationChoices.EvacGroupWeighted):
                            newInput.DestinationChoice = DestinationChoices.EvacGroupWeighted;
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
                    success = true;
                    string[] data = userInput.Split(',');
                    for (int j = 0; j < data.Length; ++j)
                    {
                        if (destinationInputs.ContainsKey(data[j]))
                        {
                            newInput.Destinations.Add(data[j]);
                        }
                        else
                        {
                            success = false;
                            PREACTInput.MissingReferenceToOtherInput(nameOfInput, data[j]);
                        }
                    }
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, true);
                }
                if (!success && (newInput.DestinationChoice == DestinationChoices.EvacGroupWeighted || newInput.DestinationChoice == DestinationChoices.EvacGroupClosestEuclidean))
                {
                    break;
                }

                //maybe critical
                if (newInput.Destinations.Count == 1)
                {
                    newInput.DestinationsCDF.Add(1.0);
                }
                else
                {
                    nameOfInput = nameof(DestinationsCDF);
                    if (inputToParse.TryGetValue(nameOfInput, out userInput))
                    {
                        string[] data = userInput.Split(',');
                        for (int j = 0; j < data.Length; ++j)
                        {
                            double cumulativeProbability;
                            success = double.TryParse(data[j], out cumulativeProbability);
                            if (success)
                            {
                                newInput.DestinationsCDF.Add(cumulativeProbability);
                            }
                            else
                            {
                                PREACTInput.CouldNotInterpretInputMessage(nameOfInput, data[j]);
                                break;
                            }
                        }
                    }
                    else
                    {
                        success = false;
                        PREACTInput.InputNotFoundMessage(nameOfInput, true);
                    }
                    if (newInput.Destinations.Count != newInput.DestinationsCDF.Count)
                    {
                        success = false;
                        PREACTInput.IncorrectInputCount(nameOfInput);
                    }
                    if (!success && (newInput.DestinationChoice == DestinationChoices.EvacGroupWeighted || newInput.DestinationChoice == DestinationChoices.EvacGroupClosestEuclidean))
                    {
                        break;
                    }
                }

                //critical
                nameOfInput = nameof(ResponseCurves);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    success = true;
                    string[] data = userInput.Split(',');
                    for (int j = 0; j < data.Length; ++j)
                    {
                        newInput.ResponseCurves.Add(data[j]);
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
                if (newInput.ResponseCurves.Count == 1)
                {
                    newInput.ResponseCurvesCDF.Add(1.0);
                }
                else
                {
                    nameOfInput = nameof(ResponseCurvesCDF);
                    if (inputToParse.TryGetValue(nameOfInput, out userInput))
                    {
                        string[] data = userInput.Split(',');
                        for (int j = 0; j < data.Length; ++j)
                        {
                            double cumulativeProbability;
                            success = double.TryParse(data[j], out cumulativeProbability);
                            if (success)
                            {
                                newInput.ResponseCurvesCDF.Add(cumulativeProbability);
                            }
                            else
                            {
                                PREACTInput.CouldNotInterpretInputMessage(nameOfInput, data[j]);
                                break;
                            }
                        }
                    }
                    else
                    {
                        success = false;
                        PREACTInput.InputNotFoundMessage(nameOfInput, true);
                    }
                    if (newInput.ResponseCurves.Count != newInput.ResponseCurvesCDF.Count)
                    {
                        success = false;
                        PREACTInput.IncorrectInputCount(nameOfInput);
                    }
                    if (!success)
                    {
                        break;
                    }
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
                Engine.Message(null, Engine.LogType.InputError, "Could not read all specified EvacuationGroups.");
            }
            return newInputs;
        }
    }
}
