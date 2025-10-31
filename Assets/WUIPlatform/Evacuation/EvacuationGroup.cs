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
    [System.Serializable]
    public class EvacuationGroup
    {
        public int[] GoalIndices;
        public double[] GoalsCumulativeWeights;
        public int[] ResponseCurveIndices;
        public string Name;
        public WUIEngineColor Color;

        public EvacuationGroup(string name, int[] goalIndices, double[] goalsCumulativeWeight, int[] responseCurveIndices, WUIEngineColor color)
        {
            Name = name;
            GoalIndices = goalIndices;
            GoalsCumulativeWeights = goalsCumulativeWeight;
            ResponseCurveIndices = responseCurveIndices;
            Color = color;
        }

        public EvacuationDestination GetWeightedEvacGoal()
        {
            float randomChoice = Random.value;
            for (int i = 0; i < GoalsCumulativeWeights.Length; i++)
            {
                if (randomChoice <= GoalsCumulativeWeights[i])
                {
                    return Engine.RuntimeData.Evacuation.Destinations[GoalIndices[i]];
                }
            }

            //this should not happen, but keep as backup as we do not want to return null
            Engine.MESSAGE(null, Engine.LogType.Warning, "The evacuation destinations specified have cumulative probability under 1.0 and a higher probability was drawn, using last user destination specified as fallback.");
            return Engine.RuntimeData.Evacuation.Destinations[GoalIndices[GoalIndices.Length - 1]];
        }

        public static EvacuationGroup[] LoadEvacGroupFiles(out bool success)
        {
            success = false;
            EvacuationGroup[] result = null;
            List<EvacuationGroup> evacGroups = new List<EvacuationGroup>();

            for (int i = 0; i < Engine.Input.Evacuation.EvacuationGroupFiles.Length; i++)
            {
                string path = Path.Combine(Engine.WorkingFolder, Engine.Input.Evacuation.EvacuationGroupFiles[i] + ".eg");
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
                        WUIEngineColor color = WUIEngineColor.white;

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

                        //goal names
                        data = dataLines[3].Split(':');
                        data = data[1].Split(',');
                        for (int j = 0; j < data.Length; j++)
                        {
                            string value = data[j].Trim();
                            value = value.Trim('"');
                            destinationNames.Add(value);
                        }

                        //goal probabilities
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

                        //colors
                        data = dataLines[5].Split(':');
                        data = data[1].Split(',');
                        if(data.Length >= 3)
                        {
                            float.TryParse(data[0], out r);
                            float.TryParse(data[1], out g);
                            float.TryParse(data[2], out b);
                            color = new WUIEngineColor(r, g, b);
                        }

                        int[] goalIndices = new int[destinationNames.Count];
                        for (int j = 0; j < destinationNames.Count; j++)
                        {
                            goalIndices[j] = Engine.RuntimeData.Evacuation.GetEvacGoalIndexFromName(destinationNames[j]);
                        }

                        int[] responseCurveIndices = new int[responseCurveNames.Count];
                        for (int j = 0; j < responseCurveNames.Count; j++)
                        {
                            responseCurveIndices[j] =  Engine.RuntimeData.Evacuation.GetResponseCurveIndexFromName(responseCurveNames[j]);
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
                result = evacGroups.ToArray();
                success = true;
                Engine.MESSAGE(null, Engine.LogType.Log, " Evacuation group files loaded, " + evacGroups.Count + " valid evacuation groups were found.");
            }
            else
            {
                Engine.MESSAGE(null, Engine.LogType.Warning, "No valid evacuation group data could be found or loaded, evacuation simulation will not run.");
            }

            return result;
        }

        public static void SaveEvacGroupIndices()
        {
            string filename = Engine.Input.Simulation.Id;

            string[] data = new string[4];
            //nrows
            data[0] = Engine.RuntimeData.Evacuation.CellCount.x.ToString();
            //ncols
            data[1] = Engine.RuntimeData.Evacuation.CellCount.y.ToString();
            //how many evac groups
            data[2] = Engine.Input.Evacuation.EvacuationGroupFiles.Length.ToString();
            //actual data
            data[3] = "";
            for (int i = 0; i < Engine.RuntimeData.Evacuation.EvacGroupIndices.Length; ++i)
            {
                data[3] += Engine.RuntimeData.Evacuation.EvacGroupIndices[i] + " ";
            }

            File.WriteAllLines(Engine.WorkingFolder + "/" + filename + ".egs", data);
        }

        public static void LoadEvacGroupIndices(string path, out bool success)
        {
            success = false;
            try
            {
                using (StreamReader sr = new StreamReader(path))
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
                    if (ncols == Engine.RuntimeData.Evacuation.CellCount.x && nrows == Engine.RuntimeData.Evacuation.CellCount.y && evacGroupCount <= Engine.Input.Evacuation.EvacuationGroupFiles.Length)
                    {
                        string[] data = header[3].Split(' ');
                        int[] eGsIndices = new int[ncols * nrows];
                        for (int i = 0; i < eGsIndices.Length; ++i)
                        {
                            int.TryParse(data[i], out eGsIndices[i]);
                        }
                        Engine.RuntimeData.Evacuation.UpdateEvacGroupIndices(eGsIndices);
                        Engine.MESSAGE(null, Engine.LogType.Log, " Evac groups loaded from file, cells: " + ncols + ", " + nrows);
                        success = true;
                    }
                    else
                    {
                        Engine.RuntimeData.Evacuation.UpdateEvacGroupIndices(null);
                        Engine.MESSAGE(null, Engine.LogType.Warning, "Evac groups file does not match current mesh, using default.");
                    }
                }
            }
            catch (System.Exception e)
            {
                Engine.RuntimeData.Evacuation.UpdateEvacGroupIndices(null);
                Engine.MESSAGE(null, Engine.LogType.Warning, "Evac groups file " + path + " not found, using default.");
                //WUInity.WUINITY_SIM.LogMessage(e.Message);
            }
            
        }
    }
}
