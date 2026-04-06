//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Numerics;
using PREACT.Input;
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
        private float _travelTime;

        PopulationData.HouseholdData _houseHoldData;
        Vector2 _homePosition, carPosition;
        EvacuationGroup _evacuationGroup;

        public EvacuationGroup EvacuationGroup { get => _evacuationGroup; }
        public Vector2d HomePosition { get => _houseHoldData.originLatLon; }

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
            MacroHouseholdSimInput houseInput = simulation.Input.PedestrianModule.MacroHouseholdSimInput;
            _evacuationGroup = evacuationGroup;            

            _houseHoldData = householdData;
            peopleInHousehold = householdData.peopleCount;
            cars = 1;
            if (evacuationGroup.Demographics.AllowMoreThanOneCar)
            {
                if (peopleInHousehold >= 2)
                {
                    if (Random.valueF <= evacuationGroup.Demographics.MaxCarsProbability)
                    {
                        cars = Mathf.Min(peopleInHousehold, evacuationGroup.Demographics.MaxCars);
                    }
                }
            }

            reachedCar = false;
            Vector2d temp = simulation.Spatial.GetSimulationPosition(householdData.originLatLon);
            _homePosition = new Vector2((float)temp.x, (float)temp.y);           
            temp = simulation.Spatial.GetSimulationPosition(householdData.roadAccessLatLon);
            carPosition = new Vector2((float)temp.x, (float)temp.y);
            walkingDistance = Vector2.Distance(_homePosition, carPosition) * houseInput.WalkingDistanceModifier;

            ResponseTime = _evacuationGroup.GetWeightedRandomResponseTime((float)simulation.Time.GetSimulationTime(_evacuationGroup.EvacuationOrderDateTime));

            _travelTime = walkingDistance / walkingSpeed;
            if (ResponseTime == float.MaxValue)
            {
                evacuationTime = float.MaxValue;
            }
            else
            {
                evacuationTime = _travelTime + ResponseTime;
            }
            isMoving = false;            
        }

        public void StartEvacuation(double simulationTime)
        {
            evacuationTime = (float)simulationTime + _travelTime;
        }

        public Vector2d GetVehicleLatLon()
        {
            return _houseHoldData.roadAccessLatLon;
        }

        public Vector4 GetPositionAndState(double time)
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

            double ratio = (time - ResponseTime) / (evacuationTime - ResponseTime);
            ratio = Mathd.Clamp01(ratio);
            Vector2 position = Vector2.Lerp(_homePosition, carPosition, (float)ratio);
            return new Vector4(position.X, position.Y, peopleInHousehold, state);
        }
    }
}