using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Configuration;
using TomPIT.Distributed;
using TomPIT.Environment;

namespace TomPIT.Cdn.Mail
{
    internal class MailService : HostedService
    {
        private Lazy<List<MailDispatcher>> _dispatchers = new Lazy<List<MailDispatcher>>();

        private void OnSettingChanged(object sender, SettingEventArgs e)
        {
            if (string.Compare(e.Name, "MailServiceTimer", true) == 0 && string.IsNullOrWhiteSpace(e.Type) && string.IsNullOrWhiteSpace(e.PrimaryKey))
                SetInterval();
        }

        protected override bool OnInitialize(CancellationToken cancel)
        {
            if (Instance.State == InstanceState.Initializing)
                return false;

            SetInterval();

            Tenant.GetService<ISettingService>().SettingChanged += OnSettingChanged;

            foreach (var i in Tenant.GetService<IResourceGroupService>().QuerySupported())
                Dispatchers.Add(new MailDispatcher(i.Name));

            return true;
        }
        private void SetInterval()
        {
            var interval = Tenant.GetService<ISettingService>().GetValue<int>("MailServiceTimer", null, null, null);

            if (interval == 0)
                interval = 5000;

            IntervalTimeout = TimeSpan.FromMilliseconds(interval);
        }

        protected override Task OnExecute(CancellationToken cancel)
        {
            Parallel.ForEach(Dispatchers, (f) =>
            {
                var messages = Instance.SysProxy.Management.Mail.Dequeue(f.Available);

                if (cancel.IsCancellationRequested)
                    return;

                if (messages == null)
                    return;

                foreach (var i in messages)
                {
                    if (cancel.IsCancellationRequested)
                        return;

                    f.Enqueue(i);
                }
            });

            return Task.CompletedTask;
        }

        private List<MailDispatcher> Dispatchers { get { return _dispatchers.Value; } }

        public override void Dispose()
        {
            foreach (var dispatcher in Dispatchers)
                dispatcher.Dispose();

            Dispatchers.Clear();

            base.Dispose();
        }
    }
}
