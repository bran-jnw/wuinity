//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;       
using UnityEngine;
using PREACT.IO;
using PREACT.Pedestrian;                     
using PREACT.Traffic;                          
using System.IO;
using PREACT;
using WUInity.UI;
using PREACT.Population;
using WUInity.Visualization;

namespace WUInity
{
    public enum DataSampleMode { None, LocalGPW, PopulationMap, Relocated, TrafficDens, Paint, Farsite }

    [RequireComponent(typeof(WUInityGUI))]
    [RequireComponent(typeof(Visualization.EvacuationRenderer))]
    [RequireComponent(typeof(Visualization.FireRenderer))]
    public class WUInityManager : MonoBehaviour, IExternalManager                     
    {
        public EvacuationRenderer EvacuationRenderer
        {
            get
            {
                if (_evacuationRenderer == null)
                {
                    _evacuationRenderer = GetComponent<EvacuationRenderer>();
                    if(_evacuationRenderer == null)
                    {
                        _evacuationRenderer = gameObject.AddComponent<EvacuationRenderer>();
                    }
                }
                return _evacuationRenderer;
            }
        }

        public FireRenderer FireRenderer
        {
            get
            {
                if (_fireRenderer == null)
                {
                    _fireRenderer = GetComponent<FireRenderer>();
                    if (_fireRenderer == null)
                    {
                        _fireRenderer = gameObject.AddComponent<FireRenderer>();
                    }
                }
                return _fireRenderer;
            }
        }

        private Mapbox.Unity.Map.AbstractMap _mapboxMap;
        public Mapbox.Unity.Map.AbstractMap Map { get => _mapboxMap; }

        private Painter _painter;
        public Painter Painter{ get => _painter; }

        [SerializeField] private GodCamera _godCamera;

        [Header("Options")]
        public bool DeveloperMode = false;
        public bool SuppressMessages = false;
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
                
        public DataSampleMode dataSampleMode = DataSampleMode.None;

        private WUInityGUI _wuiGUI;
        PREACTInput _input;
        public PREACTInput PREACTInput { get => _input; }


        private FireRenderer _fireRenderer;
        private EvacuationRenderer _evacuationRenderer;
        private SimulationDomainVisualizerUnity _simulationDomainVisualizer;
        private FireDomainVisualizerUnity _fireDomainVisualizer;

        public SimulationDomainVisualizerUnity SimulationDomainVisualizer { get => _simulationDomainVisualizer; }
        public FireDomainVisualizerUnity FireDomainVisualizer { get => _fireDomainVisualizer; }
        
        //List<GameObject> drawnRoad_s;
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
        PREACT.Runtime.WorkingData _workingData;
        Engine _engine;
        public Engine Engine { get => _engine; }
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
            
            _engine = new Engine(this);

            //gui
            _workingData = new PREACT.Runtime.WorkingData();
            _wuiGUI = GetComponent<WUInityGUI>();
            if (_wuiGUI == null)
            {
                gameObject.AddComponent<WUInityGUI>();
            }
            _wuiGUI.SetManager(this, _engine, _workingData);

            //map
            _mapboxMap = FindFirstObjectByType<Mapbox.Unity.Map.AbstractMap>();
            if (_mapboxMap == null)
            {
                GameObject g = new GameObject();
                g.name = "Mapbox Map";
                g.transform.parent = transform;
                _mapboxMap = g.AddComponent<Mapbox.Unity.Map.AbstractMap>();
            }

            _painter = FindFirstObjectByType<Painter>();
            if (_painter == null)
            {
                GameObject g = new GameObject();
                g.transform.parent = transform;
                g.name = "WUI Painter";
                _painter = g.AddComponent<Painter>();
                g.SetActive(false);
            }

            _godCamera = FindFirstObjectByType<GodCamera>();
            if (_godCamera == null)
            {
                GameObject g = new GameObject();
                g.transform.parent = transform;
                g.name = "GodCamera";
                _godCamera = g.AddComponent<GodCamera>();
            }

            _simulationDomainVisualizer = new SimulationDomainVisualizerUnity(transform);
            _fireDomainVisualizer = new FireDomainVisualizerUnity(transform);
        }

        private void Start()
        {
            if (AutoLoadExample && DeveloperMode)
            {
                bool success = false;
                string file = Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "Examples\\Roxborough\\Roxborough_example.wui");                
                if (File.Exists(file))
                {                    
                    _engine.LoadInputFromFile(file, out success);

                }
                else
                {
                    print("Could not find input file for auto load in path " + file);
                }
            }
        }

        private void OnApplicationQuit()
        {
            if (_engine != null)
            {
                _engine.CloseSimulations(false);
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

        public void LoadMapbox(PREACTInput input)
        {
            //Mapbox: calculate the amount of grids needed based on zoom level, coord and size
            Mapbox.Unity.Map.MapOptions mOptions = Map.Options; // new Mapbox.Unity.Map.MapOptions();

            mOptions.locationOptions.latitudeLongitude = "" + input.Simulation.LowerLeftLatLon.x + "," + input.Simulation.LowerLeftLatLon.y;
            mOptions.locationOptions.zoom = input.Map.ZoomLevel;
            mOptions.extentOptions.extentType = Mapbox.Unity.Map.MapExtentType.RangeAroundCenter;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.west = 0;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.south = 0;
            //https://wiki.openstreetmap.org/wiki/Zoom_levels
            double degreesPerTile = 360.0 / (Mathf.Pow(2.0f, mOptions.locationOptions.zoom));
            PREACT.Math.Vector2d mapDegrees = LocalGPWData.SizeToDegrees(input.Simulation.LowerLeftLatLon, input.Simulation.DomainSize);
            int tilesX = (int)(mapDegrees.x / degreesPerTile) + 1;
            int tilesY = (int)(mapDegrees.y / (degreesPerTile * Mathf.Cos((Mathf.PI / 180.0f) * (float)input.Simulation.LowerLeftLatLon.x))) + 1;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.east = tilesX;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.north = tilesY;
            mOptions.placementOptions.placementType = Mapbox.Unity.Map.MapPlacementType.AtLocationCenter;
            mOptions.placementOptions.snapMapToZero = true;
            mOptions.scalingOptions.scalingType = Mapbox.Unity.Map.MapScalingType.WorldScale;

            if (!Map.IsAccessTokenValid)
            {
                Engine.Message(null, Engine.LogType.SimulationError, "Mapbox token not valid.");
                return;
            }

            Engine.Message(null, Engine.LogType.Log, "Starting to load Mapbox map.");
            Map.Initialize(new Mapbox.Utils.Vector2d(input.Simulation.LowerLeftLatLon.x, input.Simulation.LowerLeftLatLon.y), input.Map.ZoomLevel);
            Engine.Message(null, Engine.LogType.Log, "Map loaded succesfully.");

            //do warping to better fit UTM
            for (int i = 0; i < Map.transform.childCount; ++i)
            {
                Mapbox.Unity.MeshGeneration.Data.UnityTile tile = Map.transform.GetChild(i).GetComponent<Mapbox.Unity.MeshGeneration.Data.UnityTile>();
                if(tile != null)
                {
                    Vector3[] vertices = tile.GetComponent<MeshFilter>().mesh.vertices;
                    for(int v = 0; v < vertices.Length; ++v)
                    {
                        Vector3 worldPos = tile.transform.TransformPoint(vertices[v]);
                        var wgs84Pos = Map.WorldToGeoPosition(worldPos); //GeoConversions.MetersToLatLon(new Vector2d(worldPos.x, worldPos.z) + WUIEngine.RUNTIME_DATA.Simulation.CenterMercator);
                        PREACT.Utility.LatLngUTMConverter.UTMResult utmPos = PREACT.Utility.LatLngUTMConverter.WGS84.convertLatLngToUtm(wgs84Pos.x, wgs84Pos.y);
                        Vector3 newWorldPos = new Vector3((float)(utmPos.Easting - input.Simulation.Data.UTMOrigin.x), 0f, (float)(utmPos.Northing - input.Simulation.Data.UTMOrigin.y));
                        vertices[v] = tile.transform.InverseTransformPoint(newWorldPos);
                    }
                    tile.GetComponent<MeshFilter>().mesh.SetVertices(vertices);
                    tile.GetComponent<MeshFilter>().mesh.RecalculateBounds();
                }
            }
            //MAP.transform.localScale = new Vector3((float)WUIEngine.RUNTIME_DATA.Simulation.MercatorToUtmScale.x, 1.0f, (float)WUIEngine.RUNTIME_DATA.Simulation.MercatorToUtmScale.y);  
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
                Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
                float enter = 0.0f;
                if (_yPlane.Raycast(ray, out enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);
                    float xNorm = hitPoint.x / (float)_input.Simulation.DomainSize.x;
                    //xNorm = Mathf.Clamp01(xNorm);
                    int x = (int)(_input.Evacuation.Data.CellCount.x * xNorm);

                    float yNorm = hitPoint.z / (float)_input.Simulation.DomainSize.y;
                    //yNorm = Mathf.Clamp01(yNorm);
                    int y = (int)(_input.Evacuation.Data.CellCount.y * yNorm);
                    GetCellInfo(hitPoint, x, y);
                }
            }    

            //always update visuals, even when paused
            if(_engine.Simulation != null)
            {
                if (_engine.Simulation.State == Simulation.SimulationState.Running) // !WUIEngine.RUNTIME_DATA.Simulation.MultipleSimulations && 
                {
                    if (!_visualsExist)
                    {
                        CreateVisualizers();
                    }
                    EvacuationRenderer.UpdateEvacuationRenderer(_renderHouseholds, _renderTraffic, _engine.Simulation.PedestrianModule, _engine.Simulation.TrafficModule);
                    FireRenderer.UpdateFireRenderer(_renderFireSpread, _renderSmokeDispersion, _engine.Simulation);
                }
            }            

            if (updateOSMBorder)
            {
                //UpdateOSMBorder();
            }                
        }

        public void RunSimulation(EngineTask engineTask)
        {
            _visualsExist = false;
            SetSampleMode(DataSampleMode.TrafficDens);
            _engine.RunSimulations(engineTask);
        }

        bool _visualsExist = false;
        public void CreateVisualizers()
        {
            //this needs to be done AFTER simulation has started since we need some data from the sim
            //fix everything for evac rendering
            EvacuationRenderer.CreateBuffers(_input.Simulation.RunPedestrianModule, _input.Simulation.RunTrafficModule, _input.Simulation.DomainSize, _engine.Simulation.PedestrianModule);            

            _renderHouseholds = _input.Simulation.RunPedestrianModule;
            _renderTraffic = _input.Simulation.RunTrafficModule;

            //and then for fire rendering
            FireRenderer.CreateBuffers(_engine.Simulation);
            _renderFireSpread = _input.Simulation.RunFireModule;
            _renderSmokeDispersion = _input.Simulation.RunSmokeModule;

            _visualsExist = true;

            ActivateSuitableVisuals();
        }

        public void RunAllCasesInFolder(string folder, EngineTask engineTask)
        {            
            string[] inputFiles = Directory.GetFiles(folder, "*.wui");
            bool success;
            for (int i = 0; i < inputFiles.Length; i++)
            {
                _engine.LoadInputFromFile(inputFiles[i], out success);
                if(success)
                {
                    RunSimulation(engineTask);
                }                
            }
        }

        public void StopSimulations()
        {
            HideAllRuntimeVisuals();
            _engine.CloseSimulations(false);
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
            if(!_engine.DataStatus.HaveInput)
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
                _simBorder.SetPosition(1, _simBorder.GetPosition(0) + Vector3.right * (float)_input.Simulation.DomainSize.x);
                _simBorder.SetPosition(2, _simBorder.GetPosition(1) + Vector3.forward * (float)_input.Simulation.DomainSize.y);
                _simBorder.SetPosition(3, _simBorder.GetPosition(2) - Vector3.right * (float)_input.Simulation.DomainSize.x);
                _simBorder.SetPosition(4, _simBorder.GetPosition(0));
            }        
        }

        /*void UpdateOSMBorder()
        {            
            if (_osmBorder != null)
            {
                _osmBorder.SetPosition(0, -Vector3.right * WUIEngine.RUNTIME_DATA.Routing.BorderSize - Vector3.forward * WUIEngine.RUNTIME_DATA.Routing.BorderSize + Vector3.up * 10f);
                _osmBorder.SetPosition(1, _osmBorder.GetPosition(0) + Vector3.right * ((float)WUIEngine.INPUT.Simulation.Size.x + WUIEngine.RUNTIME_DATA.Routing.BorderSize * 2f));
                _osmBorder.SetPosition(2, _osmBorder.GetPosition(1) + Vector3.forward * ((float)WUIEngine.INPUT.Simulation.Size.y + WUIEngine.RUNTIME_DATA.Routing.BorderSize * 2f));
                _osmBorder.SetPosition(3, _osmBorder.GetPosition(2) - Vector3.right * ((float)WUIEngine.INPUT.Simulation.Size.x + WUIEngine.RUNTIME_DATA.Routing.BorderSize * 2f));
                _osmBorder.SetPosition(4, _osmBorder.GetPosition(0));
            }
        }*/

        void GetCellInfo(Vector3 pos, int x, int y)
        {
            dataSampleString = "No data to sample.";
            if (dataSampleMode == DataSampleMode.LocalGPW && _engine.WorkingData.LocalGPWData != null)
            {                
                if (_simulationDomainVisualizer.IsDataPlaneActive())
                {
                    float xCellSize = (float)(_engine.WorkingData.LocalGPWData.RealWorldSize.x / _engine.WorkingData.LocalGPWData.CellCount.x);
                    float yCellSize = (float)(_engine.WorkingData.LocalGPWData.RealWorldSize.y / _engine.WorkingData.LocalGPWData.CellCount.y);
                    double cellArea = xCellSize * yCellSize / (1000000d);
                    dataSampleString = "GPW people count: " + System.Convert.ToInt32(_engine.WorkingData.LocalGPWData.GetDensitySimulationSpace(new PREACT.Math.Vector2d(pos.x, pos.z)) * cellArea);
                }
                else
                {
                    dataSampleString = "GPW data not visible, activate to sample data.";
                }
            }
            else if (x < 0 || x > _input.Evacuation.Data.CellCount.x || y < 0 || y > _input.Evacuation.Data.CellCount.y)
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
            else if (_simulationDomainVisualizer.IsDataPlaneActive())
            {
                if (dataSampleMode == DataSampleMode.PopulationMap)
                {
                    dataSampleString = "Interpolated people count: " + _engine.WorkingData.PopulationMap.GetPeopleCount(x, y);
                }
                else if (dataSampleMode == DataSampleMode.Relocated)
                {
                    if (_engine.Simulation.PedestrianModule != null)
                    {
                        dataSampleString = "Rescaled and relocated people count: " + ((MacroHouseholdSim)_engine.Simulation.PedestrianModule).GetPopulation(x, y);
                    }
                }
                else if (dataSampleMode == DataSampleMode.TrafficDens)
                {
                    int people = currentPeopleInCells[x + y * _input.Evacuation.Data.CellCount.x];
                    dataSampleString = "People: " + people;
                    if (currenttrafficDensityData != null && currenttrafficDensityData[x + y * _input.Evacuation.Data.CellCount.x] != null)
                    {
                        int peopleInCars = currenttrafficDensityData[x + y * _input.Evacuation.Data.CellCount.x].peopleCount;
                        int cars = currenttrafficDensityData[x + y * _input.Evacuation.Data.CellCount.x].carCount;

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
            else if(paintMode == Painter.PaintMode.EvacGroup)
            {
                DisplayEvacGroupMap();
            }
            else if (paintMode == Painter.PaintMode.PopulationMask)
            {
                DisplayPopulationMask();
            }
            else
            {
                Engine.Message(null, Engine.LogType.Warning, "Paint mode not set correctly.");
            }
            dataSampleMode = DataSampleMode.Paint;

            if(fireEdit)
            {
                _simulationDomainVisualizer.SetVisibility(false);
                _fireDomainVisualizer.SetVisibility(true);
            }
            else
            {
                _simulationDomainVisualizer.SetVisibility(true);
                _fireDomainVisualizer.SetVisibility(false);
            }
        }

        public void StopPainter()
        {
            Painter.gameObject.SetActive(false);
            dataSampleMode = DataSampleMode.None;
            _simulationDomainVisualizer.SetVisibility(false);
            _fireDomainVisualizer.SetVisibility(false);
        }
        
                
        TrafficCellData[] currenttrafficDensityData;
        int[] currentPeopleInCells;
        /*public void DisplayClosestDensityData(float time)
        {
            if(_input.Simulation.RunTrafficModule)
            {
                int index = UnityEngine.Mathf.Max(0, (int)time / 600);
                if (index > outputTextures.Count - 1)
                {
                    index = outputTextures.Count - 1;
                }
                Texture2D tex = outputTextures[index];

                currenttrafficDensityData = trafficDensityData[index];
                currentPeopleInCells = peopleInCells[index];

                SetDataPlaneTexture(tex);
            }            
        }*/

        public void ActivateSuitableVisuals()
        {
            if(_input.Simulation.RunPedestrianModule)
            {
                SetHouseholdRendering(true);
            }

            if (_input.Simulation.RunTrafficModule)
            {
                SetTrafficRendering(true);
            }

            if (_input.Simulation.RunFireModule)
            {
                SetFireSpreadRendering(true);
            }

            if (_input.Simulation.RunSmokeModule)
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

        public void DisplayEvacGroupMap()
        {
            _simulationDomainVisualizer.SetSimulationPlaneTexture(Painter.GetEvacGroupTexture());
        }

        public void DisplayPopulationMask()
        {
            _simulationDomainVisualizer.SetSimulationPlaneTexture(Painter.GetPopulationMaskTexture());
        }

        public void DisplayTrafficUsageMap()
        {
            if(_trafficUsageMap == null)
            {
                CreateTrafficUsageMapTexture();
            }
            //SetDataPlaneTexture(_trafficUsageMap);
            //SetDomainDataPlane(true);
        }

        private void DisplayWUIAreaMap()
        {
            _fireDomainVisualizer.SetLCPPlaneTexture(Painter.GetWUIAreaTexture());
        }

        public void DisplayRandomIgnitionAreaMap()
        {
            _fireDomainVisualizer.SetLCPPlaneTexture(Painter.GetRandomIgnitionTexture());
        }

        public void DisplayInitialIgnitionMap()
        {
            _fireDomainVisualizer.SetLCPPlaneTexture(Painter.GetInitialIgnitionTexture());
        }

        Texture2D _trafficUsageMap;
        private void CreateTrafficUsageMapTexture()
        {
            float[,] data = ((SUMOModule)_engine.Simulation.TrafficModule).GetUsageMap();
            float maxData = ((SUMOModule)_engine.Simulation.TrafficModule).GetMaxUsage();
            _trafficUsageMap = new Texture2D(data.GetLength(0), data.GetLength(1));
            _trafficUsageMap.filterMode = FilterMode.Point;
            for (uint y = 0; y < data.GetLength(1); ++y)
            {
                for (uint x = 0; x < data.GetLength(0); ++x)
                {
                    float ratio = (float)data[x, y] / maxData;
                    Color color = Color.HSVToRGB(0.67f - 0.67f * ratio, 1.0f, 1.0f);
                    color.a = 1f;
                    if (data[x, y] == 0)
                    {
                        color.a = 0f;
                    }
                    _trafficUsageMap.SetPixel((int)x, (int)y, color);
                }
            }
            _trafficUsageMap.Apply();
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
            _renderSmokeDispersion = FireRenderer.ToggleSoot(_input);
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
            _renderFireSpread = FireRenderer.ToggleFire(_input);
            return _renderFireSpread;
        }        

        PREACTColor GetTrafficDensityColor(int cars)
        {
            float fraction = UnityEngine.Mathf.Lerp(0f, 1f, cars / 20f);
            PREACTColor c = PREACTColor.HSVToRGB(0.67f - 0.67f * fraction, 1.0f, 1.0f);

            return c;
        }

        List<TrafficCellData[]> trafficDensityData;
        List<int[]> peopleInCells;
        public List<Texture2D> outputTextures;
        
        public void UpdateInput(PREACTInput input)
        {
            _input = input;
            _painter.SetLCPData(_input.Fire.Data.LCPData);            
            _godCamera.SetInput(_input);
            _wuiGUI.UpdateInput(_input);            
            //this needs map and evac goals
            _simulationDomainVisualizer.SpawnEvacuationGoalMarkers(_input, _markerPrefab);
            UpdateMap();
            UpdateSimBorders();
        }

        public void UpdateMap()
        {
            LoadMapbox(_input);
        }

        public void NewLogMessage(string message)
        {
            if (Application.isEditor && !SuppressMessages) //&& || UnityEngine.Debug.isDebugBuild
            {
                Debug.Log(message);
            }
            _wuiGUI.NewMessage(message);
        }
        public void SimulationStarted()
        {
            _wuiGUI.SimulationStarted();
        }
        public void SimulationsFinished()
        {
            _wuiGUI.SimulationsFinished();
        }

        public void PauseSimulations()
        {
            _engine.PauseSimulations();
        }

        public string WorkingFolder { get => _engine.WorkingFolder; }
    }
}