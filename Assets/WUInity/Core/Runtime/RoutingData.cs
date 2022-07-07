using UnityEngine;
using System.IO;
using Itinero;
using Itinero.IO.Osm;
using Itinero.Osm.Vehicles;
using OsmSharp.Streams;
using WUInity.Population;
using static WUInity.WUInity;

namespace WUInity.Runtime
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

        public void LoadAll()
        {
            LoadRouterDb(Path.Combine(WORKING_FOLDER, INPUT.Routing.routerDbFile), false);
            LoadRouteCollection(Path.Combine(WORKING_FOLDER, INPUT.Routing.routeCollectionFile), false);
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
                WUInity.LOG("LOG: Router database loaded succesfully.");
            }
            else
            {
                WUInity.LOG("ERROR: Router database could not be found.");
            }

            WUInity.DATA_STATUS.RouterDbLoaded = success;
            if (success && updateInputFile)
            {
                INPUT.Routing.routerDbFile = path;
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
                path = Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.Simulation.SimulationID + ".rc");
            }

            if (File.Exists(path))
            {
                string input = File.ReadAllText(path);
                RouteCollectionWrapper rCWS = JsonUtility.FromJson<RouteCollectionWrapper>(input);

                newRouteCollection = rCWS.Convert();
                rCWS = null;
                System.GC.Collect();

                if (newRouteCollection == null)
                {
                    WUInity.LOG("ERROR: Tried loading route collection from " + path + " but route collection is not valid for current input.");
                }
                else
                {
                    WUInity.LOG("LOG: Loaded route collection from " + path);
                }
            }
            else
            {
                WUInity.LOG("WARNING: Route collection file not found in " + path + ", have to be built at runtime (will take some time).");
            }

            if (newRouteCollection != null)
            {
                _routeCollections = newRouteCollection;

                //update the selected route based on the current route choice (as that might have been changed)
                for (int i = 0; i < RouteCollections.Length; i++)
                {
                    if (RouteCollections[i] != null)
                    {
                        RouteCreator.SelectCorrectRoute(RouteCollections[i], i);
                    }
                }
                success = true;
            }
            else
            {
                success = false;
            }

            DATA_STATUS.RouteCollectionLoaded = success;
            if (success && updateInputFile)
            {
                INPUT.Routing.routeCollectionFile = path;
                WUInityInput.SaveInput();
            }

            return success;
        }

        public RouteCollection GetCellRouteCollection(Vector2D pos)
        {
            if (WUInity.RUNTIME_DATA.Routing.RouteCollections != null)
            {
                Mapbox.Utils.Vector2d p = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(pos.x, pos.y, WUInity.MAP.CenterMercator, WUInity.MAP.WorldRelativeScale);
                int x = (int)(p.x / WUInity.INPUT.Evacuation.RouteCellSize);
                int y = (int)(p.y / WUInity.INPUT.Evacuation.RouteCellSize);
                int index = x + y * WUInity.RUNTIME_DATA.Evacuation.CellCount.x;
                if (index >= 0 && index < WUInity.RUNTIME_DATA.Routing.RouteCollections.Length && WUInity.RUNTIME_DATA.Routing.RouteCollections[index] != null)
                {
                    return WUInity.RUNTIME_DATA.Routing.RouteCollections[index];
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
                    Vector2D border = LocalGPWData.SizeToDegrees(WUInity.INPUT.Simulation.LowerLeftLatLong, new Vector2D(WUInity.RUNTIME_DATA.Routing.BorderSize, WUInity.RUNTIME_DATA.Routing.BorderSize));
                    float left = (float)(WUInity.INPUT.Simulation.LowerLeftLatLong.y - border.x);
                    float bottom = (float)(WUInity.INPUT.Simulation.LowerLeftLatLong.x - border.y);
                    Vector2D size = LocalGPWData.SizeToDegrees(WUInity.INPUT.Simulation.LowerLeftLatLong, WUInity.INPUT.Simulation.Size);
                    float right = (float)(WUInity.INPUT.Simulation.LowerLeftLatLong.y + size.x + border.x);
                    float top = (float)(WUInity.INPUT.Simulation.LowerLeftLatLong.x + size.y + border.y);
                    OsmStreamSource filtered = source.FilterBox(left, top, right, bottom, true);
                    //create a new filtered file
                    string path = Path.Combine(WUInity.WORKING_FOLDER, "filtered_" + Path.GetFileNameWithoutExtension(osmFile) + ".osm.pbf");
                    using (FileStream targetStream = File.OpenWrite(path))
                    {
                        PBFOsmStreamTarget target = new PBFOsmStreamTarget(targetStream, compress: false);
                        target.RegisterSource(filtered);
                        target.Pull();

                        success = true;
                    }
                }
            }

            if (success)
            {
                WUInity.LOG("LOG: Succesfully filtered OSM data to user selected boundary. Use this filtered data to build router database.");
            }
            else
            {
                WUInity.LOG("ERROR: Could not filter the selected OSM file.");
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
                RouterDb.LoadOsmData(source, settings, Vehicle.Car);

                WUInity.LOG("LOG: Router database created from OSM file.");

                // write the routerdb to disk.
                string internalRouterName = WUInity.INPUT.Simulation.SimulationID + ".routerdb";
                string path = Path.Combine(WUInity.WORKING_FOLDER, internalRouterName);
                using (FileStream outputStream = new FileInfo(path).Open(FileMode.Create))
                {
                    RouterDb.Serialize(outputStream);
                    WUInity.LOG("LOG: Router database saved to file " + path);
                }
            }
        }

        public void BuildAndSaveRouteCollection()
        {
            _routeCollections = WUInity.SIM.RouteCreator.CalculateCellRoutes();
            SaveRouteCollections();
            DATA_STATUS.RouteCollectionLoaded = true;
        }

        /// <summary>
        /// Saves a collection of routes by converting to a format that is serializable (Itinero objects does not serialize using Unitys JSONUtility)
        /// </summary>
        /// <param name="filename"></param>
        public static void SaveRouteCollections()
        {
            string path = Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.Simulation.SimulationID + ".rc");

            RouteCollectionWrapper save = new RouteCollectionWrapper(WUInity.RUNTIME_DATA.Routing.RouteCollections);
            string json = JsonUtility.ToJson(save, false);
            File.WriteAllText(path, json);

            //slow
            /*BinaryFormatter bf = new BinaryFormatter();
            System.IO.FileStream file = System.IO.File.Create(path);
            bf.Serialize(file, save);
            file.Close();*/

            WUInity.LOG("LOG: Saved route collection to " + path);
        }
    }
}