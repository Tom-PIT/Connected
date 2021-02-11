using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.Diagnostics;
using TomPIT.Sys.Model;
using TomPIT.Sys.Model.Diagnostics;

namespace TomPIT.Sys.Controllers
{
	public class LoggingController : SysController
	{
		[HttpPost]
		public void Insert()
		{
			var body = FromBody();

			var data = body.Optional<JArray>("data", null);

			if (data == null)
				return;

			var items = new List<ILogEntry>();

			foreach (var i in data)
			{
				if (!(i is JObject jo))
					continue;

				items.Add(new LogEntry
				{
					Category = jo.Optional("category", string.Empty),
					Message = jo.Optional("message", string.Empty),
					Level = jo.Optional("level", TraceLevel.Off),
					Source = jo.Optional("source", string.Empty),
					EventId = jo.Optional("eventId", 0),
					Component = jo.Optional("component", Guid.Empty),
					Element = jo.Optional("element", Guid.Empty),
					Metric = jo.Optional("metric", Guid.Empty),
					Created = jo.Optional("created", DateTime.UtcNow)
				});
			}

			if (items.Count == 0)
				return;

			DataModel.Logging.Insert(items);
		}
	}
}
