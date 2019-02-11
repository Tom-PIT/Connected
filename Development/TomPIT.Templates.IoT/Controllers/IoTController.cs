using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using TomPIT.IoT.Models;

namespace TomPIT.IoT.Controllers
{
	[AllowAnonymous]
	public class IoTController : Controller
	{
		[HttpPost]
		public IActionResult Partial()
		{
			var body = Request.Body.ToType<JObject>();
			var model = new IoTPartialModel();

			var ms = RouteData.Values["microService"].ToString();
			var view = RouteData.Values["view"].ToString();
			var stencils = body.Required<JArray>("stencils");

			foreach (JValue i in stencils)
				model.Stencils.Add(i.Value<string>());

			var data = body.Optional<JObject>("data", null);

			if (data != null)
			{
				model.VerifyData(data);
				var timestamp = data.Optional("$timestamp", DateTime.MinValue);

				foreach (var i in data)
				{
					if (string.Compare(i.Key, "$timestamp", true) == 0
						|| string.Compare(i.Key, "$checkSum", true) == 0)
						continue;

					model.ForwardState.Add(new IoTFieldState
					{
						Modified = timestamp,
						Field = i.Key,
						Value = Types.Convert<string>(((JValue)i.Value).Value, CultureInfo.InvariantCulture)
					});
				}
			}

			model.Initialize(this, ms, view);

			if (!model.Authorize(model.Component))
				return Unauthorized();

			return PartialView("~/Views/IoT/IoTPartialView.cshtml", model);
		}
	}
}
