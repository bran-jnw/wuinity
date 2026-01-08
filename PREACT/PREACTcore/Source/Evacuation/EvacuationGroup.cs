//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using PREACT.Math;
using PREACT.IO;

namespace PREACT.Evacuation
{
    public class EvacuationGroup
    {
        public string Name;
        /*public int[] DestinationIndices;
        public double[] DestinationCumulativeWeights;
        public int[] ResponseCurveIndices;
        public double[] ResponseCurveCumulativeWeights;*/
        public PREACTColor Color;

        public List<string> Destinations;
        public List<double> DestinationProbabilities;
        public List<ResponseCurve> ResponseCurves;
        public List<double> ResponseCurveProbabilities;
        public string ShapeFilePath;
        public bool Default;

        public EvacuationGroup(string name, int[] goalIndices, double[] goalsCumulativeWeight, int[] responseCurveIndices, PREACTColor color, string shapeFilePath, bool defaultDestination = false)
        {
            Name = name;
            DestinationIndices = goalIndices;
            DestinationCumulativeWeights = goalsCumulativeWeight;
            ResponseCurveIndices = responseCurveIndices;
            Color = color;
        }

        public EvacuationGroup()
        {
            Destinations = new List<string>();
            DestinationProbabilities = new List<double>();
            ResponseCurves = new List<ResponseCurve>();
            ResponseCurveProbabilities = new List<double>();
        }

        public EvacuationDestination GetWeightedRandomDestination(List<EvacuationDestination> destinations)
        {
            float randomChoice = Random.valuef;
            for (int i = 0; i < DestinationCumulativeWeights.Length; i++)
            {
                if (randomChoice <= DestinationCumulativeWeights[i])
                {
                    return destinations[DestinationIndices[i]];
                }
            }

            //this should not happen, but keep as backup as we do not want to return null
            Engine.Message(null, Engine.LogType.Warning, "The evacuation destinations specified have cumulative probability under 1.0 and a higher probability was drawn, using last user destination specified as fallback.");
            return destinations[DestinationIndices[DestinationIndices.Length - 1]];
        }

        public EvacuationDestination GetClosestDestination(List<EvacuationDestination> destinations, Vector2d startLatLon, Simulation simulation)
        {
            Vector2d householdPos = simulation.Input.Simulation.Data.GetSimulationPosition(startLatLon);
            int closestIndex = 0;
            double closestDistance = double.MaxValue;
            for (int i = 0; i < DestinationIndices.Length; ++i)
            {
                Vector2d destPos = simulation.Input.Simulation.Data.GetSimulationPosition(destinations[DestinationIndices[i]].LatLon);
                double distance = Vector2d.SqrMagnitude(destPos - householdPos);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = DestinationIndices[i];
                }                
            }
            
            return simulation.Destinations[closestIndex];
        }

        public static Dictionary<string, EvacuationGroup> Parse(string[] inputLines, List<int> evacGroupLineIndices, Dictionary<string, EvacuationDestinationInput> destinationInputs, Dictionary<string, ResponseCurve> responseCurves, string rootFolder, out bool success)
        {
            Dictionary<string, EvacuationGroup> newInputs = new Dictionary<string, EvacuationGroup>();
            success = false;

            for (int i = 0; i < evacGroupLineIndices.Count; ++i)
            {
                EvacuationGroup newInput = new EvacuationGroup();
                success = false;
                int issues = 0;
                Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, evacGroupLineIndices[i]);
                string nameOfInput, userInput;

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
                nameOfInput = nameof(Destinations);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    string[] data = userInput.Split(',');
                    for(int j = 0; j < data.Length; ++i)
                    {
                        if (destinationInputs.ContainsKey(data[j]))
                        {
                            newInput.Destinations.Add(data[j]);
                        }
                        else
                        {
                            success = false;
                            PREACTInput.MissingReferenceToOtherInput(nameOfInput, data[j]);
                        }
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

                //maybe critical
                if(newInput.Destinations.Count == 1)
                {
                    newInput.DestinationProbabilities.Add(1.0);
                }
                else
                {
                    nameOfInput = nameof(DestinationProbabilities);
                    if (inputToParse.TryGetValue(nameOfInput, out userInput))
                    {
                        string[] data = userInput.Split(',');
                        for (int j = 0; j < data.Length; ++i)
                        {
                            double cumulativeProbability;
                            success = double.TryParse(data[j], out cumulativeProbability);
                            if (success)
                            {
                                newInput.DestinationProbabilities.Add(cumulativeProbability);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        success = false;
                        PREACTInput.InputNotFoundMessage(nameOfInput, true);
                    }
                    if (newInput.Destinations.Count != newInput.DestinationProbabilities.Count)
                    {
                        success = false;
                        PREACTInput.IncorrectInputCount(nameOfInput);
                    }
                    if (!success)
                    {
                        break;
                    }
                }                    

                //critical
                nameOfInput = nameof(ResponseCurves);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    string[] data = userInput.Split(',');
                    for (int j = 0; j < data.Length; ++i)
                    {
                        ResponseCurve rC;
                        if (responseCurves.TryGetValue(data[j], out rC))
                        {
                            newInput.ResponseCurves.Add(rC);
                        }
                        else
                        {
                            success = false;
                            PREACTInput.MissingReferenceToOtherInput(nameOfInput, data[j]);
                        }
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

                //maybe critical
                if(newInput.ResponseCurves.Count == 1)
                {
                    newInput.DestinationProbabilities.Add(1.0);
                }
                else
                {
                    nameOfInput = nameof(ResponseCurveProbabilities);
                    if (inputToParse.TryGetValue(nameOfInput, out userInput))
                    {
                        string[] data = userInput.Split(',');
                        for (int j = 0; j < data.Length; ++i)
                        {
                            double cumulativeProbability;
                            success = double.TryParse(data[j], out cumulativeProbability);
                            if (success)
                            {
                                newInput.DestinationProbabilities.Add(cumulativeProbability);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        success = false;
                        PREACTInput.InputNotFoundMessage(nameOfInput, true);
                    }
                    if (newInput.ResponseCurves.Count != newInput.ResponseCurveProbabilities.Count)
                    {
                        success = false;
                        PREACTInput.IncorrectInputCount(nameOfInput);
                    }
                    if (!success)
                    {
                        break;
                    }
                }                    

                //maybe critical
                nameOfInput = nameof(ShapeFilePath);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    newInput.ShapeFilePath = userInput;
                    PREACTInput.CheckIfFileExist(nameOfInput, userInput, rootFolder, out success);
                }
                else
                {
                    success = false;
                    PREACTInput.InputNotFoundMessage(nameOfInput, true);
                }
                if (!success && evacGroupLineIndices.Count > 1) //if only one then it is not critical
                {
                    break;
                }

                //not critical
                if (newInputs.Count == 0)
                {
                    newInput.Default = true;
                }
                else
                {
                    nameOfInput = nameof(Default);
                    if (inputToParse.TryGetValue(nameOfInput, out userInput))
                    {
                        bool.TryParse(userInput, out newInput.Default);
                    }
                    else
                    {
                        success = false;
                        PREACTInput.InputNotFoundMessage(nameOfInput);
                    }
                    if (!success)
                    {
                        newInput.Default = false;
                    }
                    else if (newInput.Default)
                    {
                        foreach (KeyValuePair<string, EvacuationGroup> prevInput in newInputs)
                        {
                            EvacuationGroup eG = prevInput.Value;
                            eG.Default = false;
                        }
                    }
                }

                //not critical
                nameOfInput = nameof(Color);
                if (inputToParse.TryGetValue(nameOfInput, out userInput))
                {
                    string[] data = userInput.Split(',');
                    if (data.Length == 3)
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
                if (!success)
                {
                    newInput.Color = PREACTColor.Random();
                }
            }

            return newInputs;
        }

        /*public static List<EvacuationGroup> LoadEvacGroupFiles(string rootFolder, IO.EvacuationData evacuationData, List<string> evacuationGroupFiles, out bool success)
        {
            success = false;
            List<EvacuationGroup> evacGroups = new List<EvacuationGroup>();

            for (int i = 0; i < evacuationGroupFiles.Count; i++)
            {
                string path = Path.Combine(rootFolder, evacuationGroupFiles[i]);
                bool fileExists = File.Exists(path);
                EvacuationGroup eG = null;
                if (fileExists)
                {
                    string[] dataLines = File.ReadAllLines(path);
                    //skip first line (header)
                    if(dataLines.Length >= 6)
                    {
                        string name;
                        List<string> responseCurveNames = new List<string>(), destinationNames = new List<string>();
                        List<float> responseCurveProbabilities = new List<float>();
                        List<double> goalProbabilities = new List<double>();
                        float r, g, b;
                        PREACTColor color = PREACTColor.white;

                        //get name
                        string[] data = dataLines[0].Split(':');
                        data[1].Trim('"');
                        name = data[1].Trim(' ');

                        //response curve names
                        data = dataLines[1].Split(':');
                        data = data[1].Split(',');
                        for (int j = 0; j < data.Length; j++)
                        {
                            string value = data[j].Trim();
                            value = value.Trim('"');
                            responseCurveNames.Add(value);
                        }

                        //response curve probabilities
                        data = dataLines[2].Split(':');
                        data = data[1].Split(',');
                        for (int j = 0; j < data.Length; j++)
                        {
                            float value;
                            bool b1 = float.TryParse(data[j], out value);
                            if(b1)
                            {
                                responseCurveProbabilities.Add(value);
                            }
                        }

                        //destination names
                        data = dataLines[3].Split(':');
                        data = data[1].Split(',');
                        for (int j = 0; j < data.Length; j++)
                        {
                            string value = data[j].Trim();
                            value = value.Trim('"');
                            destinationNames.Add(value);
                        }

                        //destination probabilities
                        data = dataLines[4].Split(':');
                        data = data[1].Split(',');
                        for (int j = 0; j < data.Length; j++)
                        {
                            float value;
                            bool b1 = float.TryParse(data[j], out value);
                            if (b1)
                            {
                                goalProbabilities.Add(value);
                            }
                        }

                        //color
                        data = dataLines[5].Split(':');
                        data = data[1].Split(',');
                        if(data.Length >= 3)
                        {
                            float.TryParse(data[0], out r);
                            float.TryParse(data[1], out g);
                            float.TryParse(data[2], out b);
                            color = new PREACTColor(r, g, b);
                        }

                        int[] goalIndices = new int[destinationNames.Count];
                        for (int j = 0; j < destinationNames.Count; j++)
                        {
                            goalIndices[j] = evacuationData.GetEvacGoalIndexFromName(destinationNames[j]);
                        }

                        int[] responseCurveIndices = new int[responseCurveNames.Count];
                        for (int j = 0; j < responseCurveNames.Count; j++)
                        {
                            responseCurveIndices[j] =  evacuationData.GetResponseCurveIndexFromName(responseCurveNames[j]);
                        }

                        //TODO: check if input count and probabilities match

                        eG = new EvacuationGroup(name, goalIndices, goalProbabilities.ToArray(), responseCurveIndices, color, string.Empty);
                        evacGroups.Add(eG);
                    }
                    
                }
                else
                {
                    Engine.Message(null, Engine.LogType.Warning, "Evacuation group file " + path + " not found and could not be loaded.");
                }

                
                if (fileExists && eG == null)
                {
                    Engine.Message(null, Engine.LogType.Warning, "Evacuation group file " + path + " was found but did not contain any valid data.");
                }
            }

            if (evacGroups.Count > 0)
            {
                success = true;
                Engine.Message(null, Engine.LogType.Log, " Evacuation group files loaded, " + evacGroups.Count + " valid evacuation groups were found.");
            }
            else
            {
                Engine.Message(null, Engine.LogType.Warning, "No valid evacuation group data could be found or loaded, evacuation simulation will not run.");
            }

            return evacGroups;
        }*/

        public static void SaveEvacGroupIndices(string filePath, Vector2int cells, int groupCount, int[] EvacGroupIndices)
        {

            string[] data = new string[4];
            //nrows
            data[0] = cells.x.ToString();
            //ncols
            data[1] = cells.y.ToString();
            //how many evac groups
            data[2] = groupCount.ToString();
            //actual data
            data[3] = "";
            for (int i = 0; i < EvacGroupIndices.Length; ++i)
            {
                data[3] += EvacGroupIndices[i] + " ";
            }

            File.WriteAllLines(filePath, data);
        }

        public static void LoadEvacGroupIndices(string file, IO.EvacuationData evacuationData, out int[] evacGroupIndices, out bool success)
        {
            success = false;
            try
            {
                using (StreamReader sr = new StreamReader(file))
                {
                    string[] header = new string[4];
                    for (int i = 0; i < 4; ++i)
                    {
                        header[i] = sr.ReadLine();
                    }

                    int ncols, nrows, evacGroupCount;
                    int.TryParse(header[0], out ncols);
                    int.TryParse(header[1], out nrows);
                    int.TryParse(header[2], out evacGroupCount);

                    //make sure we have the correct size
                    if (ncols == evacuationData.CellCount.x && nrows == evacuationData.CellCount.y && evacGroupCount <= evacuationData.EvacuationGroups.Count)
                    {
                        string[] data = header[3].Split(' ');
                        int[] eGsIndices = new int[ncols * nrows];
                        for (int i = 0; i < eGsIndices.Length; ++i)
                        {
                            int.TryParse(data[i], out eGsIndices[i]);
                        }
                        evacGroupIndices = eGsIndices;
                        Engine.Message(null, Engine.LogType.Log, " Evac groups loaded from file, cells: " + ncols + ", " + nrows);
                        success = true;
                    }
                    else
                    {
                        evacGroupIndices = null;
                        Engine.Message(null, Engine.LogType.Warning, "Evac groups file does not match current mesh.");
                    }
                }
            }
            catch (System.Exception e)
            {
                evacGroupIndices = null;
                Engine.Message(null, Engine.LogType.Warning, "Evac groups file " + file + " not found.");
                //WUInity.WUINITY_SIM.LogMessage(e.Message);
            }            
        }
    }
}
