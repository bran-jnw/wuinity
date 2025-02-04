//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using WUIPlatform.Runtime;

namespace WUIPlatform.Population
{
    public class PopulationMap
    {
        public Vector2d _lowerLeftLatLong;
        public Vector2d _size;
        public Vector2int _cells;
        public float _cellSize;
        public int _totalPopulation;
        public int _totalActiveCells;
        public int[] _cellPopulations;
        public Vector2d[] _cellRoadAccessLatLon;
        public  double _cellArea;
        private bool[] _mask;
        public bool[] Mask {  get =>_mask; }


        //not saved
        private PopulationData _populationData;
        private bool _haveData;
        public bool HaveData { get => _haveData; }
        private bool _correctedForRoadAccess;
        public bool CorrectedForRoadAccess { get => _correctedForRoadAccess; }
        private string _fileName;
        public string FileName{ get => _fileName; }

        public PopulationMap(PopulationData populationData)
        {
            /*_lowerLeftLatLong = WUIEngine.INPUT.Simulation.LowerLeftLatLong;
            _size = WUIEngine.INPUT.Simulation.Size;
            _cells = WUIEngine.RUNTIME_DATA.Evacuation.CellCount;
            _cellSize = WUIEngine.INPUT.Evacuation.RouteCellSize;

            _cellArea = _cellSize * _cellSize / (1000000d); // people/square km
            _totalPopulation = 0;

            _cellPopulations = new int[_cells.x * _cells.y];
            populationMask = new bool[_cells.x * _cells.y];

            isLoaded = false;
            correctedForRoutes = false;*/
            _populationData = populationData;
            _haveData = false;
            _correctedForRoadAccess = false;
        }

        /// <summary>
        /// Gets the total amount of people inside this cell. Uses array index.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int GetPeopleCount(int x, int y)
        {
            return _cellPopulations[x + y * _cells.x];
        }

        /// <summary>
        /// Get number of people in cell based on "world space" coordinates. Clamps to dimensions of defined area.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int GetPopulationSimulationSpace(double x, double y)
        {
            int xInt = (int)((x / WUIEngine.INPUT.Simulation.Size.x) * _cells.x);
            int yInt = (int)((y / WUIEngine.INPUT.Simulation.Size.y) * _cells.y);
            return GetPeopleCount(xInt, yInt);
        }

        public bool GetMaskValue(int x, int y)
        {
            return _mask[x + y * _cells.x];
        }

        public void CreateAndSave(LocalGPWData localGPWData, float cellSize)
        {
            _lowerLeftLatLong = WUIEngine.INPUT.Simulation.LowerLeftLatLon;
            _size = WUIEngine.INPUT.Simulation.Size;
            _cellSize = cellSize;
            _cells = new Vector2int((int)(0.5f + _size.x / cellSize), (int)(0.5f + _size.y / cellSize));
            _size = new Vector2d(cellSize * _cells.x, cellSize * _cells.y); 
            _cellPopulations = new int[_cells.x * _cells.y];
            _cellRoadAccessLatLon = new Vector2d[_cells.x * _cells.y];
            _mask = new bool[_cells.x * _cells.y];
            _cellArea = cellSize * cellSize / (1000000d); // people/square km
            _totalPopulation = 0;
            _totalActiveCells = 0;

            for (int y = 0; y < _cells.y; ++y)
            {
                double yPos = (y + 0.5) * cellSize;
                for (int x = 0; x < _cells.x; ++x)
                {
                    int index = x + y * _cells.x;
                    double xPos = (x + 0.5) * cellSize;
                    double density = localGPWData.GetDensitySimulationSpaceBilinear(new Vector2d(xPos, yPos));
                    int pop = 0;
                    //if data has negative values (NO_DATA) it should be zero
                    //else force at least one person if there is some density
                    if (density > 0.0)
                    {
                        pop = Mathf.Max(1, Mathf.RoundToInt((float)(_cellArea * density)));
                    }

                    _cellPopulations[index] = pop;
                    _totalPopulation += pop;
                    if (pop > 0)
                    {
                        ++_totalActiveCells;
                        _mask[index] = true;
                    }
                }
            }
            
            //the bilinear interpolation might not have conserved the amount of people correctly, or we have masked off some people
            //UPDATE: maybe issue to do this as the area of thje GPW piece might be much larger than the are aof interest
            if (localGPWData.totalPopulation < _totalPopulation)
            {
                ScaleTotalPopulation(localGPWData.totalPopulation, false);
            }     
            
            _haveData = true;
            _correctedForRoadAccess = false;
            _fileName = WUIEngine.INPUT.Simulation.SimulationID;
            SaveToFile(_fileName);
            _populationData.Visualizer.CreatePopulationMapTexture(this);
            _populationData.Visualizer.CreatePopulationMapMaskTexture(this);
            WUIEngine.LOG(WUIEngine.LogType.Log, "Created population map from local GPW data.");
        }

        public void UpdatePopulationMapBasedOnRoadAccess(Itinero.Router router)
        {
            int stuckPeople = 0;
            for (int i = 0; i < _cellPopulations.Length; ++i)
            {
                if (_cellPopulations[i] > 0)
                {
                    int yIndex = i / _cells.x;
                    int xIndex = i - yIndex * _cells.x;
                    Vector2d cellCenterMercatorPos = new Vector2d((xIndex + 0.5f) * _cellSize, (yIndex + 0.5) * _cellSize) / WUIEngine.RUNTIME_DATA.Simulation.MercatorCorrectionScale + WUIEngine.RUNTIME_DATA.Simulation.CenterMercator;
                    Vector2d coord = GeoConversions.MetersToLatLon(cellCenterMercatorPos);
                    Itinero.RouterPoint p = Traffic.RouteCreator.GetValidRouterPoint(router, coord, Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), _cellSize);
                    if(p != null)
                    {
                        _cellRoadAccessLatLon[i].x = p.Latitude;
                        _cellRoadAccessLatLon[i].y = p.Longitude;                        
                    }  
                    else
                    {
                        stuckPeople += _cellPopulations[i];
                        //delete people in the current cell since we are relocating them
                        _cellPopulations[i] = 0;
                    }
                }
            }

            if (stuckPeople > 0)
            {
                RelocateStuckPeople(stuckPeople);
            }

            _correctedForRoadAccess = true;
            SaveToFile(_fileName);
        }

        public void ApplyMaskToPopulation()
        {
            int stuckPeople = 0;
            for (int i = 0; i < _cellPopulations.Length; ++i)
            {
                if (_cellPopulations[i] > 0 && _mask[i] == false)
                {
                    stuckPeople += _cellPopulations[i];
                    _cellPopulations[i] = 0;
                }
            }

            RelocateStuckPeople(stuckPeople);
            SaveToFile(_fileName);
        }

        /// <summary>
        /// Relocates stuck people (no route in cell), relocation based on ratio between people in cell / total people, so relative density is conserved
        /// </summary>
        /// <param name="stuckPeople"></param>
        private void RelocateStuckPeople(int stuckPeople)
        {
            if (stuckPeople > 0)
            {
                int oldTotalPopulation = _totalPopulation;
                int remainingPop = _totalPopulation - stuckPeople;
                _totalPopulation = 0;
                _totalActiveCells = 0;
                for (int i = 0; i < _cellPopulations.Length; ++i)
                {
                    if (_cellPopulations[i] > 0)
                    {
                        float weight = _cellPopulations[i] / (float)remainingPop;
                        int extraPersonsToCell = Mathf.Max(1, Mathf.RoundToInt(weight * stuckPeople));
                        _cellPopulations[i] += extraPersonsToCell;
                        _totalPopulation += _cellPopulations[i];
                        ++_totalActiveCells;
                    }
                }

                if (_totalPopulation != oldTotalPopulation)
                {
                    ScaleTotalPopulation(oldTotalPopulation, false);
                }
            }
        }        

        /// <summary>
        /// Creates uniform population based on a pre-made population mask (made in painter).
        /// </summary>
        /// <param name="newTotalPopulation"></param>
        public void CreatePopulationFromMask(int newTotalPopulation)
        {
            _totalActiveCells = 0;
            for (int i = 0; i < _mask.Length; i++)
            {
                if(_mask[i] == true)
                {
                    ++_totalActiveCells;
                }
            }

            int peoplePerCell = Mathf.Max(1, Mathf.RoundToInt((float)newTotalPopulation / _totalActiveCells));
            _totalPopulation = 0;
            for (int i = 0; i < _mask.Length; i++)
            {
                _cellPopulations[i] = 0;
                if (_mask[i] == true)
                {                    
                    _cellPopulations[i] = peoplePerCell;
                    _totalPopulation += peoplePerCell;
                }
            }

            if(newTotalPopulation != _totalPopulation)
            {
                ScaleTotalPopulation(newTotalPopulation, false);
            }

            _haveData = true;
            _populationData.Visualizer.CreatePopulationMapTexture(this);
            SaveToFile(WUIEngine.INPUT.Simulation.SimulationID);            
        }    

        /// <summary>
        /// Scales the actual poulation count from GPW down to desired amount of people
        /// </summary>
        /// <param name="desiredPopulation"></param>
        /// <returns></returns>
        public void ScaleTotalPopulation(int desiredPopulation, bool saveWhenDone)
        {
            int newTotalPop = 0;
            List<int> activeCellIndices = new List<int>();
            for (int i = 0; i < _cellPopulations.Length; ++i)
            {
                if (_cellPopulations[i] > 0)
                {
                    float weight = _cellPopulations[i] / (float)_totalPopulation;
                    int newPop = Mathf.Max(1, (int)(weight * desiredPopulation));
                    _cellPopulations[i] = newPop;
                    newTotalPop += newPop;

                    //save all of the indices for later random distribution
                    activeCellIndices.Add(i);
                }
            }
            _totalPopulation = newTotalPop;

            //make sure we hit our target, if we have more people than cells we should always be lower if not matching since we are flooring the int
            if(desiredPopulation > _totalPopulation)
            {
                int loopCount = desiredPopulation - _totalPopulation;
                for (int i = 0; i < loopCount; i++)
                {
                    int randomIndex = Random.Range(0, activeCellIndices.Count - 1);
                    ++_cellPopulations[activeCellIndices[randomIndex]];
                    ++_totalPopulation;
                }
            }
            //this can happen when we have too many active cells
            else if(desiredPopulation < _totalPopulation)
            {
                int loopCount = _totalPopulation - desiredPopulation;
                for (int i = 0; i < loopCount; i++)
                {
                    int randomIndex = Random.Range(0, activeCellIndices.Count - 1);
                    --_cellPopulations[activeCellIndices[randomIndex]];
                    --_totalPopulation;
                    if(_cellPopulations[activeCellIndices[randomIndex]] < 1)
                    {
                        activeCellIndices.RemoveAt(randomIndex);
                    }
                }
                _totalActiveCells = activeCellIndices.Count;
            }

            WUIEngine.LOG(WUIEngine.LogType.Log, "Re-scaled the population map to " + desiredPopulation + " people.");
            _populationData.Visualizer.CreatePopulationMapTexture(this);
            if(saveWhenDone)
            {
                SaveToFile(_fileName);
            }            
        }

        private void SaveToFile(string fileName)
        {
            string[] data = new string[9];

            data[0] = _lowerLeftLatLong.x.ToString();
            data[1] = _lowerLeftLatLong.y.ToString();
            data[2] = _size.x.ToString();
            data[3] = _size.y.ToString();
            data[4] = _cells.x.ToString();
            data[5] = _cells.y.ToString();
            data[6] = _cellSize.ToString();
            data[7] = _totalPopulation.ToString();
            data[8] = "";
            for (int i = 0; i < _cellPopulations.Length; i++)
            {
                data[8] += _cellPopulations[i] + " ";
            }

            string path = Path.Combine(WUIEngine.WORKING_FOLDER, fileName + ".pop");
            File.WriteAllLines(path, data);
            WUIEngine.LOG(WUIEngine.LogType.Log, "Saved population map to " + path);
        }

        public void SavePopulationMask(string newFilename)
        {
            string[] data = new string[9];

            data[0] = _lowerLeftLatLong.x.ToString();
            data[1] = _lowerLeftLatLong.y.ToString();
            data[2] = _size.x.ToString();
            data[3] = _size.y.ToString();
            data[4] = _cells.x.ToString();
            data[5] = _cells.y.ToString();
            data[6] = _cellSize.ToString();
            data[7] = _totalPopulation.ToString();
            data[8] = "";
            for (int i = 0; i < _mask.Length; i++)
            {
                data[8] += _mask[i] == true ? 1 + " " : 0 + " ";
            }

            string path = Path.Combine(WUIEngine.WORKING_FOLDER, newFilename + ".pmk");
            File.WriteAllLines(path, data);
            WUIEngine.LOG(WUIEngine.LogType.Log, "Saved population map mask to " + path);
        }

        public bool LoadPopulationMask(string populationMaskFile)
        {
            string[] d = File.ReadAllLines(populationMaskFile);

            bool success = false;
            if (d.Length == 9)
            {
                _totalActiveCells = 0;
                double.TryParse(d[0], out _lowerLeftLatLong.x);
                double.TryParse(d[1], out _lowerLeftLatLong.y);
                double.TryParse(d[2], out _size.x);
                double.TryParse(d[3], out _size.y);
                int temp;
                int.TryParse(d[4], out temp);
                _cells.x = temp;
                int.TryParse(d[5], out temp);
                _cells.y = temp;
                float.TryParse(d[6], out _cellSize);
                _cellArea = _cellSize * _cellSize / 1000000d; // people/square km
                int.TryParse(d[7], out _totalPopulation);
                _mask = new bool[_cells.x * _cells.y];
                string[] dummy = d[8].Split(' ');
                for (int i = 0; i < _mask.Length; ++i)
                {
                    int input;
                    int.TryParse(dummy[i], out input);
                    if (input == 1)
                    {
                        _mask[i] = true;
                    }
                }
                _populationData.Visualizer.CreatePopulationMapMaskTexture(this);
                success = true;
                WUIEngine.LOG(WUIEngine.LogType.Log, " Loaded population map mask from file " + populationMaskFile + ".");
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, " Population data not valid for current map.");
            }

            return success;
        }

        public bool LoadFromFile(string path)
        {
            string[] d = File.ReadAllLines(path);

            bool success = false;
            if (d.Length == 9)
            {
                _totalActiveCells = 0;
                double.TryParse(d[0], out _lowerLeftLatLong.x);
                double.TryParse(d[1], out _lowerLeftLatLong.y);
                double.TryParse(d[2], out _size.x);
                double.TryParse(d[3], out _size.y);
                int temp;
                int.TryParse(d[4], out temp);
                _cells.x = temp;
                int.TryParse(d[5], out temp);
                _cells.y = temp;
                float.TryParse(d[6], out _cellSize);
                _cellArea = _cellSize * _cellSize / 1000000d; // people/square km
                int.TryParse(d[7], out _totalPopulation);
                _cellPopulations = new int[_cells.x * _cells.y];
                _mask = new bool[_cells.x * _cells.y];                
                string[] dummy = d[8].Split(' ');
                for (int i = 0; i < _cellPopulations.Length; ++i)
                {
                    int.TryParse(dummy[i], out _cellPopulations[i]);
                    if(_cellPopulations[i] > 0)
                    {
                        _mask[i] = true;
                        ++_totalActiveCells;
                    }
                }
                _cellRoadAccessLatLon = new Vector2d[_cells.x * _cells.y];
                _populationData.Visualizer.CreatePopulationMapTexture(this);
                _populationData.Visualizer.CreatePopulationMapMaskTexture(this);
                _haveData = true;
                _correctedForRoadAccess = false;
                success = true;           
                _fileName = Path.GetFileNameWithoutExtension(path);
                WUIEngine.LOG(WUIEngine.LogType.Log, " Loaded population from file " + path + ".");
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, " Population data not valid for current map.");
            }

            return success;
        }

        public void CreateAndLoadPopulation()
        {
            string file = Path.Combine(WUIEngine.WORKING_FOLDER, _fileName + "_households.csv");
            using (StreamWriter sW = new StreamWriter(file))
            {
                sW.WriteLine("OriginLat,OriginLon,AccessLat,AccessLon,People");

                for (int i = 0; i < _cellRoadAccessLatLon.Length; i++)
                {
                    if (_cellPopulations[i] > 0)
                    {
                        int peopleWithoutHouseHold = _cellPopulations[i];
                        List<int> householdCounts = new List<int>();
                        while (peopleWithoutHouseHold > 0)
                        {
                            int p = Random.Range(WUIEngine.INPUT.Evacuation.minHouseholdSize, WUIEngine.INPUT.Evacuation.maxHouseholdSize);
                            if (p > peopleWithoutHouseHold)
                            {
                                p = peopleWithoutHouseHold;
                            }
                            householdCounts.Add(p);
                            peopleWithoutHouseHold -= p;
                        }

                        int yIndex = i / _cells.x;
                        int xIndex = i - yIndex * _cells.x;
                        Vector2d nodeCenter = new Vector2d((xIndex + 0.5f) * _cellSize, (yIndex + 0.5) * _cellSize);
                        for (int j = 0; j < householdCounts.Count; ++j)
                        {
                            Vector2d householdStartPos = nodeCenter;
                            householdStartPos.x += _cellSize * Random.Range(-0.5f, 0.5f);
                            householdStartPos.y += _cellSize * Random.Range(-0.5f, 0.5f);
                            Vector2d householdStartCoord = householdStartPos.GetGeoPosition(WUIEngine.RUNTIME_DATA.Simulation.CenterMercator, WUIEngine.RUNTIME_DATA.Simulation.MercatorCorrectionScale);

                            double goalLat = _cellRoadAccessLatLon[i].x;
                            double goalLon = _cellRoadAccessLatLon[i].y;
                            sW.WriteLine(householdStartCoord.x + "," + householdStartCoord.y + "," + _cellRoadAccessLatLon[i].x + "," + _cellRoadAccessLatLon[i].y + "," + householdCounts[j]);
                        }
                    }
                }

                WUIEngine.LOG(WUIEngine.LogType.Log, "Generated and saved population to file " + file);
            }

            WUIEngine.RUNTIME_DATA.Population.LoadPopulation(file);
        }
    }
}

