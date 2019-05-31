using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel.Events;

namespace TomPIT.ComponentModel.Workers
{
	public interface IQueueWorker : IConfiguration
	{
		IMetricConfiguration Metrics { get; }
		IServerEvent Invoke { get; }
	}
}
