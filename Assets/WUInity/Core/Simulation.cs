using System.Collections.Generic;
using WUInity.Pedestrian;
using WUInity.Traffic;
using WUInity.Fire;
using System.Threading;
using WUInity.Smoke;
using System.Data;
using System.Diagnostics;

namespace WUInity
{
    [System.Serializable]
    public class Simulation
    {    
        public enum SimulationState { Idle, Initializing, Running, Stopped};
        private SimulationState _state;
        public SimulationState State
        {
            get{ return _state; }
        }

        private bool _isPaused = false;
        public bool IsPaused
        {
            get { return _isPaused; }
        }

        bool _haveResults = false;
        public bool HaveResults
        { 
            get { return _haveResults; } 
        }

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
        public TrafficModule TrafficModule
        {
            get { return _trafficModule; }            
        }

        private PedestrianModule _pedestrianModule;
        public PedestrianModule PedestrianModule
        {
            get{ return _pedestrianModule; }            
        }

        private FireModule _fireModule;
        public FireModule FireModule
        {
            get{ return _fireModule; }            
        }

        //when is this needed, should only be internal?
        private SmokeModule _smokeModule;
        public SmokeModule SmokeModule
        {
            get =>_smokeModule;
        }

        private float _startTime;
        public float StartTime 
        {
            get => _startTime; 
        }

        private float _currentTime;
        public float CurrentTime
        {
            get => _currentTime;
        } 

        private Stopwatch _stopWatch;
        private float _stepExecutionTime;
        public float StepExecutionTime
        {
            get => _stepExecutionTime;
        }
        public Simulation()
        {
            _stopWatch = new Stopwatch();
        }  

        /// <summary>
        /// Should only be set by traffic verification basically, otherwise it is internally created.
        /// </summary>
        /// <param name="mTS"></param>
        public void SetMacroTrafficSim(MacroTrafficSim mTS)
        {
            _trafficModule = mTS;
        }     

        int runNumber;
        public  void Start()
        {

            WUInityInput input = WUInity.INPUT;            

            //run in real time with gui updates
            if(!WUInity.RUNTIME_DATA.Simulation.MultipleSimulations)
            {
                runNumber = 0;
                CreateSubModules(0);
                System.Threading.Tasks.Task simTask = System.Threading.Tasks.Task.Run(RunSimulation);
            }
            else
            {
                float averageTotalEvacTime = 0.0f;
                int actualRuns = 0;
                int convergedInSequence = 0;
                List<List<float>> trafficArrivalDataCollection = new List<List<float>>();
                for (int i = 0; i < WUInity.RUNTIME_DATA.Simulation.NumberOfRuns; i++)
                {
                    runNumber = i;
                    CreateSubModules(i);
                    //do actual simulation
                    RunSimulation();

                    trafficArrivalDataCollection.Add(_trafficModule.GetArrivalData());
                    ++actualRuns;    
                    //need at least 2 simulation sto have valid average
                    if (i > 0)
                    {
                        float pastAverage = (averageTotalEvacTime / i);
                        averageTotalEvacTime += CurrentTime;
                        float currentAverage = (averageTotalEvacTime / (i + 1));
                        float convergenceCriteria = (currentAverage - pastAverage) / currentAverage;
                        //if convergence met we can stop
                        if (convergenceCriteria < WUInity.RUNTIME_DATA.Simulation.ConvergenceMaxDifference)
                        {
                            ++convergedInSequence;
                            //we are done
                            if(WUInity.INPUT.Simulation.StopAfterConverging && convergedInSequence > WUInity.RUNTIME_DATA.Simulation.ConvergenceMinSequence)
                            {
                                i = WUInity.RUNTIME_DATA.Simulation.NumberOfRuns;
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
                    WUInity.LOG(WUInity.LogType.Log, " Average total evacuation time: " + averageTotalEvacTime / actualRuns + " seconds, ran " + actualRuns + " simulations before converging according to user set criteria.");
                }
                else
                {
                    WUInity.LOG(WUInity.LogType.Log, " Average total evacuation time: " + averageTotalEvacTime / actualRuns + " seconds, ran " + actualRuns + " simulations.");
                }
                WUInity.OUTPUT.totalEvacTime = CurrentTime;
                _state = SimulationState.Idle;
                _haveResults = true;
                WUInity.LOG(WUInity.LogType.Log, " Simulation done.");

                //plot results
                double[] xData = new double[averageCurve.Length];
                double[] yData = new double[averageCurve.Length];
                for (int i = 0; i < averageCurve.Length; i++)
                {
                    xData[i] = averageCurve[i] / 3600.0f;
                    yData[i] = i + 1;
                }
                PlotResults(xData, yData);
            }
        }

        void PlotResults(double[] xData, double[] yData)
        {
            if(xData.Length > 0 && yData.Length > 0)
            {
                ScottPlot.Plot timeTraffic = new ScottPlot.Plot(512, 512);
                timeTraffic.AddScatterLines(xData, yData);
                timeTraffic.Title("Average cumulative arrival of cars");
                timeTraffic.YLabel("Number of cars [-]");
                timeTraffic.XLabel("Time [h]");
                //string plotPath = timeTraffic.SaveFig(System.IO.Path.Combine(WUInity.OUTPUT_FOLDER, "traffic_avg.png"));
                byte[] byteData = timeTraffic.GetImageBytes();

                #if USING_UNITY
                UnityEngine.Texture2D plotFig = new UnityEngine.Texture2D(2, 2);
                UnityEngine.ImageConversion.LoadImage(plotFig, byteData);
                WUInity.GUI.SetPlotTexture(plotFig);
                #else

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
            WUInityInput wuiIn = WUInity.INPUT;
            string path = System.IO.Path.Combine(WUInity.OUTPUT_FOLDER, wuiIn.Simulation.SimulationID + "_traffic_average.csv");
            System.IO.File.WriteAllLines(path, output);
        }
        
        private void CreateSubModules(int runIndex)
        {                        
            CreateFireModule();
            CreateSmokeModule();
            CreatePedestrianModule(runIndex);
            CreateTrafficModule();    
        }

        private void CreateFireModule()
        {
            WUInityInput input = WUInity.INPUT;

            if (input.Simulation.RunFireModule)
            {
                if (WUInity.INPUT.Fire.fireModuleChoice == FireInput.FireModuleChoice.FarsiteOffline)
                {
                    _fireModule = new FarsiteOffline();
                }
                else
                {
                    _fireModule = new FireMesh(WUInity.RUNTIME_DATA.Fire.LCPData, WUInity.RUNTIME_DATA.Fire.WeatherInput, WUInity.RUNTIME_DATA.Fire.WindInput, WUInity.RUNTIME_DATA.Fire.InitialFuelMoistureData, WUInity.RUNTIME_DATA.Fire.IgnitionPoints);
                    ((FireMesh)_fireModule).spreadMode = WUInity.INPUT.Fire.spreadMode;
                }
            }            
        }

        private void CreateSmokeModule()
        {
            WUInityInput input = WUInity.INPUT;

            //can only run together
            if (input.Simulation.RunSmokeModule && input.Simulation.RunFireModule)
            {
                //smokeBoxDispersionModel = new Smoke.BoxDispersionModel(fireMesh);
                if (_fireModule == null)
                {
                    WUInity.LOG(WUInity.LogType.Error, "No fire module could be created, smoke simulation can not be performed.");
                    input.Simulation.RunSmokeModule = false;
                }
                else
                {
                    if (input.Smoke.smokeModuleChoice == SmokeInput.SmokeModuleChoice.AdvectDiffuse)
                    {
                        if (_smokeModule != null)
                        {
                            ((Smoke.AdvectDiffuseModel)_smokeModule).Release();
                        }
                        _smokeModule = new Smoke.AdvectDiffuseModel(_fireModule, 250f, WUInity.INSTANCE.AdvectDiffuseCompute, WUInity.INSTANCE.NoiseTex, WUInity.INSTANCE.WindTex);
                    }
                }
            }
            else
            {
                WUInity.LOG(WUInity.LogType.Error, "Smoke simulation was enabled but no fire module was enabled, aborting.");
                input.Simulation.RunSmokeModule = false;
            }
        }

        private void CreatePedestrianModule(int runIndex)
        {
            WUInityInput input = WUInity.INPUT;

            if (input.Simulation.RunPedestrianModule)
            {
                if (runIndex == 0)
                {
                    //we could not load from disk, so have to build all routes
                    if (WUInity.RUNTIME_DATA.Routing.RouteCollections == null)
                    {
                        WUInity.RUNTIME_DATA.Routing.BuildAndSaveRouteCollection();
                    }

                    WUInity.POPULATION.GetPopulationData().UpdatePopulationBasedOnRoutes(WUInity.RUNTIME_DATA.Routing.RouteCollections);
                }

                if (input.Evacuation.pedestrianModuleChoice == EvacuationInput.PedestrianModuleChoice.SUMO)
                {
                    //placeholder for JupedSim
                }
                else
                {
                    _pedestrianModule = new MacroHouseholdSim();
                    MacroHouseholdSim macroHouseholdSim = (MacroHouseholdSim)_pedestrianModule;
                    //place people
                    macroHouseholdSim.PopulateCells(WUInity.RUNTIME_DATA.Routing.RouteCollections, WUInity.POPULATION.GetPopulationData());
                    //distribute people
                    macroHouseholdSim.PlaceHouseholdsInCells();
                }
            }
        }

        private void CreateTrafficModule()
        {
            WUInityInput input = WUInity.INPUT;

            if (input.Simulation.RunTrafficModule)
            {
                if (WUInity.INPUT.Traffic.trafficModuleChoice == TrafficInput.TrafficModuleChoice.SUMO)
                {
                    _trafficModule = new SUMOModule();
                }
                else
                {
                    _trafficModule = new MacroTrafficSim(RouteCreator);
                }
            }
        }

        
        private void RunSimulation()
        {
            WUInityInput input = WUInity.INPUT;

            //if we do multiple runs the goals have to be reset
            for (int i = 0; i < WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals.Count; i++)
            {
                WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals[i].ResetPeopleAndCars();
            }

            //pick start time based on curve or 0 (fire start)
            _currentTime = 0f;
            for (int i = 0; i < WUInity.RUNTIME_DATA.Evacuation.ResponseCurves.Length; i++)
            {
                float t = WUInity.RUNTIME_DATA.Evacuation.ResponseCurves[i].dataPoints[0].time + input.Evacuation.EvacuationOrderStart;
                _currentTime = Mathf.Min(CurrentTime, t);
            }
            _startTime = CurrentTime;

            if(input.Simulation.RunTrafficModule)
            {
                for (int i = 0; i < WUInity.INPUT.Traffic.trafficAccidents.Length; i++)
                {
                    _trafficModule.InsertNewTrafficEvent(WUInity.INPUT.Traffic.trafficAccidents[i]);
                }

                for (int i = 0; i < WUInity.INPUT.Traffic.reverseLanes.Length; i++)
                {
                    _trafficModule.InsertNewTrafficEvent(WUInity.INPUT.Traffic.trafficAccidents[i]);
                }
            }            

            _stopSim = false;
            _state = SimulationState.Running;
            nextFireUpdate = 0f;  
            if(WUInity.RUNTIME_DATA.Simulation.MultipleSimulations)
            {
                while (!_stopSim)
                {
                    //checks if we are done
                    CheckCompletion();    
                    Step();
                    if (_stopSim)
                    {
                        break;
                    }
                }
                SaveOutput(runNumber);
            }
            else
            {
                _haveResults = true;
                while (!_stopSim)
                {
                    UpdateRealtimeSim();
                }
            }
        }        

        private void CheckCompletion()
        {
            if(_stopSim)
            {
                return;
            }

            WUInityInput input = WUInity.INPUT;
            _stopSim = CurrentTime <= input.Simulation.MaxSimTime ? false : true;

            if (input.Simulation.StopWhenEvacuated)
            {
                bool pedestrianDone = true;
                if (input.Simulation.RunPedestrianModule)
                {
                    pedestrianDone = _pedestrianModule.SimulationDone();
                }
                bool trafficDone = true;
                if (input.Simulation.RunTrafficModule)
                {
                    trafficDone = _trafficModule.SimulationDone();
                }

                if (pedestrianDone && trafficDone)
                {
                    _stopSim = true;
                }
            }
        }

        public void TogglePause()
        {
            _isPaused = !_isPaused; 
        }

        private void UpdateRealtimeSim()
        {
            if(_isPaused)
            {
                Thread.Sleep(1000);
            }
            else
            {
                CheckCompletion();                
                if (_stopSim && _state != SimulationState.Stopped)
                {
                    _state = SimulationState.Idle;
                    SaveOutput(0);
                    WUInity.LOG(WUInity.LogType.Log, " Simulation done.");

                    //plot results
                    List<float> arrivalData = _trafficModule.GetArrivalData();
                    double[] xData = new double[arrivalData.Count];
                    double[] yData = new double[arrivalData.Count];
                    for (int i = 0; i < arrivalData.Count; i++)
                    {
                        xData[i] = arrivalData[i] / 3600.0f;
                        yData[i] = i + 1;
                    }
                    PlotResults(xData, yData);
                }
                else
                {
                    Step();
                }
            }    
        }

        bool _runMultiThreaded = true;
        private void Step()
        {
            WUInityInput input = WUInity.INPUT;

            _stopWatch.Start();           

            if (_runMultiThreaded)
            {
                UpdateEvents();
                System.Threading.Tasks.Task task1 = System.Threading.Tasks.Task.Run(UpdateFireModule);
                //can't be run on thread due to Unity stuff for now
                UpdateSmokeModule();
                System.Threading.Tasks.Task task3 = System.Threading.Tasks.Task.Run(UpdatePedestrianModule);
                System.Threading.Tasks.Task task4 = System.Threading.Tasks.Task.Run(UpdateTrafficModule);

                task1.Wait();
                task3.Wait();
                task4.Wait();
            }
            else
            {
                UpdateEvents();
                UpdateFireModule();
                UpdateSmokeModule();
                UpdatePedestrianModule();
                UpdateTrafficModule();
            }

            //take care of arrived cars and inject for next time step
            _trafficModule.PostUpdate();

            //increase time
            float deltaTime = input.Simulation.DeltaTime;
            //if only fire running we can take longer steps potentially
            if (input.Simulation.RunFireModule && !input.Simulation.RunPedestrianModule && !input.Simulation.RunTrafficModule && !input.Simulation.RunSmokeModule)
            {
                deltaTime = (float)_fireModule.GetInternalDeltaTime();
            }
            _currentTime += deltaTime;
            WUInity.OUTPUT.totalEvacTime = _currentTime;

            _stopWatch.Stop();
            int t =(int)(0.01f * _stopWatch.ElapsedMilliseconds + 0.99f * _stepExecutionTime);
            _stepExecutionTime = t;
            _stopWatch.Reset();
        }

        private void UpdateEvents()
        {
            WUInityInput input = WUInity.INPUT;

            if (input.Simulation.RunTrafficModule)
            {
                //check for global events
                if (WUInity.RUNTIME_DATA.Evacuation.BlockGoalEvents != null)
                {
                    for (int i = 0; i < WUInity.RUNTIME_DATA.Evacuation.BlockGoalEvents.Length; i++)
                    {
                        BlockGoalEvent bGE = WUInity.RUNTIME_DATA.Evacuation.BlockGoalEvents[i];
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
        private void UpdateFireModule()
        {
            WUInityInput input = WUInity.INPUT;

            //update fire mesh if needed
            fireUpdated = false;
            if (input.Simulation.RunFireModule)
            {
                if (CurrentTime >= 0.0f && CurrentTime >= nextFireUpdate)
                {
                    fireUpdated = true;
                    _fireModule.Step(_currentTime, input.Simulation.DeltaTime);
                    nextFireUpdate += (float)_fireModule.GetInternalDeltaTime();
                    // Route analysis: consider calling RoutingData::ModifyRouterDB at this point if the fire interferes with the road network
                    // Note: we need to preprocess each cell which has a road on it
                }
            }
        }

        private void UpdateSmokeModule()
        {
            WUInityInput input = WUInity.INPUT;

            //sync with fire
            if (CurrentTime >= 0.0f && input.Simulation.RunSmokeModule)
            {
                if(input.Smoke.smokeModuleChoice == SmokeInput.SmokeModuleChoice.AdvectDiffuse)
                {
                    ((AdvectDiffuseModel)_smokeModule).Update(input.Simulation.DeltaTime, _fireModule.GetCurrentWindData().direction, _fireModule.GetCurrentWindData().speed, fireUpdated);
                }
                else if(input.Smoke.smokeModuleChoice == SmokeInput.SmokeModuleChoice.BoxModel)
                {
                    //smokeBoxDispersionModel.Update(input.deltaTime, fireMesh.currentWindData.direction, fireMesh.currentWindData.speed);
                }
                
            }
        }

        private void UpdatePedestrianModule()
        {
            WUInityInput input = WUInity.INPUT;

            //advance pedestrian
            if (input.Simulation.RunPedestrianModule)
            {
                _pedestrianModule.Step(CurrentTime, input.Simulation.DeltaTime);
            }
        }

        private void UpdateTrafficModule()
        {
            WUInityInput input = WUInity.INPUT;

            //advance traffic
            if (input.Simulation.RunTrafficModule)
            {
                _trafficModule.Step(input.Simulation.DeltaTime, CurrentTime);
            }

            if (input.Simulation.RunFireModule)
            {
                //check if any goal has been blocked by fire, this is done after everything has progressed the current time step
                CheckEvacuationGoalStatus();
                //can get set when evac goals are all gone
                if (_stopSim)
                {
                    return;
                }
            }
        }

        void CheckEvacuationGoalStatus()
        {
            for (int i = 0; i < WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals.Count; i++)
            {
                EvacuationGoal eG = WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals[i];
                if(!eG.blocked)
                {
                    Fire.FireCellState cellState = _fireModule.GetFireCellState(eG.latLong);
                    if (cellState == Fire.FireCellState.Burning)
                    {
                        WUInity.LOG(WUInity.LogType.Log, " Goal blocked by fire: " + eG.name);
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
                WUInity.LOG(WUInity.LogType.Log, stopMessage);
            }            
        }

        public void BlockEvacGoal(int index)
        {
            if (!WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals[index].blocked)
            {
                WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals[index].blocked = true;
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
            for (int i = 0; i < WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals.Count; i++)
            {
                if(!WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals[i].blocked)
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
            if(WUInity.RUNTIME_DATA.Routing.RouteCollections != null)
            {
                for (int i = 0; i < WUInity.RUNTIME_DATA.Routing.RouteCollections.Length; i++)
                {
                    if(WUInity.INPUT.Evacuation.pedestrianModuleChoice == EvacuationInput.PedestrianModuleChoice.MacroHouseholdSim)
                    {
                        if (WUInity.RUNTIME_DATA.Routing.RouteCollections[i] != null && !((MacroHouseholdSim)_pedestrianModule).IsCellEvacuated(i))
                        {
                            if (((MacroHouseholdSim)_pedestrianModule).GetPeopleLeftInCellIntendingToLeave(i) > 0)
                            {
                                WUInity.RUNTIME_DATA.Routing.RouteCollections[i].CheckAndUpdateRoute();
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
                WUInity.LOG(WUInity.LogType.Log, " " + cellsWithoutRouteButNoonePlansToLeave +  " cells have no routes left after goal was blocked, but noone left planning to leave from those cells.");
            }
            //update cars already in traffic
            _trafficModule.UpdateEvacuationGoals();              
        }

        void SaveOutput(int runNumber)
        {
            WUInityInput input = WUInity.INPUT;
            if (input.Simulation.RunTrafficModule)
            {
                WUInity.LOG(WUInity.LogType.Log, " Total cars in simulation: " + _trafficModule.GetTotalCarsSimulated());
                _trafficModule.SaveToFile(runNumber);
            }
            if (input.Simulation.RunPedestrianModule)
            {
                if (WUInity.INPUT.Evacuation.pedestrianModuleChoice == EvacuationInput.PedestrianModuleChoice.MacroHouseholdSim)
                {
                    MacroHouseholdSim mHS = (MacroHouseholdSim)_pedestrianModule;
                    mHS.SaveToFile(runNumber);
                }                    
            }

            WUInityOutput.SaveOutput(WUInity.INPUT.Simulation.SimulationID + "_" + runNumber);            
        }
    }    
}