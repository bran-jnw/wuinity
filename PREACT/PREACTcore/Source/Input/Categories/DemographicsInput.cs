using System.Collections.Generic;
using PREACT.IO;

namespace PREACT.Evacuation
{
    public class DemographicsInput
    {
        public string Name = string.Empty;
        public bool AllowMoreThanOneCar = true;
        public int MaxCars = 2;
        public float MaxCarsProbability = 0.3f;
        public bool Default = false;

        public DemographicsInput() 
        {
            
        }

        public static Dictionary<string, DemographicsInput> Parse(string[] inputLines, List<int> demographicsLineIndices, out bool success)
        {
            Dictionary<string, DemographicsInput> newInputs = new Dictionary<string, DemographicsInput>();
            success = false;

            for (int i = 0; i < demographicsLineIndices.Count; ++i)
            {
                DemographicsInput newInput = new DemographicsInput();
                success = false;
                int issues = 0;
                Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, demographicsLineIndices[i]);
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

                nameOfInput = nameof(AllowMoreThanOneCar);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    bool.TryParse(userInput, out newInput.AllowMoreThanOneCar);
                }
                else
                {
                    PREACTInput.InputNotFoundMessage(nameOfInput);
                }

                nameOfInput = nameof(MaxCars);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    int.TryParse(userInput, out newInput.MaxCars);
                }
                else
                {
                    PREACTInput.InputNotFoundMessage(nameOfInput);
                }

                nameOfInput = nameof(MaxCarsProbability);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    float.TryParse(userInput, out newInput.MaxCarsProbability);
                }
                else
                {
                    PREACTInput.InputNotFoundMessage(nameOfInput);
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
                        foreach (DemographicsInput prevInput in newInputs.Values)
                        {
                            prevInput.Default = false;
                        }
                    }
                }

                newInputs.Add(newInput.Name, newInput);
            }

            if (newInputs.Count == demographicsLineIndices.Count)
            {
                success = true;
            }
            else
            {
                Engine.Message(null, Engine.LogType.InputError, "Could not read all specified Demographics.");
            }
            return newInputs;
        }
    }
}
