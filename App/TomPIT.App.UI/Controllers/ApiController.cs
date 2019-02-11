using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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

			return Json(JsonConvert.SerializeObject(m.Invoke<object>(m.QualifierName, m.Body)));
		}

		[HttpPost]
		public IActionResult Partial()
		{
			var model = CreatePartialModel();

			return PartialView(string.Format("~/Views/Dynamic/Partial/{0}.cshtml", model.QualifierName), model);
		}

		private ApiModel CreateModel()
		{
			var r = new ApiModel
			{
				Body = FromBody()
			};

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
