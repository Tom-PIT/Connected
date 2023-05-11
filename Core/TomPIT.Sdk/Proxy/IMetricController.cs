using TomPIT.Diagnostics;

namespace TomPIT.Proxy
{
	public interface IMetricController
	{
		void Write(IMetric d);
		void Flush();
	}
}
