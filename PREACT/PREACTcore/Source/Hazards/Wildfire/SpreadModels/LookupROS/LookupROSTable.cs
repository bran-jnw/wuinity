using System.IO;
using System.Collections.Generic;

namespace PREACT.Wildfire
{ 
    public struct LookupROSEntry
    {
        public int FuelNumber;
        public double NoWindNoSlopeROS;
        public double WindCoefficient;

        public LookupROSEntry(int fuelNumber, double noWindNoSlopeROS, double windCoefficient)
        {
            FuelNumber = fuelNumber;
            NoWindNoSlopeROS = noWindNoSlopeROS;
            WindCoefficient = windCoefficient;
        }
    }
    public class LookupROSTable
    {
        private Dictionary<int, LookupROSEntry> _database;
        public LookupROSTable()
        {
            _database = new Dictionary<int, LookupROSEntry>(10);
        }

        public LookupROSEntry GetSpreadRate(int fuelModel, out bool success)
        {
            success = _database.TryGetValue(fuelModel, out LookupROSEntry result);

            return result;
        }

        public void Parse(string filePath, out bool success)
        {            
            Engine.Message(null, Engine.LogType.Log, " Attempting to load spread rate lookup table.");
            success = false;

            string[] lines;
            if (File.Exists(filePath))
            {
                lines = File.ReadAllLines(filePath);
            }
            else
            {
                Engine.Message(null, Engine.LogType.Warning, "Spread rate lookup table file " + filePath + " not found, using default.");
                return;
            }

            if (lines.Length < 2)
            {
                Engine.Message(null, Engine.LogType.Warning, "Spread rate lookup table file " + filePath + " does not contain data, using default.");            
                return;
            }

            _database.Clear();

            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using (var sr = new StreamReader(fs))
            {
                //skip header
                string line = sr.ReadLine();

                while(!sr.EndOfStream)
                {
                    line = sr.ReadLine();

                    string[] data = line.Split(',');
                    if (line.Length >= 3)
                    {
                        int fuelNumber;
                        double spreadRate, windCoeff;

                        int.TryParse(data[0], out fuelNumber);
                        double.TryParse(data[1], out spreadRate);
                        double.TryParse(data[2], out windCoeff);
                        _database.Add(fuelNumber, new LookupROSEntry(fuelNumber, spreadRate, windCoeff));
                    }
                }
            }

            if (_database.Count == 0)
            {
                Engine.Message(null, Engine.LogType.Log,  "Attempted to load spread rate lookup table but did not find any fuel data.");
                success = false;
            }
            else
            {
                Engine.Message(null, Engine.LogType.Log, $"Found {_database.Count} valid fuel entries in lookup table.");
                success = true;
            }
        }
    }
}
