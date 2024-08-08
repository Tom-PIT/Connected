using Microsoft.Extensions.Configuration;

using System;
using System.Net;

using TomPIT.Design;
using TomPIT.Middleware;
using TomPIT.Security;

namespace TomPIT.Routing
{
	internal class DeployRouteHandler : RouteHandlerBase
	{
		//TODO Override from configuration
		private const string Remote = "https://sys-connected.tompit.com/rest";

		private string? _baseUrl = null;

		private string BaseUrl => _baseUrl ??= Shell.Configuration.GetSection("deployment").GetValue("remoteUrl", Remote) ?? Remote;

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
         var verb = body.Optional("verb", DeploymentVerb.Deploy);
         var startCommit = body.Optional("startCommit", 0L);
			var resourceGroup = body.Optional<string?>("resourceGroup", null);

         ctx.GetService<IDesignService>().Deployment.Deploy(BaseUrl, repository, branch, commit, startCommit, key, verb, resourceGroup);
		}
	}
}
