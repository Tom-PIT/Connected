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

			w.CreateParameter("@component", component);

			w.Execute();
		}

		public void Clear(Guid component, Guid element)
		{
			var w = new Writer("tompit.metric_clr");

			w.CreateParameter("@component", component);
			w.CreateParameter("@element", element);

			w.Execute();
		}

		public void Insert(List<IMetric> items)
		{
			var w = new Writer("tompit.metric_ins");

			var a = new JArray();

			foreach (var i in items)
			{
				a.Add(new JObject
				{
					{"component",  i.Component},
					{"consumption_in",  i.ConsumptionIn},
					{"consumption_out",  (int)i.ConsumptionOut},
					{"element",  i.Element},
					{"end",  i.End},
					{"instance",  (int)i.Instance},
					{"request_ip",  i.IP.ToString()},
					{"parent",  i.Parent},
					{"request",  i.Request},
					{"response",  i.Response},
					{"result",  (int)i.Result},
					{"session",  i.Session},
					{"start",  i.Start}
				});
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
			r.CreateParameter("@element", element);

			return r.Execute().ToList<IMetric>();
		}
	}
}
