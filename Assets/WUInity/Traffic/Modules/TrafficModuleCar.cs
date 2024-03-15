using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace WUInity.Traffic
{
    public abstract class TrafficModuleCar
    {
        public uint carID, numberOfPeopleInCar;

        public TrafficModuleCar(uint carID, uint numberOfPeopleInCar)
        {
            this.carID = carID;
            this.numberOfPeopleInCar = numberOfPeopleInCar;
        }

        public abstract void Arrive();
    }
}

    
