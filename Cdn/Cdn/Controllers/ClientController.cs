using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Cdn;
using TomPIT.Controllers;
using TomPIT.Middleware;

namespace TomPIT.Cdn.Controllers
{
	[Authorize(AuthenticationSchemes = "TomPIT")]
	public class ClientController : ServerController
	{
		[HttpPost]
		public void Notify()
		{
			var body = FromBody();
			var token = body.Required<string>("token");
			var method = body.Required<string>("method");
			var arguments = body.Optional<Newtonsoft.Json.Linq.JObject>("arguments", null);

			MiddlewareDescriptor.Current.Tenant.GetService<ICdnClientNotificationProxy>()
				.Notify(token, method, arguments);
		}
	}
}
