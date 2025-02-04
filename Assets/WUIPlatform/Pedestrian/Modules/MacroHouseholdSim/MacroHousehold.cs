//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Numerics;
using WUIPlatform.IO;

namespace WUIPlatform.Pedestrian
{
    /// <summary>
    /// A unit of people (could also be just one person) that travel together to reach their goal (car).
    /// A response time and total travel time is pre-calculated when intialized and late rused to determine if they have reached that goal.
    /// </summary>
    public class MacroHousehold
    {
        public float evacuationTime;
        public float responseTime;
        public int peopleInHousehold;
        public bool reachedCar;
        public int cars;
        public bool isMoving;
        public float walkingDistance;

        Runtime.PopulationData.HouseholdData _houseHoldData;
        Vector2 homePosition, carPosition;
        int _cellIndex;

        /// <summary>
        /// Creates a household that will move as a unit.
        /// evacuation time is determined based in distance/walking speed and response time
        /// </summary>
        /// <param name="humanRaster"></param>
        /// <param name="nodeCenter"></param>
        /// <param name="peopleInHousehold"></param>
        /// <param name="walkingSpeed"></param>
        /// <param name="responseTime"></param>
        public MacroHousehold(Runtime.PopulationData.HouseholdData householdData, float walkingSpeed, float responseTime, int cellIndex)
        {
            EvacuationInput eO = WUIEngine.INPUT.Evacuation;

            _houseHoldData = householdData;
            _cellIndex = cellIndex;
            peopleInHousehold = householdData.peopleCount;
            cars = 1;
            if (eO.allowMoreThanOneCar)
            {
                if (peopleInHousehold >= 2)
                {
                    if (Random.Range(0f, 1f) <= eO.maxCarsChance)
                    {
                        cars = Mathf.Min(peopleInHousehold, eO.maxCars);
                    }
                }
            }

            reachedCar = false;
            Vector2d temp = GeoConversions.GeoToWorldPosition(householdData.originLatLon.x, householdData.originLatLon.y, WUIEngine.RUNTIME_DATA.Simulation.CenterMercator, WUIEngine.RUNTIME_DATA.Simulation.MercatorCorrectionScale);
            homePosition = new Vector2((float)temp.x, (float)temp.y);
            temp = GeoConversions.GeoToWorldPosition(householdData.roadAccessLatLon.x, householdData.roadAccessLatLon.y, WUIEngine.RUNTIME_DATA.Simulation.CenterMercator, WUIEngine.RUNTIME_DATA.Simulation.MercatorCorrectionScale);
            carPosition = new Vector2((float)temp.x, (float)temp.y);
            walkingDistance = Vector2.Distance(homePosition, carPosition) * eO.walkingDistanceModifier;
            float travelTime = walkingDistance / walkingSpeed;
            this.responseTime = responseTime;
            if (responseTime == float.MaxValue)
            {
                evacuationTime = float.MaxValue;
            }
            else
            {
                evacuationTime = travelTime + responseTime;
            }
            isMoving = false;            
        }

        public Vector2d GetCarLatLon()
        {
            return _houseHoldData.roadAccessLatLon;
        }

        public int GetCellIndex()
        {
            return _cellIndex;
        }

        public Vector4 GetPositionAndState(float time)
        {
            //states are use din shader to apply color
            float state = 0.375f;
            if(evacuationTime == float.MaxValue)
            {
                state = 0.125f;
            }
            else if(time >= evacuationTime)
            {
                state = 0.875f;
            }
            else if(isMoving)
            {
                state = 0.625f;
            }

            float ratio = (time - responseTime) / (evacuationTime - responseTime);
            ratio = Mathf.Clamp01(ratio);
            Vector2 position = Vector2.Lerp(homePosition, carPosition, ratio);
            return new Vector4(position.X, position.Y, peopleInHousehold, state);
        }
    }
}