using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Services;

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
			Instance.GetService<IIndexingService>().Flush();

			return Task.CompletedTask;
		}
	}
}
