using BitMiracle.LibTiff.Classic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using WUInity.Utility;

namespace WUInity.Traffic
{
    public class SUMOModule : TrafficModule
    {
        private Dictionary<string, SUMOCar> cars;
        private int carsInSystem;
        private int totalCarsSimulated;
        private Vector2d offset;
        Dictionary<StartGoal, string> validRouteIDs;

        public SUMOModule()
        {
            try
            {
                if (LIBSUMO.Simulation.isLoaded())
                {
                    LIBSUMO.Simulation.close();
                }

                cars = new Dictionary<string, SUMOCar>();
                string inputFile = Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.Traffic.sumoInput.inputFile);
                //see here for options https://sumo.dlr.de/docs/sumo.html, setting input file, start and end time
                LIBSUMO.Simulation.start(new LIBSUMO.StringVector(new String[] { "sumo", "-c", inputFile, "-b", WUInity.SIM.StartTime.ToString(), "-e", WUInity.INPUT.Simulation.MaxSimTime.ToString() })); //, "--ignore-route-errors"

                //need to use UTM projection in SUMO to match data, and since Mapbox is using Web mercator for calculations but UTM for tiles we need to do offset in UTM space
                Vector2d sumoUTM = new Vector2d(-WUInity.INPUT.Traffic.sumoInput.UTMoffset.x, -WUInity.INPUT.Traffic.sumoInput.UTMoffset.y);
                offset = sumoUTM - WUInity.RUNTIME_DATA.Simulation.UTMOrigin;

                validRouteIDs = new Dictionary<StartGoal, string>();

                WUInity.LOG(WUInity.LogType.Log, "SUMO initiated.");
            }
            catch
            {
                WUInity.LOG(WUInity.LogType.Error, "Could not start SUMO, aborting.");
            }
            
        }

        ~SUMOModule()
        {
            LIBSUMO.Simulation.close();
        }

        public override void Step(float deltaTime, float currentTime)
        {
            if(!LIBSUMO.Simulation.isLoaded())
            {
                WUInity.LOG(WUInity.LogType.Error, "Trying to update SUMO but SUMO DLL is not loaded.");
                return;
            }

            //https://sumo.dlr.de/doxygen/d0/d17/classlibsumo_1_1_simulation.html#afc1f3d5c1c92f49a8bf40e42bdb333ab
            LIBSUMO.Simulation.step(currentTime + deltaTime); // advances sim up to given time

            //update positions
            LIBSUMO.StringVector activeCars = LIBSUMO.Vehicle.getIDList();
            carsInSystem = activeCars.Count;
            if(activeCars.Count > 0)
            {
                for (int i = 0; i < activeCars.Count; i++)
                {
                    string sumoID = activeCars[i];
                    SUMOCar car;
                    cars.TryGetValue(sumoID, out car);
                    if(car != null)
                    {
                        car.SetPosRot(LIBSUMO.Vehicle.getPosition(sumoID), (float)LIBSUMO.Vehicle.getAngle(sumoID));
                    }                        
                }
            }     

            //check if any cars have arrived
            if (LIBSUMO.Simulation.getArrivedNumber() > 0)
            {
                LIBSUMO.StringVector arrivedCars = LIBSUMO.Simulation.getArrivedIDList();
                for (int i = 0; i < arrivedCars.Count; i++)
                {
                    SUMOCar car;
                    cars.TryGetValue(arrivedCars[i], out car);
                    if(car != null)
                    {
                        car.Arrive();
                    }
                    cars.Remove(arrivedCars[i]);
                    arrivalData.Add(currentTime + deltaTime);
                }
            }

            //finally update visuals            
            if(carsToRender == null || carsToRender.Length != cars.Count)
            {
                Vector4[] buffer = new Vector4[cars.Count];
                int index = 0;
                foreach (SUMOCar car in cars.Values)
                {
                    buffer[index] = car.GetPositionAndSpeed(true);
                    buffer[index].X += (float)offset.x;
                    buffer[index].Y += (float)offset.y;
                    ++index;
                }
                carsToRender = buffer;
            }      
        }

        private struct StartGoal
        {
            public double startLat,startLong, goalLat, goalLong;

            public StartGoal(double startLat, double startLong, double goalLat, double goalLong)
            {
                this.startLat = startLat;
                this.startLong = startLong;
                this.goalLat = goalLat;
                this.goalLong = goalLong;
            }
        }
        public override void PostStep()
        {
            /*if (!LIBSUMO.Simulation.isLoaded())
            {
                WUInity.LOG(WUInity.LogType.Error, "Trying to inject new car but SUMO is not loaded.");
                return;
            }*/

            foreach (InjectedCar injectedCar in carsToInject)
            {
                EvacuationGoal evacuationGoal = injectedCar.evacuationGoal;
                uint numberOfPeopleInCar = injectedCar.numberOfPeopleInCar;
                Vector2d startLatLong = injectedCar.startLatLong;                
                Vector2d goalPos = evacuationGoal.latLong;
                //used to get a hash that represent the route
                StartGoal startGoal = new StartGoal(startLatLong.x, startLatLong.y, goalPos.x, goalPos.y);

                bool invalidRoute = true;
                LIBSUMO.TraCIStage route = null;
                string routeID;

                //see if route already exist
                if (validRouteIDs.TryGetValue(startGoal, out routeID))
                {
                    uint carID = GetNewCarID();
                    string sumoID = carID.ToString();
                    LIBSUMO.Vehicle.add(sumoID, routeID);
                    LIBSUMO.TraCIPosition startPos = LIBSUMO.Vehicle.getPosition(sumoID);
                    //if we reach here the route should be valid
                    SUMOCar car = new SUMOCar(carID, sumoID, startPos, 0, numberOfPeopleInCar, evacuationGoal);
                    cars.Add(sumoID, car);
                    invalidRoute = false;
                }
                //no route found, try to create new one
                else
                {
                    //IMPORTANT!!! Longitude then latitude
                    LIBSUMO.TraCIRoadPosition startRoad = LIBSUMO.Simulation.convertRoad(startLatLong.y, startLatLong.x, true);
                    LIBSUMO.TraCIRoadPosition goalRoad = LIBSUMO.Simulation.convertRoad(goalPos.y, goalPos.x, true);

                    try
                    {
                        route = LIBSUMO.Simulation.findRoute(startRoad.edgeID, goalRoad.edgeID);
                        if (route.edges.Count > 0)
                        {
                            invalidRoute = false;
                        }
                    }
                    catch (Exception e)
                    {
                        //WUInity.print(e.Message);
                    }

                    //SUMO returned route that might work
                    if (!invalidRoute)
                    {
                        invalidRoute = true;
                        //https://sumo.dlr.de/docs/Simulation/Routing.html#travel-time_values_for_routing
                        try
                        {                            
                            routeID = "route_" + validRouteIDs.Count;
                            LIBSUMO.Route.add(routeID, route.edges);
                            uint carID = GetNewCarID();
                            string sumoID = carID.ToString();
                            LIBSUMO.Vehicle.add(sumoID, routeID);
                            LIBSUMO.TraCIPosition startPos = LIBSUMO.Vehicle.getPosition(sumoID);

                            //if we reach here the route should be valid
                            SUMOCar car = new SUMOCar(carID, sumoID, startPos, 0, numberOfPeopleInCar, evacuationGoal);
                            cars.Add(sumoID, car);
                            validRouteIDs.Add(startGoal, routeID);
                            invalidRoute = false;
                        }
                        catch (Exception e)
                        {
                            WUInity.print(e.Message);
                        }
                    }
                }

                //if we reach here we need to teleport the car to a new location as no valid route could be found
                if (invalidRoute)
                {
                    if (validRouteIDs.Count > 0)
                    {
                        uint carID = GetNewCarID();
                        string sumoID = carID.ToString();
                        int routeIndex = Random.Range(0, validRouteIDs.Count);
                        routeID = validRouteIDs.ElementAt(routeIndex).Value;
                        LIBSUMO.Vehicle.add(sumoID, routeID);
                        LIBSUMO.TraCIPosition startPos = LIBSUMO.Vehicle.getPosition(sumoID);
                        SUMOCar car = new SUMOCar(carID, sumoID, startPos, 0, numberOfPeopleInCar, evacuationGoal);
                        cars.Add(sumoID, car);
                        invalidRoute = false;
                    }
                    else
                    {
                        WUInity.LOG(WUInity.LogType.Warning, "Car could not be injected as no valid route was found or cached.");
                    }
                }

                if(!invalidRoute)
                {
                    ++totalCarsSimulated;
                }                
            } 
            
            carsToInject.Clear();
        }

        public override bool IsSimulationDone()
        {
            return carsInSystem > 0 ? false : true;
        }

        public override Vector4[] GetCarPositionsAndStates()
        {
            if (!LIBSUMO.Simulation.isLoaded())
            {
                WUInity.LOG(WUInity.LogType.Error, "SUMO is not loaded when trying to access car positions.");
            }
            
            return carsToRender;
        }

        public override int GetCarsInSystem()
        {
            return carsInSystem;
        }

        public override int GetTotalCarsSimulated()
        {
            return totalCarsSimulated;
        }        

        public override void InsertNewTrafficEvent(TrafficEvent tE)
        {
            //throw new System.NotImplementedException();
        }

        public override void SaveToFile(int runNumber)
        {
            //throw new NotImplementedException();
        }
                
        public override void UpdateEvacuationGoals()
        {
            //throw new System.NotImplementedException();
        }
    }

}
