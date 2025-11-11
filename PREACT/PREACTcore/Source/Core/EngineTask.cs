namespace PREACT
{
    public class EngineTask
    {
        public enum ExecutionMode { Serial, Parallel, ParallelProcess };

        public ExecutionMode Execution = ExecutionMode.Serial;
        public int NumberOfRuns = 1;
        public bool StopAfterConverging = true;        
        public int ConvergenceMinSequence = 10;
        public float ConvergenceMaxDifference = 0.02f;

        public EngineTask()
        {

        }

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