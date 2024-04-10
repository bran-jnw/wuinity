using WUIPlatform.Population;
namespace WUIPlatform.Pedestrian
{
    public abstract class PedestrianModule : SimulationModule
    {
        public abstract int GetTotalCars();
        public abstract int GetPeopleStaying();
        public abstract int GetPeopleLeft();
        public abstract int GetCarsReached();
    }
}

