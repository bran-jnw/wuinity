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

namespace WUIPlatform.Runtime
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
            WUIEngine.LOG(WUIEngine.LogType.Log, "Loading Routing data...");
        }

        public bool LoadRouterDb(string routerDbFile)
        {
            bool success = false;

            if (File.Exists(routerDbFile))
            {
                using (FileStream stream = new FileInfo(routerDbFile).OpenRead())
                {
                    _routerDb = RouterDb.Deserialize(stream);
                    success = true;
                }
            }

            if (success)
            {
                //some road networks returns zero routes without this contract being signed (especially Swedish road networks)...
                _routerDb.AddContracted(_routerDb.GetSupportedProfile("Car"));
                _router = new Router(_routerDb);
                WUIEngine.LOG(WUIEngine.LogType.Log, "Router database loaded succesfully.");
                
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, "Router database could not be found.");
            }

            return success;
        }

        public bool CreateAndSaveRouterDb(string osmFile)
        {
            bool success = false;

            if (File.Exists(osmFile))
            {
                //stream in data from OSM
                using (FileStream stream = new FileInfo(osmFile).OpenRead())
                {
                    OsmStreamSource source;
                    if (osmFile.EndsWith("pbf"))
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

                    //build db from OSM betwork
                    if (_routerDb == null)
                    {
                        _routerDb = new RouterDb();
                    }
                    _routerDb.LoadOsmData(source, settings, Vehicle.Car);
                    WUIEngine.LOG(WUIEngine.LogType.Log, "Router database created from OSM file.");

                    // write the new routerdb to disk.
                    string internalRouterName = WUIEngine.INPUT.Simulation.Id + ".routerdb";
                    osmFile = Path.Combine(WUIEngine.WORKING_FOLDER, internalRouterName);
                    using (FileStream outputStream = new FileInfo(osmFile).Open(FileMode.Create))
                    {
                        _routerDb.Serialize(outputStream);
                        WUIEngine.LOG(WUIEngine.LogType.Log, "Router database saved to file " + osmFile);
                    }

                    success = true;
                }
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, "Router database file could not be found.");
            }

            return success;
        }
    }
}