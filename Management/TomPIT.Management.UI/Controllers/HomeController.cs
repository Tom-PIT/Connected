using Microsoft.AspNetCore.Authorization;
using TomPIT.Ide.Controllers;
using TomPIT.Ide.Models;
using TomPIT.Management.Models;

namespace TomPIT.Management.Controllers
{
	[Authorize(Roles = "Full Control")]
	public class HomeController : IdeControllerBase
	{
		protected override IdeModelBase CreateModel()
		{
			var r = new HomeModel();

			r.Initialize(this, null);

			if (string.IsNullOrWhiteSpace(Request.ContentType)
				|| Request.ContentType.Contains("application/json"))
				r.RequestBody = FromBody();

			r.Databind();

			return r;
		}
	}
}
