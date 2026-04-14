using PREACT.Tools;

namespace FeatureTester
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await TestWorldPopDownloader();
        }

        static async Task TestWorldPopDownloader()
        {
            Console.WriteLine("Starting WorldPop download.");

            PREACT.Math.Vector2d lowerLeft = new PREACT.Math.Vector2d(39.41857677248346, -105.11157621791409);
            PREACT.Math.Vector2d upperRight = new PREACT.Math.Vector2d(39.5213373891081, -105.00632656522413);
            PREACT.Math.Vector2d center = (lowerLeft + upperRight) * 0.5;

            WorldPopDownloader client = new PREACT.Tools.WorldPopDownloader();            
            await client.DownloadRegionUTM(2015, lowerLeft, upperRight, "_output");
            Console.WriteLine("Finished WorldPop download.");
        }
    }
}
