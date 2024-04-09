using System.Collections.Generic;

namespace WUIEngine.IO
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
            string[] log = Engine.GetLog();
            string path = System.IO.Path.Combine(Engine.WORKING_FOLDER, filename + ".wuiout");
            //System.IO.File.WriteAllText(path, log);
        }
    }

    [System.Serializable]
    public class EvacOutput
    {        
        public int actualTotalEvacuees;    
        public int stayingPeople;

        [System.NonSerialized] public int[] rawPopulation;

        //[System.NonSerialized] public Texture2D relocatedPopTexture;
        //[System.NonSerialized] public Texture2D popStuckTexture;
    }
}

