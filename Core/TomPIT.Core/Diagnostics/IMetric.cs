using System;
using TomPIT.Environment;

namespace TomPIT.Diagnostics
{
	public enum SessionResult
	{
		Success = 1,
		Fail = 2
	}

	public interface IMetric
	{
		Guid Session { get; }
		DateTime Start { get; }
		DateTime End { get; }
		SessionResult Result { get; }
		InstanceFeatures Features { get; }
		string IP { get; }
		Guid Component { get; }
		Guid Element { get; }
		Guid Parent { get; }
		string Request { get; }
		string Response { get; }
		long ConsumptionIn { get; }
		long ConsumptionOut { get; }
	}
}
