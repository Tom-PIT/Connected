using Microsoft.AspNetCore.Mvc;
using TomPIT.Development.Models;

namespace TomPIT.Development.Controllers
{
	public class DevHomeController : DevelopmentController
	{
		public IActionResult Index()
		{
			return View("~/Views/Index.cshtml", CreateModel());
		}

		private HomeModel CreateModel()
		{
			var r = new HomeModel();

			r.Initialize(this, null);
			r.Databind();

			return r;
		}
	}
}