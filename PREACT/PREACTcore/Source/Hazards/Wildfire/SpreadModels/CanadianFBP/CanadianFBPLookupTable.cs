using System.Collections.Generic;
using System.IO;

namespace PREACT.Wildfire
{
    public struct CanadianFBPLookupEntry
    {
        public int grid_value;
        public string fuel_type;
        public int r, g, b;
    }

    public class CanadianFBPLookupTable
    {
        Dictionary<int, CanadianFBPLookupEntry> _database = new Dictionary<int, CanadianFBPLookupEntry>(256);

        public CanadianFBPLookupTable()
        {
            
        }

        public CanadianFBPLookupEntry GetLookupEntry(int fuelModelNumber, out bool success)
        {
            CanadianFBPLookupEntry entry;
            success = _database.TryGetValue(fuelModelNumber, out entry);
            return entry;
        }

        /// <summary>
        /// Reads the user defined FBP lookup table and creates a database that can be used by the fire spread model.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="success"></param>
        /// <returns></returns>
        public void Parse(string filePath, out bool success)
        {
            _database.Clear();
            Engine.Message(null, Engine.LogType.Log, " Attempting to load FBP lookup table.");
            success = false;

            string[] lines;
            if (File.Exists(filePath))
            {
                lines = File.ReadAllLines(filePath);
            }
            else
            {
                Engine.Message(null, Engine.LogType.Warning, "FBP lookup table file " + filePath + " not found.");
                return;
            }

            if (lines.Length < 2)
            {
                Engine.Message(null, Engine.LogType.Warning, "FBP lookup table file " + filePath + " does not contain data.");
                return;
            }

            //skip first line
            for(int i = 1; i < lines.Length; ++i)
            {
                string[] line = lines[i].Split(',');
                if(line.Length > 6)
                {
                    CanadianFBPLookupEntry entry = new CanadianFBPLookupEntry();
                    int.TryParse(line[0], out entry.grid_value);
                    entry.fuel_type = line[3];
                    int.TryParse(line[4], out entry.r);
                    int.TryParse(line[5], out entry.r);
                    int.TryParse(line[6], out entry.b);
                    _database.Add(entry.grid_value, entry);
                    //Engine.Message(null, Engine.LogType.Debug, $"Canadian fuel lookup entry fuel_type is {entry.fuel_type}.");
                }
            }

            if (_database.Count == 0)
            {
                success = false;
            }
            else
            {
                success = true;
            }
        }
    }
}
