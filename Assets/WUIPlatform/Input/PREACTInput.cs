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
        public string RootFolder;
        public SimulationInput Simulation;
        public MapInput Map;                
        public PopulationInput Population;
        public EventsInput Events;
        public EvacuationInput Evacuation;
        public PedestrianInput Pedestrian;
        public TrafficInput Traffic;    
        public FireInput Fire;        
        public SmokeInput Smoke;
        public TriggerBufferInput TriggerBuffer;
        public WUIShowInput WUIShow;        

        public PREACTInput(string rootFolder, bool loadingFromDisk)
        {
            RootFolder = rootFolder;
            if(!loadingFromDisk)
            {
                Simulation = new SimulationInput();
                Map = new MapInput();   
                Population = new PopulationInput();
                Events = new EventsInput();
                Evacuation = new EvacuationInput(Simulation);
                Pedestrian = new PedestrianInput();
                Traffic = new TrafficInput();                
                Fire = new FireInput();            
                Smoke = new SmokeInput();
                TriggerBuffer = new TriggerBufferInput();
                WUIShow = new WUIShowInput();
            }
        }

        public static void SaveToDisk(PREACTInput input, string filePath)
        {
            //TODO: fix new format save
            //string json = UnityEngine.JsonUtility.ToJson(WUIEngine.INPUT, true);
            //File.WriteAllText(WUIEngine.WORKING_FILE, json);
            //EvacuationGroup.SaveEvacGroupIndices();
            //GraphicalFireInput.SaveGraphicalFireInput();

            Engine.Message(null, Engine.LogType.Log, " Input file " + filePath + " saved.");       
        }

        public static PREACTInput LoadFromDisk(string filePath, out bool success)
        {
            success = false;
            string rootFolder = Path.GetDirectoryName(filePath);
            PREACTInput result = null;
            if(!File.Exists(filePath))
            {
                Engine.Message(null, Engine.LogType.InputError, " Input file " + filePath + " does not exist.");
            }
            else
            {
                Engine.Message(null, Engine.LogType.Log, " Reading input file " + filePath + ".");
                PREACTInput input = ParseInput(rootFolder, File.ReadAllLines(filePath), out success);
                if (success)
                {      
                    Engine.Message(null, Engine.LogType.Log, " Input file " + filePath + " loaded.");
                }
                else
                {
                    Engine.Message(null, Engine.LogType.Log, " Input file " + filePath + " could not be loaded, see log.");
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
            PREACTInput newInput = new PREACTInput(rootFolder, true);
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
                newInput.Simulation = SimulationInput.Parse(inputLines, lineindex, out success);
            }
            else
            {
                //critical
                Engine.Message(null, Engine.LogType.SimulationError, input + " header not found." + pleaseCheckInput);
                return null;
            }
            if(!success)
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
                Engine.Message(null, Engine.LogType.Warning, input + " header not found, using defaults.");
            }            

            //population            
            if (newInput.Simulation.RunPedestrianModule)
            {
                input = nameof(Population);
                if (headerLineIndex.TryGetValue(input, out lineindex))
                {
                    ReadingInputMessage(input);
                    newInput.Population = PopulationInput.Parse(inputLines, lineindex, newInput.Simulation, rootFolder, out success);
                }
                else
                {
                    //critical
                    Engine.Message(null, Engine.LogType.SimulationError, input + " header not found but user has requested pedestrian module." + pleaseCheckInput);
                    return null;
                }      
            }

            //events
            input = nameof(Events);
            if (headerLineIndex.TryGetValue(input, out lineindex))
            {
                ReadingInputMessage(input);
                newInput.Events = EventsInput.Parse(inputLines, lineindex, rootFolder, out success);
            }
            else
            {
                //does not matter
                newInput.Events = new EventsInput();
                Engine.Message(null, Engine.LogType.Warning, input + " header not found, no events will be added.");
            }

            //evacuation
            if (newInput.Simulation.RunPedestrianModule || newInput.Simulation.RunTrafficModule)
            {
                input = nameof(Evacuation);
                if (headerLineIndex.TryGetValue(input, out lineindex))
                {                    
                    ReadingInputMessage(input);
                    newInput.Evacuation = EvacuationInput.Parse(inputLines, lineindex, newInput.Simulation, newInput.Events, rootFolder, out success);
                }
                else
                {
                    //critical
                    Engine.Message(null, Engine.LogType.SimulationError, input + " header not found but user has requested pedestrian and/or traffic modules." + pleaseCheckInput);
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
                    newInput.Pedestrian = PedestrianInput.Parse(inputLines, lineindex, headerLineIndex, newInput.Simulation, out success);
                }
                else
                {
                    //critical
                    Engine.Message(null, Engine.LogType.SimulationError, input + " header not found but user has requested pedestrian module." + pleaseCheckInput);
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
                    newInput.Traffic = TrafficInput.Parse(inputLines, lineindex, headerLineIndex, newInput.Simulation, rootFolder, out success);
                }
                else
                {
                    Engine.Message(null, Engine.LogType.SimulationError, input + " header not found but user has requested traffic module." + pleaseCheckInput);
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
                    newInput.Fire = FireInput.Parse(inputLines, lineindex, headerLineIndex, newInput.Simulation, rootFolder, out success);
                }
                else
                {
                    //critical                
                    Engine.Message(null, Engine.LogType.SimulationError, input + " header not found but user has requested fire module." + pleaseCheckInput);
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
                    newInput.Smoke = SmokeInput.Parse(inputLines, lineindex, headerLineIndex, newInput.Simulation, rootFolder, out success);
                }
                else
                {
                    //critical
                    Engine.Message(null, Engine.LogType.SimulationError, input + " header not found but user has requested smoke module." + pleaseCheckInput);
                    return null;
                }
            }

            //trigger buffer
            input = nameof(TriggerBuffer);
            if (headerLineIndex.TryGetValue(input, out lineindex))
            {
                ReadingInputMessage(input);
                newInput.TriggerBuffer = TriggerBufferInput.Parse(inputLines, lineindex, headerLineIndex, rootFolder, out success);
            }
            else
            {
                //does not matter, not active per default
                newInput.TriggerBuffer = new TriggerBufferInput();
                Engine.Message(null, Engine.LogType.Warning, input + " header not found, using defaults (disabled).");
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
                Engine.Message(null, Engine.LogType.Warning, input + " header not found, using defaults (disabled).");
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
            Engine.Message(null, Engine.LogType.Log, nameOfInput + " input is being read...");
        }

        public static void InputNotFoundMessage(string nameOfInput, bool critical = false)
        {
            if(critical)
            {
                Engine.Message(null, Engine.LogType.InputError, nameOfInput + " was not found, this value is critical for the simulation to function based on the given input parameters." + pleaseCheckInput);
            }
            else
            {
                Engine.Message(null, Engine.LogType.InputError, nameOfInput + " was not found, default value has been used.");
            }                
        }

        public static void CouldNotInterpretInputMessage(string nameOfInput, string userInput)
        {
            Engine.Message(null, Engine.LogType.InputError, "Could not interpret user input " + userInput + " for " + nameOfInput + ".");
        }

        public static void CheckIfFileExist(string nameOfInput, string inputData, string rootFolder, out bool success)
        {
            success = true;
            string filePath = Path.Combine(rootFolder, inputData);

            if (!File.Exists(filePath))
            {
                success = false;
                nameOfInput += "(" + inputData + ")";
                PREACTInput.InputNotFoundMessage(nameOfInput, true);
            }
        }

        public static void CheckIfFilesExists(string nameOfInput, string[] inputData, string rootFolder, out bool success)
        {
            success = true;
            int issues = 0;

            for (int i = 0; i < inputData.Length; ++i)
            {                
                CheckIfFileExist(nameOfInput, inputData[i], rootFolder, out success);
                issues += success ? 0 : 1;
            }

            if (issues > 0)
            {
                success = false;
            }
        }
    }
}

