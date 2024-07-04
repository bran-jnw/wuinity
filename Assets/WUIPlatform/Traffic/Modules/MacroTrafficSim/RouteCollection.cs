//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace WUIPlatform
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
                WUIEngine.SIM.Stop("No route selected (likely due to all routes being blocked), people will get stuck.", true);
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
            int oldSelectedRouteIndex = selectedRouteIndex;
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
                WUIEngine.SIM.Stop("STOP: Route selection failed, no routes left that are not blocked", true);
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
                WUIEngine.SIM.Stop("No routes left to take", true);
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
