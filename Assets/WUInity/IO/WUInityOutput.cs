using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity
{
    [System.Serializable]
    public class WUInityOutput
    {
        public float totalEvacTime;
        public EvacOutput evac;

        public WUInityOutput()
        {
            evac = new EvacOutput();
        }
    }

    [System.Serializable]
    public class EvacOutput
    {        
        public int actualTotalEvacuees;
        public Texture2D rawPopTexture;
        public Texture2D relocatedPopTexture;
        public Texture2D popStuckTexture;
        public Texture2D popStayingTexture;       
        public int stayingPeople;
        public int[] rawPopulation;
        public int[] stayingPopulation;
    }
}

