using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using TomPIT.Diagnostics;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Serialization;

namespace TomPIT.Worker.Services
{
    internal class QueueWorkerService : HostedService
    {
        private Lazy<List<QueueWorkerDispatcher>> _dispatchers = new Lazy<List<QueueWorkerDispatcher>>();

        private readonly IQueueMonitoringService _queueMonitoringService;

        public static QueueWorkerService ServiceInstance { get; private set; }

        public QueueWorkerService()
        {
            IntervalTimeout = TimeSpan.FromMilliseconds(490);
            _queueMonitoringService = Tenant.GetService<IQueueMonitoringService>();
            ServiceInstance = this;
        }

        protected override bool OnInitialize(CancellationToken cancel)
        {
            if (Instance.State == InstanceState.Initializing)
                return false;

            Dispatchers.Add(new QueueWorkerDispatcher());

            return true;
        }
        protected override Task OnExecute(CancellationToken cancel)
        {
            Parallel.ForEach(Dispatchers, (f) =>
            {
                if (f.Available < 1)
                    return;

                var jobs = Instance.SysProxy.Management.Queue.Dequeue(f.Available);

                _queueMonitoringService?.SignalEnqueued(jobs?.Count ?? 0);

                var batch = Guid.NewGuid();

                if (cancel.IsCancellationRequested)
                    return;

                if (jobs is null)
                    return;

                foreach (var i in jobs)
                {
                    if (cancel.IsCancellationRequested)
                        return;

                    MiddlewareDescriptor.Current.Tenant.GetService<ILoggingService>().Dump($"{typeof(QueueWorkerService).FullName.PadRight(64)}| Batch {batch} => Enqueue {Serializer.Serialize(i)}");

                    f.Enqueue(i);
                }
            });

            return Task.CompletedTask;
        }

        public List<QueueWorkerDispatcher> Dispatchers { get { return _dispatchers.Value; } }

        public override void Dispose()
        {
            foreach (var dispatcher in Dispatchers)
                dispatcher.Dispose();

            Dispatchers.Clear();

            base.Dispose();
        }
    }
}
