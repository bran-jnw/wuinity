using System;
using System.Collections.Generic;
using PREACT.Math;

namespace PREACT.Input
{
    public class WeatherInput
    {
        public string WeatherFile = string.Empty;
        public Vector2d DesiredLatLon = Vector2d.zero;

        public WeatherInput()
        {

        }

        public void Parse(string[] inputLines, int startIndex, string rootFolder, out bool success)
        {
            int issues = 0;
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string nameOfInput, userInput;

            //stream in if not exists
            nameOfInput = nameof(WeatherFile);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                WeatherFile = userInput;
                PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success);
            }
            else
            {
                success = false;
                PREACTInput.InputNotFoundMessage(nameOfInput);
            }
            if (success == false)
            {
                WeatherFile = string.Empty;
            }

            nameOfInput = nameof(DesiredLatLon);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                string[] data = userInput.Split(',');
                issues += double.TryParse(data[0], out DesiredLatLon.x) ? 0 : 1;
                issues += double.TryParse(data[1], out DesiredLatLon.y) ? 0 : 1;
                if (issues > 0)
                {
                    PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                }
            }
            else
            {
                PREACTInput.InputNotFoundMessage(nameOfInput, false);
            }
            if (issues > 0)
            {
                success = false;
                return;
            }
            issues = 0;

            success = true;
        }
    }
}
