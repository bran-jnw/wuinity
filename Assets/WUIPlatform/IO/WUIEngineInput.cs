//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Numerics;
using WUIPlatform.Traffic;
using System.Collections.Generic;

namespace WUIPlatform.IO
{
    [System.Serializable]
    public class WUIEngineInput
    {    
        public SimulationInput Simulation;
        public MapInput Map;
        public WUIShowInput WUIShow;
        public PopulationInput Population;
        public RoutingInput Routing; 
        public EvacuationInput Evacuation;
        public TrafficInput Traffic;    
        public FireInput Fire;
        public SmokeInput Smoke;

        public static readonly char[] inputSplit = { '=', '#' };

        public WUIEngineInput()
        {
            Simulation = new SimulationInput();
            Map = new MapInput();
            WUIShow = new WUIShowInput();
            Population = new PopulationInput();
            Routing = new RoutingInput();
            Evacuation = new EvacuationInput();
            Traffic = new TrafficInput();    
            Fire = new FireInput();
            Smoke = new SmokeInput();
        }

        public static void SaveInput()
        {
            string json = UnityEngine.JsonUtility.ToJson(WUIEngine.INPUT, true);
            System.IO.File.WriteAllText(WUIEngine.WORKING_FILE, json);
            EvacGroup.SaveEvacGroupIndices();
            GraphicalFireInput.SaveGraphicalFireInput();

            WUIEngine.LOG(WUIEngine.LogType.Log, " Input file " + WUIEngine.WORKING_FILE + " saved.");
        }

        public static void LoadInput(string path)
        {
            string input = System.IO.File.ReadAllText(path);
            if (input != null)
            {                
                WUIEngineInput wui = UnityEngine.JsonUtility.FromJson<WUIEngineInput>(input);
                WUIEngine.WORKING_FILE = path;
                WUIEngine.LOG(WUIEngine.LogType.Log, " Reading input file " + WUIEngine.WORKING_FILE + ".");
                WUIEngine.ENGINE.SetNewInputData(wui);
                WUIEngine.LOG(WUIEngine.LogType.Log, " Input file " + WUIEngine.WORKING_FILE + " loaded.");
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, " Input file " + path + " not found.");
            }
        }

        private static WUIEngineInput ParseInput(string[] inputLines)
        {
            WUIEngineInput wuiIn = new WUIEngineInput();

            for(int i = 0; i < inputLines.Length; ++i)
            {
                if (inputLines[i].StartsWith("[Simulation"))
                {
                    wuiIn.Simulation = SimulationInput.Parse(inputLines, i);
                }
                else if(inputLines[i].StartsWith("[WUIShow"))
                {
                    wuiIn.WUIShow = WUIShowInput.Parse(inputLines, i);
                }
            }

            return wuiIn;
        }

        public static Dictionary<string, string> GetCategoryInput(string[] inputLines, int startIndex)
        {
            Dictionary<string, string> inputToParse = new Dictionary<string, string>();
            //first line is header
            int lineIndex = startIndex + 1;
            while (true)
            {
                if (lineIndex >= inputLines.Length)
                {
                    break;
                }
                string line = inputLines[lineIndex].Trim();
                //we have found next header, exit
                if (line.StartsWith('['))
                {
                    break;
                }
                //empty or comment
                if (line.Length == 0 || line.StartsWith('#'))
                {
                    continue;
                }

                string[] input = line.Split(WUIEngineInput.inputSplit);
                if (input.Length > 1)
                {
                    inputToParse.Add(input[0], input[1]);
                }
                ++lineIndex;
            }

            return inputToParse;
        }
    }

    

    

    [System.Serializable]
    public class PopulationInput
    {
        public string HouseholdsFile;
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
    public class RoutingInput
    {
        public string routerDbFile = "example.routerdb";
    }

    [System.Serializable]
    public class TrafficInput
    {
        public enum TrafficModuleChoice { SUMO, MacroTrafficSim }
        public TrafficModuleChoice trafficModuleChoice = TrafficModuleChoice.SUMO;        
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
    public class FireInput
    {
        public enum FireModuleChoice { AscImport, Cells, VectorCells, FarsiteDLL, PrometheusCOM }
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

        public AscInput ascData;

        public bool calculateTriggerBuffer = false;
        public enum TriggerBufferChoice { kPERIL, backwardsCell}
        public TriggerBufferChoice triggerBufferChoice= TriggerBufferChoice.kPERIL;
        public float kPerilMidFlameWindspeed = 0f;
        public bool calculateROSFromBehave = true;

    }

    [System.Serializable]
    public class AscInput
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

