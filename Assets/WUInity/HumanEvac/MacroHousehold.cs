using UnityEngine;

namespace WUInity.Evac
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
        public MacroHousehold(HumanEvacCell humanRaster, Vector2D nodeCenter, int peopleInHousehold, float walkingSpeed, float responseTime)
        {
            EvacInput eO = WUInity.INPUT.evac;

            Vector2 rand = Random.insideUnitCircle;
            Vector2D randD = new Vector2D(rand.x * humanRaster.cellWorldSize.x, rand.y * humanRaster.cellWorldSize.x);
            Vector2D startPos = nodeCenter + randD * 0.707070;
            //startPos.x += humanRaster.cellWorldSize.x * Random.Range(-0.5f, 0.5f); //nicer to be in circle instead of square?
            //startPos.y += humanRaster.cellWorldSize.y * Random.Range(-0.5f, 0.5f);
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
            walkingDistance = (float)Vector2D.Distance(startPos, humanRaster.closestNodeUnitySpace) * eO.walkingDistanceModifier;
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
            goalPosition = new Vector2((float)humanRaster.closestNodeUnitySpace.x, (float)humanRaster.closestNodeUnitySpace.y);
        }

        public Vector4 GetPositionAndState(float time)
        {
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

            Vector2 position = Vector2.Lerp(startPosition, goalPosition, (time - responseTime) / (evacuationTime - responseTime));
            return new Vector4(position.x, position.y, peopleInHousehold, state);
        }
    }
}