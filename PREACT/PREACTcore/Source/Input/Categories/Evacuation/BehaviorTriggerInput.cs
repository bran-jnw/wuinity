using System;
using System.Collections.Generic;
using System.Text;

namespace PREACT.Input
{
    public class BehaviorTriggerInput
    {
        public enum TriggerType { NeighborInfluence, SpeciesDensity, WildfireDistance, WildfireTime }

        public string Name = string.Empty;
        public TriggerType Type;
        public string Threshold = string.Empty;      


        public BehaviorTriggerInput()
        {
        }

        public static void Parse(Dictionary<string, BehaviorTriggerInput> newInputs, string[] inputLines, List<int> evacuationBehaviorLineIndices, SimulationInput simulationInput, out bool success)
        {
            success = false;
            newInputs.Clear();

            for (int i = 0; i < evacuationBehaviorLineIndices.Count; i++)
            {
                BehaviorTriggerInput newInput = new BehaviorTriggerInput();
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

                nameOfInput = nameof(Type);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    success = Enum.TryParse<TriggerType>(userInput, out newInput.Type);
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

                newInputs.Add(newInput.Name, newInput);
            }

            success = true;
        }
    }
}
