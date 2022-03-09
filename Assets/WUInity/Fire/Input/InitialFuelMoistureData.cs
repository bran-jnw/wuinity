using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity.Fire
{
    [System.Serializable]
    public struct FuelMoisture
    {
        public double OneHour;
        public double TenHour;
        public double HundredHour;
        public double LiveHerbaceous;
        public double LiveWoody;

        public FuelMoisture(double oneHour, double tenHour, double hundredHour, double liveHerbaceous, double liveWoody)
        {
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
    public struct InitialFuelMoistureData
    {
        [SerializeField] private FuelMoisture[] fuelMoistures;

        public InitialFuelMoistureData(int nFuels)
        {
            fuelMoistures = new FuelMoisture[nFuels];
        }

        public FuelMoisture GetInitialFuelMoisture(int fuelNumber)
        {
            if(fuelNumber < 1 || fuelNumber > 13)
            {
                return new FuelMoisture();
            }
            return fuelMoistures[fuelNumber - 1];
        }

        public static InitialFuelMoistureData GetDefaults()
        {
            InitialFuelMoistureData data = new InitialFuelMoistureData(13);
            for (int i = 0; i < 13; i++)
            {
                data.fuelMoistures[i].UpdateData(6, 7, 8, 60, 90);
            }

            return data;
        }
    }    
}

