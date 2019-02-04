using System;
using TomPIT.Diagnostics;

namespace TomPIT
{
	public static class ManagementExtensions
	{
		public static double Duration(this IMetric metric)
		{
			if (metric.End == DateTime.MinValue)
				return 0;

			return metric.End.Subtract(metric.Start).TotalMilliseconds;
		}
	}
}
