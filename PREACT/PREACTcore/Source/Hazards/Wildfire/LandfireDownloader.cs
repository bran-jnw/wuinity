using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Math;
using PREACT.Math;

namespace PREACT.Wildfire
{
    class LandfireLandscapeDownloader
    {
        private static readonly HttpClient client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        const string EMAIL = "your_email@example.com";

        const int MAX_TOTAL_MINUTES = 30;
        const int POLL_SECONDS = 20;
        const int MAX_RETRIES = 5;

        public static async Task DownloadLandfire(Vector2d lowerLeftLatLon, Vector2d upperRighLatLon)
        {            
            string jobId = await SubmitJobWithRetryAsync(lowerLeftLatLon, upperRighLatLon);
            await PollUntilCompleteAsync(jobId);
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

        static async Task<string> SubmitJobWithRetryAsync(Vector2d lowerLeftLatLon, Vector2d upperRighLatLon)
        {
            int utmEpsg = GetUtmEpsgFromAoi(lowerLeftLatLon, upperRighLatLon);
            string products = "ELEV2020,SLPD2020,ASP2020,240FBFM40,240CC,240CH,240CBD,240CBH";
            string AOI = $"{lowerLeftLatLon.x},{lowerLeftLatLon.y},{upperRighLatLon.y},{upperRighLatLon.y}";

            string submitUrl = $"https://lfps.usgs.gov/lfps?" + $"products={products}" + $"&aoi={AOI}" + $"&projection={utmEpsg}" + $"&email={EMAIL}" + $"&background=true";

            for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
            {
                try
                {
                    Console.WriteLine($"Submitting LFPS job (attempt {attempt})…");
                    var response = await client.GetAsync(submitUrl);
                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);

                    string jobId = doc.RootElement.GetProperty("request").GetProperty("job_id").GetString();
                    Console.WriteLine($"✅ Job submitted. Job ID: {jobId}");
                    return jobId;
                }
                catch (Exception ex)
                {
                    if (attempt == MAX_RETRIES)
                        throw;

                    int backoff = attempt * 5;
                    Console.WriteLine($"Submit failed: {ex.Message}. Retrying in {backoff}s…");
                    await Task.Delay(TimeSpan.FromSeconds(backoff));
                }
            }

            throw new Exception("Job submission failed.");
        }

        static async Task PollUntilCompleteAsync(string jobId)
        {
            string statusUrl = $"https://lfps.usgs.gov/lfps/status/{jobId}";
            string outputZip = "landfire_landscape_utm.zip";

            DateTime start = DateTime.UtcNow;

            while (true)
            {
                if ((DateTime.UtcNow - start).TotalMinutes > MAX_TOTAL_MINUTES)
                    throw new TimeoutException("LFPS job exceeded max runtime.");

                try
                {
                    var response = await client.GetAsync(statusUrl);
                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);

                    string status = doc.RootElement.GetProperty("job_status").GetString();
                    Console.WriteLine($"Job status: {status}");

                    if (status == "complete")
                    {
                        string downloadUrl = doc.RootElement.GetProperty("download").GetProperty("url").GetString();

                        await DownloadWithRetryAsync(downloadUrl, outputZip);
                        Console.WriteLine($"✅ Landscape downloaded: {outputZip}");
                        return;
                    }

                    if (status == "failed")
                        throw new Exception("LFPS job failed.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Status check error: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromSeconds(POLL_SECONDS));
            }
        }

        static async Task DownloadWithRetryAsync(string url, string outputPath)
        {
            for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
            {
                try
                {
                    Console.WriteLine($"Downloading result (attempt {attempt})…");
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    using var fs = new FileStream(outputPath, FileMode.Create);
                    await response.Content.CopyToAsync(fs);
                    return;
                }
                catch
                {
                    if (attempt == MAX_RETRIES)
                        throw;

                    int backoff = attempt * 5;
                    await Task.Delay(TimeSpan.FromSeconds(backoff));
                }
            }
        }
    }
}
