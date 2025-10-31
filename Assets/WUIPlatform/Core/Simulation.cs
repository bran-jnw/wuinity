//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using PREACT.Utility.Math;
using PREACT.Evacuation;
using PREACT.Pedestrian;
using PREACT.Traffic;
using PREACT.Fire;
using PREACT.Smoke;
using PREACT.IO;
using System.Threading;

namespace PREACT
{
    [System.Serializable]
    public class Simulation
    {    
        public enum SimulationState { Initializing, Running, Finished, Error};
        private SimulationState _state;
        public SimulationState State { get => _state; }

        private bool _isPaused = false;
        public bool IsPaused { get => _isPaused; }

        bool _haveResults = false;
        public bool HaveResults { get => _haveResults; }

        private bool _stopRun;

        private TrafficModule _trafficModule;
        public TrafficModule TrafficModule { get => _trafficModule; }

        private PedestrianModule _pedestrianModule;
        public PedestrianModule PedestrianModule { get => _pedestrianModule; }

        private FireModule _fireModule;
        public FireModule FireModule { get => _fireModule; }

        private SmokeModule _smokeModule;
        public SmokeModule SmokeModule { get =>_smokeModule; }

        private float _startTime;
        public float StartTime { get => _startTime; }

        private float _currentTime;
        public float CurrentTime { get => _currentTime; } 
                
        private float _stepExecutionTime;
        public float StepExecutionTime { get => _stepExecutionTime; }
                
        float[,] _triggerBufferDataOutput;

        private System.Diagnostics.Stopwatch _simulationStopWatch = new System.Diagnostics.Stopwatch();
        private System.Diagnostics.Stopwatch _trafficStopwatch = new System.Diagnostics.Stopwatch();
        private System.Diagnostics.Stopwatch _pedestrianStopwatch = new System.Diagnostics.Stopwatch();
        private System.Diagnostics.Stopwatch _fireStopwatch = new System.Diagnostics.Stopwatch();
        private System.Diagnostics.Stopwatch _smokeStopwatch = new System.Diagnostics.Stopwatch();
        private System.Diagnostics.Stopwatch _pathfindingStopwatch = new System.Diagnostics.Stopwatch();

        Input _input;
        public Input Input { get => _input; }
        int _simulationIndex;
        public int SimulationIndex { get => _simulationIndex; }
        private Engine _engine;
        public Engine Engine { get => _engine; }


        public Simulation(Engine engine, Input input, int simulationId)
        {
            _engine = engine;
            _input = input;
            _simulationIndex = simulationId;
        }

        /// <summary>
        /// Starts and runs the simulation until completed or halted.
        /// </summary>
        public void Run()
        {
            PreRun();

            //actual time step loop
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

            StopModules();
            _engine.Output.AddEvacTime(CurrentTime);

            if (!_stoppedDueToError)
            {
                SaveOutput();
            }

            //force garbage collection                
            //System.GC.Collect();
            PostRun();
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
            _trafficStopwatch.Reset();
            _pedestrianStopwatch.Reset();
            _fireStopwatch.Reset();
            _smokeStopwatch.Reset();
            _stopRun = false;
            _stoppedDueToError = false;

            Message(Engine.LogType.Log, "Simulation  " + _simulationIndex + " started, please wait.");
            CreateSubModules();
            //when creating modules we migth have found an issue
            if (_stopRun)
            {
                _state = SimulationState.Error;
                return;
            }            

            //if we do multiple runs the goals have to be reset
            /*for (int i = 0; i < _engine.RuntimeData.Evacuation.Destinations.Count; i++)
            {
                _engine.RuntimeData.Evacuation.Destinations[i].ResetPeopleAndCars();
            }*/

            //pick start time based on curve or 0 (fire start)
            _currentTime = 0f;
            for (int i = 0; i < _engine.RuntimeData.Evacuation.ResponseCurves.Length; i++)
            {
                float t = _engine.RuntimeData.Evacuation.ResponseCurves[i].dataPoints[0].time + _engine.Input.Evacuation.EvacuationOrderStart;
                _currentTime = Mathf.Min(CurrentTime, t);
            }
            _startTime = CurrentTime;

            //inject any traffic events into traffic module
            if (_engine.Input.Simulation.RunTrafficModule && _engine.Input.Traffic.TrafficModule == TrafficInput.TrafficModuleChoice.MacroTrafficSim)
            {
                for (int i = 0; i < _engine.Input.Traffic.MacroTrafficSimInput.TrafficAccidents.Length; i++)
                {
                    _trafficModule.InsertNewTrafficEvent(_engine.Input.Traffic.MacroTrafficSimInput.TrafficAccidents[i]);
                }

                for (int i = 0; i < _engine.Input.Traffic.MacroTrafficSimInput.ReverseLanes.Length; i++)
                {
                    _trafficModule.InsertNewTrafficEvent(_engine.Input.Traffic.MacroTrafficSimInput.ReverseLanes[i]);
                }
            }

            //do actual simulation steps
            _stopRun = false;
            _state = SimulationState.Running;
            nextFireUpdate = 0f; //fire start is always 0 seconds

            //only update visuals if doing single run
            //if (!WUI_engine.RUNTIME_DATA.Simulation.MultipleSimulations)
            //{
            _haveResults = true;
            //}
        }

        private void PostRun()
        {
            if (!_stoppedDueToError)
            {
                _haveResults = true;

                if (_engine.Input.TriggerBuffer.CalculateTriggerBuffer)
                {
                    if (_engine.Input.TriggerBuffer.TriggerBuffer == TriggerBufferInput.TriggerBufferChoice.kPERIL)
                    {
                        if (_engine.Input.TriggerBuffer.kPERILInput.CalculateROSFromBehave || _fireModule != null)
                        {
                            _triggerBufferDataOutput = WUIPlatformPERIL.RunPERIL(_engine.Input.TriggerBuffer.kPERILInput.MidflameWindspeed);
                        }
                    }
                }
            }

            _simulationStopWatch.Stop();
            Message(Engine.LogType.Log, "Total time spent [s]:" + _simulationStopWatch.ElapsedMilliseconds * 0.001);
            Message(Engine.LogType.Log, "Total time spent in pedestrian module [s]:" + _pedestrianStopwatch.ElapsedMilliseconds * 0.001 + string.Format(" [{0}%]", (int)(100.0 * _pedestrianStopwatch.ElapsedMilliseconds / _simulationStopWatch.ElapsedMilliseconds)));
            Message(Engine.LogType.Log, "Total time spent in traffic module [s]:" + _trafficStopwatch.ElapsedMilliseconds * 0.001 + string.Format(" [{0}%]", (int)(100.0 * _trafficStopwatch.ElapsedMilliseconds / _simulationStopWatch.ElapsedMilliseconds)));
            Message(Engine.LogType.Log, "Total time spent in fire module [s]:" + _fireStopwatch.ElapsedMilliseconds * 0.001 + string.Format(" [{0}%]", (int)(100.0 * _fireStopwatch.ElapsedMilliseconds / _simulationStopWatch.ElapsedMilliseconds)));
            Message(Engine.LogType.Log, "Total time spent in smoke module [s]:" + _smokeStopwatch.ElapsedMilliseconds * 0.001 + string.Format(" [{0}%]", (int)(100.0 * _smokeStopwatch.ElapsedMilliseconds / _simulationStopWatch.ElapsedMilliseconds)));
            Message(Engine.LogType.Log, "Total time spent on initial traffic route pathfinding [s]:" + _pathfindingStopwatch.ElapsedMilliseconds * 0.001 + string.Format(" [{0}%]", (int)(100.0 * _pathfindingStopwatch.ElapsedMilliseconds / _simulationStopWatch.ElapsedMilliseconds)));
            _state = SimulationState.Finished;
            Message(Engine.LogType.Log, " Simulation done.");            
        }
        
        private void CreateSubModules()
        {
            _state = SimulationState.Initializing;

            CreateFireModule();
            if (_stopRun)
            {
                return;
            }
            
            CreateSmokeModule();
            if (_stopRun)
            {
                return;
            }

            CreatePedestrianModule();
            if (_stopRun)
            {
                return;
            }

            CreateTrafficModule();
            if (_stopRun)
            {
                return;
            }

            Message(Engine.LogType.Log, "All requested sub-modules initiated successfully.");
        }

        private void CreateFireModule()
        {            
            if (_engine.Input.Simulation.RunFireModule)
            {
                if (_engine.Input.Fire.FireModule == FireInput.FireModuleChoice.AscImport)
                {
                    _fireModule = new AscFireImport(this);
                    Message(Engine.LogType.Log, "Fire module AscImport initiated.");
                }
                else if(_engine.Input.Fire.FireModule == FireInput.FireModuleChoice.FireCell)
                {
                    _fireModule = new FireMesh(this, _engine.RuntimeData.Fire.LCPData, _engine.RuntimeData.Fire.WeatherInput, _engine.RuntimeData.Fire.WindInput, _engine.RuntimeData.Fire.InitialFuelMoistureData, _engine.RuntimeData.Fire.IgnitionPoints);
                    Message(Engine.LogType.Log, "Fire module FireCell initiated.");
                }
                else
                {
                    Message(Engine.LogType.SimError, "Could not initiate fire mdoule, aborting.");
                }
            }  
            else
            {
                Message(Engine.LogType.Log, "No fire module was enabled.");
            }
        }

        private void CreateSmokeModule()
        {
            //can only run together
            if (_engine.Input.Simulation.RunSmokeModule)
            {
                //this module does not need the fire
                if (_engine.Input.Smoke.SmokeModule == SmokeInput.SmokeModuleChoice.GlobalSmoke)
                {
                    string file = System.IO.Path.Combine(_engine.WorkingFolder, _engine.Input.Smoke.GlobalSmokeInput.ExtinctionFile);
                    _smokeModule = new GlobalSmoke(this, file);
                    return;
                }                

                if (!_engine.Input.Simulation.RunFireModule)
                {
                    Message(Engine.LogType.SimError, "Smoke module that needs fire as source was enabled but no fire module was enabled, aborting.");
                }
                else
                {                       
                    if(_engine.Input.Smoke.SmokeModule == SmokeInput.SmokeModuleChoice.AdvectDiffuseMixingLayer)
                    {
                        _smokeModule = new AdvectDiffuseMixingLayer(this);
                    }
                    else if (_engine.Input.Smoke.SmokeModule == SmokeInput.SmokeModuleChoice.AdvectDiffuse3D)
                    {
                        /*if (_smokeModule != null)
                        {
                            ((Smoke.AdvectDiffuseModel)_smokeModule).Release();
                        }
                        //_smokeModule = new Smoke.AdvectDiffuseModel(_fireModule, 250f, WUInity.INSTANCE.AdvectDiffuseCompute, WUInity.INSTANCE.NoiseTex, WUInity.INSTANCE.WindTex);*/                            
                        Message(Engine.LogType.Log, "Smoke module AdvectDiffuse initiated.");
                    }
                    else if (_engine.Input.Smoke.SmokeModule == SmokeInput.SmokeModuleChoice.BoxModel)
                    {
                        //smokeBoxDispersionModel = new Smoke.BoxDispersionModel(fireMesh);
                    }                  
                }
            } 
            else
            {
                Message(Engine.LogType.Log, "No smoke module was enabled.");
            }
        }

        private void CreatePedestrianModule()
        {

            if (_input.Simulation.RunPedestrianModule)
            {
                if (_engine.Input.Pedestrian.PedestrianModule == PedestrianInput.PedestrianModuleChoice.JupedSimSUMO)
                {
                    //placeholder for JupedSim
                }
                else if (_input.Pedestrian.PedestrianModule == PedestrianInput.PedestrianModuleChoice.MacroHouseholdSim)
                {
                    _pedestrianModule = new MacroHouseholdSim(this);
                    MacroHouseholdSim macroHouseholdSim = (MacroHouseholdSim)_pedestrianModule;
                    //place people
                    //macroHouseholdSim.PopulateCells(WUI_engine.RUNTIME_DATA.Routing.RouteCollections, WUI_engine.POPULATION.GetPopulationData());
                    macroHouseholdSim.PopulateSimulation(_engine.RuntimeData.Population.Households);
                    Message(Engine.LogType.Log, "Pedestrian module MacroPedestrianSim initiated.");
                }
            }
            else
            {
                Message(Engine.LogType.Log, "No pedestrian module was enabled.");
            }
        }

        private void CreateTrafficModule()
        {
            if (_engine.Input.Simulation.RunTrafficModule)
            {
                if (_engine.Input.Traffic.TrafficModule == TrafficInput.TrafficModuleChoice.SUMO)
                {
                    bool success;
                    _trafficModule = new SUMOModule(this, out success);
                    if(success)
                    {
                        Message(Engine.LogType.Log, "Traffic module SUMO initiated.");
                    }  
                    else
                    {
                        _stopRun = true;
                    }
                }
                else
                {
                    _trafficModule = new MacroTrafficSim(this);
                    Message(Engine.LogType.Log, "Traffic module MacroTrafficSim initiated.");
                }
            }
            else
            {
                Message(Engine.LogType.Log, "No traffic module was enabled.");
            }
        }
        
        bool _runRealtime = false;
        private void Step()
        {  
            long startTime = _simulationStopWatch.ElapsedMilliseconds;
            UpdateEvents();
            //this state represents the positions at the start of the time step
            if (_talkToWUIShow && _input.WUIShow.SendDataToWUIShow && _trafficModule != null)
            {
                _engine.WUIShow.SendData(_currentTime);
            }

            //step all modules forward in time
            System.Threading.Tasks.Task fireTask = System.Threading.Tasks.Task.Run(StepFireModule, _stopThreadsToken.Token);
            System.Threading.Tasks.Task smokeTask =  System.Threading.Tasks.Task.Run(StepSmokeModule, _stopThreadsToken.Token);
            System.Threading.Tasks.Task pedestrianTask = System.Threading.Tasks.Task.Run(StepPedestrianModule, _stopThreadsToken.Token);
            System.Threading.Tasks.Task trafficTask = System.Threading.Tasks.Task.Run(StepTrafficModule, _stopThreadsToken.Token);

            System.Threading.Tasks.Task.WaitAll(fireTask, smokeTask,pedestrianTask, trafficTask);

            //handle any fire effects on road network
            if (_fireModule != null)
            {
                if(_trafficModule != null)
                {
                    _trafficModule.HandleIgnitedFireCells(_fireModule.GetIgnitedFireCells());
                }     
                _fireModule.ConsumeIgnitedFireCells();
            }

            //handle/inject cars that arrived this timestep
            if (_trafficModule != null)
            {
                _pathfindingStopwatch.Start();
                _trafficModule.HandleNewCars();
                _pathfindingStopwatch.Stop();
            }                

            //increase time
            float deltaTime = _engine.Input.Simulation.DeltaTime;
            //if only fire running we can take longer steps potentially
            if (_engine.Input.Simulation.RunFireModule && !_engine.Input.Simulation.RunPedestrianModule && !_engine.Input.Simulation.RunTrafficModule && !_engine.Input.Simulation.RunSmokeModule)
            {
                deltaTime = (float)_fireModule.GetInternalDeltaTime();
            }
            _currentTime += deltaTime;

            //see if we are done or not
            CheckCompletion();

            if (_engine.Input.Simulation.RunFireModule)
            {
                //check if any goal has been blocked by fire, this is done after everything has progressed the current time step
                CheckEvacuationGoalStatus();
                //can get set when evac goals are all gone
                if (_stopRun)
                {
                    return;
                }
            }

            //just some stuff for controlling execution mode and timing performance
            long timeSpent =  _simulationStopWatch.ElapsedMilliseconds - startTime;
            if(_runRealtime)
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

            bool endTimeReached = CurrentTime <= _engine.Input.Simulation.MaxSimTime ? false : true;

            if(endTimeReached)
            {
                Stop("Simulation has reached specified end time.", false);
            }

            if (!_stopRun && _engine.Input.Simulation.StopWhenEvacuated)
            {
                bool pedestrianDone = true;
                if (_engine.Input.Simulation.RunPedestrianModule)
                {
                    pedestrianDone = _pedestrianModule.IsSimulationDone();
                }
                bool trafficDone = true;
                if (_engine.Input.Simulation.RunTrafficModule)
                {
                    trafficDone = _trafficModule.IsSimulationDone();
                }

                if (pedestrianDone && trafficDone)
                {
                    Stop("Both pedestrian and traffic simulations are done, stopping as per user settings.", false);
                }
            }
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
            Input input = _engine.Input;

            if (input.Simulation.RunTrafficModule)
            {
                //check for global events
                if (_engine.RuntimeData.Evacuation.BlockGoalEvents != null)
                {
                    for (int i = 0; i < _engine.RuntimeData.Evacuation.BlockGoalEvents.Length; i++)
                    {
                        BlockGoalEvent bGE = _engine.RuntimeData.Evacuation.BlockGoalEvents[i];
                        if (CurrentTime >= bGE.startTime && !bGE.triggered)
                        {
                            bGE.ApplyEffects();
                        }
                    }
                }
            }
        }

        bool fireUpdated = false;
        float nextFireUpdate;
        private void StepFireModule()
        {
            //update fire mesh if needed
            fireUpdated = false;
            if (_engine.Input.Simulation.RunFireModule)
            {
                if (CurrentTime >= nextFireUpdate && CurrentTime >= 0.0f)
                {
                    fireUpdated = true;
                    _fireStopwatch.Start();
                    _fireModule.Step(_currentTime, _engine.Input.Simulation.DeltaTime);
                    _fireStopwatch.Stop();
                    nextFireUpdate += _fireModule.GetInternalDeltaTime();
                    // Route analysis: consider calling RoutingData::ModifyRouterDB at this point if the fire interferes with the road network
                    // Note: we need to preprocess each cell which has a road on it
                }
            }
        }

        private void StepSmokeModule()
        {
            //sync with fire
            if (_engine.Input.Simulation.RunSmokeModule && CurrentTime >= 0.0f)
            {
                _smokeStopwatch.Start();
                if (_engine.Input.Smoke.SmokeModule == SmokeInput.SmokeModuleChoice.BoxModel)
                {
                    //smokeBoxDispersionModel.Update(input.deltaTime, fireMesh.currentWindData.direction, fireMesh.currentWindData.speed);
                }
                else
                {
                    _smokeModule.Step(_currentTime, _engine.Input.Simulation.DeltaTime);
                }
                _smokeStopwatch.Stop();
            }
        }

        private void StepPedestrianModule()
        {
            //advance pedestrian
            if (_engine.Input.Simulation.RunPedestrianModule)
            {
                _pedestrianStopwatch.Start();
                _pedestrianModule.Step(CurrentTime, _engine.Input.Simulation.DeltaTime);
                _pedestrianStopwatch.Stop();
            }
        }

        private void StepTrafficModule()
        {
            //advance traffic
            if (_engine.Input.Simulation.RunTrafficModule)
            {
                _trafficStopwatch.Start();
                _trafficModule.Step(_engine.Input.Simulation.DeltaTime, CurrentTime);
                _trafficStopwatch.Stop();
            }
        }


        CancellationTokenSource _stopThreadsToken = new CancellationTokenSource();
        private void StopModules()
        {
            //_stopThreadsToken.Cancel();

            if (_engine.Input.Simulation.RunPedestrianModule && _pedestrianModule != null)
            {
                _pedestrianModule.Stop();
            }

            if (_engine.Input.Simulation.RunTrafficModule && _trafficModule != null)
            {
                _trafficModule.Stop();
            }

            if (_engine.Input.Simulation.RunFireModule && _fireModule != null)
            {
                _fireModule.Stop();
            }

            if (_engine.Input.Simulation.RunSmokeModule && _smokeModule != null)
            {
                _smokeModule.Stop();
            }
        }

        void CheckEvacuationGoalStatus()
        {
            for (int i = 0; i < _engine.RuntimeData.Evacuation.Destinations.Count; i++)
            {
                EvacuationDestination eG = _engine.RuntimeData.Evacuation.Destinations[i];
                if(!eG.Blocked)
                {
                    FireCellState cellState = _fireModule.GetFireCellState(eG.LatLon);
                    if (cellState == FireCellState.Burning)
                    {
                        Message(Engine.LogType.Log, " Goal blocked by fire: " + eG.Name);
                        BlockEvacGoal(i);
                    }
                }                
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
                Message(Engine.LogType.Log, stopMessage);
            }            
        }

        public void InsertNewCar(Vector2d startLatLon, EvacuationDestination evacuationGoal, uint numberOfPeopleInCar)
        {
            if(_trafficModule != null)
            {
                _trafficModule.InsertNewCar(startLatLon, evacuationGoal, numberOfPeopleInCar);
            }
        }

        public void BlockEvacGoal(int index)
        {
            if (!_engine.RuntimeData.Evacuation.Destinations[index].Blocked)
            {
                _engine.RuntimeData.Evacuation.Destinations[index].Blocked = true;
                UpdateRoutes();
            }
        }        

        /// <summary>
        /// Called from goal when blocked internally.
        /// </summary>
        public void GoalBlocked()
        {
            UpdateRoutes();
        }

        private void UpdateRoutes(MacroTrafficSim externalMacroTrafficSim = null)
        {
            //check that we have at least one goal left
            bool allBlocked = true;
            for (int i = 0; i < _engine.RuntimeData.Evacuation.Destinations.Count; i++)
            {
                if(!_engine.RuntimeData.Evacuation.Destinations[i]._blocked)
                {
                    allBlocked = false;
                    break;
                }
            }
            if(allBlocked)
            {
                Stop("No evacuation goals available, stopping simulation.", false);
                return;
            }

            //update raster evac routes first as traffic might use some of the updated choices
            //TODO

            //update cars already in traffic
            _trafficModule.UpdateEvacuationGoals();              
        }        

        void SaveOutput()
        {
            Input input = _engine.Input;
            if (input.Simulation.RunTrafficModule)
            {
                Message(Engine.LogType.Log, " Total cars in simulation: " + _trafficModule.GetTotalCarsSimulated());
                _trafficModule.SaveToFile(_simulationIndex);
            }
            if (input.Simulation.RunPedestrianModule)
            {
                if (_engine.Input.Pedestrian.PedestrianModule == PedestrianInput.PedestrianModuleChoice.MacroHouseholdSim)
                {
                    MacroHouseholdSim mHS = (MacroHouseholdSim)_pedestrianModule;
                    mHS.SaveToFile(_simulationIndex);
                }                    
            }                        
        }

        public void Message(Engine.LogType logType, string message)
        {
            if(_engine != null)
            {
                _engine.Message(this, logType, message);
            }   
        }

        public float[,] GetTriggerBufferData()
        {
            if (_triggerBufferDataOutput != null)
            {
                return _triggerBufferDataOutput;
            }

            else return null;
        }

        public void SetTriggerBufferData(float[,] data)
        {
            _triggerBufferDataOutput = data;
        }

        public void DisplayTriggerBuffer()
        {
            if(_triggerBufferDataOutput != null)
            {
                _engine.RuntimeData.Fire.Visualizer.CreateTriggerBufferVisuals(_triggerBufferDataOutput);
                _engine.RuntimeData.Fire.Visualizer.SetLCPViewMode(Visualization.FireDataVisualizer.LcpViewMode.TriggerBuffer);
                _engine.RuntimeData.Fire.Visualizer.SetLCPDataPlane(true);
            }            
        }

    }    
}