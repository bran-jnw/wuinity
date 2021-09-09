using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity.Traffic
{
    [System.Serializable]
    public class TrafficInjection
    {
        public int cars;
        public Vector2D latLong;
        public EvacuationGoal desiredGoal;
        public bool pickGoalFromMap = false;
        public Vector2[] timeFlow;

        private RouteCollection routeCollection;
        private float accumulatedFlow;

        public TrafficInjection(int cars, Vector2D startPos, EvacuationGoal desiredGoal, Vector2[] timeFlow)
        {
            this.cars = cars;
            this.latLong = startPos;
            this.desiredGoal = desiredGoal;
            this.timeFlow = timeFlow;
            accumulatedFlow = 0f;
        }

        public void UpdateInjection(float time, float deltaTime, MacroTrafficSim trafficSim)
        {
            int timeFlowIndex = 0;
            for (int i = 1; i < timeFlow.Length; i++)
            {
                timeFlowIndex = i;
                if (time < timeFlow[i].x)
                {                    
                    break;
                }
            }
            
            float fraction = (time - timeFlow[timeFlowIndex - 1].x) / (timeFlow[timeFlowIndex].x - timeFlow[timeFlowIndex - 1].x);
            float injection = Mathf.Lerp(timeFlow[timeFlowIndex - 1].y, timeFlow[timeFlowIndex].y, fraction);  
            injection *= deltaTime;
            accumulatedFlow += injection;
            int newCars = (int)accumulatedFlow;
            accumulatedFlow -= newCars;

            for (int i = 0; i < newCars; i++)
            {
                trafficSim.InsertNewCar(routeCollection.GetSelectedRoute(), 1);
            }            
        }    
        
        public static TrafficInjection[] GetTemplate()
        {
            TrafficInjection[] t = new TrafficInjection[1];
            Vector2[] timeFlow = new Vector2[2];
            t[0] = new TrafficInjection(1, Vector2D.zero, null, timeFlow);

            return t;
        }
    }

    [System.Serializable]
    public struct SimpleTrafficInjection
    {
        public int cars;
        public Vector2D startPos;
        public EvacuationGoal desiredGoal;

        public SimpleTrafficInjection(int cars, Vector2D startPos, EvacuationGoal desiredGoal)
        {
            this.cars = cars;
            this.startPos = startPos;
            this.desiredGoal = desiredGoal;
        }
    }
}


