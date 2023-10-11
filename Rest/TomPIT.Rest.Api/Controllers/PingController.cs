using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Controllers;

namespace TomPIT.Rest.Controllers
{
	[AllowAnonymous]
	[Route("sys/rest/pingcontroller")]
	public class PingController : ServerController
	{
		[HttpGet]
		public IActionResult Invoke()
		{
			return new EmptyResult();
		}
	}
}
