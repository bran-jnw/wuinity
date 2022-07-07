namespace WUInity.Runtime
{
    public class SimulationData
    {
        public bool MultipleSimulations;
        public int NumberOfRuns = 100;
        public int ConvergenceMinSequence = 10;
        public float ConvergenceMaxDifference = 0.02f;
    }
}