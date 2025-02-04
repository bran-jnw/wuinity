//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace WUIPlatform.Traffic
{
    public class SUMOModule : TrafficModule
    {
        private Dictionary<string, SUMOCar> cars;        
        private Vector2d _offset;
        Dictionary<StartGoal, string> validRouteIDs;
        private double adjustX, adjustY;

        //output
        private uint totalCarsArrived, totalPeopleArrived;
        private int currentCarsInSystem;
        private int totalCarsInjected;
        private List<string> output;

        public SUMOModule(out bool success)
        {
            success = true;
            try
            {
                /*if (LIBSUMO.Simulation.isLoaded())
                {
                    LIBSUMO.Simulation.close();
                }*/

                cars = new Dictionary<string, SUMOCar>();
                string inputFile = Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Traffic.sumoInput.inputFile);
                //see here for options https://sumo.dlr.de/docs/sumo.html, setting input file, start and end time
                LIBSUMO.Simulation.start(new LIBSUMO.StringVector(new String[] { "sumo", "-c", inputFile, "-b", WUIEngine.SIM.StartTime.ToString(), "-e", WUIEngine.INPUT.Simulation.MaxSimTime.ToString() })); //, "--ignore-route-errors"

                //need to use UTM projection in SUMO and WUInity to overlay data (an dapproximate overlay with web mercator, e.g. Mapbox)
                Vector2d sumoUTM = new Vector2d(-WUIEngine.INPUT.Traffic.sumoInput.UTMoffset.x, -WUIEngine.INPUT.Traffic.sumoInput.UTMoffset.y);
                _offset = sumoUTM - WUIEngine.RUNTIME_DATA.Simulation.UTMOrigin;

                WUIEngine.LOG(WUIEngine.LogType.Debug, "SUMO origin offset [x, y]: " + _offset.x + ", " + _offset.y);

                //keep for now if we ever want to do projection corrections here
                //https://gis.stackexchange.com/questions/14528/better-distance-measurements-in-web-mercator-projection
                /*double e = 0.081819191;
                double lat = Math.PI * WUIEngine.INPUT.Simulation.LowerLeftLatLong.x / 180.0;
                double cosLat = Math.Cos(lat);
                adjustX = cosLat / Math.Sqrt(1.0 - e * e * Math.Sin(lat) * Math.Sin(lat));
                adjustX = 1.0 / adjustX;
                adjustY = cosLat * (1.0 - e * e) / Math.Pow(1 - e * e * Math.Sin(lat) * Math.Sin(lat), 1.5);
                adjustY = 1.0 / adjustY;;*/

                validRouteIDs = new Dictionary<StartGoal, string>();

                output = new List<string>();
                string header = "Time(s),Total cars injected, Total cars arrived,Current cars in system, Exiting people";
                output.Add(header);

                SortEdgesInFireCells();
            }
            catch
            {
                success = false; 
                WUIEngine.LOG(WUIEngine.LogType.Error, "Could not start SUMO, aborting");
            }
            
        }

        //might crash SUMO when running a new instance of SUMOModule while the old one is garbage collected
        /*~SUMOModule()
        {
            LIBSUMO.Simulation.close();
        }*/

        public override void Step(float deltaTime, float currentTime)
        {
            if(!LIBSUMO.Simulation.isLoaded())
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, "Trying to update SUMO but SUMO DLL is not loaded.");
                return;
            }            

            //https://sumo.dlr.de/doxygen/d0/d17/classlibsumo_1_1_simulation.html#afc1f3d5c1c92f49a8bf40e42bdb333ab
            LIBSUMO.Simulation.step(currentTime + deltaTime); // advances sim up to given time

            //update positions
            LIBSUMO.StringVector activeCars = LIBSUMO.Vehicle.getIDList();
            currentCarsInSystem = 0;
            if(activeCars.Count > 0)
            {
                for (int i = 0; i < activeCars.Count; i++)
                {
                    string sumoID = activeCars[i];
                    SUMOCar car;
                    cars.TryGetValue(sumoID, out car);
                    if(car != null)
                    {
                        LIBSUMO.TraCIPosition pos = LIBSUMO.Vehicle.getPosition(sumoID);
                        car.SetPosRot(pos, (float)LIBSUMO.Vehicle.getAngle(sumoID));
                        if(car.numberOfPeopleInCar > 0)
                        {
                            currentCarsInSystem++;
                        }
                    }
                    //this can happen since SUMO can have control of car injection as well, not only injected from WUInity
                    else
                    {
                        car = new SUMOCar(GetNewCarID(), sumoID, LIBSUMO.Vehicle.getPosition(sumoID), LIBSUMO.Vehicle.getAngle(sumoID), 0, null);
                        cars.Add(sumoID, car);
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
                    //if car is internal to SUMO they have 0 passengers from the point of view of the simulation
                    if(car.numberOfPeopleInCar > 0)
                    {
                        arrivalData.Add(currentTime + deltaTime);
                        totalCarsArrived++;
                        totalPeopleArrived += car.numberOfPeopleInCar;
                    }                    
                }
            }

            //only update if we have any cars, else we break the carsToRender buffer (default dummy of 1 car)
            if(cars.Count > 0)
            {
                //finally update visuals            
                if (carsToRender == null || carsToRender.Length != cars.Count)
                {
                    Vector4[] buffer = new Vector4[cars.Count];
                    int index = 0;
                    foreach (SUMOCar car in cars.Values)
                    {
                        buffer[index] = car.GetPositionAndSpeed(true);
                        buffer[index].X += (float)_offset.x;
                        buffer[index].Y += (float)_offset.y;
                        if(WUIEngine.INPUT.Simulation.ScaleToWebMercator)
                        {
                            buffer[index].X *= (float)WUIEngine.RUNTIME_DATA.Simulation.UtmToMercatorScale.x;
                            buffer[index].Y *= (float)WUIEngine.RUNTIME_DATA.Simulation.UtmToMercatorScale.y;
                        }                        
                        ++index;
                    }
                    carsToRender = buffer;
                }
            }

            //Time(s),Total cars injected, Total cars arrived,Current cars in system, Exiting people
            string dataLine = currentTime + "," + totalCarsInjected + "," + totalCarsArrived + "," + currentCarsInSystem + "," + totalPeopleArrived;
            output.Add(dataLine);
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

        public override void HandleNewCars()
        {
            foreach (InjectedCar injectedCar in carsToInject)
            {
                EvacuationGoal evacuationGoal = injectedCar.evacuationGoal;
                uint numberOfPeopleInCar = injectedCar.numberOfPeopleInCar;
                Vector2d startLatLon = injectedCar.startLatLong;                
                Vector2d goalLatLon = evacuationGoal.latLon;
                //used to get a hash that represent the route
                StartGoal startGoal = new StartGoal(startLatLon.x, startLatLon.y, goalLatLon.x, goalLatLon.y);

                bool invalidRoute = true;
                LIBSUMO.TraCIStage route = null;
                string routeID;

                //TODO: create input for this...
                string vehicleType = "evacuation_car";

                //see if route already exist
                if (validRouteIDs.TryGetValue(startGoal, out routeID))
                {
                    uint carID = GetNewCarID();
                    string sumoID = carID.ToString();
                    LIBSUMO.Vehicle.add(sumoID, routeID);//, vehicleType);
                    LIBSUMO.TraCIPosition startPos = LIBSUMO.Vehicle.getPosition(sumoID);
                    //if we reach here the route should be valid
                    SUMOCar car = new SUMOCar(carID, sumoID, startPos, 0, numberOfPeopleInCar, evacuationGoal);
                    cars.Add(sumoID, car);
                    invalidRoute = false;
                }
                //no route found, try to create new one
                else
                {
                    //IMPORTANT!!! Longitude then latitude in SUMO
                    LIBSUMO.TraCIRoadPosition startRoad = LIBSUMO.Simulation.convertRoad(startLatLon.y, startLatLon.x, true);
                    LIBSUMO.TraCIRoadPosition goalRoad = LIBSUMO.Simulation.convertRoad(goalLatLon.y, goalLatLon.x, true);       

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
                        WUIEngine.LOG(WUIEngine.LogType.Warning, "SUMO: " + e.Message);
                    }

                    //SUMO returned route that might work
                    if (!invalidRoute)
                    {
                        invalidRoute = true;
                        //https://sumo.dlr.de/docs/Simulation/Routing.html#travel-time_values_for_routing
                        try
                        {                            
                            routeID = "route_" + (validRouteIDs.Count + routeIDoffset);
                            LIBSUMO.Route.add(routeID, route.edges);
                            uint carID = GetNewCarID();
                            string sumoID = carID.ToString();
                            LIBSUMO.Vehicle.add(sumoID, routeID);//, vehicleType);
                            LIBSUMO.TraCIPosition startPos = LIBSUMO.Vehicle.getPosition(sumoID);

                            //if we reach here the route should be valid
                            SUMOCar car = new SUMOCar(carID, sumoID, startPos, 0, numberOfPeopleInCar, evacuationGoal);
                            cars.Add(sumoID, car);
                            validRouteIDs.Add(startGoal, routeID);
                            invalidRoute = false;
                        }
                        catch (Exception e)
                        {
                            WUIEngine.LOG(WUIEngine.LogType.Warning, "SUMO: " + e.Message);
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
                        int routeIndex = Random.Range(0, validRouteIDs.Count - 1);
                        routeID = validRouteIDs.ElementAt(routeIndex).Value;
                        LIBSUMO.Vehicle.add(sumoID, routeID);//, vehicleType);
                        LIBSUMO.TraCIPosition startPos = LIBSUMO.Vehicle.getPosition(sumoID);
                        SUMOCar car = new SUMOCar(carID, sumoID, startPos, 0, numberOfPeopleInCar, evacuationGoal);
                        cars.Add(sumoID, car);
                        invalidRoute = false;
                        WUIEngine.LOG(WUIEngine.LogType.Warning, "No route could be found for the injected car, so it was teleported to a valid location.");
                    }
                    else
                    {
                        WUIEngine.LOG(WUIEngine.LogType.Warning, "Car could not be injected as no valid route was found or cached.");
                    }
                }

                if(!invalidRoute)
                {
                    ++totalCarsInjected;
                }                
            } 
            
            carsToInject.Clear();
        }

        public override bool IsSimulationDone()
        {
            if(totalCarsArrived == totalCarsInjected)
            {
                return true;
            }

            return false;
        }

        public override Vector4[] GetCarPositionsAndStates()
        {
            //safe to skip?
            /*if (!LIBSUMO.Simulation.isLoaded())
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, "SUMO is not loaded when trying to access car positions.");
            }*/
            
            return carsToRender;
        }

        public override int GetCarsInSystem()
        {
            return currentCarsInSystem;
        }

        public override int GetTotalCarsSimulated()
        {
            return totalCarsInjected;
        }        

        public override void InsertNewTrafficEvent(TrafficEvent tE)
        {
            //throw new System.NotImplementedException();
        }

        public override void SaveToFile(int runNumber)
        {
            try
            {
                string path = Path.Combine(WUIEngine.OUTPUT_FOLDER, WUIEngine.INPUT.Simulation.SimulationID + "_traffic_output_" + runNumber + ".csv");
                File.WriteAllLines(path, output);
            }
            catch(Exception e)
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, e.Message);
            }
        }
                
        public override void UpdateEvacuationGoals()
        {
            //throw new System.NotImplementedException();
        }

        /// <summary>
        /// This method requires adding the getIncomingEdes call to SUMO, this is done in the current DLL but keep an eye on it.
        /// </summary>
        List<string>[,] fireCellEdges;
        private void SortEdgesInFireCells()
        {
            if(!WUIEngine.INPUT.Simulation.RunFireModule)
            {
                WUIEngine.LOG(WUIEngine.LogType.Log, "No fire module loaded, can't  sort SUMO network edges in fire cells.");
                return;
            }

            try
            {
                int fireCellsWithJunctions = 0;
                LIBSUMO.StringVector junctions = LIBSUMO.Junction.getIDList();
                fireCellEdges = new List<string>[WUIEngine.SIM.FireModule.GetCellCountX(), WUIEngine.SIM.FireModule.GetCellCountY()];

                for (int i = 0; i < junctions.Count; i++)
                {
                    LIBSUMO.TraCIPosition nodePos = LIBSUMO.Junction.getPosition(junctions[i]);
                    int cellIndexX = (int)((nodePos.x + _offset.x) / WUIEngine.SIM.FireModule.GetCellSizeX());
                    int cellIndexY = (int)((nodePos.y + _offset.y) / WUIEngine.SIM.FireModule.GetCellSizeY());

                    if (cellIndexX > 0 && cellIndexX < WUIEngine.SIM.FireModule.GetCellCountX() - 1 &&
                        cellIndexY > 0 && cellIndexY < WUIEngine.SIM.FireModule.GetCellCountY() - 1)
                    {
                        LIBSUMO.StringVector incomingEdges = LIBSUMO.Junction.getIncomingEdges(junctions[i]);
                        for (int j = 0; j < incomingEdges.Count; j++)
                        {
                            //internal edges starts with ":", skip these
                            if (!incomingEdges[j].StartsWith(":"))
                            {
                                if (fireCellEdges[cellIndexX, cellIndexY] == null)
                                {
                                    fireCellEdges[cellIndexX, cellIndexY] = new List<string>();
                                    ++fireCellsWithJunctions;
                                }

                                fireCellEdges[cellIndexX, cellIndexY].Add(incomingEdges[j]);
                            }                            
                        }
                    }
                }

                WUIEngine.LOG(WUIEngine.LogType.Log, "Number of fire cells that have road junctions and will affect traffic:" + fireCellsWithJunctions);
            }
            catch (Exception e) 
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, e.Message);
            }            
        }

        public override void HandleIgnitedFireCells(List<Vector2int> cellIndices)
        {
            for (int i = 0; i < cellIndices.Count; i++)
            {
                FireCellIgnited(cellIndices[i].x, cellIndices[i].y);
            }
        }

        int routeIDoffset = 0;
        private void FireCellIgnited(int x, int y)
        {
            //make fire affected edges (based on junction) really slow 
            if (fireCellEdges[x, y] != null)
            {
                WUIEngine.LOG(WUIEngine.LogType.Log, "Cell " + x + "," + y + " has been ignited, handling road closure.");
                for (int i = 0; i < fireCellEdges[x, y].Count; i++)
                {
                    //https://sumo.dlr.de/docs/Simulation/Routing.html
                    //after testing this ewems to be the best option
                    LIBSUMO.Edge.adaptTraveltime(fireCellEdges[x, y][i], double.MaxValue);
                }
            }
            else
            {
                return;
            }

            //need to update cached routes
            try
            {
                Dictionary<StartGoal, string> newValidRoutesIDs = new Dictionary<StartGoal, string>();
                routeIDoffset += validRouteIDs.Count + 1;
                int index = 0;
                foreach (var p in validRouteIDs)
                {
                    LIBSUMO.StringVector routeEdges = LIBSUMO.Route.getEdges(p.Value);
                    LIBSUMO.TraCIStage route = LIBSUMO.Simulation.findRoute(routeEdges.First(), routeEdges.Last());

                    string routeID = "route_" + (index + routeIDoffset);
                    ++index;
                    if (route.edges.Count == 0 )
                    {
                        if(newValidRoutesIDs.Count > 0)
                        {
                            int routeIndex = Random.Range(0, newValidRoutesIDs.Count - 1);
                            routeID = newValidRoutesIDs.ElementAt(routeIndex).Value;
                        }
                        else
                        {
                            //this should not happen, this meansa that no route could be created and we use the old one
                            newValidRoutesIDs.Add(p.Key, p.Value);
                        }
                    }
                    else
                    {
                        LIBSUMO.Route.add(routeID, route.edges);
                    }
                    newValidRoutesIDs.Add(p.Key, routeID);
                }
                validRouteIDs.Clear();
                validRouteIDs = newValidRoutesIDs;
            }
            catch (Exception e)
            {
                WUIEngine.LOG(WUIEngine.LogType.Log, e.Message);
            }

            //force update routes on all exisiting cars in system
            foreach (SUMOCar car in cars.Values)
            {
                LIBSUMO.Vehicle.rerouteTraveltime(car.GetVehicleID());
            }
        }

        public override bool IsNetworkReachable(Vector2d startLatLong)
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            LIBSUMO.Simulation.close();
        }
    }
}
