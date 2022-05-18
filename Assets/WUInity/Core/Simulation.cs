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

            if(input.runInRealTime)
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
                for (int i = 0; i < input.numberOfRuns; i++)
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
                        if (convergenceCriteria < WUInity.INPUT.convergenceCriteria)
                        {
                            ++convergedInSequence;
                            //we are done
                            if(WUInity.INPUT.stopAfterConverging && convergedInSequence > 10)
                            {
                                i = input.numberOfRuns;
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

                if(convergedInSequence >= 10)
                {
                    WUInity.WUI_LOG("LOG: Average total evacuation time: " + averageTotalEvacTime / actualRuns + " seconds, ran " + actualRuns + " simulations before converging according to user set criteria.");
                }
                else
                {
                    WUInity.WUI_LOG("LOG: Average total evacuation time: " + averageTotalEvacTime / actualRuns + " seconds, ran " + actualRuns + " simulations.");
                }
                WUInity.OUTPUT.totalEvacTime = Time;
                IsRunning = false;
                HaveResults = true;
                WUInity.WUI_LOG("LOG: Simulation done.");
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
            string path = System.IO.Path.Combine(WUInity.OUTPUT_FOLDER, wuiIn.simName + "_traffic_average.csv");
            System.IO.File.WriteAllLines(path, output);
        }
        
        private void CreateSubSims(int i)
        {
            WUInityInput input = WUInity.INPUT;

            if (input.runFireSim)
            {
                CreateFireSim();                
            }

            //can only run together
            if(input.runSmokeSim && input.runFireSim)
            {
                //smokeBoxDispersionModel = new Smoke.BoxDispersionModel(fireMesh);
                if(_fireMesh == null)
                {
                    WUInity.WUI_LOG("WARNING: No fire mesh has been created, disabling smoke spread simulation.");
                    input.runSmokeSim = false;
                }
                else
                {    
                    _advectDiffuseSim = new Smoke.AdvectDiffuseModel(_fireMesh, 250f, WUInity.INSTANCE.AdvectDiffuseCompute, WUInity.INSTANCE.NoiseTex, WUInity.INSTANCE.WindTex);
                }                
            }
            else
            {
                input.runSmokeSim = false;
            }

            if (input.runEvacSim)
            {
                if (i == 0)
                {     
                    if (!WUInity.DATA_STATUS.RouteCollectionLoaded)
                    {
                        WUInity.SIM_DATA.LoadRouteCollections();
                    }                      
                    //we could not load from disk, so have to build all routes
                    if (WUInity.SIM_DATA.routes == null)
                    {
                        WUInity.SIM_DATA.BuildAndSaveRouteCollection();
                    }

                    WUInity.POPULATION.GetPopulationData().UpdatePopulationBasedOnRoutes(WUInity.SIM_DATA.routes);
                }

                _macroHumanSim = new MacroHumanSim();
                //place people
                _macroHumanSim.PopulateCells(WUInity.SIM_DATA.routes, WUInity.POPULATION.GetPopulationData());                
                //distribute people
                _macroHumanSim.PlaceHouseholdsInCells();
            }

            if (input.runTrafficSim)
            {
                _macroTrafficSim = new MacroTrafficSim(RouteCreator);
            }
        }

        private void CreateFireSim()
        {            
            _fireMesh = new FireMesh(WUInity.INPUT.fire.lcpFile, WUInity.SIM_DATA.WeatherInput, WUInity.SIM_DATA.WindInput, WUInity.SIM_DATA.InitialFuelMoistureData, WUInity.INPUT.fire.ignitionPoints);
            _fireMesh.spreadMode = WUInity.INPUT.fire.spreadMode;           
        }

        private void RunSimulation(int runNumber)
        {
            WUInityInput input = WUInity.INPUT;

            //if we do multiple runs the goals have to be reset
            for (int i = 0; i < WUInity.INPUT.traffic.evacuationGoals.Length; i++)
            {
                WUInity.INPUT.traffic.evacuationGoals[i].ResetPeopleAndCars();
            }

            //pick start time based on curve or 0 (fire start)
            _time = 0f;
            for (int i = 0; i < WUInity.SIM_DATA.ResponseCurves.Length; i++)
            {
                float t = WUInity.SIM_DATA.ResponseCurves[i].dataPoints[0].time + input.evac.evacuationOrderStart;
                _time = Mathf.Min(Time, t);
            }
            _startTime = Time;

            if(input.runTrafficSim)
            {
                for (int i = 0; i < WUInity.INPUT.traffic.trafficAccidents.Length; i++)
                {
                    _macroTrafficSim.InsertNewTrafficEvent(WUInity.INPUT.traffic.trafficAccidents[i]);
                }

                for (int i = 0; i < WUInity.INPUT.traffic.reverseLanes.Length; i++)
                {
                    _macroTrafficSim.InsertNewTrafficEvent(WUInity.INPUT.traffic.trafficAccidents[i]);
                }
            }            

            _stopSim = false;
            IsRunning = true;
            nextFireUpdate = 0f;  
            if(!input.runInRealTime)
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
            _stopSim = Time <= input.maxSimTime ? false : true;

            if (input.stopWhenEvacuated)
            {
                bool evacDone = true;
                if (input.runEvacSim)
                {
                    evacDone = _macroHumanSim.evacuationDone;
                }
                bool trafficDone = true;
                if (input.runTrafficSim)
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
                WUInity.WUI_LOG("LOG: Simulation done.");
            }
        }

        
        float nextFireUpdate;
        private void UpdateSimLoop()
        {
            WUInityInput input = WUInity.INPUT;
            
            if(input.runTrafficSim)
            {
                //check for global events
                for (int i = 0; i < WUInity.INPUT.evac.blockGoalEvents.Length; i++)
                {
                    BlockGoalEvent bGE = WUInity.INPUT.evac.blockGoalEvents[i];
                    if (Time >= bGE.startTime && !bGE.triggered)
                    {
                        bGE.ApplyEffects();
                    }
                }
            }

            //update fire mesh if needed
            bool fireUpdated = false;
            if (input.runFireSim)
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
            if(Time >= 0.0f && input.runSmokeSim)
            {
                //smokeBoxDispersionModel.Update(input.deltaTime, fireMesh.currentWindData.direction, fireMesh.currentWindData.speed);
                _advectDiffuseSim.Update(input.deltaTime, _fireMesh.currentWindData.direction, _fireMesh.currentWindData.speed, fireUpdated);
            }

            //advance evac
            if (input.runEvacSim)
            {
                _macroHumanSim.Update(input.deltaTime, Time);
            }

            //advance traffic
            if (input.runTrafficSim)
            {
                _macroTrafficSim.AdvanceTrafficSimulation(input.deltaTime, Time);
            }

            //increase time
            float deltaTime = input.deltaTime;
            if (input.runFireSim && !input.runEvacSim && !input.runTrafficSim)
            {
                deltaTime = (float)_fireMesh.dt;
            }
            _time += deltaTime;
        }

        void CheckEvacuationGoalStatus()
        {
            for (int i = 0; i < WUInity.INPUT.traffic.evacuationGoals.Length; i++)
            {
                EvacuationGoal eG = WUInity.INPUT.traffic.evacuationGoals[i];
                if(!eG.blocked)
                {
                    Fire.FireCellState cellState = _fireMesh.GetFireCellState(eG.latLong);
                    if (cellState == Fire.FireCellState.Burning)
                    {
                        WUInity.WUI_LOG("LOG: Goal blocked by fire: " + eG.name);
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
                WUInity.WUI_LOG(stopMessage);
            }            
        }

        public void BlockEvacGoal(int index)
        {
            if (!WUInity.INPUT.traffic.evacuationGoals[index].blocked)
            {
                WUInity.INPUT.traffic.evacuationGoals[index].blocked = true;
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
            for (int i = 0; i < WUInity.INPUT.traffic.evacuationGoals.Length; i++)
            {
                if(!WUInity.INPUT.traffic.evacuationGoals[i].blocked)
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
            if(WUInity.SIM_DATA.routes != null)
            {
                for (int i = 0; i < WUInity.SIM_DATA.routes.Length; i++)
                {
                    if (WUInity.SIM_DATA.routes[i] != null && !_macroHumanSim.IsCellEvacuated(i))
                    {
                        if(_macroHumanSim.GetPeopleLeftInCellIntendingToLeave(i) > 0)
                        {
                            WUInity.SIM_DATA.routes[i].CheckAndUpdateRoute();
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
                WUInity.WUI_LOG("LOG: " + cellsWithoutRouteButNoonePlansToLeave +  " cells have no routes left after goal was blocked, but noone left planning to leave from those cells.");
            }
            //update cars already in traffic
            _macroTrafficSim.UpdateEvacuationGoals();              
        }

        void SaveOutput(int runNumber)
        {
            WUInityInput input = WUInity.INPUT;
            if (input.runTrafficSim)
            {
                WUInity.WUI_LOG("LOG: Total cars in simulation: " + _macroTrafficSim.GetTotalCarsSimulated());
                _macroTrafficSim.SaveToFile(runNumber);
            }
            if (input.runEvacSim)
            {
                _macroHumanSim.SaveToFile(runNumber);
            }

            SaveLoadWUI.SaveOutput(WUInity.INPUT.simName + "_" + runNumber);
        }
    }    
}