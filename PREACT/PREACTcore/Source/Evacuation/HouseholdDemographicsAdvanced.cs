using System.Collections.Generic;
using System.IO;
using PREACT.Utility.Math;

namespace PREACT.Evacuation
{    
    public class HouseholdDemographicsAdvanced
    {
        private CDFSampler<HouseholdType> _householdTypes;
        private Dictionary<HouseholdType, CDFSampler<AdultAgeBrackets>> _adultAges;
        private Dictionary<HouseholdType, Dictionary<AdultAgeBrackets, double>> _driversLicenses;
        private Dictionary<HouseholdType, Dictionary<AdultAgeBrackets, double>> _impairments;
        private Dictionary<HouseholdType, Dictionary<AdultAgeBrackets, double>> _employments;
        private Dictionary<HouseholdType, double> _workFromHome;
        private Dictionary<HouseholdType, CDFSampler<int>> _childCounts;
        private Dictionary<HouseholdType, CDFSampler<ChildAgeBrackets>> _childAges;
        private Dictionary<HouseholdType, CDFSampler<int>> _vehicleCounts;

        private CDFSampler<int> _householdSizeCDF;

        public HouseholdType GetHouseholdType()
        {
            return _householdTypes.DrawSample();
        }

        public AdultAgeBrackets GetAdultAgeBracket(HouseholdType householdType)
        {
            bool success = _adultAges.TryGetValue(householdType, out CDFSampler<AdultAgeBrackets> cdf);

            AdultAgeBrackets adultAgeBracket = AdultAgeBrackets.age_35_64;
            if (success)
            {
                adultAgeBracket = cdf.DrawSample();
            }

            return adultAgeBracket;
        }

        public bool GetLicense(HouseholdType householdType, AdultAgeBrackets ageBracket)
        {
            bool haveLicense = false;

            bool success = _driversLicenses.TryGetValue(householdType, out Dictionary<AdultAgeBrackets, double> brackets);
            if(success)
            {
                success = brackets.TryGetValue(ageBracket, out double probability);

                if (success)
                {
                    if(Math.Random.valueD < probability)
                    {
                        haveLicense = true;
                    }
                }
            }   

            return haveLicense;
        }

        public bool GetDisability(HouseholdType householdType, AdultAgeBrackets ageBracket)
        {
            bool haveDisability = false;

            bool success = _impairments.TryGetValue(householdType, out Dictionary<AdultAgeBrackets, double> brackets);
            if (success)
            {
                success = brackets.TryGetValue(ageBracket, out double probability);

                if (success)
                {
                    if (Math.Random.valueD < probability)
                    {
                        haveDisability = true;
                    }
                }
            }

            return haveDisability;
        }

        public bool GetEmployment(HouseholdType householdType, AdultAgeBrackets ageBracket)
        {
            bool haveDisability = false;

            bool success = _employments.TryGetValue(householdType, out Dictionary<AdultAgeBrackets, double> brackets);
            if (success)
            {
                success = brackets.TryGetValue(ageBracket, out double probability);
                if (success)
                {
                    if (Math.Random.valueD < probability)
                    {
                        haveDisability = true;
                    }
                }
            }

            return haveDisability;
        }

        public bool GetWorkFromHome(HouseholdType householdType)
        {
            bool workFromHome = false;

            bool success = _workFromHome.TryGetValue(householdType, out double probability);
            if (success)
            {
                if (Math.Random.valueD < probability)
                {
                    workFromHome = true;
                }
            }

            return workFromHome;
        }

        public int GetChildCount(HouseholdType householdType)
        {
            bool success = _childCounts.TryGetValue(householdType, out CDFSampler<int> cdf);

            int childCount = 0;
            if (success)
            {
                childCount = cdf.DrawSample();
            }

            return childCount;
        }

        public ChildAgeBrackets GetChildAgeBracket(HouseholdType householdType)
        {
            bool success = _childAges.TryGetValue(householdType, out CDFSampler<ChildAgeBrackets> cdf);

            ChildAgeBrackets childAgeBracket = ChildAgeBrackets.age_6_17;
            if (success)
            {
                childAgeBracket = cdf.DrawSample();
            }

            return childAgeBracket;
        }

        public int GetVehicleCount(HouseholdType householdType)
        {
            bool success = _vehicleCounts.TryGetValue(householdType, out CDFSampler<int> cdf);

            int vehicleCount = 0;
            if(success)
            {
                vehicleCount = cdf.DrawSample();
            }

            return vehicleCount;
        }


        public HouseholdDemographicsAdvanced(List<int> householdSizes, List<double> cdf)
        {
            _householdSizeCDF = new CDFSampler<int>(cdf.ToArray(), householdSizes.ToArray());
        }

        public int SampleHouseholdSize()
        {
            return _householdSizeCDF.DrawSample();
        }

        public static Dictionary<string, HouseholdDemographicsAdvanced> CreateHouseholdDemographicsFromInput(string rootFolder, Dictionary<string, HouseholdDemographicsInput> householdDemographicsInput)
        {
            Dictionary<string, HouseholdDemographicsAdvanced> householdDemographics = new Dictionary<string, HouseholdDemographicsAdvanced>(householdDemographicsInput.Count);

            foreach (KeyValuePair<string, HouseholdDemographicsInput> h in householdDemographicsInput)
            {
                if (h.Value != null)
                {
                    HouseholdDemographicsAdvanced? result = Parse(Path.Combine(rootFolder, h.Value.HouseholdDemographicsFile), out bool success);
                    if (result != null)
                    {
                        householdDemographics.Add(h.Key, result);
                    }
                }
            }

            return householdDemographics;
        }

        public static HouseholdDemographicsAdvanced? Parse(string filePath, out bool success)
        {
            success = false;
            HouseholdDemographicsAdvanced? result = null;

            if (File.Exists(filePath))
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    List<int> data = new List<int>();
                    List<double> probabilities = new List<double>();
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
                                    probabilities.Add(cumProb);
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

                    if (data.Count > 0 && probabilities.Count > 0)
                    {
                        result = new HouseholdDemographicsAdvanced(data, probabilities);
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
