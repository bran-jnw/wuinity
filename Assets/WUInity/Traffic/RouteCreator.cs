
using UnityEngine;
using System.Collections.Generic;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;

using Itinero;
using Itinero.Osm.Vehicles;
using WUInity.Population;

namespace WUInity
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
                _instance.router = new Router(WUInity.SIM_DATA.GetRouterDb());
            }

            RouterPoint rP = _instance.router.Resolve(_instance.GetRouterProfile(), latitude, longitude, 1f);
            var way_ids = WUInity.SIM_DATA.GetRouterDb().EdgeData.Get("way_id");
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
                router = new Router(WUInity.RUNTIME_DATA.GetRouterDb());
            }
            DetermineValidGoalsAndRouterPoints(false);
        }

        /// <summary>
        /// Calculates available routes in all raster cells, updates them instead if old routes are supplied
        /// </summary>
        public RouteCollection[] CalculateCellRoutes()
        {
            WUInity.LOG("LOG: Calculating route collection for cells, this will take some time...");

            AbstractMap _map = WUInity.MAP;
            WUInity.INSTANCE.DeleteDrawnRoads();

            Vector2D size = WUInity.INPUT.size;
            Vector2Int cells = WUInity.RUNTIME_DATA.EvacCellCount;
            Vector3[] startPoints;
            startPoints = new Vector3[cells.x * cells.y];
            //create all waypoints in cells
            for (int y = 0; y < cells.y; ++y)
            {
                for (int x = 0; x < cells.x; ++x)
                {
                    float xPos = (float)size.x * ((float)x + 0.5f) / (float)cells.x;
                    float yPos = (float)size.y * ((float)y + 0.5f) / (float)cells.y;
                    startPoints[x + y * cells.x] = new Vector3(xPos, 0.0f, yPos);
                }
            }

            if (router == null)
            {
                router = new Router(WUInity.RUNTIME_DATA.GetRouterDb());
            }

            //initialize some stuff            
            RouteCollection[] cellRoutes = new RouteCollection[cells.x * cells.y];
            Itinero.Profiles.Profile routerProfile = GetRouterProfile();
            float cellSize = WUInity.INPUT.evac.routeCellSize;

            DetermineValidGoalsAndRouterPoints(true);

            int cellsWithGoalsCount = 0;
            for (int i = 0; i < startPoints.Length; i++)
            {
                //check that the cell has actual people, else no need for calculating routes
                int populationInCell = WUInity.POPULATION.GetPopulationUnitySpace(startPoints[i].x, startPoints[i].z);
                if (populationInCell > 0)
                {
                    Vector2d start = startPoints[i].GetGeoPosition(_map.CenterMercator, _map.WorldRelativeScale);

                    //check if valid start was found
                    RouterPoint startRouterPoint = CheckIfStartIsValid(new Vector2D(start.x, start.y), routerProfile, cellSize);

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
                            if (WUInity.INPUT.visuals.drawRoads)
                            {
                                WUInity.INSTANCE.DrawRoad(cellRoutes[i], i);
                            }

                            ++cellsWithGoalsCount;
                        }
                    }
                }
            }
            if(cellsWithGoalsCount == 0)
            {
                WUInity.SIM.StopSim("ERROR: Not a single route was found, make sure OSM network is valid.");
            }
            return cellRoutes;
        }

        Itinero.Profiles.Profile GetRouterProfile()
        {
            TrafficInput tO = WUInity.INPUT.traffic;

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
            EvacuationGoal[] evacuatonGoals = WUInity.RUNTIME_DATA.EvacuationGoals;
            Itinero.Profiles.Profile routerProfile = GetRouterProfile();

            //check that evac goals are valid
            validEvacuationGoalRouterPoints = new List<RouterPoint>();
            validEvacuationGoals = new List<EvacuationGoal>();
            for (int i = 0; i < evacuatonGoals.Length; i++)
            {
                try
                {
                    //TODO: hard-coded search of 200 meters, setup as option?
                    RouterPoint rP = router.Resolve(routerProfile, (float)evacuatonGoals[i].latLong.x, (float)evacuatonGoals[i].latLong.y, 200f);
                    validEvacuationGoalRouterPoints.Add(rP);
                    validEvacuationGoals.Add(evacuatonGoals[i]);
                    if (logMessages)
                    {
                        WUInity.LOG("Evac goal start position valid: " + evacuatonGoals[i].name);
                    }
                }
                catch (Itinero.Exceptions.ResolveFailedException)
                {
                    if (logMessages)
                    {
                        WUInity.LOG("WARNING! Evac goal start position NOT valid: " + evacuatonGoals[i].name);
                    }
                }
            }
        }

        RouterPoint CheckIfStartIsValid(Vector2D latLong, Itinero.Profiles.Profile p, float cellSize)
        {
            //check within the radius of the diagonal of the cell (so complete cell plus some parts of neighboring cells)
            RouterPoint start = null;
            try
            {
                if (router == null)
                {
                    router = new Router(WUInity.RUNTIME_DATA.GetRouterDb());
                }
                start = router.Resolve(p, (float)latLong.x, (float)latLong.y, cellSize * 0.70711f); //half cell size * sqrt 2
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
        static RouteCollection CheckIfNeighborsHaveSameStart(RouterPoint startRouterPoint, int currentIndex, global::WUInity.RouteCollection[] rasterRoutes, float cellSize)
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
                            Vector2D v = LocalGPWData.SizeToDegrees(new Vector2D(startRouterPoint.Latitude, startRouterPoint.Longitude), new Vector2D(maxDelta, maxDelta));

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

            TrafficInput tO = WUInity.INPUT.traffic;
            RouteData routeData = null;

            try
            {
                if (router == null)
                {
                    router = new Router(WUInity.RUNTIME_DATA.GetRouterDb());
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
        public RouteData CalcTrafficRoute(Vector2D startPos)
        {
            float cellSize = WUInity.INPUT.evac.routeCellSize;
            Itinero.Profiles.Profile routerProfile = GetRouterProfile();

            //TODO: reasonable? maybe also check if street is same or actual distance between points?
            //this is a quick way of getting a route from an approximate position of the car
            //we just check if cell we are in has a good route and use that
            RouteCollection rC = WUInity.RUNTIME_DATA.GetCellRouteCollection(startPos);
            if (rC != null && rC.GetSelectedRoute() != null)
            {
                return rC.GetSelectedRoute();
            }

            //check if valid start was found
            RouterPoint startRouterPoint = CheckIfStartIsValid(new Vector2D(startPos.x, startPos.y), routerProfile, cellSize);

            //no need in calculating route when start is not resolved
            if (startRouterPoint == null)
            {
                WUInity.SIM.StopSim("WARNING! Car could not find a valid start position, abort!");
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
                WUInity.SIM.StopSim("STOPPING! No routes found for car, will get stuck.");
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
            return MathD.Approximately(start.Latitude, end.Latitude) && MathD.Approximately(start.Longitude, end.Longitude);
        }

        private static void SelectCorrectRouteFromCar(RouteCollection rC)
        {
            SelectCorrectRoute(rC, -1);
        }

        public static void UpdateRouteCollectionBasedOnRouteChoice(RouteCollection rC, int cellIndex)
        {
            if(WUInity.INPUT.traffic.routeChoice == TrafficInput.RouteChoice.EvacGroup || WUInity.INPUT.traffic.routeChoice == TrafficInput.RouteChoice.Random)
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
            TrafficInput tO = WUInity.INPUT.traffic;
            Vector2Int cells = WUInity.RUNTIME_DATA.EvacCellCount;

            if (tO.routeChoice == TrafficInput.RouteChoice.EvacGroup)
            {
                if (cellIndex >= 0)
                {
                    EvacGroup group = WUInity.RUNTIME_DATA.GetEvacGroup(cellIndex);
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
                int randomChoice = Random.Range(0, WUInity.RUNTIME_DATA.EvacuationGoals.Length);
                rC.SelectForcedNonBlocked(WUInity.RUNTIME_DATA.EvacuationGoals[randomChoice]);
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
