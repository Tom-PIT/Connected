using System;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Distributed;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers
{
	public class WorkerController : SysController
	{
		[HttpGet]
		public IScheduledJob Select(Guid worker)
		{
			return DataModel.Workers.Select(worker);
		}
	}
}
