using System.Collections.Generic;
using WUInity.Pedestrian;
using WUInity.Traffic;
using WUInity.Fire;
using System.Threading;

namespace WUInity
{
    [System.Serializable]
    public class Simulation
    {    
        public bool HaveResults = false;
        public bool IsRunning = false;

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
            get
            {
                if (_trafficModule == null)
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
                return _trafficModule;
            }            
        }

        private PedestrianModule _pedestrianModule;
        public PedestrianModule PedestrianModule()
        {
            if (_pedestrianModule == null)
            {
                if(WUInity.INPUT.Evacuation.pedestrianModuleChoice == EvacuationInput.PedestrianModuleChoice.MacroHouseholdSim)
                {
                    _pedestrianModule = new MacroHouseholdSim();
                }
                else
                {
                    _pedestrianModule = new MacroHouseholdSim();
                }
               
            }

            return _pedestrianModule;
        }

        private FireMesh _fireMesh;
        public FireMesh FireMesh()
        {
            if (_fireMesh == null)
            {
                CreateFireSim();
            }

            return _fireMesh;
        }

        //when is this needed, should only be internal?
        private Smoke.AdvectDiffuseModel _advectDiffuseSim;
        public Smoke.AdvectDiffuseModel AdvectDiffuseSim()
        {
            if(_advectDiffuseSim == null)
            {
                _advectDiffuseSim = new Smoke.AdvectDiffuseModel(_fireMesh, 250f, WUInity.INSTANCE.AdvectDiffuseCompute, WUInity.INSTANCE.NoiseTex, WUInity.INSTANCE.WindTex);
            }
            return _advectDiffuseSim;
        }

        private float _startTime;
        public float StartTime 
        {
            get 
            { 
                return _startTime; 
            }
        }

        private float _currentTime;
        public float CurrentTime
        {
            get
            {
                return _currentTime;
            }
        } 

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

        public float GetFireWindSpeed()
        {
            return _fireMesh.currentWindData.speed;
        }

        public float GetFireWindDirection()
        {
            return _fireMesh.currentWindData.direction;
        }       
        
        public  void StartSimulation()
        {

            WUInityInput input = WUInity.INPUT;            

            //run in real time with gui updates
            if(!WUInity.RUNTIME_DATA.Simulation.MultipleSimulations)
            {
                CreateSubSims(0);
                RunSimulation(0);
            }
            else
            {
                float averageTotalEvacTime = 0.0f;
                int actualRuns = 0;
                int convergedInSequence = 0;
                List<List<float>> trafficArrivalDataCollection = new List<List<float>>();
                for (int i = 0; i < WUInity.RUNTIME_DATA.Simulation.NumberOfRuns; i++)
                {
                    CreateSubSims(i);
                    //do actual simulation
                    RunSimulation(i);

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
                IsRunning = false;
                HaveResults = true;
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
        
        private void CreateSubSims(int i)
        {
            WUInityInput input = WUInity.INPUT;

            if (input.Simulation.RunFireModule)
            {
                CreateFireSim();                
            }

            //can only run together
            if(input.Simulation.RunSmokeModule && input.Simulation.RunFireModule)
            {
                //smokeBoxDispersionModel = new Smoke.BoxDispersionModel(fireMesh);
                if(_fireMesh == null)
                {
                    WUInity.LOG(WUInity.LogType.Warning, "No fire mesh has been created, disabling smoke spread simulation.");
                    input.Simulation.RunSmokeModule = false;
                }
                else
                {    if(_advectDiffuseSim != null)
                    {
                        _advectDiffuseSim.Release();
                    }
                    _advectDiffuseSim = new Smoke.AdvectDiffuseModel(_fireMesh, 250f, WUInity.INSTANCE.AdvectDiffuseCompute, WUInity.INSTANCE.NoiseTex, WUInity.INSTANCE.WindTex);
                }                
            }
            else
            {
                input.Simulation.RunSmokeModule = false;
            }

            if (input.Simulation.RunPedestrianModule)
            {
                if (i == 0)
                {                     
                    //we could not load from disk, so have to build all routes
                    if (WUInity.RUNTIME_DATA.Routing.RouteCollections == null)
                    {
                        WUInity.RUNTIME_DATA.Routing.BuildAndSaveRouteCollection();
                    }

                    WUInity.POPULATION.GetPopulationData().UpdatePopulationBasedOnRoutes(WUInity.RUNTIME_DATA.Routing.RouteCollections);
                }

                if(input.Evacuation.pedestrianModuleChoice == EvacuationInput.PedestrianModuleChoice.SUMO)
                {
                    
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

            if (input.Simulation.RunTrafficModule)
            {
                if(WUInity.INPUT.Traffic.trafficModuleChoice == TrafficInput.TrafficModuleChoice.SUMO)
                {
                    _trafficModule = new SUMOModule();
                }
                else
                {
                    _trafficModule = new MacroTrafficSim(RouteCreator);
                }
               
            }
        }

        private void CreateFireSim()
        {            
            _fireMesh = new FireMesh(WUInity.RUNTIME_DATA.Fire.LCPData, WUInity.RUNTIME_DATA.Fire.WeatherInput, WUInity.RUNTIME_DATA.Fire.WindInput, WUInity.RUNTIME_DATA.Fire.InitialFuelMoistureData, WUInity.RUNTIME_DATA.Fire.IgnitionPoints);
            _fireMesh.spreadMode = WUInity.INPUT.Fire.spreadMode;     
        }

        private void RunSimulation(int runNumber)
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
            IsRunning = true;
            nextFireUpdate = 0f;  
            if(WUInity.RUNTIME_DATA.Simulation.MultipleSimulations)
            {
                while (!_stopSim)
                {
                    //checks if we are done
                    UpdateSimStatus();    
                    Update();
                    if (_stopSim)
                    {
                        break;
                    }
                }
                SaveOutput(runNumber);
            }
            else
            {
                HaveResults = true;
            }
        }        

        private void UpdateSimStatus()
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

        public void UpdateRealtimeSim()
        {
            if(!IsRunning)
            {
                return;
            }

            //checks if we are done
            UpdateSimStatus();
            if (!_stopSim)
            {
                WUInity.OUTPUT.totalEvacTime = CurrentTime;
                Update();
            }
            else if(IsRunning)
            {
                IsRunning = false;
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
        }

        bool _runMultiThreaded = true;
        private void Update()
        {
            WUInityInput input = WUInity.INPUT;

            if(_runMultiThreaded)
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
            if (input.Simulation.RunFireModule && !input.Simulation.RunPedestrianModule && !input.Simulation.RunTrafficModule)
            {
                deltaTime = (float)_fireMesh.dt;
            }
            _currentTime += deltaTime;
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
                    _fireMesh.Update(_currentTime, input.Simulation.DeltaTime);
                    nextFireUpdate += (float)_fireMesh.dt;
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
                //smokeBoxDispersionModel.Update(input.deltaTime, fireMesh.currentWindData.direction, fireMesh.currentWindData.speed);
                _advectDiffuseSim.Update(input.Simulation.DeltaTime, _fireMesh.currentWindData.direction, _fireMesh.currentWindData.speed, fireUpdated);
            }
        }

        private void UpdatePedestrianModule()
        {
            WUInityInput input = WUInity.INPUT;

            //advance pedestrian
            if (input.Simulation.RunPedestrianModule)
            {
                _pedestrianModule.Update(CurrentTime, input.Simulation.DeltaTime);
            }
        }

        private void UpdateTrafficModule()
        {
            WUInityInput input = WUInity.INPUT;

            //advance traffic
            if (input.Simulation.RunTrafficModule)
            {
                _trafficModule.Update(input.Simulation.DeltaTime, CurrentTime);
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
                    Fire.FireCellState cellState = _fireMesh.GetFireCellState(eG.latLong);
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