using System;
using System.Collections.Generic;
using TomPIT.Diagnostics;
using TomPIT.Sys.Api.Database;

namespace TomPIT.Sys.Data
{
	public class Metrics
	{
		public void Insert(List<IMetric> items)
		{
			Shell.GetService<IDatabaseService>().Proxy.Diagnostics.Metrics.Insert(items);
		}

		public void Clear()
		{
			Shell.GetService<IDatabaseService>().Proxy.Diagnostics.Metrics.Clear();
		}

		public void Clear(Guid component)
		{
			Shell.GetService<IDatabaseService>().Proxy.Diagnostics.Metrics.Clear(component);
		}

		public void Clear(Guid component, Guid element)
		{
			Shell.GetService<IDatabaseService>().Proxy.Diagnostics.Metrics.Clear(component, element);
		}

		public List<IMetric> Query(DateTime date, Guid component)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Diagnostics.Metrics.Query(date, component);
		}

		public List<IMetric> Query(DateTime date, Guid component, Guid element)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Diagnostics.Metrics.Query(date, component, element);
		}

	}
}
