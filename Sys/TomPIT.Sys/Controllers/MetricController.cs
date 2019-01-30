using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using TomPIT.Diagnostics;
using TomPIT.Environment;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers
{
	public class MetricController : SysController
	{
		[HttpPost]
		public void Insert()
		{
			var body = FromBody();

			var data = body.Optional<JArray>("data", null);

			if (data == null)
				return;

			var items = new List<IMetric>();

			foreach (var i in data)
			{
				if (!(i is JObject jo))
					continue;

				items.Add(new Metric
				{
					ConsumptionIn = jo.Optional("consumptionId", 0L),
					ConsumptionOut = jo.Optional("consumptionOut", 0L),
					End = jo.Optional("end", DateTime.UtcNow),
					Instance = jo.Optional("instance", InstanceType.Unknown),
					IP = jo.Optional("ip", IPAddress.None),
					Parent = jo.Optional("parent", Guid.Empty),
					Component = jo.Required<Guid>("component"),
					Element = jo.Optional("element", Guid.Empty),
					Request = jo.Optional("request", string.Empty),
					Response = jo.Optional("response", string.Empty),
					Result = jo.Optional("result", SessionResult.Success),
					Session = jo.Required<Guid>("session"),
					Start = jo.Optional("start", DateTime.MinValue)
				});
			}

			if (items.Count == 0)
				return;

			DataModel.Metrics.Insert(items);
		}
	}
}
