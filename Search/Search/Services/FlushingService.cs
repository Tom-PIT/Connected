using System;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Distributed;
using TomPIT.Middleware;

namespace TomPIT.Search.Services
{
	internal class FlushingService : HostedService
	{
		public FlushingService()
		{
			IntervalTimeout = TimeSpan.FromSeconds(15);
		}

		protected override Task Process(CancellationToken cancel)
		{
			MiddlewareDescriptor.Current.Tenant.GetService<IIndexingService>().Flush();

			return Task.CompletedTask;
		}
	}
}
