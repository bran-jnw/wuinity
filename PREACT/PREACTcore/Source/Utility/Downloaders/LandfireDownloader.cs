using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Math;
using PREACT.Math;
using System.IO.Compression;

namespace PREACT.Tools
{
    public static class LandfireLandscapeDownloader
    {
        private static readonly HttpClient client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        const string EMAIL = "jonathan.wahlqvist@brand.lth.se";

        const int MAX_TOTAL_MINUTES = 30;
        const int POLL_SECONDS = 20;
        const int MAX_RETRIES = 5;
        static int[] _availableYears = new int[] { 2016, 2020, 2023, 2024 };

        public static async Task Download(int year, bool useAnderson13, Vector2d lowerLeftLatLon, Vector2d upperRighLatLon, string downloadFolder)
        {            
            string jobId = await SubmitJobWithRetryAsync(year, useAnderson13, lowerLeftLatLon, upperRighLatLon);
            await PollUntilCompleteAsync(jobId, downloadFolder);
        }

        private static int GetUtmEpsgFromAoi(Vector2d lowerLeftLatLon, Vector2d upperRighLatLon)
        {

            double centerLon = (lowerLeftLatLon.y + upperRighLatLon.y) / 2.0;
            double centerLat = (lowerLeftLatLon.x + upperRighLatLon.x) / 2.0;

            int utmZone = (int)Floor((centerLon + 180) / 6) + 1;
            bool north = centerLat >= 0;

            int epsg = north ? 32600 + utmZone : 32700 + utmZone;
            return epsg;
        }

        static string BuildLandscapeLayerList(int year, bool useAnderson13)
        {
            string fuelModel;
            if(useAnderson13)
            {
                fuelModel = $"LF{year}_FBFM13";
            }
            else
            {
                fuelModel = $"LF{year}_FBFM40";
            }

            return string.Join(";", $"LF2020_Elev", $"LF2020_SlpD", $"LF2020_Asp", fuelModel , $"LF{year}_CC", $"LF{year}_CH", $"LF{year}_CBH", $"LF{year}_CBD", "LF2023_FCCS");
        }
        
        private static async Task<string> SubmitJobWithRetryAsync(int year, bool useAnderson13, Vector2d lowerLeftLatLon, Vector2d upperRighLatLon)
        {
            int utmEpsg = GetUtmEpsgFromAoi(lowerLeftLatLon, upperRighLatLon);

            int closestYear = _availableYears[0];
            int minDiff = Mathd.Abs(year - closestYear);
            for(int i = 1; i < _availableYears.Length; ++i)
            {
                int diff = Mathd.Abs(year - _availableYears[i]);
                if(diff <= minDiff)
                {
                    closestYear = _availableYears[i];
                }
            }
            Engine.Message(null, Engine.LogType.Log, $"Requesting data from Landfire version {closestYear} which was closest to the requested {year}.");

            //
            string products = BuildLandscapeLayerList(closestYear, useAnderson13);
            string AOI = $"{lowerLeftLatLon.y}%20{lowerLeftLatLon.x}%20{upperRighLatLon.y}%20{upperRighLatLon.x}";


            string submitUrl = "https://lfps.usgs.gov/api/job/submit?" + $"Output_Projection={utmEpsg}" + $"&Layer_List={products}" + $"&Area_of_Interest={AOI}" + $"&Email={EMAIL}";

            Engine.Message(null, Engine.LogType.Log, "Job spec. built, will now submit job " + submitUrl);

            for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
            {
                try
                {
                    Engine.Message(null, Engine.LogType.Log, $"Submitting LFPS job (attempt {attempt}.");
                    var response = await client.GetAsync(submitUrl);
                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);

                    string jobId = doc.RootElement.GetProperty("jobId").GetString();
                    Engine.Message(null, Engine.LogType.Log, $"Job submitted. Job ID: {jobId}");
                    return jobId;
                }
                catch (Exception ex)
                {
                    if (attempt == MAX_RETRIES)
                        throw;

                    int backoff = attempt * 5;
                    Engine.Message(null, Engine.LogType.Log, $"Submit failed: {ex.Message}. Retrying in {backoff}s.");
                    await Task.Delay(TimeSpan.FromSeconds(backoff));
                }
            }

            throw new Exception("Job submission failed.");
        }

        static async Task PollUntilCompleteAsync(string jobId, string downloadFolder)
        {
            string statusUrl = $"https://lfps.usgs.gov/api/job/status?JobId={jobId}";
            string outputZip = Path.Combine(downloadFolder, $"{jobId}.zip");

            DateTime start = DateTime.UtcNow;

            while (true)
            {
                if ((DateTime.UtcNow - start).TotalMinutes > MAX_TOTAL_MINUTES)
                {
                    throw new TimeoutException("LFPS job exceeded max runtime.");
                }                    

                try
                {
                    var response = await client.GetAsync(statusUrl);
                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);

                    string status;
                    if (doc.RootElement.TryGetProperty("jobStatus", out JsonElement js))
                    {
                        status = js.GetString();
                    }
                    else if (doc.RootElement.TryGetProperty("status", out JsonElement s))
                    {
                        status = s.GetString();
                    }
                    else
                    {
                        throw new Exception(
                            "LFPS status response contains neither 'jobStatus' nor 'status'.\n" + json);
                    }

                    int queue = doc.RootElement.GetProperty("queuePosition").GetInt32();
                    Engine.Message(null, Engine.LogType.Log, $"Job status: {status}, queue position: {queue}");

                    if (status == "Succeeded")
                    {
                        string downloadUrl = doc.RootElement.GetProperty("outputFile").GetString();

                        await DownloadWithRetryAsync(downloadUrl, outputZip);
                        Engine.Message(null, Engine.LogType.Log, $"Landscape downloaded: {outputZip}");
                        return;
                    }

                    if (status == "Failed")
                    {
                        throw new Exception("LFPS job failed.");
                    }
                }
                catch (Exception ex)
                {
                    Engine.Message(null, Engine.LogType.Log, $"Status check error: {ex.Message}");
                    return;
                }

                await Task.Delay(TimeSpan.FromSeconds(POLL_SECONDS));
            }
        }

        static async Task DownloadWithRetryAsync(string url, string outputPath)
        {
            bool success = false;

            for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
            {
                try
                {
                    Engine.Message(null, Engine.LogType.Log, $"Downloading result (attempt {attempt}).");
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    using var fs = new FileStream(outputPath, FileMode.Create);
                    await response.Content.CopyToAsync(fs);
                    success = true;
                    break;
                }
                catch
                {
                    if (attempt == MAX_RETRIES)
                        throw;

                    int backoff = attempt * 5;
                    await Task.Delay(TimeSpan.FromSeconds(backoff));
                }
            }

            if(success)
            {
                UnZip(outputPath);
            }
        }

        private static void UnZip(string zipPath)
        {
            string extractPath = Path.GetDirectoryName(zipPath);

            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith(".tif", StringComparison.OrdinalIgnoreCase))
                    {
                        // Gets the full path to ensure that relative segments are removed.
                        string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));

                        // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
                        // are case-insensitive.
                        if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
                        {
                            entry.ExtractToFile(destinationPath);
                        }                            
                    }
                }
            }
        }
    }
}
