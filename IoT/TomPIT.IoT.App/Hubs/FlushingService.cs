using System;
using System.Threading.Tasks;
using TomPIT.Distributed;
using TomPIT.Middleware;

namespace TomPIT.IoT.Hubs
{
	internal class FlushingService : HostedService
	{
		public FlushingService()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(250);
		}

		protected override Task Process()
		{
			try
			{
				MiddlewareDescriptor.Current.Tenant.GetService<IIoTHubService>().FlushChanges();
			}
			catch
			{
				//TODO: log exception
			}

			return Task.CompletedTask;
		}
	}
}
