//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using PREACT.Evacuation;
using PREACT.Input;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using PREACT.Output;

namespace PREACT
{
    [System.Serializable]
    public class Simulation
    {
        public enum SimulationState { Initializing, Running, Completed, Error };

        //References
        private Engine _engine;
        private SimulationState _state;
        private Stopwatch _simulationStopwatch = new Stopwatch();
        private PREACTInput _input;
        private SimulationOutput _output;        
        private TimeManager _time;
        private WeatherManager _weather;
        private SpatialManager _spatial;
        private EvacuationManager _evacuation;
        private HazardManager _hazards;                             

        private List<SimulationModule> _simulationModules = new List<SimulationModule>();
        private Stopwatch[] _moduleStopwatches;

        private JobSystem _moduleJobSystem;

        //Data
        private int _simulationIndex;
        private bool _isPaused = false;
        private bool _stopRun;
        private bool _haveResults = false;
        private float _stepExecutionTime;        

        //References
        public Engine Engine { get => _engine; }
        public SimulationState State { get => _state; }
        public PREACTInput Input { get => _input; }
        public SimulationOutput Output { get => _output; }
        public TimeManager Time { get => _time; }
        public WeatherManager Weather { get => _weather; }
        public SpatialManager Spatial { get => _spatial; }
        public EvacuationManager Evacuation { get => _evacuation; }
        public HazardManager Hazards { get => _hazards; }                

        //Data
        public int SimulationIndex { get => _simulationIndex; }
        public bool IsPaused { get => _isPaused; }        
        public bool HaveResults { get => _haveResults; }         
        public float StepExecutionTime { get => _stepExecutionTime; }                


        public Simulation(Engine engine, PREACTInput input, int simulationIndex)
        {
            _engine = engine;
            _simulationIndex = simulationIndex;
            _input = input;
            _output = new SimulationOutput(this);            
            _time = new TimeManager(_input, this);
            _spatial = new SpatialManager(this);
            _weather = new WeatherManager(this, _time);
            _hazards = new HazardManager(this);
            _evacuation = new EvacuationManager(this);              
        }

        /// <summary>
        /// Starts and runs the simulation until completed or halted.
        /// </summary>
        public void Run(bool startWUIshow = false)
        {
            _state = SimulationState.Initializing;
            PreRun(startWUIshow);            

            //actual time step loop
            _state = SimulationState.Running;
            _haveResults = true;
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
            _state = SimulationState.Completed;
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
        private void PreRun(bool startWUIShow)
        {
            if (startWUIShow)
            {
                _engine.StartWUIShow();
            }

            _simulationStopwatch.Restart();
            _stopRun = false;
            _stoppedDueToError = false;

            Engine.Message(this, Engine.LogType.Log, "Simulation  " + _simulationIndex + " started, please wait."); 

            CreateSimulationModules();
            //when creating modules we might have found an issue
            if (_stopRun)
            {
                _state = SimulationState.Error;
                return;
            }

            //TODO: basically get rid of
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
        }

        bool _runRealtime = false;
        private void Step()
        {
            long startTime = _simulationStopwatch.ElapsedMilliseconds;

            //update weather for current time step
            _weather.Update(_time.CurrentDateTime);

            //this state represents the positions at the start of the time step
            if (_talkToWUIShow && _input.WUIShow.SendDataToWUIShow && _evacuation.TrafficModule != null)
            {
                _engine.WUIShow.SendData(_time.SimulationTime);
            }

            UpdateEvents();

            //step all modules forward in time            
            float deltaTime = _input.Simulation.DeltaTime;
            //if only fire running we can take longer steps potentially
            /*if (_hazards.WildfireModule != null && _input.WildfireModule.Enabled && !_input.PedestrianModule.Enabled && !_input.TrafficModule.Enabled && !_input.SmokeModule.Enabled)
            {
                deltaTime = (float)_hazards.WildfireModule.GetInternalDeltaTime();
            }*/
            for (int i = 0; i < _simulationModules.Count; ++i)
            {
                SimulationModule module = _simulationModules[i];
                if (!module.IsSimulationDone())
                {
                    Stopwatch stopwatch = _moduleStopwatches[i];                    
                    _moduleJobSystem.Schedule(() => StepModule(stopwatch, module, _time.SimulationTime, deltaTime));
                }                
            }
            _moduleJobSystem.ExecuteJobs();

            //advance time            
            _time.Step(deltaTime);

            //deal with what has happen during time step
            _evacuation.UpdateConsequences();

            //see if we are done or not   
            CheckCompletion();
            UpdatePerformanceTimer(startTime, deltaTime);
        }

        private void CheckCompletion()
        {
            if (_stopRun)
            {
                return;
            }

            bool endTimeReached = _time.SimulationEndTime - _time.SimulationTime < 0.001f ? true : false;

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

        private void UpdatePerformanceTimer(long startTime, float deltaTime)
        {
            //just some stuff for controlling execution mode and timing performance
            long timeSpent = _simulationStopwatch.ElapsedMilliseconds - startTime;
            if (_runRealtime)
            {
                int sleepTime = (int)deltaTime * 1000 - (int)timeSpent;
                if (sleepTime > 0)
                {
                    Thread.Sleep(sleepTime);
                }
            }

            if(timeSpent > 100) //for when one can actually see time taken
            {
                _stepExecutionTime = timeSpent;
            }
            else
            {
                _stepExecutionTime = 0.01f * timeSpent + 0.99f * _stepExecutionTime;
            }                
        }

        private void PostRun()
        {
            StopModules();                     

            if (!_stoppedDueToError)
            {
                _output.AddEvacTime(_time.SimulationTime);
                _output.SaveOutput();
                _haveResults = true;
                _evacuation.CreateAndRunTriggerBufferModule(this, _input, _weather, _time);
            }

            _simulationStopwatch.Stop();
            Engine.Message(this, Engine.LogType.Log, "Total time spent [s]:" + _simulationStopwatch.ElapsedMilliseconds * 0.001);
            if(_moduleStopwatches != null)
            {
                for (int i = 0; i < _moduleStopwatches.Length; ++i)
                {
                    Engine.Message(this, Engine.LogType.Log, $"Total time spent in {_simulationModules[i].GetType().Name} [s]:" + _moduleStopwatches[i].ElapsedMilliseconds * 0.001 + string.Format(" [{0}%]", (int)(100.0 * _moduleStopwatches[i].ElapsedMilliseconds / _simulationStopwatch.ElapsedMilliseconds)));
                }
            }            
            _evacuation.PostRun(_simulationStopwatch);

            _state = SimulationState.Completed;
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
                Engine.Message(this, Engine.LogType.Log, "All requested hazard modules initiated successfully.");
            }
            else
            {
                _stopRun = true;
                Engine.Message(this, Engine.LogType.Log, "Failed to create all requested hazard modules, aborting.");
                return;
            }

            createdModules = _evacuation.CreateModules(_weather, _time, out success);
            if (success)
            {
                _simulationModules.AddRange(createdModules);
                Engine.Message(this, Engine.LogType.Log, "All requested evacuation modules initiated successfully.");
            }
            else
            {
                _stopRun = true;
                Engine.Message(this, Engine.LogType.Log, "Failed to create all requested evacuation modules, aborting.");
                return;
            }

            //stuff for running threads and timing
            _moduleStopwatches = new Stopwatch[_simulationModules.Count];
            for (int i = 0; i < _simulationModules.Count; ++i)
            {
                _moduleStopwatches[i] = new Stopwatch();
            }
            _moduleJobSystem = new JobSystem(System.Math.Min(System.Environment.ProcessorCount, _simulationModules.Count));

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

        public void ToggleRealtime()
        {
            _runRealtime = !_runRealtime;
        }

        //TODO: move
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
                        if (_time.SimulationTime >= bGE.StartTime && !bGE.Triggered)
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

        private void StopModules()
        {
            foreach(SimulationModule sim in _simulationModules)
            {
                sim.Stop();
            }

            if(_moduleJobSystem != null)
            {
                _moduleJobSystem.Dispose();
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