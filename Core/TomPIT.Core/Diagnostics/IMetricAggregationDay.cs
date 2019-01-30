using System;

namespace TomPIT.Diagnostics
{
	public interface IMetricAggregationDay
	{
		Guid Component { get; }
		Guid Element { get; }
		DateTime Date { get; }
		int Count { get; }
		int Success { get; }
		long Duration { get; }
		int Max { get; }
		int Min { get; }
		long ConsumptionIn { get; }
		long ConsumptionOut { get; }
		long MaxConsumptionIn { get; }
		long MaxConsumptionOut { get; }
	}
}
