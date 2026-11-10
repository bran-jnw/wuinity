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
        public enum State{ Routine, PreEvacuation, Defending, WalkingToAccessPoint, WaitingForPickup, EvacByVehicle, EvacByFoot, Evacuated }


        private State _state;
        private float _evacuationAwareTime, _startMovingTime, _reachVehicleTime;
        private int _peopleInHousehold;
       
        public int vehicleCount;
        private float _travelTime;

        public int PeopleInHousehold { get => _peopleInHousehold; }
        public State CurrentState { get => _state; }

        PopulationData.HouseholdData _houseHoldData;
        Vector2 _homePosition, vehiclePosition;
        EvacuationGroup _evacuationGroup;

        public EvacuationGroup EvacuationGroup { get => _evacuationGroup; }
        public Vector2d HomePosition { get => _houseHoldData.originLatLon; }

        /// <summary>
        /// Creates a household that will move as a unit.
        /// evacuation time is determined based in distance/walking speed and response time
        /// </summary>
        public MacroHousehold(PopulationData.HouseholdData householdData, float walkingSpeed, EvacuationGroup evacuationGroup, Simulation simulation)
        {
            PopulationInput popInput = simulation.Input.Population;
            MacroHouseholdSimInput houseInput = simulation.Input.PedestrianModule.MacroHouseholdSimInput;
            _evacuationGroup = evacuationGroup;            

            _houseHoldData = householdData;
            _peopleInHousehold = householdData.peopleCount;
            vehicleCount = Mathd.Min(PeopleInHousehold, evacuationGroup.Demographics.VehicleDemographics.SampleVehicleCount(PeopleInHousehold));

            Vector2d temp = simulation.Spatial.GetSimulationPosition(householdData.originLatLon);
            _homePosition = new Vector2((float)temp.x, (float)temp.y);           
            temp = simulation.Spatial.GetSimulationPosition(householdData.roadAccessLatLon);
            vehiclePosition = new Vector2((float)temp.x, (float)temp.y);
            float walkingDistance = Vector2.Distance(_homePosition, vehiclePosition) * houseInput.WalkingDistanceModifier;  
            _travelTime = walkingDistance / walkingSpeed;

            _state = State.Routine;        
        }

        public void Step(MacroHouseholdSim sim, double simulationTime, double deltaTime)
        {
            if(_state == State.Routine ||_state == State.Defending || _state == State.EvacByVehicle)
            {
                return;
            }

            if(_state == State.PreEvacuation)
            {
                if(simulationTime >= _startMovingTime)                    
                {
                    _state = State.WalkingToAccessPoint;
                    
                }                
            }
            else if(_state == State.WalkingToAccessPoint)
            {
                if (simulationTime >= _reachVehicleTime)
                {
                    if (vehicleCount > 0)
                    {
                        _state = State.EvacByVehicle;
                        sim.ReachedVehicle(this);
                    }
                    else
                    {
                        _state = State.WaitingForPickup;
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
            if(_state != State.Routine)
            {
                //TODO: if new evacuation trigger is more urgent than previous, we could update the response time and travel time to reflect that
                return;
            }

            _evacuationAwareTime = (float)simulationTime;

            float responseTime = _evacuationGroup.GetWeightedRandomResponseTime();
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
            if(responseTime == float.MaxValue)
            {
                _state = State.Defending;
            }
            else
            {
                if (responseTime == 0.0)
                {
                    _state = State.WalkingToAccessPoint;
                }
                else
                {
                    _state = State.PreEvacuation;
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
            float state = ((int)_state + 0.5f) * 0.125f;

            double ratio = Mathd.Clamp01((time - _startMovingTime) / (_reachVehicleTime - _startMovingTime));
            if(_state == State.Routine)
            {
                ratio = 0.0;
            }
            Vector2 position = Vector2.Lerp(_homePosition, vehiclePosition, (float)ratio);
            return new Vector4(position.X, position.Y, PeopleInHousehold, state);
        }
    }
}