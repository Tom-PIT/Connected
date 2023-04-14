using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using TomPIT.Exceptions;

namespace TomPIT.Distributed
{
   public abstract class Dispatcher<T> : IDispatcher<T>
   {
      private bool _disposed = false;
      private readonly CancellationTokenSource _cancel = new CancellationTokenSource();

      protected Dispatcher(int workerSize)
      {
         WorkerSize = workerSize;
         Queue = new();
         Jobs = new();
         QueuedDispatchers = new();
      }

      private CancellationTokenSource Cancel => _cancel;
      private int WorkerSize { get; }
      public ConcurrentQueue<T> Queue { get; }
      public List<DispatcherJob<T>> Jobs { get; }
      public int Available => Math.Max(0, WorkerSize * 4 - Queue.Count - QueuedDispatchers.Values.Sum(f => f.Count));
      public ConcurrentDictionary<string, QueuedDispatcher<T>> QueuedDispatchers { get; }
      public ProcessBehavior Behavior => ProcessBehavior.Parallel;
      public abstract DispatcherJob<T> CreateWorker(IDispatcher<T> owner, CancellationToken cancel);
      public bool Dequeue(out T item)
      {
         return Queue.TryDequeue(out item);
      }

      public bool Enqueue(string key, T item)
      {
         if (EnsureDispatcher(key) is not QueuedDispatcher<T> dispatcher)
            throw new RuntimeException($"{SR.ErrCannotCreateStackedDispatcher} ({key})");

         return dispatcher.Enqueue(item);
      }

      public bool Enqueue(T item)
      {
         Queue.Enqueue(item);

         lock (Jobs)
         {
            var jobs = Jobs.Count;
            var items = Queue.Count;

            if (jobs > items && jobs <= WorkerSize)
               return true;
         }

         CreateWorker();

         return true;
      }

      private void CreateWorker()
      {
         var worker = CreateWorker(this, Cancel.Token);

         worker.Completed += OnCompleted;

         worker.Run();
         
         lock (Jobs)
         {
            Jobs.Add(worker);
         }
      }

      private void OnCompleted(object sender, EventArgs e)
      {
         if (sender is not DispatcherJob<T> job)
            return;

         try
         {
            if (Queue.IsEmpty)
               DisposeJob(job);
            else
               job.Run();
         }
         catch (Exception ex)
         {
            DisposeJob(job);

            Debug.WriteLine(ex.Message, "Dispatcher Completed Exception");
         }
      }

      private void DisposeJob(DispatcherJob<T> job)
      {
         lock (Jobs)
         {
            Jobs.Remove(job);
         }

         job.Completed -= OnCompleted;
         job.Dispose();
         job = null;

         if (Jobs.Count == 0 && !Queue.IsEmpty)
            CreateWorker();
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
         GC.SuppressFinalize(this);
      }

      private QueuedDispatcher<T> EnsureDispatcher(string key)
      {
         if (QueuedDispatchers.TryGetValue(key, out QueuedDispatcher<T> result))
            return result;

         result = new QueuedDispatcher<T>(this, key);

         result.Completed += OnQueuedCompleted;

         if (!QueuedDispatchers.TryAdd(key, result))
         {
            result.Completed -= OnQueuedCompleted;

            result.Dispose();

            if (QueuedDispatchers.TryGetValue(key, out QueuedDispatcher<T> retryResult))
               return retryResult;
            else
               return null;
         }

         return result;
      }

      private void OnQueuedCompleted(object sender, EventArgs e)
      {
         var dispatcher = sender as QueuedDispatcher<T>;
         /*
			 * If service has enqueued an item in the meantime we do nothing just keep the
			 * current queued dispatcher alive.
			 */
         if (dispatcher.Count > 0)
            return;
         /*
			 * There are no items yet in the queue so we can try to remove it and prepare it for
			 * dispose.
			 */
         if (QueuedDispatchers.Remove(dispatcher.QueueName, out QueuedDispatcher<T> removed))
         {
            /*
				 * It's still possible that an item has been enqueued while the dispatcher has been
				 * removing so it's crucial to check for this scenario.
				 */
            if (removed.Count > 0)
            {
               /*
					 * This is not very likely but possible. It's also possible that a dispatcher with the
					 * same name has been added too so we'll just find a key that doesn't exist yet to 
					 * keep the dispatcher alive utils it finishes the job.
					 */
               while (!QueuedDispatchers.TryAdd($"{removed.QueueName}_{Guid.NewGuid()}", removed))
               {
               }

               return;
            }
         }

         try
         {
            dispatcher.Dispose();
         }
         catch { }
      }
   }
}

