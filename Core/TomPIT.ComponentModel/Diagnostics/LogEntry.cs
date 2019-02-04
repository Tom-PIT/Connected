using System;
using System.Diagnostics;

namespace TomPIT.Diagnostics
{
	public class LogEntry : ILogEntry
	{
		public string Category { get; set; }
		public string Message { get; set; }
		public TraceLevel Level { get; set; }
		public string Source { get; set; }
		public DateTime Created { get; set; } = DateTime.UtcNow;
		public int EventId { get; set; }
		public Guid Component { get; set; }
		public Guid Element { get; set; }
		public long Id { get; set; }
		public Guid Metric { get; set; }
	}
}
