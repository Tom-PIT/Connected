using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Exceptions;

namespace TomPIT.Distributed
{
    public abstract class Dispatcher<T> : IDispatcher<T>
    {
        private ConcurrentQueue<T> _items = null;
        private List<DispatcherJob<T>> _workers = null;
        private Lazy<ConcurrentDictionary<string, QueuedDispatcher<T>>> _queuedDispatchers = new Lazy<ConcurrentDictionary<string, QueuedDispatcher<T>>>();
        private bool _disposed = false;
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();

        protected Dispatcher(int workerSize)
        {
            WorkerSize = workerSize;
        }

        private CancellationTokenSource Cancel => _cancel;

        private int WorkerSize { get; }

        public abstract DispatcherJob<T> CreateWorker(IDispatcher<T> owner, CancellationToken cancel);

        public int Available => Math.Max(0, WorkerSize * 4) - Queue.Count - QueuedDispatchers.Sum(f => f.Value.Count);

        public bool Dequeue(out T item)
        {
            return Queue.TryDequeue(out item);
        }

        public bool Enqueue(string queue, T item)
        {
            if (EnsureDispatcher(queue) is not QueuedDispatcher<T> dispatcher)
                throw new RuntimeException($"{SR.ErrCannotCreateStackedDispatcher} ({queue})");

            return dispatcher.Enqueue(item);
        }

        public bool Enqueue(T item)
        {
            Queue.Enqueue(item);

            if (Jobs.Count < WorkerSize)
            {
                var worker = CreateWorker(this, Cancel.Token);

                worker.Completed += OnCompleted;

                lock (Jobs)
                {
                    Jobs.Add(worker);
                }

                worker.Run();
            }

            return true;
        }

        private void OnCompleted(object sender, EventArgs e)
        {
            try
            {
                if (sender is not DispatcherJob<T> job)
                    return;

                if (job.Success && Queue.IsEmpty)
                {
                    lock (Jobs)
                    {
                        Jobs.Remove(job);
                    }

                    job.Dispose();
                    job = null;
                }
                else
                    job.Run();
            }
            catch { }
        }

        private ConcurrentQueue<T> Queue
        {
            get
            {
                if (_items == null)
                    _items = new ConcurrentQueue<T>();

                return _items;
            }
        }

        private List<DispatcherJob<T>> Jobs
        {
            get
            {
                if (_workers == null)
                    _workers = new List<DispatcherJob<T>>();

                return _workers;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {

                    try
                    {
                        Cancel.Cancel();
                        Queue.Clear();

                        foreach (var job in Jobs)
                            job.Dispose();

                        Jobs.Clear();

                        foreach (var dispatcher in QueuedDispatchers)
                            dispatcher.Value.Dispose();

                        QueuedDispatchers.Clear();
                        Cancel.Dispose();
                    }
                    catch { }
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private QueuedDispatcher<T> EnsureDispatcher(string stack)
        {
            if (QueuedDispatchers.TryGetValue(stack, out QueuedDispatcher<T> result))
                return result;

            result = new QueuedDispatcher<T>(this, stack);

            result.Completed += OnQueuedCompleted;

            if (!QueuedDispatchers.TryAdd(stack, result))
            {
                if (QueuedDispatchers.TryGetValue(stack, out QueuedDispatcher<T> retryResult))
                    return retryResult;
                else
                    return null;
            }

            return result;
        }

        private void OnQueuedCompleted(object sender, EventArgs e)
        {
            var dispatcher = sender as QueuedDispatcher<T>;

            if (dispatcher.Count > 0)
                return;

            QueuedDispatchers.Remove(dispatcher.QueueName, out _);

            try
            {
                dispatcher.Dispose();
            }
            catch { }
        }

        private ConcurrentDictionary<string, QueuedDispatcher<T>> QueuedDispatchers => _queuedDispatchers.Value;

        public ProcessBehavior Behavior => ProcessBehavior.Parallel;
    }
}

