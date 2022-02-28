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
                    return WUInity.INPUT.traffic.evacuationGoals[goalIndices[i]];
                }
            }

            return null;
        }

        public static void SaveEvacGroupIndices()
        {
            string filename = WUInity.INPUT.simName;

            string[] data = new string[4];
            //nrows
            data[0] = WUInity.SIM_DATA.EvacCellCount.x.ToString();
            //ncols
            data[1] = WUInity.SIM_DATA.EvacCellCount.y.ToString();
            //how many evac groups
            data[2] = WUInity.INPUT.evac.evacGroups.Length.ToString();
            //actual data
            data[3] = "";
            for (int i = 0; i < WUInity.SIM_DATA.evacGroupIndices.Length; ++i)
            {
                data[3] += WUInity.SIM_DATA.evacGroupIndices[i] + " ";
            }

            File.WriteAllLines(WUInity.WORKING_FOLDER + "/" + filename + ".egs", data);
        }

        public static void LoadEvacGroupIndices()
        {
            string filename = WUInity.INPUT.simName;
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
                    if (ncols == WUInity.SIM_DATA.EvacCellCount.x && nrows == WUInity.SIM_DATA.EvacCellCount.y && evacGroupCount <= WUInity.INPUT.evac.evacGroups.Length)
                    {
                        string[] data = header[3].Split(' ');
                        int[] eGsIndices = new int[ncols * nrows];
                        for (int i = 0; i < eGsIndices.Length; ++i)
                        {
                            int.TryParse(data[i], out eGsIndices[i]);
                        }
                        WUInity.SIM_DATA.UpdateEvacGroups(eGsIndices);
                        WUInity.WUI_LOG("LOG: Evac groups loaded from file, cells: " + ncols + ", " + nrows);
                    }
                    else
                    {
                        WUInity.SIM_DATA.UpdateEvacGroups(null);
                        WUInity.WUI_LOG("WARNING: Evac groups file does not match current mesh, using default.");
                    }
                }
            }
            catch (System.Exception e)
            {
                WUInity.SIM_DATA.UpdateEvacGroups(null);
                WUInity.WUI_LOG("WARNING: Evac groups file " + path + " not found, using default.");
                //WUInity.WUINITY_SIM.LogMessage(e.Message);
            }
            
        }
    }
}
