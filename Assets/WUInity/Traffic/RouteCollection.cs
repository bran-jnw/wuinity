namespace WUInity
{
    /// <summary>
    /// Contains route/routes calculated from a location to possibly several goals.
    /// selectedRoute is the current route that was prioritized based on fastest/closest/forced profile.
    /// selectedRoute might also change to fastest when a goal gest blocked (by e.g. fire or just full capacity).
    /// routes array should not contain null references (before they could if a route could not be found at some point despite valid start and end, e.g. no actual roads connecting two valid points)
    /// but list should now be clean (different route collections might not contain all evac goals if they are discarded)
    /// </summary>
    [System.Serializable]
    public class RouteCollection
    {
        public RoutePriority routePriority; 
        public int selectedRouteIndex;
        public RouteData[] routes;

        public RouteCollection(int numberOfRoutes)
        {
            routes = new RouteData[numberOfRoutes];
        }

        public RouteData GetSelectedRoute()
        {
            if(selectedRouteIndex == -1)
            {
                return null;
            }
            else
            {
                return routes[selectedRouteIndex];
            }            
        }

        public void CheckAndUpdateRoute()
        {
            if(selectedRouteIndex == -1)
            {
                WUInity.WUINITY_SIM.StopSim("No route selected (likely due to all routes being blocked), people will get stuck.");
                return;
            }

            //nothing to update
            if(!GetSelectedRoute().evacGoal.blocked)
            {
                return;
            }

            if(routePriority == RoutePriority.Closest)
            {
                SelectClosestNonBlocked();
            }
            else if(routePriority == RoutePriority.Fastest)
            {
                SelectFastestNonBlocked();
            }
            else if (routePriority == RoutePriority.Forced)
            {
                //SelectForcedNonBlocked(selectedRoute.evacGoal);
                //shortcut, we will be thrown here anyway as if we are changing our forced goal has been blocked
                SelectFastestNonBlocked();
            }
        }

        public void SelectClosestNonBlocked()
        {
            routePriority = RoutePriority.Closest;
            selectedRouteIndex = -1;
            if (routes != null)
            {
                for (int i = 0; i < routes.Length; i++)
                {
                    if (!routes[i].evacGoal.blocked)
                    {
                        if (selectedRouteIndex == -1)
                        {
                            selectedRouteIndex = i;
                        }
                        else if (routes[i].route.TotalDistance < GetSelectedRoute().route.TotalDistance)
                        {
                            selectedRouteIndex = i;
                        }
                    }
                }
            }

            if (selectedRouteIndex == -1)
            {
                WUInity.WUINITY_SIM.StopSim("STOPPING: Route selection failed, no routes left that are not blocked");
            }
        }

        public void SelectFastestNonBlocked()
        {
            routePriority = RoutePriority.Fastest;
            selectedRouteIndex = -1;
            if (routes != null)
            {
                for (int i = 0; i < routes.Length; i++)
                {
                    if (!routes[i].evacGoal.blocked)
                    {
                        if (selectedRouteIndex == -1)
                        {
                            selectedRouteIndex = i;
                        }
                        else if (routes[i].route.TotalTime < GetSelectedRoute().route.TotalTime)
                        {
                            selectedRouteIndex = i;
                        }
                    }
                }
            }

            if(selectedRouteIndex == -1)
            {
                WUInity.WUINITY_SIM.StopSim("no routes left to take");
            }
        }

        public void SelectForcedNonBlocked(EvacuationGoal goal)
        {
            routePriority = RoutePriority.Forced;
            selectedRouteIndex = -1;
            if (routes != null)
            {
                for (int i = 0; i < routes.Length; i++)
                {
                    if (!routes[i].evacGoal.blocked)
                    {
                        if (routes[i].evacGoal == goal)
                        {
                            selectedRouteIndex = i;
                            break;
                        }
                    }
                }
            }

            //if the desired evac goal is not present, select fastest
            if (selectedRouteIndex == -1)
            {
                SelectFastestNonBlocked();
            }
        }
    }
}
