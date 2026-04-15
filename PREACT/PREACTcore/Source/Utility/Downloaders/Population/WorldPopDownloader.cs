using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using OSGeo.GDAL;
using System.Collections.Generic;
using System.Text.Json;
using PREACT.Math;

namespace PREACT.Tools
{
    public class WorldPopResponse
    {
        public string id { get; set; }
        public string title { get; set; }
        public string desc { get; set; }
        public string doi { get; set; }
        public string date { get; set; }
        public string popyear { get; set; }
        public string citation { get; set; }
        public string data_file { get; set; }
        public string archive { get; set; }
        public string @public { get; set; }
        public string source { get; set; }
        public string data_format { get; set; }
        public string author_email { get; set; }
        public string author_name { get; set; }
        public string maintainer_name { get; set; }
        public string maintainer_email { get; set; }
        public string project { get; set; }
        public string category { get; set; }
        public string gtype { get; set; }
        public string continent { get; set; }
        public string country { get; set; }
        public string iso3 { get; set; }
        public List<string> files { get; set; }
        public string url_img { get; set; }
        public string organisation { get; set; }
        public string license { get; set; }
        public string url_summary { get; set; }
    }

    public class WorldPopData
    {
        public List<WorldPopResponse> data { get; set; }
    }

    public static class WorldPopDownloader
    {
        private static readonly HttpClient _http = new HttpClient();
        private const string _worldPopApiUrl = "https://www.worldpop.org/rest/data/pop/wpgp";

        public static async Task<string> LatLonToISO3(double lat, double lon)
        {
            string url = $"https://api.bigdatacloud.net/data/reverse-geocode-client?latitude={lat}&longitude={lon}&localityLanguage=en";

            var json = await _http.GetStringAsync(url);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Example path: root["countryCode"] gives ISO2
            string iso2 = root.GetProperty("countryCode").GetString();

            // Convert ISO2 to ISO3
            var region = new System.Globalization.RegionInfo(iso2);

            return region.ThreeLetterISORegionName;
        }

        public static async Task<string> DownloadRegionUTM(int year, Vector2d lowerLeftLatLon, Vector2d upperRightLatLon, string outputFolder, string clippedFileName)
        {
            if(clippedFileName == null)
            {
                clippedFileName = "worldPop_clipped";
            }

            Vector2d center = (lowerLeftLatLon + upperRightLatLon) * 0.5;
            string iso3 = await LatLonToISO3(center.x, center.y);
            Engine.Message(null, Engine.LogType.Log, $"Identified ISO3: {iso3}, proceeding to download country WorldPop data, this will take a while if no local cache of country WorldPop is found in folder.");

            iso3 = iso3.ToUpperInvariant();
            string yearStr = year.ToString();

            //Fetch dataset list
            string url = $"{_worldPopApiUrl}?iso3={iso3}";
            using Stream stringStream = await _http.GetStreamAsync(url);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            WorldPopData datasets = await JsonSerializer.DeserializeAsync<WorldPopData>(stringStream, options);

            if (datasets == null || datasets.data.Count == 0)
            {
                throw new Exception("WorldPop returned no datasets.");
            }

            //select wanted data
            WorldPopResponse selected = null;
            for (int i = 0; i < datasets.data.Count; i++)
            {
                var d = datasets.data[i];

                if (d.popyear == yearStr)
                {
                    selected = d;
                    Engine.Message(null, Engine.LogType.Log, $"Selected data Id: {selected.id}, {selected.desc}.");
                    break;
                }
            }

            //could not find anything
            if (selected == null)
            {
                throw new Exception("No dataset found for " + iso3 + " in " + year + ".");
            }
            if (selected.data_file == null)
            {
                throw new Exception("No data files found for " + iso3 + " in " + year + ".");
            }

            //output
            Directory.CreateDirectory(outputFolder);
            string countryFilePath = Path.Combine(outputFolder, iso3 + "_" + selected.id + ".tif");
            if (!File.Exists(countryFilePath))
            {
                Engine.Message(null, Engine.LogType.Log, $"No local cache was found, downloading WorldPop for {iso3} and will the proceed to clip to AIO and warp to UTM zone.");
                //Download the GeoTIFF
                using (var stream = await _http.GetStreamAsync(selected.files[0]))
                {
                    using (var file = File.Create(countryFilePath))
                    {
                        await stream.CopyToAsync(file);
                    }
                }
            }

            string clippedFilePath = Path.Combine(outputFolder, clippedFileName + ".tif");
            return ExtractRegionAndProjectToUTM(countryFilePath, clippedFilePath, lowerLeftLatLon, upperRightLatLon);
        }

        static string ExtractRegionAndProjectToUTM(string countryFilePath, string clippedFilePath, Vector2d lowerLeftLatLon, Vector2d upperRightLatLon)
        {
            Engine.Message(null, Engine.LogType.Log, "Starting clipping WorldPop to AIO.");

            // Bounding box in the raster's coordinate system
            double west = lowerLeftLatLon.y;
            double south = lowerLeftLatLon.x;
            double east = upperRightLatLon.y;
            double north = upperRightLatLon.x;

            Dataset src = Gdal.Open(countryFilePath, Access.GA_ReadOnly);
            if (src == null)
            {
                Engine.Message(null, Engine.LogType.Log, "Could not open source country WorldPop.");
                return null;
            }

            double[] gt = new double[6];
            src.GetGeoTransform(gt);

            // Convert geospatial coords to pixel coords
            int pxMin = (int)((west - gt[0]) / gt[1]);
            int pxMax = (int)((east - gt[0]) / gt[1]);
            int pyMin = (int)((north - gt[3]) / gt[5]);
            int pyMax = (int)((south - gt[3]) / gt[5]);

            int width = pxMax - pxMin;
            int height = pyMax - pyMin;

            Driver drv = Gdal.GetDriverByName("GTiff");
            Dataset destinationDataSet = drv.Create(clippedFilePath, width, height, src.RasterCount, DataType.GDT_Float32, null);

            // New geotransform for the clipped raster, https://gdal.org/en/stable/tutorials/geotransforms_tut.html
            double[] newGT = new double[6];
            newGT[0] = gt[0] + pxMin * gt[1];
            newGT[1] = gt[1];
            newGT[2] = gt[2];
            newGT[3] = gt[3] + pyMin * gt[5];
            newGT[4] = gt[4];
            newGT[5] = gt[5];
            destinationDataSet.SetGeoTransform(newGT);
            destinationDataSet.SetProjection(src.GetProjection());

            // Copy each band
            for (int b = 1; b <= src.RasterCount; b++)
            {
                Band srcBand = src.GetRasterBand(b);
                Band dstBand = destinationDataSet.GetRasterBand(b);

                float[] buffer = new float[width * height];
                srcBand.ReadRaster(pxMin, pyMin, width, height, buffer, width, height, 0, 0);
                dstBand.WriteRaster(0, 0, width, height, buffer, width, height, 0, 0);
            }

            destinationDataSet.FlushCache();
            destinationDataSet.Dispose();
            src.Dispose();

            Engine.Message(null, Engine.LogType.Log, "Clipping WorldPop to AIO complete.");

            int zone = (int)Mathd.Floor(((east - west) * 0.5 + west + 180) / 6) + 1;
            string epsg;
            if (south >= 0.0)
            {
                epsg = "EPSG:" + (32600 + zone);   // northern hemisphere
            }
            else
            {
                epsg = "EPSG:" + (32700 + zone);   // southern hemisphere
            }

            string utmFilePath = Path.Combine(Path.GetDirectoryName(clippedFilePath), Path.GetFileNameWithoutExtension(clippedFilePath) + "_UTM.tif");
            ReprojectToUTM(clippedFilePath, utmFilePath, epsg);

            return utmFilePath;
        }

        public static void ReprojectToUTM(string sourceFilePath, string utmFilePath, string targetEPSG)
        {
            Gdal.AllRegister();

            Dataset src = Gdal.Open(sourceFilePath, Access.GA_ReadOnly);
            if (src == null)
            {
                throw new Exception("Could not open input raster.");
            }                

            var warpOptions = new GDALWarpAppOptions(new string[]
            {
                "-t_srs", targetEPSG,
                "-r", "near",
                "-overwrite"
            });

            Gdal.GDALProgressFuncDelegate progress = (pct, msg, data) => 1;
            string callbackData = "";

            //reprojection
            Dataset dst = Gdal.Warp(
                utmFilePath,          // destination filename
                new Dataset[] { src },       // source datasets
                warpOptions,
                progress,
                callbackData
            );

            if (dst == null)
            {
                throw new Exception("Warp failed.");
            }                

            dst.FlushCache();
            dst.Dispose();
            src.Dispose();
        }
    }
}
