using System.Collections.Generic;
using System.Numerics;

namespace WUInity.Traffic
{
    public abstract class TrafficModule
    {
        protected List<float> arrivalData;

        public TrafficModule()
        {
            arrivalData = new List<float>();
        }

        public abstract void Update(float deltaTime, float currentTime);
        public abstract bool SimulationDone();
        public abstract void InsertNewCar(Vector2d startLatLong, EvacuationGoal evacuationGoal, RouteData routeData, uint numberOfPeopleInCar);
        public abstract void InsertNewTrafficEvent(TrafficEvent tE);
        public abstract int GetTotalCarsSimulated();        
        public abstract int GetCarsInSystem();
        public abstract void UpdateEvacuationGoals();
        public abstract Vector4[] GetCarPositionsAndStates();
        public abstract void SaveToFile(int runNumber);

        static uint carCount = 0;
        protected static uint GetNewCarID()
        {
            ++carCount;
            return carCount;
        }
        public List<float> GetArrivalData()
        {
            return arrivalData;
        }
    }
}