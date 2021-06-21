using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity
{
    [System.Serializable]
    public class EvacGroup
    {
        public int[] goalIndices;
        public double[] cumulativeWeights;
        public string name;
        public Color color;

        public static EvacGroup[] GetDefault()
        {
            EvacGroup[] evacGroups = new EvacGroup[3]; 

            EvacGroup eG = new EvacGroup();
            eG.goalIndices = new int[] { 0, 1, 2};
            eG.cumulativeWeights = new double[3] { 0.4, 0.7, 1.0 };
            eG.name = "Group1";
            eG.color = Color.magenta;
            evacGroups[0] = eG;

            eG = new EvacGroup();
            eG.goalIndices = new int[] { 0, 1, 2 };
            eG.cumulativeWeights = new double[3] {0.4, 0.7, 1.0};
            eG.name = "Group2";
            eG.color = Color.cyan;
            evacGroups[1] = eG;

            eG = new EvacGroup();
            eG.goalIndices = new int[] { 0, 1, 2 };
            eG.cumulativeWeights = new double[3] { 0.4, 0.7, 1.0 };
            eG.name = "Group3";
            eG.color = Color.yellow;
            evacGroups[2] = eG;

            return evacGroups;
        }

        public EvacuationGoal GetWeightedEvacGoal()
        {
            float randomChoice = Random.value;
            for (int i = 0; i < cumulativeWeights.Length; i++)
            {
                if (randomChoice < cumulativeWeights[i])
                {
                    return WUInity.WUINITY_IN.traffic.evacuationGoals[goalIndices[i]];
                }
            }

            return null;
        }
    }
}
