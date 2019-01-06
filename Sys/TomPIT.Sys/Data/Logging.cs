using System;
using System.Collections.Generic;
using System.Diagnostics;
using TomPIT.Caching;
using TomPIT.Diagnostics;
using TomPIT.Sys.Api.Database;

namespace TomPIT.Sys.Data
{
	public class Logging : CacheRepository<ILogEntry, long>
	{
		public static int Avatar = 1;

		public const string CategoryWorker = "Worker";

		public Logging(IMemoryCache container) : base(container, "blob")
		{
		}

		public void Insert(string category, string source, string message, TraceLevel level, int eventId)
		{
			Insert(category, source, message, level, eventId, Guid.Empty, Guid.Empty, null, null, null, null, null);
		}

		public void Insert(string category, string source, string message, TraceLevel level, int eventId, Guid microService, Guid contextMicroService, string authorityId, string authority,
			string contextAuthority, string contextAuthorityId, string contextProperty)
		{
			Shell.GetService<IDatabaseService>().Proxy.Diagnostics.Logging.Insert(DateTime.UtcNow, category, source, message, level, eventId, microService, contextMicroService,
				authorityId, authority, contextAuthority, contextAuthorityId, contextProperty);
		}

		public void Clear()
		{
			Shell.GetService<IDatabaseService>().Proxy.Diagnostics.Logging.Clear();
		}

		public void Delete(long id)
		{
			Shell.GetService<IDatabaseService>().Proxy.Diagnostics.Logging.Delete(id);
		}

		public List<ILogEntry> Query(DateTime date)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Diagnostics.Logging.Query(date);
		}
	}
}
