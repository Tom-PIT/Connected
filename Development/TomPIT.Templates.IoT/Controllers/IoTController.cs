using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace TomPIT.IoT.Controllers
{
	[AllowAnonymous]
	public class IoTController : Controller
	{
		[HttpPost]
		public IActionResult Partial()
		{
			var body = Request.Body.ToType<JObject>();

			return Ok();
		}
	}
}
