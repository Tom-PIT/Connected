using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.App.Models;
using TomPIT.Controllers;
using TomPIT.Serialization;

namespace TomPIT.App.Controllers
{
	[AllowAnonymous]
	public class ApiController : ServerController
	{
		[HttpPost]
		public IActionResult Invoke()
		{
			using var m = CreateModel();

			return Json(Serializer.Serialize(m.Interop.Invoke<object, JObject>(m.QualifierName, m.Body)));
		}

		[HttpPost]
		public IActionResult UIInjection()
		{
			using var model = CreateUIInjectionModel();

			return PartialView("~/Views/UIInjectionView.cshtml", model);
		}

		[HttpPost]
		public IActionResult Partial()
		{
			using var model = CreatePartialModel();

			return PartialView(string.Format("~/Views/Dynamic/Partial/{0}.cshtml", model.QualifierName), model);
		}

		[HttpPost]
		public IActionResult Search()
		{
			using var model = CreateSearchModel();

			return Json(model.Search());
		}

		[HttpPost]
		public IActionResult SetUserData()
		{
			using var model = CreateUserDataModel();

			model.SetData();

			return new EmptyResult();
		}

		[HttpPost]
		public IActionResult GetUserData()
		{
			using var model = CreateUserDataModel();

			return Json(model.GetData());
		}

		[HttpPost]
		public IActionResult QueryUserData()
		{
			using var model = CreateUserDataModel();

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

		private UIInjectionModel CreateUIInjectionModel()
		{
			var r = new UIInjectionModel
			{
				Body = FromBody()
			};

			r.Databind();
			r.Initialize(this, r.MicroService);

			return r;
		}

		private SearchModel CreateSearchModel()
		{
			var r = new SearchModel { Body = FromBody() };

			r.Databind();
			r.Initialize(this, r.MicroService);

			return r;
		}
	}
}
