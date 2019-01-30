using System;
using System.Collections.Generic;
using TomPIT.Diagnostics;

namespace TomPIT.SysDb.Diagnostics
{
	public interface IMetricHandler
	{
		void Insert(List<IMetric> items);
		void Clear();
		void Clear(Guid component);
		void Clear(Guid component, Guid element);

		List<IMetric> Query(DateTime date, Guid component);
		List<IMetric> Query(DateTime date, Guid component, Guid element);
	}
}
