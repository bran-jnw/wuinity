//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using PREACT.Utility;
using System.IO;
using PREACT.Evacuation;
using PREACT.Pedestrian;
using PREACT.Traffic;
using PREACT.Wildfire;
using PREACT.Dispersion;
using PREACT.Input;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using PREACT.Math;
using PREACT.Output;

namespace PREACT
{
    [System.Serializable]
    public class Simulation
    {
        public enum SimulationState { Initializing, Running, Finished, Error };

        //References
        private SimulationState _state;
        private Engine _engine;
        private TimeManager _time;
        private WeatherManager _weather;
        private SpatialManager _spatial;
        private EvacuationManager _evacuation;
        private HazardManager _hazards;
        private PREACTInput _input;
        private SimulationOutput _output;                      

        private List<SimulationModule> _simulationModules;
        private System.Threading.Tasks.Task[] _simulationModuleTasks;

        private Stopwatch _simulationStopWatch = new Stopwatch();
        private Stopwatch _pathfindingStopwatch = new Stopwatch();
        private Stopwatch[] _moduleStopwatches;

        //Data
        private int _simulationIndex;
        private bool _visualize;
        private bool _isPaused = false;
        private bool _stopRun;
        private bool _haveResults = false;
        private float _stepExecutionTime;
        

        //References
        public Engine Engine { get => _engine; }
        public SimulationState State { get => _state; }
        public TimeManager Time { get => _time; }
        public WeatherManager Weather { get => _weather; }
        public SpatialManager Spatial { get => _spatial; }
        public EvacuationManager Evacuation { get => _evacuation; }
        public HazardManager Hazards { get => _hazards; }        
        public PREACTInput Input { get => _input; }
        public SimulationOutput Output { get => _output; }

        //Data
        public int SimulationIndex { get => _simulationIndex; }
        public bool Visualize { get => _visualize; }
        public bool IsPaused { get => _isPaused; }        
        public bool HaveResults { get => _haveResults; }         
        public float SimulationTime { get => _time.SimulationTime; }  
        public float StepExecutionTime { get => _stepExecutionTime; }        
        public Vector2d UTMOrigin { get => _input.Simulation.Data.UTMOrigin; }
        public LatLngUTMConverter.UTMResult UTMData { get => _input.Simulation.Data.UTMData; }


        public Simulation(Engine engine, PREACTInput input, int simulationIndex)
        {
            _engine = engine;
            _input = input;
            _simulationIndex = simulationIndex;
            _time = new TimeManager(_input, this);
            _spatial = new SpatialManager(this);
            _weather = new WeatherManager(this);
            _evacuation = new EvacuationManager(this);
            _hazards = new HazardManager(this);
            _output = new SimulationOutput(this);

            _simulationModules = new List<SimulationModule>();
        }

        /// <summary>
        /// Starts and runs the simulation until completed or halted.
        /// </summary>
        public void Run(bool startWUIshow = false)
        {
            _state = SimulationState.Initializing;
            PreRun();

            if(startWUIshow)
            {
                _engine.StartWUIShow();
            }

            //actual time step loop
            _state = SimulationState.Running;
            while (!_stopRun)
            {
                if (_isPaused)
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    Step();
                }
            }
                        
            PostRun();
            _state = SimulationState.Finished;
        }

        bool _talkToWUIShow = false;
        public void SetAsMainSimulation()
        {
            _talkToWUIShow = true;
        }

        public void SetAsBackgroundSimulation()
        {
            _talkToWUIShow = false;
        }

        /// <summary>
        /// Sets up all modules and timing of simulation.
        /// </summary>
        private void PreRun()
        {            
            _simulationStopWatch.Restart();
            _stopRun = false;
            _stoppedDueToError = false;

            Engine.Message(this, Engine.LogType.Log, "Simulation  " + _simulationIndex + " started, please wait.");

            _weather.Initialize(_time);
            if (_stopRun)
            {
                _state = SimulationState.Error;
                return;
            }

            if (_stopRun)
            {
                _state = SimulationState.Error;
                return;
            }        

            CreateSimulationModules();
            //when creating modules we migth have found an issue
            if (_stopRun)
            {
                _state = SimulationState.Error;
                return;
            }

            //TODO: check that these are within time frame given
            /*foreach (ResponseCurve rC in _input.Evacuation.ResponseCurves.Values)
            {
                float t = rC.DataPoints[0].time + _input.Evacuation.EvacuationOrderStart;
                //_simulationTime = Mathf.Min(SimulationTime, t);
            }*/

            //inject any traffic events into traffic module
            if (_input.TrafficModule.Enabled && _input.TrafficModule.Module == TrafficModuleInput.TrafficModules.MacroTrafficSim)
            {
                for (int i = 0; i < _input.TrafficModule.MacroTrafficSimInput.TrafficAccidents.Count; i++)
                {
                    _evacuation.TrafficModule.InsertNewTrafficEvent(_input.TrafficModule.MacroTrafficSimInput.TrafficAccidents[i]);
                }

                for (int i = 0; i < _input.TrafficModule.MacroTrafficSimInput.ReverseLanes.Count; i++)
                {
                    _evacuation.TrafficModule.InsertNewTrafficEvent(_input.TrafficModule.MacroTrafficSimInput.ReverseLanes[i]);
                }
            }

            //stuff for running threads and timing
            _simulationModuleTasks = new System.Threading.Tasks.Task[_simulationModules.Count];
            _moduleStopwatches = new Stopwatch[_simulationModules.Count];
            for (int i = 0; i < _simulationModules.Count; ++i)
            {
                _moduleStopwatches[i] = new Stopwatch();              
            }

            //do actual simulation steps
            _stopRun = false;
            _state = SimulationState.Running;
            //nextFireUpdate = 0f; //fire start is always 0 seconds

            //only update visuals if doing single run
            //if (!WUI_engine.RUNTIME_DATA.Simulation.MultipleSimulations)
            //{
            _haveResults = true;
            //}
        }

        bool _runRealtime = false;
        private void Step()
        {
            long startTime = _simulationStopWatch.ElapsedMilliseconds;

            UpdateEvents();
            //this state represents the positions at the start of the time step
            if (_talkToWUIShow && _input.WUIShow.SendDataToWUIShow && _evacuation.TrafficModule != null)
            {
                _engine.WUIShow.SendData(_time.SimulationTime);
            }

            //step all modules forward in time
            for(int i = 0; i < _simulationModules.Count; ++i)
            {
                Stopwatch stopwatch = _moduleStopwatches[i];
                SimulationModule module = _simulationModules[i];
                _simulationModuleTasks[i] = System.Threading.Tasks.Task.Run(() => StepModule(stopwatch, module, _time.SimulationTime, _input.Simulation.DeltaTime), _stopThreadsToken.Token);                  
            }
            System.Threading.Tasks.Task.WaitAll(_simulationModuleTasks);

            //handle all damage/impact on road network
            AffectRoadNetwork();

            //inject vehicles from all sources
            HandleNewVehicles();            

            //increase time
            float deltaTime = _input.Simulation.DeltaTime;
            //if only fire running we can take longer steps potentially
            if (_hazards.WildfireModule != null && _input.WildfireModule.Enabled && !_input.PedestrianModule.Enabled && !_input.TrafficModule.Enabled && !_input.SmokeModule.Enabled)
            {
                deltaTime = (float)_hazards.WildfireModule.GetInternalDeltaTime();
            }

            //see if we are done or not
            _time.Step(deltaTime);
            _weather.Step(_time.SimulationTime, _time.CurrentDateTime);
            CheckCompletion();

            if (_input.WildfireModule.Enabled)
            {
                //check if any goal has been blocked by fire, this is done after everything has progressed the current time step
                _evacuation.CheckEvacuationGoalStatus();
                //can get set when evac goals are all gone
                if (_stopRun)
                {
                    return;
                }
            }

            UpdateTiming(startTime, deltaTime);
        }

        private void AffectRoadNetwork()
        {
            //handle any fire effects on road network
            if (_hazards.WildfireModule != null)
            {
                if (_evacuation.TrafficModule != null)
                {
                    _evacuation.TrafficModule.HandleIgnitedFireCells(_hazards.WildfireModule.GetIgnitedFireCells());
                }
                _hazards.WildfireModule.ConsumeIgnitedFireCells();
            }
        }

        private void HandleNewVehicles()
        {
            //handle/inject cars that arrived this timestep
            if (_evacuation.TrafficModule != null)
            {
                _pathfindingStopwatch.Start();
                _evacuation.TrafficModule.HandleNewCars();
                _pathfindingStopwatch.Stop();
            }
        }

        private void UpdateTiming(long startTime, float deltaTime)
        {
            //just some stuff for controlling execution mode and timing performance
            long timeSpent = _simulationStopWatch.ElapsedMilliseconds - startTime;
            if (_runRealtime)
            {
                int sleepTime = (int)deltaTime * 1000 - (int)timeSpent;
                if (sleepTime > 0)
                {
                    Thread.Sleep(sleepTime);
                }
            }
            _stepExecutionTime = 0.01f * timeSpent + 0.99f * _stepExecutionTime;
        }

        private void CheckCompletion()
        {
            if (_stopRun)
            {
                return;
            }

            bool endTimeReached = _time.SimulationEndTime - SimulationTime < 0.001f ? true : false;

            if (endTimeReached)
            {
                Stop("Simulation has reached specified end time.", false);
            }

            if (!_stopRun && _input.Simulation.StopWhenEvacuated)
            {
                bool pedestrianDone = true;
                if (_input.PedestrianModule.Enabled)
                {
                    pedestrianDone = _evacuation.PedestrianModule.IsSimulationDone();
                }
                bool trafficDone = true;
                if (_input.TrafficModule.Enabled)
                {
                    trafficDone = _evacuation.TrafficModule.IsSimulationDone();
                }

                if (pedestrianDone && trafficDone)
                {
                    Stop("Both pedestrian and traffic simulations are completed, stopping as per user settings.", false);
                }
            }
        }

        private void PostRun()
        {
            StopModules();
            _output.AddEvacTime(SimulationTime);           

            if (!_stoppedDueToError)
            {
                _output.SaveOutput();
            }

            if (!_stoppedDueToError)
            {
                _haveResults = true;
                _evacuation.CreateAndRunTriggerBufferModule(this, _input, _weather, _time);
            }

            _simulationStopWatch.Stop();
            Engine.Message(this, Engine.LogType.Log, "Total time spent [s]:" + _simulationStopWatch.ElapsedMilliseconds * 0.001);
            for(int i = 0; i < _moduleStopwatches.Length; ++i)
            {
                Engine.Message(this, Engine.LogType.Log, $"Total time spent in {_simulationModules[i].GetType().Name} [s]:" + _moduleStopwatches[i].ElapsedMilliseconds * 0.001 + string.Format(" [{0}%]", (int)(100.0 * _moduleStopwatches[i].ElapsedMilliseconds / _simulationStopWatch.ElapsedMilliseconds)));
            }            
            Engine.Message(this, Engine.LogType.Log, "Total time spent on initial traffic route pathfinding [s]:" + _pathfindingStopwatch.ElapsedMilliseconds * 0.001 + string.Format(" [{0}%]", (int)(100.0 * _pathfindingStopwatch.ElapsedMilliseconds / _simulationStopWatch.ElapsedMilliseconds)));
            _state = SimulationState.Finished;
            Engine.Message(this, Engine.LogType.Log, " Simulation done.");
            //force garbage collection                
            System.GC.Collect();
        }
        
        private void CreateSimulationModules()
        {
            _state = SimulationState.Initializing;

            List<SimulationModule> createdModules = _hazards.CreateModules(_weather, _time, out bool success);
            if (success)
            {
                _simulationModules.AddRange(createdModules);
            }
            else
            {
                _stopRun = true;
                return;
            }

            createdModules = _evacuation.CreateModules(_weather, _time, out success);
            if (success)
            {
                _simulationModules.AddRange(createdModules);
            }
            else
            {
                _stopRun = true;
                return;
            }

            Engine.Message(this, Engine.LogType.Log, "All requested sub-modules initiated successfully.");
        }          

        public void SetPause(bool pause)
        {
            _isPaused = pause;
        }

        public void TogglePause()
        {
            _isPaused = !_isPaused;
        }

        public void ToogleRealtime()
        {
            _runRealtime = !_runRealtime;
        }

        private void UpdateEvents()
        {
            if (_input.TrafficModule.Enabled)
            {
                //check for global events
                if (_input.Events.Data.BlockDestinationEvents != null)
                {
                    for (int i = 0; i < _input.Events.Data.BlockDestinationEvents.Count; i++)
                    {
                        BlockDestinationEvent bGE = _input.Events.Data.BlockDestinationEvents[i];
                        if (SimulationTime >= bGE.StartTime && !bGE.Triggered)
                        {
                            bGE.ApplyEffects();
                        }
                    }
                }
            }
        }

        private static void StepModule(Stopwatch timer, SimulationModule module, double currentTime, double deltaTime)
        {
            timer.Start();
            module.Step(currentTime, deltaTime);
            timer.Stop();
        }

        CancellationTokenSource _stopThreadsToken = new CancellationTokenSource();
        private void StopModules()
        {
            //_stopThreadsToken.Cancel();

            foreach(SimulationModule sim in _simulationModules)
            {
                sim.Stop();
            }
        }        

        bool _stoppedDueToError = false;
        public bool StoppedDueToError { get => _stoppedDueToError; }
        public void Stop(string stopMessage, bool stoppedDueToError)
        {
            if(!_stopRun)
            {    
                _stopRun = true;                              

                _stoppedDueToError |= stoppedDueToError;
                Engine.Message(this, Engine.LogType.Log, stopMessage);
            }            
        }        
    }    
}