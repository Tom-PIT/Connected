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

		[HttpGet]
		public List<ILogEntry> Query(DateTime date)
		{
			return DataModel.Logging.Query(date);
		}
	}
}
