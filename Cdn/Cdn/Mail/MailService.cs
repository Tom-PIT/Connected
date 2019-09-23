using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TomPIT.Configuration;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Runtime.Configuration;

namespace TomPIT.Cdn.Mail
{
	internal class MailService : HostedService
	{
		private CancellationTokenSource _cancel = new CancellationTokenSource();
		private Lazy<List<MailDispatcher>> _dispatchers = new Lazy<List<MailDispatcher>>();

		public MailService()
		{
			Instance.Tenant.GetService<ISettingService>().SettingChanged += OnSettingChanged;

			SetInterval();

			foreach (var i in Shell.GetConfiguration<IClientSys>().ResourceGroups)
				Dispatchers.Add(new MailDispatcher(i, _cancel));
		}

		private void OnSettingChanged(object sender, SettingEventArgs e)
		{
			if (e.ResourceGroup == Guid.Empty && string.Compare(e.Name, "MailServiceTimer", true) == 0)
				SetInterval();
		}

		private void SetInterval()
		{
			var interval = Instance.Tenant.GetService<ISettingService>().GetValue<int>(Guid.Empty, "MailServiceTimer");

			if (interval == 0)
				interval = 5000;

			IntervalTimeout = TimeSpan.FromMilliseconds(interval);
		}

		protected override Task Process()
		{
			Parallel.ForEach(Dispatchers, (f) =>
			{
				var url = Instance.Tenant.CreateUrl("MailManagement", "Dequeue");

				var e = new JObject
				{
					{ "count", f.Available },
				};

				var messages = Instance.Tenant.Post<List<MailMessage>>(url, e);

				if (messages == null)
					return;

				foreach (var i in messages)
					f.Enqueue(i);
			});

			return Task.CompletedTask;
		}

		private List<MailDispatcher> Dispatchers { get { return _dispatchers.Value; } }
	}
}
