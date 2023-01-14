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
   internal class QueueingModel : IdentityRepository<QueueMessage, long>
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

         var targets = new List<QueueMessage>();

         foreach (var i in All())
         {
            if (i.Scope != scope || i.NextVisible > DateTime.UtcNow || i.Expire <= DateTime.UtcNow)
               continue;

            if (string.Compare(i.Queue ?? string.Empty, queue ?? string.Empty, true) == 0)
               targets.Add(i);
         }

         if (!targets.Any())
            return ImmutableList<QueueMessage>.Empty;

         var ordered = targets.OrderBy(f => f.NextVisible).ThenBy(f => f.Id);

         if (ordered.Count() <= count)
            return ordered.ToImmutableList();

         return ordered.Take(count).ToImmutableList();
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
