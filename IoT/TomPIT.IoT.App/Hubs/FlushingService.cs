using System;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Distributed;
using TomPIT.Middleware;

namespace TomPIT.IoT.Hubs
{
	internal class FlushingService : HostedService
	{
		public FlushingService()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(1000);
		}

		protected override bool OnInitialize(CancellationToken cancel)
		{
			return Instance.State == InstanceState.Running;
		}
		protected override Task OnExecute(CancellationToken cancel)
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
