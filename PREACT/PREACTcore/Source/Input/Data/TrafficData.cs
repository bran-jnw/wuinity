//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.IO;
using PREACT.Input;

namespace PREACT.Input
{
    public class TrafficData
    {       
        private Traffic.RoadTypeData _roadTypeData;
        public Traffic.RoadTypeData RoadTypeData { get => _roadTypeData; }

        public TrafficData() 
        { 

        }

        public void LoadAll(TrafficModuleInput trafficInput, string rootFolder, out bool success)
        {
            success = true;

            if (trafficInput.Module == TrafficModuleInput.TrafficModules.MacroTrafficSim)
            {
                LoadRoadTypeData(Path.Combine(rootFolder, trafficInput.MacroTrafficSimInput.RoadTypesFile), out success);
            }
        }

        public void LoadRoadTypeData(string filePath, out bool success)
        {
            //success in this case means that we loaded a file and not defaults
            _roadTypeData = Traffic.RoadTypeData.LoadRoadTypeData(filePath, out success);
        } 
    }
}