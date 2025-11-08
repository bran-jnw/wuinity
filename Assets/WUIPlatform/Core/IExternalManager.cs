namespace PREACT
{
    public interface IExternalManager
    {
        public void UpdateInput(IO.PREACTInput input);
        public void NewLogMessage(string message);
        public void SimulationStarted();
        public void SimulationStopped();
        public void StopSimulations();
        public void PauseSimulations();
    }
}