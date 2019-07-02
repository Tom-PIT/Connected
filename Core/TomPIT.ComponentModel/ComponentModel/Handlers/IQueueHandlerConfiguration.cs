using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel.Events;

namespace TomPIT.ComponentModel.Handlers
{
	public interface IQueueHandlerConfiguration : IConfiguration, ISourceCode
	{
		IMetricConfiguration Metrics { get; }
	}
}
