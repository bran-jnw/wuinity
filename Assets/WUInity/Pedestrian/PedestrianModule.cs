using WUInity.Population;
namespace WUInity.Pedestrian
{
    public abstract class PedestrianModule
    {
        public abstract void Update(float currentTime, float deltaTime);
        public abstract bool SimulationDone();
        public abstract int GetTotalCars();
        public abstract int GetPeopleStaying();
        public abstract int GetPeopleLeft();
        public abstract int GetCarsReached();
    }
}

