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

        Vector2 startPosition;
        Vector2 goalPosition;

        /// <summary>
        /// Creates a household that will move as a unit.
        /// evacuation time is determined based in distance/walking speed and response time
        /// </summary>
        /// <param name="humanRaster"></param>
        /// <param name="nodeCenter"></param>
        /// <param name="peopleInHousehold"></param>
        /// <param name="walkingSpeed"></param>
        /// <param name="responseTime"></param>
        public MacroHousehold(HumanEvacCell humanRaster, Vector2d nodeCenter, int peopleInHousehold, float walkingSpeed, float responseTime)
        {
            EvacuationInput eO = WUIEngine.INPUT.Evacuation;

            //nicer to be in circle instead of square?
            /*Vector2 rand = Random.insideUnitCircle;
            Vector2d randD = new Vector2d(rand.x * humanRaster.cellWorldSize.x, rand.y * humanRaster.cellWorldSize.x);
            Vector2d startPos = nodeCenter + randD * 0.707070;*/

            Vector2d startPos = nodeCenter;
            startPos.x += humanRaster.cellWorldSize.x * Random.Range(-0.5f, 0.5f); 
            startPos.y += humanRaster.cellWorldSize.y * Random.Range(-0.5f, 0.5f);
            this.peopleInHousehold = peopleInHousehold;
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
            walkingDistance = (float)Vector2d.Distance(startPos, humanRaster.closestNodeSimulationSpace) * eO.walkingDistanceModifier;
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

            //for tracking progress visually
            startPosition = new Vector2((float)startPos.x, (float)startPos.y);
            goalPosition = new Vector2((float)humanRaster.closestNodeSimulationSpace.x, (float)humanRaster.closestNodeSimulationSpace.y);
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
            Vector2 position = Vector2.Lerp(startPosition, goalPosition, ratio);
            return new Vector4(position.X, position.Y, peopleInHousehold, state);
        }
    }
}