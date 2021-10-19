using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WUInity.Evac;
using WUInity.Traffic;
using WUInity.GPW;

namespace WUInity
{
    [System.Serializable]
    public class WUInityInput
    {
        public string simName = "rox";
        public float deltaTime = 1f;
        public float maxSimTime = 864000f; //10 days
        public bool stopWhenEvacuated = true;
        public int numberOfRuns = 1;
        public Vector2D lowerLeftLatLong = new Vector2D(39.409924, -105.104505);
        public Vector2D size = new Vector2D(10000, 10000);
        public int zoomLevel = 13;
        public bool runInRealTime = false;

        public EvacInput evac;
        public TrafficInput traffic;
        public GPWInput gpw;
        public ItineroInput itinero;
        public FarsiteInput farsite;
        public VisualizationOptions visuals;
        public FireInput fire;
    }

    [System.Serializable]
    public class GPWInput
    {
        public bool readGPWFromSave = true;
        public string localGPWFilename = "roxburough.gpw";
    }

    [System.Serializable]
    public class EvacInput
    {
        public bool overrideTotalPopulation = true;
        public int totalPopulation = 500;
        public float routeCellSize = 200f;

        public Vector2Int routeCellCount;

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
        [System.NonSerialized] public int[] evacGroupIndices;

        //TODO: fix saving these?
        [System.NonSerialized] public Texture2D evacuationForceTex;
        [System.NonSerialized] public EvacuationGoal[] paintedForcedGoals; //contains all the forced goals per cell        
    }    

    [System.Serializable]
    public class ItineroInput
    {
        public string osmDataName = "colorado-latest";
        public string routerDatabaseName = "colorado-latest";
        public float osmBorderSize = 1000f;
    }

    [System.Serializable]
    public class TrafficInput
    {
        public enum RouteChoice { Fastest, Closest, ForceMap, Random, WeightedRandom, EvacGroup };

        public EvacuationGoal[] evacuationGoals = EvacuationGoal.GetRoxburoughGoals();
        public RouteChoice routeChoice = RouteChoice.Closest;
        //public float jamModifier = 0.05f;
        public float stallSpeed = 5f;
        public Vector2 backGroundDensityMinMax = Vector2.zero;
        public bool visibilityAffectsSpeed = false;
        public float opticalDensity = 0.05f;
        public RoadTypes roadTypes;
        public float saveInterval = 600f;

        public MacroTrafficSim.TrafficAccident[] trafficAccidents = MacroTrafficSim.TrafficAccident.GetDummy();
        public MacroTrafficSim.ReverseLanes[] reverseLanes = MacroTrafficSim.ReverseLanes.GetDummy();
        public TrafficInjection[] trafficInjections = TrafficInjection.GetTemplate();

        public string precalcRoutesName = "roxborough";
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
        public string lcpFileName;
        public Fire.WUInityFireIgnition[] ignitionPoints = Fire.WUInityFireIgnition.GetDefault();
        public Fire.SpreadMode spreadMode = Fire.SpreadMode.SixteenDirections;
        public Fire.WeatherInput weather = Fire.WeatherInput.GetTemplate();
        public Fire.WindInput wind = Fire.WindInput.GetTemplate();
        public Fire.InitialFuelMoistureData initialFuelMoisture = Fire.InitialFuelMoistureData.GetDefaults();
        public float windMultiplier = 1f;
    }
}

