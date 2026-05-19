//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;       
using UnityEngine;
using PREACT.Input;                    
using PREACT.Traffic;                          
using System.IO;
using PREACT;
using WUInity.UI;
using PREACT.Population;
using WUInity.Visualization;
using Assets.WUInity.GUI.DearIMGUI;
using Mapbox.Utils;

namespace WUInity
{
    public enum DataSampleMode { None, LocalGPW, PopulationMap, Relocated, TrafficDens, Paint, Farsite }

    [RequireComponent(typeof(EvacuationRenderer))]
    [RequireComponent(typeof(FireRenderer))]
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

        private Painter _painter;
        public Painter Painter{ get => _painter; }

        [SerializeField] private OverviewCamera _godCamera;

        [Header("Options")]
        public bool DeveloperMode = false;
        public bool SuppressMessages = false;
        public bool AutoLoadExample = true;
        [SerializeField] float _renderScale = 1.0f;
        public float RenderScale { get => _renderScale; }

        [Header("Prefabs")]        
        [SerializeField] private GameObject _destinationMarkerPrefab;
        [SerializeField] private GameObject _wildfireIgnitionMarkerPrefab;

        [Header("References")]
        [SerializeField] private Mapbox.Examples.QuadTreeCameraMovement _webMercatorCameraMovement;
        [SerializeField] private Mapbox.Unity.Map.AbstractMap _utmMap;
        public Mapbox.Unity.Map.AbstractMap UTMMap { get => _utmMap; }
        [SerializeField] private Mapbox.Unity.Map.AbstractMap _webMercatorMap;
        [SerializeField] private LineRenderer _simBorder;
        [SerializeField] private LineRenderer _boundingBoxRenderer;

        public DataSampleMode dataSampleMode = DataSampleMode.None;

        private PreactGUI _wuiGUI;
        PREACTInput _input;
        public PREACTInput PREACTInput { get => _input; }


        private FireRenderer _fireRenderer;
        private EvacuationRenderer _evacuationRenderer;
        private SimulationDomainVisualizerUnity _simulationDomainVisualizer;
        private FireDomainVisualizerUnity _fireDomainVisualizer;

        public SimulationDomainVisualizerUnity SimulationDomainVisualizer { get => _simulationDomainVisualizer; }
        public FireDomainVisualizerUnity FireDomainVisualizer { get => _fireDomainVisualizer; }
        
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
            _boundingBoxRenderer.gameObject.SetActive(false);

            //gui            
            _wuiGUI = FindAnyObjectByType<PreactGUI>();

            //map
            _utmMap.gameObject.SetActive(false);
            _webMercatorMap.gameObject.SetActive(true);
            SetWebMercatorMapInteraction(false);

            _engine = new Engine(this);
            _workingData = new PREACT.Runtime.WorkingData();
            _wuiGUI.SetManager(this, _engine, _workingData);  

            _painter = FindFirstObjectByType<Painter>();
            if (_painter == null)
            {
                GameObject g = new GameObject();
                g.transform.parent = transform;
                g.name = "WUI Painter";
                _painter = g.AddComponent<Painter>();
                g.SetActive(false);
            }

            _godCamera = FindFirstObjectByType<OverviewCamera>();
            if (_godCamera == null)
            {
                GameObject g = new GameObject();
                g.transform.parent = transform;
                g.name = "GodCamera";
                _godCamera = g.AddComponent<OverviewCamera>();
            }
            _godCamera.SetManager(this);

            _simulationDomainVisualizer = new SimulationDomainVisualizerUnity(transform);
            _fireDomainVisualizer = new FireDomainVisualizerUnity(transform);            
        }

        private void Start()
        {
            if (AutoLoadExample && DeveloperMode)
            {
                bool success = false;
                string file = Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "..\\Examples\\Development\\Development.wui");                
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
            if (Input.GetMouseButtonDown(0))
            {
                if (dataSampleMode != DataSampleMode.None)
                {
                    Plane _yPlane = new Plane(Vector3.up, 0f);
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    float enter = 0.0f;
                    if (_yPlane.Raycast(ray, out enter))
                    {
                        /*Vector3 hitPoint = ray.GetPoint(enter);
                        float xNorm = hitPoint.x / (float)_input.Simulation.DomainSize.x;
                        //xNorm = Mathf.Clamp01(xNorm);
                        int x = (int)(_input.Evacuation.Data.CellCount.x * xNorm);

                        float yNorm = hitPoint.z / (float)_input.Simulation.DomainSize.y;
                        //yNorm = Mathf.Clamp01(yNorm);
                        int y = (int)(_input.Evacuation.Data.CellCount.y * yNorm);
                        GetCellInfo(hitPoint, x, y);*/
                    }
                }                
            }

            //temp hack for changing height in smoke sim
            /*if (Input.GetKey(KeyCode.KeypadPlus))
            {
                print("Going up.");
                ((PREACT.Dispersion.AdvectDiffuse3D)_engine.Simulation.Hazards.Smoke).IncreaseOutputHeight();
            }
            else if (Input.GetKey(KeyCode.KeypadMinus))
            {
                print("Going down.");
                ((PREACT.Dispersion.AdvectDiffuse3D)_engine.Simulation.Hazards.Smoke).DecreaseOutputHeight();
            }*/

            //always update visuals, even when paused
            if (_engine.Simulation != null)
            {
                if (_engine.Simulation.State == Simulation.SimulationState.Running)
                {
                    if (!_visualsExist)
                    {
                        CreateVisualizers();
                    }
                    EvacuationRenderer.UpdateEvacuationRenderer(_renderHouseholds, _renderTraffic, _engine.Simulation.Evacuation.PedestrianModule, _engine.Simulation.Evacuation.TrafficModule);
                    FireRenderer.UpdateFireRenderer(_renderFireSpread, _renderSmokeDispersion, _engine.Simulation);
                }
            }   

            if(_pickingPos)
            {
                //collect click
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (_yPlane.Raycast(ray, out float enter))
                    {
                        Vector3 pos = ray.GetPoint(enter);
                        FinishPickPosOnMap(pos);
                    }
                }
            }            
            else if(_pickingBoundingBox)
            {
                //collect clicks
                if(Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (_yPlane.Raycast(ray, out float enter))
                    {
                        _webMercatorCameraMovement.enabled = false;
                        Vector3 pos = ray.GetPoint(enter);
                        _boundingBoxRenderer.SetPosition(0, new Vector3(pos.x, 10f, pos.z));
                        var clickLatLon = _webMercatorMap.WorldToGeoPosition(pos);
                        _clickLatLons[_clicks] = new PREACT.Math.Vector2d(clickLatLon.x, clickLatLon.y);
                        ++_clicks;
                        if (_clicks > 1)
                        {
                            FinishPickBoundingBoxOnMap();
                        }
                    }
                }                

                //update bounding box
                if (_clicks == 1)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    float enter;
                    if (_yPlane.Raycast(ray, out enter))
                    {
                        Vector3 pos = ray.GetPoint(enter);
                        _boundingBoxRenderer.SetPosition(1, new Vector3(pos.x, 10f, _boundingBoxRenderer.GetPosition(0).z));
                        _boundingBoxRenderer.SetPosition(2, new Vector3(pos.x, 10f, pos.z));
                        _boundingBoxRenderer.SetPosition(3, new Vector3(_boundingBoxRenderer.GetPosition(0).x, 10f, pos.z));
                    }
                }
            }            
        }

        Plane _yPlane = new Plane(Vector3.up, 0f);

        public void UpdateDestinationForVehicles(Vector3 boundingBoxPoint1, Vector3 boundingBoxPoint2, Vector3 manualDestination)
        {
            PREACT.Math.Vector2d lowerLeft = new PREACT.Math.Vector2d(Mathf.Min(boundingBoxPoint1.x, boundingBoxPoint2.x), Mathf.Min(boundingBoxPoint1.z, boundingBoxPoint2.z));
            PREACT.Math.Vector2d upperRight = new PREACT.Math.Vector2d(Mathf.Max(boundingBoxPoint1.x, boundingBoxPoint2.x), Mathf.Max(boundingBoxPoint1.z, boundingBoxPoint2.z));
            PREACT.Math.Vector2d simulationPos = new PREACT.Math.Vector2d(manualDestination.x, manualDestination.z);

            List<TrafficModuleVehicle> vehicles = _engine.Simulation.Evacuation.TrafficModule.GetVehiclesInBoundingBox(lowerLeft, upperRight);
            if(vehicles.Count > 0)
            {
                PREACT.Math.Vector2d wgs84 = _engine.Simulation.Input.Simulation.Data.GetWGS84FromSimulationPosition(simulationPos);
                PREACT.Evacuation.EvacuationDestination eD = _engine.Simulation.Evacuation.AddRuntimeDestination(wgs84);
                _engine.Simulation.Evacuation.TrafficModule.SetManualDestination(vehicles, simulationPos, eD);
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
            EvacuationRenderer.CreateBuffers(_input.PedestrianModule.Enabled, _input.TrafficModule.Enabled, _input.Simulation.DomainSize, _engine.Simulation.Evacuation.PedestrianModule);            

            _renderHouseholds = _input.PedestrianModule.Enabled;
            _renderTraffic = _input.TrafficModule.Enabled;

            //and then for fire rendering
            FireRenderer.CreateBuffers(_engine.Simulation);
            _renderFireSpread = _input.WildfireModule.Enabled;
            _renderSmokeDispersion = _input.SmokeModule.Enabled;

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

        public void UpdateSimBorders()
        {
            _simBorder.gameObject.SetActive(true);
            Vector3 upOffset = Vector3.up * 50f;
            _simBorder.SetPosition(0, Vector3.zero + upOffset);
            _simBorder.SetPosition(1, _simBorder.GetPosition(0) + Vector3.right * (float)_input.Simulation.DomainSize.x);
            _simBorder.SetPosition(2, _simBorder.GetPosition(1) + Vector3.forward * (float)_input.Simulation.DomainSize.y);
            _simBorder.SetPosition(3, _simBorder.GetPosition(2) - Vector3.right * (float)_input.Simulation.DomainSize.x);
            _simBorder.SetPosition(4, _simBorder.GetPosition(0));   
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
            /*else if (x < 0 || x > _input.Evacuation.Data.CellCount.x || y < 0 || y > _input.Evacuation.Data.CellCount.y)
            {
                //dataSampleString = "Outside of data range.";
                return;
            }*/
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
                /*else if (dataSampleMode == DataSampleMode.TrafficDens)
                {
                    int people = currentPeopleInCells[x + y * _input.Evacuation.Data.CellCount.x];
                    dataSampleString = "People: " + people;
                    if (currenttrafficDensityData != null && currenttrafficDensityData[x + y * _input.Evacuation.Data.CellCount.x] != null)
                    {
                        int peopleInCars = currenttrafficDensityData[x + y * _input.Evacuation.Data.CellCount.x].peopleCount;
                        int cars = currenttrafficDensityData[x + y * _input.Evacuation.Data.CellCount.x].carCount;

                        dataSampleString += " | People in cars: " + peopleInCars + " (Cars: " + cars + "). Total people " + (people + peopleInCars);
                    }
                }*/
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
            if(_input.TrafficModule.Active)
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
            if(_input.PedestrianModule.Enabled)
            {
                SetHouseholdRendering(true);
            }

            if (_input.TrafficModule.Enabled)
            {
                SetTrafficRendering(true);
            }

            if (_input.WildfireModule.Enabled)
            {
                SetFireSpreadRendering(true);
            }

            if (_input.SmokeModule.Enabled)
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
            double[,] data = ((SUMOModule)_engine.Simulation.Evacuation.TrafficModule).GetUsageMap();
            double maxData = ((SUMOModule)_engine.Simulation.Evacuation.TrafficModule).GetMaxUsage();
            _trafficUsageMap = new Texture2D(data.GetLength(0), data.GetLength(1));
            _trafficUsageMap.filterMode = FilterMode.Point;
            for (uint y = 0; y < data.GetLength(1); ++y)
            {
                for (uint x = 0; x < data.GetLength(0); ++x)
                {
                    float ratio = (float)(data[x, y] / maxData);
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

        public List<Texture2D> outputTextures;
        
        public void UpdateInput(PREACTInput input)
        {
            _input = input;
            _painter.SetLCPData(_input.WildfireModule.Data.LandscapeData);            
            _godCamera.SetInput(_input);
            _wuiGUI.SetInput(_input);            
            //this needs map and evac goals
            _simulationDomainVisualizer.SpawnEvacuationGoalMarkers(_input, _destinationMarkerPrefab);
            _simulationDomainVisualizer.SpawnWildfireIgnitionMarkers(_input, _wildfireIgnitionMarkerPrefab);

            //map stuff
            ShowUTMMap();
            LoadUTMMap(_input);
            UpdateSimBorders();
        }

        public void UpdateDestinations(List<PREACT.Evacuation.EvacuationDestination> destinations)
        {
            _simulationDomainVisualizer.SpawnEvacuationGoalMarkers(_input, destinations, _destinationMarkerPrefab);
        }

        public void LoadUTMMap(PREACTInput input)
        {
            //Mapbox: calculate the amount of grids needed based on zoom level, coord and size
            Mapbox.Unity.Map.MapOptions mOptions = _utmMap.Options; // new Mapbox.Unity.Map.MapOptions();

            mOptions.locationOptions.latitudeLongitude = "" + input.Simulation.LowerLeftLatLon.x + "," + input.Simulation.LowerLeftLatLon.y;
            mOptions.locationOptions.zoom = input.Map.ZoomLevel;
            mOptions.extentOptions.extentType = Mapbox.Unity.Map.MapExtentType.CameraBounds;// Mapbox.Unity.Map.MapExtentType.RangeAroundCenter;
            /*mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.west = 0;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.south = 0;
            //https://wiki.openstreetmap.org/wiki/Zoom_levels
            double degreesPerTile = 360.0 / (Mathf.Pow(2.0f, mOptions.locationOptions.zoom));
            PREACT.Math.Vector2d mapDegrees = LocalGPWData.SizeToDegrees(input.Simulation.LowerLeftLatLon, input.Simulation.DomainSize);
            int tilesX = (int)(mapDegrees.x / degreesPerTile) + 1;
            int tilesY = (int)(mapDegrees.y / (degreesPerTile * Mathf.Cos((Mathf.PI / 180.0f) * (float)input.Simulation.LowerLeftLatLon.x))) + 1;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.east = tilesX;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.north = tilesY;*/
            mOptions.placementOptions.placementType = Mapbox.Unity.Map.MapPlacementType.AtLocationCenter;
            mOptions.placementOptions.snapMapToZero = false;
            mOptions.scalingOptions.scalingType = Mapbox.Unity.Map.MapScalingType.WorldScale;

            if (!_utmMap.IsAccessTokenValid)
            {
                Engine.Message(null, Engine.LogType.SimulationError, "Mapbox token not valid.");
                return;
            }

            Engine.Message(null, Engine.LogType.Log, "Starting to load Mapbox map.");
            _utmMap.Initialize(new Mapbox.Utils.Vector2d(input.Simulation.LowerLeftLatLon.x, input.Simulation.LowerLeftLatLon.y), input.Map.ZoomLevel);
            Engine.Message(null, Engine.LogType.Log, "Map loaded succesfully.");

            //do warping to better fit UTM
            for (int i = 0; i < _utmMap.transform.childCount; ++i)
            {
                Mapbox.Unity.MeshGeneration.Data.UnityTile tile = _utmMap.transform.GetChild(i).GetComponent<Mapbox.Unity.MeshGeneration.Data.UnityTile>();
                if (tile != null)
                {
                    Vector3[] vertices = tile.GetComponent<MeshFilter>().mesh.vertices;
                    for (int v = 0; v < vertices.Length; ++v)
                    {
                        Vector3 worldPos = tile.transform.TransformPoint(vertices[v]);
                        Vector2d wgs84Pos = _utmMap.WorldToGeoPosition(worldPos); //GeoConversions.MetersToLatLon(new Vector2d(worldPos.x, worldPos.z) + WUIEngine.RUNTIME_DATA.Simulation.CenterMercator);
                        PREACT.Math.Vector2d utmSimPos = input.Simulation.Data.GetSimulationPosition(new PREACT.Math.Vector2d(wgs84Pos.x, wgs84Pos.y));
                        Vector3 newWorldPos = new Vector3((float)utmSimPos.x, 0f, (float)utmSimPos.y);
                        vertices[v] = tile.transform.InverseTransformPoint(newWorldPos);
                    }
                    tile.GetComponent<MeshFilter>().mesh.SetVertices(vertices);
                    tile.GetComponent<MeshFilter>().mesh.RecalculateBounds();
                }
            }
            //MAP.transform.localScale = new Vector3((float)WUIEngine.RUNTIME_DATA.Simulation.MercatorToUtmScale.x, 1.0f, (float)WUIEngine.RUNTIME_DATA.Simulation.MercatorToUtmScale.y);  
        }

        public void NewLogMessage(string message)
        {
            if (Application.isEditor && !SuppressMessages)
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

        private bool _pickingBoundingBox;
        private bool _pickingPos;
        private int _clicks = 0;
        private PREACT.Math.Vector2d[] _clickLatLons = new PREACT.Math.Vector2d[2];
        private System.Action<PREACT.Math.Vector2d[]> _onClicks;
        private System.Action<PREACT.Math.Vector2d> _onClick;
        public void PickBoundingBoxOnMap(System.Action<PREACT.Math.Vector2d[]> clicks)
        {
            _onClicks = clicks;
            _clicks = 0;
            SetWebMercatorMapInteraction(true);
            _pickingBoundingBox = true;
            _boundingBoxRenderer.gameObject.SetActive(true);
            _boundingBoxRenderer.startWidth = 0.5f;
            _boundingBoxRenderer.endWidth = 0.5f;
            for (int i = 0; i < _boundingBoxRenderer.positionCount; ++i)
            {
                _boundingBoxRenderer.SetPosition(i, Vector3.zero - Vector3.down * 100);
            }
        }
        private void FinishPickBoundingBoxOnMap()
        {
            _boundingBoxRenderer.gameObject.SetActive(false);
            SetWebMercatorMapInteraction(false);
            _pickingBoundingBox = false;
            _onClicks(_clickLatLons);
            _onClicks = null;
        }

        public void PickPosOnMap(System.Action<PREACT.Math.Vector2d> onClick)
        {
            _pickingPos = true;
            _onClick = onClick;
        }

        private void FinishPickPosOnMap(Vector3 clickPos)
        {
            _pickingPos = false;
            _onClick(new PREACT.Math.Vector2d(clickPos.x, clickPos.z));
            _onClick = null;
        }


        public void SetWebMercatorMapInteraction(bool canInteract)
        {
            _webMercatorCameraMovement.enabled = canInteract;
        }

        public void ShowUTMMap()
        {
            _boundingBoxRenderer.gameObject.SetActive(false);
            _webMercatorMap.gameObject.SetActive(false);
            _utmMap.gameObject.SetActive(true);
        }

        public void ShowWebMercatorMap()
        {
            _boundingBoxRenderer.gameObject.SetActive(false);
            _simBorder.gameObject.SetActive(false);
            _godCamera.SetToWebMercatorMode();
            _webMercatorMap.gameObject.SetActive(true);
            _utmMap.gameObject.SetActive(false);
        }
    }
}