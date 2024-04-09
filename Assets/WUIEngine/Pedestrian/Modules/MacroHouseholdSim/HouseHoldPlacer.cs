using OsmSharp.Complete;
using OsmSharp.Streams;
using System.IO;
using System.Linq;

namespace WUIEngine.Pedestrian
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

