//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using PREACT.Population;
using PREACT.Utility.Math;
using OsmSharp.Streams;
using System.IO;
using PREACT.IO;

namespace PREACT.Tools
{
    public static class PopulationTools
    {
        public static LocalGPWData CreateLocalGPWData(PREACTInput input, string globalGpwFolder, out bool success)
        {
            return LocalGPWData.CreateLocalGPWData(input, globalGpwFolder, out success);
        }

        public static void SaveLocalGPWData(string filePath, LocalGPWData localGPWData)
        {
            localGPWData.SaveToDisk(filePath);
        }

        public static LocalGPWData LoadLocalGPWData(string localGpwFile, out bool success)
        {           
            return LocalGPWData.LoadFromFile(localGpwFile, out success);   
        }

        public static PopulationMap CreateAndSavePopulationMap(string localGPWFile, string cellSize, string filePath, out bool success)
        {
            success = false;
            PopulationMap populationMap = null;

            float c;
            if(float.TryParse(cellSize, out c))
            {
                LocalGPWData localGPWData = LoadLocalGPWData(localGPWFile, out success);
                if(success)
                {
                    populationMap = CreateAndSavePopulationMap(localGPWData, c, filePath, out success);
                }                
            }
            else
            {
                Engine.MESSAGE(null, Engine.LogType.Warning, "Population map cell size is not a valid number, please check your input.");
            }

            return populationMap;
        }

        private static PopulationMap CreateAndSavePopulationMap(PREACTInput input, LocalGPWData localGPWData, float cellSize, string filePath, out bool success)
        {
            PopulationMap populationMap = new PopulationMap();
            populationMap.CreateFromLocalGPW(input, localGPWData, cellSize, out success);
            populationMap.SaveToFile(filePath);
            return populationMap;
        }

        public static PopulationMap LoadPopulationMap(string populationMapFile, out bool success)
        {
            PopulationMap populationMap = new PopulationMap();
            populationMap.LoadFromFile(populationMapFile, out success);
            return populationMap;
        }

        public static void ScaleTotalPopulation(PopulationMap populationMap, string desiredPopulation, out bool success)
        {
            success = false;

            int newPop;
            if (int.TryParse(desiredPopulation, out newPop))
            {
                ScaleTotalPopulation(populationMap, newPop, out success);
            }
            else
            {
                Engine.MESSAGE(null, Engine.LogType.Warning, " New population count not a number, please check your input.");
            }
        }

        public static void ScaleTotalPopulation(PopulationMap populationMap, int desiredPopulation, out bool success)
        {
            success = false;

            if(populationMap.HaveData)
            {
                populationMap.ScaleTotalPopulation(desiredPopulation);
                success = true;
            }
            else
            {
                Engine.MESSAGE(null, Engine.LogType.Warning, "No data in population map, cannot scale.");
            }
        }

        /// <summary>
        /// Filters the interpolated GPW data set to account for the user created population mask as well as checking for road access.
        /// </summary>
        public static void RoadAccessCorrectPopulationMap(PopulationMap populationMap, string lat, string lon, string routerDbFile, out bool success)
        {
            success = false;

            if (populationMap.HaveData)
            {
                Itinero.RouterDb rDb = RoutingData.LoadRouterDb(routerDbFile, out success);
                if (success)
                {
                    Vector2d latLon;
                    if(double.TryParse(lat, out latLon.x) && double.TryParse(lon, out latLon.y))
                    {
                        SimulationData sData = new SimulationData(latLon);
                        populationMap.UpdatePopulationMapBasedOnRoadAccess(sData, rDb);
                    }                    
                }                
            }
            else
            {
                Engine.MESSAGE(null, Engine.LogType.Warning, "No population map loaded, can't correct it for road access.");
            }
        }

        public static void ApplyPopulationMapMask(PopulationMap populationMap, string populationMaskFile)
        {
            if(populationMap.HaveData && populationMap.LoadPopulationMask(populationMaskFile))
            {
                populationMap.ApplyMaskToPopulation();
            }            
        }

        public static void SavePopulationMask(PopulationMap populationMap, string filePath)
        {
            populationMap.SavePopulationMask(filePath);
        }

        /*public static void LoadPopulationMask(string populationMaskFile)
        {
            WUIengine.RUNTIME_DATA.Population.PopulationMap.LoadPopulationMask(populationMaskFile);
        }*/ 

        public static void CreatePopulation(string minHouseholdSize, string maxHouseholdSize, PopulationMap populationMap, SimulationData simulationData, string filePath, out bool success)
        {
            success = false;

            if (populationMap.HaveData && populationMap.CorrectedForRoadAccess)
            {
                int min, max;
                if(int.TryParse(minHouseholdSize, out min) && int.TryParse(maxHouseholdSize, out max) && min <= max)
                {
                    populationMap.CreatePopulation(min, max, simulationData, filePath, out success);
                }    
            }
            else
            {
                Engine.MESSAGE(null, Engine.LogType.Warning, "Need population map that is corrected for road access, cannot create population.");
            }                     
        }

        public static bool CreateAndSaveRouterDb(string osmInputFile, string outputFile)
        {
            return RoutingData.CreateAndSaveRouterDb(osmInputFile, outputFile);
        }

        public static Itinero.RouterDb LoadRouterDb(string routerDbFile, out bool success)
        {
            return RoutingData.LoadRouterDb(routerDbFile, out success);
        }

        public static bool FilterOsmData(string osmFile, string xBorder, string yBorder, string lowerLeftLat, string lowerLeftLon, string domainSizeX, string domainSizeY)
        {
            Vector2d osmFilterBorder, domainSize, lowerLeftLatLon;
            if (double.TryParse(xBorder, out osmFilterBorder.x) 
                && double.TryParse(yBorder, out osmFilterBorder.y)
                && double.TryParse(lowerLeftLat, out lowerLeftLatLon.x)
                && double.TryParse(lowerLeftLon, out lowerLeftLatLon.y)
                && double.TryParse(domainSizeX, out domainSize.x)
                && double.TryParse(domainSizeY, out domainSize.y))
            {
                return FilterOsmData(osmFile, lowerLeftLatLon, domainSize, osmFilterBorder);
            }
            else
            {
                Engine.MESSAGE(null, Engine.LogType.Warning, "Border is not a valid number, please check your input.");
            }

            return false;
        }

        private static bool FilterOsmData(string osmFile, Vector2d lowerLeftLatLon, Vector2d domainSize, Vector2d borderSize)
        {
            bool success = false;

            if (File.Exists(osmFile))
            {
                using (FileStream stream = new FileInfo(osmFile).OpenRead())
                {
                    float left = (float)(lowerLeftLatLon.y - borderSize.x);
                    float bottom = (float)(lowerLeftLatLon.x - borderSize.y);
                    Vector2d size = LocalGPWData.SizeToDegrees(lowerLeftLatLon, domainSize);
                    float right = (float)(lowerLeftLatLon.y + size.x + borderSize.x);
                    float top = (float)(lowerLeftLatLon.x + size.y + borderSize.y);

                    OsmStreamSource source;                    
                    if (osmFile.EndsWith("pbf"))
                    {
                        source = new PBFOsmStreamSource(stream);
                    }
                    else
                    {
                        source = new XmlOsmStreamSource(stream);                        
                    }
                    OsmStreamSource filtered = source.FilterBox(left, top, right, bottom, true);
                    //create a new filtered file
                    string path = Path.Combine(Path.GetDirectoryName(osmFile), "filtered_" + Path.GetFileName(osmFile));
                    using (FileStream targetStream = File.OpenWrite(path))
                    {
                        PBFOsmStreamTarget target = new PBFOsmStreamTarget(targetStream, compress: false);
                        target.RegisterSource(filtered);
                        target.Pull();

                        success = true;
                    }
                }
            }
            else
            {
                Engine.MESSAGE(null, Engine.LogType.Warning, " Could not find the selected OSM file.");
            }

            if (success)
            {
                Engine.MESSAGE(null, Engine.LogType.Log, " Succesfully filtered OSM data to user selected boundary. Use this filtered data to build your router database.");
            }
            else
            {
                Engine.MESSAGE(null, Engine.LogType.Warning, " Could not filter the selected OSM file.");
            }

            return success;
        }
    }   
}
