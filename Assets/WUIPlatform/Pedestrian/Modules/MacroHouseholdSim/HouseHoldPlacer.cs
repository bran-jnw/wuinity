//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using OsmSharp.Complete;
using OsmSharp.Streams;
using System.IO;
using System.Linq;

namespace WUIPlatform.Pedestrian
{
    public class HouseHoldPlacer
    {
        public HouseHoldPlacer(string osmPBFFile) 
        {
            //get total amount of desired people per square

            //go through OSM and find residential buildings
            using (var fileStream = new FileInfo(osmPBFFile).OpenRead())
            {
                PBFOsmStreamSource source = new PBFOsmStreamSource(fileStream);

                // Get all building ways and nodes 
                var buildings = from osmGeo in source
                                where osmGeo.Type == OsmSharp.OsmGeoType.Node ||
                                (osmGeo.Type == OsmSharp.OsmGeoType.Way && osmGeo.Tags != null && osmGeo.Tags.ContainsKey("building"))
                                select osmGeo;

                // Should filter before calling ToComplete() to reduce memory usage
                var completes = buildings.ToComplete(); // Create Complete objects (for Ways gives them a list of Node objects)
                var ways = from osmGeo in completes
                           where osmGeo.Type == OsmSharp.OsmGeoType.Way
                           select osmGeo;
                CompleteWay[] completeWays = ways.Cast<CompleteWay>().ToArray();
            }

            //place people at building locations
        }
    }
}

