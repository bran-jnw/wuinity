using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace WUIPlatform.Traffic
{
    public abstract class TrafficModuleCar
    {
        public uint carID, numberOfPeopleInCar;
        public EvacuationGoal goal;

        public TrafficModuleCar(uint carID, uint numberOfPeopleInCar, EvacuationGoal goal)
        {
            this.carID = carID;
            this.numberOfPeopleInCar = numberOfPeopleInCar;
            this.goal = goal;
        }

        public abstract void Arrive();

        //Rik's
        public abstract System.Numerics.Vector4 GetPositionAndSpeed(bool updateData);
        public void CarMoved()
        {
            Simulation.movedCars.Add(this);
        }
    }
}

    
