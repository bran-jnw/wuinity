using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WUInity.Traffic;

namespace WUInity
{
    [System.Serializable]
    public class EvacuationGoal
    {
        public string name = "Goal_1";
        public Vector2D latLong;
        public Color color;
        public bool blocked = false;
        public int maxFlow; //cars per second?
        //public GameObject marker;
        public EvacGoalType goalType = EvacGoalType.Refugee;
        public float cumulativeWeight;
        public int maxCars = -1;
        public int maxPeople = -1;
        [System.NonSerialized] public int currentPeople;
        public List<MacroCar> cars = new List<MacroCar>();

        public void CarArrives(MacroCar arrivingCar)
        {
            cars.Add(arrivingCar);
            currentPeople += arrivingCar.numberOfPopleInCar;

            if(goalType == EvacGoalType.Refugee)
            {
                //track cars and respond
                if (maxCars > 0 && cars.Count >= maxCars && !blocked)
                {
                    blocked = true;
                    WUInity.WUINITY_SIM.LogMessage("Evacuation goal " + name + " has reached cars capacity, re-routing");
                    WUInity.WUINITY_SIM.GoalBlocked();
                }
                else if (maxCars > 0 && cars.Count > maxCars)
                {
                    WUInity.WUINITY_SIM.LogMessage("Additional car arrived at " + name + ", arrived during same time step.");
                }

                //tracka and respond people
                if (maxPeople > -1 && currentPeople >= maxPeople && !blocked)
                {
                    blocked = true;
                    WUInity.WUINITY_SIM.LogMessage("Evacuation goal " + name + " has reached people capacity, re-routing");
                    WUInity.WUINITY_SIM.GoalBlocked();
                }
                else if (maxPeople > -1 && currentPeople > maxPeople)
                {
                    WUInity.WUINITY_SIM.LogMessage("Additional people arrived at " + name + ", arrived during same time step.");
                }
            }            
        }

        public void ResetPeopleAndCars()
        {
            currentPeople = 0;
            cars.Clear();
        }

        public static EvacuationGoal[] GetRoxburoughGoals()
        {
            EvacuationGoal[] eGs = new EvacuationGoal[3];

            eGs[0] = new EvacuationGoal();
            eGs[0].name = "Rox_Goal_E";
            eGs[0].latLong = new Vector2D(39.426692, -105.071401);
            eGs[0].color = Color.red;

            eGs[1] = new EvacuationGoal();
            eGs[1].name = "Rox_Goal_R";
            eGs[1].latLong = new Vector2D(39.473858, -105.092137);
            eGs[1].color = Color.green;

            eGs[2] = new EvacuationGoal();
            eGs[2].name = "Rox_Goal_F";
            eGs[2].latLong = new Vector2D(39.466157, -105.082197);
            eGs[2].color = Color.blue;
            return eGs;
        }
    }
}
