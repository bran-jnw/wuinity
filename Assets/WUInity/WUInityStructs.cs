using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity
{
    [System.Serializable] 
    public enum RoutePriority { Fastest, Closest, Forced }    

    [System.Serializable]
    public class RouteData
    {
        //public string name;
        public Itinero.Route route;
        public EvacuationGoal evacGoal;

        public RouteData(Itinero.Route route, EvacuationGoal evacGoal)
        {
            this.route = route;
            this.evacGoal = evacGoal;
        }
    }

    [System.Serializable] public enum EvacGoalType { Exit, Refugee }
        
    [System.Serializable]
    public class TrafficCellData
    {
        public int carCount;
        public int peopleCount;
    }

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

    /// <summary>
    /// Response curve used for when people will start evacuating.
    /// Time relative to evacuation order being announced.
    /// </summary>
    [System.Serializable]
    public class ResponseData
    {
        public float probability;
        public Vector2 timeMinMax;        

        public ResponseData(float probability, Vector2 timeMinMax)
        {
            this.probability = probability;
            this.timeMinMax = timeMinMax;
        }

        /*public static ResponseData[] GetStandardCurve()
        {
            ResponseData[] r = new ResponseData[3];
            r[0] = new ResponseData(0.14f, new Vector2(-420f, 0f));
            r[1] = new ResponseData(0.81f, new Vector2(0f, 1200f));
            r[2] = new ResponseData(0.95f, new Vector2(1200f, 3600f));

            return r;
        }

        public static ResponseData[] GetRoxburoughCurve()
        {
            ResponseData[] r = new ResponseData[4];
            r[0] = new ResponseData(0.69f, new Vector2(0f, 1200f));
            r[1] = new ResponseData(0.76f, new Vector2(1200f, 1860f));
            r[2] = new ResponseData(0.85f, new Vector2(1860f, 1980f));
            r[3] = new ResponseData(1.0f, new Vector2(1980f, 6300f));

            return r;
        }

        public static void CorrectForEvacOrderTime(ResponseData[] responseData, float evacOrderTime)
        {
            for (int i = 0; i  < responseData.Length; i ++)
            {
                responseData[i].timeMinMax.x += evacOrderTime;
                responseData[i].timeMinMax.y += evacOrderTime;
            }
        }*/
    }

    [System.Serializable]
    public class ResponseCurve
    {
        public ResponseData[] dataPoints;
        bool hasBeenCorrected = false;

        public ResponseCurve(ResponseData[] dataPoints)
        {
            this.dataPoints = dataPoints;
            hasBeenCorrected = false;
        }

        public static ResponseCurve[] GetStandardCurve()
        {
            ResponseData[] r = new ResponseData[3];
            r[0] = new ResponseData(0.14f, new Vector2(-420f, 0f));
            r[1] = new ResponseData(0.81f, new Vector2(0f, 1200f));
            r[2] = new ResponseData(0.95f, new Vector2(1200f, 3600f));


            ResponseCurve rC = new ResponseCurve(r);

            ResponseCurve[] rCs = new ResponseCurve[1];
            rCs[0] = rC;

            return rCs;
        }

        public static ResponseCurve[] GetRoxburoughCurve()
        {
            ResponseData[] r = new ResponseData[4];
            r[0] = new ResponseData(0.69f, new Vector2(0f, 1200f));
            r[1] = new ResponseData(0.76f, new Vector2(1200f, 1860f));
            r[2] = new ResponseData(0.85f, new Vector2(1860f, 1980f));
            r[3] = new ResponseData(1.0f, new Vector2(1980f, 6300f));

            ResponseCurve rC = new ResponseCurve(r);

            ResponseCurve[] rCs = new ResponseCurve[1];
            rCs[0] = rC;

            return rCs;
        }

        public static void CorrectForEvacOrderTime(ResponseCurve responseCurve, float evacOrderTime)
        {
            if(!responseCurve.hasBeenCorrected)
            {
                responseCurve.hasBeenCorrected = true;
                for (int i = 0; i < responseCurve.dataPoints.Length; i++)
                {
                    responseCurve.dataPoints[i].timeMinMax.x += evacOrderTime;
                    responseCurve.dataPoints[i].timeMinMax.y += evacOrderTime;
                }
            }
            
        }
    }

    [System.Serializable]
    public class RoadTypes
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

        public RoadTypes()
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
    }
}

