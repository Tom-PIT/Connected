using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TomPIT.Compilation;
using TomPIT.Middleware;
using TomPIT.Security;

namespace TomPIT.Routing;

internal sealed class PrecompileRouteHandler : RouteHandlerBase
{
	protected override async Task OnProcessRequestAsync()
	{
		var ctx = Tenant ?? MiddlewareDescriptor.Current.Tenant;

		if (!ctx.GetService<IAuthorizationService>().Demand(MiddlewareDescriptor.Current.UserToken, SecurityUtils.FullControlRole))
		{
			Context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
			return;
		}

		var result = Precompilation.Precompile();

		if (result.Any())
		{
			Context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
			Context.Response.ContentType = "application/json";
			await Context.Response.WriteAsJsonAsync(result);
		}
	}
}