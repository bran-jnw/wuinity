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

        public void ModifyRouterDB()
        {
            // DO NOT CALL this method yet... need to work out the structure
            // ... and call after each simulation loop OR if we're importing hazards, we could receive a poke as the hazard topuches the network
            // Route analysis: thought... potentially call itinero RemoveEdges or RemoveVertex
            // https://docs.itinero.tech/itinero/Itinero.Data.Network.RoutingNetwork.html#Itinero_Data_Network_RoutingNetwork_RemoveEdge_System_UInt32_
            // Find the nth edge or vertex to modify
            uint n=0;
            _routerDb.Network.RemoveEdge(n); //..(or RemoveVertex)

        }

        private void SortVertices()
        {
            // Route analysis:

            uint vCount = _routerDb.Network.VertexCount;
            for (uint i = 0; i < vCount; ++i)
            {
                var vertex = _routerDb.Network.GetVertex(i);
                var lati = vertex.Latitude;
                var longi = vertex.Longitude;

                //now find correct cell and add vertex index to list
                //private List<uint>[] _cellSortedVertices;

                _cellSortedVertices = new List<uint>[WUIEngine.SIM.FireModule.GetCellCountX() * WUIEngine.SIM.FireModule.GetCellCountY()];

                // Sort the vertices of the cells, based on location... to be completed
                // ... todo
            }
        }

        public void CheckIfVerticesAreBlocked()
        {
            // Route analysis:
            //after each fire update, check if any new burning cell has vertex

            bool modifiedNetwork = false;
            //if so, do this:
            for (int i = 0; i < _cellSortedVertices.Length; ++i)
            {
                // If this cell just received fire, then execute the next loop
                {
                    for (int j = 0; j < _cellSortedVertices[i].Count; ++j)
                    {
                        //vertex must stay otherwise index will be off, but seems OK to only remove edges
                        _routerDb.Network.RemoveEdges(_cellSortedVertices[i][j]);
                    }
                    modifiedNetwork = true;
                }

            }
            if(modifiedNetwork )
            {
                //then do clumbsy update as there is no way of forcing update of contracted network it seems
                var carProfile = RouterDb.GetSupportedProfile("Car");
                _routerDb.RemoveContracted(carProfile);
                _routerDb.AddContracted(carProfile);

                //if any changes, update everyone already inside the network by re-calculating their routes
                // code TODO
            }
        }
    }
}