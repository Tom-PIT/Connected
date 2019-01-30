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

			var date = body.Required<DateTime>("date");
			var metric = body.Optional("metric", 0L);
			var component = body.Optional("metric", Guid.Empty);
			var element = body.Optional("metric", Guid.Empty);

			if (metric == 0 && component == Guid.Empty)
				return DataModel.Logging.Query(date);
			else if (component == Guid.Empty && metric != 0L)
				return DataModel.Logging.Query(date, component, element);
			else
				return DataModel.Logging.Query(date, metric);
		}
	}
}
