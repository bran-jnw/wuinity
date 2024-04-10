using System.Collections.Generic;
using WUIPlatform.Pedestrian;
using WUIPlatform.Traffic;
using WUIPlatform.Fire;
using System.Threading;
using WUIPlatform.Smoke;
using System.Diagnostics;
using WUIPlatform.IO;
#if USING_UNITY
using WUIPlatform.WUInity;
#endif

namespace WUIPlatform
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

        private bool _stopSim;

        private RouteCreator _routeCreator;  
        public RouteCreator RouteCreator
        {
            get 
            { 
                if(_routeCreator == null)
                {
                    _routeCreator = new RouteCreator();
                }
                return _routeCreator; 
            }
        }

        private TrafficModule _trafficModule;
        public TrafficModule TrafficModule { get => _trafficModule; }

        private PedestrianModule _pedestrianModule;
        public PedestrianModule PedestrianModule { get => _pedestrianModule; }

        private FireModule _fireModule;
        public FireModule FireModule { get => _fireModule; }

        //when is this needed, should only be internal?
        private SmokeModule _smokeModule;
        public SmokeModule SmokeModule { get =>_smokeModule; }

        private float _startTime;
        public float StartTime { get => _startTime; }

        private float _currentTime;
        public float CurrentTime { get => _currentTime; } 
                
        private float _stepExecutionTime;
        public float StepExecutionTime { get => _stepExecutionTime; }


        public Simulation()
        {
            
        }  

        /// <summary>
        /// Should only be set by traffic verification basically, otherwise it is internally created.
        /// </summary>
        /// <param name="mTS"></param>
        public void SetMacroTrafficSim(MacroTrafficSim mTS)
        {
            _trafficModule = mTS;
        }

        public async void Start()
        {
            try
            {
                System.Threading.Tasks.Task simTask = System.Threading.Tasks.Task.Run(StartSimulations);
                await simTask;
            }
            catch(System.Exception e) 
            {
                throw e;
            }
        }

        int runNumber;
        private  void StartSimulations()
        {
            runNumber = 0;
            int actualRuns = 0;
            float averageTotalEvacTime = 0.0f;
            int convergedInSequence = 0;
            List<List<float>> trafficArrivalDataCollection = new List<List<float>>();

            for (int i = 0; i < WUIEngine.RUNTIME_DATA.Simulation.NumberOfRuns; i++)
            {
                WUIEngine.LOG(WUIEngine.LogType.Log, "Simulation number " + i + " started, please wait.");
                runNumber = i;                
                CreateSubModules(i);                
                RunSimulation();
                ++actualRuns;

                trafficArrivalDataCollection.Add(_trafficModule.GetArrivalData());
                //need at least 2 simulations to have valid average
                if (i > 0)
                {
                    float pastAverage = (averageTotalEvacTime / i);
                    averageTotalEvacTime += CurrentTime;
                    float currentAverage = (averageTotalEvacTime / (i + 1));
                    float convergenceCriteria = (currentAverage - pastAverage) / currentAverage;
                    //if convergence met we can stop
                    if (convergenceCriteria < WUIEngine.RUNTIME_DATA.Simulation.ConvergenceMaxDifference)
                    {
                        ++convergedInSequence;
                        //we are done
                        if(WUIEngine.INPUT.Simulation.StopAfterConverging && convergedInSequence > WUIEngine.RUNTIME_DATA.Simulation.ConvergenceMinSequence)
                        {
                            i = WUIEngine.RUNTIME_DATA.Simulation.NumberOfRuns;
                        }                            
                    }
                    else
                    {
                        convergedInSequence = 0;
                    }
                }     
                else
                {
                    averageTotalEvacTime += CurrentTime;
                }
                //force garbage collection
                //Resources.UnloadUnusedAssets();                    
                System.GC.Collect();
            }            

            //save functional analysis
            float[] averageCurve = FunctionalAnalysis.CalculateAverageCurve(trafficArrivalDataCollection, FunctionalAnalysis.DimensionScalingMode.Average);
            SaveAverageCurve(averageCurve);    

            if (convergedInSequence >= 10)
            {
                WUIEngine.LOG(WUIEngine.LogType.Log, " Average total evacuation time: " + averageTotalEvacTime / actualRuns + " seconds, ran " + actualRuns + " simulations before converging according to user set criteria.");
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Log, " Average total evacuation time: " + averageTotalEvacTime / actualRuns + " seconds, ran " + actualRuns + " simulation/s.");
            }
            WUIEngine.OUTPUT.totalEvacTime = CurrentTime;
            _haveResults = true;            

            //plot results
            double[] xData = new double[averageCurve.Length];
            double[] yData = new double[averageCurve.Length];
            for (int i = 0; i < averageCurve.Length; i++)
            {
                xData[i] = averageCurve[i] / 3600.0f;
                yData[i] = i + 1;
            }
            PlotResults(xData, yData);

            _state = SimulationState.Finished;
            WUIEngine.LOG(WUIEngine.LogType.Log, " Simulation/s done.");
        }
        
        private void CreateSubModules(int runIndex)
        {
            _state = SimulationState.Initializing;

            CreateFireModule();
            if(_stopSim)
            {
                return;
            }

            CreateSmokeModule();
            if (_stopSim)
            {
                return;
            }

            CreatePedestrianModule(runIndex);
            if (_stopSim)
            {
                return;
            }

            CreateTrafficModule();
            if (_stopSim)
            {
                return;
            }

            WUIEngine.LOG(WUIEngine.LogType.Log, "All sub-modules initiated successfully.");
        }

        private void CreateFireModule()
        {            
            if (WUIEngine.INPUT.Simulation.RunFireModule)
            {
                if (WUIEngine.INPUT.Fire.fireModuleChoice == FireInput.FireModuleChoice.FarsiteOffline)
                {
                    _fireModule = new FarsiteOffline();
                    WUIEngine.LOG(WUIEngine.LogType.Log, "Fire module FarsiteOffline initiated.");
                }
                else
                {
                    _fireModule = new FireMesh(WUIEngine.RUNTIME_DATA.Fire.LCPData, WUIEngine.RUNTIME_DATA.Fire.WeatherInput, WUIEngine.RUNTIME_DATA.Fire.WindInput, WUIEngine.RUNTIME_DATA.Fire.InitialFuelMoistureData, WUIEngine.RUNTIME_DATA.Fire.IgnitionPoints);
                    WUIEngine.LOG(WUIEngine.LogType.Log, "Fire module Raster initiated.");
                }
            }  
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Log, "No fire module was enabled.");
            }
        }

        private void CreateSmokeModule()
        {
            //can only run together
            if (WUIEngine.INPUT.Simulation.RunSmokeModule )
            {
                if(!WUIEngine.INPUT.Simulation.RunFireModule)
                {
                    WUIEngine.LOG(WUIEngine.LogType.Error, "Smoke simulation was enabled but no fire module was enabled, aborting.");
                    //input.Simulation.RunSmokeModule = false;
                }
                else
                {
                    //smokeBoxDispersionModel = new Smoke.BoxDispersionModel(fireMesh);
                    if (_fireModule == null)
                    {
                        WUIEngine.LOG(WUIEngine.LogType.Error, "No fire module could be created, smoke simulation can not be performed, aborting");
                        //input.Simulation.RunSmokeModule = false;
                    }
                    else
                    {
                        if (WUIEngine.INPUT.Smoke.smokeModuleChoice == SmokeInput.SmokeModuleChoice.AdvectDiffuse)
                        {
                            /*if (_smokeModule != null)
                            {
                                ((Smoke.AdvectDiffuseModel)_smokeModule).Release();
                            }
                            //_smokeModule = new Smoke.AdvectDiffuseModel(_fireModule, 250f, WUInity.INSTANCE.AdvectDiffuseCompute, WUInity.INSTANCE.NoiseTex, WUInity.INSTANCE.WindTex);*/
                            _smokeModule = new MixingLayerSmokeSpread();
                            WUIEngine.LOG(WUIEngine.LogType.Log, "Smoke module AdvectDiffuse initiated.");
                        }
                    }                    
                }
            } 
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Log, "No smoke module was enabled.");
            }
        }

        private void CreatePedestrianModule(int runIndex)
        {

            if (WUIEngine.INPUT.Simulation.RunPedestrianModule)
            {
                if (runIndex == 0)
                {
                    //we could not load from disk, so have to build all routes
                    if (WUIEngine.RUNTIME_DATA.Routing.RouteCollections == null)
                    {
                        WUIEngine.RUNTIME_DATA.Routing.BuildAndSaveRouteCollection();
                    }

                    WUIEngine.POPULATION.GetPopulationData().UpdatePopulationBasedOnRoutes(WUIEngine.RUNTIME_DATA.Routing.RouteCollections);                    
                }

                if (WUIEngine.INPUT.Evacuation.pedestrianModuleChoice == EvacuationInput.PedestrianModuleChoice.SUMO)
                {
                    //placeholder for JupedSim
                }
                else
                {
                    _pedestrianModule = new MacroHouseholdSim();
                    MacroHouseholdSim macroHouseholdSim = (MacroHouseholdSim)_pedestrianModule;
                    //place people
                    macroHouseholdSim.PopulateCells(WUIEngine.RUNTIME_DATA.Routing.RouteCollections, WUIEngine.POPULATION.GetPopulationData());
                    //distribute people
                    macroHouseholdSim.PlaceHouseholdsInCells();
                    WUIEngine.LOG(WUIEngine.LogType.Log, "Pedestrian module MacroPedestrianSim initiated.");
                }
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Log, "No pedestrian module was enabled.");
            }
        }

        private void CreateTrafficModule()
        {
            if (WUIEngine.INPUT.Simulation.RunTrafficModule)
            {
                if (WUIEngine.INPUT.Traffic.trafficModuleChoice == TrafficInput.TrafficModuleChoice.SUMO)
                {
                    _trafficModule = new SUMOModule();
                    WUIEngine.LOG(WUIEngine.LogType.Log, "Traffic module SUMO initiated.");
                }
                else
                {
                    _trafficModule = new MacroTrafficSim(RouteCreator);
                    WUIEngine.LOG(WUIEngine.LogType.Log, "Traffic module MacroTrafficSim initiated.");
                }
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Log, "No traffic module was enabled.");
            }
        }

        
        private void RunSimulation()
        {
            //when creating modules we migth have found an issue
            if(_stopSim)
            {
                _state = SimulationState.Error;
                return;
            }

            //if we do multiple runs the goals have to be reset
            for (int i = 0; i < WUIEngine.RUNTIME_DATA.Evacuation.EvacuationGoals.Count; i++)
            {
                WUIEngine.RUNTIME_DATA.Evacuation.EvacuationGoals[i].ResetPeopleAndCars();
            }

            //pick start time based on curve or 0 (fire start)
            _currentTime = 0f;
            for (int i = 0; i < WUIEngine.RUNTIME_DATA.Evacuation.ResponseCurves.Length; i++)
            {
                float t = WUIEngine.RUNTIME_DATA.Evacuation.ResponseCurves[i].dataPoints[0].time + WUIEngine.INPUT.Evacuation.EvacuationOrderStart;
                _currentTime = Mathf.Min(CurrentTime, t);
            }
            _startTime = CurrentTime;

            //inject any traffic effevnts into traffic module
            if(WUIEngine.INPUT.Simulation.RunTrafficModule)
            {
                for (int i = 0; i < WUIEngine.INPUT.Traffic.trafficAccidents.Length; i++)
                {
                    _trafficModule.InsertNewTrafficEvent(WUIEngine.INPUT.Traffic.trafficAccidents[i]);
                }

                for (int i = 0; i < WUIEngine.INPUT.Traffic.reverseLanes.Length; i++)
                {
                    _trafficModule.InsertNewTrafficEvent(WUIEngine.INPUT.Traffic.reverseLanes[i]);
                }
            }            

            //do actual simulation steps
            _stopSim = false;
            _state = SimulationState.Running;
            nextFireUpdate = 0f; //fire start is always 0 seconds

            //only update visuals if doing single run
            if (!WUIEngine.RUNTIME_DATA.Simulation.MultipleSimulations)
            {
                _haveResults = true;
            }

            while (!_stopSim)
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
            SaveOutput(runNumber);
        }

        bool _runRealtime = false;
        private void Step()
        {  
            WUIEngine.ENGINE.StopWatch.Start();

            UpdateEvents();
            System.Threading.Tasks.Task fireTask = System.Threading.Tasks.Task.Run(StepFireModule);
            //can't be run on thread due to Unity stuff for now
            System.Threading.Tasks.Task smokeTask =  System.Threading.Tasks.Task.Run(StepSmokeModule);
            System.Threading.Tasks.Task pedestrianTask = System.Threading.Tasks.Task.Run(StepPedestrianModule);
            System.Threading.Tasks.Task trafficTask = System.Threading.Tasks.Task.Run(StepTrafficModule);

            fireTask.Wait();
            smokeTask.Wait();
            pedestrianTask.Wait();
            trafficTask.Wait();
                        
            //handle any fire effects on road network
            if(WUIEngine.INPUT.Simulation.RunFireModule)
            {
                _trafficModule.HandleIgnitedFireCells(_fireModule.GetIgnitedFireCells());
                _fireModule.ConsumeIgnitedFireCells();
            }
            //take care of arrived cars and inject for next time step
            _trafficModule.HandleNewCars();

            //increase time
            float deltaTime = WUIEngine.INPUT.Simulation.DeltaTime;
            //if only fire running we can take longer steps potentially
            if (WUIEngine.INPUT.Simulation.RunFireModule && !WUIEngine.INPUT.Simulation.RunPedestrianModule && !WUIEngine.INPUT.Simulation.RunTrafficModule && !WUIEngine.INPUT.Simulation.RunSmokeModule)
            {
                deltaTime = (float)_fireModule.GetInternalDeltaTime();
            }
            _currentTime += deltaTime;
            WUIEngine.OUTPUT.totalEvacTime = _currentTime;

            //see if we are done or not
            CheckCompletion();

            if (WUIEngine.INPUT.Simulation.RunFireModule)
            {
                //check if any goal has been blocked by fire, this is done after everything has progressed the current time step
                CheckEvacuationGoalStatus();
                //can get set when evac goals are all gone
                if (_stopSim)
                {
                    return;
                }
            }

            WUIEngine.ENGINE.StopWatch.Stop();
            if(_runRealtime)
            {
                int sleepTime = (int)deltaTime * 1000 - (int)WUIEngine.ENGINE.StopWatch.ElapsedMilliseconds;
                if (sleepTime > 0)
                {
                    Thread.Sleep(sleepTime);
                }                
            }
            float t =0.05f * WUIEngine.ENGINE.StopWatch.ElapsedMilliseconds + 0.95f * _stepExecutionTime;
            _stepExecutionTime = t;
            WUIEngine.ENGINE.StopWatch.Reset();
        }

        private void CheckCompletion()
        {
            if (_stopSim)
            {
                return;
            }

            bool endTimeReached = CurrentTime <= WUIEngine.INPUT.Simulation.MaxSimTime ? false : true;

            if(endTimeReached)
            {
                StopSim("Simulation has reached specified end time.");
            }

            if (!_stopSim && WUIEngine.INPUT.Simulation.StopWhenEvacuated)
            {
                bool pedestrianDone = true;
                if (WUIEngine.INPUT.Simulation.RunPedestrianModule)
                {
                    pedestrianDone = _pedestrianModule.IsSimulationDone();
                }
                bool trafficDone = true;
                if (WUIEngine.INPUT.Simulation.RunTrafficModule)
                {
                    trafficDone = _trafficModule.IsSimulationDone();
                }

                if (pedestrianDone && trafficDone)
                {
                    StopSim("Both pedestrian and traffic simulations are done, stopping as per user settings.");
                }
            }
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
            WUInityInput input = WUIEngine.INPUT;

            if (input.Simulation.RunTrafficModule)
            {
                //check for global events
                if (WUIEngine.RUNTIME_DATA.Evacuation.BlockGoalEvents != null)
                {
                    for (int i = 0; i < WUIEngine.RUNTIME_DATA.Evacuation.BlockGoalEvents.Length; i++)
                    {
                        BlockGoalEvent bGE = WUIEngine.RUNTIME_DATA.Evacuation.BlockGoalEvents[i];
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
            if (WUIEngine.INPUT.Simulation.RunFireModule)
            {
                if (CurrentTime >= nextFireUpdate && CurrentTime >= 0.0f)
                {
                    fireUpdated = true;
                    _fireModule.Step(_currentTime, WUIEngine.INPUT.Simulation.DeltaTime);
                    nextFireUpdate += (float)_fireModule.GetInternalDeltaTime();
                    // Route analysis: consider calling RoutingData::ModifyRouterDB at this point if the fire interferes with the road network
                    // Note: we need to preprocess each cell which has a road on it
                }
            }
        }

        private void StepSmokeModule()
        {
            //sync with fire
            if (WUIEngine.INPUT.Simulation.RunSmokeModule && CurrentTime >= 0.0f)
            {
                if(WUIEngine.INPUT.Smoke.smokeModuleChoice == SmokeInput.SmokeModuleChoice.AdvectDiffuse)
                {
                    ((AdvectDiffuseModel)_smokeModule).Update(WUIEngine.INPUT.Simulation.DeltaTime, _fireModule.GetCurrentWindData().direction, _fireModule.GetCurrentWindData().speed, fireUpdated);
                }
                else if(WUIEngine.INPUT.Smoke.smokeModuleChoice == SmokeInput.SmokeModuleChoice.BoxModel)
                {
                    //smokeBoxDispersionModel.Update(input.deltaTime, fireMesh.currentWindData.direction, fireMesh.currentWindData.speed);
                }
                
            }
        }

        private void StepPedestrianModule()
        {
            //advance pedestrian
            if (WUIEngine.INPUT.Simulation.RunPedestrianModule)
            {
                _pedestrianModule.Step(CurrentTime, WUIEngine.INPUT.Simulation.DeltaTime);
            }
        }

        private void StepTrafficModule()
        {
            //advance traffic
            if (WUIEngine.INPUT.Simulation.RunTrafficModule)
            {
                _trafficModule.Step(WUIEngine.INPUT.Simulation.DeltaTime, CurrentTime);
            }
        }

        void CheckEvacuationGoalStatus()
        {
            for (int i = 0; i < WUIEngine.RUNTIME_DATA.Evacuation.EvacuationGoals.Count; i++)
            {
                EvacuationGoal eG = WUIEngine.RUNTIME_DATA.Evacuation.EvacuationGoals[i];
                if(!eG.blocked)
                {
                    FireCellState cellState = _fireModule.GetFireCellState(eG.latLong);
                    if (cellState == FireCellState.Burning)
                    {
                        WUIEngine.LOG(WUIEngine.LogType.Log, " Goal blocked by fire: " + eG.name);
                        BlockEvacGoal(i);
                    }
                }                
            }
        }

        public void StopSim(string stopMessage)
        {
            if(!_stopSim)
            {
                _stopSim = true;
                WUIEngine.LOG(WUIEngine.LogType.Log, stopMessage);
            }            
        }

        public void BlockEvacGoal(int index)
        {
            if (!WUIEngine.RUNTIME_DATA.Evacuation.EvacuationGoals[index].blocked)
            {
                WUIEngine.RUNTIME_DATA.Evacuation.EvacuationGoals[index].blocked = true;
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
            for (int i = 0; i < WUIEngine.RUNTIME_DATA.Evacuation.EvacuationGoals.Count; i++)
            {
                if(!WUIEngine.RUNTIME_DATA.Evacuation.EvacuationGoals[i].blocked)
                {
                    allBlocked = false;
                    break;
                }
            }
            if(allBlocked)
            {
                StopSim("No evacuation goals available, stopping simulation.");
                return;
            }

            //update raster evac routes first as traffic might use some of the updated choices
            int cellsWithoutRouteButNoonePlansToLeave = 0;
            if(WUIEngine.RUNTIME_DATA.Routing.RouteCollections != null)
            {
                for (int i = 0; i < WUIEngine.RUNTIME_DATA.Routing.RouteCollections.Length; i++)
                {
                    if(WUIEngine.INPUT.Evacuation.pedestrianModuleChoice == EvacuationInput.PedestrianModuleChoice.MacroHouseholdSim)
                    {
                        if (WUIEngine.RUNTIME_DATA.Routing.RouteCollections[i] != null && !((MacroHouseholdSim)_pedestrianModule).IsCellEvacuated(i))
                        {
                            if (((MacroHouseholdSim)_pedestrianModule).GetPeopleLeftInCellIntendingToLeave(i) > 0)
                            {
                                WUIEngine.RUNTIME_DATA.Routing.RouteCollections[i].CheckAndUpdateRoute();
                            }
                            else
                            {
                                ++cellsWithoutRouteButNoonePlansToLeave;
                            }
                        }
                    }
                    
                }
            }          
            if(cellsWithoutRouteButNoonePlansToLeave > 0)
            {
                WUIEngine.LOG(WUIEngine.LogType.Log, " " + cellsWithoutRouteButNoonePlansToLeave +  " cells have no routes left after goal was blocked, but noone left planning to leave from those cells.");
            }
            //update cars already in traffic
            _trafficModule.UpdateEvacuationGoals();              
        }

        void SaveOutput(int runNumber)
        {
            WUInityInput input = WUIEngine.INPUT;
            if (input.Simulation.RunTrafficModule)
            {
                WUIEngine.LOG(WUIEngine.LogType.Log, " Total cars in simulation: " + _trafficModule.GetTotalCarsSimulated());
                _trafficModule.SaveToFile(runNumber);
            }
            if (input.Simulation.RunPedestrianModule)
            {
                if (WUIEngine.INPUT.Evacuation.pedestrianModuleChoice == EvacuationInput.PedestrianModuleChoice.MacroHouseholdSim)
                {
                    MacroHouseholdSim mHS = (MacroHouseholdSim)_pedestrianModule;
                    mHS.SaveToFile(runNumber);
                }                    
            }

            WUInityOutput.SaveOutput(WUIEngine.INPUT.Simulation.SimulationID + "_" + runNumber);            
        }

        void PlotResults(double[] xData, double[] yData)
        {
            if (xData.Length > 0 && yData.Length > 0)
            {
                ScottPlot.Plot timeTraffic = new ScottPlot.Plot(512, 512);
                timeTraffic.AddScatterLines(xData, yData);
                timeTraffic.Title("Average cumulative arrival of cars");
                timeTraffic.YLabel("Number of cars [-]");
                timeTraffic.XLabel("Time [h]");
                //string plotPath = timeTraffic.SaveFig(System.IO.Path.Combine(WUIEngine.OUTPUT_FOLDER, "traffic_avg.png"));
                byte[] byteData = timeTraffic.GetImageBytes();

#if USING_UNITY
                WUInityEngine.GUI.SetArrivalPlotBytes(byteData);
#endif
            }
        }

        private void SaveAverageCurve(float[] data)
        {
            string[] output = new string[data.Length + 2];
            output[0] = "Time [s],ArrivalIndex [-]";
            output[1] = "0.0, 0";
            for (int i = 0; i < data.Length; i++)
            {
                output[i + 2] = data[i].ToString() + "," + (i + 1).ToString();
            }
            WUInityInput wuiIn = WUIEngine.INPUT;
            string path = System.IO.Path.Combine(WUIEngine.OUTPUT_FOLDER, wuiIn.Simulation.SimulationID + "_traffic_average.csv");
            System.IO.File.WriteAllLines(path, output);
        }

    }    
}