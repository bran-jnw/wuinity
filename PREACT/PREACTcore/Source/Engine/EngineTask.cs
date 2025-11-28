namespace PREACT
{
    public class EngineTask
    {
        public enum ExecutionMode { Serial, Parallel, ParallelProcess };

        public ExecutionMode Execution = ExecutionMode.Serial;
        public int NumberOfRuns = 1;
        public int BatchSize = 4;
        public bool Visualize = true;
        public bool StopAfterConverging = true;        
        public int ConvergenceMinSequence = 10;
        public float ConvergenceMaxDifference = 0.02f;
        public int SimulationIndexOffset = 0;

        public EngineTask()
        {

        }

        public EngineTask(ExecutionMode Execution, int numberOfRuns, int simulationIndexOffset = 0, int batchSize = 4, bool visualize = true, bool stopAfterConverging = true, int convergenceMinSequence = 10, float convergenceMaxDifference = 0.02f)
        {
            this.Execution = Execution;
            NumberOfRuns = numberOfRuns;
            SimulationIndexOffset = simulationIndexOffset;
            BatchSize = batchSize;
            Visualize = visualize;
            StopAfterConverging = stopAfterConverging;
            ConvergenceMinSequence = convergenceMinSequence;
            ConvergenceMaxDifference = convergenceMaxDifference;
        }
    }
}