//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

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
        public TriggerBufferInput TriggerBuffer;
        public WUIShowInput WUIShow;
        public EventsInput Events;

        public WUIEngineInput()
        {
            /*Simulation = new SimulationInput();
            Map = new MapInput();            
            Population = new PopulationInput();
            Evacuation = new EvacuationInput();
            Pedestrian = new PedestrianInput();
            Traffic = new TrafficInput();    
            Fire = new FireInput();            
            Smoke = new SmokeInput();
            TriggerBuffer = new TriggerBufferInput();
            WUIShow = new WUIShowInput();*/
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
        /*const string _simulationHeader = "Simulation";
        const string _mapHeader = "Map";
        const string _populationHeader = "Population";
        const string _evacuationHeader = "Evacuation";
        const string _pedestrianHeader = "Pedestrian";
        const string _trafficHeader = "Traffic";
        const string _fireHeader = "Fire";
        const string _smokeHeader = "Smoke";
        const string _wuiShowHeader = "WUIShow";*/

        public static readonly char[] inputSplit = { '=', '#' };
        static readonly char[] headerBrackets = new char[] { '[', ']' };
        public const string pleaseCheckInput = " Please check your input file.";

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

            //now see if we have what we need
            int lineindex;
            //simulation
            if (headerLineIndex.TryGetValue(nameof(Simulation), out lineindex))
            {
                newInput.Simulation = SimulationInput.Parse(inputLines, lineindex);
            }
            else
            {
                //critical
                WUIEngine.LOG(WUIEngine.LogType.Error, nameof(Simulation) + " header not found." + pleaseCheckInput);
                return null;
            }

            //map
            if (headerLineIndex.TryGetValue(nameof(Map), out lineindex))
            {
                newInput.Map = MapInput.Parse(inputLines, lineindex);
            }
            else
            {
                //does not matter
                newInput.Map = new MapInput();
                WUIEngine.LOG(WUIEngine.LogType.Warning, nameof(Map) + " header not found, using defaults.");
            }

            //population
            if (newInput.Simulation.RunPedestrianModule)
            {
                if (headerLineIndex.TryGetValue(nameof(Population), out lineindex))
                {
                    newInput.Population = PopulationInput.Parse(inputLines, lineindex);
                }
                else
                {
                    //critical
                    WUIEngine.LOG(WUIEngine.LogType.Error, nameof(Population) + " header not found but user has requested pedestrian module." + pleaseCheckInput);
                    return null;
                }      
            }

            //evacuation
            if (newInput.Simulation.RunPedestrianModule || newInput.Simulation.RunTrafficModule)
            {
                if (headerLineIndex.TryGetValue(nameof(Evacuation), out lineindex))
                {
                    newInput.Evacuation = EvacuationInput.Parse(inputLines, lineindex);
                }
                else
                {
                    //critical
                    WUIEngine.LOG(WUIEngine.LogType.Error, nameof(Evacuation) + " header not found but user has requested pedestrian and/or traffic modules." + pleaseCheckInput);
                    return null;
                }
            }                

            //pedestrian
            if(newInput.Simulation.RunPedestrianModule)
            {
                if (headerLineIndex.TryGetValue(nameof(Pedestrian), out lineindex))
                {
                    newInput.Pedestrian = PedestrianInput.Parse(inputLines, lineindex, headerLineIndex);
                }
                else
                {
                    //critical
                    WUIEngine.LOG(WUIEngine.LogType.Error, nameof(Pedestrian) + " header not found but user has requested pedestrian module." + pleaseCheckInput);
                    return null;
                }
            }

            //traffic
            if (newInput.Simulation.RunTrafficModule)
            {
                if (headerLineIndex.TryGetValue(nameof(Traffic), out lineindex))
                {
                    newInput.Traffic = TrafficInput.Parse(inputLines, lineindex);
                }
                else
                {
                    //critical
                    WUIEngine.LOG(WUIEngine.LogType.Error, nameof(Traffic) + " header not found but user has requested traffic module." + pleaseCheckInput);
                    return null;
                }
            }

            //fire
            if (newInput.Simulation.RunFireModule)
            {
                if (headerLineIndex.TryGetValue(nameof(Fire), out lineindex))
                {
                    newInput.Fire = FireInput.Parse(inputLines, lineindex);
                }
                else
                {
                    //critical                
                    WUIEngine.LOG(WUIEngine.LogType.Error, nameof(Fire) + " header not found but user has requested fire module." + pleaseCheckInput);
                    return null;
                }
            }

            //smoke
            if (newInput.Simulation.RunSmokeModule)
            {
                if (headerLineIndex.TryGetValue(nameof(Smoke), out lineindex))
                {
                    newInput.Smoke = SmokeInput.Parse(inputLines, lineindex);
                }
                else
                {
                    //critical
                    WUIEngine.LOG(WUIEngine.LogType.Error, nameof(Smoke) + " header not found but user has requested smoke module." + pleaseCheckInput);
                    return null;
                }
            }

            //trigger buffer
            if (headerLineIndex.TryGetValue(nameof(TriggerBuffer), out lineindex))
            {
                newInput.TriggerBuffer = TriggerBufferInput.Parse(inputLines, lineindex);
            }
            else
            {
                //does not matter
                newInput.TriggerBuffer = new TriggerBufferInput();
                WUIEngine.LOG(WUIEngine.LogType.Warning, nameof(TriggerBuffer) + " header not found, using defaults.");
            }

            //WUIShow
            if (headerLineIndex.TryGetValue(nameof(WUIShow), out lineindex))
            {
                newInput.WUIShow = WUIShowInput.Parse(inputLines, lineindex);
            }
            else
            {
                //does not matter
                newInput.WUIShow = new WUIShowInput();
                WUIEngine.LOG(WUIEngine.LogType.Warning, nameof(WUIShow) + " header not found, using defaults.");
            }

            //events
            if (headerLineIndex.TryGetValue(nameof(Events), out lineindex))
            {
                newInput.Events = EventsInput.Parse(inputLines, lineindex);
            }
            else
            {
                //does not matter
                newInput.WUIShow = new WUIShowInput();
                WUIEngine.LOG(WUIEngine.LogType.Warning, nameof(WUIShow) + " header not found, using defaults.");
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
}

