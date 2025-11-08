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

namespace PREACT.IO
{
    public class FireData
    {
        private LCPData _lcpData;
        private FuelModelInput _fuelModelsData;
        private IgnitionPoint[] _ignitionPoints;
        private InitialFuelMoistureLibrary _initialFuelMoistureData;
        private WeatherInput _weatherInput;
        private WindInput _windInput;

        public bool[] WuiArea;
        public bool[] RandomIgnition;
        public bool[] InitialIgnition;
        public bool[] ManualTriggerBuffer;

               
        public LCPData LCPData { get => _lcpData; }        
        public FuelModelInput FuelModelsData { get => _fuelModelsData; }        
        public IgnitionPoint[] IgnitionPoints { get => _ignitionPoints; }       
        public InitialFuelMoistureLibrary InitialFuelMoistureData { get => _initialFuelMoistureData; }       
        public WeatherInput WeatherInput { get => _weatherInput; }       
        public WindInput WindInput { get => _windInput; }        

        public FireData()
        {
        }

        public void LoadAll(SimulationInput simulationInput, FireInput fireInput, string rootFolder, out bool success)
        {
            success = false;

            if(!simulationInput.RunFireModule)
            {
                Engine.MESSAGE(null, Engine.LogType.Log, "Skipping loading fire data as user has specified not running fire module.");
                success = true;
                return;
            }
            Engine.MESSAGE(null, Engine.LogType.Log, "Loading Fire data...");

            //we need LCP for all fires except straight import of results
            LoadLCPFile(Path.Combine(rootFolder, fireInput.LcpFile), simulationInput.Data.UTMOrigin, out success);
            if(!success && fireInput.FireModule != FireInput.FireModuleChoice.AscImport)
            {
                return;
            }

            //not critical
            string filePath = Path.Combine(rootFolder, fireInput.GraphicalFireInputFile);
            LoadGraphicalFireInput(fireInput, filePath, _lcpData, false, out success);

            if (fireInput.FireModule == FireInput.FireModuleChoice.FireCell)
            {
                int issues = 0;

                filePath = Path.Combine(rootFolder, fireInput.FireCellInput.RootFolder, fireInput.FireCellInput.FuelModelsFile);
                LoadFuelModelsInput(fireInput, filePath, false, out success);
                issues += success ? 0 : 1;

                filePath = Path.Combine(rootFolder, fireInput.FireCellInput.RootFolder, fireInput.FireCellInput.IgnitionPointsFile);
                LoadIgnitionPoints(fireInput, filePath , false, out success);
                issues += success ? 0 : 1;

                filePath = Path.Combine(rootFolder, fireInput.FireCellInput.RootFolder, fireInput.FireCellInput.InitialFuelMoistureFile);
                LoadInitialFuelMoistureData(fireInput, filePath, false, out success);
                issues += success ? 0 : 1;

                filePath = Path.Combine(rootFolder, fireInput.FireCellInput.RootFolder, fireInput.FireCellInput.WeatherFile);
                LoadWeatherInput(fireInput, filePath, false, out success);
                issues += success ? 0 : 1;

                filePath = Path.Combine(rootFolder, fireInput.FireCellInput.RootFolder, fireInput.FireCellInput.WindFile);
                LoadWindInput(fireInput, filePath, false, out success);
                issues += success ? 0 : 1;

                if(issues > 0)
                {
                    return;
                }
            }

            success = true;
        }

        public void LoadLCPFile(string filePath, Vector2d simulationUtmOrigin, out bool success)
        {
            _lcpData = new LCPData(filePath, simulationUtmOrigin);
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

        public void LoadFuelModelsInput(FireInput fireInput, string filePath, bool updateInput, out bool success)
        {
            _fuelModelsData = FuelModelInput.LoadFuelModelInputFile(filePath, out success);
            if (success && updateInput)
            {
                fireInput.FireCellInput.FuelModelsFile = Path.GetFileName(filePath);
            }
        }

        public void LoadIgnitionPoints(FireInput fireInput, string filePath, bool updateInput, out bool success)
        {
            _ignitionPoints = IgnitionPoint.LoadIgnitionPointsFile(filePath, out success);
            if (success && updateInput)
            {
                fireInput.FireCellInput.IgnitionPointsFile = Path.GetFileName(filePath);
            }
        }

        public void LoadInitialFuelMoistureData(FireInput fireInput, string filePath, bool updateInput, out bool success)
        {
            _initialFuelMoistureData = InitialFuelMoistureLibrary.LoadInitialFuelMoistureDataFile(filePath, out success);
            if (success && updateInput)
            {
                fireInput.FireCellInput.InitialFuelMoistureFile = Path.GetFileName(filePath);
            }
        }        

        public void LoadWeatherInput(FireInput fireInput, string filePath, bool updateInput, out bool success)
        {
            _weatherInput = WeatherInput.LoadWeatherInputFile(filePath, out success);
            if (success && updateInput)
            {
                fireInput.FireCellInput.WeatherFile = Path.GetFileName(filePath);
            }
        }

        public void LoadWindInput(FireInput fireInput, string filePath, bool updateInput, out bool success)
        {
            _windInput = WindInput.LoadWindInputFile(filePath, out success);
            if (success && updateInput)
            {
                fireInput.FireCellInput.WindFile = Path.GetFileName(filePath);
            }
        }

        public void LoadGraphicalFireInput(FireInput fireInput, string filePath, LCPData lcpData, bool updateInput, out bool success)
        {
            GraphicalFireInput.LoadGraphicalFireInput(filePath, lcpData, out WuiArea, out RandomIgnition, out InitialIgnition, out ManualTriggerBuffer, out success);
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
    }
}