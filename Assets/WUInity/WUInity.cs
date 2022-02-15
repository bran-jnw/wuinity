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
    [RequireComponent(typeof(Farsite.FarsiteViewer))]               //Apparently this makes sure that when an object is made, unity is absolutely sure that the following types are applied.
    [RequireComponent(typeof(WUInityGUI))]                          //however this part is not in a class or method so idk what is going on here. 
    [RequireComponent(typeof(GPWViewer))]
    public class WUInity : MonoBehaviour                            //Declare the WUINITY public class (meaning other namespaces and classes can instantiate it / use it) and have it inherit the methods & variables of the MonoBehaviour class (Unity's Base class)
    {
        private static WUInity wuinity_internal;                    //
        public static WUInity WUINITY
        {
            get
            {
                return wuinity_internal;
            }
        }

        public static WUInitySim WUINITY_SIM
        {
            get
            {
                return WUINITY.sim;
            }
        }

        public static WUInityGUI WUINITY_GUI
        {
            get
            {
                return WUINITY.wuiGUI;
            }
        }

        public static WUInityInput WUINITY_IN
        {
            get
            {
                return WUINITY.input;
            }
        }

        public static WUInityOutput WUINITY_OUT
        {
            get
            {
                return WUINITY.output;
            }
        }

        public static Farsite.FarsiteViewer WUINITY_FARSITE
        {
            get
            {
                return WUINITY.farsiteViewer;
            }
        }

        public static GPWViewer WUINITY_GPW
        {
            get
            {
                return WUINITY.gpwViewer;
            }
        }

        public static Mapbox.Unity.Map.AbstractMap WUINITY_MAP
        {
            get
            {
                return WUINITY.mapboxMap;
            }
        }

        public static WUInityPainter PAINTER
        {
            get
            {
                return WUINITY.painter;
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
                return WUINITY.workingFilePath;
            }
            set
            {
                WUINITY.workingFilePath = value;
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
        [SerializeField] public bool developerMode = false;
        [SerializeField] private WUInityInput input = new WUInityInput();
        [SerializeField] private WUInitySim sim = new WUInitySim();
        [SerializeField] private WUInityOutput output = new WUInityOutput();     
        
        [Header("Prefabs")]
        [SerializeField] GameObject markerPrefab;
        [SerializeField] Material dataPlaneMaterial;

        [Header("References")]        
        [SerializeField] public MeshFilter terrainMeshFilter;
        [SerializeField] public Material fireMaterial;
        [SerializeField] private Mapbox.Unity.Map.AbstractMap mapboxMap;
        [SerializeField] private GodCamera godCamera;
        [SerializeField] private LineRenderer simBorder;
        [SerializeField] private LineRenderer osmBorder;
        [SerializeField] [HideInInspector] private WUInityGUI wuiGUI;
        [SerializeField] [HideInInspector] private Farsite.FarsiteViewer farsiteViewer;
        [SerializeField] [HideInInspector] private GPWViewer gpwViewer;
        [SerializeField][HideInInspector] private GameObject evacDataPlane;
        [SerializeField] [HideInInspector] private GameObject fireDataPlane;
        [SerializeField] [HideInInspector] private WUInityPainter painter;        

        List<GameObject> drawnRoads;
        string workingFilePath;
        public bool haveInput = false;

        public enum DataSampleMode { None, GPW, Raw, Relocated, Staying, TrafficDens, Paint, Farsite}
        [SerializeField] public DataSampleMode dataSampleMode = DataSampleMode.None;
        GameObject[] goalMarkers;
        private void OnValidate()
        {      
            if(wuiGUI == null)
            {
                wuiGUI = GetComponent<WUInityGUI>();
            }
            wuiGUI.hideFlags = HideFlags.NotEditable;

            if(farsiteViewer == null)
            {
                farsiteViewer = GetComponent<Farsite.FarsiteViewer>();
            }
            farsiteViewer.hideFlags = HideFlags.None;

            if (gpwViewer == null)
            {
                gpwViewer = GetComponent<GPWViewer>();
            }            
            gpwViewer.hideFlags = HideFlags.NotEditable;
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

            simBorder.gameObject.SetActive(false);
            osmBorder.gameObject.SetActive(false);

            //set all singletons
            if (wuinity_internal == null)
            {
                wuinity_internal = this;
            }
            else if (wuinity_internal != this)
            {
                Destroy(wuinity_internal);
                wuinity_internal = this;
            }

            if (mapboxMap == null)
            {
                FindObjectOfType<Mapbox.Unity.Map.AbstractMap>();
            }

            if (godCamera == null)
            {
                godCamera = FindObjectOfType<GodCamera>();
            }
            godCamera.SetCameraStartPosition(input.size);

            //UnityEngine.Random.InitState(0);
        }

        void Start()
        {                
            /*WUInityPERIL peril = new WUInityPERIL();
            string path = Application.dataPath + "/Resources/_input/k_PERIL/";
            peril.RunAllCases(5, 30, 30, 50, 50, WUInityPERIL.GetDefaultWUIArea(), 5f, path, path, path + "/peril_test.csv", path + "/peril_EPI.csv");*/
            //SaveLoadWUI.LoadDefaultInputs();             
        }

        public void LoadInputData(WUInityInput input)
        {
            WUINITY.haveInput = true;
            this.input = input;
            LoadMapbox();
            SpawnMarkers();
            EvacGroup.LoadEvacGroupIndices();
            GraphicalFireInput.LoadGraphicalFireInput();
        }

        GameObject directionsGO;
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

        /*Vector2D[] ConvertMapboxVector(Vector2d[] array)
        {
            Vector2D[] r = new Vector2D[array.Length];
            for (int i = 0; i < r.Length; ++i)
            {
                r[i] = new Vector2D(array[i].x, array[i].y);
            }
            return r;
        }*/

        GameObject DrawRoute(RouteCollection rC, int index)
        {
            List<Vector3> dat = new List<Vector3>();
            foreach (Itinero.LocalGeo.Coordinate point in rC.GetSelectedRoute().route.Shape)
            {
                Vector3 v = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(point.Latitude, point.Longitude, mapboxMap.CenterMercator, mapboxMap.WorldRelativeScale).ToVector3xz();
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
            if (farsiteViewer == null)
            {
                farsiteViewer = FindObjectOfType<Farsite.FarsiteViewer>();
            }
            farsiteViewer.ImportFarsite();
            farsiteViewer.TransformCoordinates();

            wuiGUI.PrintInfo("Farsite loaded succesfully.");
        }

        public bool LoadMapbox()
        {
            if (mapboxMap == null)
            {
                mapboxMap = FindObjectOfType<Mapbox.Unity.Map.AbstractMap>();
            }
            /*else
            {
                for (int i = 0; i < mapboxMap.transform.childCount; i++)
                {
                    Destroy(mapboxMap.transform.GetChild(i).gameObject);
                }
            }*/

            //Mapbox: calculate the amount of grids needed based on zoom level, coord and size
            Mapbox.Unity.Map.MapOptions mOptions = mapboxMap.Options; // new Mapbox.Unity.Map.MapOptions();
            mOptions.locationOptions.latitudeLongitude = "" + input.lowerLeftLatLong.x + "," + input.lowerLeftLatLong.y;
            mOptions.locationOptions.zoom = input.zoomLevel;
            mOptions.extentOptions.extentType = Mapbox.Unity.Map.MapExtentType.RangeAroundCenter;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.west = 1;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.south = 1;
            //https://wiki.openstreetmap.org/wiki/Zoom_levels
            double tiles = Math.Pow(4.0, mOptions.locationOptions.zoom);
            double degreesPerTile = 360.0 / (Math.Pow(2.0, mOptions.locationOptions.zoom));
            Vector2D mapDegrees = GPWData.SizeToDegrees(input.lowerLeftLatLong, input.size);
            int tilesX = (int)(mapDegrees.x / degreesPerTile) + 1;
            int tilesY = (int)(mapDegrees.y / (degreesPerTile * Math.Cos((Math.PI / 180.0) * input.lowerLeftLatLong.x))) + 1;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.east = tilesX;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.north = tilesY;
            mOptions.placementOptions.placementType = Mapbox.Unity.Map.MapPlacementType.AtLocationCenter;
            mOptions.placementOptions.snapMapToZero = true;
            mOptions.scalingOptions.scalingType = Mapbox.Unity.Map.MapScalingType.WorldScale;

            if (!mapboxMap.IsAccessTokenValid)
            {
                WUINITY_SIM.LogMessage("ERROR: Mapbox token not valid.");
                return false;
            }
            WUINITY_SIM.LogMessage("LOG: Starting to load Mapbox map.");

            mapboxMap.Initialize(new Mapbox.Utils.Vector2d(input.lowerLeftLatLong.x, input.lowerLeftLatLong.y), input.zoomLevel);

            WUINITY_SIM.LogMessage("LOG: Map loaded succesfully.");
            return true;
        }

        public void SetSampleMode(DataSampleMode sampleMode)
        {
            dataSampleMode = sampleMode;
        }

        public void LoadGPW()
        {
            //get all population data set
            gpwViewer.CreateGPW(input.lowerLeftLatLong, input.size);
            //gpwViewer.SetDensityMapVisibility(input.gpw.displayGPW);

            wuiGUI.PrintInfo("GPW Data loaded succesfully.");
        }

        public void ToggleGPWViewer()
        {
            gpwViewer.ToggleDensityMapVisibility();
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
                    float xNorm = hitPoint.x / (float)input.size.x;
                    //xNorm = Mathf.Clamp01(xNorm);
                    int x = (int)(WUInity.WUINITY_SIM.EvacCellCount.x * xNorm);

                    float yNorm = hitPoint.z / (float)input.size.y;
                    //yNorm = Mathf.Clamp01(yNorm);
                    int y = (int)(WUInity.WUINITY_SIM.EvacCellCount.y * yNorm);
                    GetCellInfo(hitPoint, x, y);
                }
            }

            UpdateBorders();

            if(Input.GetKeyDown(KeyCode.Pause) && input.runInRealTime)
            {
                pauseSim = !pauseSim;
            }
            if(!pauseSim && input.runInRealTime && sim.simRunning)
            {
                sim.UpdateRealtimeSim();
            }
        }

        void UpdateBorders()
        {
            if(!WUINITY.haveInput)
            {
                return;
            }

            simBorder.gameObject.SetActive(true);
            osmBorder.gameObject.SetActive(true);

            Vector3 upOffset = Vector3.up * 50f;
            if (simBorder != null)
            {
                simBorder.SetPosition(0, Vector3.zero + upOffset);
                simBorder.SetPosition(1, simBorder.GetPosition(0) + Vector3.right * (float)input.size.x);
                simBorder.SetPosition(2, simBorder.GetPosition(1) + Vector3.forward * (float)input.size.y);
                simBorder.SetPosition(3, simBorder.GetPosition(2) - Vector3.right * (float)input.size.x);
                simBorder.SetPosition(4, simBorder.GetPosition(0));
            }

            if (osmBorder != null)
            {
                osmBorder.SetPosition(0, -Vector3.right * input.itinero.osmBorderSize - Vector3.forward * input.itinero.osmBorderSize + Vector3.up * 10f);
                osmBorder.SetPosition(1, osmBorder.GetPosition(0) + Vector3.right * ((float)input.size.x + input.itinero.osmBorderSize * 2f));
                osmBorder.SetPosition(2, osmBorder.GetPosition(1) + Vector3.forward * ((float)input.size.y + input.itinero.osmBorderSize * 2f));
                osmBorder.SetPosition(3, osmBorder.GetPosition(2) - Vector3.right * ((float)input.size.x + input.itinero.osmBorderSize * 2f));
                osmBorder.SetPosition(4, osmBorder.GetPosition(0));
            }
        }

        void GetCellInfo(Vector3 pos, int x, int y)
        {
            string m = "";
            if (dataSampleMode == DataSampleMode.GPW)
            {
                if (gpwViewer != null && gpwViewer.gpwData != null && gpwViewer.gpwData.density != null && gpwViewer.gpwData.density.Length > 0)
                {
                    if (gpwViewer.gameObject.activeSelf)
                    {
                        float xCellSize = (float)(gpwViewer.gpwData.realWorldSize.x / gpwViewer.gpwData.dataSize.x);
                        float yCellSize = (float)(gpwViewer.gpwData.realWorldSize.y / gpwViewer.gpwData.dataSize.y);
                        double cellArea = xCellSize * yCellSize / (1000000d);
                        m = "GPW people count: " + System.Convert.ToInt32(gpwViewer.gpwData.GetDensityUnitySpace(new Vector2D(pos.x, pos.z)) * cellArea);
                    }
                    else
                    {
                        m = "GPW data not visible, activate to sample data.";
                    }
                }
            }
            else if (x < 0 || x > WUInity.WUINITY_SIM.EvacCellCount.x || y < 0 || y > WUInity.WUINITY_SIM.EvacCellCount.y)
            {
                wuiGUI.PrintInfo("Outside of data range.");
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
                if (dataSampleMode == DataSampleMode.Raw)
                {
                    m = "Interpolated people count: " + output.evac.rawPopulation[x + y * WUInity.WUINITY_SIM.EvacCellCount.x];
                }
                else if (dataSampleMode == DataSampleMode.Relocated)
                {
                    if (sim.GetMacroHumanSim() != null)
                    {
                        m = "Rescaled and relocated people count: " + sim.GetMacroHumanSim().GetPopulation(x, y);
                    }
                }
                else if (dataSampleMode == DataSampleMode.Staying)
                {
                    m = "Staying people count: " + output.evac.stayingPopulation[x + y * WUInity.WUINITY_SIM.EvacCellCount.x];
                }
                else if (dataSampleMode == DataSampleMode.TrafficDens)
                {
                    int people = currentPeopleInCells[x + y * WUInity.WUINITY_SIM.EvacCellCount.x];
                    m = "People: " + people;
                    if (currenttrafficDensityData != null && currenttrafficDensityData[x + y * WUInity.WUINITY_SIM.EvacCellCount.x] != null)
                    {
                        int peopleInCars = currenttrafficDensityData[x + y * WUInity.WUINITY_SIM.EvacCellCount.x].peopleCount;
                        int cars = currenttrafficDensityData[x + y * WUInity.WUINITY_SIM.EvacCellCount.x].carCount;

                        m += " | People in cars: " + peopleInCars + " (Cars: " + cars + "). Total people " + (people + peopleInCars);
                    }
                }
            }
            else
            {
                m = "Data not visible, toggle on to sample data.";
            }

            wuiGUI.PrintInfo(m);
        }        

        public void SaveTexturesToDisk()
        {
            // Encode texture into PNG
            byte[] bytes = output.evac.rawPopTexture.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(WUInity.OUTPUT_FOLDER, "rawPopulation.png"), bytes);
            bytes = output.evac.popStuckTexture.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(WUInity.OUTPUT_FOLDER, "stuckPopulation.png"), bytes);
            bytes = output.evac.relocatedPopTexture.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(WUInity.OUTPUT_FOLDER, "relocatedPopulation.png"), bytes);
            bytes = output.evac.popStayingTexture.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(WUInity.OUTPUT_FOLDER, "stayingPopulation.png"), bytes);
        }        

        public bool IsPainterActive()
        {
            if(painter == null || !painter.gameObject.activeSelf)
            {
                return false;
            }

            return true;
        }

        public void StartPainter(WUInityPainter.PaintMode paintMode)
        {
            if (painter == null)
            {                
                GameObject gO = new GameObject();
                gO.name = "EvacGoalPainter";
                painter = gO.AddComponent<WUInityPainter>();
            }

            painter.gameObject.SetActive(true);
            painter.SetPainterMode(paintMode);            
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
            dataSampleMode = DataSampleMode.Paint;
        }

        public void StopPainter()
        {
            if (painter != null)
            {
                painter.gameObject.SetActive(false);
            }
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

            float width = (float)input.size.x;
            float length = (float)input.size.y;


            Vector3 offset = Vector3.zero;

            Vector2 maxUV = new Vector2((float)cellCount.x / tex.width, (float)cellCount.y / tex.height);

            GPWViewer.CreateSimplePlane(mesh, width, length, 0.0f, offset, maxUV);

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

            goalMarkers = new GameObject[input.traffic.evacuationGoals.Length];
            for (int i = 0; i < input.traffic.evacuationGoals.Length; i++)
            {
                EvacuationGoal eG = input.traffic.evacuationGoals[i];
                goalMarkers[i] = Instantiate<GameObject>(markerPrefab);
                Mapbox.Utils.Vector2d pos = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(eG.latLong.x, eG.latLong.y, mapboxMap.CenterMercator, mapboxMap.WorldRelativeScale);

                float scale = 0.01f * (float)input.size.y;
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
            int outputIndex = (int)(time - WUINITY_SIM.StartTime) / (int)input.traffic.saveInterval;
            if (outputIndex > trafficDensityData.Count - 1)
            {
                trafficDensityData.Add(new TrafficCellData[WUInity.WUINITY_SIM.EvacCellCount.x * WUInity.WUINITY_SIM.EvacCellCount.y]);

                for (int i = 0; i < cars.Count; i++)
                {
                    float lati = cars[i].goingToCoord.Latitude;
                    float longi = cars[i].goingToCoord.Longitude;

                    Mapbox.Utils.Vector2d pos = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(lati, longi, mapboxMap.CenterMercator, mapboxMap.WorldRelativeScale);

                    int x = (int)(pos.x / input.evac.routeCellSize);
                    int y = (int)(pos.y / input.evac.routeCellSize);

                    //outside of mapped data
                    if (x < 0 || x > WUInity.WUINITY_SIM.EvacCellCount.x - 1 || y < 0 || y > WUInity.WUINITY_SIM.EvacCellCount.y - 1)
                    {
                        continue;
                    }

                    //add or update data
                    if (trafficDensityData[outputIndex][x + y * WUInity.WUINITY_SIM.EvacCellCount.x] == null)
                    {
                        trafficDensityData[outputIndex][x + y * WUInity.WUINITY_SIM.EvacCellCount.x] = new TrafficCellData();
                        trafficDensityData[outputIndex][x + y * WUInity.WUINITY_SIM.EvacCellCount.x].carCount = 1;
                        trafficDensityData[outputIndex][x + y * WUInity.WUINITY_SIM.EvacCellCount.x].peopleCount = cars[i].numberOfPeopleInCar;
                    }
                    else
                    {
                        trafficDensityData[outputIndex][x + y * WUInity.WUINITY_SIM.EvacCellCount.x].carCount += 1;
                        trafficDensityData[outputIndex][x + y * WUInity.WUINITY_SIM.EvacCellCount.x].peopleCount += cars[i].numberOfPeopleInCar;
                    }
                }

                //save data from human re as well
                peopleInCells.Add(new int[WUInity.WUINITY_SIM.EvacCellCount.x * WUInity.WUINITY_SIM.EvacCellCount.y]);
                for (int y = 0; y < WUInity.WUINITY_SIM.EvacCellCount.y; y++)
                {
                    for (int x = 0; x < WUInity.WUINITY_SIM.EvacCellCount.x; x++)
                    {
                        peopleInCells[outputIndex][x + y * WUInity.WUINITY_SIM.EvacCellCount.x] = sim.GetMacroHumanSim().GetPeopleLeftInCell(x, y);
                    }
                }

                //create texture
                Vector2Int res = new Vector2Int(2, 2);
                while (WUInity.WUINITY_SIM.EvacCellCount.x > res.x)
                {
                    res.x *= 2;
                }
                while (WUInity.WUINITY_SIM.EvacCellCount.y > res.y)
                {
                    res.y *= 2;
                }

                Texture2D tex = new Texture2D(res.x, res.y);
                tex.filterMode = FilterMode.Point;

                for (int y = 0; y < WUInity.WUINITY_SIM.EvacCellCount.y; ++y)
                {
                    for (int x = 0; x < WUInity.WUINITY_SIM.EvacCellCount.x; ++x)
                    {
                        Color c = Color.grey;
                        c.a = 0.0f;
                        int count = 0;
                        if (trafficDensityData[outputIndex][x + y * WUInity.WUINITY_SIM.EvacCellCount.x] != null)
                        {
                            count += trafficDensityData[outputIndex][x + y * WUInity.WUINITY_SIM.EvacCellCount.x].carCount;
                        }
                        //count += peopleInCells[outputIndex][x + y * WUInity.WUINITY_SIM.GetCellCount.x];
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
            if(input.runTrafficSim)
            {
                int index = Mathf.Max(0, (int)time / (int)input.traffic.saveInterval);
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

        public void DisplayRawPop()
        {
            SetDataPlaneTexture(output.evac.rawPopTexture);
        }

        public void DisplayStayingPop()
        {
            SetDataPlaneTexture(output.evac.popStayingTexture);
        }

        public void DisplayRelocatedPop()
        {
            SetDataPlaneTexture(output.evac.relocatedPopTexture);
        }

        public void DisplayStuckPop()
        {
            SetDataPlaneTexture(output.evac.popStuckTexture);
        }

        private void DisplayWUIAreaMap()
        {
            SetDataPlaneTexture(painter.GetWUIAreaTexture(), true);
        }

        public void DisplayRandomIgnitionAreaMap()
        {
            SetDataPlaneTexture(painter.GetRandomIgnitionTexture(), true);
        }

        public void DisplayInitialIgnitionMap()
        {
            SetDataPlaneTexture(painter.GetInitialIgnitionTexture(), true);
        }

        public void DisplayEvacGroupMap()
        {
            SetDataPlaneTexture(painter.GetEvacGroupTexture());
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
            Vector2Int cellCount = WUInity.WUINITY_SIM.EvacCellCount;
            string name = "Evac Data Plane";
            GameObject activeDataPlane = evacDataPlane;
            if (fireMeshMode)
            {
                activeMeshRenderer = fireDataPlaneMeshRenderer;
                cellCount = WUINITY_SIM.GetFireMesh().cellCount;
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

