using System;
using System.Collections.Generic;

namespace PREACT.Input
{
    public static class ReferenceProbabilityPairs
    {
        public static bool TryParse(string input, List<(string, double)> result)
        {
            result.Clear();

            string[] data = input.Split(',');
            for (int i = 0; i < data.Length; ++i)
            {
                string[] valueProbs = data[i].Split('@');
                if (valueProbs.Length != 2)
                {
                    return false;
                }

                if (!double.TryParse(valueProbs[1], out double probability))
                {
                    return false;
                }

                result.Add((valueProbs[0], probability));
            }
            
            return true;
        }
    }
}
