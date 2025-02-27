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

        const bool _useNewInputFormat = true;
        public static void LoadInput(string path)
        {
            if(!File.Exists(path))
            {
                WUIEngine.LOG(WUIEngine.LogType.InputError, " Input file " + path + " does not exist.");
                return;
            }

            if(_useNewInputFormat)
            {
                WUIEngine.LOG(WUIEngine.LogType.Log, " Reading input file " + path + ".");
                WUIEngineInput wui = ParseInput(File.ReadAllLines(path));
                if(wui != null)
                {
                    WUIEngine.WORKING_FILE = path;
                    WUIEngine.ENGINE.SetNewInputData(wui);
                    WUIEngine.LOG(WUIEngine.LogType.Log, " Input file " + WUIEngine.WORKING_FILE + " loaded.");
                }                
                else
                {
                    WUIEngine.LOG(WUIEngine.LogType.Log, " Input file " + WUIEngine.WORKING_FILE + " could not be loaded, see log.");
                }
            }
            else
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
                    WUIEngine.LOG(WUIEngine.LogType.SimError, " Input file " + path + " not found.");
                }
            }
        }

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
                inputLines[i] = inputLines[i].Trim();
                string line = inputLines[i];
                if (inputLines[i].StartsWith("["))
                {
                    line = line.Trim(headerBrackets);
                    headerLineIndex.Add(line, i);
                }
            }

            //now see if we have what we need
            int lineindex;
            string input;

            //simulation
            input = nameof(Simulation);
            if (headerLineIndex.TryGetValue(input, out lineindex))
            {
                ReadingInputMessage(input);                
                newInput.Simulation = SimulationInput.Parse(inputLines, lineindex);
            }
            else
            {
                //critical
                WUIEngine.LOG(WUIEngine.LogType.SimError, input + " header not found." + pleaseCheckInput);
                return null;
            }
            if(newInput.Simulation == null)
            {
                return null;
            }

            //map
            input = nameof(Map);
            if (headerLineIndex.TryGetValue(input, out lineindex))
            {
                ReadingInputMessage(input);
                newInput.Map = MapInput.Parse(inputLines, lineindex);
            }
            else
            {
                //does not matter
                newInput.Map = new MapInput();
                WUIEngine.LOG(WUIEngine.LogType.Warning, input + " header not found, using defaults.");
            }            

            //population            
            if (newInput.Simulation.RunPedestrianModule)
            {
                input = nameof(Population);
                if (headerLineIndex.TryGetValue(input, out lineindex))
                {
                    ReadingInputMessage(input);
                    newInput.Population = PopulationInput.Parse(inputLines, lineindex);
                }
                else
                {
                    //critical
                    WUIEngine.LOG(WUIEngine.LogType.SimError, input + " header not found but user has requested pedestrian module." + pleaseCheckInput);
                    return null;
                }      
            }

            //evacuation
            if (newInput.Simulation.RunPedestrianModule || newInput.Simulation.RunTrafficModule)
            {
                input = nameof(Evacuation);
                if (headerLineIndex.TryGetValue(input, out lineindex))
                {                    
                    ReadingInputMessage(input);
                    newInput.Evacuation = EvacuationInput.Parse(inputLines, lineindex);
                }
                else
                {
                    //critical
                    WUIEngine.LOG(WUIEngine.LogType.SimError, input + " header not found but user has requested pedestrian and/or traffic modules." + pleaseCheckInput);
                    return null;
                }
            }                

            //pedestrian
            if(newInput.Simulation.RunPedestrianModule)
            {
                input = nameof(Pedestrian);
                if (headerLineIndex.TryGetValue(input, out lineindex))
                {
                    ReadingInputMessage(input);
                    newInput.Pedestrian = PedestrianInput.Parse(inputLines, lineindex, headerLineIndex);
                }
                else
                {
                    //critical
                    WUIEngine.LOG(WUIEngine.LogType.SimError, input + " header not found but user has requested pedestrian module." + pleaseCheckInput);
                    return null;
                }
            }

            //traffic
            if (newInput.Simulation.RunTrafficModule)
            {
                input = nameof(Traffic);
                if (headerLineIndex.TryGetValue(input, out lineindex))
                {
                    ReadingInputMessage(input);
                    newInput.Traffic = TrafficInput.Parse(inputLines, lineindex, headerLineIndex);
                }
                else
                {
                    //critical
                    WUIEngine.LOG(WUIEngine.LogType.SimError, input + " header not found but user has requested traffic module." + pleaseCheckInput);
                    return null;
                }
            }

            //fire
            if (newInput.Simulation.RunFireModule)
            {
                input = nameof(Fire);
                if (headerLineIndex.TryGetValue(input, out lineindex))
                {
                    ReadingInputMessage(input);
                    newInput.Fire = FireInput.Parse(inputLines, lineindex, headerLineIndex);
                }
                else
                {
                    //critical                
                    WUIEngine.LOG(WUIEngine.LogType.SimError, input + " header not found but user has requested fire module." + pleaseCheckInput);
                    return null;
                }
            }

            //smoke
            if (newInput.Simulation.RunSmokeModule)
            {
                input = nameof(Smoke);
                if (headerLineIndex.TryGetValue(input, out lineindex))
                {
                    ReadingInputMessage(input);
                    newInput.Smoke = SmokeInput.Parse(inputLines, lineindex, headerLineIndex);
                }
                else
                {
                    //critical
                    WUIEngine.LOG(WUIEngine.LogType.SimError, input + " header not found but user has requested smoke module." + pleaseCheckInput);
                    return null;
                }
            }

            //trigger buffer
            input = nameof(TriggerBuffer);
            if (headerLineIndex.TryGetValue(input, out lineindex))
            {
                ReadingInputMessage(input);
                newInput.TriggerBuffer = TriggerBufferInput.Parse(inputLines, lineindex, headerLineIndex);
            }
            else
            {
                //does not matter, not active per default
                newInput.TriggerBuffer = new TriggerBufferInput();
                WUIEngine.LOG(WUIEngine.LogType.Warning, input + " header not found, using defaults (disabled).");
            }

            //WUIShow
            input = nameof(WUIShow);
            if (headerLineIndex.TryGetValue(input, out lineindex))
            {
                ReadingInputMessage(input);
                newInput.WUIShow = WUIShowInput.Parse(inputLines, lineindex);
            }
            else
            {
                //does not matter
                newInput.WUIShow = new WUIShowInput();
                WUIEngine.LOG(WUIEngine.LogType.Warning, input + " header not found, using defaults (disabled).");
            }

            //events
            input = nameof(Events);
            if (headerLineIndex.TryGetValue(input, out lineindex))
            {
                ReadingInputMessage(input);
                newInput.Events = EventsInput.Parse(inputLines, lineindex);
            }
            else
            {
                //does not matter
                newInput.Events = new EventsInput();
                WUIEngine.LOG(WUIEngine.LogType.Warning, input + " header not found, will do nothing.");
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

                string line = inputLines[lineIndex];
                //we have found next header, exit
                if (line.StartsWith('['))
                {
                    break;
                }
                //empty or comment
                if (line.Length == 0 || line.StartsWith('#'))
                {
                    ++lineIndex;
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

        public static void ReadingInputMessage(string nameOfInput)
        {
            WUIEngine.LOG(WUIEngine.LogType.Log, nameOfInput + " input is being read...");
        }

        public static void InputNotFoundMessage(string nameOfInput)
        {
            WUIEngine.LOG(WUIEngine.LogType.SimError, nameOfInput + " was not found." + pleaseCheckInput);
        }
        public static void CouldNotInterpretInputMessage(string nameOfInput, string userInput)
        {
            WUIEngine.LOG(WUIEngine.LogType.InputError, "Could not interpret user input " + userInput + " for " + nameOfInput + ".");
        }
    }
}

