using System.Numerics;
using System.Collections.Generic;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;
using Itinero;
using Itinero.Osm.Vehicles;
using WUIPlatform.Population;
using WUIPlatform.IO;

namespace WUIPlatform.Traffic
{
    [System.Serializable]
    public class RouteCreator
    {
        /*private static RouteCreator _instance;
        public static long GetWayId(float latitude, float longitude)
        {
            if (_instance == null)
            {
                _instance = new RouteCreator();
            }
            if(_instance.router == null)
            {
                _instance.router = new Router(WUIEngine.SIM_DATA.GetRouterDb());
            }

            RouterPoint rP = _instance.router.Resolve(_instance.GetRouterProfile(), latitude, longitude, 1f);
            var way_ids = WUIEngine.SIM_DATA.GetRouterDb().EdgeData.Get("way_id");
            return (long)way_ids.GetRaw(rP.EdgeId);
        }*/

        private Router router;
        List<RouterPoint> validEvacuationGoalRouterPoints;
        List<EvacuationGoal> validEvacuationGoals;

        /// <summary>
        /// Hack to allow using a route creator from traffic verification
        /// </summary>
        public void SetValidGoals()
        {
            if (router == null)
            {
                router = new Router(WUIEngine.RUNTIME_DATA.Routing.RouterDb);
            }
            DetermineValidGoalsAndRouterPoints(false);
        }

        /// <summary>
        /// Calculates available routes in all raster cells, updates them instead if old routes are supplied
        /// </summary>
        public RouteCollection[] CalculateCellRoutes()
        {
            WUIEngine.LOG(WUIEngine.LogType.Log, " Calculating route collection for cells, this will take some time...");

            //AbstractMap _map = WUInity.WUInityEngine.MAP;
            //WUInity.INSTANCE.DeleteDrawnRoads();

            Vector2 size = new Vector2((float)WUIEngine.INPUT.Simulation.Size.x, (float)WUIEngine.INPUT.Simulation.Size.y);
            Vector2int cells = WUIEngine.RUNTIME_DATA.Evacuation.CellCount;
            Vector2d[] startPoints;
            startPoints = new Vector2d[cells.x * cells.y];
            // Route analysis: create all waypoints in cells
            for (int y = 0; y < cells.y; ++y)
            {
                for (int x = 0; x < cells.x; ++x)
                {
                    float xPos = size.X * (x + 0.5f) / cells.x;
                    float yPos = size.X * (y + 0.5f) / cells.y;
                    startPoints[x + y * cells.x] = new Vector2d(xPos, yPos);
                }
            }

            if (router == null)
            {
                router = new Router(WUIEngine.RUNTIME_DATA.Routing.RouterDb);
            }

            //initialize some stuff            
            RouteCollection[] cellRoutes = new RouteCollection[cells.x * cells.y];
            Itinero.Profiles.Profile routerProfile = GetRouterProfile();
            float cellSize = WUIEngine.INPUT.Evacuation.RouteCellSize;

            DetermineValidGoalsAndRouterPoints(true);

            int cellsWithGoalsCount = 0;
            for (int i = 0; i < startPoints.Length; i++)
            {
                //check that the cell has actual people, else no need for calculating routes
                int populationInCell = WUIEngine.POPULATION.GetPopulationSimulationSpace(startPoints[i].x, startPoints[i].y);
                if (populationInCell > 0)
                {
                    //Vector2d m = new Vector2d(WUIEngine.RUNTIME_DATA.Simulation.CenterMercator.x, WUIEngine.RUNTIME_DATA.Simulation.CenterMercator.y); 
                    Vector2d start = startPoints[i].GetGeoPosition(WUIEngine.RUNTIME_DATA.Simulation.CenterMercator, 1.0f);

                    //check if valid start was found
                    RouterPoint startRouterPoint = CheckIfStartIsValid(new Vector2d(start.x, start.y), routerProfile, cellSize);

                    //no need in calculating route when start is not resolved
                    if (startRouterPoint == null)
                    {
                        continue;
                    }

                    //check if we have the same start as any neighboring cells, if so just use those calculations as they will will be the same
                    RouteCollection rC = CheckIfNeighborsHaveSameStart(startRouterPoint, i, cellRoutes, cellSize);
                    if (rC != null)
                    {
                        cellRoutes[i] = rC;
                    }
                    else
                    {
                        //list that will contain all valid routes to avoid null ref in route collections
                        List<RouteData> routeData = new List<RouteData>();
                        //loop through all defined goals and save them for potential use later (old way only saved the currently needed route and the re-calced if needed)
                        for (int j = 0; j < validEvacuationGoals.Count; j++)
                        {
                            //TODO: might be cases where exits are blocked intitally but then opens, so disable this for now?
                            /*if(validEvacuationGoals[j].blocked)
                            {
                                continue;
                            }*/

                            RouteData rD = TryCalcRoute(startRouterPoint, validEvacuationGoalRouterPoints[j], validEvacuationGoals[j], routerProfile);
                            if (rD != null)
                            {
                                routeData.Add(rD);
                            }
                        }

                        //check that at least 1 route is not null to make sure we have a valid route to go somewhere
                        if (routeData.Count > 0)
                        {
                            //save actual routes
                            cellRoutes[i] = new RouteCollection(routeData.Count);
                            for (int j = 0; j < routeData.Count; j++)
                            {
                                cellRoutes[i].routes[j] = routeData[j];
                            }

                            //select correct goal out of all the calculated ones
                            SelectCorrectRoute(cellRoutes[i], i);

                            //this never draws duplicates as we continue on the loop (as in skip this part) if we copy route collection
                            if (WUIEngine.INPUT.Visualization.drawRoads)
                            {
                                //WUInity.INSTANCE.DrawRoad(cellRoutes[i], i);
                            }

                            ++cellsWithGoalsCount;
                        }
                    }
                }
            }
            if(cellsWithGoalsCount == 0)
            {
                WUIEngine.SIM.Stop("ERROR: Not a single route was found, make sure OSM network is valid.", true);
            }
            return cellRoutes;
        }

        Itinero.Profiles.Profile GetRouterProfile()
        {
            TrafficInput tO = WUIEngine.INPUT.Traffic;

            Itinero.Profiles.Profile p;

            if (tO.routeChoice == TrafficInput.RouteChoice.Closest || tO.routeChoice == TrafficInput.RouteChoice.EvacGroup)
            {
                p = Vehicle.Car.Shortest();
            }
            else
            {
                p = Vehicle.Car.Fastest();
            }

            return p;
        }

        void DetermineValidGoalsAndRouterPoints(bool logMessages)
        {
            List<EvacuationGoal> evacuatonGoals = WUIEngine.RUNTIME_DATA.Evacuation.EvacuationGoals;
            Itinero.Profiles.Profile routerProfile = GetRouterProfile();

            //check that evac goals are valid
            validEvacuationGoalRouterPoints = new List<RouterPoint>();
            validEvacuationGoals = new List<EvacuationGoal>();
            for (int i = 0; i < evacuatonGoals.Count; i++)
            {
                try
                {
                    //TODO: hard-coded search of 200 meters, setup as option?
                    RouterPoint rP = router.Resolve(routerProfile, (float)evacuatonGoals[i].latLong.x, (float)evacuatonGoals[i].latLong.y, 200f);
                    validEvacuationGoalRouterPoints.Add(rP);
                    validEvacuationGoals.Add(evacuatonGoals[i]);
                    if (logMessages)
                    {
                        WUIEngine.LOG(WUIEngine.LogType.Log, "Evac goal start position valid: " + evacuatonGoals[i].name);
                    }
                }
                catch (Itinero.Exceptions.ResolveFailedException)
                {
                    if (logMessages)
                    {
                        WUIEngine.LOG(WUIEngine.LogType.Warning, "Evac goal start position NOT valid: " + evacuatonGoals[i].name);
                    }
                }
            }
        }

        RouterPoint CheckIfStartIsValid(Vector2d coordinate, Itinero.Profiles.Profile p, float cellSize)
        {
            //check within the radius of the diagonal of the cell (so complete cell plus some parts of neighboring cells)
            RouterPoint start = null;
            try
            {
                if (router == null)
                {
                    router = new Router(WUIEngine.RUNTIME_DATA.Routing.RouterDb);
                }
                start = router.Resolve(p, (float)coordinate.x, (float)coordinate.y, cellSize * 0.70711f); //half cell size * sqrt 2
            }
            catch (Itinero.Exceptions.ResolveFailedException)
            {
                //print("resolve failed");
            }

            return start;
        }

        /// <summary>
        /// See if the wanted route is already calculated (to save computaional time).
        /// </summary>
        /// <param name="startRouterPoint"></param>
        /// <param name="evacGoal"></param>
        /// <param name="currentIndex"></param>
        /// <param name="router"></param>
        /// <param name="rasterRoutes"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        static RouteCollection CheckIfNeighborsHaveSameStart(RouterPoint startRouterPoint, int currentIndex, global::WUIPlatform.RouteCollection[] rasterRoutes, float cellSize)
        {
            //TODO: only check 8 neighbors (or actually all previous neighbors, so 4 neighbors)
            for (int i = 0; i < currentIndex; i++)
            {
                if (rasterRoutes[i] != null)
                {
                    for (int j = 0; j < rasterRoutes[i].routes.Length; j++)
                    {
                        if (rasterRoutes[i].routes[j] != null)
                        {
                            //check if they are approx. the same start coordinates
                            float latDelta = Mathf.Abs(rasterRoutes[i].routes[j].route.Shape[0].Latitude - startRouterPoint.Latitude);
                            float longDelta = Mathf.Abs(rasterRoutes[i].routes[j].route.Shape[0].Longitude - startRouterPoint.Longitude);

                            //https://www.usna.edu/Users/oceano/pguth/md_help/html/approx_equivalents.htm
                            float maxDelta = cellSize * 0.5f; //TODO: reasonable?
                            Vector2d v = LocalGPWData.SizeToDegrees(new Vector2d(startRouterPoint.Latitude, startRouterPoint.Longitude), new Vector2d(maxDelta, maxDelta));

                            if (latDelta < v.x && longDelta < v.y)
                            {
                                return rasterRoutes[i];
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Tries to return valid route data, if not null is returned. Should only be null if there is no physical connection between points (as we check of start and end is valid).
        /// </summary>
        /// <param name="start"></param>
        /// <param name="goal"></param>
        /// <param name="evacGoal"></param>
        /// <param name="routerProfile"></param>
        /// <returns></returns>
        private RouteData TryCalcRoute(RouterPoint start, RouterPoint goal, EvacuationGoal evacGoal, Itinero.Profiles.Profile routerProfile)
        {
            //still calculate for now as a goal might become accessible later in simulation
            /*//if goal is not accessible we have nothing to return
            if (evacGoal.blocked)
            {
                return null;
            }*/

            TrafficInput tO = WUIEngine.INPUT.Traffic;
            RouteData routeData = null;

            try
            {
                if (router == null)
                {
                    router = new Router(WUIEngine.RUNTIME_DATA.Routing.RouterDb);
                }
                Itinero.Route route = router.Calculate(routerProfile, start, goal);
                routeData = new RouteData(route, evacGoal);
            }
            /*catch (Itinero.Exceptions.ResolveFailedException)
            {
                //print("resolve failed");
            }*/
            catch (Itinero.Exceptions.RouteNotFoundException)
            {
                //print("no route found");
            }

            return routeData;
        }

        /// <summary>
        /// Called when a general route to any avialable evac goal is desired, resolves (at least tries) start and end. Startpos in Lat/Long
        /// Used mainly by traffic simulator.
        /// </summary>
        /// <param name="startPos"></param>
        /// <returns></returns>
        public RouteData CalcTrafficRoute(Vector2d startPos)
        {
            float cellSize = WUIEngine.INPUT.Evacuation.RouteCellSize;
            Itinero.Profiles.Profile routerProfile = GetRouterProfile();

            //TODO: reasonable? maybe also check if street is same or actual distance between points?
            //this is a quick way of getting a route from an approximate position of the car
            //we just check if cell we are in has a good route and use that
            RouteCollection rC = WUIEngine.RUNTIME_DATA.Routing.GetCellRouteCollection(startPos);
            if (rC != null && rC.GetSelectedRoute() != null)
            {
                return rC.GetSelectedRoute();
            }

            //check if valid start was found
            RouterPoint startRouterPoint = CheckIfStartIsValid(new Vector2d(startPos.x, startPos.y), routerProfile, cellSize);

            //no need in calculating route when start is not resolved
            if (startRouterPoint == null)
            {
                WUIEngine.SIM.Stop("WARNING! Car could not find a valid start position, abort!", true);
                return null;
            }

            bool foundOneValidRoute = false;

            //list that will contain all valid routes to avoid null ref in route collections
            List<RouteData> routeData = new List<RouteData>();
            //loop through all defined goals and save them for potential use later (old way only saved the currently needed route and the re-calced if needed)
            if (validEvacuationGoals == null || validEvacuationGoalRouterPoints == null)
            {
                DetermineValidGoalsAndRouterPoints(false);
            }
            for (int i = 0; i < validEvacuationGoals.Count; i++)
            {
                //TODO: use this as a quick out? this route should be both closest and fastest
                //This is no longer an issue since any goal that is being blocked is altready flagged
                //(we update new evac goals after full update), just keep going to goal (car does not change route)
                /*if(HasApproxSameCoordinate(startRouterPoint, validEvacuationGoalRouterPoints[i]))
                {
                    //WUInity.WUINITY_SIM.LogMessage("WARNING: Route has same start and end, will get you into trouble, skipping goal!");
                    continue;
                }*/

                //skip blocked goals
                if (validEvacuationGoals[i].blocked)
                {
                    continue;
                }

                RouteData rD = TryCalcRoute(startRouterPoint, validEvacuationGoalRouterPoints[i], validEvacuationGoals[i], routerProfile);

                if (rD != null)
                {
                    routeData.Add(rD);
                    foundOneValidRoute = true;
                }
            }

            //check that at least 1 route is not null to make sure we have a valid route to go somewhere
            if (!foundOneValidRoute)
            {
                //TODO: fix what happens when cars get stuck
                WUIEngine.SIM.Stop("No routes found for car, will get stuck.", true);
                return null;
            }

            rC = new RouteCollection(routeData.Count);
            for (int i = 0; i < routeData.Count; i++)
            {
                rC.routes[i] = routeData[i];
            }

            SelectCorrectRouteFromCar(rC);

            return rC.GetSelectedRoute();
        }

        bool HasApproxSameCoordinate(RouterPoint start, RouterPoint end)
        {
            return Mathd.Approximately(start.Latitude, end.Latitude) && Mathd.Approximately(start.Longitude, end.Longitude);
        }

        private static void SelectCorrectRouteFromCar(RouteCollection rC)
        {
            SelectCorrectRoute(rC, -1);
        }

        public static void UpdateRouteCollectionBasedOnRouteChoice(RouteCollection rC, int cellIndex)
        {
            if(WUIEngine.INPUT.Traffic.routeChoice == TrafficInput.RouteChoice.EvacGroup || WUIEngine.INPUT.Traffic.routeChoice == TrafficInput.RouteChoice.Random)
            {
                SelectCorrectRoute(rC, cellIndex);
            }
        }

        /// <summary>
        /// Picks the desired route froma routecollection based in inputs. 
        /// Should only consider force map when called from a evac cell (not from a car)
        /// </summary>
        /// <param name="rC"></param>
        /// <param name="considerForceMap"></param>
        /// <param name="cellIndex"></param>
        public static void SelectCorrectRoute(RouteCollection rC, int cellIndex)
        {
            TrafficInput tO = WUIEngine.INPUT.Traffic;
            Vector2int cells = WUIEngine.RUNTIME_DATA.Evacuation.CellCount;

            if (tO.routeChoice == TrafficInput.RouteChoice.EvacGroup)
            {
                if (cellIndex >= 0)
                {
                    EvacGroup group = WUIEngine.RUNTIME_DATA.Evacuation.GetEvacGroup(cellIndex);
                    EvacuationGoal goal = group.GetWeightedEvacGoal();
                    rC.SelectForcedNonBlocked(goal);
                }
                else
                {
                    rC.SelectFastestNonBlocked();
                }
            }
            else if (tO.routeChoice == TrafficInput.RouteChoice.Random)
            {
                int randomChoice = Random.Range(0, WUIEngine.RUNTIME_DATA.Evacuation.EvacuationGoals.Count);
                rC.SelectForcedNonBlocked(WUIEngine.RUNTIME_DATA.Evacuation.EvacuationGoals[randomChoice]);
            }
            else if (tO.routeChoice == TrafficInput.RouteChoice.Closest)
            {
                rC.SelectClosestNonBlocked();
            }
            else if (tO.routeChoice == TrafficInput.RouteChoice.Fastest)
            {
                rC.SelectFastestNonBlocked();
            }
        }

        /*public static void SelectCorrectRoute(RouteCollection rC)
        {
            SelectCorrectRoute(rC, false, -1);
        }*/
    }        
}
