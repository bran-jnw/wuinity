//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

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

