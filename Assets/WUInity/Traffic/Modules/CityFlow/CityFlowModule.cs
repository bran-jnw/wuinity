using System.Runtime.InteropServices;
using System;

namespace WUInity.Traffic
{
    public class CityFlowModule : TrafficModule
    {
        private IntPtr engine;

        [DllImport("cityflow_unity.dll")]
        private static extern int Test();
        [DllImport("cityflow_unity.dll")]
        private static extern IntPtr CreateEngine(string configFile, int threadNum);
        [DllImport("cityflow_unity.dll")]
        private static extern int NextStep(IntPtr engine);
        [DllImport("cityflow_unity.dll")]
        private static extern int GetVehicleCount(IntPtr engine);
        [DllImport("cityflow_unity.dll")]
        private static extern double GetCurrentTime(IntPtr engine);
        [DllImport("cityflow_unity.dll")]
        private static extern int GetSuccess(IntPtr engine);
        [DllImport("cityflow_unity.dll")]
        private static extern string GetFilePath(IntPtr engine);
        [DllImport("cityflow_unity.dll")]
        private static extern IntPtr GetVehicle(IntPtr engine, int index);
        [DllImport("cityflow_unity.dll")]
        private static extern void GetVehicles(IntPtr engine);

        public CityFlowModule()
        {
            //MonoBehaviour.print(Test());
            engine = CreateEngine("D:\\UNITY\\_PROJECTS\\CityFlow\\examples\\config.json", 1);
            for (int i = 0; i < 600; i++)
            {
                
            }
            //MonoBehaviour.print(GetCurrentTime(engine));
            //MonoBehaviour.print(GetSuccess(engine));
            //print(GetFilePath(engine));
            //print(GetFilePath(engine));
            //print(GetVehicle(engine));
        }

        public override void Update(float deltaTime, float currentTime)
        {
            NextStep(engine);
            int vehicles = GetVehicleCount(engine);
            //GetVehicles(engine);
            for (int j = 0; j < vehicles; ++j)
            {
                IntPtr b = GetVehicle(engine, j);
                string c = Marshal.PtrToStringAnsi(b);
                //MonoBehaviour.print(c);
                //print(vehicles);
            }
        }

        public override bool SimulationDone()
        {
            throw new NotImplementedException();
        }

        public override void InsertNewCar(Vector2d startLatLong, EvacuationGoal evacuationGoal, RouteData routeData, uint numberOfPeopleInCar)
        {
            throw new NotImplementedException();
        }

        public override void InsertNewTrafficEvent(TrafficEvent tE)
        {
            throw new NotImplementedException();
        }

        public override int GetTotalCarsSimulated()
        {
            throw new NotImplementedException();
        }

        public override int GetCarsInSystem()
        {
            throw new NotImplementedException();
        }

        public override void UpdateEvacuationGoals()
        {
            throw new NotImplementedException();
        }

        public override System.Numerics.Vector4[] GetCarPositionsAndStates()
        {
            throw new NotImplementedException();
        }

        public override void SaveToFile(int runNumber)
        {
            throw new NotImplementedException();
        }
    }
}

