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
        public enum MovementState{ Routine, PreEvacuation, Defending, WalkingToAccessPoint, WaitingForPickup, EvacByVehicle, EvacByFoot, Evacuated }
        public enum AwarenessState { Awake, Asleep, Distressed }


        private MovementState _movementState;
        private AwarenessState _awarenessState;
        private float _evacuationAwareTime, _startMovingTime, _reachVehicleTime;
        private int _peopleInHousehold;
       
        public int vehicleCount;
        private float _travelTime;

        public int PeopleInHousehold { get => _peopleInHousehold; }
        public MovementState CurrentState { get => _movementState; }

        PopulationData.HouseholdData _houseHoldData;
        Vector2 _homePosition, vehiclePosition;
        EvacuationGroup _evacuationGroup;
        EvacuationBehavior _evacuationBehavior;

        public EvacuationGroup EvacuationGroup { get => _evacuationGroup; }
        public Vector2d HomePosition { get => _houseHoldData.originLatLon; }
        public AwarenessState CurrentAwarenessState { get => _awarenessState; }

        /// <summary>
        /// Creates a household that will move as a unit.
        /// evacuation time is determined based in distance/walking speed and response time
        /// </summary>
        public MacroHousehold(PopulationData.HouseholdData householdData, float walkingSpeed, EvacuationGroup evacuationGroup, Simulation simulation)
        {
            PopulationInput popInput = simulation.Input.Population;
            MacroHouseholdSimInput houseInput = simulation.Input.PedestrianModule.MacroHouseholdSimInput;
            _evacuationGroup = evacuationGroup;
            _evacuationBehavior = evacuationGroup.GetProbableEvacuationBehavior();

            _houseHoldData = householdData;
            _peopleInHousehold = householdData.peopleCount;
            vehicleCount = Mathd.Min(PeopleInHousehold, evacuationGroup.Demographics.VehicleDemographics.SampleVehicleCount(PeopleInHousehold));

            Vector2d temp = simulation.Spatial.GetSimulationPosition(householdData.originLatLon);
            _homePosition = new Vector2((float)temp.x, (float)temp.y);           
            temp = simulation.Spatial.GetSimulationPosition(householdData.roadAccessLatLon);
            vehiclePosition = new Vector2((float)temp.x, (float)temp.y);
            float walkingDistance = Vector2.Distance(_homePosition, vehiclePosition) * houseInput.WalkingDistanceModifier;  
            _travelTime = walkingDistance / walkingSpeed;

            _movementState = MovementState.Routine;        
        }

        public void Step(MacroHouseholdSim sim, double simulationTime, double deltaTime)
        {
            if(_movementState == MovementState.Routine ||_movementState == MovementState.Defending || _movementState == MovementState.EvacByVehicle)
            {
                return;
            }

            if(_movementState == MovementState.PreEvacuation)
            {
                if(simulationTime >= _startMovingTime)                    
                {
                    _movementState = MovementState.WalkingToAccessPoint;
                    
                }                
            }
            else if(_movementState == MovementState.WalkingToAccessPoint)
            {
                if (simulationTime >= _reachVehicleTime)
                {
                    if (vehicleCount > 0)
                    {
                        _movementState = MovementState.EvacByVehicle;
                        sim.ReachedVehicle(this);
                    }
                    else
                    {
                        _movementState = MovementState.WaitingForPickup;
                    }                    
                }
            }
        }

        /// <summary>
        /// This can be triggered by either an order or a threshold (fire proximity, smoke density etc.)
        /// </summary>
        /// <param name="simulationTime"></param>
        public void StartEvacuation(double simulationTime, MacroHouseholdSim sim)
        {
            if(_movementState != MovementState.Routine)
            {
                //TODO: if new evacuation trigger is more urgent than previous, we could update the response time and travel time to reflect that
                return;
            }

            // if we actually receive the message to evacuate
            if(_evacuationBehavior.GetAwareness(this))
            {
                _evacuationAwareTime = (float)simulationTime;

                //do we do as we are told?
                if (_evacuationBehavior.GetCompliance(this))
                {
                    float responseTime = _evacuationBehavior.GetProbableResponseTime(this);
                    if (responseTime == float.MaxValue || vehicleCount == 0)
                    {

                        _reachVehicleTime = float.MaxValue;
                        _startMovingTime = float.MaxValue;
                    }
                    else
                    {
                        _startMovingTime = (float)simulationTime + responseTime;
                        _reachVehicleTime = (float)simulationTime + responseTime + _travelTime;
                    }


                    //set our state
                    if (responseTime == float.MaxValue)
                    {
                        _movementState = MovementState.Defending;
                    }
                    else
                    {
                        if (responseTime == 0.0)
                        {
                            _movementState = MovementState.WalkingToAccessPoint;
                        }
                        else
                        {
                            _movementState = MovementState.PreEvacuation;
                        }
                    }
                }
                else
                {
                    _movementState = MovementState.Defending;
                    _reachVehicleTime = float.MaxValue;
                    _startMovingTime = float.MaxValue;
                }               

                sim.HouseholdReacted(this);
            }            
        }

        public Vector2d GetVehicleLatLon()
        {
            return _houseHoldData.roadAccessLatLon;
        }

        public Vector4 GetPositionAndState(double time)
        {
            //states are used in shader to apply color
            float state = ((int)_movementState + 0.5f) * 0.125f;

            double ratio = Mathd.Clamp01((time - _startMovingTime) / (_reachVehicleTime - _startMovingTime));
            if(_movementState == MovementState.Routine)
            {
                ratio = 0.0;
            }
            Vector2 position = Vector2.Lerp(_homePosition, vehiclePosition, (float)ratio);
            return new Vector4(position.X, position.Y, PeopleInHousehold, state);
        }
    }
}