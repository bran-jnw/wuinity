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

            success = engine.ScenarioData.Population.LocalGPWData.CreateLocalGPWData(engine.Input, globalGpwFolder);

            return success;
        }

        public static bool LoadLocalGPWData(Engine engine, string localGpwFile)
        {            
            bool success = engine.ScenarioData.Population.LocalGPWData.LoadFromFile(localGpwFile);            
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
            if (engine.ScenarioData.Population.LocalGPWData.LoadFromFile(localGpwFile))
            {
                engine.ScenarioData.Population.PopulationMap.CreateFromLocalGPW(engine.Input, engine.ScenarioData.Population.LocalGPWData, cellSize);
            }
        }

        public static bool LoadPopulationMap(Engine engine, string populationMapFile)
        {
            bool success = false;

            success = engine.ScenarioData.Population.PopulationMap.LoadFromFile(populationMapFile);

            return success;
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

            if(engine.ScenarioData.Population.PopulationMap.HaveData)
            {
                engine.ScenarioData.Population.PopulationMap.ScaleTotalPopulation(desiredPopulation);
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
        public static void RoadAccessCorrectPopulationMap(Engine engine, string routerDbFile)
        {
            
            if (engine.ScenarioData.Population.PopulationMap.HaveData)
            {
                if(engine.ScenarioData.Routing.LoadRouterDb(routerDbFile))
                {
                    engine.ScenarioData.Population.PopulationMap.UpdatePopulationMapBasedOnRoadAccess(engine.ScenarioData, engine.ScenarioData.Routing.Router);
                }                
            }
            else
            {
                Engine.MESSAGE(null, Engine.LogType.Warning, "No population map loaded, can't correct it for road access.");
            }
        }

        public static void ApplyPopulationMapMask(Engine engine, string populationMaskFile)
        {
            if(engine.ScenarioData.Population.PopulationMap.HaveData && engine.ScenarioData.Population.PopulationMap.LoadPopulationMask(populationMaskFile))
            {
                engine.ScenarioData.Population.PopulationMap.ApplyMaskToPopulation();
            }            
        }

        public static void SavePopulationMask(Engine engine, string file)
        {
            engine.ScenarioData.Population.PopulationMap.SavePopulationMask(file);
        }

        /*public static void LoadPopulationMask(string populationMaskFile)
        {
            WUIengine.RUNTIME_DATA.Population.PopulationMap.LoadPopulationMask(populationMaskFile);
        }*/ 

        public static void CreatePopulation(Engine engine, string file)
        {
            if (engine.ScenarioData.Population.PopulationMap.HaveData && engine.ScenarioData.Population.PopulationMap.CorrectedForRoadAccess)
            {
                engine.ScenarioData.Population.PopulationMap.CreateAndLoadPopulation(engine.Input, engine.ScenarioData, file);
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

        public static bool LoadRouterDb(Engine engine, string routerDbFile)
        {

            return engine.ScenarioData.Routing.LoadRouterDb(routerDbFile);
        }

        public static bool FilterOsmData(Engine engine, string osmFile, string xBorder, string yBorder)
        {
            Vector2d osmFilterBorder;
            if (double.TryParse(xBorder, out osmFilterBorder.x) && double.TryParse(yBorder, out osmFilterBorder.y))
            {
                return FilterOsmData(engine, osmFile, osmFilterBorder);
            }
            else
            {
                Engine.MESSAGE(null, Engine.LogType.Warning, "Border is not a valid number, please check your input.");
            }

            return false;
        }

        private static bool FilterOsmData(Engine engine, string osmFile, Vector2d borderSize)
        {
            bool success = false;

            if (File.Exists(osmFile))
            {
                using (FileStream stream = new FileInfo(osmFile).OpenRead())
                {
                    float left = (float)(engine.Input.Simulation.LowerLeftLatLon.y - borderSize.x);
                    float bottom = (float)(engine.Input.Simulation.LowerLeftLatLon.x - borderSize.y);
                    Vector2d size = LocalGPWData.SizeToDegrees(engine.Input.Simulation.LowerLeftLatLon, engine.Input.Simulation.DomainSize);
                    float right = (float)(engine.Input.Simulation.LowerLeftLatLon.y + size.x + borderSize.x);
                    float top = (float)(engine.Input.Simulation.LowerLeftLatLon.x + size.y + borderSize.y);

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
