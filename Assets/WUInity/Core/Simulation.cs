using System.Collections;
using UnityEngine;
using WUInity.Evac;
using WUInity.Traffic;
using WUInity.Fire;

namespace WUInity
{
    [System.Serializable]
    public class Simulation
    {    
        public bool haveResults = false;
        public bool isRunning = false;

        private bool stopSim;

        private RouteCreator routeCreator;        
        private MacroTrafficSim macroTrafficSim;
        private MacroHumanSim macroHumanSim;
        private FireMesh fireMesh;
        private Smoke.BoxDispersionModel smokeBoxDispersionModel;
        private Smoke.AdvectDiffuseModel advectDiffuseSim;


        private float startTime;
        public float StartTime 
        {
            get { return startTime; }
        }
        public float time;       

        public Simulation()
        {

        }

        public MacroTrafficSim GetMacroTrafficSim()
        {
            return macroTrafficSim;
        }

        public MacroHumanSim GetMacroHumanSim()
        {
            return macroHumanSim;
        }

        /// <summary>
        /// Should only be set by traffic verification basically, otherwise it is internally created.
        /// </summary>
        /// <param name="mTS"></param>
        public void SetMacroTrafficSim(MacroTrafficSim mTS)
        {
            macroTrafficSim = mTS;
        }

        public Fire.FireMesh GetFireMesh()
        {
            if (fireMesh == null)
            {
                CreateFireSim();
            }

            return fireMesh;
        }

        public float GetFireWindSpeed()
        {
            return fireMesh.currentWindData.speed;
        }

        public float GetFireWindDirection()
        {
            return fireMesh.currentWindData.direction;
        }

        public Smoke.BoxDispersionModel GetSmokeDispersion()
        {            
            return smokeBoxDispersionModel;
        }

        public Smoke.AdvectDiffuseModel GetAdvectDiffuseSim()
        {
            return advectDiffuseSim;
        }

        public RouteCreator GetRouteCreator()
        {
            return routeCreator;
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
                for (int i = 0; i < input.numberOfRuns; i++)
                {
                    CreateSubSims(i);
                    //do actual simulation
                    RunSimulation(i);
                    //force garbage collection
                    //Resources.UnloadUnusedAssets();                    
                    System.GC.Collect();
                }
                WUInity.OUTPUT.totalEvacTime = time;
                isRunning = false;
                haveResults = true;
                WUInity.WUI_LOG("LOG: Simulation done.");
            }
        }
        
        private void CreateSubSims(int i)
        {
            WUInityInput input = WUInity.INPUT;

            if(routeCreator == null)
            {
                routeCreator = new RouteCreator();
            }

            if (input.runFireSim)
            {
                CreateFireSim();                
            }

            //can only run together
            if(input.runSmokeSim && input.runFireSim)
            {
                //smokeBoxDispersionModel = new Smoke.BoxDispersionModel(fireMesh);
                advectDiffuseSim = new Smoke.AdvectDiffuseModel(fireMesh, 250f, WUInity.INSTANCE.advectDiffuseCompute, WUInity.INSTANCE.noiseTex);
            }
            else
            {
                input.runSmokeSim = false;
            }

            if (input.runEvacSim)
            {
                if (i == 0)
                {     
                    if (!WUInity.DATA_STATUS.routeCollectionLoaded)
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

                macroHumanSim = new MacroHumanSim();
                //place people
                macroHumanSim.PopulateCells(WUInity.SIM_DATA.routes, WUInity.POPULATION.GetPopulationData());                
                //distribute people
                macroHumanSim.PlaceHouseholdsInCells();
            }

            if (input.runTrafficSim)
            {
                macroTrafficSim = new MacroTrafficSim(routeCreator);
            }
        }

        private void CreateFireSim()
        {            
            fireMesh = new FireMesh(WUInity.INPUT.fire.lcpFile, WUInity.INPUT.fire.weather, WUInity.INPUT.fire.wind, WUInity.INPUT.fire.initialFuelMoisture, WUInity.INPUT.fire.ignitionPoints);
            fireMesh.spreadMode = WUInity.INPUT.fire.spreadMode;           
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
            time = 0f;
            for (int i = 0; i < input.evac.responseCurves.Length; i++)
            {
                float t = input.evac.responseCurves[i].dataPoints[0].timeMinMax.x + input.evac.evacuationOrderStart;
                time = Mathf.Min(time, t);
            }
            startTime = time;

            if(input.runTrafficSim)
            {
                for (int i = 0; i < WUInity.INPUT.traffic.trafficAccidents.Length; i++)
                {
                    macroTrafficSim.InsertNewTrafficEvent(WUInity.INPUT.traffic.trafficAccidents[i]);
                }

                for (int i = 0; i < WUInity.INPUT.traffic.reverseLanes.Length; i++)
                {
                    macroTrafficSim.InsertNewTrafficEvent(WUInity.INPUT.traffic.trafficAccidents[i]);
                }
            }            

            stopSim = false;
            isRunning = true;
            nextFireUpdate = 0f;  
            if(!input.runInRealTime)
            {   
                while (!stopSim)
                {
                    //checks if we are done
                    UpdateSimStatus();    
                    UpdateSimLoop();
                    if (stopSim)
                    {
                        break;
                    }
                }
                SaveOutput(runNumber);
            }
            else
            {
                haveResults = true;
            }
        }        

        void UpdateSimStatus()
        {
            if(stopSim)
            {
                return;
            }

            WUInityInput input = WUInity.INPUT;
            stopSim = time <= input.maxSimTime ? false : true;

            if (input.stopWhenEvacuated)
            {
                bool evacDone = true;
                if (input.runEvacSim)
                {
                    evacDone = macroHumanSim.evacuationDone;
                }
                bool trafficDone = true;
                if (input.runTrafficSim)
                {
                    trafficDone = macroTrafficSim.EvacComplete();
                }

                if (evacDone && trafficDone)
                {
                    stopSim = true;
                }
            }
        }

        public void UpdateRealtimeSim()
        {
            if(!isRunning)
            {
                return;
            }

            //checks if we are done
            UpdateSimStatus();
            if (!stopSim)
            {
                WUInity.OUTPUT.totalEvacTime = time;
                UpdateSimLoop();
            }
            else if(isRunning)
            {
                isRunning = false;
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
                    if (time >= bGE.startTime && !bGE.triggered)
                    {
                        bGE.ApplyEffects();
                    }
                }
            }

            //update fire mesh if needed
            bool fireUpdated = false;
            if (input.runFireSim)
            {   
                if (time >= 0.0f && time >= nextFireUpdate)
                {
                    fireUpdated = true;
                    fireMesh.Simulate();
                    nextFireUpdate += (float)fireMesh.dt;
                    //check if any goal has been blocked by fire
                    CheckEvacuationGoalStatus();
                    //can get set when evac goals are all gone
                    if (stopSim)
                    {
                        return;
                    }
                }               
            }

            //sync with fire
            if(time >= 0.0f && input.runSmokeSim)
            {
                //smokeBoxDispersionModel.Update(input.deltaTime, fireMesh.currentWindData.direction, fireMesh.currentWindData.speed);
                advectDiffuseSim.Update(input.deltaTime, fireMesh.currentWindData.direction, fireMesh.currentWindData.speed, fireUpdated);
            }

            //advance evac
            if (input.runEvacSim)
            {
                macroHumanSim.Update(input.deltaTime, time);
            }

            //advance traffic
            if (input.runTrafficSim)
            {
                macroTrafficSim.AdvanceTrafficSimulation(input.deltaTime, time);
            }

            //increase time
            float deltaTime = input.deltaTime;
            if (input.runFireSim && !input.runEvacSim && !input.runTrafficSim)
            {
                deltaTime = (float)fireMesh.dt;
            }
            time += deltaTime;
        }

        void CheckEvacuationGoalStatus()
        {
            for (int i = 0; i < WUInity.INPUT.traffic.evacuationGoals.Length; i++)
            {
                EvacuationGoal eG = WUInity.INPUT.traffic.evacuationGoals[i];
                if(!eG.blocked)
                {
                    Fire.FireCellState cellState = fireMesh.GetFireCellState(eG.latLong);
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
            if(!stopSim)
            {
                stopSim = true;
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
                    if (WUInity.SIM_DATA.routes[i] != null && !macroHumanSim.IsCellEvacuated(i))
                    {
                        if(macroHumanSim.GetPeopleLeftInCellIntendingToLeave(i) > 0)
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
            macroTrafficSim.UpdateEvacuationGoals();              
        }

        void SaveOutput(int runNumber)
        {
            WUInityInput input = WUInity.INPUT;
            if (input.runTrafficSim)
            {
                WUInity.WUI_LOG("LOG: Total cars in simulation: " + macroTrafficSim.GetTotalCarsSimulated());
                macroTrafficSim.SaveToFile(runNumber);
            }
            if (input.runEvacSim)
            {
                macroHumanSim.SaveToFile(runNumber);
            }

            SaveLoadWUI.SaveOutput(WUInity.INPUT.simName + "_" + runNumber);
        }
    }    
}