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

namespace PREACT.Tools
{
    public static class PopulationTools
    {
        public static bool CreateAndSaveLocalGPWData(Engine engine, string globalGpwFolder)
        {
            bool success = false;

            success = engine.Scenario.Data.Population.LocalGPWData.CreateLocalGPWData(engine.Scenario.Input, globalGpwFolder);

            return success;
        }

        public static bool LoadLocalGPWData(Engine engine, string localGpwFile)
        {            
            bool success = engine.Scenario.Data.Population.LocalGPWData.LoadFromFile(localGpwFile);            
            return success;
        }

        public static void CreateAndSavePopulationMap(Engine engine, string localGPWFile, string cellSize)
        {
            float c;
            if(float.TryParse(cellSize, out c))
            {
                CreateAndSavePopulationMap(engine, localGPWFile, c);
            }
            else
            {
                Engine.MESSAGE(null, Engine.LogType.Warning, "Population map cell size is not a valid number, please check your input.");
            }
        }

        private static void CreateAndSavePopulationMap(Engine engine, string localGpwFile, float cellSize)
        {
            if (engine.Scenario.Data.Population.LocalGPWData.LoadFromFile(localGpwFile))
            {
                engine.Scenario.Data.Population.PopulationMap.CreateFromLocalGPW(engine.Scenario.Input, engine.Scenario.Data.Population.LocalGPWData, cellSize);
            }
        }

        public static void LoadPopulationMap(PopulationMap populationMap, string populationMapFile, out bool success)
        {     
            return populationMap.LoadFromFile(populationMapFile, out success);
        }

        public static bool ScaleTotalPopulation(Engine engine, string desiredPopulation)
        {
            bool success = false;
            int newPop;
            if (int.TryParse(desiredPopulation, out newPop))
            {
                success = ScaleTotalPopulation(engine, newPop);
            }
            else
            {
                Engine.MESSAGE(null, Engine.LogType.Warning, " New population count not a number, please check your input.");
            }

            return success;
        }

        public static bool ScaleTotalPopulation(Engine engine, int desiredPopulation)
        {
            bool success = false;

            if(engine.Scenario.Data.Population.PopulationMap.HaveData)
            {
                engine.Scenario.Data.Population.PopulationMap.ScaleTotalPopulation(desiredPopulation);
            }
            else
            {
                Engine.MESSAGE(null, Engine.LogType.Warning, "No population map loaded, cannot scale.");
            }

            return success;
        }

        /// <summary>
        /// Filters the interpolated GPW data set to account for the user created population mask as well as checking for road access.
        /// </summary>
        public static void RoadAccessCorrectPopulationMap(PopulationMap populationMap, Runtime.GeoData geoData, string routerDbFile, out bool success)
        {
            success = false;

            if (populationMap.HaveData)
            {
                Itinero.RouterDb rDb = Runtime.RoutingData.LoadRouterDb(routerDbFile, out success);
                if (success)
                {
                    populationMap.UpdatePopulationMapBasedOnRoadAccess(geoData, rDb);
                }                
            }
            else
            {
                Engine.MESSAGE(null, Engine.LogType.Warning, "No population map loaded, can't correct it for road access.");
            }
        }

        public static void ApplyPopulationMapMask(Engine engine, string populationMaskFile)
        {
            if(engine.Scenario.Data.Population.PopulationMap.HaveData && engine.Scenario.Data.Population.PopulationMap.LoadPopulationMask(populationMaskFile))
            {
                engine.Scenario.Data.Population.PopulationMap.ApplyMaskToPopulation();
            }            
        }

        public static void SavePopulationMask(Engine engine, string file)
        {
            engine.Scenario.Data.Population.PopulationMap.SavePopulationMask(file);
        }

        /*public static void LoadPopulationMask(string populationMaskFile)
        {
            WUIengine.RUNTIME_DATA.Population.PopulationMap.LoadPopulationMask(populationMaskFile);
        }*/ 

        public static void CreatePopulation(IO.PREACTInput input, PopulationMap populationMap, Runtime.GeoData geoData, string filePath, out bool success)
        {
            success = false;

            if (populationMap.HaveData && populationMap.CorrectedForRoadAccess)
            {
                populationMap.CreatePopulation(input, geoData, filePath, out success);
            }
            else
            {
                Engine.MESSAGE(null, Engine.LogType.Warning, "Need population map that is corrected for road access, cannot create population.");
            }                     
        }

        public static bool CreateAndSaveRouterDb(string osmInputFile, string outputFile)
        {
            return Runtime.RoutingData.CreateAndSaveRouterDb(osmInputFile, outputFile);
        }

        public static Itinero.RouterDb LoadRouterDb(Engine engine, string routerDbFile, out bool success)
        {
            return Runtime.RoutingData.LoadRouterDb(routerDbFile, out success);
        }

        public static bool FilterOsmData(IO.PREACTInput input, string osmFile, string xBorder, string yBorder)
        {
            Vector2d osmFilterBorder;
            if (double.TryParse(xBorder, out osmFilterBorder.x) && double.TryParse(yBorder, out osmFilterBorder.y))
            {
                return FilterOsmData(input, osmFile, osmFilterBorder);
            }
            else
            {
                Engine.MESSAGE(null, Engine.LogType.Warning, "Border is not a valid number, please check your input.");
            }

            return false;
        }

        private static bool FilterOsmData(IO.PREACTInput input, string osmFile, Vector2d borderSize)
        {
            bool success = false;

            if (File.Exists(osmFile))
            {
                using (FileStream stream = new FileInfo(osmFile).OpenRead())
                {
                    float left = (float)(input.Simulation.LowerLeftLatLon.y - borderSize.x);
                    float bottom = (float)(input.Simulation.LowerLeftLatLon.x - borderSize.y);
                    Vector2d size = LocalGPWData.SizeToDegrees(input.Simulation.LowerLeftLatLon, input.Simulation.DomainSize);
                    float right = (float)(input.Simulation.LowerLeftLatLon.y + size.x + borderSize.x);
                    float top = (float)(input.Simulation.LowerLeftLatLon.x + size.y + borderSize.y);

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
