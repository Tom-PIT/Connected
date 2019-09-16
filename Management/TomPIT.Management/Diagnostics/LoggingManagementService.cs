using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;
using TomPIT.Middleware;

namespace TomPIT.Management.Diagnostics
{
	internal class LoggingManagementService : TenantObject, ILoggingManagementService
	{
		public LoggingManagementService(ITenant tenant) : base(tenant)
		{

		}

		public void Clear()
		{
			var u = Tenant.CreateUrl("LoggingManagement", "Clear");

			Tenant.Post(u);
		}

		public void Delete(long id)
		{
			var u = Tenant.CreateUrl("LoggingManagement", "Delete");

			var args = new JObject
			{
				"id", id
			};

			Tenant.Post(u, args);
		}

		public List<ILogEntry> Query(DateTime date)
		{
			var u = Tenant.CreateUrl("LoggingManagement", "Query");
			var e = new JObject
			{
				"data", date
			};

			return Tenant.Post<List<LogEntry>>(u, e).ToList<ILogEntry>();
		}

		public List<ILogEntry> Query(Guid metric)
		{
			var u = Tenant.CreateUrl("LoggingManagement", "Query");
			var e = new JObject
			{
				{"metric", metric }
			};

			return Tenant.Post<List<LogEntry>>(u, e).ToList<ILogEntry>();
		}
	}
}
