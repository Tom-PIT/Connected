using Microsoft.AspNetCore.Mvc;
using TomPIT.Models;

namespace TomPIT.Controllers
{
	public class MicroServiceController : DevelopmentController
	{
		[HttpGet]
		public IActionResult Index()
		{
			return View("~/Pages/Ide/Ide.cshtml", new IdeModel());
		}
	}
}