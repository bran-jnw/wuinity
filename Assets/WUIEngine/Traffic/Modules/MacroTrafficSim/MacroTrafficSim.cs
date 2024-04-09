using System.Collections.Generic;
using System.Numerics;
using WUIEngine.IO;

namespace WUIEngine.Traffic
{
    [System.Serializable]
    public class MacroTrafficSim : TrafficModule
    {     
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

        public MacroTrafficSim(RouteCreator rC)
        {
            carsInSystem = new List<MacroCar>();
            carsOnHold = new List<MacroCar>();
            totalCarsSimulated = 0;
            oldTotalCars = 0;

            output = new List<string>();
            
            string start = "Time(s),Injected cars,Exiting cars,Current cars in system, Exiting people, Avg. v [km/h], Min. v [km/h]";
            for (int i = 0; i < Engine.RUNTIME_DATA.Evacuation.EvacuationGoals.Count; ++i)
            {
                start += ", Goal: " + Engine.RUNTIME_DATA.Evacuation.EvacuationGoals[i].name;
                start += ", " + Engine.RUNTIME_DATA.Evacuation.EvacuationGoals[i].name + " flow";
            }
            output.Add(start);
            //string output = "Time(s),Injected cars,Exiting cars,Current cars in system";
            //SaveToFile(output, true);

            reverseLanes = false;
            stallBigRoads = false;
            trafficEvents = new List<TrafficEvent>();

            routeCreator = rC;

            Engine.LOG(Engine.LogType.Log, "Macro traffic sim initiated.");
        }

        public override void HandleNewCars()
        {
            foreach (InjectedCar injectedCar in carsToInject)
            {
                MacroCar car = new MacroCar(injectedCar.routeData, injectedCar.numberOfPeopleInCar, GetNewCarID());
                carsOnHold.Add(car);
                ++totalCarsSimulated;
            }
            carsToInject.Clear();
        }

        public override void InsertNewTrafficEvent(TrafficEvent tE)
        {
            trafficEvents.Add(tE);
        }

        public override int GetTotalCarsSimulated()
        {
            return totalCarsSimulated;
        }

        public override bool IsSimulationDone()
        {
            return carsInSystem.Count == 0 ? true : false;
        }

        public override int GetCarsInSystem()
        {
            return carsInSystem.Count;
        }

        bool evacGoalsDirty = false;
        public override void UpdateEvacuationGoals()
        {
            evacGoalsDirty = true;
        }

        private void UpdateEvacGoalsInternal()
        {
            evacGoalsDirty = false;

            //TODO: Do we need to re-calc traffic density data since some cars are gone after update loop?  Conservative not to (and cheaper)
            foreach (KeyValuePair<int, RoadSegment> t in roadSegments)
            {
                Vector2d startPos = new Vector2d(t.Value.goalCoord.Latitude, t.Value.goalCoord.Longitude);
                RouteData r = routeCreator.CalcTrafficRoute(startPos);
                if (r == null)
                {
                    Engine.SIM.StopSim("STOP! Null re-route returned to car, should not happen.");
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

        public override void Step(float deltaTime, float currentTime)
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
            uint exitingPeople = 0;
            //loop through each road segment that has a car and we need the density data to calculate current speed/density relation
            foreach (KeyValuePair<int, RoadSegment> roadSegment in roadSegments)
            {
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
                        //densitySpeed = WUIEngine.Input.traffic.stallSpeed / 3.6f;
                        break;
                    }

                    MacroCar car = roadSegment.Value.cars[j];
                    float speed = densitySpeed;
                    //the first car moves unimpeded
                    if(j == 0)
                    {
                        speed = car.currentSpeedLimit;
                    }
                    
                    //check if we are going on to a new stretch of road (new traffic density) after this time step
                    if(car.WillChangeRoad(deltaTime, speed))
                    {
                        int newHash = car.GetNextHashCode();
                        RoadSegment nextSegment;
                        //if traffic density exists we have to check if we can move over there, if not we stay still and stop the entire movement on the road
                        if(roadSegments.TryGetValue(newHash, out nextSegment))
                        {
                            if(nextSegment.CanAddCar())
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
                            if (nextSegment.CanAddCar())
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
            string newOut = currentTime + "," + (totalCarsSimulated - oldTotalCars) + "," + carsToRemove.Count + "," + carsInSystem.Count + "," + exitingPeople + ", " + averageSpeed + "," + minSpeed;
            for (int i = 0; i < Engine.RUNTIME_DATA.Evacuation.EvacuationGoals.Count; ++i)
            {
                newOut += "," + Engine.RUNTIME_DATA.Evacuation.EvacuationGoals[i].currentPeople;
                newOut += "," + Engine.RUNTIME_DATA.Evacuation.EvacuationGoals[i].currentFlow;
            }

            output.Add(newOut);
            oldTotalCars = totalCarsSimulated;

            //remove cars that has arrived
            for (int i = 0; i < carsToRemove.Count; ++i)
            {
                carsInSystem.Remove(carsToRemove[i]);

                //save output data for funtional analysis
                arrivalData.Add(currentTime + deltaTime);                            
            }

            if(evacGoalsDirty)
            {
                UpdateEvacGoalsInternal();
            }

            //update rendering
            carsToRender = new Vector4[carsInSystem.Count];
            for (int i = 0; i < carsToRender.Length; i++)
            {
                carsToRender[i] = carsInSystem[i].GetUnityPositionAndSpeed(true);
            }            

            //WUInity.INSTANCE.SaveTransientDensityData(currentTime, carsInSystem, carsOnHold);
        }     
        
        public override Vector4[] GetCarPositionsAndStates()
        {
            return carsToRender;
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
               

        public static float GetMaxCapacity(string highway)
        {
            float capacity = 50.0f;
            RoadData[] r = Engine.RUNTIME_DATA.Traffic.RoadTypeData.roadData;
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
            RoadData[] r = Engine.RUNTIME_DATA.Traffic.RoadTypeData.roadData;
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
            RoadData[] r = Engine.RUNTIME_DATA.Traffic.RoadTypeData.roadData;
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

        public static bool CanReverseLanes(string highway)
        {
            bool canReverseLanes = RoadTypeData.default_value.canBeReversed;
            RoadData[] r = Engine.RUNTIME_DATA.Traffic.RoadTypeData.roadData;
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

        public override void SaveToFile(int runNumber)
        {
            WUInityInput wuiIn = Engine.INPUT;
            string path = System.IO.Path.Combine(Engine.OUTPUT_FOLDER, wuiIn.Simulation.SimulationID + "_traffic_output_" + runNumber + ".csv");
            System.IO.File.WriteAllLines(path, output);
        }

        public override void HandleIgnitedFireCells(List<Vector2int> cellIndices)
        {
            throw new System.NotImplementedException();
        }
    }
}