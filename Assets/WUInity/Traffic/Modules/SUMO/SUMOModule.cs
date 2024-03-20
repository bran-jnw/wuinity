using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.ConstrainedExecution;

namespace WUInity.Traffic
{
    public class SUMOModule : TrafficModule
    {
        private Dictionary<string, SUMOCar> cars;
        private int carsInSystem;
        private Vector2d offset;

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
                LIBSUMO.Simulation.start(new LIBSUMO.StringVector(new String[] { "sumo", "-c", inputFile, "-b", WUInity.SIM.StartTime.ToString(), "-e", WUInity.INPUT.Simulation.MaxSimTime.ToString() }));

                var v = LIBSUMO.Simulation.getNetBoundary();
                WUInity.LOG(WUInity.LogType.Log, "V: " + v.getString()); 

                Vector2d centerMercator = new Vector2d(WUInity.MAP.CenterMercator.x, WUInity.MAP.CenterMercator.y);

                //Conversions.LatLonToMeters(39.377313, -105.135152)
                //WUInity.LOG(WUInity.LogType.Log, "Center mercator, x:" + centerMercator.x + ", y:" + centerMercator.y);
                offset = new Vector2d(-2646, -3616);// - Conversions.LatLonToMeters(WUInity.INPUT.Simulation.LowerLeftLatLong.x, WUInity.INPUT.Simulation.LowerLeftLatLong.y);
                //new Vector2d(-11707243.866286842, 4774513.5555263935)
                WUInity.LOG(WUInity.LogType.Log, "SUMO started.");
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

        public override void Update(float deltaTime, float currentTime)
        {
            if(!LIBSUMO.Simulation.isLoaded())
            {
                WUInity.LOG(WUInity.LogType.Error, "Trying to update but SUMO is not loaded.");
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
        }

        /// <summary>
        /// Inject new car into the SUMO simulation, ignores the RouteData (used in MacroTrafficSim only).
        /// </summary>
        /// <param name="startLatLong"></param>
        /// <param name="evacuationGoal"></param>
        /// <param name="routeData"></param>
        /// <param name="numberOfPeopleInCar"></param>
        public override void InsertNewCar(Vector2d startLatLong, EvacuationGoal evacuationGoal, RouteData routeData, uint numberOfPeopleInCar)
        {
            if (!LIBSUMO.Simulation.isLoaded())
            {
                WUInity.LOG(WUInity.LogType.Error, "Trying to inject new car but SUMO is not loaded.");
                return;
            }

            uint carID = GetNewCarID();
            string sumoID = GetNewCarID().ToString();

            Vector2d goalPos = evacuationGoal.latLong;

            //IMPORTANT!!! Longitude then latitude
            LIBSUMO.TraCIRoadPosition startRoad = LIBSUMO.Simulation.convertRoad(startLatLong.y, startLatLong.x, true);
            LIBSUMO.TraCIRoadPosition goalRoad = LIBSUMO.Simulation.convertRoad(goalPos.y, goalPos.x, true);

            //TODO: check if route exists, if not create new one

            //https://sumo.dlr.de/docs/Simulation/Routing.html#travel-time_values_for_routing
            try
            {
                LIBSUMO.TraCIStage route = LIBSUMO.Simulation.findRoute(startRoad.edgeID, goalRoad.edgeID);
                string routeID = sumoID + "_route";

                //TODO: save route for later checks
                LIBSUMO.Route.add(routeID, route.edges);
                LIBSUMO.Vehicle.add(sumoID, routeID);

                LIBSUMO.TraCIPosition startPos = LIBSUMO.Vehicle.getPosition(sumoID);

                SUMOCar car = new SUMOCar(carID, sumoID, startPos, 0, numberOfPeopleInCar, evacuationGoal);
                cars.Add(sumoID, car);
                //WUInity.LOG(WUInity.LogType.Log, "SUMO placed vehicle.");
            }
            catch (Exception e) 
            {
                //WUInity.LOG(WUInity.LogType.Warning, "SUMO could not place vehicle.");
            }
        }

        public override bool SimulationDone()
        {
            return carsInSystem > 0 ? false : true;
        }

        public override Vector4[] GetCarPositionsAndStates()
        {
            if (!LIBSUMO.Simulation.isLoaded())
            {
                WUInity.LOG(WUInity.LogType.Warning, "SUMO is not loaded.");
                return new Vector4[0];
            }

            Vector4[] carsRendering = new Vector4[cars.Count];
            int i = 0;
            foreach(SUMOCar car in cars.Values)
            {
                carsRendering[i] = car.GetPositionAndSpeed(true);
                carsRendering[i].X += (float)offset.x;
                carsRendering[i].Y += (float)offset.y;
                ++i;
            }
            return carsRendering;
        }

        public override int GetCarsInSystem()
        {
            return carsInSystem;
        }

        public override int GetTotalCarsSimulated()
        {
            return cars.Count;
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
