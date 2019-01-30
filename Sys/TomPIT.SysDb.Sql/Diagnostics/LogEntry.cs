using System;
using System.Diagnostics;
using TomPIT.Data.Sql;
using TomPIT.Diagnostics;

namespace TomPIT.SysDb.Sql.Diagnostics
{
	internal class LogEntry : LongPrimaryKeyRecord, ILogEntry
	{
		public string Category { get; set; }
		public string Message { get; set; }
		public TraceLevel Level { get; set; }
		public string Source { get; set; }
		public DateTime Created { get; set; }
		public int EventId { get; set; }
		public Guid Component { get; set; }
		public Guid Element { get; set; }
		public long Metric { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Category = GetString("category");
			Message = GetString("message");
			Level = GetValue("trace_level", TraceLevel.Info);
			Source = GetString("source");
			Created = GetDate("created");
			EventId = GetInt("event_id");
			Component = GetGuid("component");
			Element = GetGuid("element");
			Metric = GetLong("metric");
		}
	}
}
