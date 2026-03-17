using System.Collections.Generic;

namespace PREACT.Input
{
    public class LagrangianInput
    {
        public int ParticlesPerFireCell = 50;

        public LagrangianInput()
        {
        }

        public static LagrangianInput Parse(string[] inputLines, int startIndex, out bool success)
        {
            LagrangianInput newInput = new LagrangianInput();

            success = false;
            int issues = 0;
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string input, userInput;

            input = nameof(ParticlesPerFireCell);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                issues += int.TryParse(userInput, out newInput.ParticlesPerFireCell) ? 0 : 1;
            }
            else
            {
                ++issues;
                PREACTInput.InputNotFoundMessage(input, true);
            }
            if (issues > 0)
            {
                success = false;
                return newInput;
            }

            return newInput;
        }
    }
}

