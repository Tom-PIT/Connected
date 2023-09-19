using System.Collections.Generic;
using TomPIT.Diagnostics;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local
{
	internal class MetricController : IMetricController
	{
		public void Flush()
		{

		}

		public void Write(IMetric d)
		{
			DataModel.Metrics.Insert(new List<IMetric> { d });
		}
	}
}
