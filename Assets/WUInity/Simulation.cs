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
        public bool showResults = false;
        public bool simRunning = false;

        private bool stopSim;

        private RouteCreator routeCreator;        
        private MacroTrafficSim macroTrafficSim;
        private MacroHumanSim macroHumanSim;
        private FireMesh fireMesh;
        

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

        public RouteCreator GetRouteCreator()
        {
            return routeCreator;
        }
        
        public  void StartSimulation()
        {

            WUInityInput input = WUInity.INPUT;            

            if(input.runInRealTime)
            {
                CreateSims(0);
                RunSimulation(0);
            }
            else
            {
                for (int i = 0; i < input.numberOfRuns; i++)
                {
                    CreateSims(i);
                    //do actual simulation
                    RunSimulation(i);
                    //force garbage collection
                    //Resources.UnloadUnusedAssets();                    
                    System.GC.Collect();
                }
                WUInity.OUTPUT.totalEvacTime = time;
                simRunning = false;
                showResults = true;
                WUInity.WUI_LOG("LOG: Simulation done.");
            }
        }  
        
        private void CreateSims(int i)
        {
            WUInityInput input = WUInity.INPUT;

            if (input.runFireSim)
            {
                CreateFireSim();
            }

            if (input.runEvacSim)
            {
                if (i == 0)
                {
                    if(!WUInity.DATA_STATUS.routeCollectionLoaded)
                    {
                        WUInity.SIM_DATA.LoadRouteCollections();
                    }                      
                    //we could not load from disk, so have to build all routes
                    if (WUInity.SIM_DATA.routes == null)
                    {
                        WUInity.SIM_DATA.routes = routeCreator.CalculateCellRoutes();
                        SaveLoadWUI.SaveRouteCollections();
                    }
                }
                macroHumanSim = new MacroHumanSim();
                //place people
                macroHumanSim.PopulateCells(WUInity.SIM_DATA.routes, WUInity.POPULATION.GetPopulationData());
                
                //distribute people
                macroHumanSim.PlaceHouseholdsInCells();
                MonoBehaviour.FindObjectOfType<HouseholdRenderer>().CreateBuffer(macroHumanSim.GetHouseholdPositions().Length, WUInity.INPUT.size);
            }

            if (input.runTrafficSim)
            {
                macroTrafficSim = new MacroTrafficSim(routeCreator);
            }
        }

        private void CreateFireSim()
        {            
            fireMesh = new FireMesh(WUInity.INPUT.fire.lcpFile, WUInity.INPUT.fire.weather, WUInity.INPUT.fire.wind, WUInity.INPUT.fire.initialFuelMoisture, WUInity.INPUT.fire.ignitionPoints);
            fireMesh.terrainMesh = WUInity.INSTANCE.terrainMeshFilter.mesh;
            fireMesh.spreadMode = WUInity.INPUT.fire.spreadMode;
            //WUInity.WUINITY.terrainMeshFilter.gameObject.GetComponent<MeshRenderer>().material = WUInity.WUINITY.fireMaterial;            
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
                float t = input.evac.responseCurves[i].dataPoints[0].timeMinMax.x;
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
            simRunning = true;
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
                showResults = true;
            }
        }

        void SaveOutput(int runNumber)
        {
            WUInityInput input = WUInity.INPUT;
            if (input.runTrafficSim)
            {
                WUInity.WUI_LOG("LOG: Total cars in simulation: " + macroTrafficSim.GetTotalCarsSimulated());
                macroTrafficSim.SaveToFile(runNumber);
            }
            if(input.runEvacSim)
            {
                macroHumanSim.SaveToFile(runNumber);
            }            
            
            SaveLoadWUI.SaveOutput(WUInity.INPUT.simName + "_" + runNumber);
        }

        void UpdateSimStatus()
        {
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
            if(!simRunning)
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
            else if(simRunning)
            {
                simRunning = false;
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
            if (input.runFireSim)
            {   
                if (time >= 0.0f && time >= nextFireUpdate)
                {
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
            stopSim = true;
            WUInity.WUI_LOG(stopMessage);
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
                StopSim("Simulation stopped: No evacuation goals available.");
                return;
            }

            //update raster evac routes first as traffic might use some of the updated choices
            if(WUInity.SIM_DATA.routes != null)
            {
                for (int i = 0; i < WUInity.SIM_DATA.routes.Length; i++)
                {
                    if (WUInity.SIM_DATA.routes[i] != null)
                    {
                        WUInity.SIM_DATA.routes[i].CheckAndUpdateRoute();
                    }
                }
            }            
            //update cars already in traffic
            macroTrafficSim.UpdateEvacuationGoals();              
        }                 
    }    
}