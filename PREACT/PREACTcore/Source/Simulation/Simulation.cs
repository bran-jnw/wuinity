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
        private SimulationState _state;
        private Engine _engine;
        private TimeManager _timeManager;
        private WeatherManager _weatherManager;
        private SpatialManager _spatialManager;
        private EvacuationManager _evacuationManager;
        private HazardManager _hazardManager;
        private PREACTInput _input;
        private SimulationOutput _output;        

        private TrafficModule _trafficModule;
        private PedestrianModule _pedestrianModule;
        private WildfireModule _wildfireModule;
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
        private bool _visualize;
        private bool _isPaused = false;
        private bool _stopRun;
        private bool _haveResults = false;
        private float _stepExecutionTime;
        

        //References
        public Engine Engine { get => _engine; }
        public SimulationState State { get => _state; }
        public TimeManager Time { get => _timeManager; }
        public WeatherManager Weather { get => _weatherManager; }
        public SpatialManager Spatial { get => _spatialManager; }
        public EvacuationManager Evacuation { get => _evacuationManager; }
        public HazardManager Hazards { get => _hazardManager; }
        public PedestrianModule PedestrianModule { get => _pedestrianModule; }
        public TrafficModule TrafficModule { get => _trafficModule; }
        public WildfireModule WildfireModule { get => _wildfireModule; }
        public SmokeModule SmokeModule { get => _smokeModule; }
        public TriggerBufferModule TriggerBufferModule { get => _triggerBufferModule; }
        public PREACTInput Input { get => _input; }
        public SimulationOutput Output { get => _output; }

        //Data
        public int SimulationIndex { get => _simulationIndex; }
        public bool Visualize { get => _visualize; }
        public bool IsPaused { get => _isPaused; }        
        public bool HaveResults { get => _haveResults; }         
        public float SimulationTime { get => _timeManager.SimulationTime; }  
        public float StepExecutionTime { get => _stepExecutionTime; }        
        public Vector2d UTMOrigin { get => _input.Simulation.Data.UTMOrigin; }
        public LatLngUTMConverter.UTMResult UTMData { get => _input.Simulation.Data.UTMData; }


        public Simulation(Engine engine, PREACTInput input, int simulationIndex)
        {
            _engine = engine;
            _input = input;
            _simulationIndex = simulationIndex;
            _timeManager = new TimeManager(_input, this);
            _spatialManager = new SpatialManager(this);
            _weatherManager = new WeatherManager(this);
            _evacuationManager = new EvacuationManager(this);
            _hazardManager = new HazardManager(this);
            _output = new SimulationOutput(this);
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
            _trafficStopwatch.Reset();
            _pedestrianStopwatch.Reset();
            _fireStopwatch.Reset();
            _smokeStopwatch.Reset();
            _stopRun = false;
            _stoppedDueToError = false;

            Engine.Message(this, Engine.LogType.Log, "Simulation  " + _simulationIndex + " started, please wait.");

            _weatherManager.Initialize(_timeManager);
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
                    _trafficModule.InsertNewTrafficEvent(_input.TrafficModule.MacroTrafficSimInput.TrafficAccidents[i]);
                }

                for (int i = 0; i < _input.TrafficModule.MacroTrafficSimInput.ReverseLanes.Count; i++)
                {
                    _trafficModule.InsertNewTrafficEvent(_input.TrafficModule.MacroTrafficSimInput.ReverseLanes[i]);
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

        bool _runRealtime = false;
        private void Step()
        {
            long startTime = _simulationStopWatch.ElapsedMilliseconds;
            UpdateEvents();
            //this state represents the positions at the start of the time step
            if (_talkToWUIShow && _input.WUIShow.SendDataToWUIShow && _trafficModule != null)
            {
                _engine.WUIShow.SendData(_timeManager.SimulationTime);
            }

            //step all modules forward in time
            System.Threading.Tasks.Task fireTask = System.Threading.Tasks.Task.Run(StepWildfireModule, _stopThreadsToken.Token);
            System.Threading.Tasks.Task smokeTask = System.Threading.Tasks.Task.Run(StepSmokeModule, _stopThreadsToken.Token);
            System.Threading.Tasks.Task pedestrianTask = System.Threading.Tasks.Task.Run(StepPedestrianModule, _stopThreadsToken.Token);
            System.Threading.Tasks.Task trafficTask = System.Threading.Tasks.Task.Run(StepTrafficModule, _stopThreadsToken.Token);

            System.Threading.Tasks.Task.WaitAll(fireTask, smokeTask, pedestrianTask, trafficTask);

            //handle any fire effects on road network
            if (_wildfireModule != null)
            {
                if (_trafficModule != null)
                {
                    _trafficModule.HandleIgnitedFireCells(_wildfireModule.GetIgnitedFireCells());
                }
                _wildfireModule.ConsumeIgnitedFireCells();
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
            if (_wildfireModule != null && _input.WildfireModule.Enabled && !_input.PedestrianModule.Enabled && !_input.TrafficModule.Enabled && !_input.SmokeModule.Enabled)
            {
                deltaTime = (float)_wildfireModule.GetInternalDeltaTime();
            }

            //see if we are done or not
            _timeManager.Step(deltaTime);
            _weatherManager.Step(_timeManager.SimulationTime, _timeManager.CurrentDateTime);
            CheckCompletion();

            if (_input.WildfireModule.Enabled)
            {
                //check if any goal has been blocked by fire, this is done after everything has progressed the current time step
                _evacuationManager.CheckEvacuationGoalStatus();
                //can get set when evac goals are all gone
                if (_stopRun)
                {
                    return;
                }
            }

            UpdateTiming(startTime, deltaTime);
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

            bool endTimeReached = _timeManager.SimulationEndTime - SimulationTime < 0.001f ? true : false;

            if (endTimeReached)
            {
                Stop("Simulation has reached specified end time.", false);
            }

            if (!_stopRun && _input.Simulation.StopWhenEvacuated)
            {
                bool pedestrianDone = true;
                if (_input.PedestrianModule.Enabled)
                {
                    pedestrianDone = _pedestrianModule.IsSimulationDone();
                }
                bool trafficDone = true;
                if (_input.TrafficModule.Enabled)
                {
                    trafficDone = _trafficModule.IsSimulationDone();
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

            CreateWildfireModule();
            if (_stopRun)
            {
                return;
            }
            
            CreateDispersionModule();
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

        private void CreateWildfireModule()
        {            
            if (_input.WildfireModule.Enabled)
            {
                if (_input.WildfireModule.Module == WildfireModuleInput.WildfireModules.AscImport)
                {
                    _wildfireModule = new AscFireImport(this);
                    Engine.Message(this, Engine.LogType.Log, "Fire module AscImport initiated.");
                }
                else if(_input.WildfireModule.Module == WildfireModuleInput.WildfireModules.FireCell)
                {
                    _wildfireModule = new FireMesh(this, _input.WildfireModule.Data.LandscapeData, _weatherManager, _input.WildfireModule.Data.InitialFuelMoistureData, _input.WildfireModule.Data.IgnitionPoints);
                    Engine.Message(this, Engine.LogType.Log, "Fire module FireCell initiated.");
                }
                else if (_input.WildfireModule.Module == WildfireModuleInput.WildfireModules.CellParticleHybrid)
                {
                    _wildfireModule = new CellParticleHybrid(this, _input.WildfireModule.Data.LandscapeData, _input.WildfireModule.Data.WuiArea, _input.WildfireModule.Data.FuelModelsData, _input.WildfireModule.Data.InitialFuelMoistureData, _input.WildfireModule.Data.IgnitionPoints);
                    Engine.Message(this, Engine.LogType.Log, "Fire module CellParticleHybrid initiated.");
                }
                else if (_input.WildfireModule.Module == WildfireModuleInput.WildfireModules.NarrowLevelSet)
                {
                    _wildfireModule = new NarrowLevelSet(this, _input.WildfireModule.Data.LandscapeData, _input.WildfireModule.Data.IgnitionPoints, _weatherManager, _timeManager);
                    Engine.Message(this, Engine.LogType.Log, "Fire module NarrowLevelSet initiated.");
                }
                else
                {
                    Engine.Message(this, Engine.LogType.SimulationError, "Could not initiate fire module, aborting.");
                }
            }  
            else
            {
                Engine.Message(this, Engine.LogType.Log, "No fire module was enabled.");
            }
        }

        private void CreateDispersionModule()
        {
            //can only run together
            if (_input.SmokeModule.Enabled)
            {
                //this module does not need the fire
                if (_input.SmokeModule.Module == SmokeInput.SmokeModules.GlobalSmoke)
                {
                    _smokeModule = new GlobalSmoke(this, _input.SmokeModule.Data.ExtinctionRamp);
                    return;
                }                

                if (!_input.WildfireModule.Enabled)
                {
                    Engine.Message(this, Engine.LogType.SimulationError, "Smoke module that needs fire as source was enabled but no fire module was enabled, aborting.");
                }
                else
                {                       
                    if(_input.SmokeModule.Module == SmokeInput.SmokeModules.AdvectDiffuseMixingLayer)
                    {
                        _smokeModule = new AdvectDiffuseMixingLayer(this);
                    }
                    else if (_input.SmokeModule.Module == SmokeInput.SmokeModules.AdvectDiffuse3D)
                    {
                        _smokeModule = new AdvectDiffuse3D(this);                         
                        Engine.Message(this, Engine.LogType.Log, "Smoke module AdvectDiffuse3D initiated.");
                    }
                    else if (_input.SmokeModule.Module == SmokeInput.SmokeModules.BoxModel)
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

            if (_input.PedestrianModule.Enabled)
            {
                if (_input.PedestrianModule.Module == PedestrianModuleInput.PedestrianModules.JupedSimSUMO)
                {
                    //placeholder for JupedSim
                }
                else if (_input.PedestrianModule.Module == PedestrianModuleInput.PedestrianModules.MacroHouseholdSim)
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
            if (_input.TrafficModule.Enabled)
            {
                if (_input.TrafficModule.Module == TrafficModuleInput.TrafficModules.SUMO)
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
            if (_input.TriggerBufferModule.Enabled)
            {
                if (_input.TriggerBufferModule.Module == TriggerBufferModuleInput.TriggerBufferModules.kPERIL)
                {                    
                    if( !_input.WildfireModule.Enabled && !_input.TriggerBufferModule.kPERILInput.CalculateROSFromBehave)
                    {
                        Engine.Message(this, Engine.LogType.Warning, "Can't run kPERIL without fire module (user set not to use BEHAVE).");
                        return;
                    }
                    else
                    {
                        if (_input.TriggerBufferModule.kPERILInput.CalculateROSFromBehave)
                        {
                            _triggerBufferModule = new kPERIL(_input.WildfireModule.Data.LandscapeData, _timeManager.SimulationTime, _input.WildfireModule.Data.WuiArea, _input.TriggerBufferModule.kPERILInput.MidflameWindspeed, 0f, _input.WildfireModule.Data.InitialFuelMoistureData, _input.WildfireModule.Data.FuelModelsData);
                        }
                        else
                        {
                            _triggerBufferModule = new kPERIL(_timeManager.SimulationTime, _input.WildfireModule.Data.WuiArea, _input.TriggerBufferModule.kPERILInput.MidflameWindspeed, 0f, _wildfireModule.GetMaxROS(), _wildfireModule.GetMaxROSAzimuth());
                        }
                        _triggerBufferModule.Run();
                        string outputFilePath = Path.Combine(_engine.OutputFolder, _simulationIndex + "_" + _input.TriggerBufferModule.kPERILInput.OutputName);
                        kPERIL.SaveToFile(_triggerBufferModule.TriggerBufferOutput, _wildfireModule.GetCellSizeX(), outputFilePath);
                    }                    
                }
                else
                {
                   
                }

                if(_triggerBufferModule != null)
                {
                    _output.AddTriggerBufferOutput(_triggerBufferModule.TriggerBufferOutput, _simulationIndex);
                }
            }
            else
            {
                Engine.Message(this, Engine.LogType.Log, "Trigger buffer module was enabled.");
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

        bool fireUpdated = false;
        double nextFireUpdate;
        private void StepWildfireModule()
        {
            //update fire mesh if needed
            fireUpdated = false;
            if (_input.WildfireModule.Enabled)
            {
                if (SimulationTime >= nextFireUpdate && SimulationTime >= 0.0f)
                {
                    fireUpdated = true;
                    _fireStopwatch.Start();
                    _wildfireModule.Step(_timeManager.SimulationTime, _input.Simulation.DeltaTime);
                    _fireStopwatch.Stop();
                    nextFireUpdate += _wildfireModule.GetInternalDeltaTime();
                }
            }
        }

        private void StepSmokeModule()
        {
            //sync with fire
            if (_input.SmokeModule.Enabled && SimulationTime >= 0.0f)
            {
                _smokeStopwatch.Start();
                if (_input.SmokeModule.Module == SmokeInput.SmokeModules.BoxModel)
                {
                    //smokeBoxDispersionModel.Update(input.deltaTime, fireMesh.currentWindData.direction, fireMesh.currentWindData.speed);
                }
                else
                {
                    _smokeModule.Step(_timeManager.SimulationTime, _input.Simulation.DeltaTime);
                }
                _smokeStopwatch.Stop();
            }
        }

        private void StepPedestrianModule()
        {
            //advance pedestrian
            if (_input.PedestrianModule.Enabled)
            {
                _pedestrianStopwatch.Start();
                _pedestrianModule.Step(_timeManager.SimulationTime, _input.Simulation.DeltaTime);
                _pedestrianStopwatch.Stop();
            }
        }

        private void StepTrafficModule()
        {
            //advance traffic
            if (_input.TrafficModule.Enabled)
            {
                _trafficStopwatch.Start();
                _trafficModule.Step(_timeManager.SimulationTime, _input.Simulation.DeltaTime);
                _trafficStopwatch.Stop();
            }
        }


        CancellationTokenSource _stopThreadsToken = new CancellationTokenSource();
        private void StopModules()
        {
            //_stopThreadsToken.Cancel();

            if (_pedestrianModule != null)
            {
                _pedestrianModule.Stop();
            }

            if (_trafficModule != null)
            {
                _trafficModule.Stop();
            }

            if (_wildfireModule != null)
            {
                _wildfireModule.Stop();
            }

            if (_smokeModule != null)
            {
                _smokeModule.Stop();
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

        private void SaveOutput()
        {
            if (_input.TrafficModule.Enabled)
            {
                Engine.Message(this, Engine.LogType.Log, " Total cars in simulation: " + _trafficModule.GetTotalCarsSimulated());
                _trafficModule.SaveToFile(_simulationIndex);
                SaveArrivalData();
            }
            if (_input.PedestrianModule.Enabled)
            {
                if (_input.PedestrianModule.Module == PedestrianModuleInput.PedestrianModules.MacroHouseholdSim)
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
    }    
}