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
        public string simName = "New file";
        public float deltaTime = 1f;
        public float maxSimTime = 864000f; //10 days
        public bool stopWhenEvacuated = true;
        public int numberOfRuns = 1;
        public Vector2D lowerLeftLatLong = new Vector2D(55.697354, 13.173808);
        public Vector2D size = new Vector2D(3000, 3000);
        public int zoomLevel = 13;
        public bool runInRealTime = true;
        public bool runEvacSim = true;
        public bool runTrafficSim = true;
        public bool runFireSim = true;
        public bool runSmokeSim = true;

        public EvacInput evac;
        public TrafficInput traffic;
        public PopulationInput population;
        public ItineroInput itinero;
        public FarsiteInput farsite;
        public VisualizationOptions visuals;
        public FireInput fire;

        public WUInityInput()
        {
            evac = new EvacInput();
            traffic = new TrafficInput();
            population = new PopulationInput();
            itinero = new ItineroInput();
            farsite = new FarsiteInput();
            visuals = new VisualizationOptions();
            fire = new FireInput();
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

        public float evacuationOrderStart = 420.0f;
        public ResponseCurve[] responseCurves = ResponseCurve.GetStandardCurve();

        public BlockGoalEvent[] blockGoalEvents = BlockGoalEvent.GetDummy();

        public EvacGroup[] evacGroups = EvacGroup.GetDefault();        

        //TODO: fix saving these?
        [System.NonSerialized] public Texture2D evacuationForceTex;
        [System.NonSerialized] public EvacuationGoal[] paintedForcedGoals; //contains all the forced goals per cell        
    }    

    [System.Serializable]
    public class ItineroInput
    {
        
    }

    [System.Serializable]
    public class TrafficInput
    {
        public string osmFile = "";
        public float osmBorderSize = 1000f;

        public enum RouteChoice { Fastest, Closest, Random, EvacGroup };
        public EvacuationGoal[] evacuationGoals = EvacuationGoal.GetRoxburoughGoals();
        public RouteChoice routeChoice = RouteChoice.Closest;
                
        public float stallSpeed = 5f;
        public Vector2 backGroundDensityMinMax = Vector2.zero;
        public bool visibilityAffectsSpeed = false;
        public string opticalDensityFile;
        public float opticalDensity = 0.05f;
        public RoadTypes roadTypes;
        public float saveInterval = 600f;

        public MacroTrafficSim.TrafficAccident[] trafficAccidents = MacroTrafficSim.TrafficAccident.GetDummy();
        public MacroTrafficSim.ReverseLanes[] reverseLanes = MacroTrafficSim.ReverseLanes.GetDummy();
        public TrafficInjection[] trafficInjections = TrafficInjection.GetTemplate();
        public TrafficProbe[] trafficProbes = TrafficProbe.GetTemplate();

        //public string precalcRoutesName = "roxborough";
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
        public string lcpFile, fuelModelsFile;
        public Fire.IgnitionPoint[] ignitionPoints = Fire.IgnitionPoint.GetDefault();
        public Fire.SpreadMode spreadMode = Fire.SpreadMode.SixteenDirections;
        public Fire.WeatherInput weather = Fire.WeatherInput.GetTemplate();
        public Fire.WindInput wind = Fire.WindInput.GetTemplate();
        public Fire.InitialFuelMoistureData initialFuelMoisture = Fire.InitialFuelMoistureData.GetDefaults();
        public float windMultiplier = 1f;

        public bool useRandomIgnitionMap;
        public int randomIgnitionPoints;
        public bool useInitialIgnitionMap;
    }
}

