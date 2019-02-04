using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using TomPIT.Diagnostics;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers.Management
{
	public class MetricManagementController : SysController
	{
		[HttpPost]
		public void Clear()
		{
			var body = FromBody();

			if (body == null)
				DataModel.Metrics.Clear();

			var component = body.Optional("component", Guid.Empty);
			var element = body.Optional("element", Guid.Empty);

			DataModel.Metrics.Clear(component, element);
		}

		[HttpPost]
		public List<IMetric> Query()
		{
			var body = FromBody();
			var date = body.Required<DateTime>("date");
			var component = body.Required<Guid>("component");
			var element = body.Optional("element", Guid.Empty);

			return DataModel.Metrics.Query(date, component, element);
		}
	}
}
