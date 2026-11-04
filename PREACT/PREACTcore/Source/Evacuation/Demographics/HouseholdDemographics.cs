using System.Collections.Generic;
using System.IO;

namespace PREACT.Evacuation
{
    public class HouseholdDemographics
    {
        private Utility.Math.CDFSampler<int> _householdSizeCDF;


        public HouseholdDemographics(List<int> householdSizes, List<double> cdf)
        {
            _householdSizeCDF = new Utility.Math.CDFSampler<int>(cdf.ToArray(), householdSizes.ToArray());
        }

        public int SampleHouseholdSize()
        {
            return _householdSizeCDF.DrawSample();
        }

        public static Dictionary<string, HouseholdDemographics> CreateHouseholdDemographicsFromInput(string rootFolder, Dictionary<string, HouseholdDemographicsInput> householdDemographicsInput)
        {
            Dictionary<string, HouseholdDemographics> householdDemographics = new Dictionary<string, HouseholdDemographics>(householdDemographicsInput.Count);

            foreach (KeyValuePair<string, HouseholdDemographicsInput> h in householdDemographicsInput)
            {
                if(h.Value != null)
                {
                    HouseholdDemographics? result = Parse(Path.Combine(rootFolder, h.Value.HouseholdDemographicsFile), out bool success);
                    if (result != null)
                    {
                        householdDemographics.Add(h.Key, result);
                    }                    
                }                
            }

            return householdDemographics;
        }

        public static HouseholdDemographics? Parse(string filePath, out bool success)
        {
            success = false;
            HouseholdDemographics? result = null;

            if (File.Exists(filePath))
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    List<int> data = new List<int>();
                    List<double> cdf = new List<double>();
                    sr.ReadLine(); // skip header
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            string[] parts = line.Split(',');
                            if (parts.Length == 2)
                            {
                                if (int.TryParse(parts[0], out int householdSize) && double.TryParse(parts[1], out double cumProb))
                                {
                                    data.Add(householdSize);
                                    cdf.Add(cumProb);
                                }
                                else
                                {
                                    Engine.Message(null, Engine.LogType.InputError, $"Invalid data in household demographics file {filePath}: {line}");
                                    return null;
                                }
                            }
                            else
                            {
                                Engine.Message(null, Engine.LogType.InputError, $"Invalid format in household demographics file {filePath}: {line}");
                                return null;
                            }
                        }
                    }

                    if (data.Count > 0 && cdf.Count > 0)
                    {
                        result = new HouseholdDemographics(data, cdf);
                        success = true;

                    }
                    else
                    {
                        Engine.Message(null, Engine.LogType.InputError, $"Household demographics file {filePath} did not contain any data.");
                    }
                }
            }
            else
            {
                Engine.Message(null, Engine.LogType.InputError, $"Household demographics file {filePath} could not be found.");
            }

            return result;
        }
    }    
}
