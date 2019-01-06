using System;
using System.Collections.Generic;
using System.Diagnostics;
using TomPIT.Diagnostics;

namespace TomPIT.SysDb.Diagnostics
{
	public interface ILoggingHandler
	{
		void Insert(DateTime date, string category, string source, string message, TraceLevel level, int eventId, Guid microService, Guid contextMicroService, string authorityId, string authority,
			string contextAuthority, string contextAuthorityId, string contextProperty);

		void Clear();
		void Delete(long id);
		List<ILogEntry> Query(DateTime date);
	}
}
