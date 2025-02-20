//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Numerics;

namespace WUIPlatform.Traffic
{
    public class MacroCar : TrafficModuleCar
    {
        public RouteData routeData;
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

        public MacroCar(RouteData desiredRoute, uint numberOfPeopleInCar, uint carID) : base(carID, numberOfPeopleInCar, desiredRoute.evacGoal)
        {
            routeData = desiredRoute;
            this.numberOfPeopleInCar = numberOfPeopleInCar;
            //go directly to shape 1 since shape 0 is just meta data telling that we are a car
            currentShapeIndex = 1;
            currentDistanceLeft = routeData.route.ShapeMeta[currentShapeIndex].Distance;
            currentShapeLength = currentDistanceLeft;
            //currentSpeedLimit = GetAndSetCurrentSpeedLimit();
            hasArrived = false;
            totalTravelDistance = 0.0f;

            int sI = routeData.route.ShapeMeta[currentShapeIndex].Shape;
            goingToCoord = routeData.route.Shape[sI];

            routeData.route.ShapeMeta[currentShapeIndex].Attributes.TryGetValue("name", out drivingOnStreet);
            UpdateHash();

            totalDrivingTime = 0f;
            latestSpeed = 0;

            this.carID = carID;
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
            //currentSpeedLimit = GetAndSetCurrentSpeedLimit();
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
        public float GetAndSetCurrentSpeedLimit()
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

            currentSpeedLimit = speed;
            return speed;
        }

        public void SetCurrentSpeedLimit(float speedLimit)
        {
            currentSpeedLimit = speedLimit;
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
        LinearSpline2D spline;
        public override Vector4 GetWorldPositionSpeedCarID(bool updateData)
        {
            if (updateData)
            {
                Vector2 pos = spline.GetYZValue(currentShapeLength - currentDistanceLeft);
                float speedRatio = latestSpeed / currentSpeedLimit;
                positionAndSpeed = new Vector4(pos.X, pos.Y, speedRatio, 0f);
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
                    //currentSpeedLimit = GetAndSetCurrentSpeedLimit();                                       

                    //update new going to coordinates
                    int sI = routeData.route.ShapeMeta[currentShapeIndex].Shape;
                    goingToCoord = routeData.route.Shape[sI];              
                    //routeData.route.ShapeMeta[currentShapeIndex].Attributes.TryGetValue("name", out drivingOnStreet);
                    UpdateHash();
                }
            }
        }

        public override void Arrive()
        {
            throw new System.NotImplementedException();
        }
    }
}
