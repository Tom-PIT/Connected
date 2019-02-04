using System;
using System.Collections.Generic;

namespace TomPIT.Diagnostics
{
	public interface IMetricManagementService
	{
		List<IMetric> Query(DateTime date, Guid component);
		List<IMetric> Query(DateTime date, Guid component, Guid element);

		void Clear();
		void Clear(Guid component);
		void Clear(Guid component, Guid element);
	}
}
