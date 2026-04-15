using PREACT.Math;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Collections.Generic;
using OsmSharp;
using OsmSharp.Streams;
using PREACT.Spatial;

namespace PREACT.Tools
{
    public static class OSMDownloader
    {
        public static async Task Download(Vector2d lowerLeftLatLon, Vector2d upperRightLatLon, string saveFilePath, int tries = 20)
        {
            try
            {
                OverpassClient client = new OverpassClient();                
                List<OsmGeo> data = await client.DownloadBoundingBoxAsync(lowerLeftLatLon.x, lowerLeftLatLon.y, upperRightLatLon.x, upperRightLatLon.y);

                using (var file = File.Create(saveFilePath))
                {
                    var target = new XmlOsmStreamTarget(file);
                    target.RegisterSource(data);
                    target.Pull();
                }
                Engine.Message(null, Engine.LogType.Log, "OSM data saved to " + saveFilePath);
            }
            catch (Exception e)
            {
                Engine.Message(null, Engine.LogType.Log, "OSM downloader says: " + e.Message);
                if(e.Message.Contains("504")) //timeout
                {
                    tries++;
                    if(tries < 5)
                    {
                        await Download(lowerLeftLatLon, upperRightLatLon, saveFilePath, tries);
                    }
                    
                }
            }
        }
    }
}
