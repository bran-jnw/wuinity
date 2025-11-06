namespace PREACT
{
    public interface IExternalManager
    {
        public void InputHasChanged();
        public void UpdateMap();
        public void NewLogMessage(string message);
        public void SimulationStarted();
        public void SimulationStopped();
        public void StopSimulations();
        public void PauseSimulations();
    }
}