using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Serialization;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;
using TomPIT.SysDb.Messaging;

namespace TomPIT.Sys.Model.Cdn
{
    internal class QueueingModel : SynchronizedRepository<IQueueMessage, string>
    {
        public const string Queue = "queueworker";

        public QueueingModel(IMemoryCache container) : base(container, "queueMessage")
        {
        }

        protected override void OnInitializing()
        {
            var ds = Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Query();

            foreach (var j in ds)
                Set(j.Id, j, TimeSpan.Zero);
        }

        protected override void OnInvalidate(string id)
        {
            var r = Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Select(id);

            if (r != null)
            {
                Set(id, r, TimeSpan.Zero);
                return;
            }

            Remove(id);
        }

        public void Enqueue(string queue, string message, string bufferKey, TimeSpan expire, TimeSpan nextVisible, QueueScope scope)
        {
            if (!string.IsNullOrWhiteSpace(bufferKey))
            {
                var existing = Where(f => string.Compare(f.Queue, queue, true) == 0 && string.Compare(f.BufferKey, bufferKey, true) == 0);

                if (existing.Count > 0)
                {
                    foreach (var ex in existing)
                    {
                        if (ex.NextVisible <= DateTime.UtcNow)
                            return;
                    }
                }
            }

            var descriptor = new QueueMessage
            {
                BufferKey = bufferKey,
                Created = DateTime.UtcNow,
                Expire = DateTime.UtcNow.Add(expire),
                Message = message,
                NextVisible = DateTime.UtcNow.Add(nextVisible),
                Queue = queue,
                Scope = scope
            };

            descriptor.Id = Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Insert(queue, message, bufferKey, expire, nextVisible, scope);

            Set(descriptor.Id, descriptor, TimeSpan.Zero);
        }

        public ImmutableList<IQueueMessage> Dequeue(int count, TimeSpan nextVisible, QueueScope scope, string queue)
        {
            ImmutableList<IQueueMessage> targets = SelectTargets(count, scope, queue);

            if (targets == null || targets.IsEmpty)
                return ImmutableList<IQueueMessage>.Empty;

            var results = new List<IQueueMessage>();
            foreach (var target in targets)
            {
                if (target is IQueueMessageModifier modifier && modifier.Modify(DateTime.UtcNow.Add(nextVisible), DateTime.UtcNow, target.DequeueCount + 1, Guid.NewGuid()))
                    results.Add(target);
            }

            Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Update(targets.ToList());

            foreach (IQueueMessageModifier result in results)
                result.Reset();

            return results.ToImmutableList();
        }

        private ImmutableList<IQueueMessage> SelectTargets(int count, QueueScope scope, string queue)
        {
            Initialize();

            if (Count == 0)
                return null;

            var targets = new List<IQueueMessage>();

            foreach (var i in All())
            {
                if (i.Scope != scope || i.NextVisible > DateTime.UtcNow || i.Expire <= DateTime.UtcNow)
                    continue;

                if (string.Compare(i.Queue ?? string.Empty, queue ?? string.Empty, true) == 0)
                    targets.Add(i);
            }
            //var targets = string.IsNullOrWhiteSpace(queue)
            //	? Where(f => f.Scope == scope && f.NextVisible <= DateTime.UtcNow && f.Expire > DateTime.UtcNow)
            //	: Where(f => f.Scope == scope && f.NextVisible <= DateTime.UtcNow && f.Expire > DateTime.UtcNow && string.Compare(f.Queue, queue, true) == 0);

            if (targets.Count == 0)
                return null;

            var ordered = targets.OrderBy(f => f.NextVisible).ThenBy(f => f.Id);

            if (ordered.Count() <= count)
                return ordered.ToImmutableList();

            return ordered.Take(count).ToImmutableList();
        }

        public void Ping(Guid popReceipt, TimeSpan nextVisible)
        {
            var message = Select(popReceipt);

            if (message == null)
                return;

            if (message is IQueueMessageModifier modifier)
                modifier.Modify(DateTime.UtcNow.Add(nextVisible), message.DequeueTimestamp, message.DequeueCount, message.PopReceipt);

            Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Update(new List<IQueueMessage> { message });
        }

        public void Complete(Guid popReceipt)
        {
            var message = Select(popReceipt);

            if (message == null)
                return;

            Remove(message.Id);

            Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Delete(message);
        }

        public IQueueMessage Select(Guid popReceipt)
        {
            return Get(f => f.PopReceipt == popReceipt);
        }

        public void Recycle()
        {
            Initialize();

            var orphanes = Where(f => f.Expire < DateTime.UtcNow);

            foreach (var message in orphanes)
            {
                Remove(message.Id);

                Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Delete(message);
            }
        }
    }
}
