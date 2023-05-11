using TomPIT.Connectivity;

namespace TomPIT.Diagnostics
{
	internal class MetricService : TenantObject, IMetricService
	{
		public MetricService(ITenant tenant) : base(tenant)
		{
		}
		public void Write(IMetric d)
		{
			Instance.SysProxy.Metrics.Write(d);
		}

		public void Flush()
		{
			Instance.SysProxy.Metrics.Flush();
		}
	}
}
