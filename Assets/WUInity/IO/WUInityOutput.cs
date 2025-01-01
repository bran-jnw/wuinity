using System;
using System.Globalization;
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

        public static void SaveOutput(string filename)
        {
            List<string> log = WUInity.GetLog();
            //string path = System.IO.Path.Combine(WUInity.WORKING_FOLDER, filename + ".wuiout");
            //System.IO.File.WriteAllText(path, log);

            DateTime localDate = DateTime.Now;
            var culture = new CultureInfo("en-GB");
            string logText = "System logs auto saved on: " + localDate.ToString(culture) + ", " + localDate.Kind + "\n\r";
            foreach (string logItem in log) logText += (logItem + "\n");

            string path = System.IO.Path.Combine(WUInity.OUTPUT_FOLDER, filename + ".log");
            System.IO.File.WriteAllText(path, logText);
        }
    }

    [System.Serializable]
    public class EvacOutput
    {        
        public int actualTotalEvacuees;    
        public int stayingPeople;

        [System.NonSerialized] public int[] rawPopulation;

        [System.NonSerialized] public Texture2D relocatedPopTexture;
        [System.NonSerialized] public Texture2D popStuckTexture;
    }
}

