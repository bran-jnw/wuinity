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
}

