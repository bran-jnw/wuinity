//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using static WUIPlatform.Traffic.MacroTrafficSim;
using System.Numerics;
using System;
using WUIPlatform.IO;


namespace WUIPlatform.Traffic
{
    public class RoadSegment
    {
        public Itinero.LocalGeo.Coordinate goalCoord;
        string streetName;
        public int laneCount;
        public float length;
        public List<MacroCar> cars;
        string highwayType;
        MacroTrafficSim mCS;
        public float maxCapacity;
        private int maxCarsOnRoad;
        public bool upstreamMovementBlocked;
        LinearSpline2D spline;
        public float speedLimit;

        public RoadSegment(MacroCar car, MacroTrafficSim mCS)
        {
            goalCoord = car.goingToCoord;
            streetName = car.drivingOnStreet;
            car.GetCurrentMetaData().Attributes.TryGetValue("highway", out highwayType);
            laneCount = GetNumberOfLanes(highwayType);
            maxCapacity = GetMaxCapacity(highwayType);
            length = car.currentShapeLength;
            cars = new List<MacroCar>();
            cars.Add(car);
            this.mCS = mCS;

            maxCarsOnRoad = Math.Max(1, (int)(length * 0.2f * laneCount)); //each car takes about 5 meters
            upstreamMovementBlocked = false;

            CalculateSpline(car);
            speedLimit = car.GetAndSetCurrentSpeedLimit();
            car.SetSpline(spline);
        }

        //new way of interpolating in GUI
        public void CalculateSpline(MacroCar car)
        {
            RouteData routeData = car.routeData;
            int currentShapeIndex = car.currentShapeIndex;

            int startSI = routeData.route.ShapeMeta[currentShapeIndex - 1].Shape;
            int endSI = routeData.route.ShapeMeta[currentShapeIndex].Shape;
            int points = endSI - startSI + 1;
            Vector3[] segmentCoordinates = new Vector3[points];
            float distance = 0;
            for (int i = 0; i < points; i++)
            {
                Itinero.LocalGeo.Coordinate coordinate = routeData.route.Shape[i + startSI];
                Vector2d unityPos = GeoConversions.GeoToWorldPosition(coordinate.Latitude, coordinate.Longitude, WUIEngine.RUNTIME_DATA.Simulation.CenterMercator, WUIEngine.RUNTIME_DATA.Simulation.MercatorCorrectionScale);
                if (i > 0)
                {
                    distance += Vector2.Distance(new Vector2((float)unityPos.x, (float)unityPos.y), new Vector2(segmentCoordinates[i - 1].Y, segmentCoordinates[i - 1].Z));
                }
                //segmentCoordinates[i] = new Vector3(distance / currentShapeLength, (float)unityPos.x, (float)unityPos.y); // for catmull-rom splines we need fraction of distance
                segmentCoordinates[i] = new Vector3(distance, (float)unityPos.x, (float)unityPos.y);
            }
            //spline = new CatmullRomSpline2D(segmentCoordinates);
            spline = new LinearSpline2D(segmentCoordinates);
        }

        /// <summary>
        /// Checks if street is physically filled
        /// </summary>
        /// <returns></returns>
        public bool CanAddCar()
        {
            bool success = false;
            if (cars.Count < maxCarsOnRoad)
            {
                success = true;
            }

            return success;
        }

        public void AddCar(MacroCar car)
        {
            car.SetCurrentSpeedLimit(speedLimit);
            car.SetSpline(spline);
            cars.Add(car);
        }

        public float CalculateSpeedBasedOnDensity()
        {
            TrafficInput tO = WUIEngine.INPUT.Traffic;
            //reasonable? not for now
            /*if(cars.Count == 1)
            {
                return Random.Range(0.8f, 0.9f) * SpeedLimit;
            }*/

            float speedLimit = cars[0].currentSpeedLimit;

            float dens = cars.Count / (length * 0.001f * laneCount);
            //added background traffic
            dens += Random.Range(tO.macroTrafficSimInput.backGroundDensityMinMax.X, tO.macroTrafficSimInput.backGroundDensityMinMax.Y);

            //we use the same function to check if a road is blocked due to being main road or if they reverse lanes for now
            if (mCS.stallBigRoads && CanReverseLanes(highwayType))
            {
                dens = maxCapacity; //gives  stall speed
            }
            //reverse traffic in lanes means double the amount of lanes
            else if (mCS.reverseLanes && CanReverseLanes(highwayType))
            {
                dens *= 0.5f;
            }
            float speed = Mathf.Lerp(speedLimit, tO.macroTrafficSimInput.stallSpeed / 3.6f, dens / maxCapacity);

            float speed_visibilty = speed;
            if (tO.visibilityAffectsSpeed && WUIEngine.INPUT.Smoke.smokeModuleChoice == SmokeInput.SmokeModuleChoice.GlobalSmoke)
            {
                //added Enrico & Paolo article      
                float D_L = WUIEngine.RUNTIME_DATA.Smoke.GetGlobalDensity(WUIEngine.SIM.CurrentTime);
                //get rid of any strange values of D_L, TODO: fix when checking input
                D_L = Mathf.Clamp(D_L, 0.0f, 0.2f);
                float beta = -101.57f * D_L * D_L * D_L + 49.43f * D_L * D_L - 9.2755f * D_L + 1.0f;

                //these are not needed as we implement the linear equatíon using lerp
                //float k_j = 100.0f; // number from above, as in max density is 100 cars per km and lane
                //float k = k_j * (1.0f - speed / (speedLimit * beta));

                //this is probably all that is needed? since we base speed on speed limit and not direct proportion to density
                float visibilityLimitedSpeed = beta * speedLimit;
                float stallSpeed = tO.macroTrafficSimInput.stallSpeed / 3.6f;
                if (visibilityLimitedSpeed < stallSpeed)
                {
                    //TODO: which approach is best ?
                    //stallSpeed = visibilityLimitedSpeed; 
                    visibilityLimitedSpeed = stallSpeed;
                }
                speed_visibilty = Mathf.Lerp(visibilityLimitedSpeed, stallSpeed, dens / maxCapacity);
            }

            return Mathf.Min(speed, speed_visibilty);
        }
    }
}