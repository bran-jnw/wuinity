using System.IO;
using WUIEngine.Fire;
using WUIEngine.Visualization;
using WUIEngine.IO;

namespace WUIEngine.Runtime
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
            LoadLCPFile(Path.Combine(Engine.WORKING_FOLDER, Engine.INPUT.Fire.lcpFile), false);
            LoadFuelModelsInput(Path.Combine(Engine.WORKING_FOLDER, Engine.INPUT.Fire.fuelModelsFile), false);
            LoadIgnitionPoints(Path.Combine(Engine.WORKING_FOLDER, Engine.INPUT.Fire.ignitionPointsFile), false);
            LoadInitialFuelMoistureData(Path.Combine(Engine.WORKING_FOLDER, Engine.INPUT.Fire.initialFuelMoistureFile), false);
            LoadWeatherInput(Path.Combine(Engine.WORKING_FOLDER, Engine.INPUT.Fire.weatherFile), false);
            LoadWindInput(Path.Combine(Engine.WORKING_FOLDER, Engine.INPUT.Fire.windFile), false);
            LoadGraphicalFireInput(Path.Combine(Engine.WORKING_FOLDER, Engine.INPUT.Fire.graphicalFireInputFile), false);
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

                Engine.LOG(Engine.LogType.Log, message);
            }

            Engine.DATA_STATUS.LcpLoaded = success;
            if (success && updateInputFile)
            {
                Engine.INPUT.Fire.lcpFile = Path.GetFileName(path);
                WUInityInput.SaveInput();
            }

            return success;
        }

        public bool LoadFuelModelsInput(string path, bool updateInputFile)
        {
            bool success;
            _fuelModelsData = new FuelModelInput();
            success = _fuelModelsData.LoadFuelModelInputFile(path);

            Engine.DATA_STATUS.FuelModelsLoaded = success;
            if(success && updateInputFile)
            {
                Engine.INPUT.Fire.fuelModelsFile = Path.GetFileName(path);
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
                Engine.INPUT.Fire.ignitionPointsFile = Path.GetFileName(path);
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
                Engine.INPUT.Fire.initialFuelMoistureFile = Path.GetFileName(path);
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
                Engine.INPUT.Fire.weatherFile = Path.GetFileName(path);
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
                Engine.INPUT.Fire.windFile = Path.GetFileName(path);
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
                Engine.INPUT.Fire.graphicalFireInputFile = Path.GetFileName(path);
                WUInityInput.SaveInput();
            }

            return success;
        }

        public void UpdateWUIArea(bool[] wuiAreaIndices, int xCount, int yCount)
        {
            if (wuiAreaIndices == null)
            {
                wuiAreaIndices = new bool[xCount * yCount];
            }
            Engine.RUNTIME_DATA.Fire.WuiAreaIndices = wuiAreaIndices;
        }

        public void UpdateRandomIgnitionIndices(bool[] randomIgnitionIndices, int xCount, int yCount)
        {
            if (randomIgnitionIndices == null)
            {
                randomIgnitionIndices = new bool[xCount * yCount];
            }
            Engine.RUNTIME_DATA.Fire.RandomIgnitionIndices = randomIgnitionIndices;
        }

        public void UpdateInitialIgnitionIndices(bool[] initialIgnitionIndices, int xCount, int yCount)
        {
            if (initialIgnitionIndices == null)
            {
                initialIgnitionIndices = new bool[xCount * yCount];
            }
            Engine.RUNTIME_DATA.Fire.InitialIgnitionIndices = initialIgnitionIndices;
        }

        public void UpdateTriggerBufferIndices(bool[] triggerBufferIndices, int xCount, int yCount)
        {
            if (triggerBufferIndices == null)
            {
                triggerBufferIndices = new bool[xCount * yCount];
            }
            Engine.RUNTIME_DATA.Fire.TriggerBufferIndices = triggerBufferIndices;
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