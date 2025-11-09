using PREACT.Utility.Math;
using System.Collections.Generic;
using System.IO;

namespace PREACT.IO
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

        public static List<EvacuationDestinationInput> LoadEvacuationDestinationFiles(string rootFolder, List<string> evacuationGoalFiles, out bool success)
        {
            success = false;
            List<EvacuationDestinationInput> evacDestinations = new List<EvacuationDestinationInput>();

            for (int i = 0; i < evacuationGoalFiles.Count; i++)
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