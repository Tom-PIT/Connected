using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Development.Models;
using TomPIT.Ide.Controllers;
using TomPIT.Ide.Models;

namespace TomPIT.Development.Controllers
{
	[Authorize(Policy = "Implement Micro Service")]
	public class IdeController : IdeControllerBase
	{
		protected override IdeModelBase CreateModel()
		{
			var r = new IdeModel();

			var microService = RouteData.Values["microService"] as string;

			r.Initialize(this, r.Tenant.GetService<IMicroServiceService>().SelectByUrl(microService));

			if (string.IsNullOrWhiteSpace(Request.ContentType)
				|| Request.ContentType.Contains("application/json"))
				r.RequestBody = FromBody();

			r.Databind();

			return r;
		}

		[HttpPost]
		public ActionResult SelectUserState()
		{
			var body = FromBody();
			var model = CreateModel();

			return Json(model.Services.Data.User.Select<JObject, string>(body.Required<string>("primaryKey"), body.Required<string>("topic")));
		}

		[HttpPost]
		public ActionResult UpdateUserState()
		{
			var body = FromBody();
			var model = CreateModel();

			model.Services.Data.User.Update(body.Required<string>("primaryKey"), body.Required<JObject>("value"), body.Required<string>("topic"));

			return new EmptyResult();
		}
	}
}
