using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TomPIT.Compilation;
using TomPIT.Models;

namespace TomPIT.Controllers
{
	[AllowAnonymous]
	public class ApiController : ServerController
	{
		[HttpPost]
		public IActionResult Invoke()
		{
			var m = CreateModel();

			return Json(Types.Serialize(m.Invoke<object>(m.QualifierName, m.Body)));
		}

		[HttpPost]
		public IActionResult Partial()
		{
			var model = CreatePartialModel();

			return PartialView(string.Format("~/Views/Dynamic/Partial/{0}.cshtml", model.QualifierName), model);
		}

		[HttpPost]
		public IActionResult SetUserData()
		{
			var model = CreateUserDataModel();

			model.SetData();

			return new EmptyResult();
		}

		[HttpPost]
		public IActionResult GetUserData()
		{
			var model = CreateUserDataModel();

			return Json(model.GetData());
		}

		[HttpPost]
		public IActionResult QueryUserData()
		{
			var model = CreateUserDataModel();

			return Json(model.QueryData());
		}

		private UserDataModel CreateUserDataModel()
		{
			var r = new UserDataModel
			{
				Body = FromBody()
			};

			r.Databind();
			r.Initialize(this, null);

			return r;
		}

		private ApiModel CreateModel()
		{
			ApiModel r;
			var apiHeader = Request.Headers["X-TP-API"];
			var componentHeader = Request.Headers["X-TP-COMPONENT"];

			if (!string.IsNullOrWhiteSpace(apiHeader))
				r = new ApiModel(apiHeader, componentHeader);
			else
				r = new ApiModel { Body = FromBody() };

			r.Databind();
			r.Initialize(this, r.MicroService);

			return r;
		}

		private PartialModel CreatePartialModel()
		{
			var r = new PartialModel
			{
				Body = FromBody()
			};

			r.Databind();
			r.Initialize(this, r.MicroService);

			return r;
		}
	}
}
