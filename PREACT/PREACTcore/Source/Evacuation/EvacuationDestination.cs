//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using PREACT.Traffic;
using System.IO;
using PREACT.Input;
using PREACT.Math;

namespace PREACT.Evacuation
{
    public class EvacuationDestination
    {
        //properties
        private Vector2d _latLon;
        private Vector2d _simulationPos;
        private PREACTColor _color;
        private float _maxFlow = -1f; //cars per hour
        private string _name = string.Empty;
        private DestinationTypes _goalType = DestinationTypes.Exit;
        private int _maxCars = -1;
        private int _maxPeople = -1;
        private bool _blocked = false; 

        //data
        Simulation _simulation;    
        private uint _currentPeople;
        private List<TrafficModuleVehicle> _vehicles = new List<TrafficModuleVehicle>();
        private double _currentVehicleFlow = 0f;
        private double _firstArrivalTime;
        private double  _currentTimeStep;
        private int _timeStepCars;
        //data for WUI-SHOW etc
        private double _totalTravelTime;
        private double _averageTravelTime;

        public Vector2d LatLon { get => _latLon; }
        public Vector2d SimulationPos { get => _simulationPos; }
        public PREACTColor Color { get => _color; }
        public bool Blocked { get => _blocked; }
        public float MaxFlow { get => _maxFlow; }
        public string Name { get => _name; }
        public DestinationTypes GoalType { get => _goalType; }
        public int MaxCars { get => _maxCars; }
        public int MaxPeople { get => _maxPeople; }
        public uint CurrentPeople { get => _currentPeople; }
        public List<TrafficModuleVehicle> Vehicles { get => _vehicles; }
        public double CurrentVehicleFlow { get => _currentVehicleFlow; }
        public double FirstArrivalTime { get => _firstArrivalTime; }
        public double CurrentTimeStep { get => CurrentTimeStep; }
        public int TimeStepCars { get => TimeStepCars; }
        public double TotalTravelTime { get => _totalTravelTime; }
        public double AverageTravelTime { get => _averageTravelTime; }
        
        public EvacuationDestination(Simulation simulation, Vector2d latLon, string name)
        {
            _simulation = simulation;
            _latLon = latLon;
            _simulationPos = _simulation.Spatial.GetSimulationPosition(latLon);
            _name = name;
            _color = PREACTColor.Random();
        }

        private EvacuationDestination(Simulation simulation, EvacuationDestinationInput input)
        {
            _simulation = simulation;
            _latLon = input.LatLon;
            _simulationPos = _simulation.Spatial.GetSimulationPosition(_latLon);
            _color = input.Color;
            _maxFlow = input.MaxFlow;
            _name = input.Name;
            _goalType = input.Type;
            _maxCars = input.MaxVehicles;
            _maxPeople = input.MaxPeople;
            _blocked = input.Blocked; 
        }

        public static Dictionary<string, EvacuationDestination> CreateEvacacuationDestinationsFromInput(Simulation simulation, Dictionary<string, EvacuationDestinationInput> destinationsInput)
        {
            Dictionary<string, EvacuationDestination> destinations = new Dictionary<string, EvacuationDestination>(destinationsInput.Count);

            foreach (KeyValuePair<string, EvacuationDestinationInput> e in destinationsInput)
            {
                destinations.Add(e.Key, new EvacuationDestination(simulation, e.Value));
            }

            return destinations;
        }

        public void BlockDestination()
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
        public bool CarArrives(TrafficModuleVehicle arrivingVehicle, double currentTime, double deltaTime)
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
            if (_goalType == DestinationTypes.Shelter)
            {
                //track cars and respond
                if (_maxCars > 0 && _vehicles.Count >= _maxCars && !_blocked)
                {
                    _blocked = true;
                    Engine.Message(null, Engine.LogType.Event, "Evacuation goal " + _name + " has reached vehivle capacity, re-routing");
                    _simulation.Evacuation.GoalBlocked();
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
                    _simulation.Evacuation.GoalBlocked();
                }
                else if (_maxPeople > -1 && _currentPeople > _maxPeople)
                {
                    Engine.Message(null, Engine.LogType.Log, "Additional people arrived at " + _name + ", arrived during same time step.");
                }
            }
        }

        private void UpdateFlow(double timeStamp, double deltaTime)
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
                double timestepFlow = _timeStepCars / deltaTime;
                if (timeStamp == _firstArrivalTime)
                {
                    _currentVehicleFlow = timestepFlow;
                }
                else
                {
                    _currentVehicleFlow = _vehicles.Count / (timeStamp - _firstArrivalTime);
                }
                _currentVehicleFlow = Mathd.Max(timestepFlow, _currentVehicleFlow) * 3600f;
            }
        }
    }
}
