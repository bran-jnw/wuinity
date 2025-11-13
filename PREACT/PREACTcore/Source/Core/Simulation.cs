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
using PREACT.Fire;
using PREACT.Smoke;
using PREACT.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using PREACT.Math;

namespace PREACT
{
    [System.Serializable]
    public class Simulation
    {
        public enum SimulationState { Initializing, Running, Finished, Error };

        //References
        private Engine _engine;
        private PREACTInput _input;
        private SimulationState _state;
        private TrafficModule _trafficModule;
        private PedestrianModule _pedestrianModule;
        private FireModule _fireModule;
        private SmokeModule _smokeModule;
        private TriggerBufferModule _triggerBufferModule;
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

        //References
        public Engine Engine { get => _engine; }
        public SimulationState State { get => _state; }
        public PedestrianModule PedestrianModule { get => _pedestrianModule; }
        public TrafficModule TrafficModule { get => _trafficModule; }
        public FireModule FireModule { get => _fireModule; }
        public SmokeModule SmokeModule { get => _smokeModule; }
        public TriggerBufferModule TriggerBufferModule { get => _triggerBufferModule; }
        public PREACTInput Input { get => _input; }

        //Data
        public int SimulationIndex { get => _simulationIndex; }        
        public bool IsPaused { get => _isPaused; }        
        public bool HaveResults { get => _haveResults; }          
        public float StartTime { get => _startTime; }        
        public float CurrentTime { get => _currentTime; }  
        public float StepExecutionTime { get => _stepExecutionTime; }        
        public List<EvacuationDestination> Destinations { get => _evacuationDestinations; }
        public Vector2d UTMOrigin { get => _input.Simulation.Data.UTMOrigin; }
        public LatLngUTMConverter.UTMResult UTMData { get => _input.Simulation.Data.UTMData; }


        public Simulation(Engine engine, PREACTInput input, int simulationIndex)
        {
            _engine = engine;
            _input = input;
            _simulationIndex = simulationIndex;
        }

        /// <summary>
        /// Starts and runs the simulation until completed or halted.
        /// </summary>
        public void Run()
        {
            _state = SimulationState.Initializing;
            PreRun();

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
            _trafficStopwatch.Reset();
            _pedestrianStopwatch.Reset();
            _fireStopwatch.Reset();
            _smokeStopwatch.Reset();
            _stopRun = false;
            _stoppedDueToError = false;

            Engine.Message(this, Engine.LogType.Log, "Simulation  " + _simulationIndex + " started, please wait.");

            _evacuationDestinations = EvacuationDestination.CreateEvacacuationDestinations(this, _input.Evacuation.Data.EvacuationDestinationInputs);
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

            //pick start time based on curve or 0 (fire start)
            _currentTime = 0f;
            for (int i = 0; i < _input.Evacuation.Data.ResponseCurves.Count; i++)
            {
                float t = _input.Evacuation.Data.ResponseCurves[i].dataPoints[0].time + _input.Evacuation.EvacuationOrderStart;
                _currentTime = Mathf.Min(CurrentTime, t);
            }
            _startTime = CurrentTime;

            //inject any traffic events into traffic module
            if (_input.Simulation.RunTrafficModule && _input.Traffic.TrafficModule == TrafficInput.TrafficModuleChoice.MacroTrafficSim)
            {
                for (int i = 0; i < _input.Traffic.MacroTrafficSimInput.TrafficAccidents.Count; i++)
                {
                    _trafficModule.InsertNewTrafficEvent(_input.Traffic.MacroTrafficSimInput.TrafficAccidents[i]);
                }

                for (int i = 0; i < _input.Traffic.MacroTrafficSimInput.ReverseLanes.Count; i++)
                {
                    _trafficModule.InsertNewTrafficEvent(_input.Traffic.MacroTrafficSimInput.ReverseLanes[i]);
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

            if (!_stoppedDueToError)
            {
                SaveOutput();
            }

            if (!_stoppedDueToError)
            {
                _haveResults = true;
                CreateAndRunTriggerBufferModule();
            }

            _simulationStopWatch.Stop();
            Engine.Message(this, Engine.LogType.Log, "Total time spent [s]:" + _simulationStopWatch.ElapsedMilliseconds * 0.001);
            Engine.Message(this, Engine.LogType.Log, "Total time spent in pedestrian module [s]:" + _pedestrianStopwatch.ElapsedMilliseconds * 0.001 + string.Format(" [{0}%]", (int)(100.0 * _pedestrianStopwatch.ElapsedMilliseconds / _simulationStopWatch.ElapsedMilliseconds)));
            Engine.Message(this, Engine.LogType.Log, "Total time spent in traffic module [s]:" + _trafficStopwatch.ElapsedMilliseconds * 0.001 + string.Format(" [{0}%]", (int)(100.0 * _trafficStopwatch.ElapsedMilliseconds / _simulationStopWatch.ElapsedMilliseconds)));
            Engine.Message(this, Engine.LogType.Log, "Total time spent in fire module [s]:" + _fireStopwatch.ElapsedMilliseconds * 0.001 + string.Format(" [{0}%]", (int)(100.0 * _fireStopwatch.ElapsedMilliseconds / _simulationStopWatch.ElapsedMilliseconds)));
            Engine.Message(this, Engine.LogType.Log, "Total time spent in smoke module [s]:" + _smokeStopwatch.ElapsedMilliseconds * 0.001 + string.Format(" [{0}%]", (int)(100.0 * _smokeStopwatch.ElapsedMilliseconds / _simulationStopWatch.ElapsedMilliseconds)));
            Engine.Message(this, Engine.LogType.Log, "Total time spent on initial traffic route pathfinding [s]:" + _pathfindingStopwatch.ElapsedMilliseconds * 0.001 + string.Format(" [{0}%]", (int)(100.0 * _pathfindingStopwatch.ElapsedMilliseconds / _simulationStopWatch.ElapsedMilliseconds)));
            _state = SimulationState.Finished;
            Engine.Message(this, Engine.LogType.Log, " Simulation done.");
            //force garbage collection                
            System.GC.Collect();
        }
        
        private void CreateSimulationModules()
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

            Engine.Message(this, Engine.LogType.Log, "All requested sub-modules initiated successfully.");
        }

        private void CreateFireModule()
        {            
            if (_input.Simulation.RunFireModule)
            {
                if (_input.Fire.FireModule == FireInput.FireModuleChoice.AscImport)
                {
                    _fireModule = new AscFireImport(this);
                    Engine.Message(this, Engine.LogType.Log, "Fire module AscImport initiated.");
                }
                else if(_input.Fire.FireModule == FireInput.FireModuleChoice.FireCell)
                {
                    _fireModule = new FireMesh(this, _input.Fire.Data.LCPData, _input.Fire.Data.WeatherInput, _input.Fire.Data.WindInput, _input.Fire.Data.InitialFuelMoistureData, _input.Fire.Data.IgnitionPoints);
                    Engine.Message(this, Engine.LogType.Log, "Fire module FireCell initiated.");
                }
                else
                {
                    Engine.Message(this, Engine.LogType.SimulationError, "Could not initiate fire mdoule, aborting.");
                }
            }  
            else
            {
                Engine.Message(this, Engine.LogType.Log, "No fire module was enabled.");
            }
        }

        private void CreateSmokeModule()
        {
            //can only run together
            if (_input.Simulation.RunSmokeModule)
            {
                //this module does not need the fire
                if (_input.Smoke.SmokeModule == SmokeInput.SmokeModuleChoice.GlobalSmoke)
                {
                    _smokeModule = new GlobalSmoke(this, _input.Smoke.Data.ExtinctionRamp);
                    return;
                }                

                if (!_input.Simulation.RunFireModule)
                {
                    Engine.Message(this, Engine.LogType.SimulationError, "Smoke module that needs fire as source was enabled but no fire module was enabled, aborting.");
                }
                else
                {                       
                    if(_input.Smoke.SmokeModule == SmokeInput.SmokeModuleChoice.AdvectDiffuseMixingLayer)
                    {
                        _smokeModule = new AdvectDiffuseMixingLayer(this);
                    }
                    else if (_input.Smoke.SmokeModule == SmokeInput.SmokeModuleChoice.AdvectDiffuse3D)
                    {
                        /*if (_smokeModule != null)
                        {
                            ((Smoke.AdvectDiffuseModel)_smokeModule).Release();
                        }
                        //_smokeModule = new Smoke.AdvectDiffuseModel(_fireModule, 250f, WUInity.INSTANCE.AdvectDiffuseCompute, WUInity.INSTANCE.NoiseTex, WUInity.INSTANCE.WindTex);*/                            
                        Engine.Message(this, Engine.LogType.Log, "Smoke module AdvectDiffuse initiated.");
                    }
                    else if (_input.Smoke.SmokeModule == SmokeInput.SmokeModuleChoice.BoxModel)
                    {
                        //smokeBoxDispersionModel = new Smoke.BoxDispersionModel(fireMesh);
                    }                  
                }
            } 
            else
            {
                Engine.Message(this, Engine.LogType.Log, "No smoke module was enabled.");
            }
        }

        private void CreatePedestrianModule()
        {

            if (_input.Simulation.RunPedestrianModule)
            {
                if (_input.Pedestrian.PedestrianModule == PedestrianInput.PedestrianModuleChoice.JupedSimSUMO)
                {
                    //placeholder for JupedSim
                }
                else if (_input.Pedestrian.PedestrianModule == PedestrianInput.PedestrianModuleChoice.MacroHouseholdSim)
                {
                    _pedestrianModule = new MacroHouseholdSim(this);
                    MacroHouseholdSim macroHouseholdSim = (MacroHouseholdSim)_pedestrianModule;
                    //place people
                    //macroHouseholdSim.PopulateCells(WUI_engine.RUNTIME_DATA.Routing.RouteCollections, WUI_engine.POPULATION.GetPopulationData());
                    macroHouseholdSim.PopulateSimulation(_input.Population.Data.Households);
                    Engine.Message(this, Engine.LogType.Log, "Pedestrian module MacroPedestrianSim initiated.");
                }
            }
            else
            {
                Engine.Message(this, Engine.LogType.Log, "No pedestrian module was enabled.");
            }
        }

        private void CreateTrafficModule()
        {
            if (_input.Simulation.RunTrafficModule)
            {
                if (_input.Traffic.TrafficModule == TrafficInput.TrafficModuleChoice.SUMO)
                {
                    bool success;
                    _trafficModule = new SUMOModule(this, out success);
                    if(success)
                    {
                        Engine.Message(this, Engine.LogType.Log, "Traffic module SUMO initiated.");
                    }  
                    else
                    {
                        _stopRun = true;
                    }
                }
                else
                {
                    _trafficModule = new MacroTrafficSim(this);
                    Engine.Message(this, Engine.LogType.Log, "Traffic module MacroTrafficSim initiated.");
                }
            }
            else
            {
                Engine.Message(this, Engine.LogType.Log, "No traffic module was enabled.");
            }
        }

        private void CreateAndRunTriggerBufferModule()
        {
            if (_input.TriggerBuffer.CalculateTriggerBuffer)
            {
                if (_input.TriggerBuffer.TriggerBuffer == TriggerBufferInput.TriggerBufferChoice.kPERIL)
                {                    
                    if( !_input.Simulation.RunFireModule && !_input.TriggerBuffer.kPERILInput.CalculateROSFromBehave)
                    {
                        Engine.Message(this, Engine.LogType.Warning, "Can't run kPERIL without fire module (user set not to use BEHAVE).");
                        return;
                    }
                    else
                    {
                        if (_input.TriggerBuffer.kPERILInput.CalculateROSFromBehave)
                        {
                            _triggerBufferModule = new kPERIL(_input.Fire.Data.LCPData, _currentTime, _input.Fire.Data.WuiArea, _input.TriggerBuffer.kPERILInput.MidflameWindspeed, 0f, _input.Fire.Data.InitialFuelMoistureData, _input.Fire.Data.FuelModelsData);
                        }
                        else
                        {
                            _triggerBufferModule = new kPERIL(_currentTime, _input.Fire.Data.WuiArea, _input.TriggerBuffer.kPERILInput.MidflameWindspeed, 0f, _fireModule.GetMaxROS(), _fireModule.GetMaxROSAzimuth());
                        }
                        _triggerBufferModule.Run();
                        string outputFilePath = Path.Combine(_engine.OutputFolder, _simulationIndex + "_" + _input.TriggerBuffer.kPERILInput.OutputName);
                        kPERIL.SaveToFile(_triggerBufferModule.TriggerBufferOutput, _fireModule.GetCellSizeX(), outputFilePath);
                    }                    
                }
                else
                {
                   
                }

                if(_triggerBufferModule != null)
                {
                    _engine.Output.AddTriggerBufferOutput(_triggerBufferModule.TriggerBufferOutput, _simulationIndex);
                }
            }
            else
            {
                Engine.Message(this, Engine.LogType.Log, "Trigger buffer module was enabled.");
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
            float deltaTime = _input.Simulation.DeltaTime;
            //if only fire running we can take longer steps potentially
            if (_fireModule != null && _input.Simulation.RunFireModule && !_input.Simulation.RunPedestrianModule && !_input.Simulation.RunTrafficModule && !_input.Simulation.RunSmokeModule)
            {
                deltaTime = (float)_fireModule.GetInternalDeltaTime();
            }
            _currentTime += deltaTime;

            //see if we are done or not
            CheckCompletion();

            if (_input.Simulation.RunFireModule)
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

            bool endTimeReached = CurrentTime <= _input.Simulation.MaxSimTime ? false : true;

            if(endTimeReached)
            {
                Stop("Simulation has reached specified end time.", false);
            }

            if (!_stopRun && _input.Simulation.StopWhenEvacuated)
            {
                bool pedestrianDone = true;
                if (_input.Simulation.RunPedestrianModule)
                {
                    pedestrianDone = _pedestrianModule.IsSimulationDone();
                }
                bool trafficDone = true;
                if (_input.Simulation.RunTrafficModule)
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
            if (_input.Simulation.RunTrafficModule)
            {
                //check for global events
                if (_input.Evacuation.Data.BlockGoalEvents != null)
                {
                    for (int i = 0; i < _input.Evacuation.Data.BlockGoalEvents.Length; i++)
                    {
                        BlockDestinationEvent bGE = _input.Evacuation.Data.BlockGoalEvents[i];
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
            if (_input.Simulation.RunFireModule)
            {
                if (CurrentTime >= nextFireUpdate && CurrentTime >= 0.0f)
                {
                    fireUpdated = true;
                    _fireStopwatch.Start();
                    _fireModule.Step(_currentTime, _input.Simulation.DeltaTime);
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
            if (_input.Simulation.RunSmokeModule && CurrentTime >= 0.0f)
            {
                _smokeStopwatch.Start();
                if (_input.Smoke.SmokeModule == SmokeInput.SmokeModuleChoice.BoxModel)
                {
                    //smokeBoxDispersionModel.Update(input.deltaTime, fireMesh.currentWindData.direction, fireMesh.currentWindData.speed);
                }
                else
                {
                    _smokeModule.Step(_currentTime, _input.Simulation.DeltaTime);
                }
                _smokeStopwatch.Stop();
            }
        }

        private void StepPedestrianModule()
        {
            //advance pedestrian
            if (_input.Simulation.RunPedestrianModule)
            {
                _pedestrianStopwatch.Start();
                _pedestrianModule.Step(CurrentTime, _input.Simulation.DeltaTime);
                _pedestrianStopwatch.Stop();
            }
        }

        private void StepTrafficModule()
        {
            //advance traffic
            if (_input.Simulation.RunTrafficModule)
            {
                _trafficStopwatch.Start();
                _trafficModule.Step(_input.Simulation.DeltaTime, CurrentTime);
                _trafficStopwatch.Stop();
            }
        }


        CancellationTokenSource _stopThreadsToken = new CancellationTokenSource();
        private void StopModules()
        {
            //_stopThreadsToken.Cancel();

            if (_input.Simulation.RunPedestrianModule && _pedestrianModule != null)
            {
                _pedestrianModule.Stop();
            }

            if (_input.Simulation.RunTrafficModule && _trafficModule != null)
            {
                _trafficModule.Stop();
            }

            if (_input.Simulation.RunFireModule && _fireModule != null)
            {
                _fireModule.Stop();
            }

            if (_input.Simulation.RunSmokeModule && _smokeModule != null)
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
                        Engine.Message(this, Engine.LogType.Log, " Goal blocked by fire: " + eG.Name);
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
                Engine.Message(this, Engine.LogType.Log, stopMessage);
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

        private void UpdateRoutes()
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
            if (_input.Simulation.RunTrafficModule)
            {
                Engine.Message(this, Engine.LogType.Log, " Total cars in simulation: " + _trafficModule.GetTotalCarsSimulated());
                _trafficModule.SaveToFile(_simulationIndex);
                SaveArrivalData();
            }
            if (_input.Simulation.RunPedestrianModule)
            {
                if (_input.Pedestrian.PedestrianModule == PedestrianInput.PedestrianModuleChoice.MacroHouseholdSim)
                {
                    MacroHouseholdSim mHS = (MacroHouseholdSim)_pedestrianModule;
                    string file = Path.Combine(_engine.OutputFolder, _input.Simulation.Name + "_pedestrian_output_" + _simulationIndex + ".csv");
                    mHS.SaveToFile(file);
                }                    
            }
            
        }

        private void SaveArrivalData()
        {
            string outputFilePath = Path.Combine(_engine.OutputFolder, _input.Simulation.Name + "_" + _simulationIndex + "_arrivalData.csv");
            using (StreamWriter outputFile = new StreamWriter(outputFilePath))
            {
                List<float> data = _trafficModule.GetArrivalData();
                foreach (float value in data)
                {
                    outputFile.WriteLine(value.ToString());
                }                
            }
        }

        public Vector2d GetSimulationPosition(Vector2d latLon)
        {
            return _input.Simulation.Data.GetSimulationPosition(latLon);
        }

        public Vector2d GetWGS84FromSimulationPosition(Vector2d pos)
        {
            return _input.Simulation.Data.GetWGS84FromSimulationPosition(pos);
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

        List<float> _emptyArrivalData = new List<float>();
        public List<float> GetTrafficArrivalData()
        {
            if (_trafficModule != null)
            {
                return _trafficModule.GetArrivalData();
            }
            else
            {
                return _emptyArrivalData;
            }
        }
    }    
}