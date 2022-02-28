using System.Collections;               
using System.Collections.Generic;       
using UnityEngine;                     
using OsmSharp.Streams;                 
using WUInity.Evac;                     
using WUInity.Traffic;                 
using WUInity.GPW;                     
using System;                           
using System.IO;                        
using Mapbox.Utils;                    
using Mapbox.Unity.Utilities;          


namespace WUInity
{    
    [RequireComponent(typeof(WUInityGUI))]                          
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
                
        public static SimulationData SIM_DATA
        {
            get
            {
                if (INSTANCE.internal_sim_data == null)
                {
                    INSTANCE.internal_sim_data = new SimulationData();
                }
                return INSTANCE.internal_sim_data;
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
                if (INSTANCE.internal_gpwViewer == null)
                {
                    INSTANCE.internal_gpwViewer = new PopulationManager();
                }
                return INSTANCE.internal_gpwViewer;
            }
        }

        public static Mapbox.Unity.Map.AbstractMap MAP
        {
            get
            {
                if(INSTANCE.internal_mapboxMap == null)
                {
                    INSTANCE.internal_mapboxMap = FindObjectOfType<Mapbox.Unity.Map.AbstractMap>();
                    if(INSTANCE.internal_mapboxMap == null)
                    {
                        GameObject g = new GameObject();
                        g.name = "Mapbox Map";
                        g.transform.parent = INSTANCE.transform;
                        INSTANCE.internal_mapboxMap = g.AddComponent<Mapbox.Unity.Map.AbstractMap>();
                    }
                }
                return INSTANCE.internal_mapboxMap;
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
            public bool haveInput;

            public bool mapLoaded;

            public bool populationLoaded;
            public bool populationCorrectedForRoutes;
            public bool globalGPWAvailable;
            public bool localGPWLoaded;            

            public bool routeCollectionLoaded;
            public bool routerDbLoaded;

            public bool osmFileValid;

            public bool lcpLoaded;

            public bool CanRunSimulation()
            {
                bool canRun = true;
                if(!mapLoaded)
                {
                    canRun = false;
                    WUI_LOG("ERROR: Map is not loaded.");
                }

                if(!populationLoaded && (!localGPWLoaded || !globalGPWAvailable))
                {
                    canRun = false;  
                    WUI_LOG("ERROR: Population is not loaded and no local nor global GPW file is found to build it from.");                  
                }

                if(!routerDbLoaded && !osmFileValid)
                {
                    canRun = false;
                    WUI_LOG("ERROR: No router database loaded and no valid OSM file was found to build it from.");
                }

                if(INPUT.runFireSim && !lcpLoaded)
                {
                    WUI_LOG("ERROR: No LCP file loaded but fire spread is activated.");
                }

                return canRun;
            }


            public void Reset()
            {
                //haveInput = false; //can never lose input after getting it once
                mapLoaded = false;

                populationLoaded = false;
                populationCorrectedForRoutes = false;
                globalGPWAvailable = false;
                localGPWLoaded = false;

                routeCollectionLoaded = false;
                routerDbLoaded = false;
                osmFileValid = false;

                lcpLoaded = false;
            }
        }

        [Header("Options")]
        public bool developerMode = false;        
        
        [Header("Prefabs")]
        [SerializeField] GameObject markerPrefab;
        [SerializeField] Material dataPlaneMaterial;

        [Header("References")]        
        [SerializeField] public MeshFilter terrainMeshFilter;
        [SerializeField] public Material fireMaterial;        
        [SerializeField] private GodCamera godCamera;
        [SerializeField] private LineRenderer simBorder;
        [SerializeField] private LineRenderer osmBorder;

        public enum DataSampleMode { None, GPW, Population, Relocated, Staying, TrafficDens, Paint, Farsite }
        public DataSampleMode dataSampleMode = DataSampleMode.None;

        //never directly call these, always use singletons (except once when setting input)
        private static WUInity internal_instance;
        private WUInityInput internal_input;
        private WUInityOutput internal_output;
        private Simulation internal_sim;
        SimulationData internal_sim_data;
        private Farsite.FarsiteViewer internal_farsiteViewer;
        private PopulationManager internal_gpwViewer;
        private WUInityGUI internal_wuiGUI;
        private Painter internal_painter;
        private Mapbox.Unity.Map.AbstractMap internal_mapboxMap;

        MeshRenderer evacDataPlaneMeshRenderer;
        MeshRenderer fireDataPlaneMeshRenderer;
        List<GameObject> drawnRoads;
        GameObject[] goalMarkers;
        private GameObject evacDataPlane;
        private GameObject fireDataPlane;
        GameObject directionsGO;

        string internal_workingFilePath;  
        private DataStatus internal_dataStatus;

        private struct ValidCriticalData
        {
            public Vector2D lowerLeftLatLong;
            public Vector2D size;
            public float routeCellSize;
            public float osmBorderSize;

            public ValidCriticalData(WUInityInput input)
            {
                lowerLeftLatLong = input.lowerLeftLatLong;
                size = input.size;
                routeCellSize = input.evac.routeCellSize;
                osmBorderSize = input.traffic.osmBorderSize;
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
                developerMode = true;
            }
            else
            {
                developerMode = false;
            }  

            if (godCamera == null)
            {
                godCamera = FindObjectOfType<GodCamera>();
            }            

            simBorder.gameObject.SetActive(false);
            osmBorder.gameObject.SetActive(false);
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
            DATA_STATUS.haveInput = true;
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

            //if they can't be found we get empty ones
            EvacGroup.LoadEvacGroupIndices();
            GraphicalFireInput.LoadGraphicalFireInput();

            UpdateFireResourceStatus();
            UpdatePopulationResourceStatus();
            UpdateRoutingResourceStatus();
            UpdateOSMResourceStatus();

            GUI.SetGUIDirty();
        }

        public void UpdateMapResourceStatus()
        {
            DATA_STATUS.mapLoaded = LoadMapbox();
            UpdateBorders();
            SpawnMarkers();
            godCamera.SetCameraStartPosition(INPUT.size);

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
                DATA_STATUS.mapLoaded = true;
            }

            //set cached data to be current data
            validInput.size = INPUT.size;
            validInput.lowerLeftLatLong = INPUT.lowerLeftLatLong;
        }

        public void UpdateEvacResourceStatus()
        {
            bool cellSizeIsDirty = true;
            bool osmBorderIsDirty = true;

            if (validInput.routeCellSize == INPUT.evac.routeCellSize)
            {
                cellSizeIsDirty = false;
            }

            if (validInput.osmBorderSize == INPUT.traffic.osmBorderSize)
            {
                osmBorderIsDirty = false;
            }
        }

        public void UpdatePopulationResourceStatus()
        {
            DATA_STATUS.routeCollectionLoaded = SIM_DATA.LoadRouteCollections();
            DATA_STATUS.populationLoaded = POPULATION.LoadPopulationFromFile();
            DATA_STATUS.populationCorrectedForRoutes = POPULATION.IsPopulationCorrectedForRoutes();
            DATA_STATUS.localGPWLoaded = POPULATION.LoadLocalGPWFromFile();
            DATA_STATUS.globalGPWAvailable = LocalGPWData.IsGPWAvailable();
        }

        public void UpdateFireResourceStatus()
        {
            DATA_STATUS.lcpLoaded = SIM_DATA.LoadLCPFile();
        }

        public void UpdateRoutingResourceStatus()
        {
            DATA_STATUS.routerDbLoaded = SIM_DATA.LoadRouterDatabase();
        }   

        public void UpdateOSMResourceStatus()
        {
            DATA_STATUS.osmFileValid = File.Exists(INPUT.traffic.osmFile);
        }

        private bool LoadMapbox()
        {
            //Mapbox: calculate the amount of grids needed based on zoom level, coord and size
            Mapbox.Unity.Map.MapOptions mOptions = MAP.Options; // new Mapbox.Unity.Map.MapOptions();
            mOptions.locationOptions.latitudeLongitude = "" + INPUT.lowerLeftLatLong.x + "," + INPUT.lowerLeftLatLong.y;
            mOptions.locationOptions.zoom = INPUT.zoomLevel;
            mOptions.extentOptions.extentType = Mapbox.Unity.Map.MapExtentType.RangeAroundCenter;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.west = 1;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.south = 1;
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
                WUI_LOG("ERROR: Mapbox token not valid.");
                return false;
            }
            WUI_LOG("LOG: Starting to load Mapbox map.");
            MAP.Initialize(new Mapbox.Utils.Vector2d(INPUT.lowerLeftLatLong.x, INPUT.lowerLeftLatLong.y), INPUT.zoomLevel);
            WUI_LOG("LOG: Map loaded succesfully.");
            return true;
        }

        private List<string> simLog = new List<string>();
        /// <summary>
        /// Receives all the information from a WUINITY session, used by GUI.
        /// </summary>
        /// <param name="message"></param>
        public static void WUI_LOG(string message)
        {
            if (SIM.simRunning)
            {
                message = "[" + (int)SIM.time + "s] " + message;
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

        public void LoadFarsite()
        {
            FARSITE_VIEWER.ImportFarsite();
            FARSITE_VIEWER.TransformCoordinates();

            WUI_LOG("LOG: Farsite loaded succesfully.");
        }               

        public void SetSampleMode(DataSampleMode sampleMode)
        {
            dataSampleMode = sampleMode;
        }

        bool pauseSim = false;
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
                    int x = (int)(SIM_DATA.EvacCellCount.x * xNorm);

                    float yNorm = hitPoint.z / (float)INPUT.size.y;
                    //yNorm = Mathf.Clamp01(yNorm);
                    int y = (int)(SIM_DATA.EvacCellCount.y * yNorm);
                    GetCellInfo(hitPoint, x, y);
                }
            }           

            if(Input.GetKeyDown(KeyCode.Pause) && INPUT.runInRealTime)
            {
                pauseSim = !pauseSim;
            }
            if(!pauseSim && INPUT.runInRealTime && SIM.simRunning)
            {
                SIM.UpdateRealtimeSim();
            }
        }

        void UpdateBorders()
        {
            if(!internal_dataStatus.haveInput)
            {
                return;
            }

            simBorder.gameObject.SetActive(true);
            osmBorder.gameObject.SetActive(true);

            Vector3 upOffset = Vector3.up * 50f;
            if (simBorder != null)
            {
                simBorder.SetPosition(0, Vector3.zero + upOffset);
                simBorder.SetPosition(1, simBorder.GetPosition(0) + Vector3.right * (float)INPUT.size.x);
                simBorder.SetPosition(2, simBorder.GetPosition(1) + Vector3.forward * (float)INPUT.size.y);
                simBorder.SetPosition(3, simBorder.GetPosition(2) - Vector3.right * (float)INPUT.size.x);
                simBorder.SetPosition(4, simBorder.GetPosition(0));
            }

            if (osmBorder != null)
            {
                osmBorder.SetPosition(0, -Vector3.right * INPUT.traffic.osmBorderSize - Vector3.forward * INPUT.traffic.osmBorderSize + Vector3.up * 10f);
                osmBorder.SetPosition(1, osmBorder.GetPosition(0) + Vector3.right * ((float)INPUT.size.x + INPUT.traffic.osmBorderSize * 2f));
                osmBorder.SetPosition(2, osmBorder.GetPosition(1) + Vector3.forward * ((float)INPUT.size.y + INPUT.traffic.osmBorderSize * 2f));
                osmBorder.SetPosition(3, osmBorder.GetPosition(2) - Vector3.right * ((float)INPUT.size.x + INPUT.traffic.osmBorderSize * 2f));
                osmBorder.SetPosition(4, osmBorder.GetPosition(0));
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
            else if (x < 0 || x > SIM_DATA.EvacCellCount.x || y < 0 || y > SIM_DATA.EvacCellCount.y)
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
                    if (SIM.GetMacroHumanSim() != null)
                    {
                        dataSampleString = "Rescaled and relocated people count: " + SIM.GetMacroHumanSim().GetPopulation(x, y);
                    }
                }
                else if (dataSampleMode == DataSampleMode.Staying)
                {
                    dataSampleString = "Staying people count: " + OUTPUT.evac.stayingPopulation[x + y * SIM_DATA.EvacCellCount.x];
                }
                else if (dataSampleMode == DataSampleMode.TrafficDens)
                {
                    int people = currentPeopleInCells[x + y * SIM_DATA.EvacCellCount.x];
                    dataSampleString = "People: " + people;
                    if (currenttrafficDensityData != null && currenttrafficDensityData[x + y * SIM_DATA.EvacCellCount.x] != null)
                    {
                        int peopleInCars = currenttrafficDensityData[x + y * SIM_DATA.EvacCellCount.x].peopleCount;
                        int cars = currenttrafficDensityData[x + y * SIM_DATA.EvacCellCount.x].carCount;

                        dataSampleString += " | People in cars: " + peopleInCars + " (Cars: " + cars + "). Total people " + (people + peopleInCars);
                    }
                }
            }
            else
            {
                dataSampleString = "Data not visible, toggle on to sample data.";
            }          
        }        

        /*public void SaveTexturesToDisk()
        {
            // Encode texture into PNG
            byte[] bytes = OUTPUT.evac.rawPopTexture.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(WUInity.OUTPUT_FOLDER, "rawPopulation.png"), bytes);
            bytes = OUTPUT.evac.popStuckTexture.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(WUInity.OUTPUT_FOLDER, "stuckPopulation.png"), bytes);
            bytes = OUTPUT.evac.relocatedPopTexture.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(WUInity.OUTPUT_FOLDER, "relocatedPopulation.png"), bytes);
            bytes = OUTPUT.evac.popStayingTexture.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(WUInity.OUTPUT_FOLDER, "stayingPopulation.png"), bytes);
        }*/        

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
                WUI_LOG("ERROR: Paint mode not set correctly");
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

            goalMarkers = new GameObject[INPUT.traffic.evacuationGoals.Length];
            for (int i = 0; i < INPUT.traffic.evacuationGoals.Length; i++)
            {
                EvacuationGoal eG = INPUT.traffic.evacuationGoals[i];
                goalMarkers[i] = Instantiate<GameObject>(markerPrefab);
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

        public void DisplayPopulation()
        {
            SetDataPlaneTexture(POPULATION.GetPopulationTexture());
        }

        public void DisplayStayingPop()
        {
            SetDataPlaneTexture(OUTPUT.evac.popStayingTexture);
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

        public bool ToggleFireDataPlane()
        {
            if (fireDataPlaneMeshRenderer != null)
            {
                fireDataPlaneMeshRenderer.gameObject.SetActive(!fireDataPlaneMeshRenderer.gameObject.activeSelf);
                return fireDataPlaneMeshRenderer.gameObject.activeSelf;
            }

            return false;
        }

        private void SetDataPlaneTexture(Texture2D tex, bool fireMeshMode = false)
        {
            //pick needed data plane
            MeshRenderer activeMeshRenderer = evacDataPlaneMeshRenderer;
            Vector2Int cellCount = SIM_DATA.EvacCellCount;
            string name = "Evac Data Plane";
            if (fireMeshMode)
            {
                activeMeshRenderer = fireDataPlaneMeshRenderer;
                cellCount = SIM.GetFireMesh().cellCount;
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
        public void SaveTransientDensityData(float time, List<MacroCar> cars)
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
                trafficDensityData.Add(new TrafficCellData[SIM_DATA.EvacCellCount.x * SIM_DATA.EvacCellCount.y]);

                for (int i = 0; i < cars.Count; i++)
                {
                    float lati = cars[i].goingToCoord.Latitude;
                    float longi = cars[i].goingToCoord.Longitude;

                    Mapbox.Utils.Vector2d pos = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(lati, longi, MAP.CenterMercator, MAP.WorldRelativeScale);

                    int x = (int)(pos.x / INPUT.evac.routeCellSize);
                    int y = (int)(pos.y / INPUT.evac.routeCellSize);

                    //outside of mapped data
                    if (x < 0 || x > SIM_DATA.EvacCellCount.x - 1 || y < 0 || y > SIM_DATA.EvacCellCount.y - 1)
                    {
                        continue;
                    }

                    //add or update data
                    if (trafficDensityData[outputIndex][x + y * SIM_DATA.EvacCellCount.x] == null)
                    {
                        trafficDensityData[outputIndex][x + y * SIM_DATA.EvacCellCount.x] = new TrafficCellData();
                        trafficDensityData[outputIndex][x + y * SIM_DATA.EvacCellCount.x].carCount = 1;
                        trafficDensityData[outputIndex][x + y * SIM_DATA.EvacCellCount.x].peopleCount = cars[i].numberOfPeopleInCar;
                    }
                    else
                    {
                        trafficDensityData[outputIndex][x + y * SIM_DATA.EvacCellCount.x].carCount += 1;
                        trafficDensityData[outputIndex][x + y * SIM_DATA.EvacCellCount.x].peopleCount += cars[i].numberOfPeopleInCar;
                    }
                }

                //save data from human re as well
                peopleInCells.Add(new int[SIM_DATA.EvacCellCount.x * SIM_DATA.EvacCellCount.y]);
                for (int y = 0; y < SIM_DATA.EvacCellCount.y; y++)
                {
                    for (int x = 0; x < SIM_DATA.EvacCellCount.x; x++)
                    {
                        peopleInCells[outputIndex][x + y * SIM_DATA.EvacCellCount.x] = SIM.GetMacroHumanSim().GetPeopleLeftInCell(x, y);
                    }
                }

                //create texture
                Vector2Int res = new Vector2Int(2, 2);
                while (SIM_DATA.EvacCellCount.x > res.x)
                {
                    res.x *= 2;
                }
                while (SIM_DATA.EvacCellCount.y > res.y)
                {
                    res.y *= 2;
                }

                Texture2D tex = new Texture2D(res.x, res.y);
                tex.filterMode = FilterMode.Point;

                for (int y = 0; y < SIM_DATA.EvacCellCount.y; ++y)
                {
                    for (int x = 0; x < SIM_DATA.EvacCellCount.x; ++x)
                    {
                        Color c = Color.grey;
                        c.a = 0.0f;
                        int count = 0;
                        if (trafficDensityData[outputIndex][x + y * SIM_DATA.EvacCellCount.x] != null)
                        {
                            count += trafficDensityData[outputIndex][x + y * SIM_DATA.EvacCellCount.x].carCount;
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