//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using PREACT.Evacuation;

namespace PREACT.Input
{
    [System.Serializable]
    public class PREACTInput
    {
        public string RootFolder;
        public SimulationInput Simulation;
        public MapInput Map;
        public WeatherInput Weather;
        public PopulationInput Population;
        public EventsInput Events;
        public EvacuationInput Evacuation;
        public PedestrianModuleInput PedestrianModule;
        public TrafficModuleInput TrafficModule;    
        public WildfireModuleInput WildfireModule;        
        public SmokeInput SmokeModule;
        public TriggerBufferModuleInput TriggerBufferModule;
        public WUIShowInput WUIShow;        

        public PREACTInput(string rootFolder)
        {
            RootFolder = rootFolder;

            Simulation = new SimulationInput();
            Map = new MapInput();   
            Weather = new WeatherInput();
            Population = new PopulationInput();
            Events = new EventsInput();
            Evacuation = new EvacuationInput();
            PedestrianModule = new PedestrianModuleInput();
            TrafficModule = new TrafficModuleInput();                
            WildfireModule = new WildfireModuleInput();            
            SmokeModule = new SmokeInput();
            TriggerBufferModule = new TriggerBufferModuleInput();
            WUIShow = new WUIShowInput();
        }

        public static void SaveToDisk(PREACTInput input, string saveFilePath)
        {
            //TODO: fix new format save
            //string json = UnityEngine.JsonUtility.ToJson(WUIEngine.INPUT, true);
            //File.WriteAllText(WUIEngine.WORKING_FILE, json);
            //EvacuationGroup.SaveEvacGroupIndices();
            //GraphicalFireInput.SaveGraphicalFireInput();

            Engine.Message(null, Engine.LogType.Log, " Input file " + saveFilePath + " saved.");       
        }

        public static PREACTInput LoadFromDisk(string filePath, out bool success)
        {
            success = false;
            string rootFolder = Path.GetDirectoryName(filePath);
            PREACTInput input = null;
            if(!File.Exists(filePath))
            {
                Engine.Message(null, Engine.LogType.InputError, " Input file " + filePath + " does not exist.");
            }
            else
            {
                Engine.Message(null, Engine.LogType.Log, " Reading input file " + filePath + ".");
                input = ParseInput(rootFolder, File.ReadAllLines(filePath), out success);
                if (success)
                {      
                    Engine.Message(null, Engine.LogType.Log, " Input file " + filePath + " loaded.");
                }
                else
                {
                    Engine.Message(null, Engine.LogType.Log, " Input file " + filePath + " could not be loaded, see log.");
                }
            }

            return input;
        }

        public static readonly char[] inputSplit = { '=', '#' };
        static readonly char[] headerBrackets = new char[] { '[', ']' };
        public const string pleaseCheckInput = " Please check your input file.";  
        
        private static string RemoveSpace(string input)
        {
            List<char> chars = new List<char>();
            for(int i = 0; i < input.Length; ++i)
            {
                if (input[i] != ' ')
                {
                    chars.Add(input[i]);
                }
            }

            return new string(chars.ToArray());
        }

        private static PREACTInput ParseInput(string rootFolder, string[] inputLines, out bool success)
        {
            success = false;
            PREACTInput newInput = new PREACTInput(rootFolder);
            Dictionary<string, int> headerLineIndices = new Dictionary<string, int>();

            List<int> destinationLineIndices = new List<int>();            
            List<int> responseLineIndices = new List<int>();
            List<int> groupLineIndices = new List<int>();
            List<int> demographicsLineIndices = new List<int>();

            //first index all headers
            for (int i = 0; i < inputLines.Length; ++i)
            {
                if (string.IsNullOrWhiteSpace(inputLines[i]))
                {
                    continue;
                }

                //inputLines[i] = inputLines[i].Trim();
                inputLines[i] = RemoveSpace(inputLines[i]);
                string line = inputLines[i];
                if (line.StartsWith("["))
                {      
                    line = line.Trim(headerBrackets);
                    if (line.Equals("Destination"))
                    {
                        destinationLineIndices.Add(i);
                    }                    
                    else if (line.Equals("ResponseCurve"))
                    {
                        responseLineIndices.Add(i);
                    }
                    else if (line.Equals("EvacuationGroup"))
                    {
                        groupLineIndices.Add(i);
                    }
                    else if (line.Equals("Demographics"))
                    {
                        demographicsLineIndices.Add(i);
                    }
                    else
                    {
                        headerLineIndices.Add(line, i);
                    }                       
                }
            }

            //now see if we have what we need
            int lineindex;
            string nameOfInput;

            //simulation
            nameOfInput = nameof(Simulation);
            if (headerLineIndices.TryGetValue(nameOfInput, out lineindex))
            {
                ReadingInputMessage(nameOfInput);                
                newInput.Simulation.Parse(inputLines, lineindex, out success);
            }
            else
            {
                //critical
                Engine.Message(null, Engine.LogType.InputError, nameOfInput + " header not found." + pleaseCheckInput);
                return null;
            }
            if(!success)
            {
                return null;
            }

            //map
            nameOfInput = nameof(Map);
            if (headerLineIndices.TryGetValue(nameOfInput, out lineindex))
            {
                ReadingInputMessage(nameOfInput);
                newInput.Map.Parse(inputLines, lineindex, out success);
            }
            else
            {
                //does not matter
                Engine.Message(null, Engine.LogType.Warning, nameOfInput + " header not found, using defaults.");
            }
            if (!success)
            {
                return null;
            }

            //weather
            nameOfInput = nameof(Weather);
            if (headerLineIndices.TryGetValue(nameOfInput, out lineindex))
            {
                ReadingInputMessage(nameOfInput);
                newInput.Weather.Parse(inputLines, lineindex, rootFolder, out success);
            }
            else
            {
                //might not matter
                Engine.Message(null, Engine.LogType.Warning, nameOfInput + " header not found, no weather will be loaded.");
            }
            if (!success)
            {
                return null;
            }

            //pedestrian module
            nameOfInput = nameof(PedestrianModule);
            if (headerLineIndices.TryGetValue(nameOfInput, out lineindex))
            {
                ReadingInputMessage(nameOfInput);
                newInput.PedestrianModule.Parse(inputLines, lineindex, headerLineIndices, out success);
            }
            else
            {
                Engine.Message(null, Engine.LogType.Log, "No pedestrian module defined.");
            }
            if (!success)
            {
                return null;
            }

            //traffic module
            nameOfInput = nameof(TrafficModule);
            if (headerLineIndices.TryGetValue(nameOfInput, out lineindex))
            {
                ReadingInputMessage(nameOfInput);
                newInput.TrafficModule.Parse(inputLines, lineindex, headerLineIndices, rootFolder, out success);
            }
            else
            {
                Engine.Message(null, Engine.LogType.Log, "No traffic module defined.");
            }
            if (!success)
            {
                return null;
            }

            //wildfire module
            nameOfInput = nameof(WildfireModule);
            if (headerLineIndices.TryGetValue(nameOfInput, out lineindex))
            {
                ReadingInputMessage(nameOfInput);
                newInput.WildfireModule.Parse(inputLines, lineindex, newInput.Simulation, newInput.Weather, headerLineIndices, rootFolder, out success);
            }
            else
            {               
                Engine.Message(null, Engine.LogType.Log, "No wildfire module defined.");
            }
            if (!success)
            {
                return null;
            }

            //smoke module
            nameOfInput = nameof(SmokeModule);
            if (headerLineIndices.TryGetValue(nameOfInput, out lineindex))
            {
                ReadingInputMessage(nameOfInput);
                newInput.SmokeModule.Parse(inputLines, lineindex, headerLineIndices, newInput.Weather, rootFolder, out success);
            }
            else
            {
                Engine.Message(null, Engine.LogType.Log, "No smoke module defined.");
            }
            if (!success)
            {
                return null;
            }

            //trigger buffer
            nameOfInput = nameof(TriggerBufferModule);
            if (headerLineIndices.TryGetValue(nameOfInput, out lineindex))
            {
                ReadingInputMessage(nameOfInput);
                newInput.TriggerBufferModule.Parse(inputLines, lineindex, headerLineIndices, rootFolder, out success);
            }
            else
            {
                //does not matter, not active per default
                newInput.TriggerBufferModule = new TriggerBufferModuleInput();
                Engine.Message(null, Engine.LogType.Warning, nameOfInput + " header not found, using defaults (disabled).");
            }
            if (!success)
            {
                return null;
            }

            //population, must be before evacuation due to dependence on demographics      
            nameOfInput = nameof(Population);
            if (headerLineIndices.TryGetValue(nameOfInput, out lineindex))
            {
                ReadingInputMessage(nameOfInput);
                newInput.Population.Parse(inputLines, lineindex, demographicsLineIndices, newInput.PedestrianModule, rootFolder, out success);
            }
            else if(newInput.PedestrianModule.Enabled)
            {
                //critical
                Engine.Message(null, Engine.LogType.InputError, nameOfInput + " header not found but user has requested pedestrian module." + pleaseCheckInput);
                return null;
            }
            if (!success)
            {
                return null;
            }

            //events
            nameOfInput = nameof(Events);
            if (headerLineIndices.TryGetValue(nameOfInput, out lineindex))
            {
                ReadingInputMessage(nameOfInput);
                newInput.Events.Parse(inputLines, lineindex, rootFolder, out success);
            }
            else
            {
                Engine.Message(null, Engine.LogType.Warning, nameOfInput + " header not found, no events will be added.");
            }
            if (!success)
            {
                return null;
            }

            //evacuation            
            nameOfInput = nameof(Evacuation);
            if (headerLineIndices.TryGetValue(nameOfInput, out lineindex))
            {                    
                ReadingInputMessage(nameOfInput);
                newInput.Evacuation.Parse(inputLines, lineindex, newInput.Simulation, newInput.Events, newInput.Population, newInput.PedestrianModule, newInput.TrafficModule, destinationLineIndices, responseLineIndices, groupLineIndices, rootFolder, out success);
            }
            else if(newInput.PedestrianModule.Enabled || newInput.TrafficModule.Enabled)
            {
                //critical
                success = false;
                Engine.Message(null, Engine.LogType.InputError, nameOfInput + " header not found but user has requested pedestrian and/or traffic modules." + pleaseCheckInput);
            }
            if (!success)
            {
                return null;
            }            

            //WUIShow
            nameOfInput = nameof(WUIShow);
            if (headerLineIndices.TryGetValue(nameOfInput, out lineindex))
            {
                ReadingInputMessage(nameOfInput);
                newInput.WUIShow.Parse(inputLines, lineindex, out success);
            }
            else
            {
                //does not matter
                newInput.WUIShow = new WUIShowInput();
                Engine.Message(null, Engine.LogType.Warning, nameOfInput + " header not found, using defaults (disabled).");
            }
            if (!success)
            {
                return null;
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
        public static Dictionary<string, string> GetHeaderInput(string[] inputLines, int startIndex, bool collectPureDataLines = false)
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

                //this is e.g. ramps that have no Variable=value structure
                if(collectPureDataLines)
                {                    
                    input = line.Split(',');
                    if (input.Length >= 2)
                    {
                        inputToParse.Add("dataLine" + lineIndex, line);
                    }
                }

                ++lineIndex;
            }

            return inputToParse;
        }

        public static void ReadingInputMessage(string nameOfInput)
        {
            Engine.Message(null, Engine.LogType.Log, nameOfInput + " input is being read...");
        }

        public static void InputNotFoundMessage(string nameOfInput, bool critical = false, string defaultValue = "VALUE")
        {
            if(critical)
            {
                Engine.Message(null, Engine.LogType.InputError, nameOfInput + " was not found, this value is critical for the simulation to function based on the given input parameters." + pleaseCheckInput);
            }
            else
            {
                Engine.Message(null, Engine.LogType.Warning, $"{nameOfInput} was not found, default value {defaultValue} has been used.");
            }                
        }

        public static void CriticalDependency(string missingDependency)
        {
            Engine.Message(null, Engine.LogType.InputError, $"Current module requires {missingDependency} to be set." + pleaseCheckInput);
        }

        public static void MissingReferenceToOtherInput(string nameOfInput, string missingReference)
        {
            Engine.Message(null, Engine.LogType.InputError, nameOfInput + " reference another input (" + missingReference + ") that could not be found." + pleaseCheckInput);
        }

        public static void IncorrectInputCount(string nameOfInput)
        {
            Engine.Message(null, Engine.LogType.InputError, nameOfInput + " does not contain the expected number of inputs." + pleaseCheckInput);
        }

        public static void CouldNotInterpretInputMessage(string nameOfInput, string userInput)
        {
            Engine.Message(null, Engine.LogType.InputError, "Could not interpret user input " + userInput + " for " + nameOfInput + ".");
        }

        public static void CheckIfFileExist(string nameOfInput, string inputData, string rootFolder, out bool success, bool critical = true)
        {
            success = true;
            string filePath = Path.Combine(rootFolder, inputData);

            if (!File.Exists(filePath))
            {
                success = false;
                nameOfInput += "(" + inputData + ")";
                PREACTInput.InputNotFoundMessage(nameOfInput, critical);
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

