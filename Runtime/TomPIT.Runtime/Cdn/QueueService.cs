using System;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;
using TomPIT.Middleware;
using TomPIT.Serialization;

namespace TomPIT.Cdn
{
    internal class QueueService : TenantObject, IQueueService
    {
        public QueueService(ITenant tenant) : base(tenant)
        {
        }

        public void Enqueue<T>(IQueueWorker worker, string bufferKey, T arguments)
        {
            Enqueue(worker, bufferKey, arguments, TimeSpan.FromDays(2), TimeSpan.Zero);
        }

        public void Enqueue<T>(IQueueWorker worker, string bufferKey, T arguments, TimeSpan expire, TimeSpan nextVisible)
        {
            var dumpPayload = new
            {
                Worker = worker,
                BufferKey = bufferKey,
                NextVisible = nextVisible,
                Arguments = arguments
            };

            Tenant.GetService<ILoggingService>().Dump($"{typeof(QueueService).FullName.PadRight(64)}| Triggering event: {Serializer.Serialize(dumpPayload)}");

            Tenant.Post(Tenant.CreateUrl("Queue", "Enqueue"), new
            {
                worker.Configuration().Component,
                Worker = worker.Name,
                expire,
                nextVisible,
                arguments = arguments == null ? null : Serializer.Serialize(arguments),
                bufferKey
            });
        }
    }
}
