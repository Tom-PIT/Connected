using System;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Distributed;
using TomPIT.Environment;

namespace TomPIT.Worker.Services
{
    internal class WorkerService : HostedService
    {
        public WorkerService()
        {
            IntervalTimeout = TimeSpan.FromMilliseconds(490);
        }

        private WorkerDispatcher Dispatcher { get; set; }
        protected override async Task OnExecute(CancellationToken cancel)
        {
            var jobs = Instance.SysProxy.Management.Workers.Dequeue(Dispatcher.Available);

            if (cancel.IsCancellationRequested)
                return;

            foreach (var i in jobs)
            {
                if (cancel.IsCancellationRequested)
                    return;

                Dispatcher.Enqueue(i);
            }

            await Task.CompletedTask;
        }

        protected override bool OnInitialize(CancellationToken cancel)
        {
            if (Instance.State == InstanceState.Initializing)
                return false;

            Dispatcher = new(Tenant.GetService<IResourceGroupService>().Default.Name);

            return true;
        }

        public override void Dispose()
        {
            if (Dispatcher is not null)
                Dispatcher.Dispose();

            base.Dispose();
        }
    }
}
