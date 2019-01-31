using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

		public void Insert(List<ILogEntry> items)
		{
			var w = new Writer("tompit.log_ins");

			var a = new JArray();

			foreach (var i in items)
			{
				var d = new JObject
				{
					{"created", i.Created },
					{"trace_level", (int)i.Level }
				};

				if (!string.IsNullOrWhiteSpace(i.Message))
					d.Add("message", i.Message);

				if (!string.IsNullOrWhiteSpace(i.Source))
					d.Add("source", i.Source);

				if (!string.IsNullOrWhiteSpace(i.Category))
					d.Add("category", i.Category);

				if (i.EventId > 0)
					d.Add("event_id", i.EventId);

				if (i.Metric != Guid.Empty)
					d.Add("metric", i.Metric);

				if (i.Component != Guid.Empty)
					d.Add("component", i.Component);

				if (i.Element != Guid.Empty)
					d.Add("element", i.Element);

				a.Add(d);
			}

			w.CreateParameter("@items", JsonConvert.SerializeObject(a));

			w.Execute();
		}

		public List<ILogEntry> Query(DateTime date)
		{
			var r = new Reader<LogEntry>("tompit.log_que");

			r.CreateParameter("@date", date.Date);

			return r.Execute().ToList<ILogEntry>();
		}

		public List<ILogEntry> Query(DateTime date, long metric)
		{
			var r = new Reader<LogEntry>("tompit.log_que");

			r.CreateParameter("@date", date.Date);
			r.CreateParameter("@metric", metric);

			return r.Execute().ToList<ILogEntry>();
		}

		public List<ILogEntry> Query(DateTime date, Guid component, Guid element)
		{
			var r = new Reader<LogEntry>("tompit.log_que");

			r.CreateParameter("@date", date.Date);
			r.CreateParameter("@component", component);
			r.CreateParameter("@element", element, true);

			return r.Execute().ToList<ILogEntry>();
		}
	}
}
