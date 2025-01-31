//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace WUIPlatform.Traffic
{
    [System.Serializable]
    public class TrafficInjection
    {
        public int cars;
        public Vector2d latLong;
        public EvacuationGoal desiredGoal;
        public bool pickGoalFromMap = false;
        public Vector2[] timeFlow;

        private RouteCollection routeCollection;
        private float accumulatedFlow;

        public TrafficInjection(int cars, Vector2d startPos, EvacuationGoal desiredGoal, Vector2[] timeFlow)
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
                if (time < timeFlow[i].X)
                {                    
                    break;
                }
            }
            
            float fraction = (time - timeFlow[timeFlowIndex - 1].X) / (timeFlow[timeFlowIndex].X - timeFlow[timeFlowIndex - 1].X);
            float injection = Mathf.Lerp(timeFlow[timeFlowIndex - 1].Y, timeFlow[timeFlowIndex].Y, fraction);  
            injection *= deltaTime;
            accumulatedFlow += injection;
            int newCars = (int)accumulatedFlow;
            accumulatedFlow -= newCars;

            for (int i = 0; i < newCars; i++)
            {
                RouteData rD = routeCollection.GetSelectedRoute();
                Vector2d startLatLong = new Vector2d(rD.route.Shape[0].Latitude, rD.route.Shape[0].Longitude);
                trafficSim.InsertNewCar(startLatLong, rD.evacGoal, 1);
            }            
        }    
        
        public static TrafficInjection[] GetTemplate()
        {
            TrafficInjection[] t = new TrafficInjection[1];
            Vector2[] timeFlow = new Vector2[2];
            t[0] = new TrafficInjection(1, Vector2d.zero, null, timeFlow);

            return t;
        }
    }

    [System.Serializable]
    public struct SimpleTrafficInjection
    {
        public int cars;
        public Vector2d startPos;
        public EvacuationGoal desiredGoal;

        public SimpleTrafficInjection(int cars, Vector2d startPos, EvacuationGoal desiredGoal)
        {
            this.cars = cars;
            this.startPos = startPos;
            this.desiredGoal = desiredGoal;
        }
    }
}


