using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Services;

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
			Instance.GetService<IIndexingService>().Scave();

			return Task.CompletedTask;
		}
	}
}
