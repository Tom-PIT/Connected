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
	internal class MetricsHandler : IMetricHandler
	{
		public void Clear()
		{
			new Writer("tompit.metric_clr").Execute();
		}

		public void Clear(Guid component)
		{
			var w = new Writer("tompit.metric_clr");

			w.CreateParameter("@component", component, true);

			w.Execute();
		}

		public void Clear(Guid component, Guid element)
		{
			var w = new Writer("tompit.metric_clr");

			w.CreateParameter("@component", component, true);
			w.CreateParameter("@element", element, true);

			w.Execute();
		}

		public void Insert(List<IMetric> items)
		{
			var w = new Writer("tompit.metric_ins");

			var a = new JArray();

			foreach (var i in items)
			{
				var d = new JObject
				{
					{"component",  i.Component},
					{"consumption_in",  i.ConsumptionIn},
					{"consumption_out",  (int)i.ConsumptionOut},
					{"result",  (int)i.Result},
					{"session",  i.Session},
					{"start",  i.Start},
					{"instance",  (int)i.Instance}
				};

				if (i.Element != Guid.Empty)
					d.Add("element", i.Element);

				if (i.Parent != Guid.Empty)
					d.Add("parent", i.Parent);

				if (i.End != DateTime.MinValue)
					d.Add("end", i.End);

				if (!string.IsNullOrWhiteSpace(i.IP))
					d.Add("request_ip", i.IP);

				if (!string.IsNullOrWhiteSpace(i.Request))
					d.Add("request", i.Request);

				if (!string.IsNullOrWhiteSpace(i.Response))
					d.Add("response", i.Response);

				a.Add(d);
			}

			w.CreateParameter("@items", JsonConvert.SerializeObject(a));

			w.Execute();

		}

		public List<IMetric> Query(DateTime date, Guid component)
		{
			var r = new Reader<Metric>("tompit.metric_que");

			r.CreateParameter("@date", date);
			r.CreateParameter("@component", component);

			return r.Execute().ToList<IMetric>();
		}

		public List<IMetric> Query(DateTime date, Guid component, Guid element)
		{
			var r = new Reader<Metric>("tompit.metric_que");

			r.CreateParameter("@date", date);
			r.CreateParameter("@component", component);
			r.CreateParameter("@element", element, true);

			return r.Execute().ToList<IMetric>();
		}
	}
}
