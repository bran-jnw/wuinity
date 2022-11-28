using UnityEngine;
using System.IO;
using Itinero;
using Itinero.IO.Osm;
using Itinero.Osm.Vehicles;
using OsmSharp.Streams;
using WUInity.Population;
using WUInity.Fire;


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
    }
}