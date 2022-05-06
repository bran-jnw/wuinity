using System.Collections.Generic;
using System.IO;

namespace WUInity.Fire
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

        public List<FuelModel> fuels;

        public bool LoadFuelModelInputFile(string file)
        {
            string[] fuelLines;
            if (File.Exists(file))
            {
                fuelLines = File.ReadAllLines(file);
            }
            else
            {
                WUInity.WUI_LOG("Fuel model file " + file + " not found." );
                return false;
            }

            fuels = new List<FuelModel>();

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

                    fuels.Add(newFuel);
                }
            }

            if(fuels.Count == 0)
            {
                fuels = null;
                return false;
            }

            return true;
        }
    }
}

