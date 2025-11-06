//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;
using PREACT.Utility.Math;
using PREACT.Evacuation;
using PREACT.Pedestrian;
using PREACT.Traffic;
using PREACT.Fire;
using PREACT.Smoke;
using PREACT.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

namespace PREACT
{
    [System.Serializable]
    public class Simulation
    {
        public enum SimulationState { Initializing, Running, Finished, Error };

        //References
        private Engine _engine;
        private PREACTScenario _scenario;
        private SimulationState _state;
        private TrafficModule _trafficModule;
        private PedestrianModule _pedestrianModule;
        private FireModule _fireModule;
        private SmokeModule _smokeModule;
        private Stopwatch _simulationStopWatch = new Stopwatch();
        private Stopwatch _trafficStopwatch = new Stopwatch();
        private Stopwatch _pedestrianStopwatch = new Stopwatch();
        private Stopwatch _fireStopwatch = new Stopwatch();
        private Stopwatch _smokeStopwatch = new Stopwatch();
        private Stopwatch _pathfindingStopwatch = new Stopwatch();

        //Data
        private int _simulationIndex;
        private float _startTime;
        private float _currentTime;
        private bool _isPaused = false;
        private bool _stopRun;
        private bool _haveResults = false;
        private float _stepExecutionTime;
        private List<EvacuationDestination> _evacuationDestinations;

        private float[,] _triggerBufferDataOutput;//TODO:move to somewhere else
        
        //References
        public Engine Engine { get => _engine; }
        public SimulationState State { get => _state; }
        public PedestrianModule PedestrianModule { get => _pedestrianModule; }
        public TrafficModule TrafficModule { get => _trafficModule; }
        public FireModule FireModule { get => _fireModule; }
        public SmokeModule SmokeModule { get => _smokeModule; }
        public PREACTScenario Scenario { get => _scenario; }

        //Data
        public int SimulationIndex { get => _simulationIndex; }        
        public bool IsPaused { get => _isPaused; }        
        public bool HaveResults { get => _haveResults; }          
        public float StartTime { get => _startTime; }        
        public float CurrentTime { get => _currentTime; }  
        public float StepExecutionTime { get => _stepExecutionTime; }        
        public List<EvacuationDestination> Destinations { get => _evacuationDestinations; }

        public Simulation(Engine engine, PREACTScenario scenario, int simulationId)
        {
            _engine = engine;
            _scenario = scenario;
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

            Engine.MESSAGE(this, Engine.LogType.Log, "Simulation  " + _simulationIndex + " started, please wait.");

            _evacuationDestinations = EvacuationDestination.CreateEvacacuationDestinations(this, _scenario.Data.Evacuation.EvacuationDestinationInputs);
            if (_stopRun)
            {
                _state = SimulationState.Error;
                return;
            }

            CreateSubModules();
            //when creating modules we migth have found an issue
            if (_stopRun)
            {
                _state = SimulationState.Error;
                return;
            }

            //pick start time based on curve or 0 (fire start)
            _currentTime = 0f;
            for (int i = 0; i < _scenario.Data.Evacuation.ResponseCurves.Count; i++)
            {
                float t = _scenario.Data.Evacuation.ResponseCurves[i].dataPoints[0].time + _scenario.Input.Evacuation.EvacuationOrderStart;
                _currentTime = Mathf.Min(CurrentTime, t);
            }
            _startTime = CurrentTime;

            //inject any traffic events into traffic module
            if (_scenario.Input.Simulation.RunTrafficModule && _scenario.Input.Traffic.TrafficModule == TrafficInput.TrafficModuleChoice.MacroTrafficSim)
            {
                for (int i = 0; i < _scenario.Input.Traffic.MacroTrafficSimInput.TrafficAccidents.Length; i++)
                {
                    _trafficModule.InsertNewTrafficEvent(_scenario.Input.Traffic.MacroTrafficSimInput.TrafficAccidents[i]);
                }

                for (int i = 0; i < _scenario.Input.Traffic.MacroTrafficSimInput.ReverseLanes.Length; i++)
                {
                    _trafficModule.InsertNewTrafficEvent(_scenario.Input.Traffic.MacroTrafficSimInput.ReverseLanes[i]);
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
            StopModules();
            _engine.Output.AddEvacTime(CurrentTime);

            //force garbage collection                
            //System.GC.Collect();

            if (!_stoppedDueToError)
            {
                SaveOutput();
            }

            if (!_stoppedDueToError)
            {
                _haveResults = true;

                if (_scenario.Input.TriggerBuffer.CalculateTriggerBuffer)
                {
                    if (_scenario.Input.TriggerBuffer.TriggerBuffer == TriggerBufferInput.TriggerBufferChoice.kPERIL)
                    {
                        if (_scenario.Input.TriggerBuffer.kPERILInput.CalculateROSFromBehave || _fireModule != null)
                        {
                            string outputFile = Path.Combine(_engine.OutputFolder, _scenario.Input.Simulation.Name + _simulationIndex + ".tiff");
                            _triggerBufferDataOutput = WUIPlatformPERIL.RunPERIL(_scenario.Data.Fire.LCPData, _scenario.Data.Fire.WuiArea, _scenario.Input.TriggerBuffer.kPERILInput.MidflameWindspeed, 0f, _currentTime, outputFile, _scenario.Data.Fire.InitialFuelMoistureData, _scenario.Data.Fire.FuelModelsData);
                        }
                    }
                }
            }

            _simulationStopWatch.Stop();
            Engine.MESSAGE(this, Engine.LogType.Log, "Total time spent [s]:" + _simulationStopWatch.ElapsedMilliseconds * 0.001);
            Engine.MESSAGE(this, Engine.LogType.Log, "Total time spent in pedestrian module [s]:" + _pedestrianStopwatch.ElapsedMilliseconds * 0.001 + string.Format(" [{0}%]", (int)(100.0 * _pedestrianStopwatch.ElapsedMilliseconds / _simulationStopWatch.ElapsedMilliseconds)));
            Engine.MESSAGE(this, Engine.LogType.Log, "Total time spent in traffic module [s]:" + _trafficStopwatch.ElapsedMilliseconds * 0.001 + string.Format(" [{0}%]", (int)(100.0 * _trafficStopwatch.ElapsedMilliseconds / _simulationStopWatch.ElapsedMilliseconds)));
            Engine.MESSAGE(this, Engine.LogType.Log, "Total time spent in fire module [s]:" + _fireStopwatch.ElapsedMilliseconds * 0.001 + string.Format(" [{0}%]", (int)(100.0 * _fireStopwatch.ElapsedMilliseconds / _simulationStopWatch.ElapsedMilliseconds)));
            Engine.MESSAGE(this, Engine.LogType.Log, "Total time spent in smoke module [s]:" + _smokeStopwatch.ElapsedMilliseconds * 0.001 + string.Format(" [{0}%]", (int)(100.0 * _smokeStopwatch.ElapsedMilliseconds / _simulationStopWatch.ElapsedMilliseconds)));
            Engine.MESSAGE(this, Engine.LogType.Log, "Total time spent on initial traffic route pathfinding [s]:" + _pathfindingStopwatch.ElapsedMilliseconds * 0.001 + string.Format(" [{0}%]", (int)(100.0 * _pathfindingStopwatch.ElapsedMilliseconds / _simulationStopWatch.ElapsedMilliseconds)));
            _state = SimulationState.Finished;
            Engine.MESSAGE(this, Engine.LogType.Log, " Simulation done.");            
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

            Engine.MESSAGE(this, Engine.LogType.Log, "All requested sub-modules initiated successfully.");
        }

        private void CreateFireModule()
        {            
            if (_scenario.Input.Simulation.RunFireModule)
            {
                if (_scenario.Input.Fire.FireModule == FireInput.FireModuleChoice.AscImport)
                {
                    _fireModule = new AscFireImport(this);
                    Engine.MESSAGE(this, Engine.LogType.Log, "Fire module AscImport initiated.");
                }
                else if(_scenario.Input.Fire.FireModule == FireInput.FireModuleChoice.FireCell)
                {
                    _fireModule = new FireMesh(this, _scenario.Data.Fire.LCPData, _scenario.Data.Fire.WeatherInput, _scenario.Data.Fire.WindInput, _scenario.Data.Fire.InitialFuelMoistureData, _scenario.Data.Fire.IgnitionPoints);
                    Engine.MESSAGE(this, Engine.LogType.Log, "Fire module FireCell initiated.");
                }
                else
                {
                    Engine.MESSAGE(this, Engine.LogType.SimulationError, "Could not initiate fire mdoule, aborting.");
                }
            }  
            else
            {
                Engine.MESSAGE(this, Engine.LogType.Log, "No fire module was enabled.");
            }
        }

        private void CreateSmokeModule()
        {
            //can only run together
            if (_scenario.Input.Simulation.RunSmokeModule)
            {
                //this module does not need the fire
                if (_scenario.Input.Smoke.SmokeModule == SmokeInput.SmokeModuleChoice.GlobalSmoke)
                {
                    _smokeModule = new GlobalSmoke(this, _scenario.Data.Smoke.ExtinctionRamp);
                    return;
                }                

                if (!_scenario.Input.Simulation.RunFireModule)
                {
                    Engine.MESSAGE(this, Engine.LogType.SimulationError, "Smoke module that needs fire as source was enabled but no fire module was enabled, aborting.");
                }
                else
                {                       
                    if(_scenario.Input.Smoke.SmokeModule == SmokeInput.SmokeModuleChoice.AdvectDiffuseMixingLayer)
                    {
                        _smokeModule = new AdvectDiffuseMixingLayer(this);
                    }
                    else if (_scenario.Input.Smoke.SmokeModule == SmokeInput.SmokeModuleChoice.AdvectDiffuse3D)
                    {
                        /*if (_smokeModule != null)
                        {
                            ((Smoke.AdvectDiffuseModel)_smokeModule).Release();
                        }
                        //_smokeModule = new Smoke.AdvectDiffuseModel(_fireModule, 250f, WUInity.INSTANCE.AdvectDiffuseCompute, WUInity.INSTANCE.NoiseTex, WUInity.INSTANCE.WindTex);*/                            
                        Engine.MESSAGE(this, Engine.LogType.Log, "Smoke module AdvectDiffuse initiated.");
                    }
                    else if (_scenario.Input.Smoke.SmokeModule == SmokeInput.SmokeModuleChoice.BoxModel)
                    {
                        //smokeBoxDispersionModel = new Smoke.BoxDispersionModel(fireMesh);
                    }                  
                }
            } 
            else
            {
                Engine.MESSAGE(this, Engine.LogType.Log, "No smoke module was enabled.");
            }
        }

        private void CreatePedestrianModule()
        {

            if (_scenario.Input.Simulation.RunPedestrianModule)
            {
                if (_scenario.Input.Pedestrian.PedestrianModule == PedestrianInput.PedestrianModuleChoice.JupedSimSUMO)
                {
                    //placeholder for JupedSim
                }
                else if (_scenario.Input.Pedestrian.PedestrianModule == PedestrianInput.PedestrianModuleChoice.MacroHouseholdSim)
                {
                    _pedestrianModule = new MacroHouseholdSim(this);
                    MacroHouseholdSim macroHouseholdSim = (MacroHouseholdSim)_pedestrianModule;
                    //place people
                    //macroHouseholdSim.PopulateCells(WUI_engine.RUNTIME_DATA.Routing.RouteCollections, WUI_engine.POPULATION.GetPopulationData());
                    macroHouseholdSim.PopulateSimulation(_scenario.Data.Population.Households);
                    Engine.MESSAGE(this, Engine.LogType.Log, "Pedestrian module MacroPedestrianSim initiated.");
                }
            }
            else
            {
                Engine.MESSAGE(this, Engine.LogType.Log, "No pedestrian module was enabled.");
            }
        }

        private void CreateTrafficModule()
        {
            if (_scenario.Input.Simulation.RunTrafficModule)
            {
                if (_scenario.Input.Traffic.TrafficModule == TrafficInput.TrafficModuleChoice.SUMO)
                {
                    bool success;
                    _trafficModule = new SUMOModule(this, out success);
                    if(success)
                    {
                        Engine.MESSAGE(this, Engine.LogType.Log, "Traffic module SUMO initiated.");
                    }  
                    else
                    {
                        _stopRun = true;
                    }
                }
                else
                {
                    _trafficModule = new MacroTrafficSim(this);
                    Engine.MESSAGE(this, Engine.LogType.Log, "Traffic module MacroTrafficSim initiated.");
                }
            }
            else
            {
                Engine.MESSAGE(this, Engine.LogType.Log, "No traffic module was enabled.");
            }
        }
        
        bool _runRealtime = false;
        private void Step()
        {  
            long startTime = _simulationStopWatch.ElapsedMilliseconds;
            UpdateEvents();
            //this state represents the positions at the start of the time step
            if (_talkToWUIShow && _scenario.Input.WUIShow.SendDataToWUIShow && _trafficModule != null)
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
            float deltaTime = _scenario.Input.Simulation.DeltaTime;
            //if only fire running we can take longer steps potentially
            if (_scenario.Input.Simulation.RunFireModule && !_scenario.Input.Simulation.RunPedestrianModule && !_scenario.Input.Simulation.RunTrafficModule && !_scenario.Input.Simulation.RunSmokeModule)
            {
                deltaTime = (float)_fireModule.GetInternalDeltaTime();
            }
            _currentTime += deltaTime;

            //see if we are done or not
            CheckCompletion();

            if (_scenario.Input.Simulation.RunFireModule)
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

            bool endTimeReached = CurrentTime <= _scenario.Input.Simulation.MaxSimTime ? false : true;

            if(endTimeReached)
            {
                Stop("Simulation has reached specified end time.", false);
            }

            if (!_stopRun && _scenario.Input.Simulation.StopWhenEvacuated)
            {
                bool pedestrianDone = true;
                if (_scenario.Input.Simulation.RunPedestrianModule)
                {
                    pedestrianDone = _pedestrianModule.IsSimulationDone();
                }
                bool trafficDone = true;
                if (_scenario.Input.Simulation.RunTrafficModule)
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
            if (_scenario.Input.Simulation.RunTrafficModule)
            {
                //check for global events
                if (_scenario.Data.Evacuation.BlockGoalEvents != null)
                {
                    for (int i = 0; i < _scenario.Data.Evacuation.BlockGoalEvents.Length; i++)
                    {
                        BlockDestinationEvent bGE = _scenario.Data.Evacuation.BlockGoalEvents[i];
                        if (CurrentTime >= bGE.StartTime && !bGE.Triggered)
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
            if (_scenario.Input.Simulation.RunFireModule)
            {
                if (CurrentTime >= nextFireUpdate && CurrentTime >= 0.0f)
                {
                    fireUpdated = true;
                    _fireStopwatch.Start();
                    _fireModule.Step(_currentTime, _scenario.Input.Simulation.DeltaTime);
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
            if (_scenario.Input.Simulation.RunSmokeModule && CurrentTime >= 0.0f)
            {
                _smokeStopwatch.Start();
                if (_scenario.Input.Smoke.SmokeModule == SmokeInput.SmokeModuleChoice.BoxModel)
                {
                    //smokeBoxDispersionModel.Update(input.deltaTime, fireMesh.currentWindData.direction, fireMesh.currentWindData.speed);
                }
                else
                {
                    _smokeModule.Step(_currentTime, _scenario.Input.Simulation.DeltaTime);
                }
                _smokeStopwatch.Stop();
            }
        }

        private void StepPedestrianModule()
        {
            //advance pedestrian
            if (_scenario.Input.Simulation.RunPedestrianModule)
            {
                _pedestrianStopwatch.Start();
                _pedestrianModule.Step(CurrentTime, _scenario.Input.Simulation.DeltaTime);
                _pedestrianStopwatch.Stop();
            }
        }

        private void StepTrafficModule()
        {
            //advance traffic
            if (_scenario.Input.Simulation.RunTrafficModule)
            {
                _trafficStopwatch.Start();
                _trafficModule.Step(_scenario.Input.Simulation.DeltaTime, CurrentTime);
                _trafficStopwatch.Stop();
            }
        }


        CancellationTokenSource _stopThreadsToken = new CancellationTokenSource();
        private void StopModules()
        {
            //_stopThreadsToken.Cancel();

            if (_scenario.Input.Simulation.RunPedestrianModule && _pedestrianModule != null)
            {
                _pedestrianModule.Stop();
            }

            if (_scenario.Input.Simulation.RunTrafficModule && _trafficModule != null)
            {
                _trafficModule.Stop();
            }

            if (_scenario.Input.Simulation.RunFireModule && _fireModule != null)
            {
                _fireModule.Stop();
            }

            if (_scenario.Input.Simulation.RunSmokeModule && _smokeModule != null)
            {
                _smokeModule.Stop();
            }
        }

        void CheckEvacuationGoalStatus()
        {
            for (int i = 0; i < _evacuationDestinations.Count; i++)
            {
                EvacuationDestination eG = _evacuationDestinations[i];
                if(!eG.Blocked)
                {
                    FireCellState cellState = _fireModule.GetFireCellState(eG.LatLon);
                    if (cellState == FireCellState.Burning)
                    {
                        Engine.MESSAGE(this, Engine.LogType.Log, " Goal blocked by fire: " + eG.Name);
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
                Engine.MESSAGE(this, Engine.LogType.Log, stopMessage);
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
            if (!_evacuationDestinations[index].Blocked)
            {
                _evacuationDestinations[index].BlockDestination(this);
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
            for (int i = 0; i < _evacuationDestinations.Count; i++)
            {
                if(!_evacuationDestinations[i].Blocked)
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

        private void SaveOutput()
        {
            if (_scenario.Input.Simulation.RunTrafficModule)
            {
                Engine.MESSAGE(this, Engine.LogType.Log, " Total cars in simulation: " + _trafficModule.GetTotalCarsSimulated());
                _trafficModule.SaveToFile(_simulationIndex);
            }
            if (_scenario.Input.Simulation.RunPedestrianModule)
            {
                if (_scenario.Input.Pedestrian.PedestrianModule == PedestrianInput.PedestrianModuleChoice.MacroHouseholdSim)
                {
                    MacroHouseholdSim mHS = (MacroHouseholdSim)_pedestrianModule;
                    string file = Path.Combine(_engine.OutputFolder, _scenario.Input.Simulation.Name + "_pedestrian_output_" + _simulationIndex + ".csv");
                    mHS.SaveToFile(file);
                }                    
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
                _scenario.Data.Fire.Visualizer.CreateTriggerBufferVisuals(_triggerBufferDataOutput);
                _scenario.Data.Fire.Visualizer.SetLCPViewMode(Visualization.FireDataVisualizer.LcpViewMode.TriggerBuffer);
                _scenario.Data.Fire.Visualizer.SetLCPDataPlane(true);
            }            
        }

        public uint GetTotalEvacuated()
        {
            uint result = 0;
            for (int i = 0; i < _evacuationDestinations.Count; i++)
            {
                result += _evacuationDestinations[i].CurrentPeople;
            }

            return result;
        }
    }    
}