//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;       
using UnityEngine;
using PREACT.Evacuation;
using PREACT.Pedestrian;                     
using PREACT.Traffic;                          
using System.IO;
using PREACT;
using WUInity.UI;
using PREACT.Population;

namespace WUInity
{
    public enum DataSampleMode { None, LocalGPW, PopulationMap, Relocated, TrafficDens, Paint, Farsite }

    [RequireComponent(typeof(WUInityGUI))]
    [RequireComponent(typeof(Visualization.EvacuationRenderer))]
    [RequireComponent(typeof(Visualization.FireRenderer))]
    public class WUInityManager : MonoBehaviour, ExternalManager                     
    {
        public static WUInityManager INSTANCE
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<WUInityManager>();
                    if (_instance == null)
                    {
                        GameObject g = new GameObject();
                        _instance = g.AddComponent<WUInityManager>();
                    }
                }
                return _instance;
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

        public Visualization.FireRenderer FIRE_VISUALS
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
        public Mapbox.Unity.Map.AbstractMap Map
        {
            get
            {                
                return _mapboxMap;
            }
        }

        private Painter _painter;
        public Painter Painter
        {
            get
            {
                return _painter;
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

        //never directly call these, always use singletons (except once when setting input)
        private static WUInityManager _instance;

        //private Farsite.FarsiteViewer _farsiteViewer;

        private WUInityGUI _wuiGUI;
       
        
        private Visualization.FireRenderer _fireRenderer;
        private Visualization.EvacuationRenderer _evacuationRenderer;

        MeshRenderer _domainDataMeshRenderer;
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

        Engine _engine;
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
                _godCamera = FindFirstObjectByType<GodCamera>();
            }

            _engine = new Engine(this);

            //gui
            _wuiGUI = INSTANCE.GetComponent<WUInityGUI>();
            if (_wuiGUI == null)
            {
                gameObject.AddComponent<WUInityGUI>();
            }
            _wuiGUI.SetEngines(this, _engine);

            //map
            _mapboxMap = FindFirstObjectByType<Mapbox.Unity.Map.AbstractMap>();
            if (_mapboxMap == null)
            {
                GameObject g = new GameObject();
                g.name = "Mapbox Map";
                g.transform.parent = INSTANCE.transform;
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
        }

        private void Start()
        {
            if (AutoLoadExample && DeveloperMode)
            {
                string path = Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "example\\example.wui");
                if (File.Exists(path))
                {
                    PREACT.IO.Input.LoadFromDisk(_engine, path);
                }
                else
                {
                    print("Could not find input file for auto load in path " + path);
                }
            }
        }

        private void OnApplicationQuit()
        {
            if (_engine != null)
            {
                _engine.Close(false);
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
            Mapbox.Unity.Map.MapOptions mOptions = Map.Options; // new Mapbox.Unity.Map.MapOptions();

            mOptions.locationOptions.latitudeLongitude = "" + _engine.Input.Simulation.LowerLeftLatLon.x + "," + _engine.Input.Simulation.LowerLeftLatLon.y;
            mOptions.locationOptions.zoom = _engine.Input.Map.ZoomLevel;
            mOptions.extentOptions.extentType = Mapbox.Unity.Map.MapExtentType.RangeAroundCenter;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.west = 0;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.south = 0;
            //https://wiki.openstreetmap.org/wiki/Zoom_levels
            double degreesPerTile = 360.0 / (Mathf.Pow(2.0f, mOptions.locationOptions.zoom));
            PREACT.Utility.Math.Vector2d mapDegrees = LocalGPWData.SizeToDegrees(_engine.Input.Simulation.LowerLeftLatLon, _engine.Input.Simulation.DomainSize);
            int tilesX = (int)(mapDegrees.x / degreesPerTile) + 1;
            int tilesY = (int)(mapDegrees.y / (degreesPerTile * Mathf.Cos((Mathf.PI / 180.0f) * (float)_engine.Input.Simulation.LowerLeftLatLon.x))) + 1;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.east = tilesX;
            mOptions.extentOptions.defaultExtents.rangeAroundCenterOptions.north = tilesY;
            mOptions.placementOptions.placementType = Mapbox.Unity.Map.MapPlacementType.AtLocationCenter;
            mOptions.placementOptions.snapMapToZero = true;
            mOptions.scalingOptions.scalingType = Mapbox.Unity.Map.MapScalingType.WorldScale;

            if (!Map.IsAccessTokenValid)
            {
                _engine.Message(null, Engine.LogType.SimError, "Mapbox token not valid.");
                return false;
            }

            _engine.Message(null, Engine.LogType.Log, "Starting to load Mapbox map.");
            Map.Initialize(new Mapbox.Utils.Vector2d(_engine.Input.Simulation.LowerLeftLatLon.x, _engine.Input.Simulation.LowerLeftLatLon.y), _engine.Input.Map.ZoomLevel);
            _engine.Message(null, Engine.LogType.Log, "Map loaded succesfully.");

            //do adjustement to better fit UTM
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
                        Vector3 newWorldPos = new Vector3((float)(utmPos.Easting - _engine.RuntimeData.Simulation.UTMOrigin.x), 0f, (float)(utmPos.Northing - _engine.RuntimeData.Simulation.UTMOrigin.y));
                        vertices[v] = tile.transform.InverseTransformPoint(newWorldPos);
                    }
                    tile.GetComponent<MeshFilter>().mesh.SetVertices(vertices);
                    tile.GetComponent<MeshFilter>().mesh.RecalculateBounds();
                }
            }
            //MAP.transform.localScale = new Vector3((float)WUIEngine.RUNTIME_DATA.Simulation.MercatorToUtmScale.x, 1.0f, (float)WUIEngine.RUNTIME_DATA.Simulation.MercatorToUtmScale.y);   

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
                    float xNorm = hitPoint.x / (float)_engine.Input.Simulation.DomainSize.x;
                    //xNorm = Mathf.Clamp01(xNorm);
                    int x = (int)(_engine.RuntimeData.Evacuation.CellCount.x * xNorm);

                    float yNorm = hitPoint.z / (float)_engine.Input.Simulation.DomainSize.y;
                    //yNorm = Mathf.Clamp01(yNorm);
                    int y = (int)(_engine.RuntimeData.Evacuation.CellCount.y * yNorm);
                    GetCellInfo(hitPoint, x, y);
                }
            }    

            //always update visuals, even when paused
            if(_engine.RuntimeData != null)
            {
                if (_engine.Simulation.State == Simulation.SimulationState.Running) // !WUIEngine.RUNTIME_DATA.Simulation.MultipleSimulations && 
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
                //UpdateOSMBorder();
            }                
        }

        public void RunSimulation()
        {
            _visualsExist = false;
            SetSampleMode(DataSampleMode.TrafficDens);
            _engine.RunSimulations();
        }

        bool _visualsExist = false;
        public void CreateVisualizers()
        {
            //this needs to be done AFTER simulation has started since we need some data from the sim
            //fix everything for evac rendering
            EVAC_VISUALS.CreateBuffers(_engine.Input.Simulation.RunPedestrianModule, _engine.Input.Simulation.RunTrafficModule);            

            _renderHouseholds = _engine.Input.Simulation.RunPedestrianModule;
            _renderTraffic = _engine.Input.Simulation.RunTrafficModule;

            //and then for fire rendering
            FIRE_VISUALS.CreateBuffers(_engine.Input.Simulation.RunFireModule, _engine.Input.Simulation.RunSmokeModule);
            _renderFireSpread = _engine.Input.Simulation.RunFireModule;
            _renderSmokeDispersion = _engine.Input.Simulation.RunSmokeModule;

            _visualsExist = true;

            ActivateSuitableVisuals();
        }

        public void RunAllCasesInFolder(string folder)
        {
            string[] inputFiles = Directory.GetFiles(folder, "*.wui");
            for (int i = 0; i < inputFiles.Length; i++)
            {
                PREACT.IO.Input.LoadFromDisk(_engine, inputFiles[i]);
                _engine.Input.Simulation.Id = Path.GetFileNameWithoutExtension(inputFiles[i]);
                _engine.RuntimeData.Simulation.MultipleSimulations = true;
                _engine.RuntimeData.Simulation.NumberOfRuns = 100;
                RunSimulation();
            }
        }

        public void StopSimulation()
        {
            HideAllRuntimeVisuals();
            _engine.Simulation.Stop("STOP: Stopped simulation as requested by user.", false);
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
                _simBorder.SetPosition(1, _simBorder.GetPosition(0) + Vector3.right * (float)_engine.Input.Simulation.DomainSize.x);
                _simBorder.SetPosition(2, _simBorder.GetPosition(1) + Vector3.forward * (float)_engine.Input.Simulation.DomainSize.y);
                _simBorder.SetPosition(3, _simBorder.GetPosition(2) - Vector3.right * (float)_engine.Input.Simulation.DomainSize.x);
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
            if (dataSampleMode == DataSampleMode.LocalGPW)
            {
                if (_engine.RuntimeData.Population.LocalGPWData != null && _engine.RuntimeData.Population.LocalGPWData.density != null && _engine.RuntimeData.Population.LocalGPWData.density.Length > 0)
                {
                    if (_engine.RuntimeData.Population.Visualizer.IsDataPlaneActive())
                    {
                        float xCellSize = (float)(_engine.RuntimeData.Population.LocalGPWData.realWorldSize.x / _engine.RuntimeData.Population.LocalGPWData._cells.x);
                        float yCellSize = (float)(_engine.RuntimeData.Population.LocalGPWData.realWorldSize.y / _engine.RuntimeData.Population.LocalGPWData._cells.y);
                        double cellArea = xCellSize * yCellSize / (1000000d);
                        dataSampleString = "GPW people count: " + System.Convert.ToInt32(_engine.RuntimeData.Population.LocalGPWData.GetDensitySimulationSpace(new PREACT.Utility.Math.Vector2d(pos.x, pos.z)) * cellArea);
                    }
                    else
                    {
                        dataSampleString = "GPW data not visible, activate to sample data.";
                    }
                }
            }
            else if (x < 0 || x > _engine.RuntimeData.Evacuation.CellCount.x || y < 0 || y > _engine.RuntimeData.Evacuation.CellCount.y)
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
                if (dataSampleMode == DataSampleMode.PopulationMap)
                {
                    dataSampleString = "Interpolated people count: " + _engine.RuntimeData.Population.PopulationMap.GetPeopleCount(x, y);
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
                    int people = currentPeopleInCells[x + y * _engine.RuntimeData.Evacuation.CellCount.x];
                    dataSampleString = "People: " + people;
                    if (currenttrafficDensityData != null && currenttrafficDensityData[x + y * _engine.RuntimeData.Evacuation.CellCount.x] != null)
                    {
                        int peopleInCars = currenttrafficDensityData[x + y * _engine.RuntimeData.Evacuation.CellCount.x].peopleCount;
                        int cars = currenttrafficDensityData[x + y * _engine.RuntimeData.Evacuation.CellCount.x].carCount;

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
                _engine.Message(null, Engine.LogType.Warning, "Paint mode not set correctly.");
            }
            dataSampleMode = DataSampleMode.Paint;

            if(fireEdit)
            {
                SetDomainDataPlane(false);
                SetFireDataPlane(true);
            }
            else
            {
                SetDomainDataPlane(true);
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

        private MeshRenderer CreateDataPlane(Texture2D tex, string name, PREACT.Utility.Math.Vector2d size, PREACT.Utility.Math.Vector2d originOffset)
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

            float width = (float)size.x;
            float length = (float)size.y;


            Vector3 offset = new Vector3((float)originOffset.x, 0f, (float)originOffset.y);

            Visualization.VisualizeUtilities.CreateSimplePlane(mesh, width, length, 0.0f, offset);

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

            _goalMarkers = new GameObject[_engine.RuntimeData.Evacuation.Destinations.Count];
            for (int i = 0; i < _engine.RuntimeData.Evacuation.Destinations.Count; i++)
            {
                EvacuationDestination eG = _engine.RuntimeData.Evacuation.Destinations[i];
                _goalMarkers[i] = Instantiate<GameObject>(_markerPrefab);
                PREACT.Utility.LatLngUTMConverter.UTMResult utmPos = PREACT.Utility.LatLngUTMConverter.WGS84.convertLatLngToUtm(eG._latLon.x, eG._latLon.y);
                PREACT.Utility.Math.Vector2d pos = new PREACT.Utility.Math.Vector2d(utmPos.Easting, utmPos.Northing) - _engine.RuntimeData.Simulation.UTMOrigin;

                float scale = 0.02f * (float)_engine.Input.Simulation.DomainSize.y;
                _goalMarkers[i].transform.localScale = new Vector3(scale, 100f, scale);
                _goalMarkers[i].transform.position = new Vector3((float)pos.x, 0f, (float)pos.y);
                MeshRenderer mR = _goalMarkers[i].GetComponentInChildren<MeshRenderer>();
                mR.material.color = eG._color.UnityColor;
            }            
        }        
                
        TrafficCellData[] currenttrafficDensityData;
        int[] currentPeopleInCells;
        /*public void DisplayClosestDensityData(float time)
        {
            if(_engine.Input.Simulation.RunTrafficModule)
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
            if(_engine.Input.Simulation.RunPedestrianModule)
            {
                SetHouseholdRendering(true);
            }

            if (_engine.Input.Simulation.RunTrafficModule)
            {
                SetTrafficRendering(true);
            }

            if (_engine.Input.Simulation.RunFireModule)
            {
                SetFireSpreadRendering(true);
            }

            if (_engine.Input.Simulation.RunSmokeModule)
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

        public void DisplayPopulationMap()
        {
            SetDataPlaneTexture((Texture2D)_engine.RuntimeData.Population.Visualizer.GetPopulationTexture());
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

        public void DisplayEvacGroupMap()
        {
            SetDataPlaneTexture(Painter.GetEvacGroupTexture());
        }

        public void DisplayPopulationMask()
        {
            SetDataPlaneTexture(Painter.GetPopulationMaskTexture());
        }

        public void DisplayTrafficUsageMap()
        {
            if(_trafficUsageMap == null)
            {
                CreateTrafficUsageMapTexture();
            }
            SetDataPlaneTexture(_trafficUsageMap);
            SetDomainDataPlane(true);
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

        public  void SetDomainDataPlane(bool setActive)
        {
            if (_domainDataMeshRenderer != null)
            {
                _domainDataMeshRenderer.gameObject.SetActive(setActive);
            }
        }

        public void SetFireDataPlane(bool setActive)
        {
            if (_fireDataPlaneMeshRenderer != null)
            {
                _fireDataPlaneMeshRenderer.gameObject.SetActive(setActive);
            }
        }

        public bool ToggleDomainDataPlane()
        {
            if (_domainDataMeshRenderer != null)
            {
                _domainDataMeshRenderer.gameObject.SetActive(!_domainDataMeshRenderer.gameObject.activeSelf);
                return _domainDataMeshRenderer.gameObject.activeSelf;
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
            MeshRenderer activeMeshRenderer = _domainDataMeshRenderer;
            PREACT.Utility.Math.Vector2int cellCount = _engine.RuntimeData.Evacuation.CellCount;
            PREACT.Utility.Math.Vector2d size = _engine.Input.Simulation.DomainSize;
            PREACT.Utility.Math.Vector2d offset = PREACT.Utility.Math.Vector2d.zero;
            string name = "Evac Data Plane";
            if (fireMeshMode)
            {
                activeMeshRenderer = _fireDataPlaneMeshRenderer;
                cellCount = new PREACT.Utility.Math.Vector2int(_engine.RuntimeData.Fire.LCPData.GetCellCountX(), _engine.RuntimeData.Fire.LCPData.GetCellCountY());
                size = _engine.RuntimeData.Fire.LCPData.GetSize();
                offset = _engine.RuntimeData.Fire.LCPData.OriginOffset;
                name = "Fire Data Plane";
            }

            //make sure it exists, else create
            if (activeMeshRenderer == null)
            {
                activeMeshRenderer = CreateDataPlane(tex, name, size, offset);
                if(fireMeshMode)
                {
                    _fireDataPlaneMeshRenderer = activeMeshRenderer;
                    _fireDataPlane = activeMeshRenderer.gameObject;
                }
                else
                {
                    _domainDataMeshRenderer = activeMeshRenderer;
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
        /*public void SaveTransientDensityData(float time, List<MacroVehicle> carsInSystem, List<MacroVehicle> carsOnHold)
        {
            //first time
            if (trafficDensityData == null)
            {
                trafficDensityData = new List<TrafficCellData[]>();
                outputTextures = new List<Texture2D>();
                peopleInCells = new List<int[]>();
            }
            //if new data interval
            int outputIndex = (int)(time - _engine.Simulation.StartTime) / 600;
            if (outputIndex > trafficDensityData.Count - 1)
            {
                trafficDensityData.Add(new TrafficCellData[_engine.RuntimeData.Evacuation.CellCount.x * _engine.RuntimeData.Evacuation.CellCount.y]);

                for (int i = 0; i < carsInSystem.Count; i++)
                {
                    System.Numerics.Vector4 posAndSpeed = carsInSystem[i].GetWorldPositionSpeedCarID(false);

                    int x = (int)(posAndSpeed.X / _engine.Input.Evacuation.PaintCellSize);
                    int y = (int)(posAndSpeed.Y / _engine.Input.Evacuation.PaintCellSize);

                    //outside of mapped data
                    if (x < 0 || x > _engine.RuntimeData.Evacuation.CellCount.x - 1 || y < 0 || y > _engine.RuntimeData.Evacuation.CellCount.y - 1)
                    {
                        continue;
                    }

                    //add or update data
                    if (trafficDensityData[outputIndex][x + y * _engine.RuntimeData.Evacuation.CellCount.x] == null)
                    {
                        trafficDensityData[outputIndex][x + y * _engine.RuntimeData.Evacuation.CellCount.x] = new TrafficCellData();
                        trafficDensityData[outputIndex][x + y * _engine.RuntimeData.Evacuation.CellCount.x].carCount = 1;
                        trafficDensityData[outputIndex][x + y * _engine.RuntimeData.Evacuation.CellCount.x].peopleCount = (int)carsInSystem[i].NumberOfPeople;
                    }
                    else
                    {
                        trafficDensityData[outputIndex][x + y * _engine.RuntimeData.Evacuation.CellCount.x].carCount += 1;
                        trafficDensityData[outputIndex][x + y * _engine.RuntimeData.Evacuation.CellCount.x].peopleCount += (int)carsInSystem[i].NumberOfPeople;
                    }
                }

                for (int i = 0; i < carsOnHold.Count; i++)
                {
                    System.Numerics.Vector4 posAndSpeed = carsOnHold[i].GetWorldPositionSpeedCarID(false);

                    int x = (int)(posAndSpeed.X / _engine.Input.Evacuation.PaintCellSize);
                    int y = (int)(posAndSpeed.Y / _engine.Input.Evacuation.PaintCellSize);

                    //outside of mapped data
                    if (x < 0 || x > _engine.RuntimeData.Evacuation.CellCount.x - 1 || y < 0 || y > _engine.RuntimeData.Evacuation.CellCount.y - 1)
                    {
                        continue;
                    }

                    //add or update data
                    if (trafficDensityData[outputIndex][x + y * _engine.RuntimeData.Evacuation.CellCount.x] == null)
                    {
                        trafficDensityData[outputIndex][x + y * _engine.RuntimeData.Evacuation.CellCount.x] = new TrafficCellData();
                        trafficDensityData[outputIndex][x + y * _engine.RuntimeData.Evacuation.CellCount.x].carCount = 1;
                        trafficDensityData[outputIndex][x + y * _engine.RuntimeData.Evacuation.CellCount.x].peopleCount = (int)carsOnHold[i].NumberOfPeople;
                    }
                    else
                    {
                        trafficDensityData[outputIndex][x + y * _engine.RuntimeData.Evacuation.CellCount.x].carCount += 1;
                        trafficDensityData[outputIndex][x + y * _engine.RuntimeData.Evacuation.CellCount.x].peopleCount += (int)carsOnHold[i].NumberOfPeople;
                    }
                }

                //create texture
                Texture2D tex = new Texture2D(_engine.RuntimeData.Evacuation.CellCount.x, _engine.RuntimeData.Evacuation.CellCount.y);
                tex.filterMode = FilterMode.Point;

                for (int y = 0; y < _engine.RuntimeData.Evacuation.CellCount.y; ++y)
                {
                    for (int x = 0; x < _engine.RuntimeData.Evacuation.CellCount.x; ++x)
                    {
                        WUIEngineColor c = WUIEngineColor.grey;
                        c.a = 0.0f;
                        int count = 0;
                        if (trafficDensityData[outputIndex][x + y * _engine.RuntimeData.Evacuation.CellCount.x] != null)
                        {
                            count += trafficDensityData[outputIndex][x + y * _engine.RuntimeData.Evacuation.CellCount.x].carCount;
                        }
                        //count += peopleInCells[outputIndex][x + y * WUI_engine.Simulation.GetCellCount.x];
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
                File.WriteAllBytes(Path.Combine(Engine.OutputFolder, "trafficDens_" + (int)time + "s.png"), bytes);
            }
        }*/

        public void InputHasChanged()
        {
            _wuiGUI.SetDirty();
            //this needs map and evac goals
            SpawnEvacuationGoalMarkers();
        }

        public void UpdateMap()
        {
            LoadMapbox();
            UpdateSimBorders();
            WUICamera.SetCameraStartPosition(_engine.Input.Simulation.DomainSize);
        }
    }
}