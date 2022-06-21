using System.Collections;               
using System.Collections.Generic;       
using UnityEngine;                     
using OsmSharp.Streams;                 
using WUInity.Evac;                     
using WUInity.Traffic;                 
using WUInity.Population;                     
using System;                           
using System.IO;                        
using Mapbox.Utils;                    
using Mapbox.Unity.Utilities;          


namespace WUInity
{    
    [RequireComponent(typeof(WUInityGUI))]
    [RequireComponent(typeof(Visualization.EvacuationRenderer))]
    [RequireComponent(typeof(Visualization.FireRenderer))]
    public class WUInity : MonoBehaviour                     
    {                           
        public static WUInity INSTANCE
        {
            get
            {
                if(internal_instance == null)
                {
                    internal_instance = FindObjectOfType<WUInity>();
                    if(internal_instance == null)
                    {
                        GameObject g = new GameObject();
                        internal_instance = g.AddComponent<WUInity>();
                    }
                }
                return internal_instance;
            }
        }

        public static DataStatus DATA_STATUS
        {
            get
            {
                if (INSTANCE.internal_dataStatus == null)
                {
                    INSTANCE.internal_dataStatus = new DataStatus();
                }
                return INSTANCE.internal_dataStatus;
            }
        }

        public static Visualization.EvacuationRenderer EVAC_VISUALS
        {
            get
            {
                if (INSTANCE.internal_evacuationRenderer == null)
                {
                    INSTANCE.internal_evacuationRenderer = INSTANCE.GetComponent<Visualization.EvacuationRenderer>();
                    if(INSTANCE.internal_evacuationRenderer == null)
                    {
                        INSTANCE.internal_evacuationRenderer = INSTANCE.gameObject.AddComponent<Visualization.EvacuationRenderer>();
                    }
                }
                return INSTANCE.internal_evacuationRenderer;
            }
        }

        public static Visualization.FireRenderer FIRE_VISUALS
        {
            get
            {
                if (INSTANCE.internal_fireRenderer == null)
                {
                    INSTANCE.internal_fireRenderer = INSTANCE.GetComponent<Visualization.FireRenderer>();
                    if (INSTANCE.internal_fireRenderer == null)
                    {
                        INSTANCE.internal_fireRenderer = INSTANCE.gameObject.AddComponent<Visualization.FireRenderer>();
                    }
                }
                return INSTANCE.internal_fireRenderer;
            }
        }

        public static Simulation SIM
        {
            get
            {
                if (INSTANCE.internal_sim == null)
                {
                    INSTANCE.internal_sim= new Simulation();
                }
                return INSTANCE.internal_sim;
            }
        }

        RuntimeData _runtimeData;
        public static RuntimeData RUNTIME_DATA
        {
            get
            {
                if (INSTANCE._runtimeData == null)
                {
                    INSTANCE._runtimeData = new RuntimeData();
                }
                return INSTANCE._runtimeData;
            }
        }

        public static WUInityGUI GUI
        {
            get
            {
                if(INSTANCE.internal_wuiGUI == null)
                {
                    INSTANCE.internal_wuiGUI = INSTANCE.GetComponent<WUInityGUI>();
                    if(INSTANCE.internal_wuiGUI == null)
                    {
                        INSTANCE.gameObject.AddComponent<WUInityGUI>();
                    }
                }
                return INSTANCE.internal_wuiGUI;
            }
        }

        public static WUInityInput INPUT
        {
            get
            {
                if (INSTANCE.internal_input == null)
                {
                    INSTANCE.internal_input = new WUInityInput();
                }
                return INSTANCE.internal_input;
            }
        }

        public static WUInityOutput OUTPUT
        {
            get
            {
                if(INSTANCE.internal_output == null)
                {
                    INSTANCE.internal_output = new WUInityOutput();
                }
                return INSTANCE.internal_output;
            }
        }

        public static Farsite.FarsiteViewer FARSITE_VIEWER
        {
            get
            {
                if(INSTANCE.internal_farsiteViewer == null)
                {
                    INSTANCE.internal_farsiteViewer = new Farsite.FarsiteViewer();
                }
                return INSTANCE.internal_farsiteViewer;
            }
        }

        public static PopulationManager POPULATION
        {
            get
            {
                if (INSTANCE.internal_population_manager == null)
                {
                    INSTANCE.internal_population_manager = new PopulationManager();
                }
                return INSTANCE.internal_population_manager;
            }
        }

        public static Mapbox.Unity.Map.AbstractMap MAP
        {
            get
            {
                if(INSTANCE._mapboxMap == null)
                {
                    INSTANCE._mapboxMap = FindObjectOfType<Mapbox.Unity.Map.AbstractMap>();
                    if(INSTANCE._mapboxMap == null)
                    {
                        GameObject g = new GameObject();
                        g.name = "Mapbox Map";
                        g.transform.parent = INSTANCE.transform;
                        INSTANCE._mapboxMap = g.AddComponent<Mapbox.Unity.Map.AbstractMap>();
                    }
                }
                return INSTANCE._mapboxMap;
            }
        }

        public static Painter PAINTER
        {
            get
            {
                if(INSTANCE.internal_painter == null)
                {
                    GameObject g = new GameObject();
                    g.transform.parent = INSTANCE.transform;
                    g.name = "WUI Painter";
                    INSTANCE.internal_painter = g.AddComponent<Painter>();
                    g.SetActive(false);
                }
                return INSTANCE.internal_painter;
            }
        }

        public static string DATA_FOLDER
        {
            get
            {
                return Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "external_data");
            }
        }

        public static string WORKING_FILE
        {
            get
            {
                return INSTANCE.internal_workingFilePath;
            }
            set
            {
                INSTANCE.internal_workingFilePath = value;
            }
        }

        public static string WORKING_FOLDER
        {
            get
            {
                return Path.GetDirectoryName(WORKING_FILE);            
            }
        }

        public static string OUTPUT_FOLDER
        {
            get
            {
                DirectoryInfo path = Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(WORKING_FILE).ToString(), "output"));
                return path.ToString();
            }
        }

        public class DataStatus
        {
            public bool HaveInput;

            public bool MapLoaded;

            public bool PopulationLoaded;
            public bool PopulationCorrectedForRoutes;
            public bool GlobalGPWAvailable;
            public bool LocalGPWLoaded;

            public bool OpticalDensityLoaded;

            public bool RouteCollectionLoaded;
            public bool RouterDbLoaded;

            public bool OsmFileValid;

            public bool LcpLoaded, FuelModelsLoaded;

            public bool ResponseCurvesValid;

            public bool CanRunSimulation()
            {
                bool canRun = true;
                if(!MapLoaded)
                {
                    canRun = false;
                    LOG("ERROR: Map is not loaded.");
                }

                if(!PopulationLoaded && (!LocalGPWLoaded || !GlobalGPWAvailable))
                {
                    canRun = false;  
                    LOG("ERROR: Population is not loaded and no local nor global GPW file is found to build it from.");                  
                }

                if(!RouterDbLoaded && !OsmFileValid)
                {
                    canRun = false;
                    LOG("ERROR: No router database loaded and no valid OSM file was found to build it from.");
                }

                if(INPUT.runFireSim)
                {
                    if (!LcpLoaded)
                    {
                        canRun = false;
                        LOG("ERROR: No LCP file loaded but fire spread is activated.");
                    }
                }

                if(INPUT.runEvacSim)
                {
                    if(RUNTIME_DATA.ResponseCurves == null)
                    {
                        canRun = false;
                        LOG("ERROR: No valid response curves have been loaded.");                        
                    }
                    
                }

                return canRun;
            }


            public void Reset()
            {
                //haveInput = false; //can never lose input after getting it once
                MapLoaded = false;

                PopulationLoaded = false;
                PopulationCorrectedForRoutes = false;
                GlobalGPWAvailable = false;
                LocalGPWLoaded = false;

                RouteCollectionLoaded = false;
                RouterDbLoaded = false;
                OsmFileValid = false;

                LcpLoaded = false;
                FuelModelsLoaded = false;
            }
        }

        [Header("Options")]
        public bool DeveloperMode = false;
        public bool AutoLoadExample = true;

        [Header("Prefabs")]
        [SerializeField] private GameObject _markerPrefab;

        [Header("References")]              
        [SerializeField] private GodCamera _godCamera;
        [SerializeField] private LineRenderer _simBorder;
        [SerializeField] private LineRenderer _osmBorder;
        [SerializeField] public  ComputeShader AdvectDiffuseCompute;
        [SerializeField] public Texture2D NoiseTex;
        [SerializeField] public Texture2D WindTex;

        public enum DataSampleMode { None, GPW, Population, Relocated, TrafficDens, Paint, Farsite }
        public DataSampleMode dataSampleMode = DataSampleMode.None;

        //never directly call these, always use singletons (except once when setting input)
        private static WUInity internal_instance;
        private WUInityInput internal_input;
        private WUInityOutput internal_output;
        private Simulation internal_sim;
        
        private Farsite.FarsiteViewer internal_farsiteViewer;
        private PopulationManager internal_population_manager;
        private WUInityGUI internal_wuiGUI;
        private Painter internal_painter;
        private Mapbox.Unity.Map.AbstractMap _mapboxMap;
        private Visualization.FireRenderer internal_fireRenderer;
        private Visualization.EvacuationRenderer internal_evacuationRenderer;

        MeshRenderer evacDataPlaneMeshRenderer;
        MeshRenderer fireDataPlaneMeshRenderer;
        List<GameObject> drawnRoads;
        GameObject[] goalMarkers;
        private GameObject evacDataPlane;
        private GameObject fireDataPlane;
        GameObject directionsGO;

        bool renderHouseholds = false;
        bool renderTraffic = false;
        bool renderSmokeDispersion = false;
        bool renderFireSpread = false;

        string internal_workingFilePath;  
        private DataStatus internal_dataStatus;

        private struct ValidCriticalData
        {
            public Vector2D lowerLeftLatLong;
            public Vector2D size;
            public float routeCellSize;

            public ValidCriticalData(WUInityInput input)
            {
                lowerLeftLatLong = input.lowerLeftLatLong;
                size = input.size;
                routeCellSize = input.evac.routeCellSize;
            }
        }
        ValidCriticalData validInput;

        string dataSampleString;
        public string GetDataSampleString()
        {
            return dataSampleString;
        }        

        private void Awake()
        {
            if (Application.isEditor)
            {
                DeveloperMode = true;
            }
            else
            {
                DeveloperMode = false;
            }

            _simBorder.gameObject.SetActive(false);
            _osmBorder.gameObject.SetActive(false);
            

            if (_godCamera == null)
            {
                _godCamera = FindObjectOfType<GodCamera>();
            }    
        }

        private void Start()
        {
            if (AutoLoadExample && DeveloperMode)
            {
                string path = Path.Combine(DATA_FOLDER, "example\\example.wui");
                if (File.Exists(path))
                {
                    WUInityInput.LoadInput(path);
                }
                else
                {
                    print("Could not find input file for auto load in path " + path);
                }
            }
        }

        /// <summary>
        /// Called when the user want to create a new file from scratch in the GUI.
        /// </summary>
        /// <param name="input"></param>
        public void CreateNewInputData()
        {
            SetNewInputData(null);
        }
        
        /// <summary>
        /// Load an existing file and try to validate all of the associated data.
        /// If data is valid it is also loaded.
        /// </summary>
        /// <param name="input"></param>
        public void SetNewInputData(WUInityInput input)
        {
            DATA_STATUS.Reset();
            DATA_STATUS.HaveInput = true;
            if(input == null)
            {
                internal_input = new WUInityInput();
            }
            else
            {
                internal_input = input;
            }
            
            validInput = new ValidCriticalData(internal_input);

            UpdateMapResourceStatus(); 
            UpdateFireResourceStatus();

            //need goals and curves before can load groups
            RUNTIME_DATA.ResponseCurves = ResponseCurve.LoadResponseCurves(); //if they can't be found we get empty ones             
            RUNTIME_DATA.EvacuationGoals = EvacuationGoal.LoadEvacuationGoalFiles();
            RUNTIME_DATA.EvacuationGroups = EvacGroup.LoadEvacGroupFiles();
            EvacGroup.LoadEvacGroupIndices(); //needs correct amount of evac groups to load

            RUNTIME_DATA.LoadRouterDb(null);
            RUNTIME_DATA.LoadRouteCollection(null);

            GraphicalFireInput.LoadGraphicalFireInput();     
            
            RUNTIME_DATA.RoadTypeData = RoadTypeData.LoadRoadTypeData();

            UpdatePopulationResourceStatus();        

            GUI.SetGUIDirty();

            //this needs map and evac goals
            SpawnMarkers();
        }

        public void UpdateMapResourceStatus()
        {
            DATA_STATUS.MapLoaded = LoadMapbox();
            UpdateSimBorders();            
            _godCamera.SetCameraStartPosition(INPUT.size);

            bool coordinatesAreDirty = true;
            bool sizeIsDirty = true;

            if (validInput.lowerLeftLatLong.x == INPUT.lowerLeftLatLong.x
                && validInput.lowerLeftLatLong.y == INPUT.lowerLeftLatLong.y)
            {
                coordinatesAreDirty = false;
            }

            if (validInput.size.x == INPUT.size.x
                && validInput.size.y == INPUT.size.y)
            {
                sizeIsDirty = false;
            }

            //fix any problems
            if(coordinatesAreDirty || sizeIsDirty)
            {
                //basically mark all data as not valid anymore
                DATA_STATUS.Reset();
                DATA_STATUS.MapLoaded = true;
            }

            //set cached data to be current data
            validInput.size = INPUT.size;
            validInput.lowerLeftLatLong = INPUT.lowerLeftLatLong;
        }

        public void UpdateEvacResourceStatus()
        {
            bool cellSizeIsDirty = true;

            if (validInput.routeCellSize == INPUT.evac.routeCellSize)
            {
                cellSizeIsDirty = false;
            }
        }

        public void UpdatePopulationResourceStatus()
        {
            POPULATION.LoadPopulationFromFile();
            DATA_STATUS.PopulationCorrectedForRoutes = POPULATION.IsPopulationCorrectedForRoutes();
            DATA_STATUS.LocalGPWLoaded = POPULATION.LoadLocalGPWFromFile();
            DATA_STATUS.GlobalGPWAvailable = LocalGPWData.IsGPWAvailable();
        }

        public void UpdateFireResourceStatus()
        {
            RUNTIME_DATA.InitialFuelMoistureData = Fire.InitialFuelMoistureList.LoadInitialFuelMoistureDataFile();
            RUNTIME_DATA.WeatherInput = Fire.WeatherInput.LoadWeatherInputFile();
            RUNTIME_DATA.WindInput = Fire.WindInput.LoadWindInputFile();
            RUNTIME_DATA.IgnitionPoints = Fire.IgnitionPoint.LoadIgnitionPointsFile();

            DATA_STATUS.LcpLoaded = RUNTIME_DATA.LoadLCPFile();
            DATA_STATUS.FuelModelsLoaded = RUNTIME_DATA.LoadFuelModelsFile();            
        }  

        private bool LoadMapbox()
        {
            //Mapbox: calculate the amount of grids needed based on zoom level, coord and size
            Mapbox.Unity.Map.MapOptions mOptions = MAP.Options; // new Mapbox.Unity.Map.MapOptions();
            mOptions.locationOptions.latitudeLongitude = "" + INPUT.lowerLeftLatLong.x + "," + INPUT.lowerLeftLatLong.y;
            mOptions.locationOptions.zoom = INPUT.zoomLevel;
            mOptions.extentOptions.extentType = Mapbox.Unity.Map.MapExtentType.RangeAroundCenter;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.west = 0;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.south = 0;
            //https://wiki.openstreetmap.org/wiki/Zoom_levels
            double tiles = Math.Pow(4.0, mOptions.locationOptions.zoom);
            double degreesPerTile = 360.0 / (Math.Pow(2.0, mOptions.locationOptions.zoom));
            Vector2D mapDegrees = LocalGPWData.SizeToDegrees(INPUT.lowerLeftLatLong, INPUT.size);
            int tilesX = (int)(mapDegrees.x / degreesPerTile) + 1;
            int tilesY = (int)(mapDegrees.y / (degreesPerTile * Math.Cos((Math.PI / 180.0) * INPUT.lowerLeftLatLong.x))) + 1;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.east = tilesX;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.north = tilesY;
            mOptions.placementOptions.placementType = Mapbox.Unity.Map.MapPlacementType.AtLocationCenter;
            mOptions.placementOptions.snapMapToZero = true;
            mOptions.scalingOptions.scalingType = Mapbox.Unity.Map.MapScalingType.WorldScale;

            if (!MAP.IsAccessTokenValid)
            {
                LOG("ERROR: Mapbox token not valid.");
                return false;
            }
            LOG("LOG: Starting to load Mapbox map.");
            MAP.Initialize(new Mapbox.Utils.Vector2d(INPUT.lowerLeftLatLong.x, INPUT.lowerLeftLatLong.y), INPUT.zoomLevel);
            LOG("LOG: Map loaded succesfully.");
            return true;
        }

        private List<string> simLog = new List<string>();
        /// <summary>
        /// Receives all the information from a WUINITY session, used by GUI.
        /// </summary>
        /// <param name="message"></param>
        public static void LOG(string message)
        {
            if (SIM.IsRunning)
            {
                message = "[" + (int)SIM.Time + "s] " + message;
            }

            INSTANCE.simLog.Add("[" + DateTime.Now.ToLongTimeString() + "] " + message);
            if (Application.isEditor || Debug.isDebugBuild)
            {
                Debug.Log(message);
            }
        }

        public static List<string> GetLog()
        {
            return INSTANCE.simLog;
        }

        public void DrawRoad(RouteCollection routeCollection, int index)
        {
            if(directionsGO == null)
            {
                directionsGO = new GameObject("Directions");
                directionsGO.transform.parent = null;
            }               

            GameObject gO = DrawRoute(routeCollection, index);
            if (gO != null)
            {
                drawnRoads.Add(gO);
            }

            gO.transform.parent = directionsGO.transform;
        }        

        GameObject DrawRoute(RouteCollection rC, int index)
        {
            List<Vector3> dat = new List<Vector3>();
            foreach (Itinero.LocalGeo.Coordinate point in rC.GetSelectedRoute().route.Shape)
            {
                Vector3 v = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(point.Latitude, point.Longitude, MAP.CenterMercator, MAP.WorldRelativeScale).ToVector3xz();
                v.y = 10f;
                dat.Add(v);
            }
            return CreateLineObject(dat, index);
        }

        GameObject CreateLineObject(List<Vector3> points, int index)
        {
            GameObject gO = new GameObject("Route " + index);
            gO.transform.position = points[0];
            //gO.transform.parent = directionsGO.transform;
            LineRenderer line = gO.AddComponent<LineRenderer>();
            line.widthMultiplier = 10f;
            line.positionCount = points.Count;

            for (int i = 0; i < points.Count; i++)
            {
                line.SetPosition(i, points[i]);
            }
            return gO;
        }

        public void DeleteDrawnRoads()
        {
            if (drawnRoads == null)
            {
                drawnRoads = new List<GameObject>();
            }
            else
            {
                for (int i = 0; i < drawnRoads.Count; i++)
                {
                    Destroy(drawnRoads[i]);
                }
                drawnRoads.Clear();
            }
        }

        public void DrawOSMNetwork()
        {

        }

        public void LoadFarsite()
        {
            FARSITE_VIEWER.ImportFarsite();
            FARSITE_VIEWER.TransformCoordinates();

            LOG("LOG: Farsite loaded succesfully.");
        }               

        public void SetSampleMode(DataSampleMode sampleMode)
        {
            dataSampleMode = sampleMode;
        }

        bool pauseSim = false;
        public void TogglePause()
        {
            pauseSim = !pauseSim;
        }

        public bool IsPaused()
        {
            return pauseSim;
        }
        
        void Update()
        {
            if (Input.GetMouseButtonDown(0) && dataSampleMode != DataSampleMode.None)
            {
                Plane _yPlane = new Plane(Vector3.up, 0f);
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                float enter = 0.0f;
                if (_yPlane.Raycast(ray, out enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);
                    float xNorm = hitPoint.x / (float)INPUT.size.x;
                    //xNorm = Mathf.Clamp01(xNorm);
                    int x = (int)(RUNTIME_DATA.EvacCellCount.x * xNorm);

                    float yNorm = hitPoint.z / (float)INPUT.size.y;
                    //yNorm = Mathf.Clamp01(yNorm);
                    int y = (int)(RUNTIME_DATA.EvacCellCount.y * yNorm);
                    GetCellInfo(hitPoint, x, y);
                }
            }           

            if(Input.GetKeyDown(KeyCode.Pause) && !RUNTIME_DATA.MultipleSimulations)
            {
                TogglePause();
            }

            if(!pauseSim && !RUNTIME_DATA.MultipleSimulations && SIM.IsRunning)
            {
                SIM.UpdateRealtimeSim();
            }
            //always updatre visuals, even when paused
            if (!RUNTIME_DATA.MultipleSimulations && SIM.IsRunning)
            {
                EVAC_VISUALS.UpdateEvacuationRenderer(renderHouseholds, renderTraffic);
                FIRE_VISUALS.UpdateFireRenderer(renderFireSpread, renderSmokeDispersion);
            }

            if (updateOSMBorder)
            {
                UpdateOSMBorder();
            }                
        }

        public void StartSimulation()
        {
            LOG("LOG: Simulation started, please wait.");            
            SetSampleMode(WUInity.DataSampleMode.TrafficDens);
            SetEvacDataPlane(true);            
            SIM.StartSimulation();

            //this needs to be done AFTER simulation has started since we need some data from the sim
            //fix everything for evac rendering
            EVAC_VISUALS.CreateBuffers(INPUT.runEvacSim, INPUT.runTrafficSim);
            renderHouseholds = INPUT.runEvacSim;
            renderTraffic = INPUT.runTrafficSim;

            //and then for fire rendering
            FIRE_VISUALS.CreateBuffers(INPUT.runFireSim, INPUT.runSmokeSim);
            renderFireSpread = INPUT.runFireSim;
            renderSmokeDispersion = INPUT.runSmokeSim;

            ShowAllRuntimeVisuals();
        }

        public void RunAllCasesInFolder(string folder)
        {
            string[] inputFiles = Directory.GetFiles(folder, "*.wui");
            for (int i = 0; i < inputFiles.Length; i++)
            {
                WUInityInput.LoadInput(inputFiles[i]);
                INPUT.simDataName = Path.GetFileNameWithoutExtension(inputFiles[i]);
                RUNTIME_DATA.MultipleSimulations = true;
                RUNTIME_DATA.General.NumberOfRuns = 100;
                StartSimulation();
            }
        }

        public void StopSimulation()
        {
            HideAllRuntimeVisuals();
            SIM.StopSim("STOP: Stopped simulation as requested by user.");
        }

        bool updateOSMBorder = false;
        public void SetOSMBorderVisibility(bool visible)
        {
            updateOSMBorder = visible;
            if(_osmBorder != null)
            {
                _osmBorder.gameObject.SetActive(updateOSMBorder);
            }            
        }

        void UpdateSimBorders()
        {
            if(!DATA_STATUS.HaveInput)
            {
                return;
            }

            if(!_simBorder.gameObject.activeSelf)
            {
                _simBorder.gameObject.SetActive(true);
            }

            Vector3 upOffset = Vector3.up * 50f;
            if (_simBorder != null)
            {
                _simBorder.SetPosition(0, Vector3.zero + upOffset);
                _simBorder.SetPosition(1, _simBorder.GetPosition(0) + Vector3.right * (float)INPUT.size.x);
                _simBorder.SetPosition(2, _simBorder.GetPosition(1) + Vector3.forward * (float)INPUT.size.y);
                _simBorder.SetPosition(3, _simBorder.GetPosition(2) - Vector3.right * (float)INPUT.size.x);
                _simBorder.SetPosition(4, _simBorder.GetPosition(0));
            }        
        }

        void UpdateOSMBorder()
        {            
            if (_osmBorder != null)
            {
                _osmBorder.SetPosition(0, -Vector3.right * RUNTIME_DATA.OSM.BorderSize - Vector3.forward * RUNTIME_DATA.OSM.BorderSize + Vector3.up * 10f);
                _osmBorder.SetPosition(1, _osmBorder.GetPosition(0) + Vector3.right * ((float)INPUT.size.x + RUNTIME_DATA.OSM.BorderSize * 2f));
                _osmBorder.SetPosition(2, _osmBorder.GetPosition(1) + Vector3.forward * ((float)INPUT.size.y + RUNTIME_DATA.OSM.BorderSize * 2f));
                _osmBorder.SetPosition(3, _osmBorder.GetPosition(2) - Vector3.right * ((float)INPUT.size.x + RUNTIME_DATA.OSM.BorderSize * 2f));
                _osmBorder.SetPosition(4, _osmBorder.GetPosition(0));
            }
        }

        void GetCellInfo(Vector3 pos, int x, int y)
        {
            dataSampleString = "No data to sample.";
            if (dataSampleMode == DataSampleMode.GPW)
            {
                if (POPULATION.GetLocalGPWData() != null && POPULATION.GetLocalGPWData().density != null && POPULATION.GetLocalGPWData().density.Length > 0)
                {
                    if (POPULATION.DATA_PLANE.activeSelf)
                    {
                        float xCellSize = (float)(POPULATION.GetLocalGPWData().realWorldSize.x / POPULATION.GetLocalGPWData().dataSize.x);
                        float yCellSize = (float)(POPULATION.GetLocalGPWData().realWorldSize.y / POPULATION.GetLocalGPWData().dataSize.y);
                        double cellArea = xCellSize * yCellSize / (1000000d);
                        dataSampleString = "GPW people count: " + System.Convert.ToInt32(POPULATION.GetLocalGPWData().GetDensityUnitySpace(new Vector2D(pos.x, pos.z)) * cellArea);
                    }
                    else
                    {
                        dataSampleString = "GPW data not visible, activate to sample data.";
                    }
                }
            }
            else if (x < 0 || x > RUNTIME_DATA.EvacCellCount.x || y < 0 || y > RUNTIME_DATA.EvacCellCount.y)
            {
                //dataSampleString = "Outside of data range.";
                return;
            }
            else if (dataSampleMode == DataSampleMode.Paint)
            {

            }
            else if (dataSampleMode == DataSampleMode.Farsite)
            {

            }
            else if (evacDataPlane != null && evacDataPlane.activeSelf)
            {
                if (dataSampleMode == DataSampleMode.Population)
                {
                    dataSampleString = "Interpolated people count: " + POPULATION.GetPopulation(x, y);
                }
                else if (dataSampleMode == DataSampleMode.Relocated)
                {
                    if (SIM.MacroHumanSim() != null)
                    {
                        dataSampleString = "Rescaled and relocated people count: " + SIM.MacroHumanSim().GetPopulation(x, y);
                    }
                }
                else if (dataSampleMode == DataSampleMode.TrafficDens)
                {
                    int people = currentPeopleInCells[x + y * RUNTIME_DATA.EvacCellCount.x];
                    dataSampleString = "People: " + people;
                    if (currenttrafficDensityData != null && currenttrafficDensityData[x + y * RUNTIME_DATA.EvacCellCount.x] != null)
                    {
                        int peopleInCars = currenttrafficDensityData[x + y * RUNTIME_DATA.EvacCellCount.x].peopleCount;
                        int cars = currenttrafficDensityData[x + y * RUNTIME_DATA.EvacCellCount.x].carCount;

                        dataSampleString += " | People in cars: " + peopleInCars + " (Cars: " + cars + "). Total people " + (people + peopleInCars);
                    }
                }
            }
            else
            {
                dataSampleString = "Data not visible, toggle on to sample data.";
            }          
        }          

        public bool IsPainterActive()
        {
            if(!PAINTER.gameObject.activeSelf)
            {
                return false;
            }

            return true;
        }

        public void StartPainter(Painter.PaintMode paintMode)
        {
            PAINTER.gameObject.SetActive(true);
            PAINTER.SetPainterMode(paintMode);
            bool fireEdit = false;
            if(paintMode == Painter.PaintMode.WUIArea)
            {
                fireEdit = true;
                DisplayWUIAreaMap();                
            }
            else if (paintMode == Painter.PaintMode.RandomIgnitionArea)
            {
                fireEdit = true;
                DisplayRandomIgnitionAreaMap();
            }
            else if (paintMode == Painter.PaintMode.InitialIgnition)
            {
                fireEdit = true;
                DisplayInitialIgnitionMap();
            }
            else if (paintMode == Painter.PaintMode.TriggerBuffer)
            {
                fireEdit = true;
                DisplayTriggerBufferMap();
            }
            else if(paintMode == Painter.PaintMode.EvacGroup)
            {
                DisplayEvacGroupMap();
            }
            else if (paintMode == Painter.PaintMode.CustomPopulation)
            {
                DisplayCustomPopulationData();
            }
            else
            {
                LOG("ERROR: Paint mode not set correctly");
            }
            dataSampleMode = DataSampleMode.Paint;

            if(fireEdit)
            {
                SetEvacDataPlane(false);
                SetFireDataPlane(true);
            }
            else
            {
                SetEvacDataPlane(true);
                SetFireDataPlane(false);
            }
        }

        public void StopPainter()
        {
            PAINTER.gameObject.SetActive(false);
            dataSampleMode = DataSampleMode.None;
            if (evacDataPlane != null)
            {
                evacDataPlane.SetActive(false);
            }
            if (fireDataPlane != null)
            {
                fireDataPlane.SetActive(false);
            }
        }

        private MeshRenderer CreateDataPlane(Texture2D tex, string name, Vector2Int cellCount)
        {
            GameObject gO = new GameObject(name);
            gO.transform.parent = this.transform;
            gO.isStatic = true;
            // You can change that line to provide another MeshFilter
            MeshFilter filter = gO.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh(); // filter.mesh;
            filter.mesh = mesh;
            MeshRenderer mR = gO.AddComponent<MeshRenderer>();
            mR.receiveShadows = false;
            mR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mesh.Clear();

            float width = (float)INPUT.size.x;
            float length = (float)INPUT.size.y;


            Vector3 offset = Vector3.zero;

            Vector2 maxUV = new Vector2((float)cellCount.x / tex.width, (float)cellCount.y / tex.height);

            PopulationManager.CreateSimplePlane(mesh, width, length, 0.0f, offset, maxUV);

            Material mat = new Material(Shader.Find("Unlit/Transparent"));
            mat.mainTexture = tex;

            mR.material = mat;

            //move up one meter
            gO.transform.position += Vector3.up;
            gO.SetActive(false); //create hidden

            return mR;
        }

        public void SpawnMarkers()
        {
            if (goalMarkers != null)
            {
                for (int i = 0; i < goalMarkers.Length; i++)
                {
                    if(goalMarkers[i] != null)
                    {
                        Destroy(goalMarkers[i]);
                    }                    
                }
            }

            goalMarkers = new GameObject[RUNTIME_DATA.EvacuationGoals.Length];
            for (int i = 0; i < RUNTIME_DATA.EvacuationGoals.Length; i++)
            {
                EvacuationGoal eG = RUNTIME_DATA.EvacuationGoals[i];
                goalMarkers[i] = Instantiate<GameObject>(_markerPrefab);
                Mapbox.Utils.Vector2d pos = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(eG.latLong.x, eG.latLong.y, MAP.CenterMercator, MAP.WorldRelativeScale);

                float scale = 0.01f * (float)INPUT.size.y;
                goalMarkers[i].transform.localScale = new Vector3(scale, 100f, scale);
                goalMarkers[i].transform.position = new Vector3((float)pos.x, 0f, (float)pos.y);
                MeshRenderer mR = goalMarkers[i].GetComponent<MeshRenderer>();
                mR.material.color = eG.color;
            }            
        }        
                
        TrafficCellData[] currenttrafficDensityData;
        int[] currentPeopleInCells;
        public void DisplayClosestDensityData(float time)
        {
            if(INPUT.runTrafficSim)
            {
                int index = Mathf.Max(0, (int)time / (int)INPUT.traffic.saveInterval);
                if (index > outputTextures.Count - 1)
                {
                    index = outputTextures.Count - 1;
                }
                Texture2D tex = outputTextures[index];

                currenttrafficDensityData = trafficDensityData[index];
                currentPeopleInCells = peopleInCells[index];

                SetDataPlaneTexture(tex);
            }            
        }

        public void ShowAllRuntimeVisuals()
        {
            SetHouseholdRendering(true);
            SetTrafficRendering(true);
            SetFireSpreadRendering(true);
            SetSootRendering(true);
        }

        public void HideAllRuntimeVisuals()
        {
            SetHouseholdRendering(false);
            SetTrafficRendering(false);
            SetFireSpreadRendering(false);
            SetSootRendering(false);
        }

        public void DisplayPopulation()
        {
            SetDataPlaneTexture(POPULATION.GetPopulationTexture());
        }

        private void DisplayWUIAreaMap()
        {
            SetDataPlaneTexture(PAINTER.GetWUIAreaTexture(), true);
        }

        public void DisplayRandomIgnitionAreaMap()
        {
            SetDataPlaneTexture(PAINTER.GetRandomIgnitionTexture(), true);
        }

        public void DisplayInitialIgnitionMap()
        {
            SetDataPlaneTexture(PAINTER.GetInitialIgnitionTexture(), true);
        }

        public void DisplayTriggerBufferMap()
        {
            SetDataPlaneTexture(PAINTER.GetTriggerBufferTexture(), true);
        }

        public void DisplayEvacGroupMap()
        {
            SetDataPlaneTexture(PAINTER.GetEvacGroupTexture());
        }

        public void DisplayCustomPopulationData()
        {
            SetDataPlaneTexture(PAINTER.GetCustomPopulationMaskTexture());
        }

        public  void SetEvacDataPlane(bool setActive)
        {
            if (evacDataPlaneMeshRenderer != null)
            {
                evacDataPlaneMeshRenderer.gameObject.SetActive(setActive);
            }
        }

        public void SetFireDataPlane(bool setActive)
        {
            if (fireDataPlaneMeshRenderer != null)
            {
                fireDataPlaneMeshRenderer.gameObject.SetActive(setActive);
            }
        }

        public bool ToggleEvacDataPlane()
        {
            if (evacDataPlaneMeshRenderer != null)
            {
                evacDataPlaneMeshRenderer.gameObject.SetActive(!evacDataPlaneMeshRenderer.gameObject.activeSelf);
                return evacDataPlaneMeshRenderer.gameObject.activeSelf;
            }

            return false;
        }

        public void SetHouseholdRendering(bool enable)
        {
            if (renderHouseholds != enable)
            {
                ToggleHouseholdRendering();
            }
        }

        public bool ToggleHouseholdRendering()
        {
            renderHouseholds = !renderHouseholds;
            return renderHouseholds;
        }

        public void SetTrafficRendering(bool enable)
        {
            if(renderTraffic != enable)
            {
                ToggleTrafficRendering();
            }
        }

        public bool ToggleTrafficRendering()
        {
            renderTraffic = !renderTraffic;
            return renderTraffic;
        }

        public void SetSootRendering(bool enable)
        {
            if(enable != renderSmokeDispersion)
            {
                ToggleSootRendering();
            }
        }

        public bool ToggleSootRendering()
        {
            renderSmokeDispersion = FIRE_VISUALS.ToggleSoot();
            return renderSmokeDispersion;
        }

        public void SetFireSpreadRendering(bool enable)
        {
            if(enable != renderFireSpread)
            {
                ToggleFireSpreadRendering();
            }
        }

        public bool ToggleFireSpreadRendering()
        {
            renderFireSpread = FIRE_VISUALS.ToggleFire();
            return renderFireSpread;
        }

        private void SetDataPlaneTexture(Texture2D tex, bool fireMeshMode = false)
        {
            //pick needed data plane
            MeshRenderer activeMeshRenderer = evacDataPlaneMeshRenderer;
            Vector2Int cellCount = RUNTIME_DATA.EvacCellCount;
            string name = "Evac Data Plane";
            if (fireMeshMode)
            {
                activeMeshRenderer = fireDataPlaneMeshRenderer;
                cellCount = SIM.FireMesh().cellCount;
                name = "Fire Data Plane";
            }

            //make sure it exists, else create
            if (activeMeshRenderer == null)
            {
                activeMeshRenderer = CreateDataPlane(tex, name, cellCount);
                if(fireMeshMode)
                {
                    fireDataPlaneMeshRenderer = activeMeshRenderer;
                    fireDataPlane = activeMeshRenderer.gameObject;
                }
                else
                {
                    evacDataPlaneMeshRenderer = activeMeshRenderer;
                    evacDataPlane = activeMeshRenderer.gameObject;
                }                
            }
            else
            {
                activeMeshRenderer.material.mainTexture = tex;                
            }
        }

        Color GetTrafficDensityColor(int cars)
        {
            float fraction = Mathf.Lerp(0f, 1f, cars / 20f);
            Color c = Color.HSVToRGB(0.67f - 0.67f * fraction, 1.0f, 1.0f);

            return c;
        }

        List<TrafficCellData[]> trafficDensityData;
        List<int[]> peopleInCells;
        public List<Texture2D> outputTextures;
        public void SaveTransientDensityData(float time, List<MacroCar> carsInSystem, List<MacroCar> carsOnHold)
        {
            //first time
            if (trafficDensityData == null)
            {
                trafficDensityData = new List<TrafficCellData[]>();
                outputTextures = new List<Texture2D>();
                peopleInCells = new List<int[]>();
            }
            //if new data interval
            int outputIndex = (int)(time - SIM.StartTime) / (int)INPUT.traffic.saveInterval;
            if (outputIndex > trafficDensityData.Count - 1)
            {
                trafficDensityData.Add(new TrafficCellData[RUNTIME_DATA.EvacCellCount.x * RUNTIME_DATA.EvacCellCount.y]);

                for (int i = 0; i < carsInSystem.Count; i++)
                {
                    Vector4 posAndSpeed = carsInSystem[i].GetUnityPositionAndSpeed(false);

                    int x = (int)(posAndSpeed.x / INPUT.evac.routeCellSize);
                    int y = (int)(posAndSpeed.y / INPUT.evac.routeCellSize);

                    //outside of mapped data
                    if (x < 0 || x > RUNTIME_DATA.EvacCellCount.x - 1 || y < 0 || y > RUNTIME_DATA.EvacCellCount.y - 1)
                    {
                        continue;
                    }

                    //add or update data
                    if (trafficDensityData[outputIndex][x + y * RUNTIME_DATA.EvacCellCount.x] == null)
                    {
                        trafficDensityData[outputIndex][x + y * RUNTIME_DATA.EvacCellCount.x] = new TrafficCellData();
                        trafficDensityData[outputIndex][x + y * RUNTIME_DATA.EvacCellCount.x].carCount = 1;
                        trafficDensityData[outputIndex][x + y * RUNTIME_DATA.EvacCellCount.x].peopleCount = carsInSystem[i].numberOfPeopleInCar;
                    }
                    else
                    {
                        trafficDensityData[outputIndex][x + y * RUNTIME_DATA.EvacCellCount.x].carCount += 1;
                        trafficDensityData[outputIndex][x + y * RUNTIME_DATA.EvacCellCount.x].peopleCount += carsInSystem[i].numberOfPeopleInCar;
                    }
                }

                for (int i = 0; i < carsOnHold.Count; i++)
                {
                    Vector4 posAndSpeed = carsOnHold[i].GetUnityPositionAndSpeed(false);

                    int x = (int)(posAndSpeed.x / INPUT.evac.routeCellSize);
                    int y = (int)(posAndSpeed.y / INPUT.evac.routeCellSize);

                    //outside of mapped data
                    if (x < 0 || x > RUNTIME_DATA.EvacCellCount.x - 1 || y < 0 || y > RUNTIME_DATA.EvacCellCount.y - 1)
                    {
                        continue;
                    }

                    //add or update data
                    if (trafficDensityData[outputIndex][x + y * RUNTIME_DATA.EvacCellCount.x] == null)
                    {
                        trafficDensityData[outputIndex][x + y * RUNTIME_DATA.EvacCellCount.x] = new TrafficCellData();
                        trafficDensityData[outputIndex][x + y * RUNTIME_DATA.EvacCellCount.x].carCount = 1;
                        trafficDensityData[outputIndex][x + y * RUNTIME_DATA.EvacCellCount.x].peopleCount = carsOnHold[i].numberOfPeopleInCar;
                    }
                    else
                    {
                        trafficDensityData[outputIndex][x + y * RUNTIME_DATA.EvacCellCount.x].carCount += 1;
                        trafficDensityData[outputIndex][x + y * RUNTIME_DATA.EvacCellCount.x].peopleCount += carsOnHold[i].numberOfPeopleInCar;
                    }
                }

                //save data from human re as well
                peopleInCells.Add(new int[RUNTIME_DATA.EvacCellCount.x * RUNTIME_DATA.EvacCellCount.y]);
                for (int y = 0; y < RUNTIME_DATA.EvacCellCount.y; y++)
                {
                    for (int x = 0; x < RUNTIME_DATA.EvacCellCount.x; x++)
                    {
                        peopleInCells[outputIndex][x + y * RUNTIME_DATA.EvacCellCount.x] = SIM.MacroHumanSim().GetPeopleLeftInCell(x, y);
                    }
                }

                //create texture
                Vector2Int res = new Vector2Int(2, 2);
                while (RUNTIME_DATA.EvacCellCount.x > res.x)
                {
                    res.x *= 2;
                }
                while (RUNTIME_DATA.EvacCellCount.y > res.y)
                {
                    res.y *= 2;
                }

                Texture2D tex = new Texture2D(res.x, res.y);
                tex.filterMode = FilterMode.Point;

                for (int y = 0; y < RUNTIME_DATA.EvacCellCount.y; ++y)
                {
                    for (int x = 0; x < RUNTIME_DATA.EvacCellCount.x; ++x)
                    {
                        Color c = Color.grey;
                        c.a = 0.0f;
                        int count = 0;
                        if (trafficDensityData[outputIndex][x + y * RUNTIME_DATA.EvacCellCount.x] != null)
                        {
                            count += trafficDensityData[outputIndex][x + y * RUNTIME_DATA.EvacCellCount.x].carCount;
                        }
                        //count += peopleInCells[outputIndex][x + y * WUInity.SIM.GetCellCount.x];
                        if (count > 0)
                        {
                            c = GetTrafficDensityColor(count);
                            c.a = 0.5f;
                        }
                        tex.SetPixel(x, y, c);
                    }
                }
                tex.Apply();
                outputTextures.Add(tex);
                byte[] bytes = tex.EncodeToPNG();
                File.WriteAllBytes(Path.Combine(WUInity.OUTPUT_FOLDER, "trafficDens_" + (int)time + "s.png"), bytes);
            }
        }
    }
}