using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net;
using TomPIT.Compilation;
using TomPIT.Middleware;
using TomPIT.Security;

namespace TomPIT.Routing;
internal sealed class PrecompileRouteHandler : RouteHandlerBase
{
	protected override void OnProcessRequest()
	{
		var ctx = Tenant ?? MiddlewareDescriptor.Current.Tenant;

		if (!ctx.GetService<IAuthorizationService>().Demand(MiddlewareDescriptor.Current.UserToken, SecurityUtils.FullControlRole))
		{
			Context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
			return;
		}

		var result = Precompilation.Reset();
		
		Context.Response.StatusCode = (int)HttpStatusCode.OK;
		Context.Response.ContentType = "application/json";
	}
}