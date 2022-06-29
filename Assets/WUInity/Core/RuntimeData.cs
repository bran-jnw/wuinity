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
    /// <summary>
    /// Contains all data that gets created during runtime before simulation starts.
    /// </summary>
    public class RuntimeData
    {
        public struct GeneralData
        {
            public int NumberOfRuns;
        }

        public GeneralData General;

        public struct OSMData
        {
            public float BorderSize;
        }

        public OSMData OSM;

        public int[] evacGroupIndices;
        public bool[] wuiAreaIndices;
        public bool[] randomIgnitionIndices;
        public bool[] initialIgnitionIndices;
        public bool[] triggerBufferIndices;
        LCPData lcpData;
        public bool MultipleSimulations;
        public int convergenceMinSequence = 10;
        public float convergenceMaxDifference = 0.02f;        

        FuelModelInput _fuelModelsData;
        public FuelModelInput GetFuelModelsData()
        {
            return _fuelModelsData;
        }

        Traffic.OpticalDensityRamp _opticalDensity;
        public Traffic.OpticalDensityRamp GetOpticalDensity()
        {
            return _opticalDensity;
        }

        private RouterDb _routerDb;
        public RouterDb GetRouterDb()
        {
            return _routerDb;
        }

        private  RouteCollection[] _routes;
        public RouteCollection[] Routes
        {
            get
            {
                return _routes;
            }

            set
            {
                _routes = value;
            }
        }
        
        private Vector2Int _cellCount;
        public Vector2Int EvacCellCount
        {
            get
            {
                WUInityInput input = WUInity.INPUT;
                _cellCount.x = Mathf.CeilToInt((float)input.size.x / input.evac.routeCellSize);
                _cellCount.y = Mathf.CeilToInt((float)input.size.y / input.evac.routeCellSize);
                return _cellCount;
            }
        }

        private ResponseCurve[] _responseCurves;
        public ResponseCurve[] ResponseCurves
        {
            get
            {
                if(_responseCurves == null)
                {
                    _responseCurves = ResponseCurve.LoadResponseCurves();
                }
                return _responseCurves;
            }

            set
            {
                if (value != null)
                {
                    _responseCurves = value;
                }
            }
        }

        private InitialFuelMoistureList _initialFuelMoistureData;
        public InitialFuelMoistureList InitialFuelMoistureData
        {
            get
            {
                if (_initialFuelMoistureData == null)
                {
                    _initialFuelMoistureData = InitialFuelMoistureList.LoadInitialFuelMoistureDataFile();
                }
                return _initialFuelMoistureData;
            }

            set
            {
                if(value != null)
                {
                    _initialFuelMoistureData = value;
                }
            }
        }

        private WeatherInput _weatherInput;
        public WeatherInput WeatherInput
        {
            get
            {
                if (_weatherInput == null)
                {
                    _weatherInput = WeatherInput.LoadWeatherInputFile();
                }
                return _weatherInput;
            }

            set
            {
                if (value != null)
                {
                    _weatherInput = value;
                }
            }
        }

        private WindInput _windInput;
        public WindInput WindInput
        {
            get
            {
                if (_windInput == null)
                {
                    _windInput = WindInput.LoadWindInputFile();
                }
                return _windInput;
            }

            set
            {
                if (value != null)
                {
                    _windInput = value;
                }
            }
        }

        private IgnitionPoint[] _ignitionPoints;
        public IgnitionPoint[] IgnitionPoints
        {
            get
            {
                if (_ignitionPoints == null)
                {
                    _ignitionPoints = IgnitionPoint.LoadIgnitionPointsFile();
                }
                return _ignitionPoints;
            }

            set
            {
                if (value != null)
                {
                    _ignitionPoints = value;
                }
            }
        }

        private EvacuationGoal[] _evacuationGoals;
        public EvacuationGoal[] EvacuationGoals
        {
            get
            {
                if (_evacuationGoals == null)
                {
                    _evacuationGoals = EvacuationGoal.LoadEvacuationGoalFiles();
                }
                return _evacuationGoals;
            }

            set
            {
                if (value != null)
                {
                    _evacuationGoals = value;
                }
            }
        }

        private EvacGroup[] _evacuationGroups;
        public EvacGroup[] EvacuationGroups
        {
            get
            {
                if (_evacuationGroups == null)
                {
                    _evacuationGroups = EvacGroup.LoadEvacGroupFiles();
                }
                return _evacuationGroups;
            }

            set
            {
                if (value != null)
                {
                    _evacuationGroups = value;
                }
            }
        }

        private Traffic.RoadTypeData _roadTypeData;
        public Traffic.RoadTypeData RoadTypeData
        {
            get
            {
                if (_roadTypeData == null)
                {
                    _roadTypeData = Traffic.RoadTypeData.LoadRoadTypeData();
                }
                return _roadTypeData;
            }

            set
            {
                if (value != null)
                {
                    _roadTypeData = value;
                }
            }
        }

        public RuntimeData()
        {           
            
        }

        public int GetEvacGoalIndexFromName(string name)
        {
            int index = -1;
            for (int i = 0; i < EvacuationGoals.Length; i++)
            {
                if(name == EvacuationGoals[i].name)
                {
                    index = i;
                    break;
                }
            }

            if(index < 0)
            {
                WUInity.LOG("ERROR: User has specified an evacuation goal named " + name + " but no such evacuation goal has been defined.");
            }

            return index;
        }

        public int GetResponseCurveIndexFromName(string name)
        {
            int index = -1;
            for (int i = 0; i < ResponseCurves.Length; i++)
            {
                if (name == ResponseCurves[i].name)
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                WUInity.LOG("ERROR: User has specified a response curve named " + name + " but no such response curve has been defined.");
            }

            return index;
        }

        public void BuildAndSaveRouteCollection()
        {
            WUInity.RUNTIME_DATA._routes = WUInity.SIM.RouteCreator.CalculateCellRoutes();
            SaveRouteCollections();
            WUInity.DATA_STATUS.RouteCollectionLoaded = true;
        }

        /// <summary>
        /// Saves a collection of routes by converting to a format that is serializable (Itinero objects does not serialize using Unitys JSONUtility)
        /// </summary>
        /// <param name="filename"></param>
        public static void SaveRouteCollections()
        {
            string path = Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.simDataName + ".rc");

            RouteCollectionWrapper save = new RouteCollectionWrapper(WUInity.RUNTIME_DATA.Routes);
            string json = JsonUtility.ToJson(save, false);
            File.WriteAllText(path, json);

            //slow
            /*BinaryFormatter bf = new BinaryFormatter();
            System.IO.FileStream file = System.IO.File.Create(path);
            bf.Serialize(file, save);
            file.Close();*/

            WUInity.LOG("LOG: Saved route collection to " + path);
        }

        public bool LoadLCPFile()
        {
            lcpData = new LCPData(Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.fire.lcpFile));
            WUInity.DATA_STATUS.LcpLoaded = !lcpData.CantAllocLCP;

            if(WUInity.DATA_STATUS.LcpLoaded)
            {
                int[] fuelNrs = lcpData.GetExisitingFuelModelNumbers();
                string message = "LOG: Present fuel model numbers are ";
                for (int i = 0; i < fuelNrs.Length; i++)
                {
                    message += fuelNrs[i].ToString();
                    if (i < fuelNrs.Length - 1)
                    {
                        message += ", ";
                    }
                    else
                    {
                        message += ".";
                    }
                }
                
                WUInity.LOG(message);
            }

            return !lcpData.CantAllocLCP;
        }

        public bool LoadFuelModelsFile()
        {
            _fuelModelsData = new FuelModelInput();
            return WUInity.DATA_STATUS.FuelModelsLoaded = _fuelModelsData.LoadFuelModelInputFile(Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.fire.fuelModelsFile));
        }

        public bool LoadOpticalDensityFile()
        {
            _opticalDensity = new Traffic.OpticalDensityRamp();
            return WUInity.DATA_STATUS.OpticalDensityLoaded = _opticalDensity.LoadOpticalDensityRampFile(WUInity.INPUT.traffic.opticalDensityFile);
        }

        public bool FilterOSMData(string osmFile)
        {
            bool success = false;

            if (File.Exists(osmFile))
            {       
                using (FileStream stream = new FileInfo(osmFile).OpenRead())
                {
                    PBFOsmStreamSource source = new PBFOsmStreamSource(stream);
                    Vector2D border = LocalGPWData.SizeToDegrees(WUInity.INPUT.lowerLeftLatLong, new Vector2D(WUInity.RUNTIME_DATA.OSM.BorderSize, WUInity.RUNTIME_DATA.OSM.BorderSize));
                    float left = (float)(WUInity.INPUT.lowerLeftLatLong.y - border.x);
                    float bottom = (float)(WUInity.INPUT.lowerLeftLatLong.x - border.y);
                    Vector2D size = LocalGPWData.SizeToDegrees(WUInity.INPUT.lowerLeftLatLong, WUInity.INPUT.size);
                    float right = (float)(WUInity.INPUT.lowerLeftLatLong.y + size.x + border.x);
                    float top = (float)(WUInity.INPUT.lowerLeftLatLong.x + size.y + border.y);
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

            if(success)
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
                _routerDb.LoadOsmData(source, settings, Vehicle.Car);

                WUInity.LOG("LOG: Router database created from OSM file.");

                // write the routerdb to disk.
                string internalRouterName = WUInity.INPUT.simDataName + ".routerdb";
                string path = Path.Combine(WUInity.WORKING_FOLDER, internalRouterName);
                using (FileStream outputStream = new FileInfo(path).Open(FileMode.Create))
                {
                    _routerDb.Serialize(outputStream);
                    WUInity.LOG("LOG: Router database saved to file " + path);
                }
            }            
        }

        //Load itinero database needed for pathfinding
        public bool LoadRouterDb(string path)
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
                _routerDb.AddContracted(_routerDb.GetSupportedProfile("Car"));
                WUInity.LOG("LOG: Router database loaded succesfully.");                
            }
            else
            {
                WUInity.LOG("ERROR: Router database could not be found.");
            }

            WUInity.DATA_STATUS.RouterDbLoaded = success;

            return success;
        }

        /// <summary>
        /// Loads wrapper format and converts to a proper route collection containing Itinero.Routes
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool LoadRouteCollection(string path)
        {
            bool success = false;
            RouteCollection[] newRouteCollection = null;

            //try to load default
            if(path == null)
            {
                path = Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.simDataName + ".rc");
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
                _routes = newRouteCollection;

                //update the selected route based on the current route choice (as that might have been changed)
                for (int i = 0; i < _routes.Length; i++)
                {
                    if (_routes[i] != null)
                    {
                        RouteCreator.SelectCorrectRoute(_routes[i], i);
                    }
                }
                success = true;
            }
            else
            {
                success = false;
            }

            WUInity.DATA_STATUS.RouteCollectionLoaded = success;

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
                wuiAreaIndices = new bool[WUInity.SIM.FireMesh().cellCount.x * WUInity.SIM.FireMesh().cellCount.y];
            }
            WUInity.RUNTIME_DATA.wuiAreaIndices = wuiAreaIndices;
        }

        public void UpdateRandomIgnitionIndices(bool[] randomIgnitionIndices)
        {
            if (randomIgnitionIndices == null)
            {
                randomIgnitionIndices = new bool[WUInity.SIM.FireMesh().cellCount.x * WUInity.SIM.FireMesh().cellCount.y];
            }
            WUInity.RUNTIME_DATA.randomIgnitionIndices = randomIgnitionIndices;
        }

        public void UpdateInitialIgnitionIndices(bool[] initialIgnitionIndices)
        {
            if (initialIgnitionIndices == null)
            {
                initialIgnitionIndices = new bool[WUInity.SIM.FireMesh().cellCount.x * WUInity.SIM.FireMesh().cellCount.y];
            }
            WUInity.RUNTIME_DATA.initialIgnitionIndices = initialIgnitionIndices;
        }

        public void UpdateTriggerBufferIndices(bool[] triggerBufferIndices)
        {
            if (triggerBufferIndices == null)
            {
                triggerBufferIndices = new bool[WUInity.SIM.FireMesh().cellCount.x * WUInity.SIM.FireMesh().cellCount.y];
            }
            WUInity.RUNTIME_DATA.triggerBufferIndices = triggerBufferIndices;
        }

        public RouteCollection GetCellRouteCollection(Vector2D pos)
        {   
            if(WUInity.RUNTIME_DATA._routes != null)
            {
                Mapbox.Utils.Vector2d p = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(pos.x, pos.y, WUInity.MAP.CenterMercator, WUInity.MAP.WorldRelativeScale);
                int x = (int)(p.x / WUInity.INPUT.evac.routeCellSize);
                int y = (int)(p.y / WUInity.INPUT.evac.routeCellSize);
                int index = x + y * WUInity.RUNTIME_DATA.EvacCellCount.x;
                if (index >= 0 && index < WUInity.RUNTIME_DATA._routes.Length && WUInity.RUNTIME_DATA._routes[index] != null)
                {
                    return WUInity.RUNTIME_DATA._routes[index];
                }
            }            

            return null;
        }

        public EvacuationGoal GetForcedGoal(int x, int y)
        {
            WUInityInput input = WUInity.INPUT;

            if (input.evac.paintedForcedGoals.Length < WUInity.RUNTIME_DATA.EvacCellCount.x * WUInity.RUNTIME_DATA.EvacCellCount.y)
            {
                return null;
            }
            return input.evac.paintedForcedGoals[x + y * WUInity.RUNTIME_DATA.EvacCellCount.x];
        }

        public EvacGroup GetEvacGroup(int index)
        {
            WUInityInput input = WUInity.INPUT;
            if (WUInity.RUNTIME_DATA.evacGroupIndices.Length < WUInity.RUNTIME_DATA.EvacCellCount.x * WUInity.RUNTIME_DATA.EvacCellCount.y)
            {
                return null;
            }

            index = WUInity.RUNTIME_DATA.evacGroupIndices[index];
            return WUInity.RUNTIME_DATA.EvacuationGroups[index];
        }

        public EvacGroup GetEvacGroup(int x, int y)
        {
            WUInityInput input = WUInity.INPUT;
            if (WUInity.RUNTIME_DATA.evacGroupIndices.Length < WUInity.RUNTIME_DATA.EvacCellCount.x * WUInity.RUNTIME_DATA.EvacCellCount.y)
            {
                return null;
            }

            int index = x + y * WUInity.RUNTIME_DATA.EvacCellCount.x;
            index = WUInity.RUNTIME_DATA.evacGroupIndices[index];
            return WUInity.RUNTIME_DATA.EvacuationGroups[index];
        }
    }
}

