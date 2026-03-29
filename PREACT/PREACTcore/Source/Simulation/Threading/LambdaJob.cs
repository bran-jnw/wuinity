using System;

namespace PREACT
{
    public interface IJob
    {
        void Execute();
    }

    public sealed class LambdaJob : IJob
    {
        private readonly Action _action;

        public LambdaJob(Action action) => _action = action;

        public void Execute() => _action();
    }
}
