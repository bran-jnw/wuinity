//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using PREACT.Utility.Math;
using System.IO;
using PREACT.Evacuation;

namespace PREACT.IO
{
    [System.Serializable]
    public class EvacuationInput
    {        
        public struct EvacuationDestinationInput
        {
            public Vector2d LatLon;
            public EvacGoalType Type;
            public string Name;            
            public PREACTColor Color;
            public float MaxFlow; //cars per hour
            public int MaxVehicles;
            public int MaxPeople;
            public bool Blocked;

            public EvacuationDestinationInput(Vector2d latLon, EvacGoalType type, string name, PREACTColor color, float maxFlow, int maxCars, int maxPeople, bool blocked)
            {
                 LatLon = latLon;
                Type = type;  
                Name = name;
                Color = color;
                MaxFlow = maxFlow;
                MaxVehicles = maxCars;
                MaxPeople = maxPeople;
                Blocked = blocked;
            }

            public static List<EvacuationDestinationInput> LoadEvacuationDestinationFiles(string rootFolder, string[] evacuationGoalFiles, out bool success)
            {
                success = false;
                List<EvacuationDestinationInput> evacDestinations = new List<EvacuationDestinationInput>();

                for (int i = 0; i < evacuationGoalFiles.Length; i++)
                {
                    string path = Path.Combine(rootFolder, evacuationGoalFiles[i] + ".ed");
                    bool fileExists = File.Exists(path);
                    if (fileExists)
                    {
                        string[] dataLines = File.ReadAllLines(path);

                        string name, exitType, blocked;
                        double lat, lon;
                        float maxFlow, r, g, b;
                        int maxCars, maxPeople;
                        bool initiallyBlocked;
                        EvacGoalType destinationType;
                        PREACTColor color = PREACTColor.white;

                        //name
                        string[] data = dataLines[0].Split(':');
                        name = data[1].Trim();
                        name = name.Trim('"');

                        //lat, long
                        data = dataLines[1].Split(':');
                        double.TryParse(data[1], out lat);

                        data = dataLines[2].Split(':');
                        double.TryParse(data[1], out lon);

                        //goal type
                        data = dataLines[3].Split(':');
                        exitType = data[1].Trim();
                        exitType = exitType.Trim('"');
                        if (exitType == "Refugee")
                        {
                            destinationType = EvacGoalType.Refugee;
                        }
                        else
                        {
                            destinationType = EvacGoalType.Exit;
                        }

                        //max flow, default 3600 vehicles/h
                        data = dataLines[4].Split(':');
                        float.TryParse(data[1], out maxFlow);

                        //car capacity, negative means infinite
                        data = dataLines[5].Split(':');
                        int.TryParse(data[1], out maxCars);

                        //max people´, negative means infinite
                        data = dataLines[6].Split(':');
                        int.TryParse(data[1], out maxPeople);

                        //blocked initially?
                        data = dataLines[7].Split(':');
                        blocked = data[1].Trim();
                        blocked = blocked.Trim('"');
                        if (blocked == "false")
                        {
                            initiallyBlocked = false;
                        }
                        else
                        {
                            initiallyBlocked = true;
                        }

                        //color on marker
                        data = dataLines[8].Split(':');
                        data = data[1].Split(',');
                        if (data.Length >= 3)
                        {
                            float.TryParse(data[0], out r);
                            float.TryParse(data[1], out g);
                            float.TryParse(data[2], out b);
                            color = new PREACTColor(r, g, b);
                        }

                        EvacuationDestinationInput eG = new EvacuationDestinationInput(new Vector2d(lat, lon), destinationType, name, color, maxFlow, maxCars, maxPeople, initiallyBlocked);

                        evacDestinations.Add(eG);
                    }
                    else
                    {
                        Engine.MESSAGE(null, Engine.LogType.Warning, "Evacuation goal data file " + path + " not found and could not be loaded.");
                    }
                }

                if (evacDestinations.Count > 0)
                {
                    success = true;
                    Engine.MESSAGE(null, Engine.LogType.Log, " " + evacDestinations.Count + " valid evacuation goal files were succesfully loaded.");
                }

                return evacDestinations;
            }
        }

        public float EvacuationOrderStart = 0.0f;
        private string[] EvacuationGoalFiles;
        public List<EvacuationDestinationInput> EvacuationDestinationInputs;
        private string[] ResponseCurveFiles;
        public List<ResponseCurve> ResponseCurves;
        private string[] EvacuationGroupFiles;
        public List<EvacuationGroup> EvacuationGroups;
        public string EvacuationGroupsMapFile;
        public float PaintCellSize = 200f;
        public bool UseTriggerBufferEvacuation = false;
        public string TriggerBufferFile;

        public static EvacuationInput Parse(string rootFolder, string[] inputLines, int startIndex)
        {
            int issues = 0;
            EvacuationInput newInput = new EvacuationInput();
            Dictionary<string, string> inputToParse = Input.GetHeaderInput(inputLines, startIndex);
            string input, userInput;

            input = nameof(EvacuationOrderStart);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                float.TryParse(userInput, out newInput.EvacuationOrderStart);
            }
            else
            {
                Engine.MESSAGE(null, Engine.LogType.Warning, nameof(EvacuationOrderStart) + " was not found, using default of " + newInput.EvacuationOrderStart + " seconds.");             
            }

            input = nameof(EvacuationGoalFiles);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                string[] data = userInput.Split(',');
                newInput.EvacuationGoalFiles = data;
                bool success;
                newInput.EvacuationDestinationInputs = EvacuationDestinationInput.LoadEvacuationDestinationFiles(rootFolder, data, out success);
                issues += success == true ? 1 : 0;
            }
            else
            {
                ++issues;
                Input.InputNotFoundMessage(input);
            }

            //TODO: fix error handling
            input = nameof(EvacuationGroupFiles);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                string[] data = userInput.Split(',');
                newInput.EvacuationGroupFiles = data;
                bool success;
                newInput.EvacuationGroups = EvacuationGroup.LoadEvacGroupFiles(rootFolder, data, out success);
                issues += success == true ? 1 : 0;
            }
            else
            {
                ++issues;
                Input.InputNotFoundMessage(input);
            }

            input = nameof(EvacuationGroupsMapFile);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                newInput.EvacuationGroupsMapFile = userInput;
            }
            else
            {
            }

            //TODO: fix actual reading
            input = nameof(ResponseCurveFiles);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                string[] data = userInput.Split(',');
                newInput.ResponseCurveFiles = data;
                bool success;
                newInput.ResponseCurves = ResponseCurve.LoadResponseCurves(rootFolder, data, out success);
                issues += success == true ? 1 : 0;
            }
            else
            {
                ++issues;
                Input.InputNotFoundMessage(input);
            }

            input = nameof(PaintCellSize);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                float.TryParse(userInput, out newInput.PaintCellSize);
            }
            else
            {
            }

            input = nameof(UseTriggerBufferEvacuation);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                bool.TryParse(userInput, out newInput.UseTriggerBufferEvacuation);
            }
            else
            {
            }

            input = nameof(TriggerBufferFile);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                newInput.TriggerBufferFile = userInput;
            }
            else
            {
            }

            return newInput;
        }

        
    }
}
