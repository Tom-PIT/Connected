using TomPIT.Collections;
using TomPIT.ComponentModel.Diagnostics;

namespace TomPIT.ComponentModel.Distributed
{
	public interface IQueueConfiguration : IConfiguration, INamespaceElement
	{
		ListItems<IQueueWorker> Workers { get; }
		IMetricOptions Metrics { get; }
	}
}
