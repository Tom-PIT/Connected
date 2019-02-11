using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Models;

namespace TomPIT.Controllers
{
	public class ApiTestController : DevelopmentController
	{
		public IActionResult Index()
		{
			return View("~/Views/ApiTest.cshtml", ApiTestModel.Create(this, true));
		}

		public IActionResult Invoke()
		{
			var m = ApiTestModel.Create(this, true);

			return Json(m.Invoke());
		}

		public IActionResult Save()
		{
			var m = ApiTestModel.Create(this, false);

			var id = m.Save();

			return new JsonResult(new JObject
			{
				{ "identifier",id }
			});
		}

		public IActionResult QueryTags()
		{
			var m = ApiTestModel.Create(this, false);

			return Json(m.TestCategories);
		}

		public IActionResult QueryTests()
		{
			var m = ApiTestModel.Create(this, false);

			return Json(m.QueryTests());
		}

		public IActionResult SelectBody()
		{
			var m = ApiTestModel.Create(this, false);

			return Json(new JObject
			{
				{ "body",m.SelectBody() }
			});
		}

		public IActionResult Delete()
		{
			var m = ApiTestModel.Create(this, false);

			m.Delete();

			return new EmptyResult();
		}
	}
}
