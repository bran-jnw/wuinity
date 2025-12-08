namespace PREACT
{
    public abstract class TriggerBufferModule
    {
        protected float[,] _triggerBufferOutput;

        public float[,] TriggerBufferOutput { get => _triggerBufferOutput; }    


        public abstract void Run();
    }
}