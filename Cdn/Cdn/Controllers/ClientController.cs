using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.Cdn.Clients;
using TomPIT.Controllers;
using TomPIT.Environment;
using TomPIT.Exceptions;
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
			var arguments = body.Optional<JObject>("arguments", null);

			var client = MiddlewareDescriptor.Current.Tenant.GetService<IClientService>().Select(token);

			if (client == null)
				throw new NotFoundException(SR.ErrClientNotFound);

			if (ClientHubs.Clients == null)
				return;

			ClientHubs.Clients.Clients.Group(token.ToLowerInvariant()).SendCoreAsync("message", new object[] { method, token, arguments });
		}
	}
}
