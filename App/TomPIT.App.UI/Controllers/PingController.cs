using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Controllers;

namespace TomPIT.App.Controllers
{
	[AllowAnonymous]
	[Route("sys/app/pingcontroller")]
	public class PingController : ServerController
	{
		[HttpGet]
		public IActionResult Invoke()
		{
			return new EmptyResult();
		}
	}
}
