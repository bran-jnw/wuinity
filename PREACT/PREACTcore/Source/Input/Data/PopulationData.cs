//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using PREACT.Population;
using PREACT.Input;
using PREACT.Math;

namespace PREACT.Input
{
    public class PopulationData
    {    
        public struct HouseholdData
        {
            public Vector2d originLatLon;
            public Vector2d roadAccessLatLon;
            public int peopleCount;

            public HouseholdData(Vector2d originLatLon, Vector2d roadAccessLatLon, int peopleCount)
            {
                this.originLatLon = originLatLon;
                this.roadAccessLatLon = roadAccessLatLon;
                this.peopleCount = peopleCount;
            }
        }

        private HouseholdData[] _householdData = System.Array.Empty<HouseholdData>();
        private int _totalPopulation;

        public HouseholdData[] Households { get => _householdData; }        
        public int TotalPopulation { get => _totalPopulation; }
                
        public PopulationData()
        {
            _totalPopulation = 0;
        }

        public void LoadAll(PedestrianModuleInput pedestrianInput, PopulationInput populationInput, string rootFolder, out bool success)
        {
            success = false;
            
            if(pedestrianInput.Enabled)
            {
                string filePath = Path.Combine(rootFolder, populationInput.PopulationFile);
                _householdData = LoadPopulation(filePath, out _totalPopulation, out success);
                if(!success)
                {
                    return;
                }
            }

            success = true;
        }
        
        public static HouseholdData[] LoadPopulation(string filePath, out int totalPopulation, out bool success)
        {
            success = false;
            totalPopulation = 0;
            HouseholdData[] householdData = null;

            if (File.Exists(filePath))
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    List<string> lines = new List<string>();

                    while (!sr.EndOfStream)
                    {
                        lines.Add(sr.ReadLine());
                    }

                    //skip first rom (header, and last row (should be empty)
                    householdData = new HouseholdData[lines.Count - 1];
                    for (int i = 1; i < lines.Count; ++i)
                    {
                        string[] line = lines[i].Split(",");
                        double lat = double.Parse(line[0]);
                        double lon = double.Parse(line[1]);
                        double carLat = double.Parse(line[2]);
                        double carLon = double.Parse(line[3]);
                        int people = int.Parse(line[4]);                        
                        householdData[i - 1] = new HouseholdData(new Vector2d(lat, lon), new Vector2d(carLat, carLon), people);
                        totalPopulation += people;
                    }

                    if(totalPopulation > 0)
                    {
                        success = true;
                        Engine.Message(null, Engine.LogType.Log, "Loaded population " + Path.GetFileNameWithoutExtension(filePath) + " containing " + totalPopulation + " people and " + householdData.Length + " households.");
                    }
                    else
                    {
                        Engine.Message(null, Engine.LogType.InputError, "Population file found but did not contain any population.");
                    }
                    
                }                
            }
            else
            {
                Engine.Message(null, Engine.LogType.InputError, "Population file " + filePath + " could not be found.");
            }

            return householdData;
        }
    }
}