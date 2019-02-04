using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Connectivity;

namespace TomPIT.Diagnostics
{
	internal class MetricManagementService : IMetricManagementService
	{
		public MetricManagementService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

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
			var u = Connection.CreateUrl("MetricManagement", "Clear");
			var e = new JObject
			{
				{"component", component },
				{"element", element }
			};

			Connection.Post(u, e);
		}

		public List<IMetric> Query(DateTime date, Guid component)
		{
			return Query(date, component, Guid.Empty);
		}

		public List<IMetric> Query(DateTime date, Guid component, Guid element)
		{
			var u = Connection.CreateUrl("MetricManagement", "Query");
			var e = new JObject
			{
				{"date", date },
				{"component", component },
				{"element", element }
			};

			return Connection.Post<List<Metric>>(u, e).ToList<IMetric>();
		}
	}
}
