using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Cdn.Events;
using TomPIT.Controllers;

namespace TomPIT.Cdn.Controllers
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
			return Json(EventService.ServiceInstance);
        }
    }
}
