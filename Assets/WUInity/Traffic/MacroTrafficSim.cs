using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity.Traffic
{
    [System.Serializable]
    public class MacroTrafficSim
    {        
        public abstract class TrafficEvent
        {
            public float startTime;
            public float endTime;
            public bool isActive;

            public abstract void ApplyEffects(MacroTrafficSim mTS);
            public abstract void RemoveEffects(MacroTrafficSim mTS);
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

            public static TrafficAccident[] GetDummy()
            {
                TrafficAccident[] tAs = new TrafficAccident[1];
                tAs[0] = new TrafficAccident(float.MaxValue - 1f, float.MaxValue);
                return tAs;
            }
        }        

        List<MacroCar> macroCars;
        int totalCarsSimulated;
        int oldTotalCars;
        List<string> output;
        public bool reverseLanes;
        public bool stallBigRoads;
        List<TrafficEvent> trafficEvents;
        RouteCreator routeCreator;
        Dictionary<int, TrafficDensityData> trafficDensityData;

        public MacroTrafficSim(RouteCreator rC)
        {
            macroCars = new List<MacroCar>(10000);
            totalCarsSimulated = 0;
            oldTotalCars = 0;

            output = new List<string>();
            
            string start = "Time(s),Injected cars,Exiting cars,Current cars in system, Exiting people, Avg. v [km/h], Min. v [km/h]";
            for (int i = 0; i < WUInity.WUINITY_IN.traffic.evacuationGoals.Length; ++i)
            {
                start += ", Goal: " + WUInity.WUINITY_IN.traffic.evacuationGoals[i].name;
                start += ", " + WUInity.WUINITY_IN.traffic.evacuationGoals[i].name + " flow";
            }
            output.Add(start);
            //string output = "Time(s),Injected cars,Exiting cars,Current cars in system";
            //SaveToFile(output, true);

            reverseLanes = false;
            stallBigRoads = false;
            trafficEvents = new List<TrafficEvent>();

            routeCreator = rC;
        }

        public void InsertNewCar(RouteData routeData, int numberOfPeopleInCar)
        {
            MacroCar car = new MacroCar(routeData, numberOfPeopleInCar);
            macroCars.Add(car);
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
            return macroCars.Count == 0 ? true : false;
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
                    WUInity.WUINITY_SIM.StopSim("WARNING! Null re-route returned to car, abort!");
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

            //slow as hell, works but freezes WUInity
            /*for (int i = 0; i < macroCars.Count; i++)
            {
                if(macroCars[i].routeData.evacGoal.blocked)
                {
                    Vector2D startPos = new Vector2D(macroCars[i].goingToCoord.Latitude, macroCars[i].goingToCoord.Longitude);
                    RouteData r = routeCreator.CalcTrafficRoute(startPos);
                    if (r == null)
                    {
                        WUInity.WUINITY_SIM.LogMessage("Null re-route returned to car, abort!");
                    }
                    //special case where start is almost same as end, don't change route for cars
                    else if (r.route.TotalDistance == 0 || r.route.Shape.Length == 1)
                    {
                        WUInity.WUINITY_SIM.LogMessage("Route messed up, start same as goal? Ignoring.");
                        //check if our startPos is the same as end evac goal, Itinero returns wonky route then
                        //if found, just don't re-calc route
                    }
                    //everything is as expected
                    else
                    {
                        macroCars[i].ChangeRoute(r);
                    }
                }
            }*/

            //old crap
            /*List<TrafficDensityData> tDD = CollectDensity();

            for (int i = 0; i < tDD.Count; ++i)
            {
                Vector2D startPos = new Vector2D(tDD[i].goalCoord.Latitude, tDD[i].goalCoord.Longitude);

                RouteData r = routeCreator.CalcTrafficRoute(startPos);

                if (r == null)
                {
                    MonoBehaviour.print("Null re-oute returned!");
                }
                //special case where start is almost same as end, don't change route for cars
                else if(r.route.TotalDistance == 0 || r.route.Shape.Length == 1)
                {
                    MonoBehaviour.print("Route messed up, start same as goal? Ignoring.");
                    MonoBehaviour.print(tDD[i].goalCoord.Latitude + ", " + tDD[i].goalCoord.Longitude);
                    //check if our startPos is the same as end evac goal, Itinero returns wonky route then
                    //if found, just don't re-calc route
                }
                //everything is as expected
                else
                {
                    for (int j = 0; j < tDD[i].cars.Count; ++j)
                    {
                        MacroCar car = tDD[i].cars[j];
                        //only update if actual new goal
                        if (car.routeData.evacGoal != r.evacGoal)
                        {
                            car.ChangeRoute(r);
                        }
                    }                                      
                }
            }*/
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

            trafficDensityData = CollectDensity();
            //List<TrafficDensityData> tDD = CollectDensity();
            List<MacroCar> carsToRemove = new List<MacroCar>(500);

            //old way
            /*for(int i = 0; i < macroCars.Count; ++i)
            {
                MacroCar car = macroCars[i];
                car.MoveCar(deltaTime);
                //flag cars that have arrived
                if(car.hasArrived)
                {
                    carsToRemove.Add(car);
                }
            }*/

            //new way, loop through traffic density data instead since we know each car on each segment and we need the density data
            float averageSpeed = 0;
            float minSpeed = 9999f;
            int exitingPeople = 0;
            foreach (KeyValuePair<int, TrafficDensityData> t in trafficDensityData)
            {
                float speed = t.Value.CalculateSpeedBasedOnDensity(); ; // tDD[i].CalculateSpeedModifierBasedOnDensity();
                averageSpeed += speed;
                minSpeed = speed < minSpeed ? speed : minSpeed;
                for (int j = 0; j < t.Value.cars.Count; ++j)
                {
                    MacroCar car = t.Value.cars[j];  
                    
                    //this stuff does not work anymore since we now have visibility conditions which migth break it, just using simpler stuff below
                    //first car/cars move faster
                    /*if(j < tDD[i].laneCount)
                    {
                        //car.MoveCarMod(deltaTime, 1.0f);
                        car.MoveCarSpeed(deltaTime, car.currentSpeedLimit);
                    }
                    else
                    {
                        //car.MoveCarMod(deltaTime, speedMod);
                        car.MoveCarSpeed(deltaTime, speed);
                    }*/

                    //move car, replaces stuff above
                    car.MoveCarSpeed(currentTime, deltaTime, speed);

                    //flag cars that have arrived
                    if (car.hasArrived)
                    {
                        carsToRemove.Add(car);
                        exitingPeople += car.numberOfPopleInCar;
                    }  
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
            string newOut = currentTime + "," + (totalCarsSimulated - oldTotalCars) + "," + carsToRemove.Count + "," + macroCars.Count + "," + exitingPeople + ", " + averageSpeed + "," + minSpeed;
            for (int i = 0; i < WUInity.WUINITY_IN.traffic.evacuationGoals.Length; ++i)
            {
                newOut += "," + WUInity.WUINITY_IN.traffic.evacuationGoals[i].currentPeople;
                newOut += "," + WUInity.WUINITY_IN.traffic.evacuationGoals[i].currentFlow;
            }

            output.Add(newOut);
            oldTotalCars = totalCarsSimulated;
            //string output = currentTime + "," + (totalCarsSimulated - oldTotalCars) + "," + carsToRemove.Count + "," + macroCars.Count;
            //SaveToFile(output, false);

            //remove cars that has arrived
            for (int i = 0; i < carsToRemove.Count; ++i)
            {
                //MonoBehaviour.print("Car exited: " + currentTime + ", tot travel dist: " + carsToRemove[i].totalTravelDistance + ". Expected dist: " + carsToRemove[i].route.TotalDistance + ", expected time; " + carsToRemove[i].route.TotalTime);
                //MonoBehaviour.print("Exit: " + currentTime + ", Expected t: " + carsToRemove[i].route.TotalTime + ", Used t: " + carsToRemove[i].totalDrivingTime);
                macroCars.Remove(carsToRemove[i]);                
            }

            if(evacGoalsDirty)
            {
                UpdateEvacGoalsInternal();
            }

            WUInity.WUINITY.SaveTransientDensityData(currentTime, macroCars);
        }

        public void SaveToFile(int runNumber)
        {
            WUInityInput wuiIn = WUInity.WUINITY_IN;
            System.IO.File.WriteAllLines(Application.dataPath + "/Resources/_output/" + wuiIn.simName + "_traffic_output_" + runNumber + ".csv", output);
        }

        //alternate way of saving, neeed to print each loop iteration, saves memory but is way slower. Update: useless? uses more memory?
        /*public void SaveToFile(string output, bool newFile)
        {
            if (newFile)
            {
                System.IO.File.Delete(@"C:\WUI-NITY\wui-nity_git\Assets\Resources\traffic_output.csv");
            }
            System.IO.File.AppendAllText(@"C:\WUI-NITY\wui-nity_git\Assets\Resources\traffic_output.csv", output + "\n");
            //System.IO.File.WriteAllLines(@"C:\WUI-NITY\wui-nity_git\Assets\Resources\traffic_output.csv", output);
        }*/

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

            public TrafficDensityData(MacroCar car, MacroTrafficSim mCS)
            {
                goalCoord = car.goingToCoord;
                streetName = car.drivingOnStreet;
                car.GetCurrentMetaData().Attributes.TryGetValue("highway", out highwayType);
                laneCount = GetNumberOfLanes(highwayType);
                maxCapacity = GetMaxCapacity(highwayType);
                length = car.currentShapeLength;
                cars = new List<MacroCar>(500);
                cars.Add(car);
                this.mCS = mCS;               
            }

            public void AddCar(MacroCar car)
            {
                cars.Add(car);
            }

            public float CalculateSpeedBasedOnDensity()
            {
                TrafficInput tO = WUInity.WUINITY_IN.traffic;
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

        //add parameter for flow reduction by adding background traffic as a density
        private Dictionary<int, TrafficDensityData> CollectDensity()
        {
            Dictionary<int, TrafficDensityData> tDD = new Dictionary<int, TrafficDensityData>(200);
            for (int i = 0; i < macroCars.Count; ++i)
            {
                MacroCar c = macroCars[i];

                int hash = c.densityHash;

                TrafficDensityData t;
                if (tDD.TryGetValue(hash, out t))
                {
                    //MonoBehaviour.print("We are going to the same node!");
                    t.AddCar(c);
                }
                else
                {
                    //MonoBehaviour.print("We are not going to the same node!");
                    tDD.Add(hash, new TrafficDensityData(c, this));
                }
            }
            return tDD;
        }

        /*private TrafficDensityData TrafficDensityExists(HashSet<TrafficDensityData> data, MacroCar car)
        {
            TrafficDensityData tDD = new TrafficDensityData(car, this);

            data.GetOr
            if(data.Contains(tDD))
            {
                return data;
            }
            else
            {
                return null;
            }
            //old
            int i = 0;
            foreach(TrafficDensityData tDD in data)
            {
                if (car.streetHash == tDD.streetHash && car.goingToCoord.Latitude == tDD.goalCoord.Latitude && car.goingToCoord.Longitude == tDD.goalCoord.Longitude)
                {
                    return tDD;
                }
                ++i;
            }*/

            /*for(int i = 0; i < data.Count; ++i)
            {
                if (car.streetHash == data[i].streetHash && car.goingToCoord.Latitude == data[i].goalCoord.Latitude && car.goingToCoord.Longitude == data[i].goalCoord.Longitude)
                {
                    index = i;
                    return true;
                }
            }

            return null;
        }*/   

        public static float GetMaxCapacity(string highway)
        {
            float capacity = 50.0f;
            RoadData[] r = WUInity.WUINITY_IN.traffic.roadTypes.roadData;
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
            RoadData[] r = WUInity.WUINITY_IN.traffic.roadTypes.roadData;
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
            RoadData[] r = WUInity.WUINITY_IN.traffic.roadTypes.roadData;
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
            RoadData[] r = WUInity.WUINITY_IN.traffic.roadTypes.roadData;
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