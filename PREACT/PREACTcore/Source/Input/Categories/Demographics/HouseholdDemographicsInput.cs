using System.Collections.Generic;
using PREACT.Input;

namespace PREACT.Evacuation
{
    public class HouseholdDemographicsInput
    {
        public string Name = string.Empty;
        public string HouseholdDemographicsFile = string.Empty;

        public static void Parse(Dictionary<string, HouseholdDemographicsInput> newInputs, string[] inputLines, List<int> householdDemographicsLineIndices, string rootFolder, out bool success)
        {
            success = false;
            newInputs.Clear();

            for (int i = 0; i < householdDemographicsLineIndices.Count; ++i)
            {
                HouseholdDemographicsInput newInput = new HouseholdDemographicsInput();
                success = false;
                int issues = 0;
                Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, householdDemographicsLineIndices[i]);
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

                nameOfInput = nameof(HouseholdDemographicsFile);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    newInput.HouseholdDemographicsFile = userInput;
                    PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success, true);
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

                newInputs.Add(newInput.Name, newInput);
            }

            if (newInputs.Count == householdDemographicsLineIndices.Count)
            {
                success = true;
            }
            else
            {
                Engine.Message(null, Engine.LogType.InputError, "Could not read all specified Household demographics.");
            }
        }
    }
}
