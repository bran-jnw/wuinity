//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.IO;
using PREACT.Fire;
using PREACT.Visualization;
using PREACT.IO;
using PREACT.Utility.Math;

namespace PREACT.Runtime
{
    public class FireData
    {
        private LCPData _lcpData;
        public bool[] WuiArea;
        public bool[] RandomIgnition;
        public bool[] InitialIgnition;
        public bool[] ManualTriggerBuffer;

        private FireDataVisualizer _visualizer;
        public FireDataVisualizer Visualizer {  get => _visualizer; } 

        
        public LCPData LCPData { get => _lcpData; }

        private FuelModelInput _fuelModelsData;
        public FuelModelInput FuelModelsData { get => _fuelModelsData; }

        private IgnitionPoint[] _ignitionPoints;
        public IgnitionPoint[] IgnitionPoints { get => _ignitionPoints; }

        private InitialFuelMoistureLibrary _initialFuelMoistureData;
        public InitialFuelMoistureLibrary InitialFuelMoistureData { get => _initialFuelMoistureData; }

        private WeatherInput _weatherInput;
        public WeatherInput WeatherInput { get => _weatherInput; }

        private WindInput _windInput;
        public WindInput WindInput { get => _windInput; }

        private InitialFuelMoistureLibrary _kPERILInitialFuelMoistureData;
        public InitialFuelMoistureLibrary kPERILInitialFuelMoistureData { get => _kPERILInitialFuelMoistureData; }

        public FireData()
        {
            #if USING_UNITY
            _visualizer = new FireDataVisualizerUnity(this);
            #else

            #endif
        }

        public void LoadAll(PREACTInput input, string rootFolder, Vector2d utmOrigin, out bool success)
        {
            success = false;

            if(!input.Simulation.RunFireModule)
            {
                Engine.MESSAGE(null, Engine.LogType.Log, "Skipping loading fire data as user has specified not running fire module.");
                success = true;
                return;
            }
            Engine.MESSAGE(null, Engine.LogType.Log, "Loading Fire data...");

            //we need LCP for all fires except straight import of results
            LoadLCPFile(Path.Combine(rootFolder, input.Fire.LcpFile), utmOrigin, out success);
            if(!success && input.Fire.FireModule != FireInput.FireModuleChoice.AscImport)
            {
                return;
            }

            //not critical
            string file = Path.Combine(rootFolder, input.Fire.GraphicalFireInputFile);
            LoadGraphicalFireInput(input, file, _lcpData, false, out success);

            if (input.Fire.FireModule == FireInput.FireModuleChoice.FireCell)
            {
                int issues = 0;

                file = Path.Combine(rootFolder, input.Fire.FireCellInput.RootFolder, input.Fire.FireCellInput.FuelModelsFile);
                LoadFuelModelsInput(input, file, false, out success);
                issues += success ? 0 : 1;

                file = Path.Combine(rootFolder, input.Fire.FireCellInput.RootFolder, input.Fire.FireCellInput.IgnitionPointsFile);
                LoadIgnitionPoints(input, file , false, out success);
                issues += success ? 0 : 1;

                file = Path.Combine(rootFolder, input.Fire.FireCellInput.RootFolder, input.Fire.FireCellInput.InitialFuelMoistureFile);
                LoadInitialFuelMoistureData(input, file, false, out success);
                issues += success ? 0 : 1;

                file = Path.Combine(rootFolder, input.Fire.FireCellInput.RootFolder, input.Fire.FireCellInput.WeatherFile);
                LoadWeatherInput(input, file, false, out success);
                issues += success ? 0 : 1;

                file = Path.Combine(rootFolder, input.Fire.FireCellInput.RootFolder, input.Fire.FireCellInput.WindFile);
                LoadWindInput(input, file, false, out success);
                issues += success ? 0 : 1;

                if(issues > 0)
                {
                    return;
                }
            }

            if(input.TriggerBuffer.CalculateTriggerBuffer 
                && input.TriggerBuffer.TriggerBuffer == TriggerBufferInput.TriggerBufferChoice.kPERIL
                && input.TriggerBuffer.kPERILInput.CalculateROSFromBehave)
            {
                file = Path.Combine(rootFolder, input.TriggerBuffer.kPERILInput.InitialFuelMoistureFile);
                LoadPERILInitialFuelMoistureData(input, file, false, out success);
                if(!success)
                {
                    return;
                }
            }

            success = true;
        }

        public void LoadLCPFile(string filePath, Vector2d utmOrigin, out bool success)
        {
            _lcpData = new LCPData(filePath, utmOrigin);
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

                Engine.MESSAGE(null, Engine.LogType.Log, message);
            }
            else
            {
                _lcpData = null;
            }
        }

        public void LoadFuelModelsInput(PREACTInput input, string file, bool updateInput, out bool success)
        {
            _fuelModelsData = FuelModelInput.LoadFuelModelInputFile(file, out success);
            if (success && updateInput)
            {
                input.Fire.FireCellInput.FuelModelsFile = Path.GetFileName(file);
                //input.SaveInput();
            }
        }

        public void LoadIgnitionPoints(PREACTInput input, string file, bool updateInput, out bool success)
        {
            _ignitionPoints = IgnitionPoint.LoadIgnitionPointsFile(file, out success);
            if (success && updateInput)
            {
                input.Fire.FireCellInput.IgnitionPointsFile = Path.GetFileName(file);
                //input.SaveInput();
            }
        }

        public void LoadInitialFuelMoistureData(PREACTInput input, string file, bool updateInput, out bool success)
        {
            _initialFuelMoistureData = InitialFuelMoistureLibrary.LoadInitialFuelMoistureDataFile(file, out success);
            if (success && updateInput)
            {
                input.Fire.FireCellInput.FuelModelsFile = Path.GetFileName(file);
                //input.SaveInput();
            }
        }

        private void LoadPERILInitialFuelMoistureData(PREACTInput input, string file, bool updateInput, out bool success)
        {
            _kPERILInitialFuelMoistureData = InitialFuelMoistureLibrary.LoadInitialFuelMoistureDataFile(file, out success);
        }

        public void LoadWeatherInput(PREACTInput input, string file, bool updateInput, out bool success)
        {
            _weatherInput = WeatherInput.LoadWeatherInputFile(file, out success);
        }

        public void LoadWindInput(PREACTInput input, string file, bool updateInput, out bool success)
        {
            _windInput = WindInput.LoadWindInputFile(file, out success);
        }

        public void LoadGraphicalFireInput(PREACTInput input, string file, LCPData lcpData, bool updateInput, out bool success)
        {
            GraphicalFireInput.LoadGraphicalFireInput(file, lcpData, out WuiArea, out RandomIgnition, out InitialIgnition, out ManualTriggerBuffer, out success);
        }

        public void UpdateWUIArea(bool[] wuiAreaIndices, int xCount, int yCount)
        {
            if (wuiAreaIndices == null)
            {
                wuiAreaIndices = new bool[xCount * yCount];
            }
            WuiArea = wuiAreaIndices;
        }

        public void UpdateRandomIgnitionIndices(bool[] randomIgnitionIndices, int xCount, int yCount)
        {
            if (randomIgnitionIndices == null)
            {
                randomIgnitionIndices = new bool[xCount * yCount];
            }
            RandomIgnition = randomIgnitionIndices;
        }

        public void UpdateInitialIgnitionIndices(bool[] initialIgnitionIndices, int xCount, int yCount)
        {
            if (initialIgnitionIndices == null)
            {
                initialIgnitionIndices = new bool[xCount * yCount];
            }
            InitialIgnition = initialIgnitionIndices;
        }

        //for painting trigger buffer manually
        public void UpdateTriggerBufferIndices(bool[] triggerBufferIndices, int xCount, int yCount)
        {
            if (triggerBufferIndices == null)
            {
                triggerBufferIndices = new bool[xCount * yCount];
            }
            ManualTriggerBuffer = triggerBufferIndices;
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