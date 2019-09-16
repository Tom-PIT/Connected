using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;
using TomPIT.Middleware;

namespace TomPIT.Management.Diagnostics
{
	internal class MetricManagementService : TenantObject, IMetricManagementService
	{
		public MetricManagementService(ITenant tenant) : base(tenant)
		{

		}

		public void Clear()
		{
			Clear(Guid.Empty);
		}

		public void Clear(Guid component)
		{
			Clear(component, Guid.Empty);
		}

		public void Clear(Guid component, Guid element)
		{
			var u = Tenant.CreateUrl("MetricManagement", "Clear");
			var e = new JObject
			{
				{"component", component },
				{"element", element }
			};

			Tenant.Post(u, e);
		}

		public List<IMetric> Query(DateTime date, Guid component)
		{
			return Query(date, component, Guid.Empty);
		}

		public List<IMetric> Query(DateTime date, Guid component, Guid element)
		{
			var u = Tenant.CreateUrl("MetricManagement", "Query");
			var e = new JObject
			{
				{"date", date },
				{"component", component },
				{"element", element }
			};

			return Tenant.Post<List<Metric>>(u, e).ToList<IMetric>();
		}
	}
}
