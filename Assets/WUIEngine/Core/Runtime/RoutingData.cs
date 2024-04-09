using System.IO;
using Itinero;
using Itinero.IO.Osm;
using Itinero.Osm.Vehicles;
using OsmSharp.Streams;
using WUIEngine.Population;
using Reminiscence.Collections;
using WUIEngine.IO;

namespace WUIEngine.Runtime
{
    public class RoutingData
    {
        public float BorderSize;

        private RouterDb _routerDb;

        public RouterDb RouterDb
        {
            get
            {
                return _routerDb;
            }
        }

        private RouteCollection[] _routeCollections;
        public RouteCollection[] RouteCollections
        {
            get
            {
                return _routeCollections;
            }
        }

        // Add an array of the cell sorted vertices
        private List<uint>[] _cellSortedVertices;

        public void LoadAll()
        {
            LoadRouterDb(Path.Combine(Engine.WORKING_FOLDER, Engine.INPUT.Routing.routerDbFile), false);
            LoadRouteCollection(Path.Combine(Engine.WORKING_FOLDER, Engine.INPUT.Routing.routeCollectionFile), false);
        }

        //Load itinero database needed for pathfinding
        public bool LoadRouterDb(string path, bool updateInputFile)
        {
            bool success = false;

            if (File.Exists(path))
            {
                using (FileStream stream = new FileInfo(path).OpenRead())
                {
                    _routerDb = RouterDb.Deserialize(stream);
                    success = true;
                }
            }

            if (success)
            {
                //some road networks returns zero routes without this contract being signed (especially Swedish road networks)...
                RouterDb.AddContracted(RouterDb.GetSupportedProfile("Car"));
                Engine.LOG(Engine.LogType.Log, "Router database loaded succesfully.");
            }
            else
            {
                Engine.LOG(Engine.LogType.Error, "Router database could not be found.");
            }

            Engine.DATA_STATUS.RouterDbLoaded = success;
            if (success && updateInputFile)
            {
                Engine.INPUT.Routing.routerDbFile = Path.GetFileName(path);
                WUInityInput.SaveInput();
            }

            return success;
        }

        /// <summary>
        /// Loads wrapper format and converts to a proper route collection containing Itinero.Routes
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool LoadRouteCollection(string path, bool updateInputFile)
        {
            bool success = false;
            RouteCollection[] newRouteCollection = null;

            //try to load default
            if (path == null)
            {
                path = Path.Combine(Engine.WORKING_FOLDER, Engine.INPUT.Simulation.SimulationID + ".rc");
            }

            if (File.Exists(path))
            {
                string input = File.ReadAllText(path);
                RouteCollectionWrapper rCWS = UnityEngine.JsonUtility.FromJson<RouteCollectionWrapper>(input);

                newRouteCollection = rCWS.Convert();
                rCWS = null;
                System.GC.Collect();

                if (newRouteCollection == null)
                {
                    Engine.LOG(Engine.LogType.Error, "Tried loading route collection from " + path + " but route collection is not valid for current input.");
                }
                else
                {
                    Engine.LOG(Engine.LogType.Log, "Loaded route collection from " + path);
                }
            }
            else
            {
                Engine.LOG(Engine.LogType.Error, "Route collection file not found in " + path + ", have to be built at runtime (will take some time).");
            }

            if (newRouteCollection != null)
            {
                _routeCollections = newRouteCollection;

                //update the selected route based on the current route choice (as that might have been changed)
                for (int i = 0; i < RouteCollections.Length; i++)
                {
                    if (RouteCollections[i] != null)
                    {
                        Traffic.RouteCreator.SelectCorrectRoute(RouteCollections[i], i);
                    }
                }
                success = true;
            }
            else
            {
                success = false;
            }

            Engine.DATA_STATUS.RouteCollectionLoaded = success;
            if (success && updateInputFile)
            {
                Engine.INPUT.Routing.routeCollectionFile = path;
                WUInityInput.SaveInput();
            }

            return success;
        }

        public RouteCollection GetCellRouteCollection(Vector2d pos)
        {
            if (Engine.RUNTIME_DATA.Routing.RouteCollections != null)
            {
                Mapbox.Utils.Vector2d p = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(pos.x, pos.y, WUInity.WUInityEngine.MAP.CenterMercator, WUInity.WUInityEngine.MAP.WorldRelativeScale);
                int x = (int)(p.x / Engine.INPUT.Evacuation.RouteCellSize);
                int y = (int)(p.y / Engine.INPUT.Evacuation.RouteCellSize);
                int index = x + y * Engine.RUNTIME_DATA.Evacuation.CellCount.x;
                if (index >= 0 && index < Engine.RUNTIME_DATA.Routing.RouteCollections.Length && Engine.RUNTIME_DATA.Routing.RouteCollections[index] != null)
                {
                    return Engine.RUNTIME_DATA.Routing.RouteCollections[index];
                }
            }

            return null;
        }

        public bool FilterOSMData(string osmFile)
        {
            bool success = false;

            if (File.Exists(osmFile))
            {
                using (FileStream stream = new FileInfo(osmFile).OpenRead())
                {
                    PBFOsmStreamSource source = new PBFOsmStreamSource(stream);
                    Vector2d border = LocalGPWData.SizeToDegrees(Engine.INPUT.Simulation.LowerLeftLatLong, new Vector2d(Engine.RUNTIME_DATA.Routing.BorderSize, Engine.RUNTIME_DATA.Routing.BorderSize));
                    float left = (float)(Engine.INPUT.Simulation.LowerLeftLatLong.y - border.x);
                    float bottom = (float)(Engine.INPUT.Simulation.LowerLeftLatLong.x - border.y);
                    Vector2d size = LocalGPWData.SizeToDegrees(Engine.INPUT.Simulation.LowerLeftLatLong, Engine.INPUT.Simulation.Size);
                    float right = (float)(Engine.INPUT.Simulation.LowerLeftLatLong.y + size.x + border.x);
                    float top = (float)(Engine.INPUT.Simulation.LowerLeftLatLong.x + size.y + border.y);
                    OsmStreamSource filtered = source.FilterBox(left, top, right, bottom, true);
                    //create a new filtered file
                    string path = Path.Combine(Engine.WORKING_FOLDER, "filtered_" + Path.GetFileNameWithoutExtension(osmFile) + ".osm.pbf");
                    using (FileStream targetStream = File.OpenWrite(path))
                    {
                        //XmlOsmStreamTarget target = new XmlOsmStreamTarget(targetStream);
                        PBFOsmStreamTarget target = new PBFOsmStreamTarget(targetStream, compress: false);
                        target.RegisterSource(filtered);
                        target.Pull();

                        success = true;
                    }
                }
            }

            if (success)
            {
                Engine.LOG(Engine.LogType.Log, " Succesfully filtered OSM data to user selected boundary. Use this filtered data to build router database.");
            }
            else
            {
                Engine.LOG(Engine.LogType.Error, " Could not filter the selected OSM file.");
            }

            return success;
        }

        public void CreateRouterDatabaseFromOSM(string osmPath)
        {
            //stream in data from OSM
            using (FileStream stream = new FileInfo(osmPath).OpenRead())
            {
                PBFOsmStreamSource source = new PBFOsmStreamSource(stream);

                // create the network for cars only.
                LoadSettings settings = new LoadSettings();
                settings.KeepNodeIds = true; //use to enable measure flow at nodes
                settings.KeepWayIds = true; //can be used to calc density easier?
                settings.OptimizeNetwork = true;

                //build db from OSM betwork
                _routerDb = new RouterDb();
                _routerDb.LoadOsmData(source, settings, Vehicle.Car);
                Engine.DATA_STATUS.RouterDbLoaded = true;
                Engine.LOG(Engine.LogType.Warning, "Router database created from OSM file.");

                // write the new routerdb to disk.
                string internalRouterName = Engine.INPUT.Simulation.SimulationID + ".routerdb";
                string path = Path.Combine(Engine.WORKING_FOLDER, internalRouterName);
                using (FileStream outputStream = new FileInfo(path).Open(FileMode.Create))
                {
                    RouterDb.Serialize(outputStream);
                    Engine.LOG(Engine.LogType.Warning, "Router database saved to file " + path);
                }
            }
        }
        public void ModifyRouterDB()
        {
            // DO NOT CALL this method yet... need to work out the structure
            // ... and call after each simulation loop OR if we're importing hazards, we could receive a poke as the hazard topuches the network
            // Route analysis: thought... potentially call itinero RemoveEdges or RemoveVertex
            // https://docs.itinero.tech/itinero/Itinero.Data.Network.RoutingNetwork.html#Itinero_Data_Network_RoutingNetwork_RemoveEdge_System_UInt32_
            // Find the nth edge or vertex to modify
            uint n=0;
            _routerDb.Network.RemoveEdge(n); //..(or RemoveVertex)

        }

        private void SortVertices()
        {
            // Route analysis:

            uint vCount = _routerDb.Network.VertexCount;
            for (uint i = 0; i < vCount; ++i)
            {
                var vertex = _routerDb.Network.GetVertex(i);
                var lati = vertex.Latitude;
                var longi = vertex.Longitude;

                //now find correct cell and add vertex index to list
                //private List<uint>[] _cellSortedVertices;

                _cellSortedVertices = new List<uint>[Engine.SIM.FireModule.GetCellCountX() * Engine.SIM.FireModule.GetCellCountY()];

                // Sort the vertices of the cells, based on location... to be completed
                // ... todo
            }
        }

        public void checkIfVerticesAreBlocked()
        {
            // Route analysis:
            //after each fire update, check if any new burning cell has vertex

            bool modifiedNetwork = false;
            //if so, do this:
            for (int i = 0; i < _cellSortedVertices.Length; ++i)
            {
                // If this cell just received fire, then execute the next loop
                {
                    for (int j = 0; j < _cellSortedVertices[i].Count; ++j)
                    {
                        //vertex must stay otherwise index will be off, but seems OK to only remove edges
                        _routerDb.Network.RemoveEdges(_cellSortedVertices[i][j]);
                    }
                    modifiedNetwork = true;
                }

            }
            if(modifiedNetwork )
            {
                //then do clumbsy update as there is no way of forcing update of contracted network it seems
                var carProfile = RouterDb.GetSupportedProfile("Car");
                _routerDb.RemoveContracted(carProfile);
                _routerDb.AddContracted(carProfile);

                //if any changes, update everyone already inside the network by re-calculating their routes
                // code TODO
            }
        }

        public void BuildAndSaveRouteCollection()
        {
            // Route analysis: calculate all routes
            _routeCollections = Engine.SIM.RouteCreator.CalculateCellRoutes();
            SaveRouteCollections();
            Engine.DATA_STATUS.RouteCollectionLoaded = true;
        }

        /// <summary>
        /// Saves a collection of routes by converting to a format that is serializable (Itinero objects does not serialize using Unitys JSONUtility)
        /// </summary>
        /// <param name="filename"></param>
        public static void SaveRouteCollections()
        {
            string path = Path.Combine(Engine.WORKING_FOLDER, Engine.INPUT.Simulation.SimulationID + ".rc");

            RouteCollectionWrapper save = new RouteCollectionWrapper(Engine.RUNTIME_DATA.Routing.RouteCollections);
            string json = UnityEngine.JsonUtility.ToJson(save, false);
            File.WriteAllText(path, json);

            //slow
            /*BinaryFormatter bf = new BinaryFormatter();
            System.IO.FileStream file = System.IO.File.Create(path);
            bf.Serialize(file, save);
            file.Close();*/

            Engine.LOG(Engine.LogType.Log, " Saved route collection to " + path);
        }
    }
}