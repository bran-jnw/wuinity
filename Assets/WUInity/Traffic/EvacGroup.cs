using System.Collections.Generic;
using System.IO;

namespace WUInity
{
    [System.Serializable]
    public class EvacGroup
    {
        public int[] GoalIndices;
        public double[] GoalsCumulativeWeights;
        public int[] ResponseCurveIndices;
        public string Name;
        public WUInityColor Color;

        public EvacGroup(string name, int[] goalIndices, double[] goalsCumulativeWeight, int[] responseCurveIndices, WUInityColor color)
        {
            Name = name;
            GoalIndices = goalIndices;
            GoalsCumulativeWeights = goalsCumulativeWeight;
            ResponseCurveIndices = responseCurveIndices;
            Color = color;
        }

        public static EvacGroup[] GetDefault()
        {
            EvacGroup[] evacGroups = new EvacGroup[3]; 

            int[] goalIndices = new int[] { 0, 1, 2};
            double[] goalsCumulativeWeights = new double[3] { 0.4, 0.7, 1.0 };
            string name = "Group1";
            WUInityColor color = WUInityColor.magenta;
            int[] responseCurveIndices = new int[] {0};
            evacGroups[0] = new EvacGroup(name, goalIndices, goalsCumulativeWeights, responseCurveIndices, color);

            goalIndices = new int[] { 0, 1, 2 };
            goalsCumulativeWeights = new double[3] {0.4, 0.7, 1.0};
            name = "Group2";
            color = WUInityColor.cyan;
            evacGroups[1] = new EvacGroup(name, goalIndices, goalsCumulativeWeights, responseCurveIndices, color);

            goalIndices = new int[] { 0, 1, 2 };
            goalsCumulativeWeights = new double[] { 0.4, 0.7, 1.0 };
            name = "Group3";
            color = WUInityColor.yellow;
            evacGroups[2] = new EvacGroup(name, goalIndices, goalsCumulativeWeights, responseCurveIndices, color);

            return evacGroups;
        }

        public EvacuationGoal GetWeightedEvacGoal()
        {
            float randomChoice = Random.value;
            for (int i = 0; i < GoalsCumulativeWeights.Length; i++)
            {
                if (randomChoice <= GoalsCumulativeWeights[i])
                {
                    return WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals[GoalIndices[i]];
                }
            }

            return null;
        }

        public static EvacGroup[] LoadEvacGroupFiles(out bool success)
        {
            success = false;
            EvacGroup[] result = null;
            List<EvacGroup> evacGroups = new List<EvacGroup>();

            for (int i = 0; i < WUInity.INPUT.Evacuation.EvacGroupFiles.Length; i++)
            {
                string path = Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.Evacuation.EvacGroupFiles[i] + ".eg");
                bool fileExists = File.Exists(path);
                EvacGroup eG = null;
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
                        WUInityColor color = WUInityColor.white;

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
                            color = new WUInityColor(r, g, b);
                        }

                        int[] goalIndices = new int[destinationNames.Count];
                        for (int j = 0; j < destinationNames.Count; j++)
                        {
                            goalIndices[j] = WUInity.RUNTIME_DATA.Evacuation.GetEvacGoalIndexFromName(destinationNames[j]);
                        }

                        int[] responseCurveIndices = new int[responseCurveNames.Count];
                        for (int j = 0; j < responseCurveNames.Count; j++)
                        {
                            responseCurveIndices[j] =  WUInity.RUNTIME_DATA.Evacuation.GetResponseCurveIndexFromName(responseCurveNames[j]);
                        }

                        //TODO: check if input count and probabilities match

                        eG = new EvacGroup(name, goalIndices, goalProbabilities.ToArray(), responseCurveIndices, color);
                        evacGroups.Add(eG);
                    }
                    
                }
                else
                {
                    WUInity.LOG(WUInity.LogType.Warning, "Evacuation group file " + path + " not found and could not be loaded.");
                }

                
                if (fileExists && eG == null)
                {
                    WUInity.LOG(WUInity.LogType.Warning, "Evacuation group file " + path + " was found but did not contain any valid data.");
                }
            }

            if (evacGroups.Count > 0)
            {
                result = evacGroups.ToArray();
                success = true;
                WUInity.LOG(WUInity.LogType.Log, " Evacuation group files loaded, " + evacGroups.Count + " valid evacuation groups were found.");
            }
            else
            {
                WUInity.LOG(WUInity.LogType.Warning, "No valid evacuation group data could be found or loaded, evacuation simulation will not run.");
            }

            return result;
        }

        public static void SaveEvacGroupIndices()
        {
            string filename = WUInity.INPUT.Simulation.SimulationID;

            string[] data = new string[4];
            //nrows
            data[0] = WUInity.RUNTIME_DATA.Evacuation.CellCount.x.ToString();
            //ncols
            data[1] = WUInity.RUNTIME_DATA.Evacuation.CellCount.y.ToString();
            //how many evac groups
            data[2] = WUInity.INPUT.Evacuation.EvacGroupFiles.Length.ToString();
            //actual data
            data[3] = "";
            for (int i = 0; i < WUInity.RUNTIME_DATA.Evacuation.EvacGroupIndices.Length; ++i)
            {
                data[3] += WUInity.RUNTIME_DATA.Evacuation.EvacGroupIndices[i] + " ";
            }

            File.WriteAllLines(WUInity.WORKING_FOLDER + "/" + filename + ".egs", data);
        }

        public static void LoadEvacGroupIndices(out bool success)
        {
            success = false;
            string filename = WUInity.INPUT.Simulation.SimulationID;
            string path = WUInity.WORKING_FOLDER + "/" + filename + ".egs";

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
                    if (ncols == WUInity.RUNTIME_DATA.Evacuation.CellCount.x && nrows == WUInity.RUNTIME_DATA.Evacuation.CellCount.y && evacGroupCount <= WUInity.INPUT.Evacuation.EvacGroupFiles.Length)
                    {
                        string[] data = header[3].Split(' ');
                        int[] eGsIndices = new int[ncols * nrows];
                        for (int i = 0; i < eGsIndices.Length; ++i)
                        {
                            int.TryParse(data[i], out eGsIndices[i]);
                        }
                        WUInity.RUNTIME_DATA.Evacuation.UpdateEvacGroupIndices(eGsIndices);
                        WUInity.LOG(WUInity.LogType.Log, " Evac groups loaded from file, cells: " + ncols + ", " + nrows);
                        success = true;
                    }
                    else
                    {
                        WUInity.RUNTIME_DATA.Evacuation.UpdateEvacGroupIndices(null);
                        WUInity.LOG(WUInity.LogType.Warning, "Evac groups file does not match current mesh, using default.");
                    }
                }
            }
            catch (System.Exception e)
            {
                WUInity.RUNTIME_DATA.Evacuation.UpdateEvacGroupIndices(null);
                WUInity.LOG(WUInity.LogType.Warning, "Evac groups file " + path + " not found, using default.");
                //WUInity.WUINITY_SIM.LogMessage(e.Message);
            }
            
        }
    }
}
