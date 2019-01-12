using Microsoft.AspNetCore.Mvc;
using TomPIT.Models;

namespace TomPIT.Controllers
{
	public class HomeController : DevelopmentController
	{
		public IActionResult Index()
		{
			return View("~/Views/Index.cshtml", CreateModel());
		}

		private HomeModel CreateModel()
		{
			var r = new HomeModel();

			r.Initialize(this);
			r.Databind();

			return r;
		}
	}
}