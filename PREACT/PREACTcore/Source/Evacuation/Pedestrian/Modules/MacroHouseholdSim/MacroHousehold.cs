//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Numerics;
using PREACT.IO;
using PREACT.Math;
using PREACT.Evacuation;

namespace PREACT.Pedestrian
{
    /// <summary>
    /// A unit of people (could also be just one person) that travel together to reach their goal (car).
    /// A response time and total travel time is pre-calculated when intialized and late rused to determine if they have reached that goal.
    /// </summary>
    public class MacroHousehold
    {
        public float evacuationTime;
        public float ResponseTime;
        public int peopleInHousehold;
        public bool reachedCar;
        public int cars;
        public bool isMoving;
        public float walkingDistance;

        PopulationData.HouseholdData _houseHoldData;
        Vector2 homePosition, carPosition;
        EvacuationGroup _evacuationGroup;

        public EvacuationGroup EvacuationGroup { get => _evacuationGroup; }

        /// <summary>
        /// Creates a household that will move as a unit.
        /// evacuation time is determined based in distance/walking speed and response time
        /// </summary>
        /// <param name="humanRaster"></param>
        /// <param name="nodeCenter"></param>
        /// <param name="peopleInHousehold"></param>
        /// <param name="walkingSpeed"></param>
        /// <param name="responseTime"></param>
        public MacroHousehold(PopulationData.HouseholdData householdData, float walkingSpeed, EvacuationGroup evacuationGroup, Simulation simulation)
        {
            PopulationInput popInput = simulation.Input.Population;
            MacroHouseholdSimInput houseInput = simulation.Input.Pedestrian.MacroHouseholdSimInput;
            _evacuationGroup = evacuationGroup;            

            _houseHoldData = householdData;
            peopleInHousehold = householdData.peopleCount;
            cars = 1;
            if (popInput.AllowMoreThanOneCar)
            {
                if (peopleInHousehold >= 2)
                {
                    if (Random.Range(0f, 1f) <= popInput.MaxCarsProbability)
                    {
                        cars = Mathf.Min(peopleInHousehold, popInput.MaxCars);
                    }
                }
            }

            reachedCar = false;
            Vector2d temp = simulation.GetSimulationPosition(householdData.originLatLon);
            homePosition = new Vector2((float)temp.x, (float)temp.y);           
            temp = simulation.GetSimulationPosition(householdData.roadAccessLatLon);
            carPosition = new Vector2((float)temp.x, (float)temp.y);
            walkingDistance = Vector2.Distance(homePosition, carPosition) * houseInput.WalkingDistanceModifier;

            ResponseTime = _evacuationGroup.GetWeightedRandomResponseTime(simulation.Input.Evacuation.EvacuationOrderStart);

            float travelTime = walkingDistance / walkingSpeed;
            if (ResponseTime == float.MaxValue)
            {
                evacuationTime = float.MaxValue;
            }
            else
            {
                evacuationTime = travelTime + ResponseTime;
            }
            isMoving = false;            
        }

        public Vector2d GetVehicleLatLon()
        {
            return _houseHoldData.roadAccessLatLon;
        }

        public Vector4 GetPositionAndState(float time)
        {
            //states are used in shader to apply color
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

            float ratio = (time - ResponseTime) / (evacuationTime - ResponseTime);
            ratio = Mathf.Clamp01(ratio);
            Vector2 position = Vector2.Lerp(homePosition, carPosition, ratio);
            return new Vector4(position.X, position.Y, peopleInHousehold, state);
        }
    }
}