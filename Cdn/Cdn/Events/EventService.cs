using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Distributed;
using TomPIT.Environment;

namespace TomPIT.Cdn.Events
{
    internal class EventService : HostedService
    {
        public EventService()
        {
            IntervalTimeout = TimeSpan.FromMilliseconds(490);
            ServiceInstance = this;
        }

        public static EventService ServiceInstance { get; private set; }
        private EventDispatcher Dispatcher { get; set; }

        protected override bool OnInitialize(CancellationToken cancel)
        {
            if (Instance.State == InstanceState.Initializing)
                return false;


            Dispatcher = new(Tenant.GetService<IResourceGroupService>().Default.Name);

            return true;
        }
        protected override async Task OnExecute(CancellationToken cancel)
        {
            if (Dispatcher.Available <= 0)
                return;

            if (cancel.IsCancellationRequested)
                return;

            var jobs = Instance.SysProxy.Management.Events.Dequeue(Dispatcher.Available);

            if (!jobs.Any())
                return;

            foreach (var i in jobs)
            {
                if (cancel.IsCancellationRequested)
                    return;

                Dispatcher.Enqueue(i);
            }

            await Task.CompletedTask;
        }

        public override void Dispose()
        {
            Dispatcher.Dispose();

            base.Dispose();
        }
    }
}
