using System.Collections.Generic;

namespace WUInity.Traffic
{
    public abstract class TrafficModule
    {
        public abstract void Update(float deltaTime, float currentTime);
        public abstract bool EvacComplete();
        public abstract void InsertNewCar(RouteData routeData, int numberOfPeopleInCar);
        public abstract void InsertNewTrafficEvent(TrafficEvent tE);
        public abstract int GetTotalCarsSimulated();        
        public abstract int GetCarsInSystem();
        public abstract List<float> GetArrivalData();
        public abstract void UpdateEvacuationGoals();
    }
}