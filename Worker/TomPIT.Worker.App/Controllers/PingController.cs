using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Controllers;
using TomPIT.Messaging;
using TomPIT.Worker.Services;

namespace TomPIT.Worker.Controllers
{
	[AllowAnonymous]
	public class PingController : ServerController
	{
		[HttpGet]
		public IActionResult Invoke()
		{
			return new EmptyResult();
		}

		[HttpGet]
		public IActionResult Dispatchers()
		{
			return Json(QueueWorkerService.ServiceInstance);
		}
	}
}
