//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using WUIPlatform.Evacuation;
using LIBSUMO = Eclipse.Sumo.Libsumo;

namespace WUIPlatform.Traffic
{
    public class SUMOModule : TrafficModule
    {
        private Dictionary<string, SUMOVehicle> _vehicles;        
        List<LIBSUMO.TraCIRoadPosition> _validStartPositions;

        //output
        private uint totalVehiclesArrived, totalPeopleArrived, totalSumoVehiclesArrived;
        private int currentVehiclessInSystem;
        private int totalVehiclesInjected, totalSumoVehiclesInjected;
        private List<string> output;

        float _maxUsage;
        private float[,] _usageMap;

        public SUMOModule(out bool success)
        {
            success = true;
            try
            {
                _vehicles = new Dictionary<string, SUMOVehicle>();
                string inputFile = Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Traffic.SumoInput.ConfigurationFile);
                //see here for options https://sumo.dlr.de/docs/sumo.html, setting input file, start and end time
                LIBSUMO.Simulation.start(new LIBSUMO.StringVector(new String[] { "sumo", "-c", inputFile, "-b", WUIEngine.SIM.StartTime.ToString(), "-e", WUIEngine.INPUT.Simulation.MaxSimTime.ToString() })); //, "--ignore-route-errors"

                //need to use UTM projection in SUMO and WUInity to overlay data
                Vector2d sumoUTM = new Vector2d(-WUIEngine.INPUT.Traffic.SumoInput.UTMoffset.x, -WUIEngine.INPUT.Traffic.SumoInput.UTMoffset.y);
                _originOffset = sumoUTM - WUIEngine.RUNTIME_DATA.Simulation.UTMOrigin;

                _validStartPositions = new List<LIBSUMO.TraCIRoadPosition>();

                output = new List<string>();
                string header = "Time(s),Total cars injected, Total cars arrived,Current cars in system, Exiting people,Total Sumo cars injected,Total Sumo cars arrived";
                output.Add(header);

                int xDim = Mathd.CeilToInt(WUIEngine.INPUT.Simulation.DomainSize.x / 25.0);
                int yDim = Mathd.CeilToInt(WUIEngine.INPUT.Simulation.DomainSize.y / 25.0);
                _maxUsage = 0f;
                _usageMap = new float[xDim, yDim];

                SortEdgesInFireCells();
            }
            catch(Exception e)
            {
                success = false;
                WUIEngine.LOG(WUIEngine.LogType.SimError, "Could not start SUMO, aborting. " + e.Message + ". " + e.InnerException);
            }
            
        }

        //might crash SUMO when running a new instance of SUMOModule while the old one is garbage collected
        /*~SUMOModule()
        {
            LIBSUMO.Simulation.close();
        }*/

        public override void Step(float deltaTime, float currentTime)
        {  
            //https://sumo.dlr.de/doxygen/d0/d17/classlibsumo_1_1_simulation.html#afc1f3d5c1c92f49a8bf40e42bdb333ab
            LIBSUMO.Simulation.step(currentTime + deltaTime); // advances sim up to given time

            //update positions
            LIBSUMO.StringVector activeVehicles = LIBSUMO.Vehicle.getIDList();
            currentVehiclessInSystem = 0;
            if(activeVehicles.Count > 0)
            {
                for (int i = 0; i < activeVehicles.Count; i++)
                {
                    string sumoID = activeVehicles[i];
                    SUMOVehicle vehicle;
                    _vehicles.TryGetValue(sumoID, out vehicle);
                    if(vehicle != null)
                    {
                        LIBSUMO.TraCIPosition pos = LIBSUMO.Vehicle.getPosition(sumoID);
                        vehicle.SetWorldPosItionAndRotation(pos, LIBSUMO.Vehicle.getAngle(sumoID), _originOffset);
                        if(vehicle.NumberOfPeople > 0)
                        {
                            currentVehiclessInSystem++;
                        }
                    }
                    //this can happen since SUMO can have control of car injection as well, not only injected from WUInity
                    else
                    {
                        vehicle = new SUMOVehicle(GetNewCarID(), sumoID, LIBSUMO.Vehicle.getPosition(sumoID), LIBSUMO.Vehicle.getAngle(sumoID), 0, null);
                        _vehicles.Add(sumoID, vehicle);
                        _activeVehicles.Add(vehicle.VehicleId, vehicle);
                        ++totalSumoVehiclesInjected;
                    }

                    UpdateUsageMap(vehicle, deltaTime);
                }
            }     

            //check if any cars have arrived
            if (LIBSUMO.Simulation.getArrivedNumber() > 0)
            {
                LIBSUMO.StringVector arrivedVehicles = LIBSUMO.Simulation.getArrivedIDList();
                for (int i = 0; i < arrivedVehicles.Count; i++)
                {
                    SUMOVehicle car;
                    _vehicles.TryGetValue(arrivedVehicles[i], out car);
                    if(car != null)
                    {
                        car.Arrive();
                    }
                    _vehicles.Remove(arrivedVehicles[i]);
                    //if car is internal to SUMO they have 0 passengers from the point of view of the simulation
                    if(car.NumberOfPeople > 0)
                    {
                        _arrivalData.Add(currentTime + deltaTime);
                        totalVehiclesArrived++;
                        totalPeopleArrived += car.NumberOfPeople;
                    }
                    else
                    {
                        ++totalSumoVehiclesArrived;
                    }
                }
            }

            //Time(s),Total cars injected, Total cars arrived,Current cars in system, Exiting people
            string dataLine = currentTime + "," + totalVehiclesInjected + "," + totalVehiclesArrived + "," + currentVehiclessInSystem + "," + totalPeopleArrived + "," + totalSumoVehiclesInjected + "," + totalSumoVehiclesArrived;
            output.Add(dataLine);
        }

        private void UpdateUsageMap(SUMOVehicle car, float deltaTime)
        {
            Vector2d pos = car.WorldPosition;

            int xIndex = (int)(_usageMap.GetLength(0) * pos.x / WUIEngine.INPUT.Simulation.DomainSize.x);
            int yIndex = (int)(_usageMap.GetLength(1) * pos.y / WUIEngine.INPUT.Simulation.DomainSize.y);

            //we can be outside as sometimes roads reach beyond simulation domain
            if (xIndex >= 0 && xIndex < _usageMap.GetLength(0) && yIndex >= 0 && yIndex < _usageMap.GetLength(1))
            {
                _usageMap[xIndex, yIndex] += deltaTime;
                if (_usageMap[xIndex, yIndex] > _maxUsage)
                {
                    _maxUsage = _usageMap[xIndex, yIndex];
                }
            }
        }

        public float[,] GetUsageMap()
        {
            return _usageMap;
        }

        public float GetMaxUsage()
        {
            return _maxUsage;
        }

        public override void HandleNewCars()
        {
            foreach (InjectedCar injectedCar in _carsToInject)
            {
                EvacuationDestination evacuationGoal = injectedCar.evacuationGoal;
                uint numberOfPeopleInCar = injectedCar.numberOfPeopleInCar;
                Vector2d startLatLon = injectedCar.startLatLong;                
                Vector2d goalLatLon = evacuationGoal.latLon;

                //TODO: create input for this...
                string vehicleType = "evacuation_car";

                try
                {
                    //IMPORTANT!!! Longitude then latitude in SUMO
                    LIBSUMO.TraCIRoadPosition startRoad = LIBSUMO.Simulation.convertRoad(startLatLon.y, startLatLon.x, true);
                    LIBSUMO.TraCIRoadPosition goalRoad = LIBSUMO.Simulation.convertRoad(goalLatLon.y, goalLatLon.x, true);
                    LIBSUMO.TraCIStage route;
                    //do we find route based on empty network/pure speed limits or do we take into account curretn state of network
                    if(true)
                    {
                        route = LIBSUMO.Simulation.findRoute(startRoad.edgeID, goalRoad.edgeID);
                    }
                    else
                    {
                        route = LIBSUMO.Simulation.findRoute(startRoad.edgeID, goalRoad.edgeID, "", -1, 1);
                    }

                    bool foundRoute = false;
                    if (route.edges.Count > 0)
                    {
                        _validStartPositions.Add(startRoad);
                        foundRoute = true;
                    }
                    //if we reach here we need to teleport the car to a new location as no valid route could be found
                    else if (_validStartPositions.Count > 0)
                    {
                        int randomStart = Random.Range(0, _validStartPositions.Count - 1);   
                        //TODO: actually save start/goal pairs as we might try to generate route from a random start position to a non-reachable current goal of the car
                        route = LIBSUMO.Simulation.findRoute(_validStartPositions[randomStart].edgeID, goalRoad.edgeID);    
                        if(route.edges.Count > 0)
                        {
                            foundRoute = true;
                            WUIEngine.LOG(WUIEngine.LogType.Warning, "No route could be found for the injected car, so it was teleported to a valid location. Affected lat/lon: " + startLatLon.x + ", " + startLatLon.y);
                        }
                        else
                        {
                            WUIEngine.LOG(WUIEngine.LogType.Warning, "No route could be found for the injected car, tried teleporting but no valid route could be found.");
                        }
                    }
                    else
                    {
                        WUIEngine.LOG(WUIEngine.LogType.Warning, "Car could not be injected as no valid route was found or cached.");
                    }

                    if(foundRoute)
                    {
                        uint carID = GetNewCarID();
                        string sumoID = carID.ToString();
                        string routeID = "wuiroute_" + carID;
                        LIBSUMO.Route.add(routeID, route.edges);
                        LIBSUMO.Vehicle.add(sumoID, routeID);//, vehicleType);
                        LIBSUMO.TraCIPosition startPos = LIBSUMO.Vehicle.getPosition(sumoID);
                        SUMOVehicle car = new SUMOVehicle(carID, sumoID, startPos, 0, numberOfPeopleInCar, evacuationGoal);
                        _vehicles.Add(sumoID, car);
                        _activeVehicles.Add(car.VehicleId, car);
                        ++totalVehiclesInjected;
                    }
                }
                catch (Exception e)
                {
                    WUIEngine.LOG(WUIEngine.LogType.Warning, "SUMO: " + e.Message);
                }              
            } 
            
            _carsToInject.Clear();
        }

        public override bool IsSimulationDone()
        {
            if(totalVehiclesArrived == totalVehiclesInjected)
            {
                return true;
            }

            return false;
        }

        public override int GetNumberOfCarsInSystem()
        {
            return currentVehiclessInSystem;
        }

        public override int GetTotalCarsSimulated()
        {
            return totalVehiclesInjected;
        }        

        public override void InsertNewTrafficEvent(TrafficEvent tE)
        {
            //throw new System.NotImplementedException();
        }

        public override void SaveToFile(int runNumber)
        {
            //arrival data to csv
            try
            {
                string path = Path.Combine(WUIEngine.OUTPUT_FOLDER, WUIEngine.INPUT.Simulation.Id + "_traffic_output_" + runNumber + ".csv");
                File.WriteAllLines(path, output);
            }
            catch(Exception e)
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, e.Message);
            }

            SaveUsageMap(runNumber);
        }

        private void SaveUsageMap(int runNumber)
        {
            //usage map as geotiff
            try
            {
                int xDim = _usageMap.GetLength(0);
                int yDim = _usageMap.GetLength(1);
                string path = Path.Combine(WUIEngine.OUTPUT_FOLDER, WUIEngine.INPUT.Simulation.Id + "_trafficUsageMap_" + runNumber + ".tiff");

                OSGeo.GDAL.Gdal.AllRegister();
                OSGeo.GDAL.Driver driver = OSGeo.GDAL.Gdal.GetDriverByName("GTiff");
                OSGeo.GDAL.Dataset output = driver.Create(path, xDim, yDim, 1, OSGeo.GDAL.DataType.GDT_Float32, null);

                double leftX = WUIEngine.RUNTIME_DATA.Simulation.UTMOrigin.x;
                double lowerLeftY = WUIEngine.RUNTIME_DATA.Simulation.UTMOrigin.y;
                double[] geoTransform = new double[] { leftX, 25.0, 0.0, lowerLeftY, 0.0, 25.0 };
                output.SetGeoTransform(geoTransform);

                OSGeo.OSR.SpatialReference reference = new OSGeo.OSR.SpatialReference("");
                reference.SetProjCS("UTM " + WUIEngine.RUNTIME_DATA.Simulation.UTMData.Zona + " (WGS84)");
                reference.SetWellKnownGeogCS("WGS84");
                reference.SetUTM(WUIEngine.RUNTIME_DATA.Simulation.UTMData.ZoneNumber, WUIEngine.INPUT.Simulation.LowerLeftLatLon.x > 0 ? 1 : 0); ;
                output.SetSpatialRef(reference);

                OSGeo.GDAL.Band band = output.GetRasterBand(1); //starts from 1, not zero                
                band.SetNoDataValue(-9999f);
                band.SetDescription("Time spent, in seconds, on a road overlaying with the raster.");
                float[] row = new float[xDim];
                for (int y = 0; y < yDim; ++y)
                {
                    for (int x = 0; x < xDim; ++x)
                    {
                        row[x] = _usageMap[x, y];
                        if (row[x] == 0f)
                        {
                            row[x] = -9999f;
                        }
                    }
                    band.WriteRaster(0, y, xDim, 1, row, xDim, 1, 0, 0);
                }
                band.FlushCache();
                output.FlushCache();
                //reminder, output.Close() crashes violently, do not use or investigate further why...
            }
            catch (Exception e)
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, e.Message);
            }
        }

        public override void UpdateEvacuationGoals()
        {
            //throw new System.NotImplementedException();
        }

        List<string>[,] fireCellEdges;
        private void SortEdgesInFireCells()
        {
            if(!WUIEngine.INPUT.Simulation.RunFireModule)
            {
                WUIEngine.LOG(WUIEngine.LogType.Log, "No fire module requested, won't sort SUMO network edges in fire cells.");
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
                    //TODO: include fire module offset here, as now we assume 0,0 is aligned with fire module origin
                    int cellIndexX = (int)((nodePos.x + _originOffset.x) / WUIEngine.SIM.FireModule.GetCellSizeX());
                    int cellIndexY = (int)((nodePos.y + _originOffset.y) / WUIEngine.SIM.FireModule.GetCellSizeY());

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
                WUIEngine.LOG(WUIEngine.LogType.SimError, e.Message);
            }            
        }

        public override void HandleIgnitedFireCells(List<Vector2int> cellIndices)
        {
            for (int i = 0; i < cellIndices.Count; i++)
            {
                FireCellIgnited(cellIndices[i].x, cellIndices[i].y);
            }
        }

        private void FireCellIgnited(int x, int y)
        {
            //since we only want unique cars
            HashSet<SUMOVehicle> carsToUpdate = new HashSet<SUMOVehicle>();

            //make fire affect edges (based on junction)
            if (fireCellEdges[x, y] != null)
            {                
                for (int i = 0; i < fireCellEdges[x, y].Count; i++)
                {
                    //https://sumo.dlr.de/docs/Simulation/Routing.html
                    //after testing this seems to be the best option
                    LIBSUMO.Edge.adaptTraveltime(fireCellEdges[x, y][i], double.MaxValue);

                    //collect cars in system that has the edge in their route
                    foreach (SUMOVehicle car in _vehicles.Values)
                    {
                        LIBSUMO.StringVector route = LIBSUMO.Vehicle.getRoute(car.GetSumoVehicleID());
                        if (route.Contains(fireCellEdges[x, y][i]))
                        {
                            carsToUpdate.Add(car);
                        }                            
                    }
                }

                if(carsToUpdate.Count == 0)
                {
                    WUIEngine.LOG(WUIEngine.LogType.Log, "Cell " + x + "," + y + " has been ignited and affects roads but did not affect any vehicles.");
                }
                else
                {
                    WUIEngine.LOG(WUIEngine.LogType.Log, "Cell " + x + "," + y + " has been ignited and affects roads, notifying vehicles.");
                }

                //then do update for affected cars
                foreach (SUMOVehicle car in carsToUpdate)
                {
                    LIBSUMO.Vehicle.rerouteTraveltime(car.GetSumoVehicleID());
                }
            }              
        }

        public override bool IsNetworkReachable(Vector2d pointLatLon)
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            LIBSUMO.Simulation.close();
        }
    }
}
