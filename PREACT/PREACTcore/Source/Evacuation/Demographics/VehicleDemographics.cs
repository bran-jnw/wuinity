using System.Collections.Generic;
using System.IO;

namespace PREACT.Evacuation
{   
    public class VehicleDemographics
    {
        int _maxHouseholdSize;
        public Dictionary<int, Utility.Math.CDFSampler<int>> _vehicleCountCDFs;

        public VehicleDemographics(Dictionary<int, Utility.Math.CDFSampler<int>> cdfs)
        {
            _vehicleCountCDFs = cdfs;

            foreach (var householdSize in _vehicleCountCDFs.Keys)
            {
                if (householdSize > _maxHouseholdSize)
                {
                    _maxHouseholdSize = householdSize;
                }
            }
        }

        public static Dictionary<string, VehicleDemographics> CreateVehicleDemographicsFromInput(string rootFolder, Dictionary<string, VehicleDemographicsInput> vehicleDemographicsInput)
        {
            Dictionary<string, VehicleDemographics> vehicleDemographics = new Dictionary<string, VehicleDemographics>(vehicleDemographicsInput.Count);

            foreach (KeyValuePair<string, VehicleDemographicsInput> h in vehicleDemographicsInput)
            {
                if (h.Value != null)
                {
                    VehicleDemographics? result = Parse(Path.Combine(rootFolder, h.Value.VehicleDemographicsFile), out bool success);
                    if (result != null)
                    {
                        vehicleDemographics.Add(h.Key, result);
                    }
                }
            }

            return vehicleDemographics;
        }

        public static VehicleDemographics? Parse(string filePath, out bool success)
        {
            success = false;
            VehicleDemographics? result = null;

            if (File.Exists(filePath))
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    Dictionary<int, Utility.Math.CDFSampler<int>> cdfs = new Dictionary<int, Utility.Math.CDFSampler<int>>();
                    sr.ReadLine(); // skip header
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            string[] rowData = line.Split(',');
                            if (rowData.Length >= 2)
                            {
                                if (int.TryParse(rowData[0], out int householdSize))
                                {
                                    List<int> numberofVehicles = new List<int>();
                                    List<double> cdf = new List<double>();

                                    for (int i = 1; i < rowData.Length; ++i)
                                    {
                                        if (double.TryParse(rowData[i], out double cumProb))
                                        {
                                            numberofVehicles.Add(i-1);
                                            cdf.Add(cumProb);
                                        }
                                    }

                                    if (numberofVehicles.Count > 0)
                                    {
                                        cdfs.Add(householdSize, new Utility.Math.CDFSampler<int>(cdf.ToArray(), numberofVehicles.ToArray()));
                                    }
                                    else
                                    {
                                        Engine.Message(null, Engine.LogType.InputError, $"Invalid numberofVehicles in household demographics file {filePath}: {line}, could not interpret numberofVehicles.");
                                        return null;
                                    }
                                }
                                else
                                {
                                    Engine.Message(null, Engine.LogType.InputError, $"Invalid numberofVehicles in household demographics file {filePath}: {line}, could not interpret household size.");
                                    return null;
                                }
                            }
                            else
                            {
                                Engine.Message(null, Engine.LogType.InputError, $"Invalid format in household demographics file {filePath}: {line}, interpreted numberofVehicles is less than 2 numberofVehicles.");
                                return null;
                            }
                        }
                    }

                    if (cdfs.Count > 0)
                    {
                        result = new VehicleDemographics(cdfs);
                    }
                }
            }
            else
            {
                Engine.Message(null, Engine.LogType.InputError, $"Household demographics file {filePath} could not be found.");
            }

            return result;
        }

        public int SampleVehicleCount(int householdSize)
        {
            if (householdSize < 1)
            {
                throw new System.ArgumentOutOfRangeException(nameof(householdSize), "Household size must be larger than 1.");
            }

            if (householdSize > _maxHouseholdSize)
            {
                householdSize = _maxHouseholdSize;
            }

            if (_vehicleCountCDFs.TryGetValue(householdSize, out Utility.Math.CDFSampler<int> cdf))
            {
                return cdf.DrawSample();
            }
            else
            {
                throw new System.Exception($"No vehicle count CDF found for household size {householdSize}.");
            }
        }
    }
}
