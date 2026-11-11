using System;
using System.Collections.Generic;
using System.Text;

namespace PREACT.Input
{
    public class EvacuationBehaviorInput
    {
        public string Name = string.Empty;
        public List<(string, double)> AwakeResponseCurves = new List<(string, double)>(8);
        public List<(string, double)> AsleepResponseCurves = new List<(string, double)>(8);
        public List<(string, double)> DireResponseCurves = new List<(string, double)>(8);
        public List<string> ResponseTriggers = new List<string>(8);
        public double InformationAwareness = 1.0;
        public double InformationCompliance = 1.0;

        public EvacuationBehaviorInput()
        {
        }

        public static void Parse(Dictionary<string, EvacuationBehaviorInput> newInputs, string[] inputLines, List<int> evacuationBehaviorLineIndices, SimulationInput simulationInput, out bool success)
        {
            success = false;  
            newInputs.Clear();

            for(int i = 0; i < evacuationBehaviorLineIndices.Count; i++)
            {
                EvacuationBehaviorInput newInput = new EvacuationBehaviorInput();
                Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, evacuationBehaviorLineIndices[i], true);
                string nameOfInput, userInput;

                //critical
                nameOfInput = nameof(Name);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    newInput.Name = userInput;
                    success = !string.IsNullOrEmpty(newInput.Name);
                    if (!success)
                    {
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
                    return;
                }

                nameOfInput = nameof(AwakeResponseCurves);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    success = ReferenceProbabilityPairs.TryParse(userInput, newInput.AwakeResponseCurves);
                    if (!success)
                    {
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
                    return;
                }

                nameOfInput = nameof(AsleepResponseCurves);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    success = ReferenceProbabilityPairs.TryParse(userInput, newInput.AsleepResponseCurves);
                    if (!success)
                    {
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
                    return;
                }

                nameOfInput = nameof(DireResponseCurves);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    success = ReferenceProbabilityPairs.TryParse(userInput, newInput.DireResponseCurves);
                    if (!success)
                    {
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
                    return;
                }

                //not critical
                nameOfInput = nameof(ResponseTriggers);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    string[] triggers = userInput.Split(',');
                    newInput.ResponseTriggers.Clear();
                    for (int j = 0; j < triggers.Length; ++j)
                    {
                        newInput.ResponseTriggers.Add(triggers[j]);
                    }
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, false, string.Empty);
                }
                if (!success)
                {
                    return;
                }

                //not critical
                nameOfInput = nameof(InformationAwareness);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    success = double.TryParse(userInput, out newInput.InformationAwareness);
                    if (!success)
                    {
                        PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                    }
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, false, newInput.InformationAwareness.ToString());
                }
                if (!success)
                {
                    return;
                }

                //not critical
                nameOfInput = nameof(InformationCompliance);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    success = double.TryParse(userInput, out newInput.InformationCompliance);
                    if (!success)
                    {
                        PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                    }
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, false, newInput.InformationCompliance.ToString());
                }
                if (!success)
                {
                    return;
                }

                if(success)
                {
                    newInputs.Add(newInput.Name, newInput);
                }
            }

            success = true;
        }   
    }
}
