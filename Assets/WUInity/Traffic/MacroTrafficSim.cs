using System.Collections;
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
        Dictionary<int, RoadSegment> roadSegments;
        Visualization.EvacuationRenderer evacuationRenderer;
        List<float> arrivalData;

        public MacroTrafficSim(RouteCreator rC)
        {
            carsInSystem = new List<MacroCar>();
            carsOnHold = new List<MacroCar>();
            totalCarsSimulated = 0;
            oldTotalCars = 0;

            output = new List<string>();

#if DEBUG
            string start = "Time(s),#RoadSegs,#BlockedSGs,#BlockdCars,Injected cars,carsOnHold,Exiting cars,Current cars in system,CCIS, Exiting people, Avg. v [km/h], Min. v [km/h]";
#else
            string start = "Time(s),Injected cars,Exiting cars,Current cars in system, Exiting people, Avg. v [km/h], Min. v [km/h]";
#endif

            for (int i = 0; i < WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals.Count; ++i)
            {
                start += ", Goal: " + WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals[i].name;
                start += ", " + WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals[i].name + " flow";
            }
            output.Add(start);
            //string output = "Time(s),Injected cars,Exiting cars,Current cars in system";
            //SaveToFile(output, true);

            reverseLanes = false;
            stallBigRoads = false;
            trafficEvents = new List<TrafficEvent>();

            routeCreator = rC;

            evacuationRenderer = MonoBehaviour.FindObjectOfType<Visualization.EvacuationRenderer>();

            arrivalData = new List<float>();
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

        public List<float> GetArrivalData()
        {
            return arrivalData;
        }

        bool evacGoalsDirty = false;
        public void UpdateEvacuationGoals()
        {
            evacGoalsDirty = true;
        }

        private void UpdateEvacGoalsInternal()
        {
            evacGoalsDirty = false;
            int reRoutedCar = 0;
            List<MacroCar> reroutedCarsOnHold= new List<MacroCar>();

            //TODO: Do we need to re-calc traffic density data since some cars are gone after update loop?  Conservative not to (and cheaper)
            foreach (KeyValuePair<int, RoadSegment> t in roadSegments)
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
                                int currenthash = car.roadSegmentHash;
                                carsInSystem.Remove(car);

                                car.ChangeRoute(r);
                                int newhash = car.roadSegmentHash;

                                if (currenthash!= newhash)
                                {
                                    t.Value.cars.RemoveAt(i);
                                    RoadSegment tt;
                                    if (roadSegments.TryGetValue(newhash, out tt))
                                    {
                                        if (tt.CanAddCar())
                                        {
                                            tt.AddCar(car);
                                            carsInSystem.Add(car);
                                        }
                                        else
                                        {
                                            carsOnHold.Add(car);
                                        }                                     
                                    }
                                    else
                                    {
                                        reroutedCarsOnHold.Add(car);
                                        //roadSegments.Add(newhash, new RoadSegment(car, this));
                                    }
                                }
                                else
                                {
                                    carsInSystem.Add(car);
                                }
                                                                
                                reRoutedCar++;
                            }
                        }                                               
                    }
                }
            }

            for (int i = 0; i < reroutedCarsOnHold.Count; i++)
            {
                MacroCar car = reroutedCarsOnHold[i];
                int hash = car.roadSegmentHash;
                RoadSegment t;
                if (roadSegments.TryGetValue(hash, out t))
                {
                    if (t.CanAddCar())
                    {
                        t.AddCar(car);
                        carsInSystem.Add(car);
                        reroutedCarsOnHold.Remove(car);
                    }
                    else
                    {
                        carsOnHold.Add(car);
                    }
                }
                else
                {
                    //new section of road so can add without issues, create new traffic density in case any other car wants to get in on the same road
                    roadSegments.Add(hash, new RoadSegment(car, this));
                    reroutedCarsOnHold.Remove(car);
                    carsInSystem.Add(car);
                }
            }

            WUInity.LOG(WUInity.LogType.Event, "Rerouted "+ reRoutedCar.ToString() +" cars due to evacuation goal reached full capacity.");
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
                    t.ApplyEffects(this);
                }
                if (currentTime >= t.endTime && t.isActive)
                {
                    t.RemoveEffects(this);
                }
            }

            //for each car in the system we sort them by street they are on, this is called traffic density data
            roadSegments = CollectRoadSegments();
            List<MacroCar> carsToRemove = new List<MacroCar>();
            Dictionary<int, RoadSegment> newRoadSegments = new Dictionary<int, RoadSegment>();

            float averageSpeed = 0;
            float minSpeed = 9999f;
            int exitingPeople = 0;

            int upstreamMovementBlockedSegment = 0;
            int blockedCars=0;
            int minimumMaxCarsOnRoad = 99999;
            int countCarsInSystem = 0;

            //loop through each road segment that has a car and we need the density data to calculate current speed/density relation
            foreach (KeyValuePair<int, RoadSegment> roadSegment in roadSegments)
            {
                if (minimumMaxCarsOnRoad> roadSegment.Value.GetMaxCarsOnRoad())
                {
                    minimumMaxCarsOnRoad = roadSegment.Value.GetMaxCarsOnRoad();
                }
                
                if (roadSegment.Value.cars.Count <= 0)
                    WUInity.LOG(WUInity.LogType.Log, "roadSegment has abnormal number of cars of "+ roadSegment.Value.cars.Count);
                else
                    countCarsInSystem += roadSegment.Value.cars.Count;

#if DEBUG
//                if (roadSegment.Value.cars.Count > roadSegment.Value.GetMaxCarsOnRoad())
//                    WUInity.LOG(WUInity.LogType.Warning, "RoadSegment [" + roadSegment.Value.GetStreetName() + "] has " + roadSegment.Value.cars.Count + " cars, while MaxCar= " + roadSegment.Value.GetMaxCarsOnRoad());
#endif

                //calculate the new speed based on the local density
                float densitySpeed = roadSegment.Value.CalculateSpeedBasedOnDensity(); 
                averageSpeed += densitySpeed;
                minSpeed = densitySpeed < minSpeed ? densitySpeed : minSpeed;

                //sort the cars in distance left, shortest distance goes first https://stackoverflow.com/questions/3309188/how-to-sort-a-listt-by-a-property-in-the-object
                roadSegment.Value.cars.Sort((x, y) => x.currentDistanceLeft.CompareTo(y.currentDistanceLeft));

                //go through all cars on road segment
                for (int j = 0; j < roadSegment.Value.cars.Count; ++j)
                {
                    //our traffic density is flagged as stopped upstreams so no car should move
                    if(roadSegment.Value.upstreamMovementBlocked)
                    {
                        //densitySpeed = WUInity.INPUT.traffic.stallSpeed / 3.6f;
                        //WUInity.LOG(WUInity.LogType.Log, "roadSegment.Value.upstreamMovementBlocked!!");
                        upstreamMovementBlockedSegment++;
                        blockedCars += roadSegment.Value.cars.Count;
                        break;
                    }

                    MacroCar car = roadSegment.Value.cars[j];
                    float speed = densitySpeed;
                    /*if(j == 0)
                    {
                        speed = car.currentSpeedLimit;
                    }*/
                    
                    //check if we are going on to a new stretch of road (new traffic density) after this time step
                    if(car.WillChangeRoad(deltaTime, speed))
                    {
                        int newHash = car.GetNextHashCode();
                        RoadSegment nextSegment;
                        //if traffic density exists we have to check if we can move over there, if not we stay still and stop the entire movement on the road
                        if(roadSegments.TryGetValue(newHash, out nextSegment))
                        {
                            if(nextSegment.CanAddCar()) //if (true)
                            {
                                car.MoveCar(currentTime, deltaTime, speed);
                                //we also need to add it to the next segment as otherwise we might overlad this next road
                                nextSegment.AddCar(car);
                            }
                            else
                            {
                                //this means that we cannot move upstreams 
                                roadSegment.Value.upstreamMovementBlocked = true;
                            }
                        }
                        //we also have to check if we move to any place that previous cars have already moved to that did not exists prior
                        else if (newRoadSegments.TryGetValue(newHash, out nextSegment))
                        {
                            if (nextSegment.CanAddCar()) //if (true)
                            {
                                car.MoveCar(currentTime, deltaTime, speed);
                                //we also need to add it to the next segment as otherwise we might overlad this next road
                                nextSegment.AddCar(car);
                            }
                            else
                            {
                                //this means that we cannot move upstreams 
                                roadSegment.Value.upstreamMovementBlocked = true;
                            }
                        }
                        else
                        {
                            car.MoveCar(currentTime, deltaTime, speed);
                            //now we need to add to our temporary road segment dictionary since otherwise we might overfill any new segment
                            int hash = car.roadSegmentHash;
                            nextSegment = new RoadSegment(car, this);
                            newRoadSegments.Add(hash, nextSegment);
                        }
                    }
                    else
                    {
                        car.MoveCar(currentTime, deltaTime, speed);
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
                int hash = car.roadSegmentHash;
                RoadSegment t;
                if (roadSegments.TryGetValue(hash, out t))
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
                    roadSegments.Add(hash, new RoadSegment(car, this));
                    carsOnHold.Remove(car);
                    carsInSystem.Add(car);
                }
            }
            
            //output stuff
            if(roadSegments.Count == 0)
            {
                averageSpeed = 0f;
            }
            else
            {
                averageSpeed /= (float)roadSegments.Count;
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
#if DEBUG            
            string newOut = currentTime + ","+ roadSegments.Count+","+ upstreamMovementBlockedSegment+ "," + blockedCars +","+ (totalCarsSimulated - oldTotalCars) + "," 
                            + carsOnHold.Count+"," + carsToRemove.Count + "," + carsInSystem.Count + "," + countCarsInSystem +","+ exitingPeople + ", " + averageSpeed + "," + minSpeed;
#else
            string newOut = currentTime + "," + (totalCarsSimulated - oldTotalCars) + "," + carsToRemove.Count + "," + carsInSystem.Count + "," + exitingPeople + ", " + averageSpeed + "," + minSpeed;
#endif


            for (int i = 0; i < WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals.Count; ++i)
            {
                newOut += "," + WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals[i].currentPeople;
                newOut += "," + WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals[i].currentFlow;
            }

            output.Add(newOut);
            oldTotalCars = totalCarsSimulated;

            //remove cars that has arrived
            for (int i = 0; i < carsToRemove.Count; ++i)
            {
                carsInSystem.Remove(carsToRemove[i]);

                //save output data for funtional analysis
                arrivalData.Add(currentTime);                            
            }

            if(evacGoalsDirty)
            {
                UpdateEvacGoalsInternal();
            }

            WUInity.INSTANCE.SaveTransientDensityData(currentTime, carsInSystem, carsOnHold);

#if DEBUG
//            if ((int)(currentTime - WUInity.SIM.StartTime) % 30 == 0 && upstreamMovementBlockedSegment > 0)
//            {
//                WUInity.LOG(WUInity.LogType.Log, "Blocked roadSegments: " + upstreamMovementBlockedSegment + " Blocked Cars: " + blockedCars + " minRoadCapa: " + minimumMaxCarsOnRoad);
//            }
#endif
        }

        public Vector4[] GetCarPositionsAndStates()
        {
            Vector4[] carsRendering = new Vector4[carsInSystem.Count];
            for (int i = 0; i < carsRendering.Length; i++)
            {
                carsRendering[i] = carsInSystem[i].GetUnityPositionAndSpeed(true);
            }
            return carsRendering;
        }

        //add parameter for flow reduction by adding background traffic as a density
        private Dictionary<int, RoadSegment> CollectRoadSegments()
        {
            Dictionary<int, RoadSegment> tDD = new Dictionary<int, RoadSegment>();
            for (int i = 0; i < carsInSystem.Count; ++i)
            {
                MacroCar c = carsInSystem[i];

                int hash = c.roadSegmentHash;

                RoadSegment t;
                if (tDD.TryGetValue(hash, out t))
                {
                    t.AddCar(c);
                }
                else
                {
                    tDD.Add(hash, new RoadSegment(c, this));
                }
            }
            return tDD;
        }

        private class RoadSegment
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
            public bool upstreamMovementBlocked;
            LinearSpline2D spline;
            public float speedLimit;

            public string GetStreetName()
            {
                return streetName;
            }

            public RoadSegment(MacroCar car, MacroTrafficSim mCS)
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
                upstreamMovementBlocked = false;

                CalculateSpline(car);
                speedLimit = car.GetAndSetCurrentSpeedLimit();
                car.SetSpline(spline);
            }

            //new way of interpolating in GUI
            public void CalculateSpline(MacroCar car)
            {
                RouteData routeData = car.routeData;
                int currentShapeIndex = car.currentShapeIndex;
               
                int startSI = routeData.route.ShapeMeta[currentShapeIndex - 1].Shape;
                int endSI = routeData.route.ShapeMeta[currentShapeIndex].Shape;
                int points = endSI - startSI + 1;
                Vector3[] segmentCoordinates = new Vector3[points];
                float distance = 0;
                for (int i = 0; i < points; i++)
                {
                    Itinero.LocalGeo.Coordinate coordinate = routeData.route.Shape[i + startSI];
                    Mapbox.Utils.Vector2d unityPos = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(coordinate.Latitude, coordinate.Longitude, WUInity.MAP.CenterMercator, WUInity.MAP.WorldRelativeScale);
                    if (i > 0)
                    {
                        distance += Vector2.Distance(new Vector2((float)unityPos.x, (float)unityPos.y), new Vector2(segmentCoordinates[i - 1].y, segmentCoordinates[i - 1].z));
                    }
                    //segmentCoordinates[i] = new Vector3(distance / currentShapeLength, (float)unityPos.x, (float)unityPos.y); // for catmull-rom splines we need fraction of distance
                    segmentCoordinates[i] = new Vector3(distance, (float)unityPos.x, (float)unityPos.y);
                }
                //spline = new CatmullRomSpline2D(segmentCoordinates);
                spline = new LinearSpline2D(segmentCoordinates);
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
                car.SetCurrentSpeedLimit(speedLimit);
                car.SetSpline(spline);
                cars.Add(car);
            }

            public int GetMaxCarsOnRoad()
            {
                return maxCarsOnRoad;
            }

            public float CalculateSpeedBasedOnDensity()
            {
                TrafficInput tO = WUInity.INPUT.Traffic;
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
            RoadData[] r = WUInity.RUNTIME_DATA.Traffic.RoadTypeData.roadData;
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
            float speed = RoadTypeData.default_value.speedLimit;
            RoadData[] r = WUInity.RUNTIME_DATA.Traffic.RoadTypeData.roadData;
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
            int lanes = RoadTypeData.default_value.lanes;
            RoadData[] r = WUInity.RUNTIME_DATA.Traffic.RoadTypeData.roadData;
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
            bool canReverseLanes = RoadTypeData.default_value.canBeReversed;
            RoadData[] r = WUInity.RUNTIME_DATA.Traffic.RoadTypeData.roadData;
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

        public void SaveToFile(int runNumber)
        {
            WUInityInput wuiIn = WUInity.INPUT;
            string path = System.IO.Path.Combine(WUInity.OUTPUT_FOLDER, wuiIn.Simulation.SimulationID + "_traffic_output_" + runNumber + ".csv");
            System.IO.File.WriteAllLines(path, output);
        }
    }
}