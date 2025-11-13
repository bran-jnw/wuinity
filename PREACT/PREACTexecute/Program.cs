namespace PREACT
{
    internal class Program
    {
        static void Main(string[] args)
        {
            PREACTexecute preact = new PREACTexecute();
            preact.Execute(args);
            while (!preact.IsDone)
            {
            }

            Console.WriteLine("Simulation run executed, shutting down.");           
        }
    }
}
