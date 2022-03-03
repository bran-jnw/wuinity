using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity.Traffic
{
    public class MacroCar
    {
        public RouteData routeData;
        public int numberOfPeopleInCar;
        public int currentShapeIndex;
        public float currentSpeedLimit;
        public float currentDistanceLeft;        
        public float currentShapeLength;
        public string drivingOnStreet;
        public Itinero.LocalGeo.Coordinate goingToCoord;
        public float totalTravelDistance;              
        public float totalDrivingTime;
        public bool hasArrived;

        public int densityHash;

        private Vector2 unityCurrentStartPosition;
        private Vector2 unityCurrentGoalPosition;        
        private float latestSpeed;

        public MacroCar(RouteData desiredRoute, int numberOfPeopleInCar)
        {
            routeData = desiredRoute;
            this.numberOfPeopleInCar = numberOfPeopleInCar;
            //go directly to shape 1 since shape 0 is just meta data telling that we are a car
            currentShapeIndex = 1;
            currentDistanceLeft = routeData.route.ShapeMeta[currentShapeIndex].Distance;
            currentShapeLength = currentDistanceLeft;
            currentSpeedLimit = GetCurrentSpeedLimit();
            hasArrived = false;
            totalTravelDistance = 0.0f;

            int sI = routeData.route.ShapeMeta[currentShapeIndex].Shape;
            goingToCoord = routeData.route.Shape[sI];

            routeData.route.ShapeMeta[currentShapeIndex].Attributes.TryGetValue("name", out drivingOnStreet);
            densityHash = CalcHashCode();

            totalDrivingTime = 0f;

            //save start coordinate in Unity space
            Mapbox.Utils.Vector2d pos = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(routeData.route.Shape[0].Latitude, routeData.route.Shape[0].Longitude, WUInity.MAP.CenterMercator, WUInity.MAP.WorldRelativeScale);
            unityCurrentStartPosition = new Vector2((float)pos.x, (float)pos.y);
            //save new goal coordinate in Unity space
            pos = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(goingToCoord.Latitude, goingToCoord.Longitude, WUInity.MAP.CenterMercator, WUInity.MAP.WorldRelativeScale);
            unityCurrentGoalPosition = new Vector2((float)pos.x, (float)pos.y);
        }

        private int CalcHashCode()
        {
            /*if(string.IsNullOrWhiteSpace(drivingOnStreet))
            {
                drivingOnStreet = "no street name available";
            }*/           

            int sI = routeData.route.ShapeMeta[currentShapeIndex].Shape;
            Itinero.LocalGeo.Coordinate secondToLastCoord = routeData.route.Shape[sI - 1];

            //Vector2D directionVector = new Vector2D(goingToCoord.Latitude - secondToLastCoord.Latitude, goingToCoord.Longitude - secondToLastCoord.Longitude).normalized;

            //int wayId = (int)RouteCreator.GetWayId(goingToCoord.Latitude, goingToCoord.Longitude);

            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                //hash = hash * 23 + drivingOnStreet.GetHashCode();
                //hash = hash * 23 + wayId.GetHashCode();
                hash = hash * 23 + goingToCoord.Latitude.GetHashCode();
                hash = hash * 23 + goingToCoord.Longitude.GetHashCode();
                //hash = hash * 23 + directionVector.x.GetHashCode();
                //hash = hash * 23 + directionVector.y.GetHashCode();
                hash = hash * 23 + secondToLastCoord.Latitude.GetHashCode();
                hash = hash * 23 + secondToLastCoord.Longitude.GetHashCode();
                //hash = hash * 23 + currentShapeLength.GetHashCode(); //needed since we can drive on the same street from to different direction going to the same node in a t-junction
                return hash;
            }
        }

        public int GetNextHashCode()
        {
            /*string nextStreetName;
            routeData.route.ShapeMeta[currentShapeIndex + 1].Attributes.TryGetValue("name", out nextStreetName);
            if (string.IsNullOrWhiteSpace(nextStreetName))
            {
                nextStreetName = "no street name available";
            }*/           

            int sI = routeData.route.ShapeMeta[currentShapeIndex + 1].Shape;
            Itinero.LocalGeo.Coordinate nextGoalCoord = routeData.route.Shape[sI];
            Itinero.LocalGeo.Coordinate secondToLastCoord = routeData.route.Shape[sI - 1];

            //Vector2D directionVector = new Vector2D(nextGoalCoord.Latitude - secondToLastCoord.Latitude, nextGoalCoord.Longitude - secondToLastCoord.Longitude).normalized;

            //int nextWayId = (int)RouteCreator.GetWayId(nextGoalCoord.Latitude, nextGoalCoord.Longitude);

            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                //hash = hash * 23 + nextStreetName.GetHashCode();
                //hash = hash * 23 + nextWayId.GetHashCode();
                hash = hash * 23 + nextGoalCoord.Latitude.GetHashCode();
                hash = hash * 23 + nextGoalCoord.Longitude.GetHashCode();
                //hash = hash * 23 + directionVector.x.GetHashCode();
                //hash = hash * 23 + directionVector.y.GetHashCode();
                hash = hash * 23 + secondToLastCoord.Latitude.GetHashCode();
                hash = hash * 23 + secondToLastCoord.Longitude.GetHashCode();                
                return hash;
            }            
        }

        public bool WillChangeRoad(float deltaTime, float speed)
        {
            float cDL = currentDistanceLeft;
            cDL -= deltaTime * speed;
            if (cDL <= 0.0f)
            {
                int cSI = currentShapeIndex + 1;
                //check if we have arrived or just going to next shape/node
                if (cSI == routeData.route.ShapeMeta.Length)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        public void ChangeRoute(RouteData desiredNewRoute)
        {
            routeData = desiredNewRoute;
            currentShapeIndex = 1;
            //add old distance left to the new route piece length to keep somewhat consistent travel distance
            currentDistanceLeft = routeData.route.ShapeMeta[currentShapeIndex].Distance + currentDistanceLeft;
            currentSpeedLimit = GetCurrentSpeedLimit();
            hasArrived = false;

            int sI = routeData.route.ShapeMeta[currentShapeIndex].Shape;
            goingToCoord = routeData.route.Shape[sI];

            routeData.route.ShapeMeta[currentShapeIndex].Attributes.TryGetValue("name", out drivingOnStreet);
            densityHash = CalcHashCode();

            currentShapeLength = routeData.route.ShapeMeta[currentShapeIndex].Distance;
        }

        /// <summary>
        /// Returns speed in [m/s] based on highway type if found, if not found default speed is 2.78 m/s (10 km/h).
        /// </summary>
        /// <returns></returns>
        private float GetCurrentSpeedLimit()
        {
            //default 10 km/h
            float speed = 2.78f;
            bool foundSpeed = Itinero.Attributes.IAttributeCollectionExtension.TryGetMaxSpeed(routeData.route.ShapeMeta[currentShapeIndex].Attributes, out speed);
            if (foundSpeed)
            {
                speed /= 3.6f;
            }
            else
            {
                //getting speed this way includes some sort of normal background traffic and traffic lights/intersections(?), so it is always lower than actual speed limit
                //https://stackoverflow.com/questions/32906076/osmsharp-get-informations-about-actual-road
                //Itinero.Profiles.FactorAndSpeed factorAndSpeed = Itinero.Osm.Vehicles.Vehicle.Car.Fastest().FactorAndSpeed(route.ShapeMeta[currentShapeIndex].Attributes);
                //float speed = 1.0f / factorAndSpeed.SpeedFactor;

                string highwayType;
                routeData.route.ShapeMeta[currentShapeIndex].Attributes.TryGetValue("highway", out highwayType);
                speed = MacroTrafficSim.GetSpeedLimit(highwayType);
            }
            return speed;
        }

        public Itinero.Route.Meta GetCurrentMetaData()
        {
            return routeData.route.ShapeMeta[currentShapeIndex];
        }

        Vector4 positionAndSpeed;
        public Vector4 GetUnityPositionAndSpeed(bool updateData)
        {
            if (updateData)
            {
                Vector2 pos = Vector2.Lerp(unityCurrentStartPosition, unityCurrentGoalPosition, 1.0f - (currentDistanceLeft / currentShapeLength));
                float speed = latestSpeed / currentSpeedLimit;
                positionAndSpeed = new Vector4(pos.x, pos.y, speed, 0f);
            }

            return positionAndSpeed;
        }        

        public void MoveCarSpeed(float timeStamp, float deltaTime, float speed)
        {
            latestSpeed = speed;

            currentDistanceLeft -= deltaTime * speed;
            totalTravelDistance += deltaTime * speed;
            totalDrivingTime += deltaTime;
            if (currentDistanceLeft <= 0.0f)
            {
                ++currentShapeIndex;
                //check if we have arrived or just going to next shape/node
                if (currentShapeIndex == routeData.route.ShapeMeta.Length)
                {
                    //check if we can actually arrive based on flow at goal
                    if (routeData.evacGoal.CarArrives(this, timeStamp, deltaTime))
                    {
                        hasArrived = true;
                        //reduce anyovershooting distance
                        totalTravelDistance += currentDistanceLeft;
                    }     
                    else
                    {          
                        //keep these numbers the same and try to arrive next time step
                        currentDistanceLeft += deltaTime * speed;
                        totalTravelDistance -= deltaTime * speed;
                        --currentShapeIndex;
                    }

                    //set start to goal since we are basically at goal, at worst queing
                    unityCurrentStartPosition = unityCurrentGoalPosition;
                }
                else
                {
                    //remove any potential overshoot of distance
                    totalTravelDistance += currentDistanceLeft;
                    //add "old" current distance left since there might be some residual actual travel spent (negative distance left)?
                    currentDistanceLeft = routeData.route.ShapeMeta[currentShapeIndex].Distance - routeData.route.ShapeMeta[currentShapeIndex - 1].Distance;// + currentDistanceLeft;
                    currentShapeLength = currentDistanceLeft;
                    currentSpeedLimit = GetCurrentSpeedLimit();                                       

                    //update new going to coordinates
                    int sI = routeData.route.ShapeMeta[currentShapeIndex].Shape;
                    goingToCoord = routeData.route.Shape[sI];

                    //save last goal coordinate in Unity space as it is now the start coordinate
                    unityCurrentStartPosition = unityCurrentGoalPosition;
                    //save new goal coordinate in Unity space
                    Mapbox.Utils.Vector2d pos = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(goingToCoord.Latitude, goingToCoord.Longitude, WUInity.MAP.CenterMercator, WUInity.MAP.WorldRelativeScale);
                    unityCurrentGoalPosition = new Vector2((float)pos.x, (float)pos.y);

                    routeData.route.ShapeMeta[currentShapeIndex].Attributes.TryGetValue("name", out drivingOnStreet);

                    densityHash = CalcHashCode();
                }
            }
        }
    }
}
