//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;       
using UnityEngine;                
using WUIPlatform.Pedestrian;                     
using WUIPlatform.Traffic;                          
using System.IO;
using WUIPlatform.IO;
using WUIPlatform.WUInity.UI;
using WUIPlatform.Population;

namespace WUIPlatform.WUInity
{    
    [RequireComponent(typeof(WUInityGUI))]
    [RequireComponent(typeof(Visualization.EvacuationRenderer))]
    [RequireComponent(typeof(Visualization.FireRenderer))]
    public class WUInityEngine : MonoBehaviour                     
    {
        public static WUInityEngine INSTANCE
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<WUInityEngine>();
                    if (_instance == null)
                    {
                        GameObject g = new GameObject();
                        _instance = g.AddComponent<WUInityEngine>();
                    }
                }
                return _instance;
            }
        }

        public static WUInityGUI GUI
        {
            get
            {
                if (INSTANCE._wuiGUI == null)
                {
                    INSTANCE._wuiGUI = INSTANCE.GetComponent<WUInityGUI>();
                    if (INSTANCE._wuiGUI == null)
                    {
                        INSTANCE.gameObject.AddComponent<WUInityGUI>();
                    }
                }
                return INSTANCE._wuiGUI;
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

        private Mapbox.Unity.Map.AbstractMap _mapboxMap;
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

        private Painter _painter;
        public static Painter Painter
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

        [SerializeField] private GodCamera _godCamera;
        public GodCamera WUICamera
        {
            get
            {
                if (_godCamera == null)
                {
                    _godCamera = FindFirstObjectByType<GodCamera>();
                    if(_godCamera == null)
                    {
                        GameObject g = new GameObject();
                        g.transform.parent = transform;
                        g.name = "GodCamera";
                        _godCamera = g.AddComponent<GodCamera>();
                    }                    
                }
                return _godCamera;
            }
        }

        [Header("Options")]
        public bool DeveloperMode = false;
        public bool AutoLoadExample = true;
        [SerializeField] float _renderScale = 1.0f;
        public float RenderScale { get => _renderScale; }

        [Header("Prefabs")]
        [SerializeField] private GameObject _markerPrefab;

        [Header("References")]              
        
        [SerializeField] private LineRenderer _simBorder;
        [SerializeField] private LineRenderer _osmBorder;
        [SerializeField] public  ComputeShader AdvectDiffuseCompute;
        [SerializeField] public Texture2D NoiseTex;
        [SerializeField] public Texture2D WindTex;

        public enum DataSampleMode { None, GPW, Population, Relocated, TrafficDens, Paint, Farsite }
        public DataSampleMode dataSampleMode = DataSampleMode.None;

        //never directly call these, always use singletons (except once when setting input)
        private static WUInityEngine _instance;

        //private Farsite.FarsiteViewer _farsiteViewer;

        private WUInityGUI _wuiGUI;
       
        
        private Visualization.FireRenderer _fireRenderer;
        private Visualization.EvacuationRenderer _evacuationRenderer;

        MeshRenderer _evacDataPlaneMeshRenderer;
        MeshRenderer _fireDataPlaneMeshRenderer;
        //List<GameObject> drawnRoad_s;
        GameObject[] _goalMarkers;
        private GameObject _evacDataPlane;
        private GameObject _fireDataPlane;
        GameObject _directionsGO;

        bool _renderHouseholds = false;
        bool _renderTraffic = false;
        bool _renderSmokeDispersion = false;
        bool _renderFireSpread = false;        

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
                string path = Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "example\\example.wui");
                if (File.Exists(path))
                {
                    WUIEngineInput.LoadInput(path);
                }
                else
                {
                    print("Could not find input file for auto load in path " + path);
                }
            }
        }

        private void OnApplicationQuit()
        {
            if(WUIEngine.SIM.State == Simulation.SimulationState.Running)
            {
                WUIEngine.SIM.Stop("Unity is closing, ending simulation.", true);
            }
        }

        /*public void DrawRoad(RouteCollection routeCollection, int index)
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
        }   */

        /*GameObject DrawRoute(RouteCollection rC, int index)
        {
            List<Vector3> dat = new List<Vector3>();
            foreach (Itinero.LocalGeo.Coordinate point in rC.GetSelectedRoute().route.Shape)
            {
                Vector3 v = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(point.Latitude, point.Longitude, MAP.CenterMercator, MAP.WorldRelativeScale).ToVector3xz();
                v.y = 10f;
                dat.Add(v);
            }
            return CreateLineObject(dat, index);
        }*/

        public bool LoadMapbox()
        {
            //Mapbox: calculate the amount of grids needed based on zoom level, coord and size
            Mapbox.Unity.Map.MapOptions mOptions = WUInity.WUInityEngine.MAP.Options; // new Mapbox.Unity.Map.MapOptions();

            mOptions.locationOptions.latitudeLongitude = "" + WUIEngine.INPUT.Simulation.LowerLeftLatLong.x + "," + WUIEngine.INPUT.Simulation.LowerLeftLatLong.y;
            mOptions.locationOptions.zoom = WUIEngine.INPUT.Map.zoomLevel;
            mOptions.extentOptions.extentType = Mapbox.Unity.Map.MapExtentType.RangeAroundCenter;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.west = 0;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.south = 0;
            //https://wiki.openstreetmap.org/wiki/Zoom_levels
            double degreesPerTile = 360.0 / (Mathf.Pow(2.0f, mOptions.locationOptions.zoom));
            Vector2d mapDegrees = LocalGPWData.SizeToDegrees(WUIEngine.INPUT.Simulation.LowerLeftLatLong, WUIEngine.INPUT.Simulation.Size);
            int tilesX = (int)(mapDegrees.x / degreesPerTile) + 1;
            int tilesY = (int)(mapDegrees.y / (degreesPerTile * Mathf.Cos((Mathf.PI / 180.0f) * (float)WUIEngine.INPUT.Simulation.LowerLeftLatLong.x))) + 1;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.east = tilesX;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.north = tilesY;
            mOptions.placementOptions.placementType = Mapbox.Unity.Map.MapPlacementType.AtLocationCenter;
            mOptions.placementOptions.snapMapToZero = true;
            mOptions.scalingOptions.scalingType = Mapbox.Unity.Map.MapScalingType.WorldScale;

            if (!MAP.IsAccessTokenValid)
            {
                WUIEngine.LOG(WUIEngine.LogType.Error, "Mapbox token not valid.");
                return false;
            }

            WUIEngine.LOG(WUIEngine.LogType.Log, "Starting to load Mapbox map.");
            MAP.Initialize(new Mapbox.Utils.Vector2d(WUIEngine.INPUT.Simulation.LowerLeftLatLong.x, WUIEngine.INPUT.Simulation.LowerLeftLatLong.y), WUIEngine.INPUT.Map.zoomLevel);
            WUIEngine.LOG(WUIEngine.LogType.Log, "Map loaded succesfully.");

            //generally we want to convert to UTM
            if (!WUIEngine.INPUT.Simulation.ScaleToWebMercator)
            {
                MAP.transform.localScale = new Vector3((float)WUIEngine.RUNTIME_DATA.Simulation.MercatorToUtmScale.x, 1.0f, (float)WUIEngine.RUNTIME_DATA.Simulation.MercatorToUtmScale.y);
            }           

            return true;
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

        /*public void DeleteDrawnRoads()
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
        }*/

        public void DrawOSMNetwork()
        {

        }

        /*public void LoadFarsite()
        {
            FARSITE_VIEWER.ImportFarsite();
            FARSITE_VIEWER.TransformCoordinates();

            LOG(WUIEngine.LogType.Warning, "Farsite loaded succesfully.");
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
                    float xNorm = hitPoint.x / (float)WUIEngine.INPUT.Simulation.Size.x;
                    //xNorm = Mathf.Clamp01(xNorm);
                    int x = (int)(WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x * xNorm);

                    float yNorm = hitPoint.z / (float)WUIEngine.INPUT.Simulation.Size.y;
                    //yNorm = Mathf.Clamp01(yNorm);
                    int y = (int)(WUIEngine.RUNTIME_DATA.Evacuation.CellCount.y * yNorm);
                    GetCellInfo(hitPoint, x, y);
                }
            }    

            //always update visuals, even when paused
            if(WUIEngine.RUNTIME_DATA != null)
            {
                if (!WUIEngine.RUNTIME_DATA.Simulation.MultipleSimulations && WUIEngine.SIM.State == Simulation.SimulationState.Running)
                {
                    if (!_visualsExist)
                    {
                        CreateVisualizers();
                    }
                    EVAC_VISUALS.UpdateEvacuationRenderer(_renderHouseholds, _renderTraffic);
                    FIRE_VISUALS.UpdateFireRenderer(_renderFireSpread, _renderSmokeDispersion);
                }
            }            

            if (updateOSMBorder)
            {
                UpdateOSMBorder();
            }                
        }

        public void StartSimulation()
        {
            _visualsExist = false;
            SetSampleMode(WUInityEngine.DataSampleMode.TrafficDens);
            // SetEvacDataPlane(true);   // This is turned off as we don't want to display the _evacDataPlaneMeshRenderer by default at the start of the simulation.   16/08/2023 
            WUIEngine.SIM.Start();
        }

        bool _visualsExist = false;
        public void CreateVisualizers()
        {
            //this needs to be done AFTER simulation has started since we need some data from the sim
            //fix everything for evac rendering
            EVAC_VISUALS.CreateBuffers(WUIEngine.INPUT.Simulation.RunPedestrianModule, WUIEngine.INPUT.Simulation.RunTrafficModule);            

            _renderHouseholds = WUIEngine.INPUT.Simulation.RunPedestrianModule;
            _renderTraffic = WUIEngine.INPUT.Simulation.RunTrafficModule;

            //and then for fire rendering
            FIRE_VISUALS.CreateBuffers(WUIEngine.INPUT.Simulation.RunFireModule, WUIEngine.INPUT.Simulation.RunSmokeModule);
            _renderFireSpread = WUIEngine.INPUT.Simulation.RunFireModule;
            _renderSmokeDispersion = WUIEngine.INPUT.Simulation.RunSmokeModule;

            _visualsExist = true;

            ActivateSuitableVisuals();
        }

        public void RunAllCasesInFolder(string folder)
        {
            string[] inputFiles = Directory.GetFiles(folder, "*.wui");
            for (int i = 0; i < inputFiles.Length; i++)
            {
                WUIEngineInput.LoadInput(inputFiles[i]);
                WUIEngine.INPUT.Simulation.SimulationID = Path.GetFileNameWithoutExtension(inputFiles[i]);
                WUIEngine.RUNTIME_DATA.Simulation.MultipleSimulations = true;
                WUIEngine.RUNTIME_DATA.Simulation.NumberOfRuns = 100;
                StartSimulation();
            }
        }

        public void StopSimulation()
        {
            HideAllRuntimeVisuals();
            WUIEngine.SIM.Stop("STOP: Stopped simulation as requested by user.", true);
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

        public void UpdateSimBorders()
        {
            if(!WUIEngine.DATA_STATUS.HaveInput)
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
                _simBorder.SetPosition(1, _simBorder.GetPosition(0) + Vector3.right * (float)WUIEngine.INPUT.Simulation.Size.x);
                _simBorder.SetPosition(2, _simBorder.GetPosition(1) + Vector3.forward * (float)WUIEngine.INPUT.Simulation.Size.y);
                _simBorder.SetPosition(3, _simBorder.GetPosition(2) - Vector3.right * (float)WUIEngine.INPUT.Simulation.Size.x);
                _simBorder.SetPosition(4, _simBorder.GetPosition(0));
            }        
        }

        void UpdateOSMBorder()
        {            
            if (_osmBorder != null)
            {
                _osmBorder.SetPosition(0, -Vector3.right * WUIEngine.RUNTIME_DATA.Routing.BorderSize - Vector3.forward * WUIEngine.RUNTIME_DATA.Routing.BorderSize + Vector3.up * 10f);
                _osmBorder.SetPosition(1, _osmBorder.GetPosition(0) + Vector3.right * ((float)WUIEngine.INPUT.Simulation.Size.x + WUIEngine.RUNTIME_DATA.Routing.BorderSize * 2f));
                _osmBorder.SetPosition(2, _osmBorder.GetPosition(1) + Vector3.forward * ((float)WUIEngine.INPUT.Simulation.Size.y + WUIEngine.RUNTIME_DATA.Routing.BorderSize * 2f));
                _osmBorder.SetPosition(3, _osmBorder.GetPosition(2) - Vector3.right * ((float)WUIEngine.INPUT.Simulation.Size.x + WUIEngine.RUNTIME_DATA.Routing.BorderSize * 2f));
                _osmBorder.SetPosition(4, _osmBorder.GetPosition(0));
            }
        }

        void GetCellInfo(Vector3 pos, int x, int y)
        {
            dataSampleString = "No data to sample.";
            if (dataSampleMode == DataSampleMode.GPW)
            {
                if (WUIEngine.POPULATION.GetLocalGPWData() != null && WUIEngine.POPULATION.GetLocalGPWData().density != null && WUIEngine.POPULATION.GetLocalGPWData().density.Length > 0)
                {
                    if (WUIEngine.POPULATION.Visualizer.IsDataPlaneActive())
                    {
                        float xCellSize = (float)(WUIEngine.POPULATION.GetLocalGPWData().realWorldSize.x / WUIEngine.POPULATION.GetLocalGPWData().dataSize.x);
                        float yCellSize = (float)(WUIEngine.POPULATION.GetLocalGPWData().realWorldSize.y / WUIEngine.POPULATION.GetLocalGPWData().dataSize.y);
                        double cellArea = xCellSize * yCellSize / (1000000d);
                        dataSampleString = "GPW people count: " + System.Convert.ToInt32(WUIEngine.POPULATION.GetLocalGPWData().GetDensityUnitySpace(new WUIPlatform.Vector2d(pos.x, pos.z)) * cellArea);
                    }
                    else
                    {
                        dataSampleString = "GPW data not visible, activate to sample data.";
                    }
                }
            }
            else if (x < 0 || x > WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x || y < 0 || y > WUIEngine.RUNTIME_DATA.Evacuation.CellCount.y)
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
                    dataSampleString = "Interpolated people count: " + WUIEngine.POPULATION.GetPopulation(x, y);
                }
                else if (dataSampleMode == DataSampleMode.Relocated)
                {
                    if (WUIEngine.SIM.PedestrianModule != null)
                    {
                        dataSampleString = "Rescaled and relocated people count: " + ((MacroHouseholdSim)WUIEngine.SIM.PedestrianModule).GetPopulation(x, y);
                    }
                }
                else if (dataSampleMode == DataSampleMode.TrafficDens)
                {
                    int people = currentPeopleInCells[x + y * WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x];
                    dataSampleString = "People: " + people;
                    if (currenttrafficDensityData != null && currenttrafficDensityData[x + y * WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x] != null)
                    {
                        int peopleInCars = currenttrafficDensityData[x + y * WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x].peopleCount;
                        int cars = currenttrafficDensityData[x + y * WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x].carCount;

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
            if(!Painter.gameObject.activeSelf)
            {
                return false;
            }

            return true;
        }

        public void StartPainter(Painter.PaintMode paintMode)
        {
            Painter.gameObject.SetActive(true);
            Painter.SetPainterMode(paintMode);
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
                WUIEngine.LOG(WUIEngine.LogType.Error, "Paint mode not set correctly");
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
            Painter.gameObject.SetActive(false);
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

            float width = (float)WUIEngine.INPUT.Simulation.Size.x;
            float length = (float)WUIEngine.INPUT.Simulation.Size.y;


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

        public void SpawnEvacuationGoalMarkers()
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

            _goalMarkers = new GameObject[WUIEngine.RUNTIME_DATA.Evacuation.EvacuationGoals.Count];
            for (int i = 0; i < WUIEngine.RUNTIME_DATA.Evacuation.EvacuationGoals.Count; i++)
            {
                EvacuationGoal eG = WUIEngine.RUNTIME_DATA.Evacuation.EvacuationGoals[i];
                _goalMarkers[i] = Instantiate<GameObject>(_markerPrefab);
                Vector2d pos = GeoConversions.GeoToWorldPosition(eG.latLong.x, eG.latLong.y, WUIEngine.RUNTIME_DATA.Simulation.CenterMercator, WUIEngine.RUNTIME_DATA.Simulation.MercatorCorrectionScale);

                float scale = 0.02f * (float)WUIEngine.INPUT.Simulation.Size.y;
                _goalMarkers[i].transform.localScale = new Vector3(scale, 100f, scale);
                _goalMarkers[i].transform.position = new Vector3((float)pos.x, 0f, (float)pos.y);
                MeshRenderer mR = _goalMarkers[i].GetComponentInChildren<MeshRenderer>();
                mR.material.color = eG.color.UnityColor;
            }            
        }        
                
        TrafficCellData[] currenttrafficDensityData;
        int[] currentPeopleInCells;
        public void DisplayClosestDensityData(float time)
        {
            if(WUIEngine.INPUT.Simulation.RunTrafficModule)
            {
                int index = UnityEngine.Mathf.Max(0, (int)time / (int)WUIEngine.INPUT.Traffic.saveInterval);
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

        public void ActivateSuitableVisuals()
        {
            if(WUIEngine.INPUT.Simulation.RunPedestrianModule)
            {
                SetHouseholdRendering(true);
            }

            if (WUIEngine.INPUT.Simulation.RunTrafficModule)
            {
                SetTrafficRendering(true);
            }

            if (WUIEngine.INPUT.Simulation.RunFireModule)
            {
                SetFireSpreadRendering(true);
            }

            if (WUIEngine.INPUT.Simulation.RunSmokeModule)
            {
                SetSootRendering(true);
            }
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
            SetDataPlaneTexture((Texture2D)WUIEngine.POPULATION.Visualizer.GetPopulationTexture());
        }

        private void DisplayWUIAreaMap()
        {
            SetDataPlaneTexture(Painter.GetWUIAreaTexture(), true);
        }

        public void DisplayRandomIgnitionAreaMap()
        {
            SetDataPlaneTexture(Painter.GetRandomIgnitionTexture(), true);
        }

        public void DisplayInitialIgnitionMap()
        {
            SetDataPlaneTexture(Painter.GetInitialIgnitionTexture(), true);
        }

        public void DisplayTriggerBufferMap()
        {
            SetDataPlaneTexture(Painter.GetTriggerBufferTexture(), true);
        }

        public void DisplayEvacGroupMap()
        {
            SetDataPlaneTexture(Painter.GetEvacGroupTexture());
        }

        public void DisplayCustomPopulationData()
        {
            SetDataPlaneTexture(Painter.GetCustomPopulationMaskTexture());
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
            if (_renderHouseholds != enable)
            {
                ToggleHouseholdRendering();
            }
        }

        public bool ToggleHouseholdRendering()
        {
            _renderHouseholds = !_renderHouseholds;
            return _renderHouseholds;
        }

        public void SetTrafficRendering(bool enable)
        {
            if(_renderTraffic != enable)
            {
                ToggleTrafficRendering();
            }
        }

        public bool ToggleTrafficRendering()
        {
            _renderTraffic = !_renderTraffic;
            return _renderTraffic;
        }

        public void SetSootRendering(bool enable)
        {
            if(enable != _renderSmokeDispersion)
            {
                ToggleSootRendering();
            }
        }

        public bool ToggleSootRendering()
        {
            _renderSmokeDispersion = FIRE_VISUALS.ToggleSoot();
            return _renderSmokeDispersion;
        }

        public void SetFireSpreadRendering(bool enable)
        {
            if(enable != _renderFireSpread)
            {
                ToggleFireSpreadRendering();
            }
        }

        public bool ToggleFireSpreadRendering()
        {
            _renderFireSpread = FIRE_VISUALS.ToggleFire();
            return _renderFireSpread;
        }

        private void SetDataPlaneTexture(Texture2D tex, bool fireMeshMode = false)
        {
            //pick needed data plane
            MeshRenderer activeMeshRenderer = _evacDataPlaneMeshRenderer;
            Vector2int cellCount = WUIEngine.RUNTIME_DATA.Evacuation.CellCount;
            string name = "Evac Data Plane";
            if (fireMeshMode)
            {
                activeMeshRenderer = _fireDataPlaneMeshRenderer;
                cellCount = new Vector2int(WUIEngine.SIM.FireModule.GetCellCountX(), WUIEngine.SIM.FireModule.GetCellCountY());
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

        WUIEngineColor GetTrafficDensityColor(int cars)
        {
            float fraction = UnityEngine.Mathf.Lerp(0f, 1f, cars / 20f);
            WUIEngineColor c = WUIEngineColor.HSVToRGB(0.67f - 0.67f * fraction, 1.0f, 1.0f);

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
            int outputIndex = (int)(time - WUIEngine.SIM.StartTime) / (int)WUIEngine.INPUT.Traffic.saveInterval;
            if (outputIndex > trafficDensityData.Count - 1)
            {
                trafficDensityData.Add(new TrafficCellData[WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x * WUIEngine.RUNTIME_DATA.Evacuation.CellCount.y]);

                for (int i = 0; i < carsInSystem.Count; i++)
                {
                    System.Numerics.Vector4 posAndSpeed = carsInSystem[i].GetUnityPositionAndSpeed(false);

                    int x = (int)(posAndSpeed.X / WUIEngine.INPUT.Evacuation.RouteCellSize);
                    int y = (int)(posAndSpeed.Y / WUIEngine.INPUT.Evacuation.RouteCellSize);

                    //outside of mapped data
                    if (x < 0 || x > WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x - 1 || y < 0 || y > WUIEngine.RUNTIME_DATA.Evacuation.CellCount.y - 1)
                    {
                        continue;
                    }

                    //add or update data
                    if (trafficDensityData[outputIndex][x + y * WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x] == null)
                    {
                        trafficDensityData[outputIndex][x + y * WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x] = new TrafficCellData();
                        trafficDensityData[outputIndex][x + y * WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x].carCount = 1;
                        trafficDensityData[outputIndex][x + y * WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x].peopleCount = (int)carsInSystem[i].numberOfPeopleInCar;
                    }
                    else
                    {
                        trafficDensityData[outputIndex][x + y * WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x].carCount += 1;
                        trafficDensityData[outputIndex][x + y * WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x].peopleCount += (int)carsInSystem[i].numberOfPeopleInCar;
                    }
                }

                for (int i = 0; i < carsOnHold.Count; i++)
                {
                    System.Numerics.Vector4 posAndSpeed = carsOnHold[i].GetUnityPositionAndSpeed(false);

                    int x = (int)(posAndSpeed.X / WUIEngine.INPUT.Evacuation.RouteCellSize);
                    int y = (int)(posAndSpeed.Y / WUIEngine.INPUT.Evacuation.RouteCellSize);

                    //outside of mapped data
                    if (x < 0 || x > WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x - 1 || y < 0 || y > WUIEngine.RUNTIME_DATA.Evacuation.CellCount.y - 1)
                    {
                        continue;
                    }

                    //add or update data
                    if (trafficDensityData[outputIndex][x + y * WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x] == null)
                    {
                        trafficDensityData[outputIndex][x + y * WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x] = new TrafficCellData();
                        trafficDensityData[outputIndex][x + y * WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x].carCount = 1;
                        trafficDensityData[outputIndex][x + y * WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x].peopleCount = (int)carsOnHold[i].numberOfPeopleInCar;
                    }
                    else
                    {
                        trafficDensityData[outputIndex][x + y * WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x].carCount += 1;
                        trafficDensityData[outputIndex][x + y * WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x].peopleCount += (int)carsOnHold[i].numberOfPeopleInCar;
                    }
                }

                //save data from human re as well
                peopleInCells.Add(new int[WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x * WUIEngine.RUNTIME_DATA.Evacuation.CellCount.y]);
                for (int y = 0; y < WUIEngine.RUNTIME_DATA.Evacuation.CellCount.y; y++)
                {
                    for (int x = 0; x < WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x; x++)
                    {
                        peopleInCells[outputIndex][x + y * WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x] = ((MacroHouseholdSim)WUIEngine.SIM.PedestrianModule).GetPeopleLeftInCell(x, y);
                    }
                }

                //create texture
                Vector2Int res = new Vector2Int(2, 2);
                while (WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x > res.x)
                {
                    res.x *= 2;
                }
                while (WUIEngine.RUNTIME_DATA.Evacuation.CellCount.y > res.y)
                {
                    res.y *= 2;
                }

                Texture2D tex = new Texture2D(res.x, res.y);
                tex.filterMode = FilterMode.Point;

                for (int y = 0; y < WUIEngine.RUNTIME_DATA.Evacuation.CellCount.y; ++y)
                {
                    for (int x = 0; x < WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x; ++x)
                    {
                        WUIEngineColor c = WUIEngineColor.grey;
                        c.a = 0.0f;
                        int count = 0;
                        if (trafficDensityData[outputIndex][x + y * WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x] != null)
                        {
                            count += trafficDensityData[outputIndex][x + y * WUIEngine.RUNTIME_DATA.Evacuation.CellCount.x].carCount;
                        }
                        //count += peopleInCells[outputIndex][x + y * WUIEngine.SIM.GetCellCount.x];
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
                File.WriteAllBytes(Path.Combine(WUIEngine.OUTPUT_FOLDER, "trafficDens_" + (int)time + "s.png"), bytes);
            }
        }
    }
}