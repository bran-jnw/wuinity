//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Numerics;
using WUIPlatform.Traffic;
using System.Collections.Generic;
using System.IO;

namespace WUIPlatform.IO
{
    [System.Serializable]
    public class WUIEngineInput
    {    
        public SimulationInput Simulation;
        public MapInput Map;                
        public PopulationInput Population;
        public EvacuationInput Evacuation;
        public PedestrianInput Pedestrian;
        public TrafficInput Traffic;    
        public FireInput Fire;
        public SmokeInput Smoke;
        public WUIShowInput WUIShow;

        public WUIEngineInput()
        {
            Simulation = new SimulationInput();
            Map = new MapInput();            
            Population = new PopulationInput();
            Evacuation = new EvacuationInput();
            Pedestrian = new PedestrianInput();
            Traffic = new TrafficInput();    
            Fire = new FireInput();
            Smoke = new SmokeInput();
            WUIShow = new WUIShowInput();
        }

        public static void SaveInput()
        {
            string json = UnityEngine.JsonUtility.ToJson(WUIEngine.INPUT, true);
            File.WriteAllText(WUIEngine.WORKING_FILE, json);
            EvacGroup.SaveEvacGroupIndices();
            GraphicalFireInput.SaveGraphicalFireInput();

            WUIEngine.LOG(WUIEngine.LogType.Log, " Input file " + WUIEngine.WORKING_FILE + " saved.");
        }

        public static void LoadInput(string path)
        {
            string input = File.ReadAllText(path);
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

            ParseInput(File.ReadAllLines(path));
        }

        //stuff for parsing
        const string _simulationHeader = "Simulation";
        const string _mapHeader = "Map";
        const string _populationHeader = "Population";
        const string _evacuationHeader = "Evacuation";
        const string _pedestrianHeader = "Pedestrian";
        const string _trafficHeader = "Traffic";
        const string _fireHeader = "Fire";
        const string _smokeHeader = "Smoke";
        const string _wuiShowHeader = "WUIShow";

        public static readonly char[] inputSplit = { '=', '#' };
        static readonly char[] headerBrackets = new char[] { '[', ']' };

        private static WUIEngineInput ParseInput(string[] inputLines)
        {
            WUIEngineInput newInput = new WUIEngineInput();
            Dictionary<string, int> headerLineIndex = new Dictionary<string, int>();

            //first index all headers
            for(int i = 0; i < inputLines.Length; ++i)
            {
                string line = inputLines[i].Trim();

                if (line.StartsWith("["))
                {
                    line = line.Trim(headerBrackets);
                    headerLineIndex.Add(line, i);
                }
            }

            //simulation
            int lineindex;
            if (headerLineIndex.TryGetValue(_simulationHeader, out lineindex))
            {
                newInput.Simulation = SimulationInput.Parse(inputLines, lineindex);
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, _simulationHeader + " header not found, please check your input file.");
                return null;
            }

            //map
            if (headerLineIndex.TryGetValue(_mapHeader, out lineindex))
            {
                newInput.Map = MapInput.Parse(inputLines, lineindex);
            }
            else
            {
                newInput.Map = new MapInput();
                WUIEngine.LOG(WUIEngine.LogType.Warning, "No [Map] header found in input file, using defaults.");
            }

            //population
            if (headerLineIndex.TryGetValue(_populationHeader, out lineindex))
            {
                newInput.Population = PopulationInput.Parse(inputLines, lineindex);
            }
            else
            {
                
            }

            //evacuation
            if (headerLineIndex.TryGetValue(_evacuationHeader, out lineindex))
            {
                newInput.Evacuation = EvacuationInput.Parse(inputLines, lineindex);
            }
            else
            {

            }

            return newInput;
        }

        /// <summary>
        /// Reads all input under header until next header is found.
        /// </summary>
        /// <param name="inputLines"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetHeaderInput(string[] inputLines, int startIndex)
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

                string[] input = line.Split(inputSplit);
                if (input.Length >= 2)
                {
                    inputToParse.Add(input[0], input[1]);
                }
                ++lineIndex;
            }

            return inputToParse;
        }
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

        public AscImportInput ascData;

        public bool calculateTriggerBuffer = false;
        public enum TriggerBufferChoice { kPERIL, backwardsCell}
        public TriggerBufferChoice triggerBufferChoice= TriggerBufferChoice.kPERIL;
        public float kPerilMidFlameWindspeed = 0f;
        public bool calculateROSFromBehave = true;

    }

    [System.Serializable]
    public class AscImportInput
    {
        public string rootFolder;
        public string timeOfArrival;
        public string rateOfSpread;
        public string spreadDirection;
        public string firelineIntensity;
        public string weatherStream;
    }
}

