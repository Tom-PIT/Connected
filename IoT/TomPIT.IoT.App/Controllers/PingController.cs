using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Controllers;

namespace TomPIT.IoT.Controllers
{
	[AllowAnonymous]
	[Route("sys/iot/pingcontroller")]
	public class PingController : ServerController
	{
		[HttpGet]
		public IActionResult Invoke()
		{
			return new EmptyResult();
		}
	}
}
