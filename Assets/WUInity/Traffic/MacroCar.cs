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

        public int roadSegmentHash;
      
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
            UpdateHash();

            totalDrivingTime = 0f;
            latestSpeed = 0;
        }

        private void UpdateHash()
        {      
            int sI = routeData.route.ShapeMeta[currentShapeIndex].Shape;
            Itinero.LocalGeo.Coordinate secondToLastCoord = routeData.route.Shape[sI - 1];

            roadSegmentHash = CalcHash(goingToCoord, secondToLastCoord);
        }

        private int CalcHash(Itinero.LocalGeo.Coordinate goalCoord, Itinero.LocalGeo.Coordinate secondToLastCoord)
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + goalCoord.Latitude.GetHashCode();
                hash = hash * 23 + goalCoord.Longitude.GetHashCode();
                hash = hash * 23 + secondToLastCoord.Latitude.GetHashCode();
                hash = hash * 23 + secondToLastCoord.Longitude.GetHashCode();
                return hash;
            }
        }

        public int GetNextHashCode()
        {       

            int sI = routeData.route.ShapeMeta[currentShapeIndex + 1].Shape;
            Itinero.LocalGeo.Coordinate nextGoalCoord = routeData.route.Shape[sI];
            Itinero.LocalGeo.Coordinate secondToLastCoord = routeData.route.Shape[sI - 1];

            return CalcHash(nextGoalCoord, secondToLastCoord);          
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
            UpdateHash();

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

        public void SetSpline(LinearSpline2D newSpline)
        {
            spline = newSpline; 
        }

        Vector4 positionAndSpeed;
        //CatmullRomSpline2D spline;
        LinearSpline2D spline;
        public Vector4 GetUnityPositionAndSpeed(bool updateData)
        {
            if (updateData)
            {
                Vector2 pos = spline.GetYZValue(currentShapeLength - currentDistanceLeft);
                float speed = latestSpeed / currentSpeedLimit;
                positionAndSpeed = new Vector4(pos.x, pos.y, speed, 0f);
            }

            return positionAndSpeed;
        }        

        public void MoveCar(float timeStamp, float deltaTime, float speed)
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

                    routeData.route.ShapeMeta[currentShapeIndex].Attributes.TryGetValue("name", out drivingOnStreet);

                    UpdateHash();

                    //CalculateSpline();
                    latestSpeed = 0;
                }
            }
        }
    }
}
