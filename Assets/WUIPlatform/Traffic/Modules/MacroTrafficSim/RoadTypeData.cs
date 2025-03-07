//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.IO;

namespace WUIPlatform.Traffic
{
    [System.Serializable]
    public class RoadData
    {
        public string name = "default";
        public float speedLimit = 10f;
        public int lanes = 1;
        public float maxCapacity;
        public bool canBeReversed = false;

        public RoadData(string name, float speedLimit, int lanes = 1, bool canBeReversed = false, float maxCapacity = 50)
        {
            this.name = name;
            this.speedLimit = speedLimit;
            this.lanes = lanes;
            this.canBeReversed = canBeReversed;
            this.maxCapacity = maxCapacity;
        }
    }

    [System.Serializable]
    public class RoadTypeData
    {
        public static RoadData motorway = new RoadData("motorway", 120f, 2, true, 75f);
        public static RoadData motorway_link = new RoadData("motorway_link", 120f, 2, true, 75f);
        public static RoadData trunk = new RoadData("trunk", 90f, 2, true, 75f);
        public static RoadData trunk_link = new RoadData("trunk_link", 90f, 2, true, 75f);
        public static RoadData primary = new RoadData("primary", 90f, 1, true, 75f);
        public static RoadData primary_link = new RoadData("primary_link", 90f, 1, true, 75f);
        public static RoadData secondary = new RoadData("secondary", 70f, 1, true, 75f);
        public static RoadData secondary_link = new RoadData("secondary_link", 70f, 1, true, 75f);
        public static RoadData tertiary = new RoadData("tertiary", 70f, 1, true, 60f);
        public static RoadData tertiary_link = new RoadData("tertiary_link", 70f, 1, true, 60f);
        public static RoadData unclassified = new RoadData("unclassified", 50f);
        public static RoadData residential = new RoadData("residential", 50f, 1, false, 50f);
        public static RoadData service = new RoadData("service", 30f);
        public static RoadData services = new RoadData("services", 30f);
        public static RoadData road = new RoadData("road", 30f);
        public static RoadData track = new RoadData("track", 30f);
        public static RoadData living_street = new RoadData("living_street", 5f);
        public static RoadData ferry = new RoadData("ferry", 5f);
        public static RoadData movable = new RoadData("movable", 5f);
        public static RoadData shuttle_train = new RoadData("shuttle_train", 10f);
        public static RoadData custom0 = new RoadData("custom0", 40f);
        public static RoadData custom1 = new RoadData("custom1", 40f);
        public static RoadData custom2 = new RoadData("custom2", 40f);
        public static RoadData custom3 = new RoadData("custom3", 40f);
        public static RoadData custom4 = new RoadData("custom4", 40f);
        public static RoadData default_value = new RoadData("default", 10f);

        public RoadData[] roadData;

        public RoadTypeData()
        {
            roadData = new RoadData[26];
            roadData[0] = motorway;
            roadData[1] = motorway_link;
            roadData[2] = trunk;
            roadData[3] = trunk_link;
            roadData[4] = primary;
            roadData[5] = primary_link;
            roadData[6] = secondary;
            roadData[7] = secondary_link;
            roadData[8] = tertiary;
            roadData[9] = tertiary_link;
            roadData[10] = unclassified;
            roadData[11] = residential;
            roadData[12] = service;
            roadData[13] = services;
            roadData[14] = road;
            roadData[15] = track;
            roadData[16] = living_street;
            roadData[17] = ferry;
            roadData[18] = movable;
            roadData[19] = shuttle_train;
            roadData[20] = custom0;
            roadData[21] = custom1;
            roadData[22] = custom2;
            roadData[23] = custom3;
            roadData[24] = custom4;
            roadData[25] = default_value;
        }

        public RoadTypeData(RoadData[] roadData)
        {
            this.roadData = roadData;
        }

        private static void SaveRoadTypeData(RoadTypeData rTD)
        {
            string path = Path.Combine(WUIEngine.DATA_FOLDER, "default.roads");
            string json = UnityEngine.JsonUtility.ToJson(rTD, true);
            File.WriteAllText(path, json);            
        }

        public static RoadTypeData LoadRoadTypeData(string path, out bool loadedDefaults)
        {
            RoadTypeData results = null;
            loadedDefaults = true;
            
            if (File.Exists(path))
            {
                string input = File.ReadAllText(path);
                results = UnityEngine.JsonUtility.FromJson<RoadTypeData>(input);
                loadedDefaults = false;
            }

            if(results == null)
            {
                //creates default values
                results = new RoadTypeData();
                SaveRoadTypeData(results);
                WUIEngine.INPUT.Traffic.MacroTrafficSimInput.RoadTypesFile = "default";               
            }            

            return results;
        }
    }
}
