namespace PREACT
{
    public interface IExternalManager
    {
        void UpdateInput(IO.PREACTInput input);
        void NewLogMessage(string message);
        void SimulationStarted();
        void SimulationsFinished();
        void StopSimulations();
        void PauseSimulations();
    }
}