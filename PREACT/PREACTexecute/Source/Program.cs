using System.Reflection;
using System.Runtime.Loader;

namespace PREACT
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // .NET 10 won't automatically probe the app directory for strongly-named
            // .NET Framework assemblies (e.g. GDAL C# bindings). Resolve them explicitly.
            AssemblyLoadContext.Default.Resolving += (context, name) =>
            {
                string appDir = AppContext.BaseDirectory;
                string candidate = Path.Combine(appDir, name.Name + ".dll");
                if (File.Exists(candidate))
                {
                    return context.LoadFromAssemblyPath(candidate);
                }
                return null;
            };

            PREACTexecute preact = new PREACTexecute();
            preact.Execute(args);
            while (!preact.IsDone)
            {
            }

            Console.WriteLine("Simulation run executed, shutting down.");
        }
    }
}
