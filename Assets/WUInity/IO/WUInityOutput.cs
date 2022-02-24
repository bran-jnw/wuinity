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
        public int stayingPeople;

        [System.NonSerialized] public int[] rawPopulation;
        [System.NonSerialized] public int[] stayingPopulation;

        [System.NonSerialized] public Texture2D popStayingTexture;
        [System.NonSerialized] public Texture2D relocatedPopTexture;
        [System.NonSerialized] public Texture2D popStuckTexture;
    }
}

