using System;
using System.Threading.Tasks;
using TomPIT.Distributed;

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
				Instance.Tenant.GetService<IIoTHubService>().FlushChanges();
			}
			catch
			{
				//TODO: log exception
			}

			return Task.CompletedTask;
		}
	}
}
