using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.Models;

namespace TomPIT.Controllers
{
	public class ApiTestController : DevelopmentController
	{
		public IActionResult Index()
		{
			return View("~/Views/ApiTest.cshtml", CreateModel(true));
		}

		private ApiTestModel CreateModel(bool initialize)
		{
			var r = new ApiTestModel
			{
				Body = FromBody()
			};

			if (initialize)
				r.Initialize(this);

			return r;
		}

		public IActionResult Invoke()
		{
			var m = CreateModel(true);

			return Json(m.Invoke());
		}

		public IActionResult Save()
		{
			var m = CreateModel(false);

			var id = m.Save();

			return new JsonResult(new JObject
			{
				{ "identifier",id }
			});
		}

		public IActionResult QueryTags()
		{
			var m = CreateModel(false);

			return Json(m.TestCategories);
		}

		public IActionResult QueryTests()
		{
			var m = CreateModel(false);

			return Json(m.QueryTests());
		}

		public IActionResult SelectBody()
		{
			var m = CreateModel(false);

			return Json(new JObject
			{
				{ "body",m.SelectBody() }
			});
		}

		public IActionResult Delete()
		{
			var m = CreateModel(false);

			m.Delete();

			return new EmptyResult();
		}
	}
}
