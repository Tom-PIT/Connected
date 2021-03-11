using System;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Distributed;
using TomPIT.Middleware;

namespace TomPIT.Search.Services
{
	public class ScaveningService : HostedService
	{
		public ScaveningService()
		{
			IntervalTimeout = TimeSpan.FromMinutes(5);
		}

		protected override Task OnExecute(CancellationToken token)
		{
			MiddlewareDescriptor.Current.Tenant.GetService<IIndexingService>().Scave();

			return Task.CompletedTask;
		}
	}
}
