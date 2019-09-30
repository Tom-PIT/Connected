using TomPIT.ComponentModel.Diagnostics;

namespace TomPIT.ComponentModel.Distributed
{
	public interface IQueueConfiguration : IConfiguration, IText
	{
		IMetricOptions Metrics { get; }
	}
}
