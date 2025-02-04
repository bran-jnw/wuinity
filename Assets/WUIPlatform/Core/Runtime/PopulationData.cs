//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using WUIPlatform.Population;

namespace WUIPlatform.Runtime
{
    public class PopulationData
    {    
        public struct HouseholdData
        {
            public Vector2d _houseLatLon;
            public Vector2d _carLatLon;
            public int _peopleCount;

            public HouseholdData(Vector2d houseLatLon, Vector2d carLatLon, int peopleCount)
            {
                _houseLatLon = houseLatLon;
                _carLatLon = carLatLon;
                _peopleCount = peopleCount;
            }
        }

        private HouseholdData[] _householdData;
        public HouseholdData[] Households { get => _householdData; }

        private int _totalPopulation;
        public int TotalPopulation { get => _totalPopulation; }

        protected PopulationVisualizer _visualizer;
        public PopulationVisualizer Visualizer { get =>_visualizer; }
        
        private LocalGPWData _localGPWData;
        public LocalGPWData LocalGPWData { get => _localGPWData; }

        private PopulationMap _populationMap;
        public PopulationMap PopulationMap { get => _populationMap; }

        public PopulationData()
        {
            _localGPWData = new LocalGPWData(this);
            _populationMap = new PopulationMap(this);

            #if USING_UNITY
            _visualizer = new PopulationVisualizerUnity(this);
            #else

            #endif
        }

        public void LoadAll()
        {
            LoadPopulation(Path.Combine(WUIEngine.WORKING_FOLDER, WUIEngine.INPUT.Population.HouseholdsFile));
        }
        
        private bool LoadPopulation(string path)
        {
            bool success = false;

            if (File.Exists(path))
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    List<string> lines = new List<string>();

                    while (!sr.EndOfStream)
                    {
                        lines.Add(sr.ReadLine());
                    }

                    //skip first rom (header, and last row (should be empty)
                    _householdData = new HouseholdData[lines.Count - 2];
                    _totalPopulation = 0;
                    for (int i = 1; i < lines.Count - 1; ++i)
                    {
                        string[] line = lines[i].Split(",");
                        double lat = double.Parse(line[0]);
                        double lon = double.Parse(line[1]);
                        double carLat = double.Parse(line[2]);
                        double carLon = double.Parse(line[3]);
                        int people = int.Parse(line[4]);                        
                        _householdData[i - 1] = new HouseholdData(new Vector2d(lat, lon), new Vector2d(carLat, carLon), people);
                        _totalPopulation += people;
                    }
                }

                success = true;
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, "Population file could not be found.");
            }

            return success;
        }        
    }
}