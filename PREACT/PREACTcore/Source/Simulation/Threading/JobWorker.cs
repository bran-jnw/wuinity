using System;
using System.Threading;

namespace PREACT
{
    public sealed class JobWorker
    {
        private readonly Thread _thread;
        private readonly JobQueue _queue;
        private readonly AutoResetEvent _wakeEvent = new AutoResetEvent(false);
        private readonly CancellationToken _token;

        public JobWorker(JobQueue queue, CancellationToken token, int index)
        {
            _queue = queue;
            _token = token;

            _thread = new Thread(Run)
            {
                IsBackground = true,
                Name = "JobWorker-" + index
            };

            _thread.Start();
        }

        public void Wake()
        {
            _wakeEvent.Set();
        }

        private void Run()
        {
            while (!_token.IsCancellationRequested)
            {
                _wakeEvent.WaitOne();

                while (!_token.IsCancellationRequested)
                {
                    if (_queue.TryDequeue(out IJob job))
                    {
                        try
                        {
                            job.Execute();
                        }
                        finally
                        {
                            _queue.JobCompleted();
                        }
                    }
                    else
                    {
                        break; // no more jobs
                    }
                }
            }
        }
    }
}
