using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OsmSharp;
using OsmSharp.Streams;

namespace PREACT.Spatial
{
    /// <summary>
    /// Overpass is used to download OSM data as the default OSM server does not allow big enough areas.
    /// </summary>
    public class OverpassClient
    {
        private readonly HttpClient _http;

        public OverpassClient()
        {
            _http = new HttpClient();
        }

        public async Task<List<OsmGeo>> DownloadBoundingBoxAsync(double south, double west, double north, double east)
        {
            // Build Overpass query
            string query =
                "[out:xml][timeout:180];(" +
                "node(" + south + "," + west + "," + north + "," + east + ");" +
                "way(" + south + "," + west + "," + north + "," + east + ");" +
                "relation(" + south + "," + west + "," + north + "," + east + ");" +
                ");" +
                "(._;>;>>;);" +
                "out body;";

            // Send request
            var content = new StringContent(query, Encoding.UTF8,
                "application/x-www-form-urlencoded");

            var response = await _http.PostAsync(
                "https://overpass-api.de/api/interpreter", content);

            response.EnsureSuccessStatusCode();

            string xml = await response.Content.ReadAsStringAsync();

            // Parse XML into OsmGeo objects
            var result = new List<OsmGeo>();

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                using (var source = new XmlOsmStreamSource(ms))
                {
                    foreach (var osmGeo in source)
                    {
                        result.Add(osmGeo);
                    }
                }
            }

            return result;            
        }
    }
}

