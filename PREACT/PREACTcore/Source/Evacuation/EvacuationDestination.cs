//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using PREACT.Traffic;
using System.IO;
using PREACT.IO;
using PREACT.Math;

namespace PREACT.Evacuation
{
    public class EvacuationDestination
    {
        //properties
        private Vector2d _latLon;
        private PREACTColor _color;
        private float _maxFlow = 3600f; //cars per hour
        private string _name = "Destination";
        private EvacGoalType _goalType = EvacGoalType.Refugee;
        private int _maxCars = -1;
        private int _maxPeople = -1;
        private bool _blocked = false; 

        //data
        Simulation _simulation;    
        private uint _currentPeople;
        private List<TrafficModuleVehicle> _vehicles = new List<TrafficModuleVehicle>();
        private float _currentVehicleFlow = 0f;
        private float _firstArrivalTime;
        private float _currentTimeStep;
        private int _timeStepCars;
        //data for WUI-SHOW etc
        private float _totalTravelTime;
        private float _averageTravelTime;

        public Vector2d LatLon { get => _latLon; }
        public PREACTColor Color { get => _color; }
        public bool Blocked { get => _blocked; }
        public float MaxFlow { get => _maxFlow; }
        public string Name { get => _name; }
        public EvacGoalType GoalType { get => _goalType; }
        public int MaxCars { get => _maxCars; }
        public int MaxPeople { get => _maxPeople; }
        public uint CurrentPeople { get => _currentPeople; }
        public List<TrafficModuleVehicle> Vehicles { get => _vehicles; }
        public float CurrentVehicleFlow { get => _currentVehicleFlow; }
        public float FirstArrivalTime { get => _firstArrivalTime; }
        public float CurrentTimeStep { get => CurrentTimeStep; }
        public int TimeStepCars { get => TimeStepCars; }
        public float TotalTravelTime { get => _totalTravelTime; }
        public float AverageTravelTime { get => _averageTravelTime; }
        
        private EvacuationDestination(Simulation simulation, EvacuationDestinationInput input)
        {
            _simulation = simulation;
            _latLon = input.LatLon;
            _color = input.Color;
            _maxFlow = input.MaxFlow;
            _name = input.Name;
            _goalType = input.Type;
            _maxCars = input.MaxVehicles;
            _maxPeople = input.MaxPeople;
            _blocked = input.Blocked; 
        }

        public static List<EvacuationDestination> CreateEvacacuationDestinations(Simulation simulation, List<EvacuationDestinationInput> destinationsInput)
        {
            List<EvacuationDestination> destinations = new List<EvacuationDestination>(destinationsInput.Count);

            foreach (EvacuationDestinationInput e in destinationsInput)
            {
                destinations.Add(new EvacuationDestination(simulation, e));
            }

            return destinations;
        }

        public void BlockDestination(Simulation simulation)
        {
            _blocked = true;
        }

        /// <summary>
        /// Checks flow and returns true if car arrives at goal, returns false if the car have to wait.
        /// </summary>
        /// <param name="arrivingVehicle"></param>
        /// <param name="currentTime"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public bool CarArrives(TrafficModuleVehicle arrivingVehicle, float currentTime, float deltaTime)
        {
            UpdateFlow(currentTime, deltaTime);            

            //car can arrive
            if((_maxFlow <= 0 && !_blocked) || (_currentVehicleFlow < _maxFlow && !_blocked))
            {         
                //add new cars and people that has arrived during timestep
                ++_timeStepCars;
                _vehicles.Add(arrivingVehicle);
                _currentPeople += arrivingVehicle.NumberOfPeople;
                UpdateCapacity();

                _totalTravelTime += currentTime;
                _averageTravelTime = _totalTravelTime / _vehicles.Count;

                return true;
            }

            return false;
        }

        void UpdateCapacity()
        {
            if (_goalType == EvacGoalType.Refugee)
            {
                //track cars and respond
                if (_maxCars > 0 && _vehicles.Count >= _maxCars && !_blocked)
                {
                    _blocked = true;
                    Engine.Message(null, Engine.LogType.Event, "Evacuation goal " + _name + " has reached vehivle capacity, re-routing");
                    _simulation.GoalBlocked();
                }
                else if (_maxCars > 0 && _vehicles.Count > _maxCars)
                {
                    Engine.Message(null, Engine.LogType.Log, "Additional car arrived at " + _name + ", arrived during same time step.");
                }

                //track and respond people
                if (_maxPeople > -1 && _currentPeople >= _maxPeople && !_blocked)
                {
                    _blocked = true;
                    Engine.Message(null, Engine.LogType.Event, "Evacuation goal " + _name + " has reached people capacity, re-routing");
                    _simulation.GoalBlocked();
                }
                else if (_maxPeople > -1 && _currentPeople > _maxPeople)
                {
                    Engine.Message(null, Engine.LogType.Log, "Additional people arrived at " + _name + ", arrived during same time step.");
                }
            }
        }

        private void UpdateFlow(float timeStamp, float deltaTime)
        {
            //new timestamp?
            if (_currentTimeStep != timeStamp)
            {
                _currentTimeStep = timeStamp;
                _timeStepCars = 0;
            }

            //calc current flow
            if (_vehicles.Count == 0)
            {
                _firstArrivalTime = timeStamp;
                _currentVehicleFlow = 0f;
            }
            else
            {
                float timestepFlow = _timeStepCars / deltaTime;
                if (timeStamp == _firstArrivalTime)
                {
                    _currentVehicleFlow = timestepFlow;
                }
                else
                {
                    _currentVehicleFlow = _vehicles.Count / (timeStamp - _firstArrivalTime);
                }
                _currentVehicleFlow = Mathf.Max(timestepFlow, _currentVehicleFlow) * 3600f;
            }
        }
    }
}
