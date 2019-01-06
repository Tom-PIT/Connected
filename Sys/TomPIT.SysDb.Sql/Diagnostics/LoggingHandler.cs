using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TomPIT.Data.Sql;
using TomPIT.Diagnostics;
using TomPIT.SysDb.Diagnostics;

namespace TomPIT.SysDb.Sql.Diagnostics
{
	internal class LoggingHandler : ILoggingHandler
	{
		public void Clear()
		{
			new Writer("tompit.log_clr").Execute();
		}

		public void Delete(long id)
		{
			var w = new Writer("tompit.log_del");

			w.CreateParameter("@id", id);

			w.Execute();
		}

		public void Insert(DateTime date, string category, string source, string message, TraceLevel level, int eventId, Guid microService, Guid contextMicroService, string authorityId, string authority, string contextAuthority, string contextAuthorityId, string contextProperty)
		{
			var w = new Writer("tompit.log_ins");

			w.CreateParameter("@created", date);
			w.CreateParameter("@trace_level", level);
			w.CreateParameter("@message", message, true);
			w.CreateParameter("@source", source, true);
			w.CreateParameter("@category", category, true);
			w.CreateParameter("@event_id", eventId, true);
			w.CreateParameter("@authority_id", authorityId, true);
			w.CreateParameter("@authority", authority, true);
			w.CreateParameter("@context_authority_id", contextAuthorityId, true);
			w.CreateParameter("@context_authority", contextAuthority, true);
			w.CreateParameter("@context_property", contextProperty, true);
			w.CreateParameter("@service", microService, true);
			w.CreateParameter("@context_service", contextMicroService, true);

			w.Execute();
		}

		public List<ILogEntry> Query(DateTime date)
		{
			var r = new Reader<LogEntry>("tompit.log_que");

			r.CreateParameter("@date", date.Date);

			return r.Execute().ToList<ILogEntry>();
		}
	}
}
