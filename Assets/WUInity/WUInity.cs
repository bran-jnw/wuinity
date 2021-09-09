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
    [RequireComponent(typeof(Farsite.FarsiteViewer))]
    [RequireComponent(typeof(WUInityGUI))]
    [RequireComponent(typeof(GPWViewer))]
    public class WUInity : MonoBehaviour
    {
        private static WUInity wuinity_internal;
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

        [Header("Options")]
        [SerializeField] private bool editorMode = false;
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
        [SerializeField][HideInInspector] private GameObject dataPlane;

        List<GameObject> drawnRoads;

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
            //clicks all buttons for us
            if (editorMode)
            {
                InitEditorMode();
            }

            /*WUInityPERIL peril = new WUInityPERIL();
            string path = Application.dataPath + "/Resources/_input/k_PERIL/";
            peril.RunAllCases(5, 30, 30, 50, 50, WUInityPERIL.GetDefaultWUIArea(), 5f, path, path, path + "/peril_test.csv", path + "/peril_EPI.csv");*/
            SaveWUI.LoadDefaultInputs();
        }

        public void LoadInputData(WUInityInput input)
        {
            this.input = input;
            LoadMapbox();
            SpawnMarkers();            
        }

        public void InitEditorMode()
        {
            input.evac.responseCurve = ResponseCurve.GetRoxburoughCurve(); //WUInity.ResponseData.GetStandardCurve(); //
            LoadMapbox();
            LoadGPW();
            gpwViewer.ToggleDensityMapVisibility();
            LoadFarsite();
            farsiteViewer.ToggleTerrain();
            sim.LoadItineroDatabase();
            if (input.traffic.routeChoice == TrafficInput.RouteChoice.ForceMap)
            {
                StartPainter(EvacuationPainter.PaintMode.ForceGoal);
            }
        }

        private void SetRoxburough()
        {
            input.simName = "rox";
            input.lowerLeftLatLong = new Vector2D(39.409924, -105.104505);
            input.size = new Vector2D(5000, 10000);
            input.zoomLevel = 13;
            input.itinero.osmDataName = "colorado-latest";
            input.itinero.routerDatabaseName = "colorado-latest";
            input.gpw.localGPWFilename = "roxburough.gpw";
            input.evac.routeCellSize = 200.0f;
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

        public void LoadMapbox()
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
            mapboxMap.Initialize(new Mapbox.Utils.Vector2d(input.lowerLeftLatLong.x, input.lowerLeftLatLong.y), input.zoomLevel);

            WUINITY_SIM.LogMessage("Map loaded succesfully.");
        }

        public void SetSampleMode(DataSampleMode sampleMode)
        {
            dataSampleMode = sampleMode;
        }

        public void LoadGPW()
        {
            //get all population data set
            gpwViewer.CreateGPW(input.lowerLeftLatLong, input.size, input.gpw.localGPWFilename);
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
                    int x = (int)(input.evac.routeCellCount.x * xNorm);

                    float yNorm = hitPoint.z / (float)input.size.y;
                    //yNorm = Mathf.Clamp01(yNorm);
                    int y = (int)(input.evac.routeCellCount.y * yNorm);
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
            else if (x < 0 || x > input.evac.routeCellCount.x || y < 0 || y > input.evac.routeCellCount.y)
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
            else if (dataPlane != null && dataPlane.activeSelf)
            {
                if (dataSampleMode == DataSampleMode.Raw)
                {
                    m = "Interpolated people count: " + output.evac.rawPopulation[x + y * input.evac.routeCellCount.x];
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
                    m = "Staying people count: " + output.evac.stayingPopulation[x + y * input.evac.routeCellCount.x];
                }
                else if (dataSampleMode == DataSampleMode.TrafficDens)
                {
                    int people = currentPeopleInCells[x + y * input.evac.routeCellCount.x];
                    m = "People: " + people;
                    if (currenttrafficDensityData != null && currenttrafficDensityData[x + y * input.evac.routeCellCount.x] != null)
                    {
                        int peopleInCars = currenttrafficDensityData[x + y * input.evac.routeCellCount.x].peopleCount;
                        int cars = currenttrafficDensityData[x + y * input.evac.routeCellCount.x].carCount;

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
            File.WriteAllBytes(Application.dataPath + "/Resources/_output/rawPopulation.png", bytes);
            bytes = output.evac.popStuckTexture.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/Resources/_output/stuckPopulation.png", bytes);
            bytes = output.evac.relocatedPopTexture.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/Resources/_output/relocatedPopulation.png", bytes);
            bytes = output.evac.popStayingTexture.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/Resources/_output/stayingPopulation.png", bytes);
            if (input.traffic.routeChoice == TrafficInput.RouteChoice.ForceMap)
            {
                bytes = input.evac.evacuationForceTex.EncodeToPNG();
                File.WriteAllBytes(Application.dataPath + "/Resources/_output/forceMap.png", bytes);
            }
        }

        EvacuationPainter eP;

        public bool IsPainterActive()
        {
            if(eP == null || !eP.gameObject.activeSelf)
            {
                return false;
            }

            return true;
        }

        public void StartPainter(EvacuationPainter.PaintMode paintMode)
        {
            if (eP == null)
            {                
                GameObject gO = new GameObject();
                gO.name = "EvacGoalPainter";
                eP = gO.AddComponent<EvacuationPainter>();
            }

            eP.gameObject.SetActive(true);
            eP.SetPainterMode(paintMode);            
            if(paintMode == EvacuationPainter.PaintMode.ForceGoal)
            {
                DisplayForceMap();                
            }
            else if(paintMode == EvacuationPainter.PaintMode.EvacGroup)
            {
                DisplayEvacGroupMap();
            }
            dataSampleMode = DataSampleMode.Paint;
        }

        public void StopPainter()
        {
            if (eP != null)
            {
                eP.gameObject.SetActive(false);
            }
            dataSampleMode = DataSampleMode.None;
            if (dataPlane != null)
            {
                dataPlane.SetActive(false);
            }
        }

        private MeshRenderer CreateDataPlane(Texture2D tex, string name)
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

            Vector2 maxUV = new Vector2((float)input.evac.routeCellCount.x / tex.width, (float)input.evac.routeCellCount.y / tex.height);

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
            int outputIndex = (int)(time - input.evac.responseCurve.dataPoints[0].timeMinMax.x) / (int)input.traffic.saveInterval;
            if (outputIndex > trafficDensityData.Count - 1)
            {
                trafficDensityData.Add(new TrafficCellData[input.evac.routeCellCount.x * input.evac.routeCellCount.y]);

                for (int i = 0; i < cars.Count; i++)
                {
                    float lati = cars[i].goingToCoord.Latitude;
                    float longi = cars[i].goingToCoord.Longitude;

                    Mapbox.Utils.Vector2d pos = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(lati, longi, mapboxMap.CenterMercator, mapboxMap.WorldRelativeScale);

                    int x = (int)(pos.x / input.evac.routeCellSize);
                    int y = (int)(pos.y / input.evac.routeCellSize);

                    //outside of mapped data
                    if (x < 0 || x > input.evac.routeCellCount.x - 1 || y < 0 || y > input.evac.routeCellCount.y - 1)
                    {
                        continue;
                    }

                    //add or update data
                    if (trafficDensityData[outputIndex][x + y * input.evac.routeCellCount.x] == null)
                    {
                        trafficDensityData[outputIndex][x + y * input.evac.routeCellCount.x] = new TrafficCellData();
                        trafficDensityData[outputIndex][x + y * input.evac.routeCellCount.x].carCount = 1;
                        trafficDensityData[outputIndex][x + y * input.evac.routeCellCount.x].peopleCount = cars[i].numberOfPopleInCar;
                    }
                    else
                    {
                        trafficDensityData[outputIndex][x + y * input.evac.routeCellCount.x].carCount += 1;
                        trafficDensityData[outputIndex][x + y * input.evac.routeCellCount.x].peopleCount += cars[i].numberOfPopleInCar;
                    }
                }

                //save data from human re as well
                peopleInCells.Add(new int[input.evac.routeCellCount.x * input.evac.routeCellCount.y]);
                for (int y = 0; y < input.evac.routeCellCount.y; y++)
                {
                    for (int x = 0; x < input.evac.routeCellCount.x; x++)
                    {
                        peopleInCells[outputIndex][x + y * input.evac.routeCellCount.x] = sim.GetMacroHumanSim().GetPeopleLeftInCell(x, y);
                    }
                }

                //create texture
                Vector2Int res = new Vector2Int(2, 2);
                while (input.evac.routeCellCount.x > res.x)
                {
                    res.x *= 2;
                }
                while (input.evac.routeCellCount.y > res.y)
                {
                    res.y *= 2;
                }

                Texture2D tex = new Texture2D(res.x, res.y);
                tex.filterMode = FilterMode.Point;

                for (int y = 0; y < input.evac.routeCellCount.y; ++y)
                {
                    for (int x = 0; x < input.evac.routeCellCount.x; ++x)
                    {
                        Color c = Color.grey;
                        c.a = 0.0f;
                        int count = 0;
                        if (trafficDensityData[outputIndex][x + y * input.evac.routeCellCount.x] != null)
                        {
                            count += trafficDensityData[outputIndex][x + y * input.evac.routeCellCount.x].carCount;
                        }
                        //count += peopleInCells[outputIndex][x + y * input.evac.routeCellCount.x];
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
                File.WriteAllBytes(Application.dataPath + "/Resources/_output/trafficDens_" + (int)time + "s.png", bytes);
            }
        }

        MeshRenderer dataPlaneMeshRenderer;
        TrafficCellData[] currenttrafficDensityData;
        int[] currentPeopleInCells;
        public void DisplayClosestDensityData(float time)
        {
            if(sim.runTrafficSim)
            {
                int index = (int)time / (int)input.traffic.saveInterval;
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

        public void DisplayForceMap()
        {
            SetDataPlaneTexture(input.evac.evacuationForceTex);
        }

        public void DisplayEvacGroupMap()
        {
            SetDataPlaneTexture(input.evac.evacGroupTex);
        }

        public void ToggleDataPlane()
        {
            if (dataPlaneMeshRenderer != null)
            {
                dataPlaneMeshRenderer.gameObject.SetActive(!dataPlaneMeshRenderer.gameObject.activeSelf);
            }
        }

        private void SetDataPlaneTexture(Texture2D tex)
        {
            if (dataPlaneMeshRenderer == null)
            {
                dataPlaneMeshRenderer = CreateDataPlane(tex, "Data Plane");
                dataPlane = dataPlaneMeshRenderer.gameObject;
            }
            else
            {
                dataPlaneMeshRenderer.material.mainTexture = tex;
                dataPlane.SetActive(true);
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

