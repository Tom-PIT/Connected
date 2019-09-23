using System;
using System.Threading.Tasks;
using TomPIT.Distributed;

namespace TomPIT.Search.Services
{
	internal class FlushingService : HostedService
	{
		public FlushingService()
		{
			IntervalTimeout = TimeSpan.FromSeconds(15);
		}

		protected override Task Process()
		{
			Instance.Tenant.GetService<IIndexingService>().Flush();

			return Task.CompletedTask;
		}
	}
}
