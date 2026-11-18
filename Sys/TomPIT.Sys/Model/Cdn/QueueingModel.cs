using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.Caching;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Caching;

namespace TomPIT.Sys.Model.Cdn
{
   public class QueueingModel : IdentityRepository<QueueMessage, long>
   {
      public const string Queue = "queueworker";

      public QueueingModel(IMemoryCache container) : base(container, "queueMessage")
      {
      }

      protected override void OnInitializing()
      {
         foreach (var j in Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Query())
         {
            if (j.Id > Identity)
               Seed(j.Id);

            Set(j.Id, new QueueMessage(j), TimeSpan.Zero);
         }

         Debug.WriteLine($"Initialized {Count} messages.", "Queue");
      }

      public void Enqueue(string queue, string message, string bufferKey, TimeSpan expire, TimeSpan nextVisible, QueueScope scope)
      {
         Initialize();
         /*
          * This is performance optimization.
          * We only enqueue one message with the same message argument at the same time if the buffer key has been passed.
          */
         if (!string.IsNullOrWhiteSpace(bufferKey))
         {
            var existing = Where(f => string.Compare(f.Queue, queue, true) == 0 && string.Compare(f.BufferKey, bufferKey, true) == 0);
            /*
             * Entry exists. Let's find out if we have one with the same message argument.
             */
            if (existing.Any())
            {
               foreach (var ex in existing)
               {
                  if (string.Equals(message, ex.Message, StringComparison.Ordinal))
                  {
                     /*
                      * Exists. Queueing a new item would mean the client would process the same thing more than once
                      * and this is not suppported in buffer mode. If the client needs to process every single entry regardless
                      * of duplicates it should not use the buffer mode.
                      */
                     return;
                  }
               }
            }
         }

         var descriptor = new QueueMessage
         {
            Id = Increment(),
            Message = message,
            Created = DateTime.UtcNow,
            Expire = DateTime.UtcNow.Add(expire),
            NextVisible = DateTime.UtcNow.Add(nextVisible),
            Queue = queue,
            Scope = scope,
            BufferKey = bufferKey,
         };

         Set(descriptor.Id, descriptor, TimeSpan.Zero);

         Dirty();
      }

      public ImmutableList<IQueueMessage> Dequeue(int count, TimeSpan nextVisible, QueueScope scope, string queue)
      {
#if DEBUG
         var sw = new Stopwatch();

         sw.Start();
#endif

         var results = new List<IQueueMessage>();

         var targets = SelectTargets(count, scope, queue);

         if (targets.IsEmpty)
         {
#if DEBUG
            sw.Stop();

            if (sw.ElapsedMilliseconds > 1)
               Debug.WriteLine($"Dequeue {queue} {sw.ElapsedMilliseconds:n0} ms. (empty)", "Queue");
#endif
            return ImmutableList<IQueueMessage>.Empty;
         }

         foreach (var target in targets)
         {
            try
            {
               target.BeginEdit();

               if (target.NextVisible > DateTime.UtcNow)
               {
                  target.EndEdit();
                  continue;
               }

               target.NextVisible = DateTime.UtcNow.Add(nextVisible);
               target.DequeueTimestamp = DateTime.UtcNow;
               target.DequeueCount++;
               target.PopReceipt = Guid.NewGuid();

               results.Add(target);

               Dirty();

               target.EndEdit();
            }
            catch
            {
               continue;
            }
         }

#if DEBUG
         sw.Stop();

         Debug.WriteLine($"Dequeue {queue} {sw.ElapsedMilliseconds:n0} ms.", "Queue");
#endif

         return results.ToImmutableList();
      }

      private ImmutableList<QueueMessage> SelectTargets(int count, QueueScope scope, string queue)
      {
         Initialize();

         if (Count == 0)
            return ImmutableList<QueueMessage>.Empty;

         /*
          * Fair scheduling applies only to the queueworker queue, the only one whose payload
          * carries component/worker. Every other queue keeps plain (NextVisible, Id) order.
          */
         var fair = string.Equals(queue, Queue, StringComparison.OrdinalIgnoreCase);
         var load = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
         var targets = fair ? null : new List<QueueMessage>();
         /*
          * A worker can never receive more than count messages in one batch, so each worker
          * keeps only its count oldest candidates (bounded max-heap). The scan stays O(n log count)
          * and memory O(workers * count) however large one worker's backlog is.
          */
         var buckets = fair ? new Dictionary<string, PriorityQueue<QueueMessage, (DateTime NextVisible, long Id)>>(StringComparer.OrdinalIgnoreCase) : null;
         var now = DateTime.UtcNow;

         if (count <= 0)
            return ImmutableList<QueueMessage>.Empty;

         foreach (var i in All())
         {
            if (i.Scope != scope || i.Expire <= now)
               continue;

            if (string.Compare(i.Queue ?? string.Empty, queue ?? string.Empty, true) != 0)
               continue;

            if (i.NextVisible > now)
            {
               /*
                * Invisible after at least one dequeue: either still being processed or backing
                * off after a failure. Both count as load on that worker.
                */
               if (fair && i.DequeueCount > 0)
               {
                  var key = i.WorkerKey ?? string.Empty;

                  load[key] = load.GetValueOrDefault(key) + 1;
               }

               continue;
            }

            if (!fair)
            {
               targets.Add(i);
               continue;
            }

            var worker = i.WorkerKey ?? string.Empty;

            if (!buckets.TryGetValue(worker, out var heap))
               buckets[worker] = heap = new PriorityQueue<QueueMessage, (DateTime NextVisible, long Id)>(NewestFirst);

            heap.Enqueue(i, (i.NextVisible, i.Id));

            if (heap.Count > count)
               heap.Dequeue();
         }

         if (!fair)
         {
            return targets.OrderBy(f => f.NextVisible).ThenBy(f => f.Id).Take(count).ToImmutableList();
         }

         if (buckets.Count == 0)
            return ImmutableList<QueueMessage>.Empty;

         /*
          * Each pick goes to the worker with the lowest load (in flight or backing off, plus
          * picked in this batch), oldest message first within a worker. A worker with a pile of
          * failing messages therefore yields to the others even when only one slot is free, and
          * a worker that is alone with pending work still gets every slot.
          */
         var keys = buckets.Keys.ToList();
         var groups = keys
            .Select(k => buckets[k].UnorderedItems.Select(f => f.Element).OrderBy(f => f.NextVisible).ThenBy(f => f.Id).ToList())
            .ToList();
         var next = new int[groups.Count];
         var selected = new List<QueueMessage>(count);

         while (selected.Count < count)
         {
            var best = -1;
            var bestLoad = 0;

            for (var g = 0; g < groups.Count; g++)
            {
               if (next[g] >= groups[g].Count)
                  continue;

               var current = load.GetValueOrDefault(keys[g]);

               if (best < 0 || current < bestLoad || (current == bestLoad && Precedes(groups[g][next[g]], groups[best][next[best]])))
               {
                  best = g;
                  bestLoad = current;
               }
            }

            if (best < 0)
               break;

            selected.Add(groups[best][next[best]++]);
            load[keys[best]] = bestLoad + 1;
         }

         return selected.OrderBy(f => f.NextVisible).ThenBy(f => f.Id).ToImmutableList();
      }

      /* Max-heap order: the newest (NextVisible, Id) is evicted first, so the oldest are kept. */
      private static readonly IComparer<(DateTime NextVisible, long Id)> NewestFirst =
         Comparer<(DateTime NextVisible, long Id)>.Create((a, b) => b.CompareTo(a));

      private static bool Precedes(QueueMessage a, QueueMessage b)
      {
         return a.NextVisible < b.NextVisible || (a.NextVisible == b.NextVisible && a.Id < b.Id);
      }

      public void Ping(Guid popReceipt, TimeSpan nextVisible)
      {
         if (Select(popReceipt) is not QueueMessage message)
            return;

         try
         {
            message.BeginEdit();
            message.NextVisible = DateTime.UtcNow.Add(nextVisible);

            Dirty();

            message.EndEdit();
         }
         catch { }
      }

      public void Complete(Guid popReceipt)
      {
         if (Select(popReceipt) is not QueueMessage message)
            return;

         Remove(message.Id);
         Dirty();
      }

      public IQueueMessage Select(Guid popReceipt)
      {
         return Get(f => f.PopReceipt == popReceipt);
      }

      public ImmutableList<IQueueMessage> QueryByBufferKey(string bufferKey)
      {
         return Where(f => string.Equals(f.BufferKey, bufferKey, StringComparison.OrdinalIgnoreCase)).ToImmutableList<IQueueMessage>();
      }

      protected override async Task OnFlushing()
      {
         var messages = All();
         var items = new List<IQueueMessage>();

         foreach (var message in messages)
         {
            if (message.Expire < DateTime.UtcNow)
               Remove(message.Id);

            items.Add(message);
         }

#if DEBUG
         var sw = new Stopwatch();

         sw.Start();
#endif
         await Task.Run(() => Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Update(items));

         await Task.CompletedTask;
#if DEBUG
         sw.Stop();

         Debug.WriteLine($"Queue update {sw.ElapsedMilliseconds:n0} ms. {messages.Count} items (with orphanes).", "Queue");

         //sw.Reset();
         //sw.Start();
         //await File.WriteAllTextAsync(Path.Combine(System.Environment.GetFolderPath(SpecialFolder.LocalApplicationData), "Queue.json"), JsonConvert.SerializeObject(items));
         //sw.Stop();

         //Debug.WriteLine($"Queue update file {sw.ElapsedMilliseconds:n0} ms. {messages.Count} items (with orphanes).", "Queue");
#endif
      }
   }
}
