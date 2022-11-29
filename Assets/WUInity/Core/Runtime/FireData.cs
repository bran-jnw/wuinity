using UnityEngine;
using System.IO;
using Itinero;
using Itinero.IO.Osm;
using Itinero.Osm.Vehicles;
using OsmSharp.Streams;
using WUInity.Population;
using WUInity.Fire;
using System.Xml.Linq;
using WUInity.Visualization;

namespace WUInity.Runtime
{
    public class FireData
    {
        public bool[] WuiAreaIndices;
        public bool[] RandomIgnitionIndices;
        public bool[] InitialIgnitionIndices;
        public bool[] TriggerBufferIndices;

        LCPData _lcpData;
        public LCPData LCPData
        {
            get
            {
                return _lcpData;
            }
        }

        FuelModelInput _fuelModelsData;
        public FuelModelInput FuelModelsData
        {
            get
            {
                return _fuelModelsData;
            }
        }

        private IgnitionPoint[] _ignitionPoints;
        public IgnitionPoint[] IgnitionPoints
        {
            get
            {                
                return _ignitionPoints;
            }
        }

        private InitialFuelMoistureList _initialFuelMoistureData;
        public InitialFuelMoistureList InitialFuelMoistureData
        {
            get
            {               
                return _initialFuelMoistureData;
            }
        }

        private WeatherInput _weatherInput;
        public WeatherInput WeatherInput
        {
            get
            {                
                return _weatherInput;
            }
        }

        private WindInput _windInput;
        public WindInput WindInput
        {
            get
            {                
                return _windInput;
            }
        }

        public void LoadAll()
        {
            LoadLCPFile(Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.Fire.lcpFile), false);
            LoadFuelModelsInput(Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.Fire.fuelModelsFile), false);
            LoadIgnitionPoints(Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.Fire.ignitionPointsFile), false);
            LoadInitialFuelMoistureData(Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.Fire.initialFuelMoistureFile), false);
            LoadWeatherInput(Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.Fire.weatherFile), false);
            LoadWindInput(Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.Fire.windFile), false);
            LoadGraphicalFireInput(Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.Fire.graphicalFireInputFile), false);
        }

        public bool LoadLCPFile(string path, bool updateInputFile)
        {
            bool success;
            _lcpData = new LCPData(path);
            success = !_lcpData.CantAllocLCP;

            if (success)
            {
                int[] fuelNrs = _lcpData.GetExisitingFuelModelNumbers();
                string message = "LOG: Present fuel model numbers are ";
                for (int i = 0; i < fuelNrs.Length; i++)
                {
                    message += fuelNrs[i].ToString();
                    if (i < fuelNrs.Length - 1)
                    {
                        message += ", ";
                    }
                    else
                    {
                        message += ".";
                    }
                }

                WUInity.LOG(message);
            }

            WUInity.DATA_STATUS.LcpLoaded = success;
            if (success && updateInputFile)
            {
                WUInity.INPUT.Fire.lcpFile = Path.GetFileName(path);
                WUInityInput.SaveInput();
            }

            return success;
        }

        public bool LoadFuelModelsInput(string path, bool updateInputFile)
        {
            bool success;
            _fuelModelsData = new FuelModelInput();
            success = _fuelModelsData.LoadFuelModelInputFile(path);

            WUInity.DATA_STATUS.FuelModelsLoaded = success;
            if(success && updateInputFile)
            {
                WUInity.INPUT.Fire.fuelModelsFile = Path.GetFileName(path);
                WUInityInput.SaveInput();
            }

            return success;
        }

        public bool LoadIgnitionPoints(string path, bool updateInputFile)
        {
            bool success;
            _ignitionPoints = IgnitionPoint.LoadIgnitionPointsFile(path, out success);
            if (success && updateInputFile)
            {
                WUInity.INPUT.Fire.ignitionPointsFile = Path.GetFileName(path);
                WUInityInput.SaveInput();
            }

            return success;
        }

        public bool LoadInitialFuelMoistureData(string path, bool updateInputFile)
        {
            bool success;
            _initialFuelMoistureData = InitialFuelMoistureList.LoadInitialFuelMoistureDataFile(out success);
            if (success && updateInputFile)
            {
                WUInity.INPUT.Fire.initialFuelMoistureFile = Path.GetFileName(path);
                WUInityInput.SaveInput();
            }

            return success;
        }

        public bool LoadWeatherInput(string path, bool updateInputFile)
        {
            bool success;
            _weatherInput = WeatherInput.LoadWeatherInputFile(out success);
            if (success && updateInputFile)
            {
                WUInity.INPUT.Fire.weatherFile = Path.GetFileName(path);
                WUInityInput.SaveInput();
            }

            return success;
        }

        public bool LoadWindInput(string path, bool updateInputFile)
        {
            bool success;
            _windInput = WindInput.LoadWindInputFile(out success);
            if (success && updateInputFile)
            {
                WUInity.INPUT.Fire.windFile = Path.GetFileName(path);
                WUInityInput.SaveInput();
            }

            return success;
        }

        public bool LoadGraphicalFireInput(string path, bool updateInputFile)
        {
            bool success;
            GraphicalFireInput.LoadGraphicalFireInput(out success);
            if (success && updateInputFile)
            {
                WUInity.INPUT.Fire.graphicalFireInputFile = Path.GetFileName(path);
                WUInityInput.SaveInput();
            }

            return success;
        }

        public void UpdateWUIArea(bool[] wuiAreaIndices)
        {
            if (wuiAreaIndices == null)
            {
                wuiAreaIndices = new bool[WUInity.SIM.FireMesh().cellCount.x * WUInity.SIM.FireMesh().cellCount.y];
            }
            WUInity.RUNTIME_DATA.Fire.WuiAreaIndices = wuiAreaIndices;
        }

        public void UpdateRandomIgnitionIndices(bool[] randomIgnitionIndices)
        {
            if (randomIgnitionIndices == null)
            {
                randomIgnitionIndices = new bool[WUInity.SIM.FireMesh().cellCount.x * WUInity.SIM.FireMesh().cellCount.y];
            }
            WUInity.RUNTIME_DATA.Fire.RandomIgnitionIndices = randomIgnitionIndices;
        }

        public void UpdateInitialIgnitionIndices(bool[] initialIgnitionIndices)
        {
            if (initialIgnitionIndices == null)
            {
                initialIgnitionIndices = new bool[WUInity.SIM.FireMesh().cellCount.x * WUInity.SIM.FireMesh().cellCount.y];
            }
            WUInity.RUNTIME_DATA.Fire.InitialIgnitionIndices = initialIgnitionIndices;
        }

        public void UpdateTriggerBufferIndices(bool[] triggerBufferIndices)
        {
            if (triggerBufferIndices == null)
            {
                triggerBufferIndices = new bool[WUInity.SIM.FireMesh().cellCount.x * WUInity.SIM.FireMesh().cellCount.y];
            }
            WUInity.RUNTIME_DATA.Fire.TriggerBufferIndices = triggerBufferIndices;
        }


        GameObject _lcpDataPlane;
        Texture2D _fuelModelsTexture, _elevationTexture, _slopeTexture, _aspectTexture;
        MeshRenderer lcpMeshRenderer;

        public void ToggleLCPDataPlane()
        {
            if(_lcpDataPlane == null)
            {
                CreateLCPVisuals();
            }

            _lcpDataPlane.SetActive(!_lcpDataPlane.activeSelf);
        }

        public void SetLCPDataPlane(bool setActive)
        {
            if (_lcpDataPlane == null)
            {
                CreateLCPVisuals();
            }

            _lcpDataPlane.SetActive(setActive);
        }

        public enum LcpViewMode { FuelModel, Elevation, Slope, Aspect }

        public void SetLCPViewMode(LcpViewMode lcpViewMode)
        {
            if (_lcpDataPlane == null)
            {
                CreateLCPVisuals();
            }

            if (lcpViewMode == LcpViewMode.FuelModel)
            {
                lcpMeshRenderer.material.mainTexture = _fuelModelsTexture;
            }
            else if (lcpViewMode == LcpViewMode.Elevation)
            {
                lcpMeshRenderer.material.mainTexture = _elevationTexture;
            }
            else if (lcpViewMode == LcpViewMode.Slope)
            {
                lcpMeshRenderer.material.mainTexture = _slopeTexture;
            }
            else if (lcpViewMode == LcpViewMode.Aspect)
            {
                lcpMeshRenderer.material.mainTexture = _aspectTexture;
            }
        }

        private void CreateLCPVisuals()
        {
            float xDim = (float)(_lcpData.Header.EastUtm - _lcpData.Header.WestUtm);
            float yDim = (float)(_lcpData.Header.NorthUtm - _lcpData.Header.SouthUtm);

            int xPixels = _lcpData.Header.numeast;
            int yPixels = _lcpData.Header.numnorth;

            _fuelModelsTexture = new Texture2D(xPixels, yPixels, TextureFormat.RGBA32, false);
            _fuelModelsTexture.filterMode = FilterMode.Point;

            _elevationTexture = new Texture2D(xPixels, yPixels, TextureFormat.RGBA32, false);
            _elevationTexture.filterMode = FilterMode.Point;

            _slopeTexture = new Texture2D(xPixels, yPixels, TextureFormat.RGBA32, false);
            _slopeTexture.filterMode = FilterMode.Point;

            _aspectTexture = new Texture2D(xPixels, yPixels, TextureFormat.RGBA32, false);
            _aspectTexture.filterMode = FilterMode.Point;

            float elevationRange = _lcpData.Header.hielev - _lcpData.Header.loelev;
            float slopeRange = _lcpData.Header.hislope - _lcpData.Header.loslope;
            float aspectRange = _lcpData.Header.hiaspect - _lcpData.Header.loaspect;
            float alpha = 0.85f;

            for (int y = 0; y < yPixels; y++)
            {
                for (int x = 0; x < xPixels; x++)
                {
                    LandScapeStruct l = _lcpData.GetCellData(x, y);

                    Color c = FuelModelColors.GetFuelColor((int)l.fuel_model);
                    c.a = alpha;
                    _fuelModelsTexture.SetPixel(x, y, c);

                    c = Color.white * (l.elevation - _lcpData.Header.loelev) / elevationRange;
                    c.a = alpha;
                    _elevationTexture.SetPixel(x, y, c);

                    c = Color.white * (l.slope - _lcpData.Header.loslope) / slopeRange;
                    c.a = alpha;
                    _slopeTexture.SetPixel(x, y, c);

                    c = Color.white * (l.aspect - _lcpData.Header.loaspect) / aspectRange;
                    c.a = alpha;
                    _aspectTexture.SetPixel(x, y, c);
                }
            }

            _fuelModelsTexture.Apply();
            _elevationTexture.Apply();
            _slopeTexture.Apply();
            _aspectTexture.Apply();

            CreateLCPDataPlane(WUInity.INSTANCE.transform, "LCP_plane", true, xDim, yDim);
        }

        private MeshRenderer CreateLCPDataPlane(Transform parent, string name, bool setActive, float width, float length)
        {
            Mesh mesh;

            if(_lcpDataPlane == null)
            {
                _lcpDataPlane = new GameObject(name);
                _lcpDataPlane.transform.parent = parent;
                _lcpDataPlane.transform.position += Vector3.up;
                _lcpDataPlane.isStatic = true;

                MeshFilter filter = _lcpDataPlane.AddComponent<MeshFilter>();
                mesh = new Mesh(); // filter.mesh;
                filter.mesh = mesh;
                lcpMeshRenderer = _lcpDataPlane.AddComponent<MeshRenderer>();
                lcpMeshRenderer.receiveShadows = false;
                lcpMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mesh.Clear();
                lcpMeshRenderer.material = new Material(Shader.Find("Unlit/Transparent"));
            }    
            else
            {
                mesh = _lcpDataPlane.GetComponent<MeshFilter>().mesh;
                mesh.Clear();
                lcpMeshRenderer = _lcpDataPlane.GetComponent<MeshRenderer>();
            }
            Vector3 offset = Vector3.zero;
            Vector2 maxUV = Vector2.one;

            VisualizeUtilities.CreateSimplePlane(mesh, width, length, 0.0f, offset, maxUV);

            lcpMeshRenderer.material.mainTexture = _fuelModelsTexture;

            _lcpDataPlane.SetActive(setActive);
            return lcpMeshRenderer;
        }
    }
}