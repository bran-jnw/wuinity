using PREACT.Math;
using System.Collections.Generic;
using System.IO;

namespace PREACT.IO
{
    public struct EvacuationDestinationInput
    {
        public Vector2d LatLon;       
        public string Name;
        public EvacGoalType Type;
        public float MaxFlow; //cars per hour
        public int MaxVehicles;
        public int MaxPeople;
        public bool Blocked;
        public PREACTColor Color;
        

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

        public static Dictionary<string, EvacuationDestinationInput> Parse(string[] inputLines, List<int> destinationLineIndices, string rootFolder, out bool success)
        {
            Dictionary<string, EvacuationDestinationInput> newInputs = new Dictionary<string, EvacuationDestinationInput>();
            success = false;

            for(int i = 0; i < destinationLineIndices.Count; ++i)
            {
                EvacuationDestinationInput newInput = new EvacuationDestinationInput();
                success = false;
                int issues = 0;
                Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, destinationLineIndices[i]);
                string nameOfInput, userInput;

                //critical
                nameOfInput = nameof(LatLon);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    string[] data = userInput.Split(',');
                    issues += double.TryParse(data[0], out newInput.LatLon.x) ? 0 : 1;
                    issues += double.TryParse(data[1], out newInput.LatLon.y) ? 0 : 1;
                    if (issues > 0)
                    {
                        PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                    }
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, true);
                }
                if (!success)
                {
                   break;
                }

                //critical
                nameOfInput = nameof(Name);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    newInput.Name = userInput;
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, true);
                }
                if (!success)
                {
                    break;
                }

                //critical
                nameOfInput = nameof(Type);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    switch (userInput)
                    {
                        case nameof(EvacGoalType.Exit):
                            newInput.Type = EvacGoalType.Exit;
                            break;
                        case nameof(EvacGoalType.Refugee):
                            newInput.Type = EvacGoalType.Refugee;
                            break;
                        default:
                            ++issues;
                            Engine.Message(null, Engine.LogType.SimulationError, nameOfInput + " was not recognized." + PREACTInput.pleaseCheckInput);
                            break;
                    }
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, true);
                }
                if (!success)
                {
                    break;
                }

                

                //not critical
                nameOfInput = nameof(MaxFlow);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    float.TryParse(userInput, out newInput.MaxFlow);
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput);
                }
                if(!success)
                {
                    newInput.MaxFlow = -1f;
                }

                //not critical
                nameOfInput = nameof(MaxVehicles);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    int.TryParse(userInput, out newInput.MaxVehicles);
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput);
                }
                if (!success)
                {
                    newInput.MaxVehicles = -1;
                }

                //not critical
                nameOfInput = nameof(MaxPeople);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    int.TryParse(userInput, out newInput.MaxPeople);
                }
                else
                {                    
                    PREACTInput.InputNotFoundMessage(nameOfInput);
                }
                if(!success)
                {
                    newInput.MaxPeople = -1;
                }

                //not critical
                nameOfInput = nameof(Blocked);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    bool.TryParse(userInput, out newInput.Blocked);
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput);
                }
                if(!success)
                {
                    newInput.Blocked = false;
                }

                //not critical
                nameOfInput = nameof(Color);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    string[] data = userInput.Split(',');
                    if(data.Length == 3)
                    {
                        issues += float.TryParse(data[0], out newInput.Color.r) ? 0 : 1;
                        issues += float.TryParse(data[1], out newInput.Color.g) ? 0 : 1;
                        issues += float.TryParse(data[2], out newInput.Color.b) ? 0 : 1;
                    }
                    else
                    {
                        issues++;
                    }
                    if (issues > 0)
                    {
                        PREACTInput.CouldNotInterpretInputMessage(nameOfInput, userInput);
                    }
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput);
                }
                if(!success)
                {
                    newInput.Color = PREACTColor.Random();
                }                

                newInputs.Add(newInput.Name, newInput);
            }
            
            if(newInputs.Count > 0)
            {
                success = true;
            }            
            return newInputs;
        }

        public static List<EvacuationDestinationInput> LoadEvacuationDestinationFiles(string rootFolder, List<string> evacuationGoalFiles, out bool success)
        {
            success = false;
            List<EvacuationDestinationInput> evacDestinations = new List<EvacuationDestinationInput>();

            for (int i = 0; i < evacuationGoalFiles.Count; i++)
            {
                string path = Path.Combine(rootFolder, evacuationGoalFiles[i]);
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
                    Engine.Message(null, Engine.LogType.Warning, "Evacuation goal data file " + path + " not found and could not be loaded.");
                }
            }

            if (evacDestinations.Count > 0)
            {
                success = true;
                Engine.Message(null, Engine.LogType.Log, " " + evacDestinations.Count + " valid evacuation goal files were succesfully loaded.");
            }

            return evacDestinations;
        }
    }
}