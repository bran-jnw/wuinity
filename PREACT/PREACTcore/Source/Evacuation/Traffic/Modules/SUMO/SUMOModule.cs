//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using PREACT.Evacuation;
using LIBSUMO = Eclipse.Sumo.Libsumo;
using PREACT.Math;

namespace PREACT.Traffic
{
    public class SUMOModule : TrafficModule
    {
        private Dictionary<string, SUMOVehicle> _sumoVehicles;        
        List<LIBSUMO.TraCIRoadPosition> _validStartPositions;

        //output
        private uint totalVehiclesArrived, totalPeopleArrived, totalSumoVehiclesArrived;
        private int currentVehiclessInSystem;
        private int totalVehiclesInjected, totalSumoVehiclesInjected;
        private List<string> output;

        double _maxUsage;
        private double[,] _usageMap;
        private uint[,] _carCount;
        private float[,] _accumulatedLevelOfService;
        private float[,] _accumulatedWatingTime;
        private bool _checkSmoke = false;

        private SumoConfig _sumoConfig;

        public SUMOModule(Simulation simulation, out bool success) : base(simulation)
        {
            success = true;
            try
            {
                _sumoVehicles = new Dictionary<string, SUMOVehicle>();
                string configFile = Path.Combine(_simulation.Engine.WorkingFolder, _simulation.Input.TrafficModule.SumoInput.ConfigurationFile);
                //see here for options https://sumo.dlr.de/docs/sumo.html, setting input file, start and end time
                LIBSUMO.Simulation.start(new LIBSUMO.StringVector(new String[] { "sumo", "-c", configFile, "-b", "0.0", "-e", _simulation.Time.SimulationEndTime.ToString() }));

                //check if destinations are valid, if not abort
                ValidateDestinations(simulation.Evacuation.Destinations, out bool allValid);
                if(!allValid)
                {
                    success = false;
                    return;
                }

                _sumoConfig = new SumoConfig(configFile, _simulation.Input.WildfireModule.Enabled);

                //need to use UTM projection in SUMO and WUInity to overlay data
                Vector2d sumoUTM = -_sumoConfig.Network.UTMOffset;// new Vector2d(-_simulation.Input.TrafficModule.SumoInput.UTMoffset.x, -_simulation.Input.TrafficModule.SumoInput.UTMoffset.y);
                _originOffset = sumoUTM - _simulation.Spatial.UTMOrigin;

                _validStartPositions = new List<LIBSUMO.TraCIRoadPosition>();

                output = new List<string>();
                string header = "Time(s),Total cars injected, Total cars arrived,Current cars in system,Exiting people,Total Sumo cars injected,Total Sumo cars arrived";
                for (int i = 0; i < _simulation.Evacuation.Destinations.Count; ++i)
                {
                    header += "," + _simulation.Evacuation.Destinations[i].Name + " people arrived";
                    header += "," + _simulation.Evacuation.Destinations[i].Name + " cars arrived";
                    header += "," + _simulation.Evacuation.Destinations[i].Name + " flow [veh./h]";
                }
                output.Add(header);

                int xDim = Mathd.CeilToInt(_simulation.Input.Simulation.DomainSize.x / _simulation.Input.TrafficModule.SumoInput.OutputRasterSize);
                int yDim = Mathd.CeilToInt(_simulation.Input.Simulation.DomainSize.y / _simulation.Input.TrafficModule.SumoInput.OutputRasterSize);

                _maxUsage = 0f;
                _usageMap = new double[xDim, yDim];
                _carCount = new uint[xDim, yDim];
                _accumulatedLevelOfService = new float[xDim, yDim];
                _accumulatedWatingTime = new float[xDim, yDim];

                if(_simulation.Input.SmokeModule.Enabled && (simulation.Input.TrafficModule.SumoInput.SmokeAlpha != 0f || _simulation.Input.TrafficModule.SumoInput.SmokeBeta != 0f))
                {
                    _checkSmoke = true;
                }

                SortEdgesInFireCells();
            }
            catch(Exception e)
            {
                success = false;
                Engine.Message(_simulation, Engine.LogType.SimulationError, "Could not start SUMO, aborting. " + e.Message + ". " + e.InnerException);
            }            
        }

        private void ValidateDestinations(List<EvacuationDestination> destinations, out bool allValid)
        {
            allValid = true;

            foreach (EvacuationDestination eD in destinations)
            {
                try
                {
                    //IMPORTANT!!! Longitude then latitude in SUMO
                    LIBSUMO.TraCIRoadPosition road = LIBSUMO.Simulation.convertRoad(eD.LatLon.y, eD.LatLon.x, true);
                }
                catch (Exception e)
                {
                    allValid = false;
                    Engine.Message(_simulation, Engine.LogType.SimulationError, $"Destination {eD.Name} returns error from SUMO: {e.Message}");
                }
                
            }
        }

        //might crash SUMO when running a new instance of SUMOModule while the old one is garbage collected
        /*~SUMOModule()
        {
            LIBSUMO.Simulation.close();
        }*/

        float GetSmokeSpeedReductionFactor(Vector2d pos)
        {
            float extCoeff = _simulation.Hazards.GetExtinctionCoefficientAtPos(pos);
            return 1f - _simulation.Input.TrafficModule.SumoInput.SmokeAlpha * Mathf.Exp(_simulation.Input.TrafficModule.SumoInput.SmokeBeta / extCoeff);
        }

        public override void Step(double currentTime, double deltaTime)
        {
            if(_checkSmoke)
            {
                foreach (KeyValuePair<string, SUMOVehicle> sV in _sumoVehicles)
                {
                    SUMOVehicle vehicle = sV.Value;
                    double speedFactor = vehicle.InitialSpeedFactor * GetSmokeSpeedReductionFactor(vehicle.SimulationPos);
                    LIBSUMO.Vehicle.setSpeedFactor(vehicle.GetSumoVehicleID(), speedFactor);
                }
            }           

            //https://sumo.dlr.de/doxygen/d0/d17/classlibsumo_1_1_simulation.html
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
                    _sumoVehicles.TryGetValue(sumoID, out vehicle);
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
                        vehicle = new SUMOVehicle(GetNewCarID(), sumoID, LIBSUMO.Vehicle.getPosition(sumoID), LIBSUMO.Vehicle.getAngle(sumoID), 0, null, LIBSUMO.Vehicle.getSpeedFactor(sumoID));
                        _sumoVehicles.Add(sumoID, vehicle);
                        _activeVehicles.Add(vehicle.VehicleId, vehicle);
                        ++totalSumoVehiclesInjected;
                    }

                    UpdateOutputMaps(vehicle, deltaTime);
                }
            }     

            //check if any cars have arrived
            if (LIBSUMO.Simulation.getArrivedNumber() > 0)
            {
                LIBSUMO.StringVector arrivedVehicles = LIBSUMO.Simulation.getArrivedIDList();
                for (int i = 0; i < arrivedVehicles.Count; i++)
                {
                    SUMOVehicle vehicle;
                    _sumoVehicles.TryGetValue(arrivedVehicles[i], out vehicle);
                    if(vehicle != null)
                    {
                        vehicle.Arrive(deltaTime, currentTime);
                    }
                    _sumoVehicles.Remove(arrivedVehicles[i]);
                    _activeVehicles.Remove(vehicle.VehicleId);
                    //if car is internal to SUMO they have 0 passengers from the point of view of the simulation
                    if (vehicle.NumberOfPeople > 0)
                    {
                        _arrivalData.Add(currentTime + deltaTime);
                        totalVehiclesArrived++;
                        totalPeopleArrived += vehicle.NumberOfPeople;
                    }
                    else
                    {
                        ++totalSumoVehiclesArrived;
                    }
                }
            }

            //Time(s),Total cars injected, Total cars arrived,Current cars in system, Exiting people
            string dataLine = currentTime + "," + totalVehiclesInjected + "," + totalVehiclesArrived + "," + currentVehiclessInSystem + "," + totalPeopleArrived + "," + totalSumoVehiclesInjected + "," + totalSumoVehiclesArrived;
            for (int i = 0; i < _simulation.Evacuation.Destinations.Count; ++i)
            {
                dataLine += "," + _simulation.Evacuation.Destinations[i].CurrentPeople;
                dataLine += "," + _simulation.Evacuation.Destinations[i].Vehicles.Count;
                dataLine += "," + _simulation.Evacuation.Destinations[i].CurrentVehicleFlow;
            }
            output.Add(dataLine);
        }

        private void UpdateOutputMaps(SUMOVehicle vehicle, double deltaTime)
        {
            Vector2d pos = vehicle.SimulationPos;

            int xIndex = (int)(_usageMap.GetLength(0) * pos.x / _simulation.Input.Simulation.DomainSize.x);
            int yIndex = (int)(_usageMap.GetLength(1) * pos.y / _simulation.Input.Simulation.DomainSize.y);

            //we can be outside as sometimes roads reach beyond simulation domain
            if (xIndex >= 0 && xIndex < _usageMap.GetLength(0) && yIndex >= 0 && yIndex < _usageMap.GetLength(1))
            {
                _usageMap[xIndex, yIndex] += deltaTime;
                if (_usageMap[xIndex, yIndex] > _maxUsage)
                {
                    _maxUsage = _usageMap[xIndex, yIndex];
                }

                _accumulatedLevelOfService[xIndex, yIndex] += vehicle.SpeedRatio;
                _accumulatedWatingTime[xIndex, yIndex] += (float)LIBSUMO.Vehicle.getWaitingTime(vehicle.GetSumoVehicleID());
                _carCount[xIndex, yIndex] += 1;
            }
        }

        public double[,] GetUsageMap()
        {
            return _usageMap;
        }

        public double GetMaxUsage()
        {
            return _maxUsage;
        }

        public override void HandleNewCars()
        {
            foreach (InjectedCar injectedCar in _carsToInject)
            {
                EvacuationDestination evacuationGoal = injectedCar.evacuationDestination;
                uint numberOfPeopleInCar = injectedCar.numberOfPeopleInCar;
                Vector2d startLatLon = injectedCar.startLatLon;                
                Vector2d destinationLatLon = evacuationGoal.LatLon;

                //TODO: create input for this...
                string vehicleType = "evacuation_car";

                try
                {
                    //IMPORTANT!!! Longitude then latitude in SUMO
                    LIBSUMO.TraCIRoadPosition startRoad = LIBSUMO.Simulation.convertRoad(startLatLon.y, startLatLon.x, true); //lon/lat
                    LIBSUMO.TraCIRoadPosition destinationRoad = LIBSUMO.Simulation.convertRoad(destinationLatLon.y, destinationLatLon.x, true); //lon/lat
                    LIBSUMO.TraCIStage route;
                    //TODO:do we find route based on empty network/pure speed limits or do we take into account current state of network
                    if(true)
                    {
                        route = LIBSUMO.Simulation.findRoute(startRoad.edgeID, destinationRoad.edgeID);
                    }
                    else
                    {
                        route = LIBSUMO.Simulation.findRoute(startRoad.edgeID, destinationRoad.edgeID, "", -1, 1);
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
                        int randomStart = Math.Random.Range(0, _validStartPositions.Count);   
                        //TODO: actually save start/goal pairs as we might try to generate route from a random start position to a non-reachable current goal of the car
                        route = LIBSUMO.Simulation.findRoute(_validStartPositions[randomStart].edgeID, destinationRoad.edgeID);    
                        if(route.edges.Count > 0)
                        {
                            foundRoute = true;
                            Engine.Message(null, Engine.LogType.Warning, $"No route could be found for the injected car, so it was teleported to a valid location. Origin (lat/lon) {startLatLon.x}, {startLatLon.y}, dest. (lat/lon) {destinationLatLon.x}, {destinationLatLon.y}.");
                        }
                        else
                        {
                            Engine.Message(null, Engine.LogType.Warning, $"No route could be found for the injected car, tried teleporting but no valid route could be found. Origin (lat/lon) {startLatLon.x}, {startLatLon.y}, dest. (lat/lon) {destinationLatLon.x}, {destinationLatLon.y}.");
                        }
                    }
                    else
                    {
                        Engine.Message(null, Engine.LogType.Warning, $"Car could not be injected as no valid route was found or cached. Origin (lat/lon) {startLatLon.x}, {startLatLon.y}, dest. (lat/lon) {destinationLatLon.x}, {destinationLatLon.y}.");
                    }

                    if(foundRoute)
                    {
                        uint carID = GetNewCarID();
                        string sumoID = carID.ToString();
                        string routeID = "preact_route_" + carID;
                        LIBSUMO.Route.add(routeID, route.edges);
                        LIBSUMO.Vehicle.add(sumoID, routeID);//, vehicleType);
                        LIBSUMO.TraCIPosition startPos = LIBSUMO.Vehicle.getPosition(sumoID);
                        SUMOVehicle car = new SUMOVehicle(carID, sumoID, startPos, 0, numberOfPeopleInCar, evacuationGoal, LIBSUMO.Vehicle.getSpeedFactor(sumoID));
                        _sumoVehicles.Add(sumoID, car);
                        _activeVehicles.Add(car.VehicleId, car);
                        ++totalVehiclesInjected;
                    }
                }
                catch (Exception e)
                {
                    Engine.Message(null, Engine.LogType.Warning, "Issue injecting vehicle into SUMO: " + e.Message);
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

        public override void SaveToFile(int simulationIndex)
        {
            string filePath;
            //arrival data to csv
            try
            {
                filePath = Path.Combine(_simulation.Engine.OutputFolder, _simulation.Input.Simulation.Name + "_traffic_output_" + simulationIndex + ".csv");
                File.WriteAllLines(filePath, output);
            }
            catch(Exception e)
            {
                Engine.Message(null, Engine.LogType.Warning, e.Message);
            }

            filePath = Path.Combine(_simulation.Engine.OutputFolder, _simulation.Input.Simulation.Name + "_trafficData_" + simulationIndex + ".tiff");
            SaveOutputMaps(filePath);
        }

        private void SaveOutputMaps(string filePath)
        {
            //usage map as geotiff
            try
            {
                int xDim = _usageMap.GetLength(0);
                int yDim = _usageMap.GetLength(1);

                using (OSGeo.GDAL.Driver driver = OSGeo.GDAL.Gdal.GetDriverByName("GTiff"))
                {
                    OSGeo.GDAL.Dataset output = driver.Create(filePath, xDim, yDim, 3, OSGeo.GDAL.DataType.GDT_Float32, null);

                    double leftX = _simulation.Spatial.UTMOrigin.x;
                    double lowerLeftY = _simulation.Spatial.UTMOrigin.y;
                    double[] geoTransform = new double[] { leftX, _simulation.Input.TrafficModule.SumoInput.OutputRasterSize, 0.0, lowerLeftY, 0.0, _simulation.Input.TrafficModule.SumoInput.OutputRasterSize };
                    output.SetGeoTransform(geoTransform);

                    OSGeo.OSR.SpatialReference reference = new OSGeo.OSR.SpatialReference("");
                    reference.SetProjCS("UTM " + _simulation.Spatial.UTMData.Zona + " (WGS84)");
                    reference.SetWellKnownGeogCS("WGS84");
                    reference.SetUTM(_simulation.Spatial.UTMData.ZoneNumber, _simulation.Input.Simulation.LowerLeftLatLon.x > 0 ? 1 : 0); ;
                    output.SetSpatialRef(reference);

                    //heat map
                    OSGeo.GDAL.Band band = output.GetRasterBand(1); //starts from 1, not zero                
                    band.SetNoDataValue(-9999f);
                    band.SetDescription("Heat map [s], accumulated time spent on a roads overlaying with the raster.");
                    double[] row = new double[xDim];
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

                    // average level of service
                    band = output.GetRasterBand(2);
                    band.SetNoDataValue(-9999f);
                    band.SetDescription("Average level of service (ratio of actual speed and speed limit.)");
                    for (int y = 0; y < yDim; ++y)
                    {
                        for (int x = 0; x < xDim; ++x)
                        {
                            row[x] = _accumulatedLevelOfService[x, y] / _carCount[x, y];
                            if (row[x] == 0f)
                            {
                                row[x] = -9999f;
                            }
                        }
                        band.WriteRaster(0, y, xDim, 1, row, xDim, 1, 0, 0);
                    }
                    band.FlushCache();

                    // average waiting time
                    band = output.GetRasterBand(3);
                    band.SetNoDataValue(-9999f);
                    band.SetDescription("Average waiting time [s].");
                    for (int y = 0; y < yDim; ++y)
                    {
                        for (int x = 0; x < xDim; ++x)
                        {
                            row[x] = _accumulatedWatingTime[x, y] / _carCount[x, y];
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

            }
            catch (Exception e)
            {
                Engine.Message(null, Engine.LogType.Warning, e.Message);
            }
        }

        public override void UpdateEvacuationGoals()
        {
            //throw new System.NotImplementedException();
        }

        //List<string>[,] fireCellEdges;
        Dictionary<CellIndex, HashSet<SumoEdge>> _cellsWithEdges;
        private void SortEdgesInFireCells()
        {
            if(!_simulation.Input.WildfireModule.Enabled)
            {
                Engine.Message(null, Engine.LogType.Log, "No wildfire module requested, won't sort SUMO network edges in fire cells.");
                return;
            }

            try
            {
                Vector2d wildfireOrigin = _simulation.Hazards.Wildfire.GetOriginOffset();
                double minXPos = wildfireOrigin.x - _originOffset.x; // now in sumo space
                double minYPos = wildfireOrigin.y - _originOffset.y;
                double cellSizeX = _simulation.Hazards.Wildfire.GetCellSizeX();
                double cellSizeY = _simulation.Hazards.Wildfire.GetCellSizeY();

                _cellsWithEdges = EdgeCellIntersection.SortEdgesIntoCells(_sumoConfig.Network.Edges, minXPos, minYPos, cellSizeX, cellSizeY, _simulation.Hazards.Wildfire.GetCellCountX(), _simulation.Hazards.Wildfire.GetCellCountX());
                Engine.Message(null, Engine.LogType.Log, "Number of fire cells that have road junctions and will affect traffic:" + _cellsWithEdges.Count);
            }
            catch (Exception e) 
            {
                Engine.Message(null, Engine.LogType.SimulationError, e.Message);
            }            
        }

        

        public override void HandleIgnitedFireCells(List<Vector2int> cellIndices)
        {
            for (int i = 0; i < cellIndices.Count; i++)
            {
                FireCellIgnited(cellIndices[i].x, cellIndices[i].y);
            }
        }

        HashSet<SUMOVehicle> _carsToUpdate = new HashSet<SUMOVehicle>(100);
        private void FireCellIgnited(int x, int y)
        {
            _carsToUpdate.Clear();
            CellIndex ci = new CellIndex(x, y);

            if (_cellsWithEdges.TryGetValue(ci, out HashSet<SumoEdge> edges))
            {
                foreach(SumoEdge edge in edges)
                {
                    //https://sumo.dlr.de/docs/Simulation/Routing.html
                    //after testing this seems to be the best option
                    LIBSUMO.Edge.adaptTraveltime(edge.Id, double.MaxValue);

                    //collect cars in system that has the edge in their route
                    foreach (SUMOVehicle car in _sumoVehicles.Values)
                    {
                        LIBSUMO.StringVector route = LIBSUMO.Vehicle.getRoute(car.GetSumoVehicleID());
                        if (route.Contains(edge.Id))
                        {
                            _carsToUpdate.Add(car);
                        }
                    }
                }
            }

            if (_carsToUpdate.Count == 0)
            {
                Engine.Message(_simulation, Engine.LogType.Log, "Cell " + x + "," + y + " has been ignited and affects roads but did not affect any vehicles.");
            }
            else
            {
                Engine.Message(_simulation, Engine.LogType.Log, "Cell " + x + "," + y + " has been ignited and affects roads, notifying vehicles.");
            }

            //then do update for affected cars
            foreach (SUMOVehicle car in _carsToUpdate)
            {
                LIBSUMO.Vehicle.rerouteTraveltime(car.GetSumoVehicleID());
            }
        }    

        public override bool IsNetworkReachable(Vector2d pointLatLon)
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            try
            {
                LIBSUMO.Simulation.close();
            }
            catch (Exception e)
            {
                Engine.Message(_simulation, Engine.LogType.Log, "Could not stop SUMO. " + e.Message + ". " + e.InnerException);
            }
        }

        public override void SetManualDestination(List<TrafficModuleVehicle> vehicles, Vector2d simulationPos, EvacuationDestination evacuationDestination)
        {
            for (int i = 0; i < vehicles.Count; ++i)
            {
                try
                {
                    //IMPORTANT!!! Longitude then latitude in SUMO
                    Vector2d sumoPos = simulationPos - _originOffset;
                    LIBSUMO.TraCIRoadPosition destinationEdge = LIBSUMO.Simulation.convertRoad(sumoPos.x, sumoPos.y);
                    LIBSUMO.Vehicle.changeTarget(((SUMOVehicle)vehicles[i]).VehicleId.ToString(), destinationEdge.edgeID);
                    vehicles[i].UpdateDestination(evacuationDestination);
                }
                catch (Exception e)
                {
                    Engine.Message(null, Engine.LogType.Warning, e.Message);
                }
            }
        }
    }
}
