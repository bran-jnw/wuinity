//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using PREACT.Traffic;
using System.IO;
using PREACT.Utility.Math;

namespace PREACT.Evacuation
{
    [System.Serializable]
    public class EvacuationDestination
    {
        Simulation _simulation;
        private Vector2d _latLon;
        private WUIEngineColor _color;
        private bool _blocked = false;
        private float _maxFlow = 3600f; //cars per hour
        private string _name = "Destination";
        private EvacGoalType _goalType = EvacGoalType.Refugee;
        private int _maxCars = -1;
        private int _maxPeople = -1;
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
        public WUIEngineColor Color { get => _color; }
        public bool Blocked { get => _blocked; }
        public float MaxFlow { get => _maxFlow; }
        public string Name { get => _name; }
        public EvacGoalType GoalType { get => _goalType; }
        public int MaxCars { get => _maxCars; }
        public int MaxPeople { get => _maxPeople; }
        public uint CurrentPeople { get => _currentPeople; }
        public List<TrafficModuleVehicle> Vehicles { get => _vehicles; }
        public float CurrenVehicleFlow { get => _currentVehicleFlow; }
        public float FirstArrivalTime { get => _firstArrivalTime; }
        public float CurrentTimeStep { get => CurrentTimeStep; }
        public int TimeStepCars { get => TimeStepCars; }
        public float TotalTravelTime { get => _totalTravelTime; }
        public float AverageTravelTime { get => _averageTravelTime; }
        

        public EvacuationDestination()
        {
            _name = "New goal";
            _latLon = Vector2d.zero;
            _color = WUIEngineColor.white;
        }

        public EvacuationDestination(string name, Vector2d latLon, WUIEngineColor color)
        {
            _name = name;
            _latLon = latLon;
            _color = color;
            _maxFlow = 3600f;
        }

        public EvacuationDestination(string name, Vector2d latLon, WUIEngineColor color, float maxFlow)
        {
            _name = name;
            _latLon = latLon;
            _color = color;
            _maxFlow = maxFlow;
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
                    Engine.MESSAGE(null, Engine.LogType.Event, "Evacuation goal " + _name + " has reached vehivle capacity, re-routing");
                    _simulation.GoalBlocked();
                }
                else if (_maxCars > 0 && _vehicles.Count > _maxCars)
                {
                    Engine.MESSAGE(null, Engine.LogType.Log, "Additional car arrived at " + _name + ", arrived during same time step.");
                }

                //track and respond people
                if (_maxPeople > -1 && _currentPeople >= _maxPeople && !_blocked)
                {
                    _blocked = true;
                    Engine.MESSAGE(null, Engine.LogType.Event, "Evacuation goal " + _name + " has reached people capacity, re-routing");
                    _simulation.GoalBlocked();
                }
                else if (_maxPeople > -1 && _currentPeople > _maxPeople)
                {
                    Engine.MESSAGE(null, Engine.LogType.Log, "Additional people arrived at " + _name + ", arrived during same time step.");
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

        /*public void ResetPeopleAndCars()
        {
            _blocked = false; 

            _currentPeople = 0;
            _vehicles.Clear();

            //reset stuff for flow calc
            _currentVehicleFlow = 0f;
            _timeStepCars = 0;
            _firstArrivalTime = float.MinValue;
            _currentTimeStep = float.MinValue;

            _totalTravelTime = 0f;
            _averageTravelTime = 0f;
        }*/

        public static List<EvacuationDestination> LoadEvacuationGoalFiles(Engine engine, out bool success)
        {
            success = false;
            List<EvacuationDestination> evacuationGoals = new List<EvacuationDestination>();

            for (int i = 0; i < engine.Input.Evacuation.EvacuationGoalFiles.Length; i++)
            {
                string path = Path.Combine(engine.WorkingFolder, engine.Input.Evacuation.EvacuationGoalFiles[i] + ".ed");
                bool fileExists = File.Exists(path);
                if (fileExists)
                {
                    string[] dataLines = File.ReadAllLines(path);

                    string name, exitType, blocked;
                    double lati, longi;
                    float maxFlow, r, g, b;
                    int maxCars, maxPeople;
                    bool initiallyBlocked;
                    EvacGoalType evacGoalType;
                    WUIEngineColor color = WUIEngineColor.white;

                    //name
                    string[] data = dataLines[0].Split(':');
                    name = data[1].Trim();
                    name = name.Trim('"');

                    //lat, long
                    data = dataLines[1].Split(':');
                    double.TryParse(data[1], out lati);

                    data = dataLines[2].Split(':');
                    double.TryParse(data[1], out longi);

                    //goal type
                    data = dataLines[3].Split(':');
                    exitType = data[1].Trim();
                    exitType = exitType.Trim('"');
                    if (exitType == "Refugee")
                    {
                        evacGoalType = EvacGoalType.Refugee;
                    }
                    else
                    {
                        evacGoalType = EvacGoalType.Exit;
                    }

                    //max flow
                    data = dataLines[4].Split(':');
                    float.TryParse(data[1], out maxFlow);

                    //car capacity
                    data = dataLines[5].Split(':');
                    int.TryParse(data[1], out maxCars);

                    //max people
                    data = dataLines[6].Split(':');
                    int.TryParse(data[1], out maxPeople);

                    //blocked initially?
                    data = dataLines[7].Split(':');
                    blocked = data[1].Trim();
                    blocked = blocked.Trim('"');
                    if (blocked == "false")
                    {
                        initiallyBlocked = false;
                    }
                    else
                    {
                        initiallyBlocked = true;
                    }

                    //color on marker
                    data = dataLines[8].Split(':');
                    data = data[1].Split(',');
                    if (data.Length >= 3)
                    {
                        float.TryParse(data[0], out r);
                        float.TryParse(data[1], out g);
                        float.TryParse(data[2], out b);
                        color = new WUIEngineColor(r, g, b);
                    }

                    EvacuationDestination eG = new EvacuationDestination(name, new Vector2d(lati, longi), color);
                    eG._goalType = evacGoalType;
                    eG._maxFlow = maxFlow;
                    eG._maxCars = maxCars;
                    eG._maxPeople = maxPeople;
                    eG._blocked = initiallyBlocked;

                    evacuationGoals.Add(eG);
                }
                else
                {
                    Engine.MESSAGE(null, Engine.LogType.Warning, "Evacuation goal data file " + path + " not found and could not be loaded.");
                }
            }            

            if (evacuationGoals.Count > 0)
            {
                success = true;
                Engine.MESSAGE(null, Engine.LogType.Log, " " + evacuationGoals.Count + " valid evacuation goal files were succesfully loaded.");               
            }

            return evacuationGoals;
        }
    }
}
