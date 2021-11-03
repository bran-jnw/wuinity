﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Itinero;
using Itinero.IO.Osm;
using Itinero.Osm.Vehicles;
using System.IO;
using OsmSharp.Streams;
using WUInity.Evac;
using WUInity.Traffic;
using WUInity.GPW;

namespace WUInity
{
    [System.Serializable]
    public class WUInitySim
    {     
        public bool runEvacSim = true;
        public bool runTrafficSim = true;
        public bool runFireSim = true;
        public bool showResults = false;

        private bool stopSim;
        private RouteCreator routeCreator;
        private RouterDb routerDb;
        private MacroTrafficSim macroTrafficSim;
        private MacroHumanSim macroHumanSim;
        private Fire.WUInityFireMesh fireMesh;

        private float startTime;
        public float StartTime 
        {
            get { return startTime; }
        }

        
        private  RouteCollection[] routes;

        private List<string> simLog = new List<string>(200);

        public WUInitySim()
        {

        }

        public RouteCreator GetRouteCreator()
        {
            return routeCreator;
        }

        public RouteCollection[] GetRouteCollection()
        {
            return routes;
        }

        public void LoadRouteCollections()
        {
            RouteCollection[] newRoutes = SaveWUI.LoadRouteCollections(WUInity.WUINITY_IN.traffic.precalcRoutesName);
            if(newRoutes != null)
            {
                routes = newRoutes;

                //update the selected route based on the current route choice (as that might have been changed)
                for (int i = 0; i < routes.Length; i++)
                {
                    if(routes[i] != null)
                    {
                        RouteCreator.SelectCorrectRoute(routes[i], true, i);
                    }
                }
            }            
        }

        public RouterDb GetRouterDb()
        {
            return routerDb;
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

        public void LogMessage(string message)
        {
            if(simRunning)
            {
                message = "[" + (int)time + "s] " + message;
            }
            
            simLog.Add(message);
            WUInity.WUINITY_GUI.PrintInfo(message);
            Debug.Log(message);
        }

        //create or lead itinero database needed for pathfinding
        public void LoadItineroDatabase()
        {
            WUInityInput input = WUInity.WUINITY_IN;

            string internalRouterName = input.itinero.routerDatabaseName + ".routerdb";
            string internalOSMName = input.itinero.osmDataName + ".osm.pbf";
            if (File.Exists(Application.dataPath + "/Resources/_input/osm_itinero/" + internalRouterName))
            {
                using (FileStream stream = new FileInfo(Application.dataPath + "/Resources/_input/osm_itinero/" + internalRouterName).OpenRead())
                {
                    routerDb = RouterDb.Deserialize(stream);
                }
                //routerDb.AddContracted(routerDb.GetSupportedProfile("car"));
            }
            else
            {
                // load some routing data and build a routing network.
                routerDb = new RouterDb();
                using (FileStream stream = new FileInfo(Application.dataPath + "/Resources/_input/osm_itinero/" + internalOSMName).OpenRead())
                {
                    PBFOsmStreamSource source = new PBFOsmStreamSource(stream);
                    Vector2D border = GPWData.SizeToDegrees(input.lowerLeftLatLong, new Vector2D(WUInity.WUINITY_IN.itinero.osmBorderSize, WUInity.WUINITY_IN.itinero.osmBorderSize));
                    float left = (float)(input.lowerLeftLatLong.y - border.x);
                    float bottom = (float)(input.lowerLeftLatLong.x - border.y);
                    Vector2D size = GPWData.SizeToDegrees(input.lowerLeftLatLong, input.size);
                    float right = (float)(input.lowerLeftLatLong.y + size.x + border.x);
                    float top = (float)(input.lowerLeftLatLong.x + size.y + border.y);                    
                    //print("" + left + ", " + bottom + ", " + right + ", " + top);
                    OsmStreamSource filtered = source.FilterBox(left, top, right, bottom, true);
                    //create a new filtered file
                    using (FileStream targetStream = File.OpenWrite(Application.dataPath + "/Resources/_input/osm_itinero/" + input.itinero.routerDatabaseName + "_filtered.osm.pbf"))
                    {
                        PBFOsmStreamTarget target = new PBFOsmStreamTarget(targetStream, compress: false);
                        target.RegisterSource(filtered);
                        target.Pull();
                    }
                    // create the network for cars only.
                    routerDb.LoadOsmData(filtered, Vehicle.Car);
                }

                // write the routerdb to disk.
                using (FileStream stream = new FileInfo(Application.dataPath + "/Resources/_input/osm_itinero/" + internalRouterName).Open(FileMode.Create))
                {
                    routerDb.Serialize(stream);
                }
            }

            LogMessage("Router database loaded succesfully.");
        }

        public void UpdateNeededData()
        {
            WUInityInput input = WUInity.WUINITY_IN;

            //do some calc for number of cells and set parameters in directions manager
            input.evac.routeCellCount = new Vector2Int(Mathf.CeilToInt((float)input.size.x / input.evac.routeCellSize), Mathf.CeilToInt((float)input.size.y / input.evac.routeCellSize));
            if (routeCreator == null)
            {
                routeCreator = new RouteCreator();
            }

            //offset response curve based on evac order
            /*for (int i = 0; i < input.evac.responseCurves.Length; i++)
            {
                ResponseCurve.CorrectForEvacOrderTime(input.evac.responseCurves[i], input.evac.evacuationOrderStart);
            }*/                     
        }        

        public bool StartSimFromGUI()
        {
            bool worked = false;
            if (VerifySimulationRequirements())
            {
                worked = true;
                WUInity.WUINITY.SetSampleMode(WUInity.DataSampleMode.TrafficDens);
                StartSimulation();                
            }
            return worked;
        }
        
        bool VerifySimulationRequirements()
        {            
            string error = "ERROR: ";
            bool failedInit = false;
            //is actually needed since we set a bunch of parameters in LoadMapbox()
            if (WUInity.WUINITY_MAP == null)
            {
                error += "No map loaded. ";
                failedInit = true;
            }
            //discard?
            if (WUInity.WUINITY_FARSITE == null)
            {
                error += "No FARSITE data loaded. ";
            }

            //all things evac
            if(WUInity.WUINITY_SIM.runEvacSim)
            {
                //need gpw data
                if (WUInity.WUINITY_GPW == null)
                {
                    error += "No GPW data loaded. ";
                    failedInit = true;
                }
            }

            //all things traffic
            if (WUInity.WUINITY_SIM.runTrafficSim)
            {
                //need router db
                if (routerDb == null)
                {
                    error += "No router database loaded. ";
                    failedInit = true;
                }
            }
                

            if (failedInit)
            {
                LogMessage(error);
                return false;
            }

            //creates all objects (traffic sim, route creator, evac sim)
            UpdateNeededData();

            return true;
        }

        
        public bool simRunning = false;
        private void StartSimulation()
        {

            WUInityInput input = WUInity.WUINITY_IN;            

            if(input.runInRealTime)
            {
                CreateSims(0);
                RunSimulation(0);
                //System.Threading.Thread thread = new System.Threading.Thread(RunSimThread);
                //thread.Start();
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
                WUInity.WUINITY_OUT.totalEvacTime = time;
                simRunning = false;
                showResults = true;
                LogMessage("Simulation done.");
            }
        }  

        /*void RunSimThread()
        {
            while(simRunning)
            {
                if(WUInity.WUINITY_IN.runInRealTime)
                {
                    UpdateRealtimeSim();
                }
            }
        }*/
        
        private void CreateSims(int i)
        {
            WUInityInput input = WUInity.WUINITY_IN;
            WUInityOutput output = WUInity.WUINITY_OUT;
            GPWViewer gpwViewer = WUInity.WUINITY_GPW;

            if (runFireSim)
            {
                CreateFireSim();
            }

            if (runEvacSim)
            {
                macroHumanSim = new MacroHumanSim();
                //place people
                macroHumanSim.PopulateCells(input.evac.routeCellCount, input.size, gpwViewer.gpwData);
                if (i == 0)
                {
                    //save raw pop
                    output.evac.rawPopTexture = macroHumanSim.CreatePopulationTexture();
                    //calc routes for people if not already loaded
                    LoadRouteCollections();
                    if(routes == null)
                    {
                        routes = routeCreator.CalculateCellRoutes();
                        SaveWUI.SaveRouteCollections(WUInity.WUINITY_IN.traffic.precalcRoutesName);
                    }                    
                }
                //apply routes
                macroHumanSim.SetRasterRoutes(routes);
                //fix any people being stuck
                int stuckPeople = macroHumanSim.CollectStuckPeople();
                macroHumanSim.RelocateStuckPeople(stuckPeople);
                //rescale population if needed, then place households
                if (input.evac.overrideTotalPopulation)
                {
                    output.evac.actualTotalEvacuees = macroHumanSim.ScaleTotalPopulation(input.evac.totalPopulation);
                }
                //distribute people
                macroHumanSim.PlaceHouseholdsInCells();

                if (i == 0)
                {
                    //create textures                        
                    output.evac.popStuckTexture = macroHumanSim.CreateStuckPopulationTexture();
                    output.evac.relocatedPopTexture = macroHumanSim.CreatePopulationTexture();
                    output.evac.popStayingTexture = macroHumanSim.CreateStayingPopulationTexture();
                    WUInity.WUINITY.SaveTexturesToDisk();
                }
            }

            if (runTrafficSim)
            {
                macroTrafficSim = new MacroTrafficSim(routeCreator);
            }
        }

        private void CreateFireSim()
        {            
            fireMesh = new Fire.WUInityFireMesh(WUInity.WUINITY_IN.fire.lcpFileName, WUInity.WUINITY_IN.fire.weather, WUInity.WUINITY_IN.fire.wind, WUInity.WUINITY_IN.fire.initialFuelMoisture, WUInity.WUINITY_IN.fire.ignitionPoints);
            fireMesh.terrainMesh = WUInity.WUINITY.terrainMeshFilter.mesh;
            fireMesh.spreadMode = WUInity.WUINITY_IN.fire.spreadMode;
            //WUInity.WUINITY.terrainMeshFilter.gameObject.GetComponent<MeshRenderer>().material = WUInity.WUINITY.fireMaterial;            
        }

        private void RunSimulation(int runNumber)
        {
            WUInityInput input = WUInity.WUINITY_IN;

            //if we do multiple runs the goals have to be reset
            for (int i = 0; i < WUInity.WUINITY_IN.traffic.evacuationGoals.Length; i++)
            {
                WUInity.WUINITY_IN.traffic.evacuationGoals[i].ResetPeopleAndCars();
            }

            //pick start time based on curve or 0 (fire start)
            time = 0f;
            for (int i = 0; i < input.evac.responseCurves.Length; i++)
            {
                float t = input.evac.responseCurves[i].dataPoints[0].timeMinMax.x;
                time = Mathf.Min(time, t);
            }
            startTime = time;

            if(runTrafficSim)
            {
                for (int i = 0; i < WUInity.WUINITY_IN.traffic.trafficAccidents.Length; i++)
                {
                    macroTrafficSim.InsertNewTrafficEvent(WUInity.WUINITY_IN.traffic.trafficAccidents[i]);
                }

                for (int i = 0; i < WUInity.WUINITY_IN.traffic.reverseLanes.Length; i++)
                {
                    macroTrafficSim.InsertNewTrafficEvent(WUInity.WUINITY_IN.traffic.trafficAccidents[i]);
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
            if(runTrafficSim)
            {
                LogMessage("Total cars in simulation: " + macroTrafficSim.GetTotalCarsSimulated());
                macroTrafficSim.SaveToFile(runNumber);
            }
            if(runEvacSim)
            {
                macroHumanSim.SaveToFile(runNumber);
            }            
            
            SaveWUI.SaveOutput(WUInity.WUINITY_IN.simName + "_" + runNumber);
        }

        void UpdateSimStatus()
        {
            WUInityInput input = WUInity.WUINITY_IN;
            stopSim = time <= input.maxSimTime ? false : true;

            if (input.stopWhenEvacuated)
            {
                bool evacDone = true;
                if (runEvacSim)
                {
                    evacDone = macroHumanSim.evacuationDone;
                }
                bool trafficDone = true;
                if (runTrafficSim)
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
                WUInity.WUINITY_OUT.totalEvacTime = time;
                UpdateSimLoop();
            }
            else if(simRunning)
            {
                simRunning = false;
                SaveOutput(0);                                
                LogMessage("Simulation done.");
            }
        }

        float time;
        float nextFireUpdate;
        private void UpdateSimLoop()
        {
            WUInityInput input = WUInity.WUINITY_IN;
            
            if(runTrafficSim)
            {
                //check for global events
                for (int i = 0; i < WUInity.WUINITY_IN.evac.blockGoalEvents.Length; i++)
                {
                    BlockGoalEvent bGE = WUInity.WUINITY_IN.evac.blockGoalEvents[i];
                    if (time >= bGE.startTime && !bGE.triggered)
                    {
                        bGE.ApplyEffects();
                    }
                }
            }
            
            //update fire mesh if needed
            if (runFireSim)
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
            if (runEvacSim)
            {
                macroHumanSim.AdvanceEvacuatePopulation(input.deltaTime, time);
            }

            //advance traffic
            if (runTrafficSim)
            {
                macroTrafficSim.AdvanceTrafficSimulation(input.deltaTime, time);
            }

            //increase time
            float deltaTime = input.deltaTime;
            if (runFireSim && !runEvacSim && !runTrafficSim)
            {
                deltaTime = (float)fireMesh.dt;
            }
            time += deltaTime;
        }

        void CheckEvacuationGoalStatus()
        {
            for (int i = 0; i < WUInity.WUINITY_IN.traffic.evacuationGoals.Length; i++)
            {
                EvacuationGoal eG = WUInity.WUINITY_IN.traffic.evacuationGoals[i];
                if(!eG.blocked)
                {
                    Fire.FireCellState cellState = fireMesh.GetFireCellState(eG.latLong);
                    if (cellState == Fire.FireCellState.Burning)
                    {
                        LogMessage("Goal blocked by fire: " + eG.name);
                        BlockEvacGoal(i);
                    }
                }                
            }
        }

        public void StopSim(string stopMessage)
        {
            stopSim = true;
            LogMessage(stopMessage);
        }

        public void BlockEvacGoal(int index)
        {
            if (!WUInity.WUINITY_IN.traffic.evacuationGoals[index].blocked)
            {
                WUInity.WUINITY_IN.traffic.evacuationGoals[index].blocked = true;
                UpdateRoutes();
            }
        }

        public Fire.WUInityFireMesh GetFireMesh()
        {
            if(fireMesh == null)
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

        public int[,] GetWUIarea()
        {
            return new int[1, 1];
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
            for (int i = 0; i < WUInity.WUINITY_IN.traffic.evacuationGoals.Length; i++)
            {
                if(!WUInity.WUINITY_IN.traffic.evacuationGoals[i].blocked)
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
            if(routes != null)
            {
                for (int i = 0; i < routes.Length; i++)
                {
                    if (routes[i] != null)
                    {
                        routes[i].CheckAndUpdateRoute();
                    }
                }
            }            
            //update cars already in traffic
            macroTrafficSim.UpdateEvacuationGoals();
              
        }

        public EvacuationGoal GetForcedGoal(int x, int y)
        {
            WUInityInput input = WUInity.WUINITY_IN;

            if (input.evac.paintedForcedGoals.Length < input.evac.routeCellCount.x * input.evac.routeCellCount.y)
            {
                return null;
            }
            return input.evac.paintedForcedGoals[x + y * input.evac.routeCellCount.x];
        }

        public EvacGroup GetEvacGroup(int x, int y)
        {
            WUInityInput input = WUInity.WUINITY_IN;
            if (input.evac.evacGroupIndices.Length < input.evac.routeCellCount.x * input.evac.routeCellCount.y)
            {
                return null;
            }

            int index = x + y * WUInity.WUINITY_IN.evac.routeCellCount.x;
            index = WUInity.WUINITY_IN.evac.evacGroupIndices[index];
            return WUInity.WUINITY_IN.evac.evacGroups[index];
        }

        public RouteCollection GetCellRouteCollection(Vector2D pos)
        {   
            if(routes != null)
            {
                Mapbox.Utils.Vector2d p = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(pos.x, pos.y, WUInity.WUINITY_MAP.CenterMercator, WUInity.WUINITY_MAP.WorldRelativeScale);
                int x = (int)(p.x / WUInity.WUINITY_IN.evac.routeCellSize);
                int y = (int)(p.y / WUInity.WUINITY_IN.evac.routeCellSize);
                int index = x + y * WUInity.WUINITY_IN.evac.routeCellCount.x;
                if (index >= 0 && index < routes.Length && routes[index] != null)
                {
                    return routes[index];
                }
            }            

            return null;
        }

        public void UpdateEvacGroups(int[] evacGroupIndices)
        {
            WUInity.WUINITY_IN.evac.evacGroupIndices = new int[WUInity.WUINITY_IN.evac.routeCellCount.x * WUInity.WUINITY_IN.evac.routeCellCount.y];
            for (int y = 0; y < WUInity.WUINITY_IN.evac.routeCellCount.y; y++)
            {
                for (int x = 0; x < WUInity.WUINITY_IN.evac.routeCellCount.x; x++)
                {
                    int index = x + y * WUInity.WUINITY_IN.evac.routeCellCount.x;
                    if (evacGroupIndices != null)
                    {
                        WUInity.WUINITY_IN.evac.evacGroupIndices[index] = evacGroupIndices[index];
                    }
                    else
                    {
                        WUInity.WUINITY_IN.evac.evacGroupIndices[index] = 0;
                    }                    
                }
            }
        }

        public void UpdateWUIArea(int[] wuiAreaIndices)
        {
            if(wuiAreaIndices == null)
            {
                wuiAreaIndices = new int[GetFireMesh().cellCount.x * GetFireMesh().cellCount.y];
            }
            WUInity.WUINITY_IN.fire.wuiAreaIndices = wuiAreaIndices;
        }

        public void UpdateRandomIgnitionIndices(int[] randomIgnitionIndices)
        {
            if (randomIgnitionIndices == null)
            {
                randomIgnitionIndices = new int[GetFireMesh().cellCount.x * GetFireMesh().cellCount.y];
            }
            WUInity.WUINITY_IN.fire.randomIgnitionIndices = randomIgnitionIndices;
        }

        public void UpdateInitialIgnitionIndices(int[] initialIgnitionIndices)
        {
            if (initialIgnitionIndices == null)
            {
                initialIgnitionIndices = new int[GetFireMesh().cellCount.x * GetFireMesh().cellCount.y];
            }
            WUInity.WUINITY_IN.fire.initialIgnitionIndices = initialIgnitionIndices;
        }
    }    
}