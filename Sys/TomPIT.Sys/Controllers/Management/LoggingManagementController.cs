using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using TomPIT.Diagnostics;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers.Management
{
	public class LoggingManagementController : SysController
	{
		[HttpPost]
		public void Clear()
		{
			DataModel.Logging.Clear();
		}

		[HttpPost]
		public void Delete()
		{
			var body = FromBody();

			var id = body.Required<long>("id");

			DataModel.Logging.Delete(id);
		}

		[HttpPost]
		public List<ILogEntry> Query()
		{
			var body = FromBody();

			var metric = body.Optional("metric", Guid.Empty);

			if (metric != Guid.Empty)
				return DataModel.Logging.Query(metric);

			var date = body.Required<DateTime>("date");
			var component = body.Optional("component", Guid.Empty);
			var element = body.Optional("element", Guid.Empty);

			if (component == Guid.Empty)
				return DataModel.Logging.Query(date);
			else
				return DataModel.Logging.Query(date, component, element);
		}
	}
}
