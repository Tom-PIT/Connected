using System;
using System.Threading.Tasks;
using TomPIT.Services;

namespace TomPIT.IoT.Services
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
				Instance.Connection.GetService<IIoTHubService>().FlushChanges();
			}
			catch
			{
				//TODO: log exception
			}

			return Task.CompletedTask;
		}
	}
}
