using System.Collections.Generic;

namespace WUIPlatform.IO
{
    [System.Serializable]
    public class WUIEngineOutput
    {
        public float totalEvacTime;
        public EvacOutput evac;

        public WUIEngineOutput()
        {
            evac = new EvacOutput();
        }

        public static void SaveOutput(string filename)
        {
            string[] log = WUIEngine.GetLog();
            string path = System.IO.Path.Combine(WUIEngine.WORKING_FOLDER, filename + ".wuiout");
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

