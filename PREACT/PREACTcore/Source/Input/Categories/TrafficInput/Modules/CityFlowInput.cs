
using System.Collections.Generic;
using System.IO;
using PREACT.Math;

namespace PREACT.IO
{
    public class CityFlowInput
    {
        public string ConfigurationFile;

        public static CityFlowInput Parse(string[] inputLines, int startIndex, string rootFolder, out bool success)
        {
            success = false;
            int issues = 0;
            CityFlowInput newInput = new CityFlowInput();
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string input, userInput;

            input = nameof(ConfigurationFile);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                newInput.ConfigurationFile = userInput;
                PREACTInput.CheckIfFileExist(input, userInput, rootFolder, out success);
            }
            else
            {
                ++issues;
                PREACTInput.InputNotFoundMessage(input, true);
            }            

            if (issues == 0)
            {
                success = true;
            }
            return newInput;
        }
    }
}
