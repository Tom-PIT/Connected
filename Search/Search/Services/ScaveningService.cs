using System;
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

		protected override Task Process()
		{
			MiddlewareDescriptor.Current.Tenant.GetService<IIndexingService>().Scave();

			return Task.CompletedTask;
		}
	}
}
