namespace PREACT
{
    public struct EngineTask
    {
        public bool MultipleSimulations;
        public int NumberOfRuns;
        public bool StopAfterConverging;        
        public int ConvergenceMinSequence;
        public float ConvergenceMaxDifference;

        public EngineTask(bool multipleSimulations = true, int numberOfRuns = 50, bool stopAfterConverging = true, int convergenceMinSequence = 10, float convergenceMaxDifference = 0.02f)
        {
            MultipleSimulations = multipleSimulations;
            NumberOfRuns = numberOfRuns;
            StopAfterConverging = stopAfterConverging;
            ConvergenceMinSequence = convergenceMinSequence;
            ConvergenceMaxDifference = convergenceMaxDifference;
        }
    }
}