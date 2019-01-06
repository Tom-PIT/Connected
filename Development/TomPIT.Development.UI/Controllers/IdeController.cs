using System;
using TomPIT.ComponentModel;
using TomPIT.Models;

namespace TomPIT.Controllers
{
	public class IdeController : IdeControllerBase
	{
		protected override IdeModelBase CreateModel()
		{
			var r = new IdeModel();

			r.Initialize(this);

			var microService = RouteData.Values["microService"] as string;

			r.MicroService = r.Connection.GetService<IMicroServiceService>().SelectByUrl(microService);

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
