using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Services;

namespace TomPIT.Diagnostics
{
	internal class LoggingManagementService : ILoggingManagementService
	{
		public void Clear(IExecutionContext context)
		{
			var server = context.Connection();
			var u = server.CreateUrl("LoggingManagement", "Clear");

			server.Post(u, context.JwToken());
		}

		public void Delete(IExecutionContext sender, long id)
		{
			var server = sender.Connection();
			var u = server.CreateUrl("LoggingManagement", "Delete");

			var args = new JObject
			{
				"id", id
			};

			server.Post(u, args);
		}

		public List<ILogEntry> Query(IExecutionContext sender, DateTime date)
		{
			var server = sender.Connection();
			var u = server.CreateUrl("LoggingManagement", "Query")
				.AddParameter("date", date);

			return server.Get<List<LogEntry>>(u).ToList<ILogEntry>();
		}
	}
}
