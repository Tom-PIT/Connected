using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.IoT.Models;

namespace TomPIT.IoT.Controllers
{
	[AllowAnonymous]
	public class IoTController : Controller
	{
		[HttpPost]
		public IActionResult Partial()
		{
			var body = Request.Body.ToType<JArray>();
			var model = new IoTPartialModel();

			var ms = RouteData.Values["microService"].ToString();
			var view = RouteData.Values["view"].ToString();

			foreach (JValue i in body)
				model.Stencils.Add(i.Value<string>());

			model.Initialize(this, ms, view);

			//TODO:authorize

			return PartialView("~/Views/IoT/IoTPartialView.cshtml", model);
		}
	}
}
