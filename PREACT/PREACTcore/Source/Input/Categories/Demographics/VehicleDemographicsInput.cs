using System.Collections.Generic;
using PREACT.Input;

namespace PREACT.Evacuation
{
    public class VehicleDemographicsInput
    {
        public string Name = string.Empty;
        public string VehicleDemographicsFile = string.Empty;

        public static void Parse(Dictionary<string, VehicleDemographicsInput> newInputs, string[] inputLines, List<int> vehicleDemographicsLineIndices, string rootFolder, out bool success)
        {
            success = false;
            newInputs.Clear();

            for (int i = 0; i < vehicleDemographicsLineIndices.Count; ++i)
            {
                VehicleDemographicsInput newInput = new VehicleDemographicsInput();
                success = false;
                int issues = 0;
                Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, vehicleDemographicsLineIndices[i]);
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

                nameOfInput = nameof(VehicleDemographicsFile);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    newInput.VehicleDemographicsFile = userInput;
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

            if (newInputs.Count == vehicleDemographicsLineIndices.Count)
            {
                success = true;
            }
            else
            {
                Engine.Message(null, Engine.LogType.InputError, "Could not read all specified Vehicle demographics.");
            }
        }
    }
}
