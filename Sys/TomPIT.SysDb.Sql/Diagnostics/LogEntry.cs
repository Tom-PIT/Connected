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
		public Guid MicroService { get; set; }
		public string AuthorityId { get; set; }
		public string Authority { get; set; }
		public string ContextAuthority { get; set; }
		public string ContextAuthorityId { get; set; }
		public Guid ContextMicroService { get; set; }
		public string ContextProperty { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Category = GetString("category");
			Message = GetString("message");
			Level = GetValue("tryce_level", TraceLevel.Info);
			Source = GetString("source");
			Created = GetDate("created");
			EventId = GetInt("event_id");
			MicroService = GetGuid("service");
			AuthorityId = GetString("authority_id");
			Authority = GetString("authority");
			ContextAuthorityId = GetString("context_authority_id");
			ContextAuthority = GetString("context_authority");
			ContextMicroService = GetGuid("context_service");
			ContextProperty = GetString("context_property");
		}
	}
}
