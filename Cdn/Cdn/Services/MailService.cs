using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Services;

namespace TomPIT.Cdn.Services
{
	internal class MailService : HostedService
	{
		private CancellationTokenSource _cancel = new CancellationTokenSource();
		private Lazy<List<MailDispatcher>> _dispatchers = new Lazy<List<MailDispatcher>>();

		public MailService()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(490);

			foreach (var i in Shell.GetConfiguration<IClientSys>().ResourceGroups)
				Dispatchers.Add(new MailDispatcher(i, _cancel));
		}

		protected override Task Process()
		{
			Parallel.ForEach(Dispatchers, (f) =>
			{
				var url = Instance.Connection.CreateUrl("MailManagement", "Dequeue");

				var e = new JObject
				{
					{ "count", f.Available },
				};

				var messages = Instance.Connection.Post<List<MailMessage>>(url, e);

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
