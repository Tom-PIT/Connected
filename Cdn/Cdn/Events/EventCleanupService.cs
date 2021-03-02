using System;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Distributed;

namespace TomPIT.Cdn.Events
{
	internal class EventCleanupService : HostedService
	{
		public EventCleanupService()
		{
			IntervalTimeout = TimeSpan.FromSeconds(3);
		}

		protected override async Task OnExecute(CancellationToken cancel)
		{
			EventMessagingCache.Clean();
			EventClients.Clean();

			await Task.CompletedTask;
		}
	}
}
