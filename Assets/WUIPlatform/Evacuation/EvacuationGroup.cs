//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;

namespace PREACT.Evacuation
{
    public class EvacuationGroup
    {
        public int[] DestinationIndices;
        public double[] DestinationCumulativeWeights;
        public int[] ResponseCurveIndices;
        public string Name;
        public PREACTColor Color;

        public EvacuationGroup(string name, int[] goalIndices, double[] goalsCumulativeWeight, int[] responseCurveIndices, PREACTColor color)
        {
            Name = name;
            DestinationIndices = goalIndices;
            DestinationCumulativeWeights = goalsCumulativeWeight;
            ResponseCurveIndices = responseCurveIndices;
            Color = color;
        }

        public EvacuationDestination GetWeightedRandomDestination(List<EvacuationDestination> destinations)
        {
            float randomChoice = Utility.Math.Randomf.value;
            for (int i = 0; i < DestinationCumulativeWeights.Length; i++)
            {
                if (randomChoice <= DestinationCumulativeWeights[i])
                {
                    return destinations[DestinationIndices[i]];
                }
            }

            //this should not happen, but keep as backup as we do not want to return null
            Engine.MESSAGE(null, Engine.LogType.Warning, "The evacuation destinations specified have cumulative probability under 1.0 and a higher probability was drawn, using last user destination specified as fallback.");
            return destinations[DestinationIndices[DestinationIndices.Length - 1]];
        }

        public static List<EvacuationGroup> LoadEvacGroupFiles(string rootFolder, Runtime.EvacuationData evacuationData, string[] evacuationGroupFiles, out bool success)
        {
            success = false;
            List<EvacuationGroup> evacGroups = new List<EvacuationGroup>();

            for (int i = 0; i < evacuationGroupFiles.Length; i++)
            {
                string path = Path.Combine(rootFolder, evacuationGroupFiles[i] + ".eg");
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

                        eG = new EvacuationGroup(name, goalIndices, goalProbabilities.ToArray(), responseCurveIndices, color);
                        evacGroups.Add(eG);
                    }
                    
                }
                else
                {
                    Engine.MESSAGE(null, Engine.LogType.Warning, "Evacuation group file " + path + " not found and could not be loaded.");
                }

                
                if (fileExists && eG == null)
                {
                    Engine.MESSAGE(null, Engine.LogType.Warning, "Evacuation group file " + path + " was found but did not contain any valid data.");
                }
            }

            if (evacGroups.Count > 0)
            {
                success = true;
                Engine.MESSAGE(null, Engine.LogType.Log, " Evacuation group files loaded, " + evacGroups.Count + " valid evacuation groups were found.");
            }
            else
            {
                Engine.MESSAGE(null, Engine.LogType.Warning, "No valid evacuation group data could be found or loaded, evacuation simulation will not run.");
            }

            return evacGroups;
        }

        public static void SaveEvacGroupIndices(string rootFolder, string filename, int cellsX, int cellsY, int groupCount, int[] EvacGroupIndices)
        {

            string[] data = new string[4];
            //nrows
            data[0] = cellsX.ToString();
            //ncols
            data[1] = cellsY.ToString();
            //how many evac groups
            data[2] = groupCount.ToString();
            //actual data
            data[3] = "";
            for (int i = 0; i < EvacGroupIndices.Length; ++i)
            {
                data[3] += EvacGroupIndices[i] + " ";
            }

            File.WriteAllLines(rootFolder + "/" + filename + ".egs", data);
        }

        public static void LoadEvacGroupIndices(string file, Runtime.EvacuationData evacuationData, out int[] evacGroupIndices, out bool success)
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
                        Engine.MESSAGE(null, Engine.LogType.Log, " Evac groups loaded from file, cells: " + ncols + ", " + nrows);
                        success = true;
                    }
                    else
                    {
                        evacGroupIndices = null;
                        Engine.MESSAGE(null, Engine.LogType.Warning, "Evac groups file does not match current mesh.");
                    }
                }
            }
            catch (System.Exception e)
            {
                evacGroupIndices = null;
                Engine.MESSAGE(null, Engine.LogType.Warning, "Evac groups file " + file + " not found.");
                //WUInity.WUINITY_SIM.LogMessage(e.Message);
            }            
        }
    }
}
