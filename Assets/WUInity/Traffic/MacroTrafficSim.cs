﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity.Traffic
{
    [System.Serializable]
    public class MacroTrafficSim : ITrafficSim
    {        
        public abstract class TrafficEvent
        {
            public float startTime;
            public float endTime;
            public bool isActive;

            public abstract void ApplyEffects(MacroTrafficSim mTS);
            public abstract void RemoveEffects(MacroTrafficSim mTS);
            public abstract void ResetEvent();
        }

        [System.Serializable]
        public class ReverseLanes : TrafficEvent
        {            
            public ReverseLanes(float startTime, float endTime)
            {
                this.startTime = startTime;
                this.endTime = endTime;
                isActive = false;
            }

            public override void ApplyEffects(MacroTrafficSim mTS)
            {
                if(mTS == null)
                {
                    MonoBehaviour.print("Error, no mCS set.");
                    return;
                }

                mTS.reverseLanes = true;
                //mCS.stallBigRoads = true;
                isActive = true;
            }

            public override void RemoveEffects(MacroTrafficSim mTS)
            {
                if (mTS == null)
                {
                    MonoBehaviour.print("Error, no mCS set.");
                    return;
                }

                mTS.reverseLanes = false;
                //mCS.stallBigRoads = false;
                isActive = false;
            }

            public override void ResetEvent()
            {
                isActive = false;
            }

            public static ReverseLanes[] GetDummy()
            {
                ReverseLanes[] rLs = new ReverseLanes[1];
                rLs[0] = new ReverseLanes(float.MaxValue - 1f, float.MaxValue);
                return rLs;
            }
        }

        [System.Serializable]
        public class TrafficAccident : TrafficEvent
        {
            public TrafficAccident(float startTime, float endTime)
            {
                this.startTime = startTime;
                this.endTime = endTime;
                isActive = false;
            }

            public override void ApplyEffects(MacroTrafficSim mTS)
            {
                if (mTS == null)
                {
                    MonoBehaviour.print("Error, no mCS set.");
                    return;
                }

                mTS.stallBigRoads = true;
                isActive = true;
            }

            public override void RemoveEffects(MacroTrafficSim mTS)
            {
                if (mTS == null)
                {
                    MonoBehaviour.print("Error, no mCS set.");
                    return;
                }

                mTS.stallBigRoads = false;
                isActive = false;
            }

            public override void ResetEvent()
            {
                isActive = false;
            }

            public static TrafficAccident[] GetDummy()
            {
                TrafficAccident[] tAs = new TrafficAccident[1];
                tAs[0] = new TrafficAccident(float.MaxValue - 1f, float.MaxValue);
                return tAs;
            }
        }        

        List<MacroCar> carsInSystem;
        List<MacroCar> carsOnHold;
        int totalCarsSimulated;
        int oldTotalCars;
        List<string> output;
        public bool reverseLanes;
        public bool stallBigRoads;
        List<TrafficEvent> trafficEvents;
        RouteCreator routeCreator;
        Dictionary<int, TrafficDensityData> trafficDensityData;
        EvacuationRenderer evacuationRenderer;

        public MacroTrafficSim(RouteCreator rC)
        {
            carsInSystem = new List<MacroCar>();
            carsOnHold = new List<MacroCar>();
            totalCarsSimulated = 0;
            oldTotalCars = 0;

            output = new List<string>();
            
            string start = "Time(s),Injected cars,Exiting cars,Current cars in system, Exiting people, Avg. v [km/h], Min. v [km/h]";
            for (int i = 0; i < WUInity.INPUT.traffic.evacuationGoals.Length; ++i)
            {
                start += ", Goal: " + WUInity.INPUT.traffic.evacuationGoals[i].name;
                start += ", " + WUInity.INPUT.traffic.evacuationGoals[i].name + " flow";
            }
            output.Add(start);
            //string output = "Time(s),Injected cars,Exiting cars,Current cars in system";
            //SaveToFile(output, true);

            reverseLanes = false;
            stallBigRoads = false;
            trafficEvents = new List<TrafficEvent>();

            routeCreator = rC;

            evacuationRenderer = MonoBehaviour.FindObjectOfType<EvacuationRenderer>();
        }

        public void InsertNewCar(RouteData routeData, int numberOfPeopleInCar)
        {
            MacroCar car = new MacroCar(routeData, numberOfPeopleInCar);
            carsOnHold.Add(car);
            ++totalCarsSimulated;            
        }

        public void InsertNewTrafficEvent(TrafficEvent tE)
        {
            trafficEvents.Add(tE);
        }

        public int GetTotalCarsSimulated()
        {
            return totalCarsSimulated;
        }

        public bool EvacComplete()
        {
            return carsInSystem.Count == 0 ? true : false;
        }

        public int GetCarsInSystem()
        {
            return carsInSystem.Count;
        }

        bool evacGoalsDirty = false;
        public void UpdateEvacuationGoals()
        {
            evacGoalsDirty = true;
        }

        private void UpdateEvacGoalsInternal()
        {
            evacGoalsDirty = false;

            //TODO: Do we need to re-calc traffic density data since some cars are gone after update loop?  Conservative not to (and cheaper)
            foreach (KeyValuePair<int, TrafficDensityData> t in trafficDensityData)
            {
                Vector2D startPos = new Vector2D(t.Value.goalCoord.Latitude, t.Value.goalCoord.Longitude);
                RouteData r = routeCreator.CalcTrafficRoute(startPos);
                if (r == null)
                {
                    WUInity.SIM.StopSim("STOP! Null re-route returned to car, should not happen.");
                }
                //special case where start is almost same as end
                else if (r.route.TotalDistance == 0 || r.route.Shape.Length == 1)
                {
                    //UPDATE: This should be perfectly fine since we update AFTER any goals being blocked in time step, so just keep going to goal (which is very close)
                    //WUInity.WUINITY_SIM.LogMessage("Returned route not valid, start same as goal? Ignoring changes.");
                    //check if our startPos is the same as end evac goal, Itinero returns wonky route then
                    //if found, just don't re-calc route
                }
                //everything is as expected
                else
                {
                    for (int i = 0; i < t.Value.cars.Count; i++)
                    {
                        MacroCar car = t.Value.cars[i];
                        //only update if goal is blocked, cars on the same road (density data) might be going different places
                        if(car.routeData.evacGoal.blocked)
                        {
                            if (!car.hasArrived)
                            {
                                car.ChangeRoute(r);
                            }
                        }                                               
                    }
                }
            }
        }

        public bool IsAnyoneGoingHere(EvacuationGoal goal)
        {
            for (int i = 0; i < carsInSystem.Count; i++)
            {
                if(carsInSystem[i].routeData.evacGoal == goal)
                {
                    return true;
                }
            }

            for (int i = 0; i < carsOnHold.Count; i++)
            {
                if (carsOnHold[i].routeData.evacGoal == goal)
                {
                    return true;
                }
            }

            return false;
        }

        public void AdvanceTrafficSimulation(float deltaTime, float currentTime)
        {
            //first resolve traffic events
            for (int i = 0; i < trafficEvents.Count; ++i)
            {
                TrafficEvent t = trafficEvents[i];
                if (currentTime >= t.startTime && currentTime <= t.endTime && !t.isActive)
                {
                    //MonoBehaviour.print("event started at time: " + currentTime);
                    t.ApplyEffects(this);
                }
                if (currentTime >= t.endTime && t.isActive)
                {
                    //MonoBehaviour.print("event endedat time: " + currentTime);
                    t.RemoveEffects(this);
                }
            }

            //for each car in the system we sort them by street they are on, this is called traffic density data
            trafficDensityData = CollectDensity();
            List<MacroCar> carsToRemove = new List<MacroCar>();
            Dictionary<int, TrafficDensityData> newDestinationTDD = new Dictionary<int, TrafficDensityData>();

            float averageSpeed = 0;
            float minSpeed = 9999f;
            int exitingPeople = 0;
            //loop through traffic density data since we know each car on each segment and we need the density data to calculate current speed/density relation
            foreach (KeyValuePair<int, TrafficDensityData> t in trafficDensityData)
            {
                //calculate the new speed based on the local density
                float speed = t.Value.CalculateSpeedBasedOnDensity(); 
                averageSpeed += speed;
                minSpeed = speed < minSpeed ? speed : minSpeed;

                //sort the cars in distance left, shortest distance goes first https://stackoverflow.com/questions/3309188/how-to-sort-a-listt-by-a-property-in-the-object
                t.Value.cars.Sort((x, y) => x.currentDistanceLeft.CompareTo(y.currentDistanceLeft));

                //go through all cars on road segment
                for (int j = 0; j < t.Value.cars.Count; ++j)
                {
                    //our traffic density is flagged as stopped upstreams so no car should move
                    if(t.Value.movementIsBlocked)
                    {
                        break;
                    }

                    MacroCar car = t.Value.cars[j];    
                    //check if we are going on to a new stretch of road (new traffic density) after this time step
                    if(car.WillChangeRoad(deltaTime, speed))
                    {
                        int newHash = car.GetNextHashCode();
                        TrafficDensityData tDD;
                        //if traffic density exists we have to check if we can move over there, if not we stay still and stop the entire movement on the road
                        if(trafficDensityData.TryGetValue(newHash, out tDD))
                        {
                            if(tDD.CanAddCar())
                            {
                                car.MoveCarSpeed(currentTime, deltaTime, speed);
                            }
                            else
                            {
                                //this means that we cannot move upstreams 
                                t.Value.movementIsBlocked = true;
                            }
                        }
                        //we also have to check if we can move to any place that previous cars have already moved to that did not exists prior
                        else if (newDestinationTDD.TryGetValue(newHash, out tDD))
                        {
                            if (tDD.CanAddCar())
                            {
                                car.MoveCarSpeed(currentTime, deltaTime, speed);
                                tDD.AddCar(car);
                            }
                            else
                            {
                                //this means that we cannot move upstreams 
                                t.Value.movementIsBlocked = true;
                            }
                        }
                        else
                        {
                            car.MoveCarSpeed(currentTime, deltaTime, speed);
                            //now we need to add to our temporary dictionary since otherwise we might overfill any new segment
                            int hash = car.densityHash;
                            tDD = new TrafficDensityData(car, this);
                            newDestinationTDD.Add(hash, tDD);
                            tDD.AddCar(car);
                        }
                    }
                    else
                    {
                        car.MoveCarSpeed(currentTime, deltaTime, speed);
                    }                    

                    //flag cars that have arrived
                    if (car.hasArrived)
                    {
                        carsToRemove.Add(car);
                        exitingPeople += car.numberOfPeopleInCar;
                    }  
                }                
            }

            //this fixes cars waiting to get in to the system, but not cars already in the system waiting to get to a new road
            for (int i = 0; i < carsOnHold.Count; i++)
            {
                MacroCar car = carsOnHold[i];
                int hash = car.densityHash;
                TrafficDensityData t;
                if (trafficDensityData.TryGetValue(hash, out t))
                {
                    //we test if we can add it to the desired road, if not it stays in the on hold list
                    if(t.CanAddCar())
                    {
                        //next loop will this car will be used in traffic density fro real, but we add now as we need to stop other form entering if we are physically full
                        t.AddCar(car);
                        carsOnHold.Remove(car);
                        carsInSystem.Add(car);
                    }
                }
                else
                {
                    //new section of road so can add without issues, create new traffic density in case any other car wants to get in on the same road
                    trafficDensityData.Add(hash, new TrafficDensityData(car, this));
                    carsOnHold.Remove(car);
                    carsInSystem.Add(car);
                }
            }
            
            if(trafficDensityData.Count == 0)
            {
                averageSpeed = 0f;
            }
            else
            {
                averageSpeed /= (float)trafficDensityData.Count;
                averageSpeed *= 3.6f;
            }
            if(minSpeed == 9999f)
            {
                minSpeed = 0f;
            }
            else 
            {
                minSpeed *= 3.6f;
            }            

            //saves output time, injected cars at time step, cars who reached destination during time step, cars in system at given time step            
            string newOut = currentTime + "," + (totalCarsSimulated - oldTotalCars) + "," + carsToRemove.Count + "," + carsInSystem.Count + "," + exitingPeople + ", " + averageSpeed + "," + minSpeed;
            for (int i = 0; i < WUInity.INPUT.traffic.evacuationGoals.Length; ++i)
            {
                newOut += "," + WUInity.INPUT.traffic.evacuationGoals[i].currentPeople;
                newOut += "," + WUInity.INPUT.traffic.evacuationGoals[i].currentFlow;
            }

            output.Add(newOut);
            oldTotalCars = totalCarsSimulated;

            //remove cars that has arrived
            for (int i = 0; i < carsToRemove.Count; ++i)
            {
                carsInSystem.Remove(carsToRemove[i]);                
            }

            if(evacGoalsDirty)
            {
                UpdateEvacGoalsInternal();
            }

            WUInity.INSTANCE.SaveTransientDensityData(currentTime, carsInSystem, carsOnHold);

            if(carsInSystem.Count > 0)
            {
                Vector4[] carsRendering = new Vector4[carsInSystem.Count];
                for (int i = 0; i < carsRendering.Length; i++)
                {
                    carsRendering[i] = carsInSystem[i].GetUnityPositionAndSpeed(true);
                }
                evacuationRenderer.UpdateCarsToRender(carsRendering);
            }            
        }

        public void SaveToFile(int runNumber)
        {
            WUInityInput wuiIn = WUInity.INPUT;
            string path = System.IO.Path.Combine(WUInity.OUTPUT_FOLDER, wuiIn.simName + "_traffic_output_" + runNumber + ".csv");
            System.IO.File.WriteAllLines(path, output);
        }

        //add parameter for flow reduction by adding background traffic as a density
        private Dictionary<int, TrafficDensityData> CollectDensity()
        {
            Dictionary<int, TrafficDensityData> tDD = new Dictionary<int, TrafficDensityData>();
            for (int i = 0; i < carsInSystem.Count; ++i)
            {
                MacroCar c = carsInSystem[i];

                int hash = c.densityHash;

                TrafficDensityData t;
                if (tDD.TryGetValue(hash, out t))
                {
                    t.AddCar(c);
                }
                else
                {
                    tDD.Add(hash, new TrafficDensityData(c, this));
                }
            }
            return tDD;
        }

        private class TrafficDensityData
        {
            public Itinero.LocalGeo.Coordinate goalCoord;
            string streetName;
            public int laneCount;
            public float length;
            public List<MacroCar> cars;
            string highwayType;
            MacroTrafficSim mCS;
            public float maxCapacity;
            private int maxCarsOnRoad;
            public bool movementIsBlocked;

            public TrafficDensityData(MacroCar car, MacroTrafficSim mCS)
            {
                goalCoord = car.goingToCoord;
                streetName = car.drivingOnStreet;
                car.GetCurrentMetaData().Attributes.TryGetValue("highway", out highwayType);
                laneCount = GetNumberOfLanes(highwayType);
                maxCapacity = GetMaxCapacity(highwayType);
                length = car.currentShapeLength;
                cars = new List<MacroCar>();
                cars.Add(car);
                this.mCS = mCS;

                maxCarsOnRoad = Mathf.Max(1, (int)(length * 0.2f * laneCount)); //each car takes about 5 meters
                movementIsBlocked = false;
            }

            /// <summary>
            /// Checks if street is physically filled
            /// </summary>
            /// <returns></returns>
            public bool CanAddCar()
            {
                bool success = false;
                if (cars.Count < maxCarsOnRoad)
                {
                    success = true;
                }

                return success;
            }

            public void AddCar(MacroCar car)
            {
                cars.Add(car);
            }

            public float CalculateSpeedBasedOnDensity()
            {
                TrafficInput tO = WUInity.INPUT.traffic;
                //reasonable? not for now
                /*if(cars.Count == 1)
                {
                    return Random.Range(0.8f, 0.9f) * SpeedLimit;
                }*/

                float speedLimit = cars[0].currentSpeedLimit;

                float dens = cars.Count / (length * 0.001f * laneCount);
                //added background traffic
                dens += Random.Range(tO.backGroundDensityMinMax.x, tO.backGroundDensityMinMax.y);                

                //we use the same function to check if a road is blocked due to being main road or if they reverse lanes for now
                if (mCS.stallBigRoads && CanReverseLanes(highwayType))
                {
                    dens = maxCapacity; //gives  stall speed
                }
                //reverse traffic in lanes means double the amount of lanes
                else if (mCS.reverseLanes && CanReverseLanes(highwayType))
                {                    
                    dens *= 0.5f;
                }
                float speed = Mathf.Lerp(speedLimit, tO.stallSpeed / 3.6f, dens / maxCapacity);

                float speed_visibilty = speed;
                if (tO.visibilityAffectsSpeed)
                {
                    //added Enrico & Paolo article      
                    float D_L = tO.opticalDensity;
                    //get rid of any strange values of D_L, TODO: fix when checking input
                    D_L = Mathf.Clamp(D_L, 0.0f, 0.2f);
                    float beta = -101.57f * D_L * D_L * D_L + 49.43f * D_L * D_L - 9.2755f * D_L + 1.0f;

                    //these are not needed as we implement the linear equatíon using lerp
                    //float k_j = 100.0f; // number from above, as in max density is 100 cars per km and lane
                    //float k = k_j * (1.0f - speed / (speedLimit * beta));

                    //this is probably all that is needed? since we base speed on speed limit and not direct proportion to density
                    float visibilityLimitedSpeed = beta * speedLimit;
                    float stallSpeed = tO.stallSpeed / 3.6f;
                    if (visibilityLimitedSpeed < stallSpeed)
                    {
                        //TODO: which approach is best ?
                        //stallSpeed = visibilityLimitedSpeed; 
                        visibilityLimitedSpeed = stallSpeed;
                    }
                    speed_visibilty = Mathf.Lerp(visibilityLimitedSpeed, stallSpeed, dens / maxCapacity);
                }

                return Mathf.Min(speed, speed_visibilty);                
            }
        }

        public static float GetMaxCapacity(string highway)
        {
            float capacity = 50.0f;
            RoadData[] r = WUInity.INPUT.traffic.roadTypes.roadData;
            for (int i = 0; i < r.Length; i++)
            {
                if (highway == r[i].name)
                {
                    capacity = r[i].maxCapacity;
                    break;
                }
            }
            return capacity;
        }

        //based on https://github.com/itinero/routing/blob/1764afc75db43a1459789592de175283f642123f/test/Itinero.Test/test-data/profiles/osm/car.lua
        public static float GetSpeedLimit(string highway)
        {
            float speed = RoadTypes.default_value.speedLimit;
            RoadData[] r = WUInity.INPUT.traffic.roadTypes.roadData;
            for (int i = 0; i < r.Length; i++)
            {
                if(highway == r[i].name)
                {
                    speed = r[i].speedLimit;
                    break;
                }
            }
            return speed / 3.6f;
        }

        //https://wiki.openstreetmap.org/wiki/Key:lanes#Assumptions
        public static int GetNumberOfLanes(string highway)
        {
            int lanes = RoadTypes.default_value.lanes;
            RoadData[] r = WUInity.INPUT.traffic.roadTypes.roadData;
            for (int i = 0; i < r.Length; i++)
            {
                if (highway == r[i].name)
                {
                    lanes = r[i].lanes;
                    break;
                }
            }
            return lanes;
        }

        static bool CanReverseLanes(string highway)
        {
            bool canReverseLanes = RoadTypes.default_value.canBeReversed;
            RoadData[] r = WUInity.INPUT.traffic.roadTypes.roadData;
            for (int i = 0; i < r.Length; i++)
            {
                if (highway == r[i].name)
                {
                    canReverseLanes = r[i].canBeReversed;
                    break;
                }
            }
            return canReverseLanes;
        }
    }
}