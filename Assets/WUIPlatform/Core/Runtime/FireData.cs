//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.IO;
using WUIPlatform.Fire;
using WUIPlatform.Visualization;
using WUIPlatform.IO;

namespace WUIPlatform.Runtime
{
    public class FireData
    {
        public bool[] WuiArea;
        public bool[] RandomIgnition;
        public bool[] InitialIgnition;
        public bool[] TriggerBuffer;

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
            WUIEngine.LOG(WUIEngine.LogType.Log, "Loading Fire data...");

            LoadLCPFile(Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Fire.LcpFile), false);
            LoadGraphicalFireInput(Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Fire.GraphicalFireInputFile), false);

            if (WUIEngine.INPUT.Fire.FireModule == FireInput.FireModuleChoice.FireCell)
            {
                LoadFuelModelsInput(Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Fire.FireCellInput.RootFolder, WUIEngine.INPUT.Fire.FireCellInput.FuelModelsFile), false);
                LoadIgnitionPoints(Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Fire.FireCellInput.RootFolder, WUIEngine.INPUT.Fire.FireCellInput.IgnitionPointsFile), false);
                LoadInitialFuelMoistureData(Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Fire.FireCellInput.RootFolder, WUIEngine.INPUT.Fire.FireCellInput.InitialFuelMoistureFile), false);
                LoadWeatherInput(Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Fire.FireCellInput.RootFolder, WUIEngine.INPUT.Fire.FireCellInput.WeatherFile), false);
                LoadWindInput(Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Fire.FireCellInput.RootFolder, WUIEngine.INPUT.Fire.FireCellInput.WindFile), false);                
            }            
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
            else
            {
                _lcpData = null;
            }

            WUIEngine.DATA_STATUS.LcpLoaded = success;
            if (success && updateInputFile)
            {
                WUIEngine.INPUT.Fire.LcpFile = Path.GetFileName(path);
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
                WUIEngine.INPUT.Fire.FireCellInput.FuelModelsFile = Path.GetFileName(path);
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
                WUIEngine.INPUT.Fire.FireCellInput.IgnitionPointsFile = Path.GetFileName(path);
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
                WUIEngine.INPUT.Fire.FireCellInput.InitialFuelMoistureFile = Path.GetFileName(path);
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
                WUIEngine.INPUT.Fire.FireCellInput.WeatherFile = Path.GetFileName(path);
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
                WUIEngine.INPUT.Fire.FireCellInput.WindFile = Path.GetFileName(path);
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
                WUIEngine.INPUT.Fire.GraphicalFireInputFile = Path.GetFileName(path);
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
            WUIEngine.RUNTIME_DATA.Fire.WuiArea = wuiAreaIndices;
        }

        public void UpdateRandomIgnitionIndices(bool[] randomIgnitionIndices, int xCount, int yCount)
        {
            if (randomIgnitionIndices == null)
            {
                randomIgnitionIndices = new bool[xCount * yCount];
            }
            WUIEngine.RUNTIME_DATA.Fire.RandomIgnition = randomIgnitionIndices;
        }

        public void UpdateInitialIgnitionIndices(bool[] initialIgnitionIndices, int xCount, int yCount)
        {
            if (initialIgnitionIndices == null)
            {
                initialIgnitionIndices = new bool[xCount * yCount];
            }
            WUIEngine.RUNTIME_DATA.Fire.InitialIgnition = initialIgnitionIndices;
        }

        public void UpdateTriggerBufferIndices(bool[] triggerBufferIndices, int xCount, int yCount)
        {
            if (triggerBufferIndices == null)
            {
                triggerBufferIndices = new bool[xCount * yCount];
            }
            WUIEngine.RUNTIME_DATA.Fire.TriggerBuffer = triggerBufferIndices;
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