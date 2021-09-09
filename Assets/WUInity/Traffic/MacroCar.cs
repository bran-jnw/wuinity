using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity.Traffic
{
    public class MacroCar
    {
        public RouteData routeData;
        public int numberOfPopleInCar;
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

        public MacroCar(RouteData desiredRoute, int numberOfPeopleInCar)
        {
            routeData = desiredRoute;
            this.numberOfPopleInCar = numberOfPeopleInCar;
            //go directly to shape 1 since shape 0 is just meta data telling that we are a car
            currentShapeIndex = 1;
            currentDistanceLeft = routeData.route.ShapeMeta[currentShapeIndex].Distance;
            currentSpeedLimit = GetCurrentSpeedLimit();
            hasArrived = false;
            totalTravelDistance = 0.0f;

            int sI = routeData.route.ShapeMeta[currentShapeIndex].Shape;
            goingToCoord = routeData.route.Shape[sI];

            routeData.route.ShapeMeta[currentShapeIndex].Attributes.TryGetValue("name", out drivingOnStreet);
            densityHash = CalcHashCode();

            currentShapeLength = routeData.route.ShapeMeta[currentShapeIndex].Distance;

            totalDrivingTime = 0f;
        }

        public int CalcHashCode()
        {
            if(string.IsNullOrWhiteSpace(drivingOnStreet))
            {
                drivingOnStreet = "no street name available";
            }
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + drivingOnStreet.GetHashCode();
                hash = hash * 23 + goingToCoord.Latitude.GetHashCode();
                hash = hash * 23 + goingToCoord.Longitude.GetHashCode();
                hash = hash * 23 + currentShapeLength.GetHashCode(); //needed since we can drive on the same street from to different direction going to the same node in a t-junction
                return hash;
            }
            /*int hash = 17;
            hash = hash * 31 + drivingOnStreet.GetHashCode();
            hash = hash * 31 + goingToCoord.GetHashCode();
            hash = hash * 31 + currentShapeLength.GetHashCode();*/

            // Allow arithmetic overflow, numbers will just "wrap around"
            /*unchecked
            {
                int hashcode = 1430287;
                //hashcode = hashcode * 7302013 ^ drivingOnStreet.GetHashCode();
                hashcode = hashcode * 7302013 ^ goingToCoord.GetHashCode();
                hashcode = hashcode * 7302013 ^ currentShapeLength.GetHashCode();
                return hashcode;
            }*/
        }

        public void ChangeRoute(RouteData desiredNewRoute)
        {
            /*if (routeData.route.Shape[routeData.route.Shape.Length - 1].Latitude == desiredRoute.Shape[desiredRoute.Shape.Length - 1].Latitude
                && routeData.route.Shape[routeData.route.Shape.Length - 1].Longitude == desiredRoute.Shape[desiredRoute.Shape.Length - 1].Longitude)
            {
                MonoBehaviour.print("same route");
            }
            else
            {
                MonoBehaviour.print("new route");
            }*/

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

        private float GetCurrentSpeedLimit()
        {
            //default 10 km/h
            float speed = 2.78f;
            bool foundSpeed = Itinero.Attributes.IAttributeCollectionExtension.TryGetMaxSpeed(routeData.route.ShapeMeta[currentShapeIndex].Attributes, out speed);
            if (foundSpeed)
            {
                speed /= 3.6f;
                //MonoBehaviour.print("speed found");
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
            //MonoBehaviour.print(speed);
            return speed;
        }

        public Itinero.Route.Meta GetCurrentMetaData()
        {
            return routeData.route.ShapeMeta[currentShapeIndex];
        }

        /*public void MoveCarMod(float deltaTime, float speedModifier)
        {
            currentDistanceLeft -= deltaTime * currentSpeedLimit * speedModifier;
            totalTravelDistance += deltaTime * currentSpeedLimit * speedModifier;
            totalDrivingTime += deltaTime;
            if (currentDistanceLeft <= 0.0f)
            {
                ++currentShapeIndex;
                //check if we have arrived or just going to next shape/node
                if (currentShapeIndex == routeData.route.ShapeMeta.Length)
                {
                    hasArrived = true;
                    //reduce with overshooting distance
                    totalTravelDistance += currentDistanceLeft;
                }
                else
                {
                    //add "old" current distance left since there might be some residual actual travel spent (negative distance left)
                    currentDistanceLeft = routeData.route.ShapeMeta[currentShapeIndex].Distance - routeData.route.ShapeMeta[currentShapeIndex - 1].Distance;// + currentDistanceLeft;
                    currentSpeedLimit = GetCurrentSpeedLimit();

                    int sI = routeData.route.ShapeMeta[currentShapeIndex].Shape;
                    goingToCoord = routeData.route.Shape[sI];

                    routeData.route.ShapeMeta[currentShapeIndex].Attributes.TryGetValue("name", out drivingOnStreet);
                    streetHash = Animator.StringToHash(drivingOnStreet);

                    currentShapeLength = routeData.route.ShapeMeta[currentShapeIndex].Distance - routeData.route.ShapeMeta[currentShapeIndex - 1].Distance;
                }
            }
        }*/

        public void MoveCarSpeed(float timeStamp, float deltaTime, float speed)
        {
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
                        //reduce with overshooting distance
                        totalTravelDistance += currentDistanceLeft;
                    }     
                    else
                    {          
                        //keep these numbers the same
                        currentDistanceLeft += deltaTime * speed;
                        totalTravelDistance -= deltaTime * speed;
                        --currentShapeIndex;
                    }
                }
                else
                {
                    //add "old" current distance left since there might be some residual actual travel spent (negative distance left)?
                    currentDistanceLeft = routeData.route.ShapeMeta[currentShapeIndex].Distance - routeData.route.ShapeMeta[currentShapeIndex - 1].Distance;// + currentDistanceLeft;
                    currentSpeedLimit = GetCurrentSpeedLimit();

                    int sI = routeData.route.ShapeMeta[currentShapeIndex].Shape;
                    goingToCoord = routeData.route.Shape[sI];

                    routeData.route.ShapeMeta[currentShapeIndex].Attributes.TryGetValue("name", out drivingOnStreet);
                    densityHash = CalcHashCode();

                    currentShapeLength = routeData.route.ShapeMeta[currentShapeIndex].Distance - routeData.route.ShapeMeta[currentShapeIndex - 1].Distance;
                }
            }
        }
    }
}
