using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Runtime;

namespace TomPIT.Diagnostics
{
	internal class LoggingManagementService : ILoggingManagementService
	{
		public void Clear(IApplicationContext sender)
		{
			var server = sender.GetServerContext();
			var u = server.CreateUrl("LoggingManagement", "Clear");

			server.Connection.Post(u, sender.JwToken());
		}

		public void Delete(IApplicationContext sender, long id)
		{
			var server = sender.GetServerContext();
			var u = server.CreateUrl("LoggingManagement", "Delete");

			var args = new JObject
			{
				"id", id
			};

			server.Connection.Post(u, args);
		}

		public List<ILogEntry> Query(IApplicationContext sender, DateTime date)
		{
			var server = sender.GetServerContext();
			var u = server.CreateUrl("LoggingManagement", "Query")
				.AddParameter("date", date);

			return server.Connection.Get<List<LogEntry>>(u).ToList<ILogEntry>();
		}
	}
}
