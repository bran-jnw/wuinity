namespace WUInity
{
    [System.Serializable]
    public class RouteDataSave
    {
        //public string name;
        public ItineroRouteWrapper route;
        public string evacGoal;

        public RouteDataSave(RouteData rD)
        {
            //name = rD.name;
            route = new ItineroRouteWrapper(rD.route);
            evacGoal = rD.evacGoal.name;
        }

        public RouteData Convert()
        {
            Itinero.Route r = route.Convert();
            EvacuationGoal eG = GetRealEvacGoal(evacGoal);
            RouteData rD = new RouteData(r, eG);

            return rD;
        }

        public static EvacuationGoal GetRealEvacGoal(string name)
        {
            EvacuationGoal result = null;
            for (int i = 0; i < WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals.Count; i++)
            {
                if (WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals[i].name == name)
                {
                    result = WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals[i];
                }
            }

            if(result == null)
            {
                WUInity.LOG(WUInity.LogType.Warning, " While loading the route collection the evacuation goal named " + name + " did not match any of the specified evacuation goals, the route collection is therefore not valid.");
            }

            return result;
        }
    }

    [System.Serializable]
    public class RouteCollectionSave
    {
        public RoutePriority routePriority;
        public int selectedRouteIndex;
        public RouteDataSave[] routes;

        public RouteCollectionSave(RouteCollection rC)
        {
            routePriority = rC.routePriority;
            selectedRouteIndex = rC.selectedRouteIndex;
            routes = new RouteDataSave[rC.routes.Length];
            for (int i = 0; i < rC.routes.Length; i++)
            {
                routes[i] = new RouteDataSave(rC.routes[i]);
            }
        }

        public RouteCollection Convert()
        {
            RouteCollection rC = new RouteCollection(routes.Length);
            rC.routePriority = routePriority;
            rC.selectedRouteIndex = selectedRouteIndex;
            for (int i = 0; i < rC.routes.Length; i++)
            {
                rC.routes[i] = routes[i].Convert();

                if(rC.routes[i].evacGoal == null)
                {
                    rC = null;
                    break;
                }
            }

            return rC;
        }
    }    

    [System.Serializable]
    public class RouteCollectionWrapper
    {
        public int originalArraySize;
        public int realDataCount;
        public int[] indicesToPutDataBack;
        public RouteCollectionSave[] collection;        

        public RouteCollectionWrapper(RouteCollection[] rCs)
        {
            originalArraySize = rCs.Length;
            for (int i = 0; i < rCs.Length; i++)
            {
                if(rCs[i] != null)
                {                    
                    ++realDataCount;
                }                
            }

            collection = new RouteCollectionSave[realDataCount];
            indicesToPutDataBack = new int[realDataCount];
            int currentIndex = 0;
            for (int i = 0; i < rCs.Length; i++)
            {
                if (rCs[i] != null)
                {
                    collection[currentIndex] = new RouteCollectionSave(rCs[i]);
                    indicesToPutDataBack[currentIndex] = i;
                    ++currentIndex;
                }
            }
        }

        public RouteCollection[] Convert()
        {
            RouteCollection[] rC = new RouteCollection[originalArraySize];
            int currentIndex = 0;
            for (int i = 0; i < rC.Length; i++)
            {
                if(i == indicesToPutDataBack[currentIndex])
                {
                    rC[i] = collection[currentIndex].Convert();
                    ++currentIndex;
                    if(currentIndex == realDataCount)
                    {
                        break;
                    }
                }
            }
            return rC;
        }
    }

    /// <summary>
    /// Wrapper class to enable saves of routes
    /// </summary>
    [System.Serializable]
    public class ItineroRouteWrapper
    {
        public Coordinate[] Shape;
        public AttributeCollection Attributes;
        public Stop[] Stops;
        public Meta[] ShapeMeta;
        public Branch[] Branches;
        public float TotalDistance;
        public float TotalTime;
        public string Profile;

        public ItineroRouteWrapper(Itinero.Route route)
        {
            Shape = new Coordinate[route.Shape.Length];
            for (int i = 0; i < route.Shape.Length; i++)
            {
                Shape[i] = new Coordinate(route.Shape[i]);
            }

            Attributes = new AttributeCollection(route.Attributes);

            Stops = new Stop[route.Stops.Length];
            for (int i = 0; i < route.Stops.Length; i++)
            {
                Stops[i] = new Stop(route.Stops[i]);
            }

            ShapeMeta = new Meta[route.ShapeMeta.Length];
            for (int i = 0; i < route.ShapeMeta.Length; i++)
            {
                ShapeMeta[i] = new Meta(route.ShapeMeta[i]);
            }

            Branches = new Branch[route.Branches.Length];
            for (int i = 0; i < route.Branches.Length; i++)
            {
                Branches[i] = new Branch(route.Branches[i]);
            }

            TotalDistance = route.TotalDistance;
            TotalTime = route.TotalTime;
            Profile = route.Profile;
        }

        public Itinero.Route Convert()
        {
            Itinero.Route route = new Itinero.Route();

            route.Shape = new Itinero.LocalGeo.Coordinate[Shape.Length];
            for (int i = 0; i < Shape.Length; i++)
            {
                route.Shape[i] = Shape[i].Convert();
            }

            route.Attributes = Attributes.Convert();

            route.Stops = new Itinero.Route.Stop[Stops.Length];
            for (int i = 0; i < Stops.Length; i++)
            {
                route.Stops[i] = Stops[i].Convert();
            }


            route.ShapeMeta = new Itinero.Route.Meta[ShapeMeta.Length];
            for (int i = 0; i < ShapeMeta.Length; i++)
            {
                route.ShapeMeta[i] = ShapeMeta[i].Convert();
            }


            route.Branches = new Itinero.Route.Branch[Branches.Length];
            for (int i = 0; i < Branches.Length; i++)
            {
                route.Branches[i] = Branches[i].Convert();
            }

            route.TotalDistance = TotalDistance;
            route.TotalTime = TotalTime;
            route.Profile = Profile;

            return route;
        }

        [System.Serializable]
        public struct Coordinate
        {
            public float Latitude;
            public float Longitude;
            public short? Elevation;

            public Coordinate(Itinero.LocalGeo.Coordinate coordinate)
            {
                Latitude = coordinate.Latitude;
                Longitude = coordinate.Longitude;
                Elevation = coordinate.Elevation;
            }       
            
            public Itinero.LocalGeo.Coordinate Convert()
            {                
                if(Elevation.HasValue)
                {
                    Itinero.LocalGeo.Coordinate coordinate = new Itinero.LocalGeo.Coordinate(Latitude, Longitude, (short)Elevation);
                    return coordinate;
                }
                else
                {
                    Itinero.LocalGeo.Coordinate coordinate = new Itinero.LocalGeo.Coordinate(Latitude, Longitude);
                    return coordinate;
                }    
            }
        }        

        [System.Serializable]
        public class Stop
        {
            public int Shape;
            public Coordinate Coordinate;
            public AttributeCollection Attributes;
            public float Distance;
            public float Time;

            public Stop(Itinero.Route.Stop stop)
            {
                Shape = stop.Shape;
                Coordinate = new Coordinate(stop.Coordinate);
                Attributes = new AttributeCollection(stop.Attributes);
                Distance = stop.Distance;
                Time = stop.Time;
            }     
            
            public Itinero.Route.Stop Convert()
            {
                Itinero.Route.Stop stop = new Itinero.Route.Stop();
                stop.Shape = Shape;
                stop.Coordinate = Coordinate.Convert();
                stop.Attributes = Attributes.Convert();
                stop.Distance = Distance;
                stop.Time = Time;

                return stop;
            }
        }

        [System.Serializable]
        public class Meta
        {
            public int Shape;
            public AttributeCollection Attributes;
            public bool AttributesDirection;
            public string Profile;
            public float Distance;
            public float Time;

            public Meta(Itinero.Route.Meta meta)
            {
                Shape = meta.Shape;
                Attributes = new AttributeCollection(meta.Attributes);
                AttributesDirection = meta.AttributesDirection;
                Profile = meta.Profile;
                Distance = meta.Distance;
                Time = meta.Time;
            }      
            
            public Itinero.Route.Meta Convert()
            {
                Itinero.Route.Meta meta = new Itinero.Route.Meta();
                meta.Shape = Shape;
                meta.Attributes = Attributes.Convert();
                meta.AttributesDirection = AttributesDirection;
                meta.Profile = Profile;
                meta.Distance = Distance;
                meta.Time = Time;

                return meta;
            }
        }

        [System.Serializable]
        public class Branch
        {
            public int Shape;
            public Coordinate Coordinate;
            public AttributeCollection Attributes;
            public bool AttributesDirection;

            public Branch(Itinero.Route.Branch branch)
            {
                Shape = branch.Shape;
                Coordinate = new Coordinate(branch.Coordinate);
                Attributes = new AttributeCollection(branch.Attributes);
                AttributesDirection = branch.AttributesDirection;
            }       
            
            public Itinero.Route.Branch Convert()
            {
                Itinero.Route.Branch branch = new Itinero.Route.Branch();
                branch.Coordinate = Coordinate.Convert();
                branch.Attributes = Attributes.Convert();
                branch.AttributesDirection = AttributesDirection;

                return branch;
            }
        }

        [System.Serializable]
        public class AttributeCollection
        {
            public bool IsReadOnly;
            public Attribute[] Attributes;

            public AttributeCollection(Itinero.Attributes.IAttributeCollection attributeCollection)
            {
                IsReadOnly = attributeCollection.IsReadonly;

                Attributes = new Attribute[attributeCollection.Count];
                int index = 0;
                foreach (Itinero.Attributes.Attribute a in attributeCollection)
                {
                    Attributes[index] = new Attribute(a);
                    ++index;
                }
            }

            public Itinero.Attributes.AttributeCollection Convert()
            {
                Itinero.Attributes.AttributeCollection attributes = new Itinero.Attributes.AttributeCollection();

                if(Attributes != null)
                {
                    for (int i = 0; i < Attributes.Length; i++)
                    {
                        attributes.AddOrReplace(Attributes[i].Key, Attributes[i].Value);
                    }
                }                

                return attributes;
            }
        }

        [System.Serializable]
        public class Attribute
        {
            public string Key;
            public string Value;

            public Attribute(Itinero.Attributes.Attribute attribute)
            {
                Key = attribute.Key;
                Value = attribute.Value;
            }
        }
    }
}