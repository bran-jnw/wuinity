//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using PREACT.Population;
using OsmSharp.Streams;
using System.IO;
using PREACT.Input;
using PREACT.Math;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PREACT.Tools
{
    public static class PopulationTools
    {

        public static async Task CreateBaseScenario(string path, string scenarioId, int minHouseholdSize, int maxHouseholdSize, Vector2d lowerLeftLatLon, Vector2d upperRightLatLon, int year)
        {
            string osmFilePath = Path.Combine(path, scenarioId + "_osm.xml");
            await OSMDownloader.Download(lowerLeftLatLon, upperRightLatLon, osmFilePath);

            if(osmFilePath != null)
            {
                string sumoNetFilePath = Path.Combine(path, scenarioId + "_net.net.xml");
                Process.Start("netconvert", $"--osm {osmFilePath} -o {sumoNetFilePath}"); //this needs no callback
            }
            else
            {
                return;
            }

            string routerDbFilePath = Path.Combine(path, scenarioId + ".routerdb");
            RoutingData.CreateAndSaveRouterDb(osmFilePath, routerDbFilePath, out bool success);
            if (success)
            {
                Itinero.RouterDb routerDb = RoutingData.LoadRouterDb(routerDbFilePath, out success);
                if (success)
                {
                    string worldPopFilePath = await WorldPopDownloader.DownloadRegionUTM(year, lowerLeftLatLon, upperRightLatLon, path, null);

                    if(worldPopFilePath != null)
                    {
                        string populationFilePath = Path.Combine(path, scenarioId + "_population.csv");
                        PopulationMap.CreatePopulation(worldPopFilePath, populationFilePath, routerDb, minHouseholdSize, maxHouseholdSize, out success);
                    }                        
                }
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
                Engine.Message(null, Engine.LogType.Warning, "No data in population map, cannot scale.");
            }
        }

        public static void CreatePopulationFromWorldPop(string minHouseholdSize, string maxHouseholdSize, string worldPopFilePath, string routerDbFilePath, string outputFilePath, out bool success)
        {
            success = false;

            int min, max;
            if (int.TryParse(minHouseholdSize, out min) && int.TryParse(maxHouseholdSize, out max) && min <= max)
            {
                Itinero.RouterDb routerDb = RoutingData.LoadRouterDb(routerDbFilePath, out success);
                if(success)
                {
                    PopulationMap.CreatePopulation(worldPopFilePath, outputFilePath, routerDb, min, max, out success);
                }                
            }
            else
            {
                Engine.Message(null, Engine.LogType.Warning, "Could not parse min and/or max household size.");
            }
        }

        public static void CreateAndSaveRouterDb(string osmInputFile, string outputFile, out bool success)
        {
            RoutingData.CreateAndSaveRouterDb(osmInputFile, outputFile, out success);
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
                Engine.Message(null, Engine.LogType.Warning, "Border is not a valid number, please check your input.");
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
                    if (osmFile.ToLower().EndsWith("pbf"))
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
                Engine.Message(null, Engine.LogType.Warning, " Could not find the selected OSM file.");
            }

            if (success)
            {
                Engine.Message(null, Engine.LogType.Log, " Succesfully filtered OSM data to user selected boundary. Use this filtered data to build your router database.");
            }
            else
            {
                Engine.Message(null, Engine.LogType.Warning, " Could not filter the selected OSM file.");
            }

            return success;
        }
    }   
}
