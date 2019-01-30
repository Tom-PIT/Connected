using System;
using System.Diagnostics;
using TomPIT.Data;

namespace TomPIT.Diagnostics
{
	public interface ILogEntry : ILongPrimaryKeyRecord
	{
		string Category { get; }
		string Message { get; }
		TraceLevel Level { get; }
		string Source { get; }
		DateTime Created { get; }
		int EventId { get; }
		long Metric { get; }
		Guid Component { get; }
		Guid Element { get; }
	}
}
