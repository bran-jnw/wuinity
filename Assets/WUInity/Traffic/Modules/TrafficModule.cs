using System.Collections.Generic;
using System.Numerics;

namespace WUInity.Traffic
{
    public abstract class TrafficModule : SimulationModule
    {
        protected List<float> arrivalData;
        protected LinkedList<InjectedCar> carsToInject;

        protected struct InjectedCar
        {
            public Vector2d startLatLong;
            public EvacuationGoal evacuationGoal;
            public RouteData routeData;
            public uint numberOfPeopleInCar;

            public InjectedCar(Vector2d startLatLong, EvacuationGoal evacuationGoal, RouteData routeData, uint numberOfPeopleInCar)
            {
                this.startLatLong = startLatLong;
                this.evacuationGoal = evacuationGoal;
                this.routeData = routeData;
                this.numberOfPeopleInCar = numberOfPeopleInCar;
            }
        }

        public TrafficModule()
        {
            arrivalData = new List<float>();
            carsToInject = new LinkedList<InjectedCar>();
        }

        /// <summary>
        /// Inject new car into the simulation and puts it in a waiting list (as this happens during a simulation step). Must be "consumed" later with PostUpdate().
        /// </summary>
        /// <param name="startLatLong"></param>
        /// <param name="evacuationGoal"></param>
        /// <param name="routeData"></param>
        /// <param name="numberOfPeopleInCar"></param>
        public void InsertNewCar(Vector2d startLatLong, EvacuationGoal evacuationGoal, RouteData routeData, uint numberOfPeopleInCar)
        {
            carsToInject.AddLast(new InjectedCar(startLatLong, evacuationGoal, routeData, numberOfPeopleInCar));
        }

        public abstract void PostUpdate();
        
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