namespace PREACT
{
    public struct EngineTask
    {
        public enum ExecutionMode { Serial, Parallel, ParallelProcess };

        public ExecutionMode Execution;
        public int NumberOfRuns;
        public bool StopAfterConverging;        
        public int ConvergenceMinSequence;
        public float ConvergenceMaxDifference;

        public EngineTask(ExecutionMode Execution, int numberOfRuns, bool stopAfterConverging = true, int convergenceMinSequence = 10, float convergenceMaxDifference = 0.02f)
        {
            this.Execution = Execution;
            NumberOfRuns = numberOfRuns;
            StopAfterConverging = stopAfterConverging;
            ConvergenceMinSequence = convergenceMinSequence;
            ConvergenceMaxDifference = convergenceMaxDifference;
        }
    }
}