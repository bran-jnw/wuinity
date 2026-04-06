using PREACT.Math;
using System.Collections.Generic;
using System.IO;

namespace PREACT.Input
{
    public class EvacuationDestinationInput
    {
        public string Name = string.Empty;
        public Vector2d LatLon = Vector2d.zero;        
        public DestinationTypes Type = DestinationTypes.Exit;
        public float MaxFlow = -1f; //cars per hour
        public int MaxVehicles = -1;
        public int MaxPeople = -1;
        public bool Blocked = false;
        public PREACTColor Color = PREACTColor.Random();
        
        public EvacuationDestinationInput()
        {

        }

        public EvacuationDestinationInput(string name, Vector2d latLon, DestinationTypes type, PREACTColor color, float maxFlow, int maxCars, int maxPeople, bool blocked)
        {
            Name = name;
            LatLon = latLon;
            Type = type;            
            Color = color;
            MaxFlow = maxFlow;
            MaxVehicles = maxCars;
            MaxPeople = maxPeople;
            Blocked = blocked;
        }

        public static void Parse(Dictionary<string, EvacuationDestinationInput> newInputs, string[] inputLines, List<int> destinationLineIndices, out bool success)
        {
            success = false;
            newInputs.Clear();

            for(int i = 0; i < destinationLineIndices.Count; ++i)
            {
                EvacuationDestinationInput newInput = new EvacuationDestinationInput();
                success = false;
                int issues = 0;
                Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, destinationLineIndices[i]);
                string nameOfInput, userInput;

                //critical
                nameOfInput = nameof(Name);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    newInput.Name = userInput;
                    success = true;
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
                if (!success || issues > 0)
                {
                   break;
                }

                //critical
                nameOfInput = nameof(Type);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    switch (userInput)
                    {
                        case nameof(DestinationTypes.Exit):
                            newInput.Type = DestinationTypes.Exit;
                            break;
                        case nameof(DestinationTypes.Shelter):
                            newInput.Type = DestinationTypes.Shelter;
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
                if (!success || issues > 0)
                {
                    break;
                }                

                //not critical
                nameOfInput = nameof(MaxFlow);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    success = float.TryParse(userInput, out newInput.MaxFlow);
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
                    success = int.TryParse(userInput, out newInput.MaxVehicles);
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
                    success = int.TryParse(userInput, out newInput.MaxPeople);
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
                    success = bool.TryParse(userInput, out newInput.Blocked);
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
                if(!success || issues > 0)
                {
                    newInput.Color = PREACTColor.Random();
                }                

                newInputs.Add(newInput.Name, newInput);
            }
            
            if(newInputs.Count == destinationLineIndices.Count)
            {
                success = true;
            }
            else
            {
                Engine.Message(null, Engine.LogType.InputError, "Could not read all specified EvacuationDestinations.");
            }
        }
    }
}