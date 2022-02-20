using System.Collections;               //Namespace to enable various standard class types (arraylist, stack, Hashtable, etc) and Interfaces (IEnumerable etc.)
using System.Collections.Generic;       //Similar as above but allows for more strongly typed collections (??)
using UnityEngine;                      //Just unity
using OsmSharp.Streams;                 //OpenStreetMap data stream?
using WUInity.Evac;                     //Pedestrian evacuation Part
using WUInity.Traffic;                  //Traffic evacuation part
using WUInity.GPW;                      //Gridded Population of the World data
using System;                           //general System Namespace
using System.IO;                        //general IO Namespace
using Mapbox.Utils;                     //Navigation and map data API and SDK
using Mapbox.Unity.Utilities;           //Similar as above but ported in Unity?


namespace WUInity
{    
    [RequireComponent(typeof(WUInityGUI))]                          //however this part is not in a class or method so idk what is going on here. 
    public class WUInity : MonoBehaviour                            //Declare the WUINITY public class (meaning other namespaces and classes can instantiate it / use it) and have it inherit the methods & variables of the MonoBehaviour class (Unity's Base class)
    {                           //
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

        public static WUInitySim SIM
        {
            get
            {
                if (INSTANCE.internal_sim == null)
                {
                    INSTANCE.internal_sim= new WUInitySim();
                }
                return INSTANCE.internal_sim;
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

        public static PopulationViewer GPW_VIEWER
        {
            get
            {
                if (INSTANCE.internal_gpwViewer == null)
                {
                    INSTANCE.internal_gpwViewer = new PopulationViewer();
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

        public static WUInityPainter PAINTER
        {
            get
            {
                if(INSTANCE.internal_painter == null)
                {
                    GameObject g = new GameObject();
                    g.transform.parent = INSTANCE.transform;
                    g.name = "WUI Painter";
                    INSTANCE.internal_painter = g.AddComponent<WUInityPainter>();
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

        //never directly call these, always use singletons (except once when setting input)
        private static WUInity internal_instance;
        private WUInityInput internal_input;
        private WUInitySim internal_sim;
        private WUInityOutput internal_output;
        private Farsite.FarsiteViewer internal_farsiteViewer;
        private PopulationViewer internal_gpwViewer;
        private WUInityGUI internal_wuiGUI;
        private WUInityPainter internal_painter;
        private Mapbox.Unity.Map.AbstractMap internal_mapboxMap;

        public bool haveInput = false;
        string internal_workingFilePath;
        List<GameObject> drawnRoads;     
        public enum DataSampleMode { None, GPW, Fitted, Relocated, Staying, TrafficDens, Paint, Farsite}
        public DataSampleMode dataSampleMode = DataSampleMode.None;
        GameObject[] goalMarkers;
        private GameObject evacDataPlane;
        private GameObject fireDataPlane;
        GameObject directionsGO;

        private struct CachedValidData
        {
            public Vector2D lowerLeftLatLong;
            public Vector2D size;
            public float routeCellSize;
            public float osmBorderSize;

            public CachedValidData(WUInityInput input)
            {
                lowerLeftLatLong = input.lowerLeftLatLong;
                size = input.size;
                routeCellSize = input.evac.routeCellSize;
                osmBorderSize = input.itinero.osmBorderSize;
            }
        }
        CachedValidData validInput;
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

            //UnityEngine.Random.InitState(0);
        }

        void Start()
        {                
            /*WUInityPERIL peril = new WUInityPERIL();
            string path = Application.dataPath + "/Resources/_input/k_PERIL/";
            peril.RunAllCases(5, 30, 30, 50, 50, WUInityPERIL.GetDefaultWUIArea(), 5f, path, path, path + "/peril_test.csv", path + "/peril_EPI.csv");*/
            //SaveLoadWUI.LoadDefaultInputs();             
        }

        /// <summary>
        /// Called when the user want to create a new file from scratch in the GUI.
        /// </summary>
        /// <param name="input"></param>
        public void NewInputData()
        {
            //SIM.ClearSimLog();
            INSTANCE.haveInput = true;
            this.internal_input = new WUInityInput();
            UpdateValidData();

            LoadMapbox();
            SpawnMarkers();
            godCamera.SetCameraStartPosition(INPUT.size);

            EvacGroup.LoadEvacGroupIndices();
            GraphicalFireInput.LoadGraphicalFireInput();
        }

        public bool gpwLoaded = false, routerDbLoaded = false;
        /// <summary>
        /// Load an existing file and try to validate all of the associated data.
        /// If data is valid it is also loaded.
        /// </summary>
        /// <param name="input"></param>
        public void LoadInputData(WUInityInput input)
        {
            //SIM.ClearSimLog();
            INSTANCE.haveInput = true;
            this.internal_input = input;
            UpdateValidData();

            LoadMapbox();
            SpawnMarkers();
            godCamera.SetCameraStartPosition(INPUT.size);

            EvacGroup.LoadEvacGroupIndices();
            GraphicalFireInput.LoadGraphicalFireInput();
            gpwLoaded = LoadGPW(false);
            routerDbLoaded = SIM.LoadItineroDatabase();            
        }

        bool coordinatesAreDirty = true;
        bool sizeIsDirty = true;
        bool cellSizeIsDirty = true;
        bool osmBorderIsDirty = true;
        /// <summary>
        /// Checks to see if loaded data is suitable after changes to input;
        /// </summary>
        public void CompareValidData()
        {
            coordinatesAreDirty = true;
            sizeIsDirty = true;
            cellSizeIsDirty = true;
            osmBorderIsDirty = true;

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

            if(validInput.routeCellSize == INPUT.evac.routeCellSize)
            {
                cellSizeIsDirty = false;
            }

            if (validInput.osmBorderSize == INPUT.itinero.osmBorderSize)
            {
                osmBorderIsDirty = false;
            }
        }

        public void UpdateValidData()
        {
            validInput = new CachedValidData(INPUT);
        }

        public bool IsMapDirty()
        {
            bool isDirty = false;
            if (coordinatesAreDirty || sizeIsDirty)
            {
                isDirty = true;
            }

            return isDirty;
        }

        public bool IsGPWDirty()
        {
            bool isDirty = false;
            if (coordinatesAreDirty || sizeIsDirty)
            {
                isDirty = true;
            }

            return isDirty;
        }

        public bool IsAnythingDirty()
        {
            return true == IsMapDirty();
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

            SIM.LogMessage("LOG: Farsite loaded succesfully.");
        }

        public bool LoadMapbox()
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
            Vector2D mapDegrees = GPWData.SizeToDegrees(INPUT.lowerLeftLatLong, INPUT.size);
            int tilesX = (int)(mapDegrees.x / degreesPerTile) + 1;
            int tilesY = (int)(mapDegrees.y / (degreesPerTile * Math.Cos((Math.PI / 180.0) * INPUT.lowerLeftLatLong.x))) + 1;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.east = tilesX;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.north = tilesY;
            mOptions.placementOptions.placementType = Mapbox.Unity.Map.MapPlacementType.AtLocationCenter;
            mOptions.placementOptions.snapMapToZero = true;
            mOptions.scalingOptions.scalingType = Mapbox.Unity.Map.MapScalingType.WorldScale;

            if (!MAP.IsAccessTokenValid)
            {
                SIM.LogMessage("ERROR: Mapbox token not valid.");
                return false;
            }
            SIM.LogMessage("LOG: Starting to load Mapbox map.");
            MAP.Initialize(new Mapbox.Utils.Vector2d(INPUT.lowerLeftLatLong.x, INPUT.lowerLeftLatLong.y), INPUT.zoomLevel);
            SIM.LogMessage("LOG: Map loaded succesfully.");
            return true;
        }

        public void SetSampleMode(DataSampleMode sampleMode)
        {
            dataSampleMode = sampleMode;
        }

        public bool LoadGPW(bool setActive = true)
        {
            //get all population data set
            bool success = GPW_VIEWER.CreateGPW(INPUT.lowerLeftLatLong, INPUT.size, setActive);
            //gpwViewer.SetDensityMapVisibility(input.gpw.displayGPW);

            if(success)
            {
                SIM.LogMessage("LOG: GPW Data loaded succesfully.");
            }
            else
            {
                SIM.LogMessage("LOG: GPW data could not be loaded.");
            }
            gpwLoaded = success;
            return success;
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
                    int x = (int)(SIM.EvacCellCount.x * xNorm);

                    float yNorm = hitPoint.z / (float)INPUT.size.y;
                    //yNorm = Mathf.Clamp01(yNorm);
                    int y = (int)(SIM.EvacCellCount.y * yNorm);
                    GetCellInfo(hitPoint, x, y);
                }
            }

            UpdateBorders();

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
            if(!INSTANCE.haveInput)
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
                osmBorder.SetPosition(0, -Vector3.right * INPUT.itinero.osmBorderSize - Vector3.forward * INPUT.itinero.osmBorderSize + Vector3.up * 10f);
                osmBorder.SetPosition(1, osmBorder.GetPosition(0) + Vector3.right * ((float)INPUT.size.x + INPUT.itinero.osmBorderSize * 2f));
                osmBorder.SetPosition(2, osmBorder.GetPosition(1) + Vector3.forward * ((float)INPUT.size.y + INPUT.itinero.osmBorderSize * 2f));
                osmBorder.SetPosition(3, osmBorder.GetPosition(2) - Vector3.right * ((float)INPUT.size.x + INPUT.itinero.osmBorderSize * 2f));
                osmBorder.SetPosition(4, osmBorder.GetPosition(0));
            }
        }

        void GetCellInfo(Vector3 pos, int x, int y)
        {
            dataSampleString = "No data to sample.";
            if (dataSampleMode == DataSampleMode.GPW)
            {
                if (GPW_VIEWER.rawGPWData != null && GPW_VIEWER.rawGPWData.density != null && GPW_VIEWER.rawGPWData.density.Length > 0)
                {
                    if (GPW_VIEWER.gpwDensityMap.activeSelf)
                    {
                        float xCellSize = (float)(GPW_VIEWER.rawGPWData.realWorldSize.x / GPW_VIEWER.rawGPWData.dataSize.x);
                        float yCellSize = (float)(GPW_VIEWER.rawGPWData.realWorldSize.y / GPW_VIEWER.rawGPWData.dataSize.y);
                        double cellArea = xCellSize * yCellSize / (1000000d);
                        dataSampleString = "GPW people count: " + System.Convert.ToInt32(GPW_VIEWER.rawGPWData.GetDensityUnitySpace(new Vector2D(pos.x, pos.z)) * cellArea);
                    }
                    else
                    {
                        dataSampleString = "GPW data not visible, activate to sample data.";
                    }
                }
            }
            else if (x < 0 || x > SIM.EvacCellCount.x || y < 0 || y > SIM.EvacCellCount.y)
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
                if (dataSampleMode == DataSampleMode.Fitted)
                {
                    dataSampleString = "Interpolated people count: " + WUInity.GPW_VIEWER.GetCellPopulation(x, y);
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
                    dataSampleString = "Staying people count: " + OUTPUT.evac.stayingPopulation[x + y * WUInity.SIM.EvacCellCount.x];
                }
                else if (dataSampleMode == DataSampleMode.TrafficDens)
                {
                    int people = currentPeopleInCells[x + y * WUInity.SIM.EvacCellCount.x];
                    dataSampleString = "People: " + people;
                    if (currenttrafficDensityData != null && currenttrafficDensityData[x + y * WUInity.SIM.EvacCellCount.x] != null)
                    {
                        int peopleInCars = currenttrafficDensityData[x + y * WUInity.SIM.EvacCellCount.x].peopleCount;
                        int cars = currenttrafficDensityData[x + y * WUInity.SIM.EvacCellCount.x].carCount;

                        dataSampleString += " | People in cars: " + peopleInCars + " (Cars: " + cars + "). Total people " + (people + peopleInCars);
                    }
                }
            }
            else
            {
                dataSampleString = "Data not visible, toggle on to sample data.";
            }          
        }        

        public void SaveTexturesToDisk()
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
        }        

        public bool IsPainterActive()
        {
            if(!PAINTER.gameObject.activeSelf)
            {
                return false;
            }

            return true;
        }

        public void StartPainter(WUInityPainter.PaintMode paintMode)
        {
            PAINTER.gameObject.SetActive(true);
            PAINTER.SetPainterMode(paintMode);            
            if(paintMode == WUInityPainter.PaintMode.WUIArea)
            {
                DisplayWUIAreaMap();                
            }
            else if (paintMode == WUInityPainter.PaintMode.RandomIgnitionArea)
            {
                DisplayRandomIgnitionAreaMap();
            }
            else if (paintMode == WUInityPainter.PaintMode.InitialIgnition)
            {
                DisplayInitialIgnitionMap();
            }
            else if(paintMode == WUInityPainter.PaintMode.EvacGroup)
            {
                DisplayEvacGroupMap();
            }
            else if (paintMode == WUInityPainter.PaintMode.CustomGPW)
            {
                DisplayCustomGPWData();
            }
            dataSampleMode = DataSampleMode.Paint;
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

            PopulationViewer.CreateSimplePlane(mesh, width, length, 0.0f, offset, maxUV);

            Material mat = new Material(Shader.Find("Unlit/Transparent"));
            mat.mainTexture = tex;

            mR.material = mat;

            //move up one meter
            gO.transform.position += Vector3.up;

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
                trafficDensityData.Add(new TrafficCellData[WUInity.SIM.EvacCellCount.x * WUInity.SIM.EvacCellCount.y]);

                for (int i = 0; i < cars.Count; i++)
                {
                    float lati = cars[i].goingToCoord.Latitude;
                    float longi = cars[i].goingToCoord.Longitude;

                    Mapbox.Utils.Vector2d pos = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(lati, longi, MAP.CenterMercator, MAP.WorldRelativeScale);

                    int x = (int)(pos.x / INPUT.evac.routeCellSize);
                    int y = (int)(pos.y / INPUT.evac.routeCellSize);

                    //outside of mapped data
                    if (x < 0 || x > WUInity.SIM.EvacCellCount.x - 1 || y < 0 || y > WUInity.SIM.EvacCellCount.y - 1)
                    {
                        continue;
                    }

                    //add or update data
                    if (trafficDensityData[outputIndex][x + y * WUInity.SIM.EvacCellCount.x] == null)
                    {
                        trafficDensityData[outputIndex][x + y * WUInity.SIM.EvacCellCount.x] = new TrafficCellData();
                        trafficDensityData[outputIndex][x + y * WUInity.SIM.EvacCellCount.x].carCount = 1;
                        trafficDensityData[outputIndex][x + y * WUInity.SIM.EvacCellCount.x].peopleCount = cars[i].numberOfPeopleInCar;
                    }
                    else
                    {
                        trafficDensityData[outputIndex][x + y * WUInity.SIM.EvacCellCount.x].carCount += 1;
                        trafficDensityData[outputIndex][x + y * WUInity.SIM.EvacCellCount.x].peopleCount += cars[i].numberOfPeopleInCar;
                    }
                }

                //save data from human re as well
                peopleInCells.Add(new int[WUInity.SIM.EvacCellCount.x * WUInity.SIM.EvacCellCount.y]);
                for (int y = 0; y < WUInity.SIM.EvacCellCount.y; y++)
                {
                    for (int x = 0; x < WUInity.SIM.EvacCellCount.x; x++)
                    {
                        peopleInCells[outputIndex][x + y * WUInity.SIM.EvacCellCount.x] = SIM.GetMacroHumanSim().GetPeopleLeftInCell(x, y);
                    }
                }

                //create texture
                Vector2Int res = new Vector2Int(2, 2);
                while (WUInity.SIM.EvacCellCount.x > res.x)
                {
                    res.x *= 2;
                }
                while (WUInity.SIM.EvacCellCount.y > res.y)
                {
                    res.y *= 2;
                }

                Texture2D tex = new Texture2D(res.x, res.y);
                tex.filterMode = FilterMode.Point;

                for (int y = 0; y < WUInity.SIM.EvacCellCount.y; ++y)
                {
                    for (int x = 0; x < WUInity.SIM.EvacCellCount.x; ++x)
                    {
                        Color c = Color.grey;
                        c.a = 0.0f;
                        int count = 0;
                        if (trafficDensityData[outputIndex][x + y * WUInity.SIM.EvacCellCount.x] != null)
                        {
                            count += trafficDensityData[outputIndex][x + y * WUInity.SIM.EvacCellCount.x].carCount;
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

        MeshRenderer evacDataPlaneMeshRenderer;
        MeshRenderer fireDataPlaneMeshRenderer;
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

        public void DisplayFittedPopulation()
        {
            SetDataPlaneTexture(GPW_VIEWER.GetFittedTexture());
        }

        public void DisplayStayingPop()
        {
            SetDataPlaneTexture(OUTPUT.evac.popStayingTexture);
        }

        public void DisplayRelocatedPop()
        {
            SetDataPlaneTexture(OUTPUT.evac.relocatedPopTexture);
        }

        public void DisplayStuckPop()
        {
            SetDataPlaneTexture(OUTPUT.evac.popStuckTexture);
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

        public void DisplayEvacGroupMap()
        {
            SetDataPlaneTexture(PAINTER.GetEvacGroupTexture());
        }

        public void DisplayCustomGPWData()
        {
            SetDataPlaneTexture(PAINTER.GetCustomGPWTexture());
        }

        public void ToggleDataPlane()
        {
            if (evacDataPlaneMeshRenderer != null)
            {
                evacDataPlaneMeshRenderer.gameObject.SetActive(!evacDataPlaneMeshRenderer.gameObject.activeSelf);
            }
        }

        private void SetDataPlaneTexture(Texture2D tex, bool fireMeshMode = false)
        {
            //turn everything off first
            if(fireDataPlane != null)
            {
                fireDataPlane.SetActive(false);
            }
            if (evacDataPlane != null)
            {
                evacDataPlane.SetActive(false);
            }

            //pick needed data plane
            MeshRenderer activeMeshRenderer = evacDataPlaneMeshRenderer;
            Vector2Int cellCount = SIM.EvacCellCount;
            string name = "Evac Data Plane";
            GameObject activeDataPlane = evacDataPlane;
            if (fireMeshMode)
            {
                activeMeshRenderer = fireDataPlaneMeshRenderer;
                cellCount = SIM.GetFireMesh().cellCount;
                name = "Fire Data Plane";
                activeDataPlane = fireDataPlane;
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
                activeDataPlane.SetActive(true);
            }
        }

        Color GetTrafficDensityColor(int cars)
        {
            float fraction = Mathf.Lerp(0f, 1f, cars / 20f);
            Color c = Color.HSVToRGB(0.67f - 0.67f * fraction, 1.0f, 1.0f);

            return c;
        }
    }
}

