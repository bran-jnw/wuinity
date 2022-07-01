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
        public string simDataName = "New_sim";
        public float deltaTime = 1f;
        public float maxSimTime = 864000f; //10 days
        public bool stopWhenEvacuated = true;
        //public int numberOfRuns = 1;
        public bool stopAfterConverging = true;        
        public Vector2D lowerLeftLatLong = new Vector2D(55.697354, 13.173808);
        public Vector2D size = new Vector2D(3000, 3000);
        public int zoomLevel = 13;
        public bool runEvacSim = true;
        public bool runTrafficSim = true;
        public bool runFireSim = true;
        public bool runSmokeSim = true;

        public EvacInput evac;
        public TrafficInput traffic;
        public PopulationInput population;
        public RoutingInput routing;
        public VisualizationOptions visualization;
        public FireInput fire;

        public WUInityInput()
        {
            evac = new EvacInput();
            traffic = new TrafficInput();
            population = new PopulationInput();
            routing = new RoutingInput();
            visualization = new VisualizationOptions();
            fire = new FireInput();
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
    public class PopulationInput
    {
        public string gpwDataFolder = "gpw-v4-population-density-rev10_2015_30_sec_asc";
        public string localGPWFile;
        public string populationFile;
    }

    [System.Serializable]
    public class EvacInput
    {
        public float routeCellSize = 200f;

        //public Vector2Int routeCellCount;

        public bool allowMoreThanOneCar = true;
        public int maxCars = 2;
        public float maxCarsChance = 0.3f;

        public int minHouseholdSize = 1;
        public int maxHouseholdSize = 5;

        public float walkingDistanceModifier = 1.0f;
        public Vector2 walkingSpeedMinMax = new Vector2(0.7f, 1.0f);
        public float walkingSpeedModifier = 1.0f;

        public float evacuationOrderStart = 0.0f;
        public string[] responseCurveFiles;

        public BlockGoalEvent[] blockGoalEvents = BlockGoalEvent.GetDummy();

        public string[] evacGroupFiles;      

        //TODO: fix saving these?
        [System.NonSerialized] public Texture2D evacuationForceTex;
        [System.NonSerialized] public EvacuationGoal[] paintedForcedGoals; //contains all the forced goals per cell        
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
        public string fuelModelsFile = "default";
        public string initialFuelMoistureFile = "default";
        public string weatherFile;
        public string windFile;
        public string ignitionPointsFile;
        public string graphicalFireInputFile;
        public Fire.SpreadMode spreadMode = Fire.SpreadMode.SixteenDirections;
              
        public float windMultiplier = 1f;

        public bool useRandomIgnitionMap;
        public int randomIgnitionPoints;
        public bool useInitialIgnitionMap;

        public FarsiteInput farsite;
    }    
}

