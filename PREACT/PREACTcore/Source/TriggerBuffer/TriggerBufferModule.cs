namespace PREACT
{
    public abstract class TriggerBufferModule
    {
        protected int[,] _triggerBufferOutput;

        public int[,] TriggerBufferOutput { get => _triggerBufferOutput; }    


        public abstract void Run();
    }
}