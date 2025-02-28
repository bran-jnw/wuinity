//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;

namespace WUIPlatform.Fire
{
    [System.Serializable]
    public class InitialFuelMoisture
    {
        public static InitialFuelMoisture DEFAULT = new InitialFuelMoisture(0, 6, 7, 8, 60, 90);

        public int FuelModelNumber;
        public double OneHour;
        public double TenHour;
        public double HundredHour;
        public double LiveHerbaceous;
        public double LiveWoody;

        public InitialFuelMoisture(int fuelModelNumber, double oneHour, double tenHour, double hundredHour, double liveHerbaceous, double liveWoody)
        {
            FuelModelNumber = fuelModelNumber;
            OneHour = oneHour;
            TenHour = tenHour;
            HundredHour = hundredHour;
            LiveHerbaceous = liveHerbaceous;
            LiveWoody = liveWoody;
        }

        public InitialFuelMoisture(int fuelModelNumber)
        {
            FuelModelNumber = fuelModelNumber;
            OneHour = DEFAULT.OneHour;
            TenHour = DEFAULT.TenHour;
            HundredHour = DEFAULT.HundredHour;
            LiveHerbaceous = DEFAULT.LiveHerbaceous;
            LiveWoody = DEFAULT.LiveWoody;
        }

        public void UpdateData(double oneHour, double tenHour, double hundredHour, double liveHerbaceous, double liveWoody)
        {
            OneHour = oneHour;
            TenHour = tenHour;
            HundredHour = hundredHour;
            LiveHerbaceous = liveHerbaceous;
            LiveWoody = liveWoody;
        }
    }

    [System.Serializable]
    public class InitialFuelMoistureLibrary
    {
        private Dictionary<int, InitialFuelMoisture> _initialFuelMoistures;

        /// <summary>
        /// Creates a new table of initial fuel moistures based on list (most likely from a *.fmc file).
        /// </summary>
        /// <param name="initialFuelMoistures"></param>
        public InitialFuelMoistureLibrary(List<InitialFuelMoisture> initialFuelMoistures)
        {
            _initialFuelMoistures = new Dictionary<int, InitialFuelMoisture>();
            for (int i = 0; i < initialFuelMoistures.Count; i++)
            {
                _initialFuelMoistures.Add(initialFuelMoistures[i].FuelModelNumber, initialFuelMoistures[i]);            
            }
        }

        /// <summary>
        /// Creates a list filled with default values for fuel 1-13.
        /// </summary>
        public InitialFuelMoistureLibrary()
        {
            _initialFuelMoistures = new Dictionary<int, InitialFuelMoisture>();
            for (int i = 1; i < 14; i++)
            {
                _initialFuelMoistures.Add(i, new InitialFuelMoisture(i));
            }
        }     

        public InitialFuelMoisture GetInitialFuelMoisture(int fuelModelNumber)
        {
            InitialFuelMoisture result = null;

            _initialFuelMoistures.TryGetValue(fuelModelNumber, out result);             

            if(result == null)
            {
                result = new InitialFuelMoisture(fuelModelNumber);
                _initialFuelMoistures.Add(fuelModelNumber, result);
                WUIEngine.LOG(WUIEngine.LogType.Warning, "Initial fuel moisture for fuel model " + fuelModelNumber + " was set to default as it has not been user specified.");
            }

            return result;
        }        

        public static InitialFuelMoistureLibrary LoadInitialFuelMoistureDataFile(string path, out bool success)
        {
            success = false;
            InitialFuelMoistureLibrary result = null;
            List<InitialFuelMoisture> initialFuelMoistures = new List<InitialFuelMoisture>();

            bool fileExists = File.Exists(path);
            if (fileExists)
            {
                string[] dataLines = File.ReadAllLines(path);                
                //skip first line (header)
                for (int j = 1; j < dataLines.Length; j++)
                {
                    string[] data = dataLines[j].Split(',');
                    if(data.Length >= 6)
                    {
                        int fuelMod;
                        double oneHour, tenHour, hundredHour, liveH, liveW;

                        bool b1 = int.TryParse(data[0], out fuelMod);
                        bool b2 = double.TryParse(data[1], out oneHour);
                        bool b3 = double.TryParse(data[2], out tenHour);
                        bool b4 = double.TryParse(data[3], out hundredHour);
                        bool b5 = double.TryParse(data[4], out liveH);
                        bool b6 = double.TryParse(data[5], out liveW);

                        if (b1 && b2 && b3 && b4 && b5 && b6)
                        {
                            InitialFuelMoisture iFM = new InitialFuelMoisture(fuelMod, oneHour, tenHour, hundredHour, liveH, liveW);
                            initialFuelMoistures.Add(iFM);
                            WUIEngine.LOG(WUIEngine.LogType.Log, "Loaded initial fuel moistures for fuel model " + fuelMod + ".");
                        }
                    }                    
                }                              
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, "Initial fuel moisture data file " + path + " not found and could not be loaded, using defaults.");
            }

            if (initialFuelMoistures.Count > 0)
            {
                result = new InitialFuelMoistureLibrary(initialFuelMoistures);
                success = true; 
                WUIEngine.LOG(WUIEngine.LogType.Log, " Initial fuel moisture file " + path + " was found, " + initialFuelMoistures.Count + " valid initial fuel moistures were succesfully loaded.");
            }
            else if(fileExists)
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, "Initial fuel moisture file " + path + " was found but did not contain any valid data, using defaults.");
            }

            return result;
        }
    }        
}

