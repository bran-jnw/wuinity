using System.Collections.Generic;
using System.IO;

namespace WUInity.Fire
{
    [System.Serializable]
    public class InitialFuelMoisture
    {
        public static InitialFuelMoisture DEAFULT = new InitialFuelMoisture(0, 6, 7, 8, 60, 90);

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
    public class InitialFuelMoistureList
    {        
        private InitialFuelMoisture[] initialFuelMoistures;
        bool _conditioned = false;

        /// <summary>
        /// Creates a new table of initial fuel moistures based on list (most likely from a *.fmc file).
        /// </summary>
        /// <param name="initialFuelMoistures"></param>
        public InitialFuelMoistureList(List<InitialFuelMoisture> initialFuelMoistures)
        {
            this.initialFuelMoistures = new InitialFuelMoisture[256];
            for (int i = 0; i < initialFuelMoistures.Count; i++)
            {
                if(initialFuelMoistures[i].FuelModelNumber > 0 && initialFuelMoistures[i].FuelModelNumber < 256)
                {
                    this.initialFuelMoistures[initialFuelMoistures[i].FuelModelNumber - 1] = initialFuelMoistures[i];
                }                
            }
        }

        /// <summary>
        /// Creates a list filled with default values for fuel 1-13.
        /// </summary>
        public InitialFuelMoistureList()
        {
            this.initialFuelMoistures = new InitialFuelMoisture[256];
            for (int i = 0; i < 13; i++)
            {
                this.initialFuelMoistures[i] = new InitialFuelMoisture(i + 1, 6, 7, 8, 60, 90);
            }
        }
        


    public InitialFuelMoisture GetInitialFuelMoisture(int fuelModelNumber)
        {
            InitialFuelMoisture result = null;

            if (fuelModelNumber < 1 || fuelModelNumber > 255)
            {                
                WUInity.LOG(WUInity.LogType.Warning, "Tried to get initial fuel moisture for fuel number " + fuelModelNumber + " which is outside of accepted range [1-256].");
                return InitialFuelMoisture.DEAFULT;
            }

            if(fuelModelNumber > 0 && fuelModelNumber - 1 < initialFuelMoistures.Length)
            {
                result = initialFuelMoistures[fuelModelNumber - 1];
            }
             

            if(result == null)
            {
                result = new InitialFuelMoisture(fuelModelNumber, 6.0, 7.0, 8.0, 60.0, 90.0);
                initialFuelMoistures[fuelModelNumber - 1] = result;
                WUInity.LOG(WUInity.LogType.Warning, "Initial fuel moisture for fuel model " + fuelModelNumber + " was set to default as it has not been user specified.");
            }

            return result;
        }        

        public static InitialFuelMoistureList LoadInitialFuelMoistureDataFile(string path, out bool success)
        {
            success = false;
            InitialFuelMoistureList result = null;
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
                            WUInity.LOG(WUInity.LogType.Log, "Loaded initial fuel moistures for fuel model " + fuelMod + ".");
                        }
                    }                    
                }                              
            }
            else
            {
                WUInity.LOG(WUInity.LogType.Warning, "Initial fuel moisture data file " + path + " not found and could not be loaded, using defaults.");
            }

            if (initialFuelMoistures.Count > 0)
            {
                result = new InitialFuelMoistureList(initialFuelMoistures);
                success = true; 
                WUInity.LOG(WUInity.LogType.Log, " Initial fuel moisture file " + path + " was found, " + initialFuelMoistures.Count + " valid initial fuel moistures were succesfully loaded.");
            }
            else if(fileExists)
            {
                WUInity.LOG(WUInity.LogType.Warning, "Initial fuel moisture file " + path + " was found but did not contain any valid data, using defaults.");
            }

            return result;
        }
    }        
}

