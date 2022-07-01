using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WUInity.Evac;
using WUInity.Traffic;
using WUInity.Population;

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
        public EvacInput Evacuation;
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
            Evacuation = new EvacInput();
            Traffic = new TrafficInput();    
            Fire = new FireInput();
            Smoke = new SmokeInput();
        }

        public static void SaveInput()
        {
            string json = JsonUtility.ToJson(WUInity.INPUT, true);
            System.IO.File.WriteAllText(WUInity.WORKING_FILE, json);
            EvacGroup.SaveEvacGroupIndices();
            GraphicalFireInput.SaveGraphicalFireInput();

            WUInity.LOG("LOG: Input file " + WUInity.WORKING_FILE + " saved.");
        }

        public static void LoadInput(string path)
        {
            string input = System.IO.File.ReadAllText(path);
            if (input != null)
            {
                WUInityInput wui = JsonUtility.FromJson<WUInityInput>(input);
                WUInity.WORKING_FILE = path;
                WUInity.INSTANCE.SetNewInputData(wui);
                WUInity.LOG("LOG: Input file " + WUInity.WORKING_FILE + " loaded.");
            }
            else
            {
                WUInity.LOG("ERROR: Input file " + path + " not found.");
            }
        }
    }

    [System.Serializable]
    public class SimulationInput
    {
        public string SimDataName = "New_sim";
        public float DeltaTime = 1f;
        public float MaxSimTime = 864000f; //10 days
        public bool StopWhenEvacuated = true;
        //public int numberOfRuns = 1;
        public bool StopAfterConverging = true;
        public Vector2D LowerLeftLatLong = new Vector2D(55.697354, 13.173808);
        public Vector2D Size = new Vector2D(3000, 3000);        
        public bool RunEvacSim = true;
        public bool RunTrafficSim = true;
        public bool RunFireSim = true;
        public bool RunSmokeSim = true;
    }

    [System.Serializable]
    public class PopulationInput
    {
        public string gpwDataFolder = "gpw-v4-population-density-rev10_2015_30_sec_asc";
        public string localGPWFile;
        public string populationFile;
    }

    [System.Serializable]
    public class EvacInput
    {
        public float RouteCellSize = 200f;
        public float evacuationOrderStart = 0.0f;
        public string[] responseCurveFiles;
        public string[] EvacGroupFiles;

        public int minHouseholdSize = 1;
        public int maxHouseholdSize = 5;
        public bool allowMoreThanOneCar = true;
        public int maxCars = 2;
        public float maxCarsChance = 0.3f;               

        public float walkingDistanceModifier = 1.0f;
        public Vector2 walkingSpeedMinMax = new Vector2(0.7f, 1.0f);
        public float walkingSpeedModifier = 1.0f;

        public string[] GoalEventFiles;            

        //TODO: fix saving these?
        //[System.NonSerialized] public Texture2D evacuationForceTex;
        //[System.NonSerialized] public EvacuationGoal[] paintedForcedGoals; //contains all the forced goals per cell        
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
        public enum RouteChoice { Fastest, Closest, Random, EvacGroup };
        public string[] evacuationGoalFiles;
        public RouteChoice routeChoice = RouteChoice.Closest;
                
        public float stallSpeed = 5f;
        public Vector2 backGroundDensityMinMax = Vector2.zero;
        public bool visibilityAffectsSpeed = false;
        public string opticalDensityFile;
        public float opticalDensity = 0.05f;
        public string roadTypesFile;
        public float saveInterval = 600f;

        public MacroTrafficSim.TrafficAccident[] trafficAccidents = MacroTrafficSim.TrafficAccident.GetDummy();
        public MacroTrafficSim.ReverseLanes[] reverseLanes = MacroTrafficSim.ReverseLanes.GetDummy();
        public TrafficInjection[] trafficInjections = TrafficInjection.GetTemplate();
        public TrafficProbe[] trafficProbes = TrafficProbe.GetTemplate();
    }

    [System.Serializable]
    public class FarsiteInput
    {
        public string outputPrefix = "rox";
    }

    [System.Serializable]
    public class VisualizationOptions
    {
        public bool drawRoads = false;
    }

    [System.Serializable]
    public class FireInput
    {
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

        public FarsiteInput FarsiteData;
    }

    [System.Serializable]
    public class SmokeInput
    {
        public float MixingLayerHeight = 250.0f;
    }

    [System.Serializable]
    public class MapInput
    {
        public enum MapServiceProvider { Mapbox, Bing, OSM };
        
        public MapServiceProvider MapProvider = MapServiceProvider.Mapbox;
        public int ZoomLevel = 13;
    }
}

