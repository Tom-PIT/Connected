using TomPIT.ComponentModel.Diagnostics;

namespace TomPIT.ComponentModel.Distributed
{
	public interface IQueueConfiguration : IConfiguration, ISourceCode
	{
		IMetricOptions Metrics { get; }
	}
}
