//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using WUIPlatform.Population;
using Itinero;
using OsmSharp.Streams;
using System.IO;

namespace WUIPlatform.Tools
{
    public static class PopulationTools
    {
        public static bool HaveLocalGPW { get => WUIEngine.RUNTIME_DATA.Population.LocalGPWData.HavedData; }
        public static bool HavePopulationMap { get => WUIEngine.RUNTIME_DATA.Population.PopulationMap.HaveData; }
        public static bool PopulationMapCorrectedForRoadAccess { get => WUIEngine.RUNTIME_DATA.Population.PopulationMap.CorrectedForRoadAccess; }
        public static bool HaveRouterDb { get => WUIEngine.RUNTIME_DATA.Routing.RouterDb == null ? false : true; }


        public static bool CreateAndSaveLocalGPWData(string globalGpwFolder)
        {
            bool success = false;

            success = WUIEngine.RUNTIME_DATA.Population.LocalGPWData.CreateLocalGPWData(globalGpwFolder);

            return success;
        }

        public static bool LoadLocalGPWData(string localGpwFile)
        {            
            bool success = WUIEngine.RUNTIME_DATA.Population.LocalGPWData.LoadFromFile(localGpwFile);            
            return success;
        }

        public static void CreateAndSavePopulationMap(string localGPWFile, string cellSize)
        {
            float c;
            if(float.TryParse(cellSize, out c))
            {
                CreateAndSavePopulationMap(localGPWFile, c);
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, "Population map cell size is not a valid number, please check your input.");
            }
        }

        private static void CreateAndSavePopulationMap(string localGpwFile, float cellSize)
        {
            if (WUIEngine.RUNTIME_DATA.Population.LocalGPWData.LoadFromFile(localGpwFile))
            {
                WUIEngine.RUNTIME_DATA.Population.PopulationMap.CreateAndSave(WUIEngine.RUNTIME_DATA.Population.LocalGPWData, cellSize);
            }
        }

        public static bool LoadPopulationMap(string populationMapFile)
        {
            bool success = false;

            success = WUIEngine.RUNTIME_DATA.Population.PopulationMap.LoadFromFile(populationMapFile);

            return success;
        }

        public static bool ScaleTotalPopulation(string desiredPopulation)
        {
            bool success = false;
            int newPop;
            if (int.TryParse(desiredPopulation, out newPop))
            {
                success = ScaleTotalPopulation(newPop);
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, " New population count not a number, please check your input.");
            }

            return success;
        }

        public static bool ScaleTotalPopulation(int desiredPopulation)
        {
            bool success = false;

            if(WUIEngine.RUNTIME_DATA.Population.PopulationMap.HaveData)
            {
                WUIEngine.RUNTIME_DATA.Population.PopulationMap.ScaleTotalPopulation(desiredPopulation, true);
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, "No population map loaded, cannot scale.");
            }

            return success;
        }

        /// <summary>
        /// Filters the interpolated GPW data set to account for the user created population mask as well as checking for road access.
        /// </summary>
        public static void RoadAccessCorrectPopulationMap(string routerDbFile)
        {
            
            if (WUIEngine.RUNTIME_DATA.Population.PopulationMap.HaveData)
            {
                if(WUIEngine.RUNTIME_DATA.Routing.LoadRouterDb(routerDbFile))
                {
                    WUIEngine.RUNTIME_DATA.Population.PopulationMap.UpdatePopulationMapBasedOnRoadAccess(WUIEngine.RUNTIME_DATA.Routing.Router);
                }                
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, "No population map loaded, can't correct it for road access.");
            }
        }

        public static void ApplyPopulationMapMask(string populationMaskFile)
        {
            if(WUIEngine.RUNTIME_DATA.Population.PopulationMap.HaveData && WUIEngine.RUNTIME_DATA.Population.PopulationMap.LoadPopulationMask(populationMaskFile))
            {
                WUIEngine.RUNTIME_DATA.Population.PopulationMap.ApplyMaskToPopulation();
            }            
        }

        public static void SavePopulationMask()
        {
            WUIEngine.RUNTIME_DATA.Population.PopulationMap.SavePopulationMask(WUIEngine.INPUT.Simulation.SimulationID);
        }

        /*public static void LoadPopulationMask(string populationMaskFile)
        {
            WUIEngine.RUNTIME_DATA.Population.PopulationMap.LoadPopulationMask(populationMaskFile);
        }*/ 

        public static void CreateAndLoadPopulation()
        {
            if (WUIEngine.RUNTIME_DATA.Population.PopulationMap.HaveData && WUIEngine.RUNTIME_DATA.Population.PopulationMap.CorrectedForRoadAccess)
            {
                WUIEngine.RUNTIME_DATA.Population.PopulationMap.CreateAndLoadPopulation();
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, "Need population map that is corrected for road access, cannot create population.");
            }                     
        }

        public static bool CreateAndSaveRouterDb(string osmFile)
        {
            return WUIEngine.RUNTIME_DATA.Routing.CreateAndSaveRouterDb(osmFile);
        }

        public static bool LoadRouterDb(string routerDbFile)
        {

            return WUIEngine.RUNTIME_DATA.Routing.LoadRouterDb(routerDbFile);
        }

        public static bool FilterOsmData(string osmFile, string xBorder, string yBorder)
        {
            Vector2d osmFilterBorder;
            if (double.TryParse(xBorder, out osmFilterBorder.x) && double.TryParse(yBorder, out osmFilterBorder.y))
            {
                return FilterOsmData(osmFile, osmFilterBorder);
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, "Border is not a valid number, please check your input.");
            }

            return false;
        }

        private static bool FilterOsmData(string osmFile, Vector2d borderSize)
        {
            bool success = false;

            if (File.Exists(osmFile))
            {
                using (FileStream stream = new FileInfo(osmFile).OpenRead())
                {
                    PBFOsmStreamSource source = new PBFOsmStreamSource(stream);
                    float left = (float)(WUIEngine.INPUT.Simulation.LowerLeftLatLon.y - borderSize.x);
                    float bottom = (float)(WUIEngine.INPUT.Simulation.LowerLeftLatLon.x - borderSize.y);
                    Vector2d size = LocalGPWData.SizeToDegrees(WUIEngine.INPUT.Simulation.LowerLeftLatLon, WUIEngine.INPUT.Simulation.Size);
                    float right = (float)(WUIEngine.INPUT.Simulation.LowerLeftLatLon.y + size.x + borderSize.x);
                    float top = (float)(WUIEngine.INPUT.Simulation.LowerLeftLatLon.x + size.y + borderSize.y);
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
                WUIEngine.LOG(WUIEngine.LogType.Warning, " Could not find the selected OSM file.");
            }

            if (success)
            {
                WUIEngine.LOG(WUIEngine.LogType.Log, " Succesfully filtered OSM data to user selected boundary. Use this filtered data to build your router database.");
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, " Could not filter the selected OSM file.");
            }

            return success;
        }
    }   
}
