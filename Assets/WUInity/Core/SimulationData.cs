using UnityEngine;
using System.IO;
using Itinero;
using Itinero.IO.Osm;
using Itinero.Osm.Vehicles;
using OsmSharp.Streams;
using WUInity.Population;
using WUInity.Fire;

namespace WUInity
{
    public class SimulationData
    {
        public int[] evacGroupIndices;
        public bool[] wuiAreaIndices;
        public bool[] randomIgnitionIndices;
        public bool[] initialIgnitionIndices;
        public bool[] triggerBufferIndices;
        LCPData lcpData;
        
        private RouterDb routerDb;
        public RouterDb GetRouterDb()
        {
            return routerDb;
        }

        public  RouteCollection[] routes;
        public RouteCollection[] GetRouteCollection()
        {
            return routes;
        }
        
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

        public SimulationData()
        {           
            
        }

        public bool LoadLCPFile()
        {
            lcpData = new LCPData(WUInity.INPUT.fire.lcpFile);
            return !lcpData.CantAllocLCP;
        }

        //create or lead itinero database needed for pathfinding
        public bool LoadRouterDatabase()
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
            else if(WUInity.DATA_STATUS.osmFileValid)
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
                    OsmStreamSource filtered = source.FilterBox(left, top, right, bottom, true);
                    //create a new filtered file
                    path = Path.Combine(WUInity.WORKING_FOLDER, "filtered_" + WUInity.INPUT.simName + ".osm.pbf");
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

            if (success)
            {
                WUInity.WUI_LOG("LOG: Router database loaded succesfully.");
            }
            else
            {
                WUInity.WUI_LOG("ERROR: Router database could not be loaded nor built.");
            }

            return success;
        }

        public bool LoadRouteCollections()
        {
            bool success = false;
            RouteCollection[] newRoutes = SaveLoadWUI.LoadRouteCollections();
            if (newRoutes != null)
            {
                routes = newRoutes;

                //update the selected route based on the current route choice (as that might have been changed)
                for (int i = 0; i < routes.Length; i++)
                {
                    if (routes[i] != null)
                    {
                        RouteCreator.SelectCorrectRoute(routes[i], true, i);
                    }
                }
                success = true;
            }
            else
            {
                success = false;
            }

            return success;
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
            if (wuiAreaIndices == null)
            {
                wuiAreaIndices = new bool[WUInity.SIM.GetFireMesh().cellCount.x * WUInity.SIM.GetFireMesh().cellCount.y];
            }
            WUInity.SIM_DATA.wuiAreaIndices = wuiAreaIndices;
        }

        public void UpdateRandomIgnitionIndices(bool[] randomIgnitionIndices)
        {
            if (randomIgnitionIndices == null)
            {
                randomIgnitionIndices = new bool[WUInity.SIM.GetFireMesh().cellCount.x * WUInity.SIM.GetFireMesh().cellCount.y];
            }
            WUInity.SIM_DATA.randomIgnitionIndices = randomIgnitionIndices;
        }

        public void UpdateInitialIgnitionIndices(bool[] initialIgnitionIndices)
        {
            if (initialIgnitionIndices == null)
            {
                initialIgnitionIndices = new bool[WUInity.SIM.GetFireMesh().cellCount.x * WUInity.SIM.GetFireMesh().cellCount.y];
            }
            WUInity.SIM_DATA.initialIgnitionIndices = initialIgnitionIndices;
        }

        public void UpdateTriggerBufferIndices(bool[] triggerBufferIndices)
        {
            if (triggerBufferIndices == null)
            {
                triggerBufferIndices = new bool[WUInity.SIM.GetFireMesh().cellCount.x * WUInity.SIM.GetFireMesh().cellCount.y];
            }
            WUInity.SIM_DATA.triggerBufferIndices = triggerBufferIndices;
        }

        public RouteCollection GetCellRouteCollection(Vector2D pos)
        {   
            if(WUInity.SIM_DATA.routes != null)
            {
                Mapbox.Utils.Vector2d p = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(pos.x, pos.y, WUInity.MAP.CenterMercator, WUInity.MAP.WorldRelativeScale);
                int x = (int)(p.x / WUInity.INPUT.evac.routeCellSize);
                int y = (int)(p.y / WUInity.INPUT.evac.routeCellSize);
                int index = x + y * WUInity.SIM_DATA.EvacCellCount.x;
                if (index >= 0 && index < WUInity.SIM_DATA.routes.Length && WUInity.SIM_DATA.routes[index] != null)
                {
                    return WUInity.SIM_DATA.routes[index];
                }
            }            

            return null;
        }

        public EvacuationGoal GetForcedGoal(int x, int y)
        {
            WUInityInput input = WUInity.INPUT;

            if (input.evac.paintedForcedGoals.Length < WUInity.SIM_DATA.EvacCellCount.x * WUInity.SIM_DATA.EvacCellCount.y)
            {
                return null;
            }
            return input.evac.paintedForcedGoals[x + y * WUInity.SIM_DATA.EvacCellCount.x];
        }

        public EvacGroup GetEvacGroup(int index)
        {
            WUInityInput input = WUInity.INPUT;
            if (WUInity.SIM_DATA.evacGroupIndices.Length < WUInity.SIM_DATA.EvacCellCount.x * WUInity.SIM_DATA.EvacCellCount.y)
            {
                return null;
            }

            index = WUInity.SIM_DATA.evacGroupIndices[index];
            return WUInity.INPUT.evac.evacGroups[index];
        }

        public EvacGroup GetEvacGroup(int x, int y)
        {
            WUInityInput input = WUInity.INPUT;
            if (WUInity.SIM_DATA.evacGroupIndices.Length < WUInity.SIM_DATA.EvacCellCount.x * WUInity.SIM_DATA.EvacCellCount.y)
            {
                return null;
            }

            int index = x + y * WUInity.SIM_DATA.EvacCellCount.x;
            index = WUInity.SIM_DATA.evacGroupIndices[index];
            return WUInity.INPUT.evac.evacGroups[index];
        }
    }
}

