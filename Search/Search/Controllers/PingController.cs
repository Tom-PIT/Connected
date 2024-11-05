using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Controllers;

namespace TomPIT.Search.Controllers
{
	[AllowAnonymous]
	[Route("sys/search/pingcontroller")]
	public class PingController : ServerController
	{
		[HttpGet]
		public IActionResult Invoke()
		{
			return new EmptyResult();
		}
	}
}
