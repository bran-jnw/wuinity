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
        public float maxFlow = 3600f; //cars per hour
        //public GameObject marker;
        public EvacGoalType goalType = EvacGoalType.Refugee;
        public int maxCars = -1;
        public int maxPeople = -1;
        [System.NonSerialized] public int currentPeople;
        public List<MacroCar> cars = new List<MacroCar>();

        [System.NonSerialized] public float currentFlow = 0f;
        private float firstArrivalTime, currentTimeStep = float.MinValue;
        private int timeStepCars;        

        public EvacuationGoal()
        {
            name = "New goal";
            latLong = Vector2D.zero;
            color = Color.white;
        }

        public EvacuationGoal(string name, Vector2D latLong, Color color)
        {
            this.name = name;
            this.latLong = latLong;
            this.color = color;
            maxFlow = 3600f;
        }

        public EvacuationGoal(string name, Vector2D latLong, Color color, float maxFlow)
        {
            this.name = name;
            this.latLong = latLong;
            this.color = color;
            this.maxFlow = maxFlow;
        }

        /// <summary>
        /// Checks flow and returns true if car arrives at goal safe and sound, returns false if the car have to wait.
        /// </summary>
        /// <param name="arrivingCar"></param>
        /// <param name="timeStep"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public bool CarArrives(MacroCar arrivingCar, float timeStep, float deltaTime)
        {
            UpdateFlow(timeStep, deltaTime);

            //car can arrive
            if((maxFlow <= 0 && !blocked) || (currentFlow < maxFlow && !blocked))
            {         
                //add new cars and people that has arrived during timestep
                ++timeStepCars;
                cars.Add(arrivingCar);
                currentPeople += arrivingCar.numberOfPeopleInCar;
                UpdateCapacity();

                return true;
            }

            return false;
        }

        void UpdateCapacity()
        {
            if (goalType == EvacGoalType.Refugee)
            {
                //track cars and respond
                if (maxCars > 0 && cars.Count >= maxCars && !blocked)
                {
                    blocked = true;
                    WUInity.LogMessage("Evacuation goal " + name + " has reached cars capacity, re-routing");
                    WUInity.SIM.GoalBlocked();
                }
                else if (maxCars > 0 && cars.Count > maxCars)
                {
                    WUInity.LogMessage("Additional car arrived at " + name + ", arrived during same time step.");
                }

                //track and respond people
                if (maxPeople > -1 && currentPeople >= maxPeople && !blocked)
                {
                    blocked = true;
                    WUInity.LogMessage("Evacuation goal " + name + " has reached people capacity, re-routing");
                    WUInity.SIM.GoalBlocked();
                }
                else if (maxPeople > -1 && currentPeople > maxPeople)
                {
                    WUInity.LogMessage("Additional people arrived at " + name + ", arrived during same time step.");
                }
            }
        }

        private void UpdateFlow(float timeStamp, float deltaTime)
        {
            //new timestamp?
            if (currentTimeStep != timeStamp)
            {
                currentTimeStep = timeStamp;
                timeStepCars = 0;
            }

            //calc current flow
            if (cars.Count == 0)
            {
                firstArrivalTime = timeStamp;
                currentFlow = 0f;
            }
            else
            {
                float timestepFlow = timeStepCars / deltaTime;
                if (timeStamp == firstArrivalTime)
                {
                    currentFlow = timestepFlow;
                }
                else
                {
                    currentFlow = cars.Count / (timeStamp - firstArrivalTime);
                }
                currentFlow = Mathf.Max(timestepFlow, currentFlow) * 3600f;
            }
        }

        public void ResetPeopleAndCars()
        {
            blocked = false; 

            currentPeople = 0;
            cars.Clear();

            //reset stuff for flow calc
            currentFlow = 0f;
            timeStepCars = 0;
            firstArrivalTime = float.MinValue;
            currentTimeStep = float.MinValue;
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
