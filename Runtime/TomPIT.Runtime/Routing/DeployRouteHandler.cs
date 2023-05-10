using System;
using System.Net;
using TomPIT.Design;
using TomPIT.Middleware;
using TomPIT.Security;

namespace TomPIT.Routing
{
	internal class DeployRouteHandler : RouteHandlerBase
	{
		private const string Remote = "http://sys-app/reposerver/rest";
		protected override void OnProcessRequest()
		{
			var ctx = Tenant ?? MiddlewareDescriptor.Current.Tenant;

			if (!ctx.GetService<IAuthorizationService>().Demand(MiddlewareDescriptor.Current.UserToken, SecurityUtils.FullControlRole))
			{
				Context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
				return;
			}

			var body = Context.Request.Body.ToJObject();

			var repository = body.Required<Guid>("repository");
			var key = body.Required<string>("authenticationKey");
			var branch = body.Optional("branch", 0L);
			var commit = body.Optional("commit", 0L);

			ctx.GetService<IDesignService>().Deployment.Deploy(Remote, repository, branch, commit, key);
		}
	}
}
