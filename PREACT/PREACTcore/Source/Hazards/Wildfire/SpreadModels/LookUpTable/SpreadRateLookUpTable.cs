using System.IO;
using System.Collections.Generic;

namespace PREACT.Wildfire
{ 
    public class SpreadRateLookUpTable
    {
        private Dictionary<int, double> _database;
        public SpreadRateLookUpTable()
        {
            _database = new Dictionary<int, double>(10);
            BuildDefault();
        }

        public double GetSpreadRate(int fuelModel)
        {
            double result = 0;
            _database.TryGetValue(fuelModel, out result);

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

            //skip first line, header
            for (int i = 1; i < lines.Length; ++i)
            {
                string[] line = lines[i].Split(',');
                if (line.Length == 2)
                {
                    int fuelNumber;
                    double spreadRate;

                    int.TryParse(line[0], out fuelNumber);
                    double.TryParse(line[1], out spreadRate);
                    _database.Add(fuelNumber, spreadRate);
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

        private void BuildDefault()
        {
            _database.Add(1, 0.1);
            _database.Add(2, 0.2);
            _database.Add(3, 0.3);
            _database.Add(4, 0.4);
            _database.Add(5, 0.5);
            _database.Add(6, 0.6);
            _database.Add(7, 0.7);
            _database.Add(8, 0.8);
            _database.Add(9, 0.9);
            _database.Add(10, 1.0);
        }
    }
}
