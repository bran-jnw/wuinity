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

            if(args.Length > 3)
            {
                Console.WriteLine("Press enter to close...");
                Console.ReadLine();
            }            
        }
    }
}
