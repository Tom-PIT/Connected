using Microsoft.AspNetCore.Mvc;
using TomPIT.Models;

namespace TomPIT.Controllers
{
	public class TestSuitesController : DevelopmentController
	{
		public IActionResult Index()
		{
			return View("~/Views/QA/TestSuites.cshtml", TestSuitesModel.Create(this, true));
		}

		public IActionResult Select()
		{
			return View("~/Views/QA/TestSuitesSelection.cshtml", TestSuitesModel.Create(this, true));
		}
	}
}
