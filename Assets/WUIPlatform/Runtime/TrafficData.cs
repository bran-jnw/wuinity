//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.IO;
using WUIPlatform.IO;

namespace WUIPlatform.Runtime
{
    public class TrafficData
    {       
        private Traffic.RoadTypeData _roadTypeData;
        public Traffic.RoadTypeData RoadTypeData
        {
            get
            {
                return _roadTypeData;
            }
        }

        public void LoadAll()
        {
            WUIEngine.LOG(WUIEngine.LogType.Log, "Loading Traffic data...");

            if (WUIEngine.INPUT.Traffic.TrafficModule == TrafficInput.TrafficModuleChoice.MacroTrafficSim)
            {
                LoadRoadTypeData(Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Traffic.MacroTrafficSimInput.RoadTypesFile), false);
            }
        }

        private bool LoadRoadTypeData(string path, bool updateInputFile)
        {
            //success in this case means that we loaded a file and not defaults
            bool success;
            _roadTypeData = Traffic.RoadTypeData.LoadRoadTypeData(path, out success);
            if(success && updateInputFile)
            {
                //WUIEngine.INPUT.Traffic.roadTypesFile = Path.GetFileName(path);
                WUIEngineInput.SaveInput();
            }

            return success;
        } 
    }
}