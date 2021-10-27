using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace WUInity
{
    [System.Serializable]
    public class EvacGroup
    {
        public int[] goalIndices;
        public double[] goalsCumulativeWeights;
        public int responseCurveIndex;
        public string name;
        public Color color;

        public static EvacGroup[] GetDefault()
        {
            EvacGroup[] evacGroups = new EvacGroup[3]; 

            EvacGroup eG = new EvacGroup();
            eG.goalIndices = new int[] { 0, 1, 2};
            eG.goalsCumulativeWeights = new double[3] { 0.4, 0.7, 1.0 };
            eG.name = "Group1";
            eG.color = Color.magenta;
            eG.responseCurveIndex = 0;
            evacGroups[0] = eG;

            eG = new EvacGroup();
            eG.goalIndices = new int[] { 0, 1, 2 };
            eG.goalsCumulativeWeights = new double[3] {0.4, 0.7, 1.0};
            eG.name = "Group2";
            eG.color = Color.cyan;
            eG.responseCurveIndex = 0;
            evacGroups[1] = eG;

            eG = new EvacGroup();
            eG.goalIndices = new int[] { 0, 1, 2 };
            eG.goalsCumulativeWeights = new double[3] { 0.4, 0.7, 1.0 };
            eG.name = "Group3";
            eG.color = Color.yellow;
            eG.responseCurveIndex = 0;
            evacGroups[2] = eG;            

            return evacGroups;
        }

        public EvacuationGoal GetWeightedEvacGoal()
        {
            float randomChoice = Random.value;
            for (int i = 0; i < goalsCumulativeWeights.Length; i++)
            {
                if (randomChoice < goalsCumulativeWeights[i])
                {
                    return WUInity.WUINITY_IN.traffic.evacuationGoals[goalIndices[i]];
                }
            }

            return null;
        }

        public static void SaveEvacGroupIndices()
        {
            string filename = WUInity.WUINITY_IN.simName;

            string[] data = new string[4];
            //nrows
            data[0] = WUInity.WUINITY_IN.evac.routeCellCount.x.ToString();
            //ncols
            data[1] = WUInity.WUINITY_IN.evac.routeCellCount.y.ToString();
            //how many evac groups
            data[2] = WUInity.WUINITY_IN.evac.evacGroups.Length.ToString();
            //actual data
            data[3] = "";
            for (int i = 0; i < WUInity.WUINITY_IN.evac.evacGroupIndices.Length; ++i)
            {
                data[3] += WUInity.WUINITY_IN.evac.evacGroupIndices[i] + " ";
            }

            System.IO.File.WriteAllLines(Application.dataPath + "/Resources/_input/" + filename + ".egs", data);
        }

        public static void LoadEvacGroupIndices()
        {
            //cell count might be dirty
            WUInity.WUINITY_SIM.UpdateNeededData();

            string filename = WUInity.WUINITY_IN.simName;
            string path = Application.dataPath + "/Resources/_input/" + filename + ".egs";

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
                    if (ncols == WUInity.WUINITY_IN.evac.routeCellCount.x && nrows == WUInity.WUINITY_IN.evac.routeCellCount.y && evacGroupCount <= WUInity.WUINITY_IN.evac.evacGroups.Length)
                    {
                        string[] data = header[3].Split(' ');
                        int[] eGsIndices = new int[ncols * nrows];
                        for (int i = 0; i < eGsIndices.Length; ++i)
                        {
                            int.TryParse(data[i], out eGsIndices[i]);
                        }
                        WUInity.WUINITY_SIM.UpdateEvacGroups(eGsIndices);
                    }
                    else
                    {
                        WUInity.WUINITY_SIM.UpdateEvacGroups(null);
                        WUInity.WUINITY_SIM.LogMessage("WARNING: Evac groups file does not match current mesh, using default.");
                    }
                }
            }
            catch (System.Exception e)
            {
                WUInity.WUINITY_SIM.UpdateEvacGroups(null);
                WUInity.WUINITY_SIM.LogMessage("WARNING: Evac groups file " + path + " not found, using default.");
                //WUInity.WUINITY_SIM.LogMessage(e.Message);
            }
            
        }
    }
}
