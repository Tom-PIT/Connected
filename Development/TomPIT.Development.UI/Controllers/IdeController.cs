using System;
using Microsoft.AspNetCore.Authorization;
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

			if (r.MicroService == null)
				throw new NullReferenceException();

			if (string.IsNullOrWhiteSpace(Request.ContentType)
				|| Request.ContentType.Contains("application/json"))
				r.RequestBody = FromBody();

			r.Databind();

			return r;
		}
	}
}
