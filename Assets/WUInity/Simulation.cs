using System.Collections;
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
    public class Simulation
    {    
        public bool showResults = false;

        private bool stopSim;
        private RouteCreator routeCreator;
        private RouterDb routerDb;
        private MacroTrafficSim macroTrafficSim;
        private MacroHumanSim macroHumanSim;
        private Fire.FireMesh fireMesh;
        public int[] evacGroupIndices;

        private float startTime;
        public float StartTime 
        {
            get { return startTime; }
        }
        public float time;

        private Vector2Int cellCount;
        public Vector2Int EvacCellCount
        {
            get 
            {
                WUInityInput input = WUInity.INPUT;
                cellCount.x = Mathf.CeilToInt((float)input.size.x / input.evac.routeCellSize);
                cellCount.y = Mathf.CeilToInt((float)input.size.y / input.evac.routeCellSize);
                return cellCount;
            }
        }

        private  RouteCollection[] routes;        

        public Simulation()
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

        public bool LoadRouteCollections()
        {
            RouteCollection[] newRoutes = SaveLoadWUI.LoadRouteCollections();
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
                return true;
            }  
            else
            {
                return false;
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

        //create or lead itinero database needed for pathfinding
        public bool LoadItineroDatabase()
        {
            WUInityInput input = WUInity.INPUT;

            string internalRouterName = input.simName + ".routerdb";
            string path = Path.Combine(WUInity.WORKING_FOLDER, internalRouterName);
            bool success = false;
            if (File.Exists(path))
            {
                using (FileStream stream = new FileInfo(path).OpenRead())
                {
                    routerDb = RouterDb.Deserialize(stream);
                    success = true;
                }
                //routerDb.AddContracted(routerDb.GetSupportedProfile("car"));
            }
            else
            {
                // load some routing data and build a routing network.
                routerDb = new RouterDb();
                using (FileStream stream = new FileInfo(input.traffic.osmFile).OpenRead())
                {
                    PBFOsmStreamSource source = new PBFOsmStreamSource(stream);
                    Vector2D border = LocalGPWData.SizeToDegrees(input.lowerLeftLatLong, new Vector2D(WUInity.INPUT.traffic.osmBorderSize, WUInity.INPUT.traffic.osmBorderSize));
                    float left = (float)(input.lowerLeftLatLong.y - border.x);
                    float bottom = (float)(input.lowerLeftLatLong.x - border.y);
                    Vector2D size = LocalGPWData.SizeToDegrees(input.lowerLeftLatLong, input.size);
                    float right = (float)(input.lowerLeftLatLong.y + size.x + border.x);
                    float top = (float)(input.lowerLeftLatLong.x + size.y + border.y);                    
                    //print("" + left + ", " + bottom + ", " + right + ", " + top);
                    OsmStreamSource filtered = source.FilterBox(left, top, right, bottom, true);
                    //create a new filtered file
                    string fName = Path.GetFileName(input.traffic.osmFile);
                    path = Path.Combine(WUInity.WORKING_FOLDER, "filtered_" + fName);
                    using (FileStream targetStream = File.OpenWrite(path))
                    {
                        PBFOsmStreamTarget target = new PBFOsmStreamTarget(targetStream, compress: false);
                        target.RegisterSource(filtered);
                        target.Pull();
                    }
                    // create the network for cars only.
                    LoadSettings settings = new LoadSettings();
                    settings.KeepNodeIds = true; //use to enable measure flow at nodes
                    settings.KeepWayIds = true; //can be used to calc density easier?
                    settings.OptimizeNetwork = true;
                    routerDb.LoadOsmData(filtered, settings, Vehicle.Car);
                    success = true;
                }

                // write the routerdb to disk.
                path = Path.Combine(WUInity.WORKING_FOLDER, internalRouterName);
                using (FileStream stream = new FileInfo(path).Open(FileMode.Create))
                {
                    routerDb.Serialize(stream);
                }
            }

            if(success)
            {
                WUInity.LogMessage("LOG: Router database loaded succesfully.");
            }
            else
            {
                WUInity.LogMessage("ERROR: Router database could not be loaded.");
            }

            return success;
        }

        public void UpdateNeededData()
        {
            //set parameters in directions manager
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

        public bool simRunning = false;
        public  void StartSimulation()
        {

            WUInityInput input = WUInity.INPUT;            

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
                WUInity.OUTPUT.totalEvacTime = time;
                simRunning = false;
                showResults = true;
                WUInity.LogMessage("LOG: Simulation done.");
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
                        LoadRouteCollections();
                    }                      
                    //we could not load from disk, so have to build all routes
                    if (routes == null)
                    {
                        routes = routeCreator.CalculateCellRoutes();
                        SaveLoadWUI.SaveRouteCollections();
                    }
                }
                macroHumanSim = new MacroHumanSim();
                //place people
                macroHumanSim.PopulateCells(routes, WUInity.POPULATION.GetPopulationData());
                
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
            fireMesh = new Fire.FireMesh(WUInity.INPUT.fire.lcpFile, WUInity.INPUT.fire.weather, WUInity.INPUT.fire.wind, WUInity.INPUT.fire.initialFuelMoisture, WUInity.INPUT.fire.ignitionPoints);
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
                WUInity.LogMessage("LOG: Total cars in simulation: " + macroTrafficSim.GetTotalCarsSimulated());
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
                WUInity.LogMessage("LOG: Simulation done.");
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
                        WUInity.LogMessage("LOG: Goal blocked by fire: " + eG.name);
                        BlockEvacGoal(i);
                    }
                }                
            }
        }

        public void StopSim(string stopMessage)
        {
            stopSim = true;
            WUInity.LogMessage(stopMessage);
        }

        public void BlockEvacGoal(int index)
        {
            if (!WUInity.INPUT.traffic.evacuationGoals[index].blocked)
            {
                WUInity.INPUT.traffic.evacuationGoals[index].blocked = true;
                UpdateRoutes();
            }
        }

        public Fire.FireMesh GetFireMesh()
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
            WUInityInput input = WUInity.INPUT;

            if (input.evac.paintedForcedGoals.Length < EvacCellCount.x * EvacCellCount.y)
            {
                return null;
            }
            return input.evac.paintedForcedGoals[x + y * EvacCellCount.x];
        }

        public EvacGroup GetEvacGroup(int index)
        {
            WUInityInput input = WUInity.INPUT;
            if (evacGroupIndices.Length < EvacCellCount.x * EvacCellCount.y)
            {
                MonoBehaviour.print("got here, " + evacGroupIndices.Length + "," + EvacCellCount.x * EvacCellCount.y);
                return null;
            }

            index = evacGroupIndices[index];
            return WUInity.INPUT.evac.evacGroups[index];
        }

        public EvacGroup GetEvacGroup(int x, int y)
        {
            WUInityInput input = WUInity.INPUT;
            if (evacGroupIndices.Length < EvacCellCount.x * EvacCellCount.y)
            {
                MonoBehaviour.print("got here, " + evacGroupIndices.Length + "," + EvacCellCount.x * EvacCellCount.y);
                return null;
            }

            int index = x + y * EvacCellCount.x;
            index = evacGroupIndices[index];
            return WUInity.INPUT.evac.evacGroups[index];
        }

        public RouteCollection GetCellRouteCollection(Vector2D pos)
        {   
            if(routes != null)
            {
                Mapbox.Utils.Vector2d p = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(pos.x, pos.y, WUInity.MAP.CenterMercator, WUInity.MAP.WorldRelativeScale);
                int x = (int)(p.x / WUInity.INPUT.evac.routeCellSize);
                int y = (int)(p.y / WUInity.INPUT.evac.routeCellSize);
                int index = x + y * EvacCellCount.x;
                if (index >= 0 && index < routes.Length && routes[index] != null)
                {
                    return routes[index];
                }
            }            

            return null;
        }

        public void UpdateEvacGroups(int[] indices)
        {
            evacGroupIndices = new int[EvacCellCount.x * EvacCellCount.y];
            for (int y = 0; y < EvacCellCount.y; y++)
            {
                for (int x = 0; x < EvacCellCount.x; x++)
                {
                    int index = x + y * EvacCellCount.x;
                    if (indices != null)
                    {
                        evacGroupIndices[index] = indices[index];
                    }
                    else
                    {
                        //default
                        evacGroupIndices[index] = 0;
                    }                    
                }
            }
        }

        public void UpdateWUIArea(bool[] wuiAreaIndices)
        {
            if(wuiAreaIndices == null)
            {
                wuiAreaIndices = new bool[GetFireMesh().cellCount.x * GetFireMesh().cellCount.y];
            }
            WUInity.INPUT.fire.wuiAreaIndices = wuiAreaIndices;
        }

        public void UpdateRandomIgnitionIndices(bool[] randomIgnitionIndices)
        {
            if (randomIgnitionIndices == null)
            {
                randomIgnitionIndices = new bool[GetFireMesh().cellCount.x * GetFireMesh().cellCount.y];
            }
            WUInity.INPUT.fire.randomIgnitionIndices = randomIgnitionIndices;
        }

        public void UpdateInitialIgnitionIndices(bool[] initialIgnitionIndices)
        {
            if (initialIgnitionIndices == null)
            {
                initialIgnitionIndices = new bool[GetFireMesh().cellCount.x * GetFireMesh().cellCount.y];
            }
            WUInity.INPUT.fire.initialIgnitionIndices = initialIgnitionIndices;
        }

        public void UpdateTriggerBufferIndices(bool[] triggerBufferIndices)
        {
            if (triggerBufferIndices == null)
            {
                triggerBufferIndices = new bool[GetFireMesh().cellCount.x * GetFireMesh().cellCount.y];
            }
            WUInity.INPUT.fire.triggerBufferIndices = triggerBufferIndices;
        }
    }    
}