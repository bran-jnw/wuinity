namespace PREACT
{
    public interface ExternalManager
    {
        public void InputHasChanged();
        public void UpdateMap();
        public void NewLogMessage(string message);
    }
}