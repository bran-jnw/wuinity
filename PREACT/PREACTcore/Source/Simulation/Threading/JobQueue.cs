using System;
using System.Collections.Generic;
using System.Threading;

namespace PREACT
{
    public sealed class JobQueue
    {
        private readonly Queue<IJob> _queue = new Queue<IJob>();
        private int _pending;

        public int Pending => _pending;

        public void Enqueue(IJob job)
        {
            Interlocked.Increment(ref _pending);

            lock (_queue)
            {
                _queue.Enqueue(job);
                Monitor.Pulse(_queue); // wake one worker
            }
        }

        public bool TryDequeue(out IJob job)
        {
            lock (_queue)
            {
                if (_queue.Count > 0)
                {
                    job = _queue.Dequeue();
                    return true;
                }
            }

            job = null;
            return false;
        }

        public void JobCompleted()
        {
            Interlocked.Decrement(ref _pending);
        }
    }
}
