using System.Collections.Generic;
using PREACT.Input;

namespace PREACT.Evacuation
{
    public class DemographicsInput
    {
        public string Name = string.Empty;
        public string HouseholdDemographics = string.Empty;
        public string VehicleDemographics = string.Empty;
        public bool Default = false;


        public DemographicsInput() 
        {
            
        }

        public static void Parse(Dictionary<string, HouseholdDemographicsInput> householdDemographicsInput, Dictionary<string, VehicleDemographicsInput> vehicleDemographicsInput, Dictionary<string, DemographicsInput> newInputs, string[] inputLines, List<int> demographicsLineIndices, string rootFolder, out bool success)
        {
            success = false;
            newInputs.Clear();

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

                nameOfInput = nameof(HouseholdDemographics);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    newInput.HouseholdDemographics = userInput;
                    success = householdDemographicsInput.TryGetValue(userInput, out HouseholdDemographicsInput householdDemographics);
                    if(!success)
                    {
                        PREACTInput.MissingReferenceToOtherInput(nameOfInput, userInput);
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

                nameOfInput = nameof(VehicleDemographics);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    newInput.VehicleDemographics = userInput;
                    success = vehicleDemographicsInput.TryGetValue(userInput, out VehicleDemographicsInput vehicleDemographics);
                    if (!success)
                    {
                        PREACTInput.MissingReferenceToOtherInput(nameOfInput, userInput);
                    }
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput);
                }               
                if(!success)
                {
                    continue;
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
        }
    }
}
