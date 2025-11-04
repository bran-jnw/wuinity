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

namespace PREACT.Runtime
{
    public class FireData
    {
        public bool[] WuiArea;
        public bool[] RandomIgnition;
        public bool[] InitialIgnition;
        public bool[] ManualTriggerBuffer;

        private FireDataVisualizer _visualizer;
        public FireDataVisualizer Visualizer {  get => _visualizer; } 

        private LCPData _lcpData;
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

        public void LoadAll(Input input, string rootFolder)
        {
            if(!input.Simulation.RunFireModule)
            {
                Engine.MESSAGE(null, Engine.LogType.Log, "Skipping loading fire data as user has specified not running fire module.");
                return;
            }
            Engine.MESSAGE(null, Engine.LogType.Log, "Loading Fire data...");

            LoadLCPFile(input, Path.Combine(rootFolder, input.Fire.LcpFile), false);
            LoadGraphicalFireInput(input, Path.Combine(rootFolder, input.Fire.GraphicalFireInputFile), false);

            if (input.Fire.FireModule == FireInput.FireModuleChoice.FireCell)
            {
                LoadFuelModelsInput(input, Path.Combine(rootFolder, input.Fire.FireCellInput.RootFolder, input.Fire.FireCellInput.FuelModelsFile), false);
                LoadIgnitionPoints(input, Path.Combine(rootFolder, input.Fire.FireCellInput.RootFolder, input.Fire.FireCellInput.IgnitionPointsFile), false);
                LoadInitialFuelMoistureData(input, Path.Combine(rootFolder, input.Fire.FireCellInput.RootFolder, input.Fire.FireCellInput.InitialFuelMoistureFile), false);
                LoadWeatherInput(input, Path.Combine(rootFolder, input.Fire.FireCellInput.RootFolder, input.Fire.FireCellInput.WeatherFile), false);
                LoadWindInput(input, Path.Combine(rootFolder, input.Fire.FireCellInput.RootFolder, input.Fire.FireCellInput.WindFile), false);                
            }

            if(input.TriggerBuffer.CalculateTriggerBuffer 
                && input.TriggerBuffer.TriggerBuffer == TriggerBufferInput.TriggerBufferChoice.kPERIL
                && input.TriggerBuffer.kPERILInput.CalculateROSFromBehave)
            {
                string file = Path.Combine(rootFolder, input.TriggerBuffer.kPERILInput.InitialFuelMoistureFile);
                LoadPERILInitialFuelMoistureData(file);
            }            
        }

        public bool LoadLCPFile(Input input, string file, bool updateInputFile)
        {
            bool success;
            _lcpData = new LCPData(file);
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

            if (success && updateInputFile)
            {
                input.Fire.LcpFile = Path.GetFileName(file);
                //Input.SaveInput();
            }

            return success;
        }

        public bool LoadFuelModelsInput(Input input, string file, bool updateInputFile)
        {
            bool success;
            _fuelModelsData = new FuelModelInput();
            success = _fuelModelsData.LoadFuelModelInputFile(file);

            //Engine.DataStatus.FuelModelsLoaded = success;
            if(success && updateInputFile)
            {
                input.Fire.FireCellInput.FuelModelsFile = Path.GetFileName(file);
                //Input.SaveInput();
            }

            return success;
        }

        public bool LoadIgnitionPoints(Input input, string file, bool updateInputFile)
        {
            bool success;
            _ignitionPoints = IgnitionPoint.LoadIgnitionPointsFile(file, out success);
            if (success && updateInputFile)
            {
                input.Fire.FireCellInput.IgnitionPointsFile = Path.GetFileName(file);
                //Input.SaveInput();
            }

            return success;
        }

        public bool LoadInitialFuelMoistureData(Input input, string file, bool updateInputFile)
        {
            bool success;
            _initialFuelMoistureData = InitialFuelMoistureLibrary.LoadInitialFuelMoistureDataFile(file, out success);
            if (success && updateInputFile)
            {
                input.Fire.FireCellInput.InitialFuelMoistureFile = Path.GetFileName(file);
                //Input.SaveInput();
            }

            return success;
        }

        private bool LoadPERILInitialFuelMoistureData(string file)
        {
            bool success;

            _kPERILInitialFuelMoistureData = InitialFuelMoistureLibrary.LoadInitialFuelMoistureDataFile(file, out success);

            return success;
        }

        public bool LoadWeatherInput(Input input, string file, bool updateInputFile)
        {
            bool success;
            _weatherInput = WeatherInput.LoadWeatherInputFile(out success);
            if (success && updateInputFile)
            {
                input.Fire.FireCellInput.WeatherFile = Path.GetFileName(file);
                //Input.SaveInput();
            }

            return success;
        }

        public bool LoadWindInput(Input input, string file, bool updateInputFile)
        {
            bool success;
            _windInput = WindInput.LoadWindInputFile(out success);
            if (success && updateInputFile)
            {
                input.Fire.FireCellInput.WindFile = Path.GetFileName(file);
                //Input.SaveInput();
            }

            return success;
        }

        public bool LoadGraphicalFireInput(Input input, string file, bool updateInputFile)
        {
            bool success;
            GraphicalFireInput.LoadGraphicalFireInput(out success);
            if (success && updateInputFile)
            {
                input.Fire.GraphicalFireInputFile = Path.GetFileName(file);
                //Input.SaveInput();
            }

            return success;
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