﻿using System.Collections.Generic;
using UnityEngine;
using WUInity.Evac;
using WUInity.Traffic;
using WUInity.Fire;

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

        private MacroTrafficSim _macroTrafficSim;
        public MacroTrafficSim MacroTrafficSim()
        {
            if (_macroTrafficSim== null)
            {
                _macroTrafficSim = new MacroTrafficSim(RouteCreator);
            }
            return _macroTrafficSim;
        }

        private MacroHumanSim _macroHumanSim;
        public MacroHumanSim MacroHumanSim()
        {
            if (_macroHumanSim == null)
            {
                _macroHumanSim = new MacroHumanSim();
            }

            return _macroHumanSim;
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

        private float _time;
        public float Time
        {
            get
            {
                return _time;
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
            _macroTrafficSim = mTS;
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

                    trafficArrivalDataCollection.Add(_macroTrafficSim.GetArrivalData());
                    ++actualRuns;    
                    //need at least 2 simulation sto have valid average
                    if (i > 0)
                    {
                        float pastAverage = (averageTotalEvacTime / i);
                        averageTotalEvacTime += Time;
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
                        averageTotalEvacTime += Time;
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
                    WUInity.LOG("LOG: Average total evacuation time: " + averageTotalEvacTime / actualRuns + " seconds, ran " + actualRuns + " simulations before converging according to user set criteria.");
                }
                else
                {
                    WUInity.LOG("LOG: Average total evacuation time: " + averageTotalEvacTime / actualRuns + " seconds, ran " + actualRuns + " simulations.");
                }
                WUInity.OUTPUT.totalEvacTime = Time;
                IsRunning = false;
                HaveResults = true;
                WUInity.LOG("LOG: Simulation done.");

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

                Texture2D plotFig = new Texture2D(2, 2);
                ImageConversion.LoadImage(plotFig, byteData);
                WUInity.GUI.SetPlotTexture(plotFig);
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

            if (input.Simulation.RunFireSim)
            {
                CreateFireSim();                
            }

            //can only run together
            if(input.Simulation.RunSmokeSim && input.Simulation.RunFireSim)
            {
                //smokeBoxDispersionModel = new Smoke.BoxDispersionModel(fireMesh);
                if(_fireMesh == null)
                {
                    WUInity.LOG("WARNING: No fire mesh has been created, disabling smoke spread simulation.");
                    input.Simulation.RunSmokeSim = false;
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
                input.Simulation.RunSmokeSim = false;
            }

            if (input.Simulation.RunEvacSim)
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

                _macroHumanSim = new MacroHumanSim();
                //place people
                _macroHumanSim.PopulateCells(WUInity.RUNTIME_DATA.Routing.RouteCollections, WUInity.POPULATION.GetPopulationData());                
                //distribute people
                _macroHumanSim.PlaceHouseholdsInCells();
            }

            if (input.Simulation.RunTrafficSim)
            {
                _macroTrafficSim = new MacroTrafficSim(RouteCreator);
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
            _time = 0f;
            for (int i = 0; i < WUInity.RUNTIME_DATA.Evacuation.ResponseCurves.Length; i++)
            {
                float t = WUInity.RUNTIME_DATA.Evacuation.ResponseCurves[i].dataPoints[0].time + input.Evacuation.EvacuationOrderStart;
                _time = Mathf.Min(Time, t);
            }
            _startTime = Time;

            if(input.Simulation.RunTrafficSim)
            {
                for (int i = 0; i < WUInity.INPUT.Traffic.trafficAccidents.Length; i++)
                {
                    _macroTrafficSim.InsertNewTrafficEvent(WUInity.INPUT.Traffic.trafficAccidents[i]);
                }

                for (int i = 0; i < WUInity.INPUT.Traffic.reverseLanes.Length; i++)
                {
                    _macroTrafficSim.InsertNewTrafficEvent(WUInity.INPUT.Traffic.trafficAccidents[i]);
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
                    UpdateSimLoop();
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

        void UpdateSimStatus()
        {
            if(_stopSim)
            {
                return;
            }

            WUInityInput input = WUInity.INPUT;
            _stopSim = Time <= input.Simulation.MaxSimTime ? false : true;

            if (input.Simulation.StopWhenEvacuated)
            {
                bool evacDone = true;
                if (input.Simulation.RunEvacSim)
                {
                    evacDone = _macroHumanSim.evacuationDone;
                }
                bool trafficDone = true;
                if (input.Simulation.RunTrafficSim)
                {
                    trafficDone = _macroTrafficSim.EvacComplete();
                }

                if (evacDone && trafficDone)
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
                WUInity.OUTPUT.totalEvacTime = Time;
                UpdateSimLoop();
            }
            else if(IsRunning)
            {
                IsRunning = false;
                SaveOutput(0);
                WUInity.LOG("LOG: Simulation done.");

                //plot results
                List<float> arrivalData = _macroTrafficSim.GetArrivalData();
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

        
        float nextFireUpdate;
        private void UpdateSimLoop()
        {
            WUInityInput input = WUInity.INPUT;
            
            if(input.Simulation.RunTrafficSim)
            {
                //check for global events
                if(WUInity.RUNTIME_DATA.Evacuation.BlockGoalEvents != null)
                {
                    for (int i = 0; i < WUInity.RUNTIME_DATA.Evacuation.BlockGoalEvents.Length; i++)
                    {
                        BlockGoalEvent bGE = WUInity.RUNTIME_DATA.Evacuation.BlockGoalEvents[i];
                        if (Time >= bGE.startTime && !bGE.triggered)
                        {
                            bGE.ApplyEffects();
                        }
                    }
                }                
            }

            //update fire mesh if needed
            bool fireUpdated = false;
            if (input.Simulation.RunFireSim)
            {   
                if (Time >= 0.0f && Time >= nextFireUpdate)
                {
                    fireUpdated = true;
                    _fireMesh.Simulate();
                    nextFireUpdate += (float)_fireMesh.dt;
                    //check if any goal has been blocked by fire
                    CheckEvacuationGoalStatus();
                    //can get set when evac goals are all gone
                    if (_stopSim)
                    {
                        return;
                    }
                }               
            }

            //sync with fire
            if(Time >= 0.0f && input.Simulation.RunSmokeSim)
            {
                //smokeBoxDispersionModel.Update(input.deltaTime, fireMesh.currentWindData.direction, fireMesh.currentWindData.speed);
                _advectDiffuseSim.Update(input.Simulation.DeltaTime, _fireMesh.currentWindData.direction, _fireMesh.currentWindData.speed, fireUpdated);
            }

            //advance evac
            if (input.Simulation.RunEvacSim)
            {
                _macroHumanSim.Update(input.Simulation.DeltaTime, Time);
            }

            //advance traffic
            if (input.Simulation.RunTrafficSim)
            {
                _macroTrafficSim.AdvanceTrafficSimulation(input.Simulation.DeltaTime, Time);
            }

            //increase time
            float deltaTime = input.Simulation.DeltaTime;
            if (input.Simulation.RunFireSim && !input.Simulation.RunEvacSim && !input.Simulation.RunTrafficSim)
            {
                deltaTime = (float)_fireMesh.dt;
            }
            _time += deltaTime;
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
                        WUInity.LOG("LOG: Goal blocked by fire: " + eG.name);
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
                WUInity.LOG(stopMessage);
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
                StopSim("STOP: No evacuation goals available.");
                return;
            }

            //update raster evac routes first as traffic might use some of the updated choices
            int cellsWithoutRouteButNoonePlansToLeave = 0;
            if(WUInity.RUNTIME_DATA.Routing.RouteCollections != null)
            {
                for (int i = 0; i < WUInity.RUNTIME_DATA.Routing.RouteCollections.Length; i++)
                {
                    if (WUInity.RUNTIME_DATA.Routing.RouteCollections[i] != null && !_macroHumanSim.IsCellEvacuated(i))
                    {
                        if(_macroHumanSim.GetPeopleLeftInCellIntendingToLeave(i) > 0)
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
            if(cellsWithoutRouteButNoonePlansToLeave > 0)
            {
                WUInity.LOG("LOG: " + cellsWithoutRouteButNoonePlansToLeave +  " cells have no routes left after goal was blocked, but noone left planning to leave from those cells.");
            }
            //update cars already in traffic
            _macroTrafficSim.UpdateEvacuationGoals();              
        }

        void SaveOutput(int runNumber)
        {
            WUInityInput input = WUInity.INPUT;
            if (input.Simulation.RunTrafficSim)
            {
                WUInity.LOG("LOG: Total cars in simulation: " + _macroTrafficSim.GetTotalCarsSimulated());
                _macroTrafficSim.SaveToFile(runNumber);
            }
            if (input.Simulation.RunEvacSim)
            {
                _macroHumanSim.SaveToFile(runNumber);
            }

            WUInityOutput.SaveOutput(WUInity.INPUT.Simulation.SimulationID + "_" + runNumber);            
        }
    }    
}