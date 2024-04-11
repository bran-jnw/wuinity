using System.IO;
using WUIPlatform.Fire;
using WUIPlatform.Visualization;
using WUIPlatform.IO;

namespace WUIPlatform.Runtime
{
    public class FireData
    {
        public bool[] WuiAreaIndices;
        public bool[] RandomIgnitionIndices;
        public bool[] InitialIgnitionIndices;
        public bool[] TriggerBufferIndices;

        private FireDataVisualizer _visualizer;
        public FireDataVisualizer Visualizer
        {
            get{ return _visualizer; }
        }


        private LCPData _lcpData;
        public LCPData LCPData
        {
            get
            {
                return _lcpData;
            }
        }

        private FuelModelInput _fuelModelsData;
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

        public FireData()
        {
            #if USING_UNITY
            _visualizer = new FireDataVisualizerUnity(this);
            #else

            #endif
        }

        public void LoadAll()
        {
            LoadLCPFile(Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Fire.lcpFile), false);
            LoadFuelModelsInput(Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Fire.fuelModelsFile), false);
            LoadIgnitionPoints(Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Fire.ignitionPointsFile), false);
            LoadInitialFuelMoistureData(Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Fire.initialFuelMoistureFile), false);
            LoadWeatherInput(Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Fire.weatherFile), false);
            LoadWindInput(Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Fire.windFile), false);
            LoadGraphicalFireInput(Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Fire.graphicalFireInputFile), false);
        }

        public bool LoadLCPFile(string path, bool updateInputFile)
        {
            bool success;
            _lcpData = new LCPData(path);
            success = !_lcpData.CantAllocLCP;

            if (success)
            {
                int[] fuelNrs = _lcpData.GetExisitingFuelModelNumbers();
                string message = "Present fuel model numbers are ";
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

                WUIEngine.LOG(WUIEngine.LogType.Log, message);
            }

            WUIEngine.DATA_STATUS.LcpLoaded = success;
            if (success && updateInputFile)
            {
                WUIEngine.INPUT.Fire.lcpFile = Path.GetFileName(path);
                WUIEngineInput.SaveInput();
            }

            return success;
        }

        public bool LoadFuelModelsInput(string path, bool updateInputFile)
        {
            bool success;
            _fuelModelsData = new FuelModelInput();
            success = _fuelModelsData.LoadFuelModelInputFile(path);

            WUIEngine.DATA_STATUS.FuelModelsLoaded = success;
            if(success && updateInputFile)
            {
                WUIEngine.INPUT.Fire.fuelModelsFile = Path.GetFileName(path);
                WUIEngineInput.SaveInput();
            }

            return success;
        }

        public bool LoadIgnitionPoints(string path, bool updateInputFile)
        {
            bool success;
            _ignitionPoints = IgnitionPoint.LoadIgnitionPointsFile(path, out success);
            if (success && updateInputFile)
            {
                WUIEngine.INPUT.Fire.ignitionPointsFile = Path.GetFileName(path);
                WUIEngineInput.SaveInput();
            }

            return success;
        }

        public bool LoadInitialFuelMoistureData(string path, bool updateInputFile)
        {
            bool success;
            _initialFuelMoistureData = InitialFuelMoistureList.LoadInitialFuelMoistureDataFile(out success);
            if (success && updateInputFile)
            {
                WUIEngine.INPUT.Fire.initialFuelMoistureFile = Path.GetFileName(path);
                WUIEngineInput.SaveInput();
            }

            return success;
        }

        public bool LoadWeatherInput(string path, bool updateInputFile)
        {
            bool success;
            _weatherInput = WeatherInput.LoadWeatherInputFile(out success);
            if (success && updateInputFile)
            {
                WUIEngine.INPUT.Fire.weatherFile = Path.GetFileName(path);
                WUIEngineInput.SaveInput();
            }

            return success;
        }

        public bool LoadWindInput(string path, bool updateInputFile)
        {
            bool success;
            _windInput = WindInput.LoadWindInputFile(out success);
            if (success && updateInputFile)
            {
                WUIEngine.INPUT.Fire.windFile = Path.GetFileName(path);
                WUIEngineInput.SaveInput();
            }

            return success;
        }

        public bool LoadGraphicalFireInput(string path, bool updateInputFile)
        {
            bool success;
            GraphicalFireInput.LoadGraphicalFireInput(out success);
            if (success && updateInputFile)
            {
                WUIEngine.INPUT.Fire.graphicalFireInputFile = Path.GetFileName(path);
                WUIEngineInput.SaveInput();
            }

            return success;
        }

        public void UpdateWUIArea(bool[] wuiAreaIndices, int xCount, int yCount)
        {
            if (wuiAreaIndices == null)
            {
                wuiAreaIndices = new bool[xCount * yCount];
            }
            WUIEngine.RUNTIME_DATA.Fire.WuiAreaIndices = wuiAreaIndices;
        }

        public void UpdateRandomIgnitionIndices(bool[] randomIgnitionIndices, int xCount, int yCount)
        {
            if (randomIgnitionIndices == null)
            {
                randomIgnitionIndices = new bool[xCount * yCount];
            }
            WUIEngine.RUNTIME_DATA.Fire.RandomIgnitionIndices = randomIgnitionIndices;
        }

        public void UpdateInitialIgnitionIndices(bool[] initialIgnitionIndices, int xCount, int yCount)
        {
            if (initialIgnitionIndices == null)
            {
                initialIgnitionIndices = new bool[xCount * yCount];
            }
            WUIEngine.RUNTIME_DATA.Fire.InitialIgnitionIndices = initialIgnitionIndices;
        }

        public void UpdateTriggerBufferIndices(bool[] triggerBufferIndices, int xCount, int yCount)
        {
            if (triggerBufferIndices == null)
            {
                triggerBufferIndices = new bool[xCount * yCount];
            }
            WUIEngine.RUNTIME_DATA.Fire.TriggerBufferIndices = triggerBufferIndices;
        }

        public void ToggleLCPDataPlane()
        {
            _visualizer.ToggleLCPDataPlane();
        }

        public void SetLCPDataPlane(bool setActive)
        {
            _visualizer.SetLCPDataPlane(setActive);
        }
    }
}