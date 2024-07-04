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
    public class FuelModelInput
    {
        public class FuelModel
        {
            public int fuelModelNumber;
            public string code, name;
            public double fuelBedDepth, moistureOfExtinctionDead, heatOfCombustionDead, heatOfCombustionLive,
            fuelLoadOneHour, fuelLoadTenHour, fuelLoadHundredHour, fuelLoadLiveHerbaceous,
            fuelLoadLiveWoody, savrOneHour, savrLiveHerbaceous, savrLiveWoody;
            public bool isDynamic, isReserved;

            public FuelModel(int fuelModelNumber, string code, string name,
                double fuelBedDepth, double moistureOfExtinctionDead, double heatOfCombustionDead, double heatOfCombustionLive,
                double fuelLoadOneHour, double fuelLoadTenHour, double fuelLoadHundredHour, double fuelLoadLiveHerbaceous,
                double fuelLoadLiveWoody, double savrOneHour, double savrLiveHerbaceous, double savrLiveWoody,
                bool isDynamic, bool isReserved)
            {
                this.fuelModelNumber = fuelModelNumber;
                this.code = code;
                this.name = name;
                this.fuelBedDepth = fuelBedDepth;
                this.moistureOfExtinctionDead = moistureOfExtinctionDead;
                this.heatOfCombustionDead = heatOfCombustionDead;
                this.heatOfCombustionLive = heatOfCombustionLive;
                this.fuelLoadOneHour = fuelLoadOneHour;
                this.fuelLoadTenHour = fuelLoadTenHour;
                this.fuelLoadHundredHour = fuelLoadHundredHour;
                this.fuelLoadLiveHerbaceous = fuelLoadLiveHerbaceous;
                this.fuelLoadLiveWoody = fuelLoadLiveWoody;
                this.savrOneHour = savrOneHour;
                this.savrLiveHerbaceous = savrLiveHerbaceous;
                this.savrLiveWoody = savrLiveWoody;
                this.isDynamic = isDynamic;
                this.isReserved = isReserved;
            }
        }

        public List<FuelModel> Fuels;

        public bool LoadFuelModelInputFile(string path)
        {
            WUIEngine.LOG(WUIEngine.LogType.Log, " Attempting to load fuel model file.");
            bool success = false;

            string[] fuelLines;
            if (File.Exists(path))
            {
                fuelLines = File.ReadAllLines(path);
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, "Fuel model file " + path + " not found." );
                return false;
            }

            Fuels = new List<FuelModel>();

            //skip first line as that is just the header
            for (int i = 1; i < fuelLines.Length; i++)
            {
                string[] fuelParameters = fuelLines[i].Split(',');
                //make sure there is some data and not just empty line
                if (fuelParameters.Length >= 17)
                {
                    int fuelModelNumber;
                    string code, name;
                    double fuelBedDepth, moistureOfExtinctionDead, heatOfCombustionDead, heatOfCombustionLive,
                    fuelLoadOneHour, fuelLoadTenHour, fuelLoadHundredHour, fuelLoadLiveHerbaceous,
                    fuelLoadLiveWoody, savrOneHour, savrLiveHerbaceous, savrLiveWoody;
                    bool isDynamic, isReserved;

                    int.TryParse(fuelParameters[0], out fuelModelNumber);

                    //need strings?
                    code = fuelParameters[1].Split('"')[1];
                    name = fuelParameters[2].Split('"')[1];

                    double.TryParse(fuelParameters[3], out fuelBedDepth);
                    double.TryParse(fuelParameters[4], out moistureOfExtinctionDead);
                    double.TryParse(fuelParameters[5], out heatOfCombustionDead);
                    double.TryParse(fuelParameters[6], out heatOfCombustionLive);
                    double.TryParse(fuelParameters[7], out fuelLoadOneHour);
                    double.TryParse(fuelParameters[8], out fuelLoadTenHour);
                    double.TryParse(fuelParameters[9], out fuelLoadHundredHour);
                    double.TryParse(fuelParameters[10], out fuelLoadLiveHerbaceous);
                    double.TryParse(fuelParameters[11], out fuelLoadLiveWoody);
                    double.TryParse(fuelParameters[12], out savrOneHour);
                    double.TryParse(fuelParameters[13], out savrLiveHerbaceous);
                    double.TryParse(fuelParameters[14], out savrLiveWoody);

                    //need bools?
                    bool.TryParse(fuelParameters[15], out isDynamic);
                    bool.TryParse(fuelParameters[16], out isReserved);

                    //save fuel model
                    FuelModel newFuel = new FuelModel(fuelModelNumber,
                    code, name,
                    fuelBedDepth, moistureOfExtinctionDead, heatOfCombustionDead, heatOfCombustionLive,
                    fuelLoadOneHour, fuelLoadTenHour, fuelLoadHundredHour, fuelLoadLiveHerbaceous,
                    fuelLoadLiveWoody, savrOneHour, savrLiveHerbaceous, savrLiveWoody,
                    isDynamic, isReserved);

                    Fuels.Add(newFuel);
                    WUIEngine.LOG(WUIEngine.LogType.Log, " Loaded fuel model number  " + fuelModelNumber + ", " + code + ", " + name + ".");
                }
            }

            if(Fuels.Count == 0)
            {
                Fuels = null;
                success = false;
            }
            else
            {
                success = true;
            }            

            return success;
        }
    }
}

