//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using WUIPlatform.Traffic;
using System.IO;

namespace WUIPlatform.Evacuation
{
    [System.Serializable]
    public class EvacuationGoal
    {
        private string _name = "Goal_1";
        public string Name { get => _name; }
        public Vector2d latLon;
        public WUIEngineColor color;
        public bool blocked = false;
        public float maxFlow = 3600f; //cars per hour
        public EvacGoalType goalType = EvacGoalType.Refugee;
        public int maxCars = -1;
        public int maxPeople = -1;
        [System.NonSerialized] public uint currentPeople;
        public List<TrafficModuleCar> cars = new List<TrafficModuleCar>();

        [System.NonSerialized] public float currentFlow = 0f;
        private float firstArrivalTime, currentTimeStep = float.MinValue;
        private int timeStepCars;

        //data for WUI-SHOW etc
        private float _totalTravelTime, _averageTravelTime;

        public EvacuationGoal()
        {
            _name = "New goal";
            latLon = Vector2d.zero;
            color = WUIEngineColor.white;
        }

        public EvacuationGoal(string name, Vector2d latLong, WUIEngineColor color)
        {
            this._name = name;
            this.latLon = latLong;
            this.color = color;
            maxFlow = 3600f;
        }

        public EvacuationGoal(string name, Vector2d latLon, WUIEngineColor color, float maxFlow)
        {
            this._name = name;
            this.latLon = latLon;
            this.color = color;
            this.maxFlow = maxFlow;
        }

        /// <summary>
        /// Checks flow and returns true if car arrives at goal safe and sound, returns false if the car have to wait.
        /// </summary>
        /// <param name="arrivingCar"></param>
        /// <param name="currentTime"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public bool CarArrives(TrafficModuleCar arrivingCar, float currentTime, float deltaTime)
        {
            UpdateFlow(currentTime, deltaTime);            

            //car can arrive
            if((maxFlow <= 0 && !blocked) || (currentFlow < maxFlow && !blocked))
            {         
                //add new cars and people that has arrived during timestep
                ++timeStepCars;
                cars.Add(arrivingCar);
                currentPeople += arrivingCar.numberOfPeopleInCar;
                UpdateCapacity();

                _totalTravelTime += currentTime;
                _averageTravelTime = _totalTravelTime / cars.Count;

                return true;
            }

            return false;
        }

        void UpdateCapacity()
        {
            if (goalType == EvacGoalType.Refugee)
            {
                //track cars and respond
                if (maxCars > 0 && cars.Count >= maxCars && !blocked)
                {
                    blocked = true;
                    WUIEngine.LOG(WUIEngine.LogType.Event, "Evacuation goal " + _name + " has reached cars capacity, re-routing");
                    WUIEngine.SIM.GoalBlocked();
                }
                else if (maxCars > 0 && cars.Count > maxCars)
                {
                    WUIEngine.LOG(WUIEngine.LogType.Log, "Additional car arrived at " + _name + ", arrived during same time step.");
                }

                //track and respond people
                if (maxPeople > -1 && currentPeople >= maxPeople && !blocked)
                {
                    blocked = true;
                    WUIEngine.LOG(WUIEngine.LogType.Event, "Evacuation goal " + _name + " has reached people capacity, re-routing");
                    WUIEngine.SIM.GoalBlocked();
                }
                else if (maxPeople > -1 && currentPeople > maxPeople)
                {
                    WUIEngine.LOG(WUIEngine.LogType.Log, "Additional people arrived at " + _name + ", arrived during same time step.");
                }
            }
        }

        private void UpdateFlow(float timeStamp, float deltaTime)
        {
            //new timestamp?
            if (currentTimeStep != timeStamp)
            {
                currentTimeStep = timeStamp;
                timeStepCars = 0;
            }

            //calc current flow
            if (cars.Count == 0)
            {
                firstArrivalTime = timeStamp;
                currentFlow = 0f;
            }
            else
            {
                float timestepFlow = timeStepCars / deltaTime;
                if (timeStamp == firstArrivalTime)
                {
                    currentFlow = timestepFlow;
                }
                else
                {
                    currentFlow = cars.Count / (timeStamp - firstArrivalTime);
                }
                currentFlow = Mathf.Max(timestepFlow, currentFlow) * 3600f;
            }
        }

        public void ResetPeopleAndCars()
        {
            blocked = false; 

            currentPeople = 0;
            cars.Clear();

            //reset stuff for flow calc
            currentFlow = 0f;
            timeStepCars = 0;
            firstArrivalTime = float.MinValue;
            currentTimeStep = float.MinValue;

            _totalTravelTime = 0f;
            _averageTravelTime = 0f;
        }

        public static List<EvacuationGoal> LoadEvacuationGoalFiles(out bool success)
        {
            success = false;
            List<EvacuationGoal> evacuationGoals = new List<EvacuationGoal>();

            for (int i = 0; i < WUIEngine.INPUT.Evacuation.EvacuationGoalFiles.Length; i++)
            {
                string path = Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Evacuation.EvacuationGoalFiles[i] + ".ed");
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

                    EvacuationGoal eG = new EvacuationGoal(name, new Vector2d(lati, longi), color);
                    eG.goalType = evacGoalType;
                    eG.maxFlow = maxFlow;
                    eG.maxCars = maxCars;
                    eG.maxPeople = maxPeople;
                    eG.blocked = initiallyBlocked;

                    evacuationGoals.Add(eG);
                }
                else
                {
                    WUIEngine.LOG(WUIEngine.LogType.Warning, "Evacuation goal data file " + path + " not found and could not be loaded.");
                }
            }            

            if (evacuationGoals.Count > 0)
            {
                success = true;
                WUIEngine.LOG(WUIEngine.LogType.Log, " " + evacuationGoals.Count + " valid evacuation goal files were succesfully loaded.");               
            }

            return evacuationGoals;
        }
    }
}
