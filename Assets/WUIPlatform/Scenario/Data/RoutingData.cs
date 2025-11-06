//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using Itinero;
using Reminiscence.Collections;
using System.IO;
using Itinero.IO.Osm;
using Itinero.Osm.Vehicles;
using OsmSharp.Streams;

namespace PREACT.Runtime
{
    public class RoutingData
    {
        public float BorderSize;

        private RouterDb _routerDb;
        public RouterDb RouterDb{ get => _routerDb; }

        private Router _router;
        public Router Router { get => _router; }

        private RouteCollection[] _routeCollections;
        public RouteCollection[] RouteCollections { get => _routeCollections; }

        // Add an array of the cell sorted vertices
        private List<uint>[] _cellSortedVertices;  

        public RoutingData()
        {

        }

        public void LoadAll()
        {
            Engine.MESSAGE(null, Engine.LogType.Log, "Loading Routing data...");
        }

        public static RouterDb LoadRouterDb(string filePath, out bool success)
        {
            success = false;

            RouterDb routerDb;
            if (File.Exists(filePath))
            {
                using (FileStream stream = new FileInfo(filePath).OpenRead())
                {
                    routerDb = RouterDb.Deserialize(stream);
                    success = true;
                }
            }

            if (success)
            {
                //some road networks returns zero routes without this contract being signed (especially Swedish road networks)...
                routerDb.AddContracted(routerDb.GetSupportedProfile("Car"));
                Engine.MESSAGE(null, Engine.LogType.Log, "Router database loaded succesfully.");
                
            }
            else
            {
                Engine.MESSAGE(null, Engine.LogType.Warning, "Router database could not be found.");
            }

            return routerDb;
        }

        public static bool CreateAndSaveRouterDb(string osmInputFile, string outputFile)
        {
            bool success = false;

            if (File.Exists(osmInputFile))
            {
                //stream in data from OSM
                using (FileStream stream = new FileInfo(osmInputFile).OpenRead())
                {
                    OsmStreamSource source;
                    if (osmInputFile.EndsWith("pbf"))
                    {
                        source = new PBFOsmStreamSource(stream);
                    }
                    else
                    {
                        source = new XmlOsmStreamSource(stream);
                    }

                    // create the network for cars only.
                    LoadSettings settings = new LoadSettings();
                    settings.KeepNodeIds = true; //use to enable measure flow at nodes
                    settings.KeepWayIds = true; //can be used to calc density easier?
                    settings.OptimizeNetwork = true;

                    //build db from OSM betwork, TODO: allocate och heap instead? keep track                    
                    RouterDb routerDb = new RouterDb();
                    routerDb.LoadOsmData(source, settings, Vehicle.Car);

                    // write the new routerdb to disk.
                    using (FileStream outputStream = new FileInfo(outputFile).Open(FileMode.Create))
                    {
                        routerDb.Serialize(outputStream);
                        Engine.MESSAGE(null, Engine.LogType.Log, "Router database saved to file " + outputFile);
                    }

                    success = true;
                }
            }
            else
            {
                Engine.MESSAGE(null, Engine.LogType.Warning, "Router database file could not be found.");
            }

            return success;
        }
    }
}