using WUInity.Traffic;
using System.Numerics;

namespace WUInity
{
    [System.Serializable]
    public class WUInityInput
    {    
        public SimulationInput Simulation;
        public MapInput Map;
        public VisualizationOptions Visualization;
        public PopulationInput Population;
        public RoutingInput Routing; 
        public EvacuationInput Evacuation;
        public TrafficInput Traffic;    
        public FireInput Fire;
        public SmokeInput Smoke;

        public WUInityInput()
        {
            Simulation = new SimulationInput();
            Map = new MapInput();
            Visualization = new VisualizationOptions();
            Population = new PopulationInput();
            Routing = new RoutingInput();
            Evacuation = new EvacuationInput();
            Traffic = new TrafficInput();    
            Fire = new FireInput();
            Smoke = new SmokeInput();
        }

        public static void SaveInput()
        {
            string json = UnityEngine.JsonUtility.ToJson(WUInity.INPUT, true);
            System.IO.File.WriteAllText(WUInity.WORKING_FILE, json);
            EvacGroup.SaveEvacGroupIndices();
            GraphicalFireInput.SaveGraphicalFireInput();

            WUInity.CONSOLE(WUInity.LogType.Log, " Input file " + WUInity.WORKING_FILE + " saved.");
        }

        public static void LoadInput(string path)
        {
            string input = System.IO.File.ReadAllText(path);
            if (input != null)
            {                
                WUInityInput wui = UnityEngine.JsonUtility.FromJson<WUInityInput>(input);

                WUInity.CONSOLE(WUInity.LogType.Log, "Smoek enabled? " + wui.Simulation.RunSmokeModule);

                WUInity.WORKING_FILE = path;
                WUInity.CONSOLE(WUInity.LogType.Log, " Reading input file " + WUInity.WORKING_FILE + ".");
                WUInity.INSTANCE.SetNewInputData(wui);
                WUInity.CONSOLE(WUInity.LogType.Log, " Input file " + WUInity.WORKING_FILE + " loaded.");
            }
            else
            {
                WUInity.CONSOLE(WUInity.LogType.Error, " Input file " + path + " not found.");
            }
        }
    }

    [System.Serializable]
    public class SimulationInput
    {
        public string SimulationID = "New_sim";
        public float DeltaTime = 1f;
        public float MaxSimTime = 864000f; //10 days
        public bool StopWhenEvacuated = true;
        //public int numberOfRuns = 1;
        public bool StopAfterConverging = true;
        public Vector2d LowerLeftLatLong = new Vector2d(55.697354, 13.173808);
        public Vector2d Size = new Vector2d(3000.0, 3000.0);        
        public bool RunPedestrianModule = true;
        public bool RunTrafficModule = true;
        public bool RunFireModule = true;
        public bool RunSmokeModule = false;
    }

    [System.Serializable]
    public class PopulationInput
    {
        public string gpwDataFolder = "gpw-v4-population-density-rev10_2015_30_sec_asc";
        public string localGPWFile;
        public string populationFile;
    }

    [System.Serializable]
    public class EvacuationInput
    {
        public enum PedestrianModuleChoice { MacroHouseholdSim, SUMO }
        public PedestrianModuleChoice pedestrianModuleChoice = PedestrianModuleChoice.MacroHouseholdSim;

        public float RouteCellSize = 200f;
        public float EvacuationOrderStart = 0.0f;
        public string[] ResponseCurveFiles;
        public string[] EvacGroupFiles;

        public int minHouseholdSize = 1;
        public int maxHouseholdSize = 5;
        public bool allowMoreThanOneCar = true;
        public int maxCars = 2;
        public float maxCarsChance = 0.3f;               

        public float walkingDistanceModifier = 1.0f;
        public Vector2 walkingSpeedMinMax = new Vector2(0.7f, 1.0f);
        public float walkingSpeedModifier = 1.0f;

        public string[] BlockGoalEventFiles;            

        //TODO: fix saving these?
        //[System.NonSerialized] public Texture2D evacuationForceTex;
        //[System.NonSerialized] public EvacuationGoal[] paintedForcedGoals; //contains all the forced goals per cell        
    }

    [System.Serializable]
    public class MacroHouseholdSimInput
    {

    }

    [System.Serializable]
    public class RoutingInput
    {
        public string routerDbFile = "example.routerdb";
        public string routeCollectionFile = "example.rc";
    }

    [System.Serializable]
    public class TrafficInput
    {     
        public enum TrafficModuleChoice { MacroTrafficSim, SUMO }
        public TrafficModuleChoice trafficModuleChoice = TrafficModuleChoice.MacroTrafficSim;        
        public enum RouteChoice { Fastest, Closest, Random, EvacGroup };
        public string[] evacuationGoalFiles;
        public RouteChoice routeChoice = RouteChoice.Closest;
                
        public float stallSpeed = 5f;
        public Vector2 backGroundDensityMinMax = Vector2.Zero;
        public bool visibilityAffectsSpeed = false;
        public string opticalDensityFile;
        public float opticalDensity = 0.05f;
        public string roadTypesFile;
        public float saveInterval = 600f;

        public TrafficAccident[] trafficAccidents = TrafficAccident.GetDummy();
        public ReverseLanes[] reverseLanes = ReverseLanes.GetDummy();
        public TrafficInjection[] trafficInjections = TrafficInjection.GetTemplate();
        public TrafficProbe[] trafficProbes = TrafficProbe.GetTemplate();

        public SUMOInput sumoInput;
    }

    [System.Serializable]
    public class SUMOInput
    {
        public string inputFile;
        public Vector2d UTMoffset;
    }

    [System.Serializable]
    public class VisualizationOptions
    {
        public bool drawRoads = false;
    }

    [System.Serializable]
    public class FireInput
    {
        public enum FireModuleChoice { Cells, VectorCells, FarsiteDLL, FarsiteOffline, PrometheusCOM, WISEOffline }
        public FireModuleChoice fireModuleChoice = FireModuleChoice.Cells;
        public string lcpFile;
        public string fuelModelsFile = "default.fuel";
        public string initialFuelMoistureFile = "default.fmc";
        public string weatherFile = "default.wtr";
        public string windFile = "default.wnd";
        public string ignitionPointsFile = "default.ign";
        public string graphicalFireInputFile = "default.gfi";
        public Fire.SpreadMode spreadMode = Fire.SpreadMode.SixteenDirections;
              
        public float windMultiplier = 1f;

        public bool useRandomIgnitionMap = false;
        public int randomIgnitionPoints = 0;
        public bool useInitialIgnitionMap = false;

        public FarsiteInput farsiteData;
    }

    [System.Serializable]
    public class FarsiteInput
    {
        public string rootFolder;
    }

    [System.Serializable]
    public class SmokeInput
    {
        public enum SmokeModuleChoice { AdvectDiffuse, BoxModel, Lagrangian, GaussianPuff, GaussianPlume, FFD}
        public SmokeModuleChoice smokeModuleChoice = SmokeModuleChoice.AdvectDiffuse;
        public float MixingLayerHeight = 250.0f;
    }

    [System.Serializable]
    public class MapInput
    {
        public enum MapServiceProvider { Mapbox, Bing, OSM };
        
        public MapServiceProvider mapProvider = MapServiceProvider.Mapbox;
        public int zoomLevel = 13;
    }
}

