using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Connectivity;

namespace TomPIT.Diagnostics
{
	internal class LoggingManagementService : ILoggingManagementService
	{
		public LoggingManagementService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

		public void Clear()
		{
			var u = Connection.CreateUrl("LoggingManagement", "Clear");

			Connection.Post(u);
		}

		public void Delete(long id)
		{
			var u = Connection.CreateUrl("LoggingManagement", "Delete");

			var args = new JObject
			{
				"id", id
			};

			Connection.Post(u, args);
		}

		public List<ILogEntry> Query(DateTime date)
		{
			var u = Connection.CreateUrl("LoggingManagement", "Query");
			var e = new JObject
			{
				"data", date
			};

			return Connection.Post<List<LogEntry>>(u, e).ToList<ILogEntry>();
		}

		public List<ILogEntry> Query(Guid metric)
		{
			var u = Connection.CreateUrl("LoggingManagement", "Query");
			var e = new JObject
			{
				{"metric", metric }
			};

			return Connection.Post<List<LogEntry>>(u, e).ToList<ILogEntry>();
		}
	}
}
