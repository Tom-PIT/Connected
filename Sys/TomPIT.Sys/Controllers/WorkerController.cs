using Microsoft.AspNetCore.Mvc;
using System;
using TomPIT.Services;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers
{
	public class WorkerController : SysController
	{
		[HttpGet]
		public IScheduledJob Select(Guid microService, Guid api, Guid operation)
		{
			return DataModel.Workers.Select(microService, api, operation);
		}
	}
}
