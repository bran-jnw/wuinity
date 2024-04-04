using System.Collections;               
using System.Collections.Generic;       
using UnityEngine;                     
using OsmSharp.Streams;                 
using WUInity.Pedestrian;                     
using WUInity.Traffic;                 
using WUInity.Population;                     
using System;                           
using System.IO;                        
using Mapbox.Utils;                    
using Mapbox.Unity.Utilities;
using WUInity.Runtime;
using WUInity.UI;


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
                if(_instance == null)
                {
                    _instance = FindObjectOfType<WUInity>();
                    if(_instance == null)
                    {
                        GameObject g = new GameObject();
                        _instance = g.AddComponent<WUInity>();
                    }
                }
                return _instance;
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
                if (INSTANCE._evacuationRenderer == null)
                {
                    INSTANCE._evacuationRenderer = INSTANCE.GetComponent<Visualization.EvacuationRenderer>();
                    if(INSTANCE._evacuationRenderer == null)
                    {
                        INSTANCE._evacuationRenderer = INSTANCE.gameObject.AddComponent<Visualization.EvacuationRenderer>();
                    }
                }
                return INSTANCE._evacuationRenderer;
            }
        }

        public static Visualization.FireRenderer FIRE_VISUALS
        {
            get
            {
                if (INSTANCE._fireRenderer == null)
                {
                    INSTANCE._fireRenderer = INSTANCE.GetComponent<Visualization.FireRenderer>();
                    if (INSTANCE._fireRenderer == null)
                    {
                        INSTANCE._fireRenderer = INSTANCE.gameObject.AddComponent<Visualization.FireRenderer>();
                    }
                }
                return INSTANCE._fireRenderer;
            }
        }

        public static Simulation SIM
        {
            get
            {
                if (INSTANCE._sim == null)
                {
                    INSTANCE._sim= new Simulation();
                }
                return INSTANCE._sim;
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
                if(INSTANCE._wuiGUI == null)
                {
                    INSTANCE._wuiGUI = INSTANCE.GetComponent<WUInityGUI>();
                    if(INSTANCE._wuiGUI == null)
                    {
                        INSTANCE.gameObject.AddComponent<WUInityGUI>();
                    }
                }
                return INSTANCE._wuiGUI;
            }
        }

        public static WUInityInput INPUT
        {
            get
            {
                /*if (INSTANCE._input == null)
                {
                    INSTANCE._input = new WUInityInput();
                }*/
                return INSTANCE._input;
            }
        }

        public static WUInityOutput OUTPUT
        {
            get
            {
                if(INSTANCE._output == null)
                {
                    INSTANCE._output = new WUInityOutput();
                }
                return INSTANCE._output;
            }
        }

        public static Farsite.FarsiteViewer FARSITE_VIEWER
        {
            get
            {
                if(INSTANCE._farsiteViewer == null)
                {
                    INSTANCE._farsiteViewer = new Farsite.FarsiteViewer();
                }
                return INSTANCE._farsiteViewer;
            }
        }

        public static PopulationManager POPULATION
        {
            get
            {
                if (INSTANCE._population_manager == null)
                {
                    INSTANCE._population_manager = new PopulationManager();
                }
                return INSTANCE._population_manager;
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
                if(INSTANCE._painter == null)
                {
                    GameObject g = new GameObject();
                    g.transform.parent = INSTANCE.transform;
                    g.name = "WUI Painter";
                    INSTANCE._painter = g.AddComponent<Painter>();
                    g.SetActive(false);
                }
                return INSTANCE._painter;
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
        private static WUInity _instance;
        private WUInityInput _input;
        private WUInityOutput _output;
        private Simulation _sim;
        
        private Farsite.FarsiteViewer _farsiteViewer;
        private PopulationManager _population_manager;
        private WUInityGUI _wuiGUI;
        private Painter _painter;
        private Mapbox.Unity.Map.AbstractMap _mapboxMap;
        private Visualization.FireRenderer _fireRenderer;
        private Visualization.EvacuationRenderer _evacuationRenderer;

        MeshRenderer _evacDataPlaneMeshRenderer;
        MeshRenderer _fireDataPlaneMeshRenderer;
        List<GameObject> drawnRoad_s;
        GameObject[] _goalMarkers;
        private GameObject _evacDataPlane;
        private GameObject _fireDataPlane;
        GameObject _directionsGO;

        bool renderHouseholds = false;
        bool renderTraffic = false;
        bool renderSmokeDispersion = false;
        bool renderFireSpread = false;

        string internal_workingFilePath;  
        private DataStatus internal_dataStatus;

        private struct ValidCriticalData
        {
            public Vector2d lowerLeftLatLong;
            public Vector2d size;
            public float routeCellSize;

            public ValidCriticalData(WUInityInput input)
            {
                lowerLeftLatLong = input.Simulation.LowerLeftLatLong;
                size = input.Simulation.Size;
                routeCellSize = input.Evacuation.RouteCellSize;
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
            //needed for proper reading of input files
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            if (AutoLoadExample && DeveloperMode)
            {
                string path = Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "example\\example.wui");
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
                _input = new WUInityInput();
            }
            else
            {
                _input = input;
            }
            
            validInput = new ValidCriticalData(_input);

            //transform input to actual data
            RUNTIME_DATA.Population.LoadAll();
            RUNTIME_DATA.Evacuation.LoadAll();
            //need to load evacuation goals before routing as they rely on evacuation goals
            RUNTIME_DATA.Routing.LoadAll();            
            RUNTIME_DATA.Traffic.LoadAll();
            RUNTIME_DATA.Fire.LoadAll();

            UpdateMapResourceStatus();           

            GUI.SetDirty();

            //this needs map and evac goals
            SpawnMarkers();
        }

        public void UpdateMapResourceStatus()
        {
            DATA_STATUS.MapLoaded = RUNTIME_DATA.LoadMapbox();
            UpdateSimBorders();            
            _godCamera.SetCameraStartPosition(INPUT.Simulation.Size);

            bool coordinatesAreDirty = true;
            bool sizeIsDirty = true;

            if (validInput.lowerLeftLatLong.x == INPUT.Simulation.LowerLeftLatLong.x
                && validInput.lowerLeftLatLong.y == INPUT.Simulation.LowerLeftLatLong.y)
            {
                coordinatesAreDirty = false;
            }

            if (validInput.size.x == INPUT.Simulation.Size.x
                && validInput.size.y == INPUT.Simulation.Size.y)
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
            validInput.size = INPUT.Simulation.Size;
            validInput.lowerLeftLatLong = INPUT.Simulation.LowerLeftLatLong;
        }

        public void UpdateEvacResourceStatus()
        {
            bool cellSizeIsDirty = true;

            if (validInput.routeCellSize == INPUT.Evacuation.RouteCellSize)
            {
                cellSizeIsDirty = false;
            }
        }     

        private List<string> simLog = new List<string>();
        public enum LogType { Log, Warning, Error, Event };
        /// <summary>
        /// Receives all the information from a WUINITY session, used by GUI.
        /// </summary>
        /// <param name="message"></param>
        public static void CONSOLE(LogType logType, string message)
        {            
            if (SIM.State == Simulation.SimulationState.Running)
            {
                message = "[" + (int)SIM.CurrentTime + "s] " + message;
            }

            if (logType == LogType.Warning)
            {
                message = "WARNING: " + message;
            }
            else if (logType == LogType.Error)
            {
                message = "ERROR: " + message;                
            }
            else if (logType == LogType.Event)
            {
                message = "EVENT: " + message;
            }
            else
            {
                message = "LOG: " + message;
            }

            INSTANCE.simLog.Add("[" + DateTime.Now.ToLongTimeString() + "] " + message);
            if (Application.isEditor || Debug.isDebugBuild)
            {
                Debug.Log(message);
            }

            if (logType == LogType.Error)
            {
                SIM.StopSim("Simulation can't run, please check log.");
            }
        }

        public static List<string> GetLog()
        {
            return INSTANCE.simLog;
        }

        public void DrawRoad(RouteCollection routeCollection, int index)
        {
            if(_directionsGO == null)
            {
                _directionsGO = new GameObject("Directions");
                _directionsGO.transform.parent = null;
            }               

            GameObject gO = DrawRoute(routeCollection, index);
            if (gO != null)
            {
                drawnRoad_s.Add(gO);
            }

            gO.transform.parent = _directionsGO.transform;
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
            if (drawnRoad_s == null)
            {
                drawnRoad_s = new List<GameObject>();
            }
            else
            {
                for (int i = 0; i < drawnRoad_s.Count; i++)
                {
                    Destroy(drawnRoad_s[i]);
                }
                drawnRoad_s.Clear();
            }
        }

        public void DrawOSMNetwork()
        {

        }

        /*public void LoadFarsite()
        {
            FARSITE_VIEWER.ImportFarsite();
            FARSITE_VIEWER.TransformCoordinates();

            LOG(WUInity.LogType.Warning, "Farsite loaded succesfully.");
        }*/           

        public void SetSampleMode(DataSampleMode sampleMode)
        {
            dataSampleMode = sampleMode;
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
                    float xNorm = hitPoint.x / (float)INPUT.Simulation.Size.x;
                    //xNorm = Mathf.Clamp01(xNorm);
                    int x = (int)(RUNTIME_DATA.Evacuation.CellCount.x * xNorm);

                    float yNorm = hitPoint.z / (float)INPUT.Simulation.Size.y;
                    //yNorm = Mathf.Clamp01(yNorm);
                    int y = (int)(RUNTIME_DATA.Evacuation.CellCount.y * yNorm);
                    GetCellInfo(hitPoint, x, y);
                }
            }    

            //always updatre visuals, even when paused
            if (!RUNTIME_DATA.Simulation.MultipleSimulations && SIM.State == Simulation.SimulationState.Running)
            {
                if(!_visualsExist)
                {
                    CreateVisualizers();
                }
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
            _visualsExist = false;
            SetSampleMode(WUInity.DataSampleMode.TrafficDens);
            // SetEvacDataPlane(true);   // This is turned off as we don't want to display the _evacDataPlaneMeshRenderer by default at the start of the simulation.   16/08/2023 
            SIM.Start(INPUT);
        }

        bool _visualsExist = false;
        public void CreateVisualizers()
        {
            //this needs to be done AFTER simulation has started since we need some data from the sim
            //fix everything for evac rendering
            EVAC_VISUALS.CreateBuffers(INPUT.Simulation.RunPedestrianModule, INPUT.Simulation.RunTrafficModule);
            renderHouseholds = INPUT.Simulation.RunPedestrianModule;
            renderTraffic = INPUT.Simulation.RunTrafficModule;

            //and then for fire rendering
            FIRE_VISUALS.CreateBuffers(INPUT.Simulation.RunFireModule, INPUT.Simulation.RunSmokeModule);
            renderFireSpread = INPUT.Simulation.RunFireModule;
            renderSmokeDispersion = INPUT.Simulation.RunSmokeModule;

            _visualsExist = true;

            ShowAllRuntimeVisuals();
        }

        public void RunAllCasesInFolder(string folder)
        {
            string[] inputFiles = Directory.GetFiles(folder, "*.wui");
            for (int i = 0; i < inputFiles.Length; i++)
            {
                WUInityInput.LoadInput(inputFiles[i]);
                INPUT.Simulation.SimulationID = Path.GetFileNameWithoutExtension(inputFiles[i]);
                RUNTIME_DATA.Simulation.MultipleSimulations = true;
                RUNTIME_DATA.Simulation.NumberOfRuns = 100;
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
                _simBorder.SetPosition(1, _simBorder.GetPosition(0) + Vector3.right * (float)INPUT.Simulation.Size.x);
                _simBorder.SetPosition(2, _simBorder.GetPosition(1) + Vector3.forward * (float)INPUT.Simulation.Size.y);
                _simBorder.SetPosition(3, _simBorder.GetPosition(2) - Vector3.right * (float)INPUT.Simulation.Size.x);
                _simBorder.SetPosition(4, _simBorder.GetPosition(0));
            }        
        }

        void UpdateOSMBorder()
        {            
            if (_osmBorder != null)
            {
                _osmBorder.SetPosition(0, -Vector3.right * RUNTIME_DATA.Routing.BorderSize - Vector3.forward * RUNTIME_DATA.Routing.BorderSize + Vector3.up * 10f);
                _osmBorder.SetPosition(1, _osmBorder.GetPosition(0) + Vector3.right * ((float)INPUT.Simulation.Size.x + RUNTIME_DATA.Routing.BorderSize * 2f));
                _osmBorder.SetPosition(2, _osmBorder.GetPosition(1) + Vector3.forward * ((float)INPUT.Simulation.Size.y + RUNTIME_DATA.Routing.BorderSize * 2f));
                _osmBorder.SetPosition(3, _osmBorder.GetPosition(2) - Vector3.right * ((float)INPUT.Simulation.Size.x + RUNTIME_DATA.Routing.BorderSize * 2f));
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
                    if (POPULATION.Visualizer.IsDataPlaneActive())
                    {
                        float xCellSize = (float)(POPULATION.GetLocalGPWData().realWorldSize.x / POPULATION.GetLocalGPWData().dataSize.x);
                        float yCellSize = (float)(POPULATION.GetLocalGPWData().realWorldSize.y / POPULATION.GetLocalGPWData().dataSize.y);
                        double cellArea = xCellSize * yCellSize / (1000000d);
                        dataSampleString = "GPW people count: " + System.Convert.ToInt32(POPULATION.GetLocalGPWData().GetDensityUnitySpace(new Vector2d(pos.x, pos.z)) * cellArea);
                    }
                    else
                    {
                        dataSampleString = "GPW data not visible, activate to sample data.";
                    }
                }
            }
            else if (x < 0 || x > RUNTIME_DATA.Evacuation.CellCount.x || y < 0 || y > RUNTIME_DATA.Evacuation.CellCount.y)
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
            else if (_evacDataPlane != null && _evacDataPlane.activeSelf)
            {
                if (dataSampleMode == DataSampleMode.Population)
                {
                    dataSampleString = "Interpolated people count: " + POPULATION.GetPopulation(x, y);
                }
                else if (dataSampleMode == DataSampleMode.Relocated)
                {
                    if (SIM.PedestrianModule != null)
                    {
                        dataSampleString = "Rescaled and relocated people count: " + ((MacroHouseholdSim)SIM.PedestrianModule).GetPopulation(x, y);
                    }
                }
                else if (dataSampleMode == DataSampleMode.TrafficDens)
                {
                    int people = currentPeopleInCells[x + y * RUNTIME_DATA.Evacuation.CellCount.x];
                    dataSampleString = "People: " + people;
                    if (currenttrafficDensityData != null && currenttrafficDensityData[x + y * RUNTIME_DATA.Evacuation.CellCount.x] != null)
                    {
                        int peopleInCars = currenttrafficDensityData[x + y * RUNTIME_DATA.Evacuation.CellCount.x].peopleCount;
                        int cars = currenttrafficDensityData[x + y * RUNTIME_DATA.Evacuation.CellCount.x].carCount;

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
                CONSOLE(WUInity.LogType.Error, "Paint mode not set correctly");
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
            if (_evacDataPlane != null)
            {
                _evacDataPlane.SetActive(false);
            }
            if (_fireDataPlane != null)
            {
                _fireDataPlane.SetActive(false);
            }
        }

        private MeshRenderer CreateDataPlane(Texture2D tex, string name, Vector2int cellCount)
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

            float width = (float)INPUT.Simulation.Size.x;
            float length = (float)INPUT.Simulation.Size.y;


            Vector3 offset = Vector3.zero;

            Vector2 maxUV = new Vector2((float)cellCount.x / tex.width, (float)cellCount.y / tex.height);

            Visualization.VisualizeUtilities.CreateSimplePlane(mesh, width, length, 0.0f, offset, maxUV);

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
            if (_goalMarkers != null)
            {
                for (int i = 0; i < _goalMarkers.Length; i++)
                {
                    if(_goalMarkers[i] != null)
                    {
                        Destroy(_goalMarkers[i]);
                    }                    
                }
            }

            _goalMarkers = new GameObject[RUNTIME_DATA.Evacuation.EvacuationGoals.Count];
            for (int i = 0; i < RUNTIME_DATA.Evacuation.EvacuationGoals.Count; i++)
            {
                EvacuationGoal eG = RUNTIME_DATA.Evacuation.EvacuationGoals[i];
                _goalMarkers[i] = Instantiate<GameObject>(_markerPrefab);
                Mapbox.Utils.Vector2d pos = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(eG.latLong.x, eG.latLong.y, MAP.CenterMercator, MAP.WorldRelativeScale);

                float scale = 0.01f * (float)INPUT.Simulation.Size.y;
                _goalMarkers[i].transform.localScale = new Vector3(scale, 100f, scale);
                _goalMarkers[i].transform.position = new Vector3((float)pos.x, 0f, (float)pos.y);
                MeshRenderer mR = _goalMarkers[i].GetComponent<MeshRenderer>();
                mR.material.color = eG.color.UnityColor;
            }            
        }        
                
        TrafficCellData[] currenttrafficDensityData;
        int[] currentPeopleInCells;
        public void DisplayClosestDensityData(float time)
        {
            if(INPUT.Simulation.RunTrafficModule)
            {
                int index = Mathf.Max(0, (int)time / (int)INPUT.Traffic.saveInterval);
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
            SetDataPlaneTexture((Texture2D)POPULATION.Visualizer.GetPopulationTexture());
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
            if (_evacDataPlaneMeshRenderer != null)
            {
                _evacDataPlaneMeshRenderer.gameObject.SetActive(setActive);
            }
        }

        public void SetFireDataPlane(bool setActive)
        {
            if (_fireDataPlaneMeshRenderer != null)
            {
                _fireDataPlaneMeshRenderer.gameObject.SetActive(setActive);
            }
        }

        public bool ToggleEvacDataPlane()
        {
            if (_evacDataPlaneMeshRenderer != null)
            {
                _evacDataPlaneMeshRenderer.gameObject.SetActive(!_evacDataPlaneMeshRenderer.gameObject.activeSelf);
                return _evacDataPlaneMeshRenderer.gameObject.activeSelf;
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
            MeshRenderer activeMeshRenderer = _evacDataPlaneMeshRenderer;
            Vector2int cellCount = RUNTIME_DATA.Evacuation.CellCount;
            string name = "Evac Data Plane";
            if (fireMeshMode)
            {
                activeMeshRenderer = _fireDataPlaneMeshRenderer;
                cellCount = new Vector2int(SIM.FireModule.GetCellCountX(), SIM.FireModule.GetCellCountY());
                name = "Fire Data Plane";
            }

            //make sure it exists, else create
            if (activeMeshRenderer == null)
            {
                activeMeshRenderer = CreateDataPlane(tex, name, cellCount);
                if(fireMeshMode)
                {
                    _fireDataPlaneMeshRenderer = activeMeshRenderer;
                    _fireDataPlane = activeMeshRenderer.gameObject;
                }
                else
                {
                    _evacDataPlaneMeshRenderer = activeMeshRenderer;
                    _evacDataPlane = activeMeshRenderer.gameObject;
                }                
            }
            else
            {
                activeMeshRenderer.material.mainTexture = tex;                
            }
        }

        WUInityColor GetTrafficDensityColor(int cars)
        {
            float fraction = Mathf.Lerp(0f, 1f, cars / 20f);
            WUInityColor c = WUInityColor.HSVToRGB(0.67f - 0.67f * fraction, 1.0f, 1.0f);

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
            int outputIndex = (int)(time - SIM.StartTime) / (int)INPUT.Traffic.saveInterval;
            if (outputIndex > trafficDensityData.Count - 1)
            {
                trafficDensityData.Add(new TrafficCellData[RUNTIME_DATA.Evacuation.CellCount.x * RUNTIME_DATA.Evacuation.CellCount.y]);

                for (int i = 0; i < carsInSystem.Count; i++)
                {
                    System.Numerics.Vector4 posAndSpeed = carsInSystem[i].GetUnityPositionAndSpeed(false);

                    int x = (int)(posAndSpeed.X / INPUT.Evacuation.RouteCellSize);
                    int y = (int)(posAndSpeed.Y / INPUT.Evacuation.RouteCellSize);

                    //outside of mapped data
                    if (x < 0 || x > RUNTIME_DATA.Evacuation.CellCount.x - 1 || y < 0 || y > RUNTIME_DATA.Evacuation.CellCount.y - 1)
                    {
                        continue;
                    }

                    //add or update data
                    if (trafficDensityData[outputIndex][x + y * RUNTIME_DATA.Evacuation.CellCount.x] == null)
                    {
                        trafficDensityData[outputIndex][x + y * RUNTIME_DATA.Evacuation.CellCount.x] = new TrafficCellData();
                        trafficDensityData[outputIndex][x + y * RUNTIME_DATA.Evacuation.CellCount.x].carCount = 1;
                        trafficDensityData[outputIndex][x + y * RUNTIME_DATA.Evacuation.CellCount.x].peopleCount = (int)carsInSystem[i].numberOfPeopleInCar;
                    }
                    else
                    {
                        trafficDensityData[outputIndex][x + y * RUNTIME_DATA.Evacuation.CellCount.x].carCount += 1;
                        trafficDensityData[outputIndex][x + y * RUNTIME_DATA.Evacuation.CellCount.x].peopleCount += (int)carsInSystem[i].numberOfPeopleInCar;
                    }
                }

                for (int i = 0; i < carsOnHold.Count; i++)
                {
                    System.Numerics.Vector4 posAndSpeed = carsOnHold[i].GetUnityPositionAndSpeed(false);

                    int x = (int)(posAndSpeed.X / INPUT.Evacuation.RouteCellSize);
                    int y = (int)(posAndSpeed.Y / INPUT.Evacuation.RouteCellSize);

                    //outside of mapped data
                    if (x < 0 || x > RUNTIME_DATA.Evacuation.CellCount.x - 1 || y < 0 || y > RUNTIME_DATA.Evacuation.CellCount.y - 1)
                    {
                        continue;
                    }

                    //add or update data
                    if (trafficDensityData[outputIndex][x + y * RUNTIME_DATA.Evacuation.CellCount.x] == null)
                    {
                        trafficDensityData[outputIndex][x + y * RUNTIME_DATA.Evacuation.CellCount.x] = new TrafficCellData();
                        trafficDensityData[outputIndex][x + y * RUNTIME_DATA.Evacuation.CellCount.x].carCount = 1;
                        trafficDensityData[outputIndex][x + y * RUNTIME_DATA.Evacuation.CellCount.x].peopleCount = (int)carsOnHold[i].numberOfPeopleInCar;
                    }
                    else
                    {
                        trafficDensityData[outputIndex][x + y * RUNTIME_DATA.Evacuation.CellCount.x].carCount += 1;
                        trafficDensityData[outputIndex][x + y * RUNTIME_DATA.Evacuation.CellCount.x].peopleCount += (int)carsOnHold[i].numberOfPeopleInCar;
                    }
                }

                //save data from human re as well
                peopleInCells.Add(new int[RUNTIME_DATA.Evacuation.CellCount.x * RUNTIME_DATA.Evacuation.CellCount.y]);
                for (int y = 0; y < RUNTIME_DATA.Evacuation.CellCount.y; y++)
                {
                    for (int x = 0; x < RUNTIME_DATA.Evacuation.CellCount.x; x++)
                    {
                        peopleInCells[outputIndex][x + y * RUNTIME_DATA.Evacuation.CellCount.x] = ((MacroHouseholdSim)SIM.PedestrianModule).GetPeopleLeftInCell(x, y);
                    }
                }

                //create texture
                Vector2Int res = new Vector2Int(2, 2);
                while (RUNTIME_DATA.Evacuation.CellCount.x > res.x)
                {
                    res.x *= 2;
                }
                while (RUNTIME_DATA.Evacuation.CellCount.y > res.y)
                {
                    res.y *= 2;
                }

                Texture2D tex = new Texture2D(res.x, res.y);
                tex.filterMode = FilterMode.Point;

                for (int y = 0; y < RUNTIME_DATA.Evacuation.CellCount.y; ++y)
                {
                    for (int x = 0; x < RUNTIME_DATA.Evacuation.CellCount.x; ++x)
                    {
                        WUInityColor c = WUInityColor.grey;
                        c.a = 0.0f;
                        int count = 0;
                        if (trafficDensityData[outputIndex][x + y * RUNTIME_DATA.Evacuation.CellCount.x] != null)
                        {
                            count += trafficDensityData[outputIndex][x + y * RUNTIME_DATA.Evacuation.CellCount.x].carCount;
                        }
                        //count += peopleInCells[outputIndex][x + y * WUInity.SIM.GetCellCount.x];
                        if (count > 0)
                        {
                            c = GetTrafficDensityColor(count);
                            c.a = 0.5f;
                        }
                        tex.SetPixel(x, y, c.UnityColor);
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