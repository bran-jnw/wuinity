using System;
using System.Collections.Generic;
using PREACT.Math;

namespace PREACT.IO
{
    public class WeatherInput
    {
        public string WeatherFile = string.Empty;
        public Vector2d DesiredCoordinate = Vector2d.zero;

        public WeatherInput()
        {

        }

        public void Parse(string[] inputLines, int startIndex, string rootFolder, out bool success)
        {
            int issues = 0;
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string nameOfInput, userInput;

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
                return;
            }

            nameOfInput = nameof(DesiredCoordinate);
            if (inputToParse.TryGetValue(nameOfInput, out userInput))
            {
                string[] data = userInput.Split(',');
                issues += double.TryParse(data[0], out DesiredCoordinate.x) ? 0 : 1;
                issues += double.TryParse(data[1], out DesiredCoordinate.y) ? 0 : 1;
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
