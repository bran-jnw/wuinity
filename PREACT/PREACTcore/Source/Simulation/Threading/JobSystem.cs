using System;
using System.Threading;

namespace PREACT
{
    public sealed class JobSystem : IDisposable
    {
        private readonly JobQueue _queue = new JobQueue();
        private readonly JobWorker[] _workers;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public JobSystem(int workerCount)
        {
            if (workerCount <= 0)
            {
                workerCount = Environment.ProcessorCount;
            }                

            _workers = new JobWorker[workerCount];

            for (int i = 0; i < workerCount; i++)
            {
                _workers[i] = new JobWorker(_queue, _cts.Token, i);
            }                
        }

        public void Schedule(IJob job)
        {
            _queue.Enqueue(job);
        }

        public void Schedule(Action action)
        {
            Schedule(new LambdaJob(action));
        }

        public void ExecuteJobs()
        {
            // Wake all workers
            for (int i = 0; i < _workers.Length; i++)
            {
                _workers[i].Wake();
            }                

            // Spin until all jobs are done
            while (_queue.Pending > 0)
            {
                Thread.Yield();
            }
        }

        public void Dispose()
        {
            _cts.Cancel();

            for (int i = 0; i < _workers.Length; i++)
            {
                _workers[i].Wake();
            }                

            _cts.Dispose();
        }
    }
}
