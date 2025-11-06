//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using PREACT.Evacuation;

namespace PREACT.IO
{
    [System.Serializable]
    public class PREACTInput
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

        public PREACTInput()
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

        public void SaveToDisk(string file)
        {
            //TODO: fix new format save
            //string json = UnityEngine.JsonUtility.ToJson(WUIEngine.INPUT, true);
            //File.WriteAllText(WUIEngine.WORKING_FILE, json);
            //EvacuationGroup.SaveEvacGroupIndices();
            //GraphicalFireInput.SaveGraphicalFireInput();

            Engine.MESSAGE(null, Engine.LogType.Log, " Input file " + file + " saved.");       
        }

        public static PREACTInput LoadFromDisk(string filePath, out bool success)
        {
            success = false;
            string rootFolder = Path.GetDirectoryName(filePath);
            PREACTInput result = null;
            if(!File.Exists(filePath))
            {
                Engine.MESSAGE(null, Engine.LogType.InputError, " Input file " + filePath + " does not exist.");
            }
            else
            {
                Engine.MESSAGE(null, Engine.LogType.Log, " Reading input file " + filePath + ".");
                PREACTInput input = ParseInput(rootFolder, File.ReadAllLines(filePath), out success);
                if (success)
                {      
                    Engine.MESSAGE(null, Engine.LogType.Log, " Input file " + filePath + " loaded.");
                }
                else
                {
                    Engine.MESSAGE(null, Engine.LogType.Log, " Input file " + filePath + " could not be loaded, see log.");
                    return null;
                }
            }

            success = true;
            return result;
        }

        public static readonly char[] inputSplit = { '=', '#' };
        static readonly char[] headerBrackets = new char[] { '[', ']' };
        public const string pleaseCheckInput = " Please check your input file.";

        private static PREACTInput ParseInput(string rootFolder, string[] inputLines, out bool success)
        {
            success = false;
            PREACTInput newInput = new PREACTInput();
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
            uint criticalIssues;

            //simulation
            input = nameof(Simulation);
            if (headerLineIndex.TryGetValue(input, out lineindex))
            {
                ReadingInputMessage(input);                
                newInput.Simulation = SimulationInput.Parse(inputLines, lineindex, out criticalIssues);
            }
            else
            {
                //critical
                Engine.MESSAGE(null, Engine.LogType.SimulationError, input + " header not found." + pleaseCheckInput);
                return null;
            }
            if(criticalIssues > 0)
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
                Engine.MESSAGE(null, Engine.LogType.Warning, input + " header not found, using defaults.");
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
                    Engine.MESSAGE(null, Engine.LogType.SimulationError, input + " header not found but user has requested pedestrian module." + pleaseCheckInput);
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
                    newInput.Evacuation = EvacuationInput.Parse(rootFolder, inputLines, lineindex);
                }
                else
                {
                    //critical
                    Engine.MESSAGE(null, Engine.LogType.SimulationError, input + " header not found but user has requested pedestrian and/or traffic modules." + pleaseCheckInput);
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
                    Engine.MESSAGE(null, Engine.LogType.SimulationError, input + " header not found but user has requested pedestrian module." + pleaseCheckInput);
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
                    Engine.MESSAGE(null, Engine.LogType.SimulationError, input + " header not found but user has requested traffic module." + pleaseCheckInput);
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
                    Engine.MESSAGE(null, Engine.LogType.SimulationError, input + " header not found but user has requested fire module." + pleaseCheckInput);
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
                    Engine.MESSAGE(null, Engine.LogType.SimulationError, input + " header not found but user has requested smoke module." + pleaseCheckInput);
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
                Engine.MESSAGE(null, Engine.LogType.Warning, input + " header not found, using defaults (disabled).");
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
                Engine.MESSAGE(null, Engine.LogType.Warning, input + " header not found, using defaults (disabled).");
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
                Engine.MESSAGE(null, Engine.LogType.Warning, input + " header not found, no events will be added.");
            }

            success = true;
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
            Engine.MESSAGE(null, Engine.LogType.Log, nameOfInput + " input is being read...");
        }

        public static void InputNotFoundMessage(string nameOfInput)
        {
            Engine.MESSAGE(null, Engine.LogType.SimulationError, nameOfInput + " was not found." + pleaseCheckInput);
        }
        public static void CouldNotInterpretInputMessage(string nameOfInput, string userInput)
        {
            Engine.MESSAGE(null, Engine.LogType.InputError, "Could not interpret user input " + userInput + " for " + nameOfInput + ".");
        }
    }
}

